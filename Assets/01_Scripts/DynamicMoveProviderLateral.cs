using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Variant of the XR Interaction Toolkit's DynamicMoveProvider that ignores the
/// joystick's forward/back axis and only allows strafe (left/right).
/// Intended for scenes using AutomaticMovementVR, where forward movement is
/// driven by the waypoint script and must not compete with manual input.
/// </summary>
public class DynamicMoveProviderLateral : DynamicMoveProvider
{
    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        return base.ComputeDesiredMove(new Vector2(input.x, 0f));
    }
}
