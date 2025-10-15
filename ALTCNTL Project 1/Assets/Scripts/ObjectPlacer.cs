using UnityEngine;

/// <summary>
/// 用于放置可平衡物体的交互脚本
/// 演示如何在运行时添加物体并触发平衡计算
/// </summary>
public class ObjectPlacer : MonoBehaviour
{
    [Header("放置设置")]
    [Tooltip("可放置的物体预制体")]
    public GameObject[] placeablePrefabs;
    
    [Tooltip("当前选择的预制体索引")]
    public int currentPrefabIndex = 0;
    
    [Tooltip("放置高度偏移")]
    public float placeHeightOffset = 1.0f;
    
    [Tooltip("是否显示放置预览")]
    public bool showPlacementPreview = true;
    
    [Header("输入设置")]
    [Tooltip("放置物体的按键")]
    public KeyCode placeKey = KeyCode.Space;
    
    [Tooltip("切换预制体的按键")]
    public KeyCode switchPrefabKey = KeyCode.Tab;
    
    [Tooltip("是否使用鼠标位置")]
    public bool useMousePosition = true;

    [Header("调试")]
    public bool logPlacementEvents = true;

    private BalanceManager balanceManager;
    private Vector3 currentPlacementPosition;
    private GameObject previewObject;
    private Camera mainCamera;

    void Start()
    {
        balanceManager = FindObjectOfType<BalanceManager>();
        
        if (balanceManager == null)
        {
            Debug.LogWarning("[ObjectPlacer] 未找到 BalanceManager（可选）");
        }

        mainCamera = Camera.main;
        
        // 验证预制体数组
        if (placeablePrefabs != null && placeablePrefabs.Length > 0)
        {
            // 确保索引有效
            if (currentPrefabIndex < 0 || currentPrefabIndex >= placeablePrefabs.Length)
            {
                currentPrefabIndex = 0;
                Debug.LogWarning($"[ObjectPlacer] currentPrefabIndex 超出范围，已重置为 0");
            }
            
            if (showPlacementPreview)
            {
                CreatePreviewObject();
            }
        }
        else
        {
            Debug.LogWarning("[ObjectPlacer] 没有配置预制体数组！");
        }
    }

    void Update()
    {
        // 更新放置位置
        UpdatePlacementPosition();
        
        // 更新预览对象位置
        if (previewObject != null)
        {
            previewObject.transform.position = currentPlacementPosition;
        }

        // 处理输入
        HandleInput();
    }

