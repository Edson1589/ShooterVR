using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

/// <summary>
/// Variante del DynamicMoveProvider del XR Interaction Toolkit que ignora el eje
/// adelante/atrás del joystick y solo permite el strafe (izquierda/derecha).
/// Pensado para escenas con MovimientoAutomaticoVR, donde el avance hacia adelante
/// es controlado por el script de waypoints y no debe competir con el input manual.
/// </summary>
public class DynamicMoveProviderLateral : DynamicMoveProvider
{
    protected override Vector3 ComputeDesiredMove(Vector2 input)
    {
        return base.ComputeDesiredMove(new Vector2(input.x, 0f));
    }
}
