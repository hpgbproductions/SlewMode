// Attach as a child object in the part, to provide access to useful scripts

using Jundroo.SimplePlanes.ModTools.Mocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SlewModeCommon : MonoBehaviour
{
    public enum VectorComponents { X, Y, Z }

    public enum ControlModes { Absolute, Velocity, Acceleration }

    public float GetInputValue(InputChannelBehaviour i)
    {
        if (i == null)
        {
            return 0;
        }
        else
        {
            return i.InputValue;
        }
    }

    /// <summary>
    /// Searches up the hierarchy for an AircraftScript component.
    /// </summary>
    /// <returns></returns>
    public Component GetParentAircraftComponent()
    {
        Transform testTransform = transform;
        GameObject testObject;

        while (testTransform.parent != null)
        {
            testTransform = testTransform.parent;
            testObject = testTransform.gameObject;

            Component[] components = testObject.GetComponents<Component>();
            foreach (Component c in components)
            {
                if (c.GetType().Name == "AircraftScript")
                {
                    return c;
                }
            }
        }

        Debug.LogError("No AircraftScript in hierarchy!");
        return null;
    }

    public struct AircraftInfo
    {
        /*
         * local space movement not following pitch and roll
         * set angular velocity only working in world space
         * implement tracker objects
        */

        public Component aircraftScript;
        public Transform aircraftTransform;
        public Rigidbody[] aircraftRigidbodies;
        private Type aircraftScriptType;

        private PropertyInfo globalPositionInfo;    // Vector3 GlobalPosition
        private PropertyInfo velocityInfo;          // Vector3 Velocity
        private MethodInfo setVelocityInfo;         // void SetVelocity(Vector3 velocity, bool ignoreDisconnectedBodies)

        private PropertyInfo rotationInfo;    // Vector3 Rotation

        // Use for calculation by providing transforms
        private GameObject aircraftTrackerObject;
        private GameObject aircraftTrackerChildObject;
        private Transform aircraftTracker;
        private Transform aircraftTrackerChild;

        public Vector3 GetGlobalPosition()
        {
            return (Vector3)globalPositionInfo.GetValue(aircraftScript);
        }

        public Vector3 GetGlobalVelocity()
        {
            return (Vector3)velocityInfo.GetValue(aircraftScript);
        }

        public Vector3 GetGlobalRotation()
        {
            return (Vector3)rotationInfo.GetValue(aircraftScript);
        }

        public void SetGlobalPosition(Vector3 gpos)
        {
            globalPositionInfo.SetValue(aircraftScript, gpos);
        }

        /// <summary>
        /// Set the velocity of the aircraft.
        /// </summary>
        /// <param name="vel">Velocity.</param>
        /// <param name="velSpace">Space of the provided velocity.</param>
        public void SetVelocity(Vector3 vel, Space velSpace)
        {
            if (velSpace == Space.Self)
            {
                // Convert the local space velocity to the world space velocity used by SetPosition
                UpdateAircraftTracker();
                vel = aircraftTracker.TransformVector(vel);
            }

            object[] parameters = new object[] { vel, true };
            setVelocityInfo.Invoke(aircraftScript, parameters);
        }

        public void SetGlobalRotation(Vector3 grot)
        {
            rotationInfo.SetValue(aircraftScript, grot);
        }

        /// <summary>
        /// Set the angular velocity.
        /// </summary>
        /// <param name="av">Angular velocity in degrees per second.</param>
        /// <param name="avSpace">Space of the provided angular velocity.</param>
        public void SetAngularVelocity(Vector3 av, Space avSpace)
        {
            Vector3 globalAngVel;

            if (avSpace == Space.Self)
            {
                UpdateAircraftTracker();
                globalAngVel = aircraftTracker.TransformDirection(av);
            }
            else
            {
                globalAngVel = av;
            }

            foreach (Rigidbody rigidbody in aircraftRigidbodies)
            {
                rigidbody.angularVelocity = globalAngVel * Mathf.Deg2Rad;
            }
        }

        /// <summary>
        /// Adds an acceleration vector to the velocity.
        /// </summary>
        /// <param name="accel">Acceleration rate vector.</param>
        /// <param name="accelSpace">Space of the provided acceleration.</param>
        /// <param name="deltaTime">Time multiplier for acceleration.</param>
        public void Accelerate(Vector3 accel, Space accelSpace, float deltaTime)
        {
            if (accelSpace == Space.Self)
            {
                // Convert the acceleration space from local to world space
                UpdateAircraftTracker();
                accel = aircraftTracker.TransformVector(accel);
            }

            object[] parameters = new object[] { accel * deltaTime + GetGlobalVelocity(), true };
            setVelocityInfo.Invoke(aircraftScript, parameters);
        }

        /// <summary>
        /// Adds an angular acceleration.
        /// </summary>
        /// <param name="aa">Angular acceleration rate vector. Angles in degrees.</param>
        /// <param name="aaSpace">Space of the provided angular acceleration.</param>
        public void AngularAccelerate(Vector3 aa, Space aaSpace)
        {
            Vector3 globalAngAcc;

            if (aaSpace == Space.Self)
            {
                UpdateAircraftTracker();
                globalAngAcc = aircraftTracker.TransformDirection(aa);
            }
            else
            {
                globalAngAcc = aa;
            }

            foreach (Rigidbody rigidbody in aircraftRigidbodies)
            {
                rigidbody.AddTorque(globalAngAcc, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Update the aircraft tracker transform. Must be called before space conversion.
        /// </summary>
        private void UpdateAircraftTracker()
        {
            aircraftTracker.position = GetGlobalPosition();
            aircraftTracker.eulerAngles = GetGlobalRotation();
        }

        public AircraftInfo(Component aircraft)
        {
            aircraftScript = aircraft;
            aircraftTransform = aircraftScript.transform;
            aircraftScriptType = aircraftScript.GetType();

            aircraftRigidbodies = aircraftScript.gameObject.GetComponentsInChildren<Rigidbody>();

            globalPositionInfo = aircraftScriptType.GetProperty("GlobalPosition");
            velocityInfo = aircraftScriptType.GetProperty("Velocity");
            setVelocityInfo = aircraftScriptType.GetMethod("SetVelocity");

            rotationInfo = aircraftScriptType.GetProperty("Rotation");

            aircraftTrackerObject = new GameObject("SlewMode AircraftTracker: " + aircraftScript.gameObject.name);
            aircraftTracker = aircraftTrackerObject.transform;

            aircraftTrackerChildObject = new GameObject();
            aircraftTrackerChild = aircraftTrackerChildObject.transform;
            aircraftTrackerChild.parent = aircraftTracker;
        }
    }
}
