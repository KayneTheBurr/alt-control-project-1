using UnityEngine;
using System.Collections.Generic;

public class SmartObstacleSpawner : MonoBehaviour
{
    [Header("普通障碍 Prefabs (index 0~4)")]
    public GameObject[] normalObstacles;

    [Header("每个障碍的速度倍率 (与上方 Prefab 一一对应)")]
    [Tooltip("决定每种障碍的速度差异，比如石头=0，汽车=1，飞机=2")]
    public float[] normalObstacleSpeedMultipliers = { 1f, 1f, 1f, 1f, 1f };

    [Header("飞机 Prefab (index 0 only)")]
    public GameObject planePrefab;

    [Header("飞机速度倍率")]
    public float planeSpeedMultiplier = 2f; // ✈️ 飞机速度通常比地面障碍快

    [Header("Lane X 坐标 (3条)")]
    public float[] lanePositions = { -2f, 0f, 2f };

    [Header("飞机起始位置 (左占双 / 右占双)")]
    public Vector2 leftPlaneOffset = new Vector2(-1f, 0f);
    public Vector2 rightPlaneOffset = new Vector2(1f, 0f);

    [Header("生成参数")]
    public float spawnInterval = 2f;
    public float planeSpawnChance = 0.2f;

    private float spawnY;
    private float despawnY;
    private float timer;

    void Start()
    {
        // 自动相机范围检测
        Camera cam = Camera.main;
        Vector3 top = cam.ViewportToWorldPoint(new Vector3(0.5f, 1f, cam.nearClipPlane + 10f));
        Vector3 bottom = cam.ViewportToWorldPoint(new Vector3(0.5f, 0f, cam.nearClipPlane + 10f));
        spawnY = top.y + 2f;
        despawnY = bottom.y - 2f;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            GeneratePattern();
        }
    }

    void GeneratePattern()
    {
        bool spawnPlane = Random.value < planeSpawnChance;
        if (spawnPlane)
        {
            SpawnPlane();
            return;
        }

        // ---- 普通障碍逻辑 ----
        int count = Random.value < 0.6f ? 1 : 2;
        List<int> availableLanes = new List<int> { 0, 1, 2 };

        for (int i = 0; i < count; i++)
        {
            if (availableLanes.Count == 0) break;
            int randomLane = availableLanes[Random.Range(0, availableLanes.Count)];
            availableLanes.Remove(randomLane);

            int randomIndex = Random.Range(0, normalObstacles.Length);
            Vector3 pos = new Vector3(lanePositions[randomLane], spawnY, 0f);
            SpawnObstacle(normalObstacles[randomIndex], pos, randomIndex);
        }
    }

    void SpawnPlane()
    {
        bool spawnLeftPlane = Random.value < 0.5f;
        Vector3 pos = spawnLeftPlane
            ? new Vector3(leftPlaneOffset.x, spawnY, 0f)
            : new Vector3(rightPlaneOffset.x, spawnY, 0f);

        GameObject plane = Instantiate(planePrefab, pos, Quaternion.identity);
        var mover = plane.GetComponent<ObstacleMover>();
        if (mover == null)
            mover = plane.AddComponent<ObstacleMover>();

        mover.speed = GameSpeedManager.Instance.currentSpeed * planeSpeedMultiplier; // ✈️ 单独倍速
        mover.despawnY = despawnY;
    }

    void SpawnObstacle(GameObject prefab, Vector3 pos, int index)
    {
        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);
        var mover = obj.GetComponent<ObstacleMover>();
        if (mover == null)
            mover = obj.AddComponent<ObstacleMover>();

        // 🚗 每个障碍独立倍速
        float multiplier = 1f;
        if (index >= 0 && index < normalObstacleSpeedMultipliers.Length)
            multiplier = normalObstacleSpeedMultipliers[index];

        mover.speed = GameSpeedManager.Instance.currentSpeed * multiplier;
        mover.despawnY = despawnY;
    }
}
