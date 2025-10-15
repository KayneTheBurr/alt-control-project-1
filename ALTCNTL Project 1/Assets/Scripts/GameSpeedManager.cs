using UnityEngine;

public class GameSpeedManager : MonoBehaviour
{
    public static GameSpeedManager Instance;

    [Header("全局速度控制")]
    public float startSpeed = 3f;       // 初始速度
    public float maxSpeed = 10f;        // 最大速度
    public float acceleration = 0.2f;   // 每秒增加速度
    public float delayBeforeSpeedUp = 2f;

    [HideInInspector] public float currentSpeed;
    private float timer;

    void Awake()
    {
        Instance = this;
        currentSpeed = startSpeed;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > delayBeforeSpeedUp)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        }
    }

    /// <summary>
    /// 获取视觉匹配后的“跑道滚动速度”
    /// </summary>
    public float GetVisualScrollSpeed(float visualScale)
    {
        return currentSpeed * visualScale;
    }
}
