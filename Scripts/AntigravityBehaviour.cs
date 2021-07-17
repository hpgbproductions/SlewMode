using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AntigravityBehaviour : Jundroo.SimplePlanes.ModTools.Parts.PartModifierBehaviour
{
    private Antigravity modifier;
    private SlewModeCommon SlewModeCommon;

    private Component aircraft;
    private SlewModeCommon.AircraftInfo aircraftInfo;

    private bool active = false;
    private bool activePrevious = false;

    private void Start()
    {
        modifier = (Antigravity)PartModifier;
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
                    rigidbody.useGravity = false;
            }
        }
        else if (activePrevious && !active)
        {
            foreach (Rigidbody rigidbody in aircraftInfo.aircraftRigidbodies)
            {
                if (rigidbody != null)
                    rigidbody.useGravity = true;
            }
        }

        activePrevious = active;
    }
}