using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatically moves the player (XR Origin) through a list of waypoints,
/// with smooth movement. Intended for guided VR sequences (tutorials, intros).
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class AutomaticMovementVR : MonoBehaviour
{
    [Header("Path")]
    [Tooltip("Waypoints that define the path, in the order they are visited.")]
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    [Header("Movement Settings")]
    [Tooltip("Movement speed in meters per second.")]
    [SerializeField] private float speed = 0.8f;

    [Tooltip("Distance (in meters) at which a waypoint is considered reached.")]
    [SerializeField] private float arrivalDistance = 0.1f;

    private CharacterController characterController;
    private int currentWaypointIndex;
    private bool isMovementActive;
    private readonly HashSet<Object> pauseOwners = new HashSet<Object>();

    public bool IsMovementPaused => pauseOwners.Count > 0;

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
        if (!isMovementActive || IsMovementPaused || currentWaypointIndex >= waypoints.Count)
        {
            return;
        }

        Transform currentWaypoint = waypoints[currentWaypointIndex];
        if (currentWaypoint == null)
        {
            AdvanceToNextWaypoint();
            return;
        }

        // Only the horizontal plane is controlled here: the Y axis is left to the
        // Locomotion System's gravity provider (avoids both fighting each other).
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = new Vector3(currentWaypoint.position.x, currentPosition.y, currentWaypoint.position.z);

        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
        MovePlayer(newPosition - currentPosition);

        if (Vector3.Distance(transform.position, targetPosition) <= arrivalDistance)
        {
            AdvanceToNextWaypoint();
        }
    }

    private void MovePlayer(Vector3 delta)
    {
        // XRI's Body Transformer briefly disables the CharacterController during a
        // turn (snap turn). Moving the Transform directly during that frame would skip
        // the collision check and let the player clip through walls/objects. Better to
        // just wait a frame.
        if (characterController.enabled)
        {
            characterController.Move(delta);
        }
    }

    private void AdvanceToNextWaypoint()
    {
        currentWaypointIndex++;

        if (currentWaypointIndex >= waypoints.Count)
        {
            StopMovement();
        }
    }

    /// <summary>Pauses automatic movement (e.g. on a gameplay event).</summary>
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

    /// <summary>Resumes or starts automatic movement from the current waypoint.</summary>
    public void ResumeMovement()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning($"{nameof(AutomaticMovementVR)}: no waypoints assigned.", this);
            return;
        }

        isMovementActive = currentWaypointIndex < waypoints.Count;
    }

    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Count == 0)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Vector3 previous = transform.position;

        foreach (Transform waypoint in waypoints)
        {
            if (waypoint == null)
            {
                continue;
            }

            Gizmos.DrawSphere(waypoint.position, 0.15f);
            Gizmos.DrawLine(previous, waypoint.position);
            previous = waypoint.position;
        }
    }
}
