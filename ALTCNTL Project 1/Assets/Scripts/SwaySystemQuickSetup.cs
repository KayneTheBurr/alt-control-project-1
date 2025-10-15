using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 摇摆系统快速设置 - 一键创建完整的摇摆系统
/// </summary>
public class SwaySystemQuickSetup : MonoBehaviour
{
    [Header("快速设置")]
    [Tooltip("点击此按钮自动创建完整系统")]
    public bool setupSystem = false;

    [Header("配置选项")]
    [Tooltip("初始堆叠物体数量")]
    [Range(3, 20)]
    public int initialStackCount = 5;
    
    [Tooltip("物体间距")]
    [Range(0.5f, 2.0f)]
    public float objectSpacing = 1.0f;
    
    [Tooltip("使用自定义预制体")]
    public GameObject customObjectPrefab;
    
    [Tooltip("创建默认立方体物体")]
    public bool createDefaultCubes = true;

    [Header("UI设置")]
    [Tooltip("自动创建阶段显示UI Slider")]
    public bool createPhaseSlider = true;
    
    [Tooltip("Slider位置")]
    public SwaySliderUISetup.SliderPosition sliderPosition = SwaySliderUISetup.SliderPosition.BottomCenter;
    
    [Tooltip("显示阶段标签")]
    public bool showPhaseLabels = true;
    
    [Tooltip("显示当前阶段文本")]
    public bool showCurrentPhaseText = true;

    [Header("难度设置")]
    [Tooltip("难度预设")]
    public DifficultyPreset difficulty = DifficultyPreset.Normal;

    public enum DifficultyPreset
    {
        Easy,       // 简单：长倒计时，慢速
        Normal,     // 正常：标准参数
        Hard,       // 困难：短倒计时，快速
        Extreme,    // 极难：极短倒计时
        Custom      // 自定义
    }

    [Header("自定义难度参数")]
    [Tooltip("转换过渡时间（秒）")]
    public float transitionDuration = 1.0f;
    
    [Tooltip("居中随机转换最小间隔（秒）")]
    public float centerMinInterval = 4f;
    
    [Tooltip("居中随机转换最大间隔（秒）")]
    public float centerMaxInterval = 8f;
    
    [Tooltip("倾斜阶段倒计时（秒）")]
    public float tiltCountdown = 5f;
    
    [Tooltip("倾倒掉落间隔（秒）")]
    public float toppleDropInterval = 3f;

    void OnValidate()
    {
        if (setupSystem && !Application.isPlaying)
        {
            Debug.LogWarning("[QuickSetup] 请在Play模式下执行快速设置！");
            setupSystem = false;
            return;
        }

        if (setupSystem && Application.isPlaying)
        {
            setupSystem = false;
            PerformQuickSetup();
        }
    }

    [ContextMenu("执行快速设置")]
    void PerformQuickSetup()
    {
        Debug.Log("=== 开始摇摆系统快速设置 ===");

        // 1. 检查是否已有组件
        SwayController existingSway = GetComponent<SwayController>();
        StackController existingStack = GetComponent<StackController>();
        SwaySystemDemo existingDemo = GetComponent<SwaySystemDemo>();

        if (existingSway != null || existingStack != null)
        {
            Debug.LogWarning("[QuickSetup] 系统已存在！如需重新设置，请先移除现有组件。");
            return;
        }

        // 2. 添加SwayController
        SwayController swayController = gameObject.AddComponent<SwayController>();
        ConfigureSwayController(swayController);
        Debug.Log("[QuickSetup] ✓ 已添加 SwayController");

        // 3. 添加StackController
        StackController stackController = gameObject.AddComponent<StackController>();
        ConfigureStackController(stackController);
        Debug.Log("[QuickSetup] ✓ 已添加 StackController");

        // 4. 添加StackVisualController
        StackVisualController visualController = gameObject.AddComponent<StackVisualController>();
        ConfigureVisualController(visualController, stackController);
        Debug.Log("[QuickSetup] ✓ 已添加 StackVisualController");

        // 5. 创建UI Slider（如果启用）
        if (createPhaseSlider)
        {
            CreatePhaseSliderUI(swayController);
            Debug.Log("[QuickSetup] ✓ 已创建 阶段显示UI Slider");
        }

        // 6. 添加演示脚本
        if (existingDemo == null)
        {
            gameObject.AddComponent<SwaySystemDemo>();
            Debug.Log("[QuickSetup] ✓ 已添加 SwaySystemDemo");
        }

        Debug.Log("=== 摇摆系统设置完成 ===");
        Debug.Log("提示：");
        Debug.Log("  • 使用箭头键控制倾斜");
        Debug.Log("  • 查看Game视图的UI Slider和调试信息");
        Debug.Log("  • 查看Scene视图观察效果");
        Debug.Log("========================");
    }

