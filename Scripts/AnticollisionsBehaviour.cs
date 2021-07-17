using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AnticollisionsBehaviour : Jundroo.SimplePlanes.ModTools.Parts.PartModifierBehaviour
{
    private Anticollisions modifier;
    private SlewModeCommon SlewModeCommon;

    private Component aircraft;
    private SlewModeCommon.AircraftInfo aircraftInfo;

    private bool active = false;
    private bool activePrevious = false;

    private void Start()
    {
        modifier = (Anticollisions)PartModifier;
        SlewModeCommon = GetComponentInChildren<SlewModeCommon>();

        if (!ServiceProvider.Instance.GameState.IsInDesigner)
        {
            aircraft = SlewModeCommon.GetParentAircraftComponent();
            aircraftInfo = new SlewModeCommon.AircraftInfo(aircraft);
        }
    }

    private void FixedUpdate()
    {
        if (ServiceProvider.Instance.GameState.IsInDesigner)
        {
            return;
        }

        // In simulation

        active = InputController.Active;

        if (!activePrevious && active)
        {
            foreach (Rigidbody rigidbody in aircraftInfo.aircraftRigidbodies)
            {
                if (rigidbody != null)
                    rigidbody.detectCollisions = false;
            }
        }
        else if (activePrevious && !active)
        {
            foreach (Rigidbody rigidbody in aircraftInfo.aircraftRigidbodies)
            {
                if (rigidbody != null)
                    rigidbody.detectCollisions = true;
            }
        }

        activePrevious = active;
    }
}