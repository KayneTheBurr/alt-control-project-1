using UnityEngine;

public class CameraBoundsHelper : MonoBehaviour
{
    void Start()
    {
        Camera cam = Camera.main;

        // 屏幕中心（0.5, 0.5）是画面中央
        Vector3 topPoint = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, cam.nearClipPlane + 10f));
        Vector3 bottomPoint = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, cam.nearClipPlane + 10f));

        Debug.Log("Top Y: " + topPoint.y);
        Debug.Log("Bottom Y: " + bottomPoint.y);
    }
}
