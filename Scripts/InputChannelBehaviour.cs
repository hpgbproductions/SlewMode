using Jundroo.SimplePlanes.ModTools.Parts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputChannelBehaviour : Jundroo.SimplePlanes.ModTools.Parts.PartModifierBehaviour
{
    private InputChannel modifier;
    private SlewModeCommon SlewModeCommon;

    public Component aircraft;

    public int Channel = 0;
    public SlewModeCommon.VectorComponents Component;

    public float InputValue = 0f;
    public bool InputActive = true;

    private void Start()
    {
        modifier = (InputChannel)PartModifier;
        SlewModeCommon = GetComponentInChildren<SlewModeCommon>();

        if (!ServiceProvider.Instance.GameState.IsInDesigner)
        {
            aircraft = SlewModeCommon.GetParentAircraftComponent();
            Channel = modifier.Channel;
            Component = modifier.Component;
        }
    }

    private void Update()
    {
        if (!ServiceProvider.Instance.GameState.IsInDesigner)
        {
            InputValue = InputController.Value;
            InputActive = InputController.Active;
        }
    }
}