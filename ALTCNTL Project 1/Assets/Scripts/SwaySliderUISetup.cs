using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 摇摆阶段Slider UI快速设置工具
/// </summary>
public class SwaySliderUISetup : MonoBehaviour
{
    [Header("UI设置")]
    [Tooltip("Slider位置")]
    public SliderPosition position = SliderPosition.BottomCenter;

    [Tooltip("Slider宽度")]
    public float sliderWidth = 400f;

    [Tooltip("Slider高度")]
    public float sliderHeight = 30f;

    [Tooltip("距离边缘的间距")]
    public float margin = 50f;

    [Header("视觉设置")]
    [Tooltip("显示阶段标签")]
    public bool showPhaseLabels = true;

    [Tooltip("显示当前阶段文本")]
    public bool showCurrentPhaseText = true;

    [Header("Slider行为设置")]
    [Tooltip("启用平滑过渡")]
    public bool useSmoothTransition = true;

    [Tooltip("根据阶段改变颜色")]
    public bool changeColorByPhase = true;

    [Tooltip("安全区域颜色（居中）")]
    public Color safeColor = Color.green;

    [Tooltip("警告区域颜色（倾1、倾2）")]
    public Color warningColor = Color.yellow;

    [Tooltip("危险区域颜色（倾倒）")]
    public Color dangerColor = Color.red;

    [Tooltip("显示调试信息")]
    public bool showDebugInfo = false;

    [Header("执行设置")]
    [Tooltip("勾选以自动创建UI")]
    public bool createUI = false;

    public enum SliderPosition
    {
        TopCenter,
        BottomCenter,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    [Header("引用")]
    public SwayController swayController;
    
    [HideInInspector]
    public GameObject createdCanvas;

    void Start()
    {
        if (createUI)
        {
            CreateSliderUI();
            createUI = false;
        }
    }

    /// <summary>
    /// 创建Slider UI（可在Inspector右键菜单调用）
    /// </summary>
    [ContextMenu("创建Slider UI")]
    public void CreateSliderUI()
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
            
            createdCanvas = canvasObj;
            Debug.Log("[SwaySliderUISetup] 创建了新的Canvas");
        }

        // 创建Slider容器
        GameObject sliderContainer = new GameObject("SwayPhaseSliderContainer");
        sliderContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = sliderContainer.AddComponent<RectTransform>();
        SetSliderPosition(containerRect);

        // 创建背景
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderContainer.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.sizeDelta = Vector2.zero;

        // 创建Slider
        GameObject sliderObj = new GameObject("PhaseSlider");
        sliderObj.transform.SetParent(sliderContainer.transform, false);

        Slider slider = sliderObj.AddComponent<Slider>();
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.sizeDelta = new Vector2(sliderWidth - 40, sliderHeight - 10);
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
        fillImage.type = Image.Type.Filled;
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.sizeDelta = Vector2.zero;

        // 配置Slider引用
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImage;

        // 创建Handle（可选，用于视觉指示）
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = new Vector2(0, 0);
        handleAreaRect.anchorMax = new Vector2(1, 1);
        handleAreaRect.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = Color.white;
        
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);

        slider.handleRect = handleRect;

        // 创建阶段标签
        if (showPhaseLabels)
        {
            CreatePhaseLabels(sliderContainer.transform);
        }

        // 创建当前阶段文本
        if (showCurrentPhaseText)
        {
            CreateCurrentPhaseText(sliderContainer.transform);
        }

        // 添加SwayPhaseSlider组件
        SwayPhaseSlider phaseSlider = sliderObj.AddComponent<SwayPhaseSlider>();
        phaseSlider.swayController = swayController;
        phaseSlider.useSmoothTransition = useSmoothTransition;
        phaseSlider.changeColorByPhase = changeColorByPhase;
        phaseSlider.safeColor = safeColor;
        phaseSlider.warningColor = warningColor;
        phaseSlider.dangerColor = dangerColor;
        phaseSlider.showDebugInfo = showDebugInfo;

        Debug.Log("[SwaySliderUISetup] ✅ UI Slider创建完成！");
    }

    /// <summary>
    /// 设置Slider容器位置
    /// </summary>
    void SetSliderPosition(RectTransform rect)
    {
        switch (position)
        {
            case SliderPosition.TopCenter:
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0, -margin);
                break;

            case SliderPosition.BottomCenter:
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0, margin);
                break;

            case SliderPosition.TopLeft:
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(margin, -margin);
                break;

            case SliderPosition.TopRight:
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-margin, -margin);
                break;

            case SliderPosition.BottomLeft:
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(margin, margin);
                break;

            case SliderPosition.BottomRight:
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-margin, margin);
                break;
        }

        rect.sizeDelta = new Vector2(sliderWidth, sliderHeight);
    }

    /// <summary>
    /// 创建阶段标签
    /// </summary>
    void CreatePhaseLabels(Transform parent)
    {
        // 标签数据：位置和文本（已反向：左倾在右，右倾在左）
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
    /// 创建当前阶段显示文本
    /// </summary>
    void CreateCurrentPhaseText(Transform parent)
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

    [ContextMenu("删除创建的Canvas")]
    public void DestroyCreatedCanvas()
    {
        if (createdCanvas != null)
        {
            DestroyImmediate(createdCanvas);
            Debug.Log("[SwaySliderUISetup] 已删除创建的Canvas");
        }
    }
}

/// <summary>
/// 阶段文本更新器
/// </summary>
public class SwayPhaseTextUpdater : MonoBehaviour
{
    public SwayController swayController;
    public Text textComponent;

    void Start()
    {
        if (swayController == null)
        {
            swayController = FindObjectOfType<SwayController>();
        }

        if (swayController != null)
        {
            swayController.OnPhaseChanged += OnPhaseChanged;
            UpdateText(swayController.CurrentPhase);
        }
    }

    void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase)
    {
        UpdateText(newPhase);
    }

    void UpdateText(SwayPhase phase)
    {
        if (textComponent != null)
        {
            textComponent.text = $"当前阶段：{phase.GetChineseName()}";
            
            // 根据阶段改变颜色
            switch (phase)
            {
                case SwayPhase.Center:
                    textComponent.color = Color.green;
                    break;
                case SwayPhase.LeftTilt1:
                case SwayPhase.RightTilt1:
                case SwayPhase.LeftTilt2:
                case SwayPhase.RightTilt2:
                    textComponent.color = Color.yellow;
                    break;
                case SwayPhase.LeftTopple:
                case SwayPhase.RightTopple:
                    textComponent.color = Color.red;
                    break;
            }
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