    /// <summary>
    /// 更新当前放置位置
    /// </summary>
    void UpdatePlacementPosition()
    {
        if (useMousePosition && mainCamera != null)
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            currentPlacementPosition = new Vector3(
                mouseWorldPos.x,
                mouseWorldPos.y + placeHeightOffset,
                0
            );
        }
        else
        {
            currentPlacementPosition = transform.position;
        }
    }

    /// <summary>
    /// 处理用户输入
    /// </summary>
    void HandleInput()
    {
        // 放置物体
        if (Input.GetKeyDown(placeKey))
        {
            PlaceObject();
        }

        // 切换预制体
        if (Input.GetKeyDown(switchPrefabKey))
        {
            SwitchPrefab();
        }

        // 数字键快速选择
        for (int i = 0; i < Mathf.Min(9, placeablePrefabs?.Length ?? 0); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                currentPrefabIndex = i;
                UpdatePreviewObject();
                
                if (logPlacementEvents)
                {
                    Debug.Log($"[ObjectPlacer] 选择预制体 {i}: {placeablePrefabs[i].name}");
                }
            }
        }
    }

    /// <summary>
    /// 放置一个新物体
    /// </summary>
    public void PlaceObject()
    {
        if (placeablePrefabs == null || placeablePrefabs.Length == 0)
        {
            Debug.LogWarning("[ObjectPlacer] 没有可放置的预制体！");
            return;
        }

        // 确保索引在有效范围内
        if (currentPrefabIndex < 0 || currentPrefabIndex >= placeablePrefabs.Length)
        {
            currentPrefabIndex = 0;
        }

        GameObject prefab = placeablePrefabs[currentPrefabIndex];
        
        if (prefab == null)
        {
            Debug.LogWarning($"[ObjectPlacer] 预制体索引 {currentPrefabIndex} 为空！");
            return;
        }

        // 实例化物体
        GameObject newObject = Instantiate(prefab, currentPlacementPosition, Quaternion.identity);
        
        // 确保物体有 BalanceableObject 组件
        BalanceableObject balanceableObject = newObject.GetComponent<BalanceableObject>();
        
        if (balanceableObject == null)
        {
            balanceableObject = newObject.AddComponent<BalanceableObject>();
            
            if (logPlacementEvents)
            {
                Debug.LogWarning($"[ObjectPlacer] {newObject.name} 没有 BalanceableObject 组件，已自动添加。");
            }
        }

        // 注册到平衡管理器
        if (balanceManager != null)
        {
            balanceManager.RegisterObject(balanceableObject);
            
            if (logPlacementEvents)
            {
                Debug.Log($"[ObjectPlacer] 放置物体: {newObject.name} 在位置 {currentPlacementPosition}");
                
                // 计算并记录初始质心
                balanceableObject.UpdateWorldCenterOfMass();
                Vector2 initialCOM = balanceableObject.CalculateCombinedCenterOfMass();
                Debug.Log($"[ObjectPlacer] 初始组合质心: {initialCOM}");
                
                // 如果有支撑物体，检查稳定性
                if (balanceableObject.objectsBelow.Count > 0)
                {
                    string stabilityReport = balanceManager.GetStabilityReport(balanceableObject);
                    Debug.Log($"[ObjectPlacer] 稳定性报告: {stabilityReport}");
                }
            }
        }
    }

    /// <summary>
    /// 在指定位置放置物体
    /// </summary>
    public void PlaceObjectAt(Vector3 position)
    {
        currentPlacementPosition = position;
        PlaceObject();
    }

    /// <summary>
    /// 切换到下一个预制体
    /// </summary>
    public void SwitchPrefab()
    {
        if (placeablePrefabs == null || placeablePrefabs.Length == 0) return;

        currentPrefabIndex = (currentPrefabIndex + 1) % placeablePrefabs.Length;
        UpdatePreviewObject();

        if (logPlacementEvents)
        {
            Debug.Log($"[ObjectPlacer] 切换到预制体: {placeablePrefabs[currentPrefabIndex].name}");
        }
    }

    /// <summary>
    /// 创建预览对象
    /// </summary>
    void CreatePreviewObject()
    {
        if (placeablePrefabs == null || placeablePrefabs.Length == 0) return;

        // 确保索引在有效范围内
        if (currentPrefabIndex < 0 || currentPrefabIndex >= placeablePrefabs.Length)
        {
            currentPrefabIndex = 0;
        }

        GameObject prefab = placeablePrefabs[currentPrefabIndex];
        if (prefab == null) return;

        previewObject = Instantiate(prefab, currentPlacementPosition, Quaternion.identity);
        previewObject.name = "PlacementPreview";
        
        // 禁用所有脚本组件
        MonoBehaviour[] scripts = previewObject.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            script.enabled = false;
        }

        // 设置半透明
        SpriteRenderer[] renderers = previewObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            Color color = renderer.color;
            color.a = 0.5f;
            renderer.color = color;
        }
    }

    /// <summary>
    /// 更新预览对象
    /// </summary>
    void UpdatePreviewObject()
    {
        if (!showPlacementPreview) return;

        if (previewObject != null)
        {
            Destroy(previewObject);
        }

        CreatePreviewObject();
    }

    void OnDestroy()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 绘制放置位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(currentPlacementPosition, 0.2f);
        Gizmos.DrawLine(
            currentPlacementPosition + Vector3.left * 0.3f,
            currentPlacementPosition + Vector3.right * 0.3f
        );
        Gizmos.DrawLine(
            currentPlacementPosition + Vector3.up * 0.3f,
            currentPlacementPosition + Vector3.down * 0.3f
        );
    }
}