    void ConfigureSwayController(SwayController controller)
    {
        // 根据难度预设配置参数
        switch (difficulty)
        {
            case DifficultyPreset.Easy:
                controller.transitionDuration = 1.5f;
                controller.centerPhaseMinInterval = 6f;
                controller.centerPhaseMaxInterval = 12f;
                controller.tiltPhaseCountdown = 8f;
                controller.toppleFirstDropCountdown = 4f;
                break;

            case DifficultyPreset.Normal:
                controller.transitionDuration = 1.0f;
                controller.centerPhaseMinInterval = 4f;
                controller.centerPhaseMaxInterval = 8f;
                controller.tiltPhaseCountdown = 5f;
                controller.toppleFirstDropCountdown = 3f;
                break;

            case DifficultyPreset.Hard:
                controller.transitionDuration = 0.7f;
                controller.centerPhaseMinInterval = 3f;
                controller.centerPhaseMaxInterval = 6f;
                controller.tiltPhaseCountdown = 3f;
                controller.toppleFirstDropCountdown = 2f;
                break;

            case DifficultyPreset.Extreme:
                controller.transitionDuration = 0.5f;
                controller.centerPhaseMinInterval = 2f;
                controller.centerPhaseMaxInterval = 4f;
                controller.tiltPhaseCountdown = 2f;
                controller.toppleFirstDropCountdown = 1.5f;
                break;

            case DifficultyPreset.Custom:
                controller.transitionDuration = transitionDuration;
                controller.centerPhaseMinInterval = centerMinInterval;
                controller.centerPhaseMaxInterval = centerMaxInterval;
                controller.tiltPhaseCountdown = tiltCountdown;
                controller.toppleFirstDropCountdown = toppleDropInterval;
                break;
        }

        // 设置输入按键（小键盘输入系统 v1.5.0）
        controller.instantLeftToppleKey = KeyCode.Keypad1;
        controller.tiltLeftKey = KeyCode.Keypad2;
        controller.tiltRightKey = KeyCode.Keypad3;
        controller.instantRightToppleKey = KeyCode.Keypad4;
        
        // 长按设置
        controller.holdDuration = 1.0f;
        controller.enableHoldRepeat = true;

        controller.showDebugInfo = true;
        controller.logPhaseChanges = true;
    }

    void ConfigureStackController(StackController controller)
    {
        controller.initialStackCount = initialStackCount;
        controller.objectSpacing = objectSpacing;

        if (customObjectPrefab != null)
        {
            controller.objectPrefab = customObjectPrefab;
        }
        else if (createDefaultCubes)
        {
            // 将使用默认立方体（在StackController中自动创建）
            controller.objectPrefab = null;
        }

        controller.dropForce = 5.0f;
        controller.dropTorque = 2.0f;
        controller.droppedObjectLifetime = 3.0f;
        
        controller.showDebugInfo = true;
        controller.logDropEvents = true;
    }

    void ConfigureVisualController(StackVisualController controller, StackController stackController)
    {
        controller.baseOffsetPerTilt = 0.15f;
        controller.heightMultiplier = 1.0f;
        controller.objectSpacing = objectSpacing;
        controller.offsetMode = StackVisualController.OffsetMode.Exponential;
        controller.smoothTime = 0.2f;
        controller.useSmoothMovement = true;
        controller.showDebugInfo = true;
        controller.logOffsetCalculations = false;
        
        // 确保引用设置正确
        controller.stackController = stackController;
        stackController.visualController = controller;
    }

    /// <summary>
    /// 创建阶段显示UI Slider
    /// </summary>
    void CreatePhaseSliderUI(SwayController swayController)
    {
        // 查找或创建Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("SwayPhaseCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            Debug.Log("[QuickSetup] 创建了新的Canvas");
        }

