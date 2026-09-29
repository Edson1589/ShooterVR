using System.Collections.Generic;
using UnityEngine;

public class AutomaticMovementVR : MonoBehaviour
{
    [Header("Recorrido")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Configuración de Movimiento")]
    public float speed = 0.8f;
    public float arrivalDistance = 0.1f;

    private CharacterController characterController;
    private int currentWaypointIndex;
    private bool isMovementActive;

    private readonly HashSet<Object> pauseOwners = new HashSet<Object>();

    public bool IsMovementPaused => pauseOwners.Count > 0;
    public bool HasCompletedPath => waypoints.Count > 0 && currentWaypointIndex >= waypoints.Count;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        ResumeMovement();
    }

    private void Update()
    {
        if (!isMovementActive || IsMovementPaused || HasCompletedPath) return;

        Transform waypoint = waypoints[currentWaypointIndex];

        if (waypoint == null)
        {
            NextWaypoint();
            return;
        }

        Vector3 targetPosition = waypoint.position;
        targetPosition.y = transform.position.y;

        Vector3 direction = targetPosition - transform.position;

        characterController.Move(Vector3.ClampMagnitude(direction, speed * Time.deltaTime));

        if (Vector3.Distance(transform.position, targetPosition) <= arrivalDistance) NextWaypoint();
    }

    private void NextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Count) isMovementActive = false;
    }

    public void StopMovement()
    {
        isMovementActive = false;
    }

    public void PauseMovement(Object owner)
    {
        if (owner != null) pauseOwners.Add(owner);
    }

    public void ResumeMovement(Object owner)
    {
        if (owner != null) pauseOwners.Remove(owner);
    }

    public void ResumeMovement()
    {
        if (!HasCompletedPath && waypoints.Count > 0) isMovementActive = true;
    }
}