using Jundroo.SimplePlanes.ModTools.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PositionControllerBehaviour : Jundroo.SimplePlanes.ModTools.Parts.PartModifierBehaviour
{
    private PositionController modifier;
    private SlewModeCommon SlewModeCommon;

    private Component aircraft;
    private SlewModeCommon.AircraftInfo aircraftInfo;
    private InputChannelBehaviour inputX;
    private InputChannelBehaviour inputY;
    private InputChannelBehaviour inputZ;
    private float valueX = 0f;
    private float valueY = 0f;
    private float valueZ = 0f;

    private int Channel = 0;
    private SlewModeCommon.ControlModes ControlMode;
    private Space ControlSpace = Space.Self;

    // True if the controller has tried to reference the input channels
    private bool CheckedChannels = false;

    private void Start()
    {
        modifier = (PositionController)PartModifier;
        SlewModeCommon = GetComponentInChildren<SlewModeCommon>();

        if (!ServiceProvider.Instance.GameState.IsInDesigner)
        {
            aircraft = SlewModeCommon.GetParentAircraftComponent();
            aircraftInfo = new SlewModeCommon.AircraftInfo(aircraft);
            Channel = modifier.Channel;
            ControlMode = modifier.ControlMode;
            ControlSpace = modifier.ControlSpace;
        }
    }

    private void FixedUpdate()
    {
        if (ServiceProvider.Instance.GameState.IsInDesigner)
        {
            return;
        }

        // In simulation

        if (!CheckedChannels)
        {
            InputChannelBehaviour[] channels = FindObjectsOfType<InputChannelBehaviour>();
            foreach (InputChannelBehaviour ch in channels)
            {
                if (Channel == ch.Channel && aircraft == ch.aircraft)
                {
                    if (ch.Component == SlewModeCommon.VectorComponents.X)
                        inputX = ch;
                    else if (ch.Component == SlewModeCommon.VectorComponents.Y)
                        inputY = ch;
                    else if (ch.Component == SlewModeCommon.VectorComponents.Z)
                        inputZ = ch;
                }
            }

            if (inputX == null)
                Debug.LogErrorFormat("Channel {0}: Component X is missing!", Channel);
            if (inputY == null)
                Debug.LogErrorFormat("Channel {0}: Component Y is missing!", Channel);
            if (inputZ == null)
                Debug.LogErrorFormat("Channel {0}: Component Z is missing!", Channel);

            CheckedChannels = true;
        }

        // Do not use Slew Mode if deactivated
        if (!InputController.Active)
        {
            return;
        }

        valueX = SlewModeCommon.GetInputValue(inputX);
        valueY = SlewModeCommon.GetInputValue(inputY);
        valueZ = SlewModeCommon.GetInputValue(inputZ);
        Vector3 InputVector = new Vector3(valueX, valueY, valueZ);

        if (ControlMode == SlewModeCommon.ControlModes.Absolute)
        {
            aircraftInfo.SetGlobalPosition(InputVector);
        }
        else if (ControlMode == SlewModeCommon.ControlModes.Velocity)
        {
            aircraftInfo.SetVelocity(InputVector, ControlSpace);
        }
        else    // Acceleration
        {
            aircraftInfo.Accelerate(InputVector, ControlSpace, Time.fixedDeltaTime);
        }
    }
}