        // 创建Slider容器
        GameObject sliderContainer = new GameObject("SwayPhaseSliderContainer");
        sliderContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = sliderContainer.AddComponent<RectTransform>();
        SetSliderPosition(containerRect);

        // 创建容器背景
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderContainer.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.sizeDelta = Vector2.zero;

        // 创建Slider主体
        GameObject sliderObj = new GameObject("PhaseSlider");
        sliderObj.transform.SetParent(sliderContainer.transform, false);

        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.sizeDelta = new Vector2(360, 20);
        sliderRect.anchoredPosition = Vector2.zero;

        // 创建Slider Background
        GameObject sliderBg = new GameObject("Background");
        sliderBg.transform.SetParent(sliderObj.transform, false);
        
        Image sliderBgImage = sliderBg.AddComponent<Image>();
        sliderBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        RectTransform sliderBgRect = sliderBg.GetComponent<RectTransform>();
        sliderBgRect.anchorMin = new Vector2(0, 0.25f);
        sliderBgRect.anchorMax = new Vector2(1, 0.75f);
        sliderBgRect.sizeDelta = Vector2.zero;

        // 创建Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.sizeDelta = Vector2.zero;

        // 创建Fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = Color.green;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;

        // 创建Handle Area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);
        handleAreaRect.sizeDelta = new Vector2(-20, 0);

