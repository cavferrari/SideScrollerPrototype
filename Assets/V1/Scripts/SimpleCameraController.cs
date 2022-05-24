using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    public Transform player;

    public Vector3 offset;

    private Vector3 newPosition;

    void LateUpdate()
    {
        newPosition = player.position + offset;
        newPosition.x = transform.position.x;
        newPosition.y = transform.position.y;
        transform.position = newPosition;
    }
}
