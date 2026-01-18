using UnityEngine;

public class MinimapController : MonoBehaviour
{
    public Transform player;

    void LateUpdate()
    {
        if (player != null)
        {
            // For 2D games: follow player on XY plane, keep camera Z position fixed
            Vector3 newPosition = player.position;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
    }
}