        // 创建Handle
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);

        // 配置Slider组件
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;
        slider.interactable = false;

        // 创建阶段标签
        if (showPhaseLabels)
        {
            CreatePhaseLabels(sliderContainer.transform);
        }

        // 创建当前阶段文本
        if (showCurrentPhaseText)
        {
            CreateCurrentPhaseText(sliderContainer.transform, swayController);
        }

        // 添加SwayPhaseSlider脚本
        SwayPhaseSlider phaseSlider = sliderObj.AddComponent<SwayPhaseSlider>();
        phaseSlider.swayController = swayController;
        phaseSlider.useSmoothTransition = true;
        phaseSlider.changeColorByPhase = true;
        phaseSlider.safeColor = Color.green;
        phaseSlider.warningColor = Color.yellow;
        phaseSlider.dangerColor = Color.red;
        phaseSlider.showDebugInfo = false;
    }

    /// <summary>
    /// 设置Slider容器位置
    /// </summary>
    void SetSliderPosition(RectTransform rect)
    {
        float sliderWidth = 400f;
        float sliderHeight = 100f;
        float margin = 50f;

        switch (sliderPosition)
        {
            case SwaySliderUISetup.SliderPosition.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0, -margin);
                break;

            case SwaySliderUISetup.SliderPosition.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0, margin);
                break;

            case SwaySliderUISetup.SliderPosition.TopLeft:
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(margin, -margin);
                break;

            case SwaySliderUISetup.SliderPosition.TopRight:
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-margin, -margin);
                break;

            case SwaySliderUISetup.SliderPosition.BottomLeft:
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(margin, margin);
                break;

            case SwaySliderUISetup.SliderPosition.BottomRight:
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-margin, margin);
                break;
        }

        rect.sizeDelta = new Vector2(sliderWidth, sliderHeight);
    }

    /// <summary>
    /// 创建阶段标签（反向：左倾在右，右倾在左）
    /// </summary>
    void CreatePhaseLabels(Transform parent)
    {
        // 标签数据：Slider值位置和文本（已反向）
        (float pos, string label)[] labels = new (float, string)[]
        {
            (0.0f, "右倾倒"),   // 反向：右倾倒在最左边
            (0.15f, "右倾2"),   // 反向
            (0.25f, "右倾1"),   // 反向
            (0.5f, "居中"),     // 居中保持不变
            (0.65f, "左倾1"),   // 反向
            (0.85f, "左倾2"),   // 反向
            (1.0f, "左倾倒")    // 反向：左倾倒在最右边
        };

        foreach (var (pos, label) in labels)
        {
            GameObject labelObj = new GameObject($"Label_{label}");
            labelObj.transform.SetParent(parent, false);

            Text text = labelObj.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 10;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            RectTransform textRect = labelObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(pos, 0f);
            textRect.anchorMax = new Vector2(pos, 0f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.sizeDelta = new Vector2(50, 20);
            textRect.anchoredPosition = new Vector2(0, -35);
        }
    }

    /// <summary>
    /// 创建当前阶段文本显示
    /// </summary>
    void CreateCurrentPhaseText(Transform parent, SwayController swayController)
    {
        GameObject textObj = new GameObject("CurrentPhaseText");
        textObj.transform.SetParent(parent, false);

        Text text = textObj.AddComponent<Text>();
        text.text = "当前阶段：居中";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 14;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 0f);
        textRect.sizeDelta = new Vector2(200, 25);
        textRect.anchoredPosition = new Vector2(0, 5);

        // 添加动态更新脚本
        SwayPhaseTextUpdater updater = textObj.AddComponent<SwayPhaseTextUpdater>();
        updater.swayController = swayController;
        updater.textComponent = text;
    }

    [ContextMenu("创建物体预制体模板")]
    void CreateObjectPrefabTemplate()
    {
        // 创建一个示例预制体
        GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
        template.name = "StackObjectTemplate";
        
        // 移除碰撞器
        DestroyImmediate(template.GetComponent<Collider>());
        
        // 设置2D外观
        template.transform.localScale = new Vector3(1f, 1f, 0.2f);
        
        // 添加颜色
        Renderer renderer = template.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.3f, 0.6f, 1f);
        }

        Debug.Log("[QuickSetup] 创建了物体模板: StackObjectTemplate");
        Debug.Log("提示：将此GameObject拖到Project窗口创建预制体，然后赋值给 customObjectPrefab");
        
        // 选中创建的对象
        #if UNITY_EDITOR
        UnityEditor.Selection.activeGameObject = template;
        #endif
    }

    [ContextMenu("显示系统信息")]
    void ShowSystemInfo()
    {
        Debug.Log("=== 摇摆系统信息 ===");
        
        SwayController sway = GetComponent<SwayController>();
        if (sway != null)
        {
            Debug.Log($"✓ SwayController:");
            Debug.Log($"  当前阶段: {sway.CurrentPhase.GetChineseName()}");
            Debug.Log($"  转换时长: {sway.transitionDuration}秒");
            Debug.Log($"  倾斜倒计时: {sway.tiltPhaseCountdown}秒");
        }
        else
        {
            Debug.Log("✗ 未找到 SwayController");
        }

        StackController stack = GetComponent<StackController>();
        if (stack != null)
        {
            Debug.Log($"✓ StackController:");
            Debug.Log($"  堆叠高度: {stack.GetStackHeight()}");
            Debug.Log($"  物体间距: {stack.objectSpacing}");
        }
        else
        {
            Debug.Log("✗ 未找到 StackController");
        }

        Debug.Log("===================");
    }

    [ContextMenu("测试：模拟游戏流程")]
    void SimulateGameFlow()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("请在Play模式下运行此测试");
            return;
        }

        StartCoroutine(SimulateGameFlowCoroutine());
    }

    System.Collections.IEnumerator SimulateGameFlowCoroutine()
    {
        SwayController sway = GetComponent<SwayController>();
        if (sway == null)
        {
            Debug.LogError("未找到 SwayController！");
            yield break;
        }

        Debug.Log("=== 开始游戏流程模拟 ===");

        // 1. 居中阶段（等待自动倾斜）
        Debug.Log("阶段1: 居中，等待自动倾斜...");
        yield return new WaitForSeconds(2f);

        // 2. 手动向右倾斜
        Debug.Log("阶段2: 手动向右倾斜");
        sway.TryMovePhase(1); // 右倾1
        yield return new WaitForSeconds(2f);

        // 3. 继续向右
        Debug.Log("阶段3: 继续向右");
        sway.TryMovePhase(1); // 右倾2
        yield return new WaitForSeconds(2f);

        // 4. 稳定当前阶段
        Debug.Log("阶段4: 稳定（重置倒计时）");
        sway.StabilizeCurrentPhase();
        yield return new WaitForSeconds(2f);

        // 5. 向中心移动
        Debug.Log("阶段5: 向中心移动");
        sway.TryMoveTowardCenter(); // 右倾1
        yield return new WaitForSeconds(1f);
        sway.TryMoveTowardCenter(); // 居中
        yield return new WaitForSeconds(2f);

        Debug.Log("=== 游戏流程模拟完成 ===");
    }
}

