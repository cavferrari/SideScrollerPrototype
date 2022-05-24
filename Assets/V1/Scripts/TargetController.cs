using UnityEngine;

public class TargetController : MonoBehaviour
{
    private Vector3 screenPoint;

    void Update()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);
        curPosition.z = 0f;
        transform.position = curPosition;
    }
}
