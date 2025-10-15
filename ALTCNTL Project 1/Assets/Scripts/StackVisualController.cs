using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 堆叠视觉控制器 - 根据倾斜阶段动态调整物体位置，模拟真实堆叠效果
/// </summary>
public class StackVisualController : MonoBehaviour
{
    [Header("视觉偏移参数")]
    [Tooltip("每级倾斜的基础x轴偏移量")]
    public float baseOffsetPerTilt = 0.1f;
    
    [Tooltip("高度影响系数（越高的物体偏移越大）")]
    [Range(0f, 2f)]
    public float heightMultiplier = 1.0f;
    
    [Tooltip("物体间隔（Y轴）")]
    public float objectSpacing = 1.0f;
    
    [Tooltip("最低端物体的基准x位置")]
    public float baseXPosition = 0f;

    [Header("偏移模式")]
    [Tooltip("偏移计算模式")]
    public OffsetMode offsetMode = OffsetMode.Linear;
    
    public enum OffsetMode
    {
        Linear,         // 线性偏移（均匀）
        Exponential,    // 指数偏移（越高偏移越明显）
        Sine,           // 正弦曲线偏移（更自然）
        Custom          // 自定义曲线
    }
    
    [Tooltip("自定义偏移曲线（当模式为Custom时使用）")]
    public AnimationCurve customOffsetCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("动画设置")]
    [Tooltip("位置调整的平滑时间")]
    [Range(0f, 1f)]
    public float smoothTime = 0.2f;
    
    [Tooltip("是否启用平滑移动")]
    public bool useSmoothMovement = true;

    [Header("引用")]
    public StackController stackController;
    public SwayController swayController;

    [Header("调试")]
    public bool showDebugInfo = true;
    public bool logOffsetCalculations = false;

    // 私有变量
    private Dictionary<GameObject, Vector3> targetPositions = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Vector3> velocities = new Dictionary<GameObject, Vector3>();

    void Start()
    {
        if (stackController == null)
        {
            stackController = GetComponent<StackController>();
        }

        if (swayController == null)
        {
            swayController = GetComponent<SwayController>();
        }

        // 订阅阶段改变事件
        if (swayController != null)
        {
            swayController.OnPhaseChanged += OnPhaseChanged;
        }

        // 延迟初始化，确保StackController已创建物体
        StartCoroutine(DelayedInitialization());
    }

    /// <summary>
    /// 延迟初始化，等待StackController创建完物体
    /// </summary>
    System.Collections.IEnumerator DelayedInitialization()
    {
        // 等待一帧，确保StackController.Start已执行
        yield return null;
        
        // 初始化位置
        RefreshPositions();
        
        if (logOffsetCalculations)
        {
            Debug.Log("[StackVisualController] 初始化完成");
        }
    }

    void Update()
    {
        UpdateStackVisuals();
    }

    /// <summary>
    /// 当摇摆阶段改变时调用
    /// </summary>
    void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase)
    {
        // 阶段改变时重新计算位置
        CalculateAllPositions();
    }

    /// <summary>
    /// 更新堆叠物体的视觉位置
    /// </summary>
    void UpdateStackVisuals()
    {
        if (stackController == null || stackController.stackedObjects == null || stackController.stackedObjects.Count == 0)
            return;

        // 计算目标位置
        CalculateAllPositions();

        // 应用位置
        ApplyPositions();
    }

    /// <summary>
    /// 计算所有物体的目标位置（使用累积滑落效果）
    /// </summary>
    void CalculateAllPositions()
    {
        if (stackController == null || stackController.stackedObjects == null || stackController.stackedObjects.Count == 0)
            return;

        List<GameObject> objects = stackController.stackedObjects;
        int objectCount = objects.Count;

        // 获取当前倾斜阶段和方向
        SwayPhase currentPhase = swayController != null ? swayController.CurrentPhase : SwayPhase.Center;
        int tiltDirection = currentPhase.GetDirection(); // -1, 0, 1
        int tiltLevel = Mathf.Abs((int)currentPhase);    // 0-3

        // 最低端物体（索引0）保持在基准位置
        GameObject baseObject = objects[0];
        Vector3 basePos = new Vector3(baseXPosition, 0, 0);
        targetPositions[baseObject] = basePos;

        if (logOffsetCalculations)
        {
            Debug.Log($"[StackVisual] 计算位置 - 阶段: {currentPhase.GetChineseName()}, 方向: {tiltDirection}, 级别: {tiltLevel}");
        }

        // 累积偏移：每个物品在前一个物品的基础上继续滑落
        float cumulativeXOffset = 0f;

        // 计算其他物体的位置
        for (int i = 1; i < objectCount; i++)
        {
            GameObject obj = objects[i];
            
            // 计算Y位置（垂直堆叠）
            float yPos = i * objectSpacing;
            
            // 计算本层的滑落增量
            float layerSlide = CalculateLayerSlide(i, objectCount, tiltLevel, tiltDirection);
            
            // 累积滑落
            cumulativeXOffset += layerSlide;
            
            // 最终X位置 = 基准位置 + 累积偏移
            float xPos = baseXPosition + cumulativeXOffset;
            
            // 设置目标位置（相对于StackRoot）
            targetPositions[obj] = new Vector3(xPos, yPos, 0);

            if (logOffsetCalculations)
            {
                Debug.Log($"[StackVisual] 物体 {i}: 本层滑落={layerSlide:F3}, 累积偏移={cumulativeXOffset:F3}, 目标位置={targetPositions[obj]}");
            }
        }
    }

    /// <summary>
    /// 计算单层的滑落增量
    /// </summary>
    float CalculateLayerSlide(int objectIndex, int totalCount, int tiltLevel, int tiltDirection)
    {
        if (tiltLevel == 0 || tiltDirection == 0)
            return 0f; // 居中阶段无滑落

        // 归一化高度（0-1，0为最底部，1为最顶部）
        float normalizedHeight = (float)objectIndex / (totalCount - 1);
        
        // 基础滑落量（每层的基础偏移）
        float baseSlide = baseOffsetPerTilt * tiltLevel;
        
        // 根据模式计算高度系数
        float heightFactor = CalculateHeightFactor(normalizedHeight);
        
        // 本层滑落 = 基础滑落 * 高度系数 * 高度影响系数 * 方向
        float layerSlide = baseSlide * heightFactor * heightMultiplier * tiltDirection;
        
        return layerSlide;
    }

    /// <summary>
    /// 根据偏移模式计算高度影响系数
    /// </summary>
    float CalculateHeightFactor(float normalizedHeight)
    {
        switch (offsetMode)
        {
            case OffsetMode.Linear:
                // 线性：高度越高，系数越大（0到1）
                return normalizedHeight;

            case OffsetMode.Exponential:
                // 指数：高度越高，增长越快
                return Mathf.Pow(normalizedHeight, 2f);

            case OffsetMode.Sine:
                // 正弦：更自然的曲线
                return Mathf.Sin(normalizedHeight * Mathf.PI * 0.5f);

            case OffsetMode.Custom:
                // 自定义曲线
                return customOffsetCurve.Evaluate(normalizedHeight);

            default:
                return normalizedHeight;
        }
    }

    /// <summary>
    /// 应用计算好的位置到物体
    /// </summary>
    void ApplyPositions()
    {
        if (stackController == null) return;

        foreach (var obj in stackController.stackedObjects)
        {
            if (obj == null) continue;
            if (!targetPositions.ContainsKey(obj)) continue;

            Vector3 targetPos = targetPositions[obj];

            if (useSmoothMovement && smoothTime > 0)
            {
                // 平滑移动
                if (!velocities.ContainsKey(obj))
                {
                    velocities[obj] = Vector3.zero;
                }

                Vector3 currentVelocity = velocities[obj];
                obj.transform.localPosition = Vector3.SmoothDamp(
                    obj.transform.localPosition,
                    targetPos,
                    ref currentVelocity,
                    smoothTime
                );
                velocities[obj] = currentVelocity;
            }
            else
            {
                // 直接设置
                obj.transform.localPosition = targetPos;
            }
        }
    }

    /// <summary>
    /// 获取最低端物体的x轴位置
    /// </summary>
    public float GetBaseObjectXPosition()
    {
        if (stackController == null || stackController.stackedObjects == null || stackController.stackedObjects.Count == 0)
            return baseXPosition;

        GameObject baseObject = stackController.stackedObjects[0];
        if (baseObject == null)
            return baseXPosition;
            
        return baseObject.transform.localPosition.x;
    }

    /// <summary>
    /// 获取所有堆叠物品的x轴位置数组
    /// </summary>
    public float[] GetAllXPositions()
    {
        if (stackController == null || stackController.stackedObjects == null || stackController.stackedObjects.Count == 0)
            return new float[0];

        List<GameObject> objects = stackController.stackedObjects;
        float[] xPositions = new float[objects.Count];

        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] != null)
            {
                xPositions[i] = objects[i].transform.localPosition.x;
            }
            else
            {
                xPositions[i] = baseXPosition; // 如果物体为null，使用基准位置
            }
        }

        return xPositions;
    }

    /// <summary>
    /// 获取指定索引物体的x轴位置
    /// </summary>
    public float GetObjectXPosition(int index)
    {
        if (stackController == null || 
            stackController.stackedObjects == null ||
            index < 0 || 
            index >= stackController.stackedObjects.Count)
            return baseXPosition;

        GameObject obj = stackController.stackedObjects[index];
        return obj != null ? obj.transform.localPosition.x : baseXPosition;
    }

    /// <summary>
    /// 获取堆叠物体的偏移信息（调试用）
    /// </summary>
    public string GetOffsetInfo()
    {
        if (stackController == null || swayController == null)
            return "未初始化";
        
        if (stackController.stackedObjects == null || stackController.stackedObjects.Count == 0)
            return "堆叠为空";

        int count = stackController.GetStackHeight();
        SwayPhase phase = swayController.CurrentPhase;
        float baseX = GetBaseObjectXPosition();
        float[] allX = GetAllXPositions();

        string info = $"堆叠视觉信息：\n";
        info += $"物体数量: {count}\n";
        info += $"当前阶段: {phase.GetChineseName()}\n";
        info += $"基准X位置: {baseX:F3}\n";
        info += $"偏移模式: {offsetMode}\n";
        info += $"X位置列表:\n";

        for (int i = 0; i < allX.Length; i++)
        {
            float offset = allX[i] - baseX;
            info += $"  物体{i}: x={allX[i]:F3} (偏移: {offset:F3})\n";
        }

        return info;
    }

    /// <summary>
    /// 手动刷新所有位置（在添加或移除物体后调用）
    /// </summary>
    public void RefreshPositions()
    {
        // 清空速度缓存
        velocities.Clear();
        
        // 重新计算位置
        CalculateAllPositions();
    }

    /// <summary>
    /// 设置基准x位置
    /// </summary>
    public void SetBaseXPosition(float x)
    {
        baseXPosition = x;
        RefreshPositions();
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;
        if (stackController == null || stackController.stackedObjects == null) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 12;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.cyan;

        string info = GetOffsetInfo();
        GUI.Box(new Rect(10, 240, 280, 200), info, style);
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo || !Application.isPlaying) return;
        if (stackController == null || stackController.stackedObjects == null || stackController.stackedObjects.Count == 0) return;

        // 绘制堆叠物体的连线
        List<GameObject> objects = stackController.stackedObjects;
        
        Gizmos.color = Color.yellow;
        for (int i = 0; i < objects.Count - 1; i++)
        {
            if (objects[i] != null && objects[i + 1] != null)
            {
                Gizmos.DrawLine(
                    objects[i].transform.position,
                    objects[i + 1].transform.position
                );
            }
        }

        // 绘制基准线
        Gizmos.color = Color.green;
        Vector3 basePos = stackController.StackRoot.TransformPoint(new Vector3(baseXPosition, 0, 0));
        Gizmos.DrawLine(
            basePos + Vector3.up * -0.5f,
            basePos + Vector3.up * (objects.Count * objectSpacing + 0.5f)
        );

        // 绘制每个物体的偏移指示
        Gizmos.color = Color.red;
        for (int i = 1; i < objects.Count; i++)
        {
            if (objects[i] == null) continue;
            
            Vector3 objPos = objects[i].transform.position;
            Vector3 baseLinePos = stackController.StackRoot.TransformPoint(
                new Vector3(baseXPosition, i * objectSpacing, 0)
            );
            
            Gizmos.DrawLine(baseLinePos, objPos);
        }
    }

    void OnDestroy()
    {
        if (swayController != null)
        {
            swayController.OnPhaseChanged -= OnPhaseChanged;
        }
    }
}

