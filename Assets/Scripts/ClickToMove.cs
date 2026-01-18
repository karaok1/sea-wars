using Pathfinding;
using UnityEngine;

using Unity.Netcode;

public class ClickToMove : NetworkBehaviour, IClickable
{
    private Seeker seeker;
    private AILerp aiLerp;
    private AstarPath astarPath;
    private PlayerDirectionSpriteController spriteController;
    private Path currentPath;
    private Vector3 previousPosition;
    private Vector3 lastValidDirection;

    private int currentWaypointIndex = 0;
    private const float waypointDistanceThreshold = 0.1f;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        aiLerp = GetComponent<AILerp>();
        astarPath = AstarPath.active;
        spriteController = GetComponent<PlayerDirectionSpriteController>();
        seeker.pathCallback += OnPathComplete;
        previousPosition = transform.position;
    }

    public void OnClick(Vector3 position)
    {
        if (!IsOwner) return;

        GraphNode nearestNode = astarPath.GetNearest(position).node;

        if (nearestNode != null && nearestNode.Walkable)
        {
            Vector3 nearestNodePosition = (Vector3)nearestNode.position;
            aiLerp.destination = nearestNodePosition;
        }
    }

    private void OnPathComplete(Path p)
    {
        currentPath = p;

        if (!p.error)
        {
            currentWaypointIndex = 0;
            if (Vector3.Distance(transform.position, p.vectorPath[currentWaypointIndex]) < waypointDistanceThreshold)
            {
                currentWaypointIndex++;
            }
        }
        else
        {
            Debug.LogError("Pathfinding error: " + p.errorLog);
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Direction calculation based on actual movement
        Vector3 displacement = transform.position - previousPosition;
        if (displacement.magnitude > 0.001f) // Use a threshold to prevent small inaccuracies
        {
            lastValidDirection = displacement.normalized;
            spriteController.UpdateSprite(lastValidDirection);
        }
        else if (aiLerp.velocity == Vector3.zero && lastValidDirection != Vector3.zero)
        {
            // When the player stops
            spriteController.UpdateSprite(lastValidDirection);
        }

        previousPosition = transform.position; // Store the current position for the next frame
    }
}
