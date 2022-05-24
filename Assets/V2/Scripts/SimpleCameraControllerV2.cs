using UnityEngine;

public class SimpleCameraControllerV2 : MonoBehaviour
{
    public Transform player;

    public Vector3 offset;

    private Vector3 newPosition;

    void LateUpdate()
    {
        newPosition = player.position + offset;
        newPosition.y = transform.position.y;
        newPosition.z = transform.position.z;
        transform.position = newPosition;
    }
}
