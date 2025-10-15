using UnityEngine;

public class PlayerLaneController : MonoBehaviour
{
    [Header("Lane Positions (X Axis)")]
    public float[] lanePositions = { -2f, 0f, 2f };  // 三个车道的中心点
    private int currentLane = 1; // 默认在中间车道 (Lane 2)

    [Header("Move Settings")]
    public float moveSpeed = 10f; // 控制切换的插值速度（可调）

    private Vector3 targetPosition;

    void Start()
    {
        // 初始目标位置为中间车道
        targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
        transform.position = targetPosition;
    }

    void Update()
    {
        HandleInput();
        MoveToTargetLane();
    }

    void HandleInput()
    {
        // 按下 A -> 向左车道移动
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (currentLane > 0)
            {
                currentLane--;
                targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
            }
        }

        // 按下 D -> 向右车道移动
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (currentLane < lanePositions.Length - 1)
            {
                currentLane++;
                targetPosition = new Vector3(lanePositions[currentLane], transform.position.y, transform.position.z);
            }
        }
    }

    void MoveToTargetLane()
    {
        // 平滑移动到目标车道中心
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("💥 Game Over!");
            Time.timeScale = 0f; // 暂停游戏
                                 // 你也可以在这里调用 GameManager 显示“Game Over”界面
        }
    }

}
