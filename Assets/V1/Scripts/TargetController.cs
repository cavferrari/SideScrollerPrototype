using UnityEngine;

public class TargetController : MonoBehaviour
{
    public PlayerControllerV2 player;

    private Vector3 screenPoint;

    void Update()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        if (player.HasEnemy())
        {
            curPosition.z = player.GetEnemyPosition().z;
        }
        else
        {
            curPosition.z = 0f;
        }
        transform.position = curPosition;
    }
}
