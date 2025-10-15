using UnityEngine;

public class InfiniteRoad : MonoBehaviour
{
    [Header("滚动方向")]
    public bool scrollUp = false;

    [Header("视觉滚动比例")]
    [Tooltip("视觉匹配倍数，0.1 表示跑道纹理滚动速度为障碍的 10%")]
    public float visualScale = 0.1f;

    private Renderer rend;
    private Vector2 offset;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        if (GameSpeedManager.Instance == null) return;

        // 从 GameSpeedManager 获取视觉匹配速度
        float visualSpeed = GameSpeedManager.Instance.GetVisualScrollSpeed(visualScale);

        float direction = scrollUp ? 1f : -1f;
        offset.y += direction * visualSpeed * Time.deltaTime;
        rend.material.mainTextureOffset = offset;
    }
}
