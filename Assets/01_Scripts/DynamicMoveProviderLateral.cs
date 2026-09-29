using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class DynamicMoveProviderLateral : DynamicMoveProvider
{
    protected override Vector3 ComputeDesiredMove(Vector2 input) => base.ComputeDesiredMove(new Vector2(input.x, 0f));
}
