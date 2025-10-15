using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 摇摆阶段UI Slider显示器 - 将倾斜阶段可视化为Slider
/// </summary>
[RequireComponent(typeof(Slider))]
public class SwayPhaseSlider : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("摇摆控制器引用")]
    public SwayController swayController;

    [Header("显示设置")]
    [Tooltip("是否启用平滑过渡")]
    public bool useSmoothTransition = true;

    [Tooltip("Slider变化的平滑速度（将自动匹配SwayController的过渡时间）")]
    [HideInInspector]
    public float transitionSpeed = 1f;

    [Header("视觉反馈")]
    [Tooltip("是否根据阶段改变Slider颜色")]
    public bool changeColorByPhase = true;

    [Tooltip("安全区域颜色（居中）")]
    public Color safeColor = Color.green;

    [Tooltip("警告区域颜色（倾1、倾2）")]
    public Color warningColor = Color.yellow;

    [Tooltip("危险区域颜色（倾倒）")]
    public Color dangerColor = Color.red;

    [Header("调试")]
    public bool showDebugInfo = false;

    // 私有变量
    private Slider slider;
    private Image fillImage;
    private float targetValue;
    private float currentValue;
    private float transitionVelocity;

    void Start()
    {
        // 获取Slider组件
        slider = GetComponent<Slider>();
        
        // 配置Slider基本属性
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false; // 不可交互，仅用于显示

        // 获取Fill Image（用于改变颜色）
        if (slider.fillRect != null)
        {
            fillImage = slider.fillRect.GetComponent<Image>();
        }

        // 自动查找SwayController
        if (swayController == null)
        {
            swayController = FindObjectOfType<SwayController>();
        }

        if (swayController == null)
        {
            Debug.LogError("[SwayPhaseSlider] 未找到SwayController！");
            enabled = false;
            return;
        }

        // 订阅阶段改变事件
        swayController.OnPhaseChanged += OnPhaseChanged;

        // 初始化Slider值
        currentValue = GetSliderValueForPhase(swayController.CurrentPhase);
        targetValue = currentValue;
        slider.value = currentValue;

        // 自动匹配过渡时间
        UpdateTransitionSpeed();

        UpdateSliderColor(swayController.CurrentPhase);

        if (showDebugInfo)
        {
            Debug.Log($"[SwayPhaseSlider] 初始化完成 - 当前值: {currentValue}");
        }
    }

    void Update()
    {
        if (slider == null) return;

        // 平滑过渡到目标值
        if (useSmoothTransition)
        {
            currentValue = Mathf.SmoothDamp(
                currentValue,
                targetValue,
                ref transitionVelocity,
                1f / transitionSpeed
            );
        }
        else
        {
            currentValue = targetValue;
        }

        slider.value = currentValue;
    }

    /// <summary>
    /// 当摇摆阶段改变时调用
    /// </summary>
    void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase)
    {
        targetValue = GetSliderValueForPhase(newPhase);
        
        // 更新过渡速度（匹配SwayController的过渡时间）
        UpdateTransitionSpeed();

        // 更新颜色
        if (changeColorByPhase)
        {
            UpdateSliderColor(newPhase);
        }

        if (showDebugInfo)
        {
            Debug.Log($"[SwayPhaseSlider] 阶段改变: {oldPhase.GetChineseName()} → {newPhase.GetChineseName()}, 目标值: {targetValue}");
        }
    }

    /// <summary>
    /// 根据阶段获取对应的Slider值（反向：左倾在右，右倾在左）
    /// </summary>
    float GetSliderValueForPhase(SwayPhase phase)
    {
        switch (phase)
        {
            case SwayPhase.LeftTopple:  return 1.0f;   // 反向：左倾倒在最右边
            case SwayPhase.LeftTilt2:   return 0.85f;  // 反向
            case SwayPhase.LeftTilt1:   return 0.65f;  // 反向
            case SwayPhase.Center:      return 0.5f;   // 居中保持不变
            case SwayPhase.RightTilt1:  return 0.25f;  // 反向
            case SwayPhase.RightTilt2:  return 0.15f;  // 反向
            case SwayPhase.RightTopple: return 0.0f;   // 反向：右倾倒在最左边
            default:                    return 0.5f;
        }
    }

    /// <summary>
    /// 根据阶段更新Slider颜色
    /// </summary>
    void UpdateSliderColor(SwayPhase phase)
    {
        if (fillImage == null) return;

        Color targetColor;

        switch (phase)
        {
            case SwayPhase.Center:
                targetColor = safeColor;
                break;

            case SwayPhase.LeftTilt1:
            case SwayPhase.RightTilt1:
            case SwayPhase.LeftTilt2:
            case SwayPhase.RightTilt2:
                targetColor = warningColor;
                break;

            case SwayPhase.LeftTopple:
            case SwayPhase.RightTopple:
                targetColor = dangerColor;
                break;

            default:
                targetColor = safeColor;
                break;
        }

        fillImage.color = targetColor;
    }

    /// <summary>
    /// 更新过渡速度以匹配SwayController的过渡时间
    /// </summary>
    void UpdateTransitionSpeed()
    {
        if (swayController != null)
        {
            float transitionDuration = swayController.transitionDuration;
            transitionSpeed = transitionDuration > 0 ? 1f / transitionDuration : 1f;
        }
    }

    /// <summary>
    /// 手动设置Slider值（用于测试）
    /// </summary>
    public void SetSliderValue(float value)
    {
        targetValue = Mathf.Clamp01(value);
    }

    /// <summary>
    /// 获取当前Slider值
    /// </summary>
    public float GetCurrentValue()
    {
        return currentValue;
    }

    /// <summary>
    /// 获取目标Slider值
    /// </summary>
    public float GetTargetValue()
    {
        return targetValue;
    }

    void OnDestroy()
    {
        if (swayController != null)
        {
            swayController.OnPhaseChanged -= OnPhaseChanged;
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 12;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.cyan;

        string info = "Slider 调试信息：\n";
        info += $"当前值: {currentValue:F3}\n";
        info += $"目标值: {targetValue:F3}\n";
        
        if (swayController != null)
        {
            info += $"当前阶段: {swayController.CurrentPhase.GetChineseName()}\n";
        }

        GUI.Box(new Rect(10, 450, 220, 90), info, style);
    }
}


