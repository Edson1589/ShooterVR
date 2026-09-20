using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mueve automáticamente al jugador (XR Origin) a través de una lista de waypoints,
/// con desplazamiento suave. Pensado para secuencias guiadas en VR (tutoriales, intros).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class MovimientoAutomaticoVR : MonoBehaviour
{
    [Header("Ruta")]
    [Tooltip("Waypoints que definen la ruta, en el orden en que se recorren.")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("Configuración de movimiento")]
    [Tooltip("Velocidad de desplazamiento en metros por segundo.")]
    [SerializeField] private float velocidad = 0.8f;

    [Tooltip("Distancia (en metros) a la que se considera alcanzado un waypoint.")]
    [SerializeField] private float distanciaLlegada = 0.1f;

    private CharacterController characterController;
    private int indiceWaypointActual;
    private bool movimientoActivo;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        ReanudarMovimiento();
    }

    private void Update()
    {
        if (!movimientoActivo || waypoints.Count == 0)
        {
            return;
        }

        Transform waypointActual = waypoints[indiceWaypointActual];
        if (waypointActual == null)
        {
            AvanzarAlSiguienteWaypoint();
            return;
        }

        // Solo se controla el plano horizontal: el eje Y queda a cargo del
        // proveedor de gravedad del Locomotion System (evita que ambos compitan).
        Vector3 posicionActual = transform.position;
        Vector3 posicionObjetivo = new Vector3(waypointActual.position.x, posicionActual.y, waypointActual.position.z);

        Vector3 nuevaPosicion = Vector3.MoveTowards(posicionActual, posicionObjetivo, velocidad * Time.deltaTime);
        MoverJugador(nuevaPosicion - posicionActual);

        if (Vector3.Distance(transform.position, posicionObjetivo) <= distanciaLlegada)
        {
            AvanzarAlSiguienteWaypoint();
        }
    }

    private void MoverJugador(Vector3 delta)
    {
        // El Body Transformer de XRI deshabilita el CharacterController brevemente durante
        // un giro (snap turn). Si movemos el Transform directo en ese frame nos saltamos el
        // chequeo de colisiones y el jugador atraviesa paredes/objetos. Mejor esperar un frame.
        if (characterController.enabled)
        {
            characterController.Move(delta);
        }
    }

    private void AvanzarAlSiguienteWaypoint()
    {
        indiceWaypointActual++;

        if (indiceWaypointActual >= waypoints.Count)
        {
            DetenerMovimiento();
        }
    }

    /// <summary>Pausa el avance automático (por ejemplo, ante un evento de gameplay).</summary>
    public void DetenerMovimiento()
    {
        movimientoActivo = false;
    }

    /// <summary>Reanuda o inicia el avance automático desde el waypoint actual.</summary>
    public void ReanudarMovimiento()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning($"{nameof(MovimientoAutomaticoVR)}: no hay waypoints asignados.", this);
            return;
        }

        movimientoActivo = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Vector3 anterior = transform.position;

        foreach (Transform waypoint in waypoints)
        {
            if (waypoint == null)
            {
                continue;
            }

            Gizmos.DrawSphere(waypoint.position, 0.15f);
            Gizmos.DrawLine(anterior, waypoint.position);
            anterior = waypoint.position;
        }
    }
}
