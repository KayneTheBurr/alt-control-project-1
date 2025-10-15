using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 摇摆控制器 - 管理物体的倾斜阶段转换
/// </summary>
public class SwayController : MonoBehaviour
{
    [Header("当前状态")]
    [SerializeField] private SwayPhase currentPhase = SwayPhase.Center;
    
    [Header("转换设置")]
    [Tooltip("阶段转换过渡时间（秒）")]
    public float transitionDuration = 1.0f;
    
    [Tooltip("居中阶段随机转换最小间隔（秒）")]
    public float centerPhaseMinInterval = 4f;
    
    [Tooltip("居中阶段随机转换最大间隔（秒）")]
    public float centerPhaseMaxInterval = 8f;
    
    [Tooltip("非居中阶段倒计时时间（秒）")]
    public float tiltPhaseCountdown = 5f;
    
    [Tooltip("倾倒阶段第一次掉落倒计时（秒）")]
    public float toppleFirstDropCountdown = 3f;

    [Header("玩家输入")]
    [Tooltip("小键盘1：立即左倾倒")]
    public KeyCode instantLeftToppleKey = KeyCode.Keypad1;
    
    [Tooltip("小键盘2：向左倾斜一个阶段")]
    public KeyCode tiltLeftKey = KeyCode.Keypad2;
    
    [Tooltip("小键盘3：向右倾斜一个阶段")]
    public KeyCode tiltRightKey = KeyCode.Keypad3;
    
    [Tooltip("小键盘4：立即右倾倒")]
    public KeyCode instantRightToppleKey = KeyCode.Keypad4;

    [Header("长按设置")]
    [Tooltip("长按触发时间（秒）")]
    public float holdDuration = 1.0f;
    
    [Tooltip("是否启用长按重复触发")]
    public bool enableHoldRepeat = true;

    [Header("视觉设置")]
    [Tooltip("堆叠物体的根Transform")]
    public Transform stackRoot;
    
    [Tooltip("旋转动画曲线")]
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("调试")]
    public bool showDebugInfo = true;
    public bool logPhaseChanges = true;

    // 事件
    public event Action<SwayPhase, SwayPhase> OnPhaseChanged; // (oldPhase, newPhase)
    public event Action OnToppleDrop; // 倾倒掉落事件
    
    // 私有变量
    private float currentCountdown = 0f;
    private bool isTransitioning = false;
    private Coroutine transitionCoroutine;
    private Coroutine centerRandomCoroutine;
    private Coroutine tiltCountdownCoroutine;
    private Coroutine toppleDropCoroutine;
    
    // 长按检测变量
    private float leftToppleHoldTime = 0f;
    private float tiltLeftHoldTime = 0f;
    private float tiltRightHoldTime = 0f;
    private float rightToppleHoldTime = 0f;
    private bool leftToppleHoldTriggered = false;
    private bool tiltLeftHoldTriggered = false;
    private bool tiltRightHoldTriggered = false;
    private bool rightToppleHoldTriggered = false;

    // 属性
    public SwayPhase CurrentPhase => currentPhase;
    public bool IsTransitioning => isTransitioning;
    public float CurrentCountdown => currentCountdown;

    void Start()
    {
        if (stackRoot == null)
        {
            stackRoot = transform;
        }

        // 从居中阶段开始
        currentPhase = SwayPhase.Center;
        StartCenterRandomTimer();

        if (logPhaseChanges)
        {
            Debug.Log($"[SwayController] 初始化完成，当前阶段: {currentPhase.GetChineseName()}");
        }
    }

    void Update()
    {
        HandlePlayerInput();
        UpdateDebugDisplay();
    }

    /// <summary>
    /// 处理玩家输入（包含长按检测）
    /// </summary>
    void HandlePlayerInput()
    {
        if (isTransitioning) return;

        // 处理小键盘1（左倾倒）
        HandleKeyInput(
            instantLeftToppleKey, 
            ref leftToppleHoldTime, 
            ref leftToppleHoldTriggered,
            () => HandleLeftToppleInput()
        );

        // 处理小键盘2（左倾斜）
        HandleKeyInput(
            tiltLeftKey, 
            ref tiltLeftHoldTime, 
            ref tiltLeftHoldTriggered,
            () => TryMovePhase(-1)
        );

        // 处理小键盘3（右倾斜）
        HandleKeyInput(
            tiltRightKey, 
            ref tiltRightHoldTime, 
            ref tiltRightHoldTriggered,
            () => TryMovePhase(1)
        );

        // 处理小键盘4（右倾倒）
        HandleKeyInput(
            instantRightToppleKey, 
            ref rightToppleHoldTime, 
            ref rightToppleHoldTriggered,
            () => HandleRightToppleInput()
        );
    }

    /// <summary>
    /// 处理单个按键的输入和长按检测
    /// </summary>
    void HandleKeyInput(KeyCode key, ref float holdTime, ref bool holdTriggered, System.Action action)
    {
        // 按下瞬间触发
        if (Input.GetKeyDown(key))
        {
            holdTime = 0f;
            holdTriggered = false;
            action?.Invoke();
        }
        // 按住检测
        else if (Input.GetKey(key) && enableHoldRepeat)
        {
            holdTime += Time.deltaTime;
            
            // 长按触发
            if (holdTime >= holdDuration && !holdTriggered)
            {
                holdTriggered = true;
                action?.Invoke();
                
                if (logPhaseChanges)
                {
                    Debug.Log($"[SwayController] 长按触发: {key}");
                }
            }
        }
        // 松开重置
        else if (Input.GetKeyUp(key))
        {
            holdTime = 0f;
            holdTriggered = false;
        }
    }

    /// <summary>
    /// 处理左倾倒输入
    /// </summary>
    void HandleLeftToppleInput()
    {
        // 如果在右倾倒阶段，跨越4个阶段
        if (currentPhase == SwayPhase.RightTopple)
        {
            JumpPhases(-4); // 向左跨越4个阶段
        }
        // 否则立即进入左倾倒
        else
        {
            InstantTopple(SwayPhase.LeftTopple);
        }
    }

    /// <summary>
    /// 处理右倾倒输入
    /// </summary>
    void HandleRightToppleInput()
    {
        // 如果在左倾倒阶段，跨越4个阶段
        if (currentPhase == SwayPhase.LeftTopple)
        {
            JumpPhases(4); // 向右跨越4个阶段
        }
        // 否则立即进入右倾倒
        else
        {
            InstantTopple(SwayPhase.RightTopple);
        }
    }

    /// <summary>
    /// 跨越多个阶段
    /// </summary>
    void JumpPhases(int steps)
    {
        int targetValue = (int)currentPhase + steps;
        
        // 限制在有效范围内
        targetValue = Mathf.Clamp(targetValue, (int)SwayPhase.LeftTopple, (int)SwayPhase.RightTopple);
        
        SwayPhase targetPhase = (SwayPhase)targetValue;
        
        if (logPhaseChanges)
        {
            Debug.Log($"[SwayController] 跨越阶段: {currentPhase.GetChineseName()} → {targetPhase.GetChineseName()} (跨越 {Mathf.Abs(steps)} 个阶段)");
        }
        
        ChangePhase(targetPhase);
    }

    /// <summary>
    /// 尝试向指定方向移动阶段
    /// </summary>
    public void TryMovePhase(int direction)
    {
        if (isTransitioning) return;
        if (direction == 0) return;

        SwayPhase targetPhase;

        // 特殊处理：在倾倒阶段时，只允许反方向回退
        if (currentPhase.IsTopplePhase())
        {
            // 在左倾倒阶段
            if (currentPhase == SwayPhase.LeftTopple)
            {
                if (direction > 0) // 按向右键（反方向）
                {
                    targetPhase = SwayPhase.LeftTilt2; // 回到左倾2
                    ChangePhase(targetPhase);
                    
                    if (logPhaseChanges)
                    {
                        Debug.Log("[SwayController] 从倾倒阶段回退到: " + targetPhase.GetChineseName());
                    }
                }
                // 如果按向左键（同方向），不做任何操作
                return;
            }
            // 在右倾倒阶段
            else if (currentPhase == SwayPhase.RightTopple)
            {
                if (direction < 0) // 按向左键（反方向）
                {
                    targetPhase = SwayPhase.RightTilt2; // 回到右倾2
                    ChangePhase(targetPhase);
                    
                    if (logPhaseChanges)
                    {
                        Debug.Log("[SwayController] 从倾倒阶段回退到: " + targetPhase.GetChineseName());
                    }
                }
                // 如果按向右键（同方向），不做任何操作
                return;
            }
            
            return;
        }

        // 正常倾斜阶段的移动逻辑
        int currentValue = (int)currentPhase;
        
        if (direction < 0)
        {
            // 向左移动
            targetPhase = (SwayPhase)Mathf.Max((int)SwayPhase.LeftTopple, currentValue - 1);
        }
        else
        {
            // 向右移动
            targetPhase = (SwayPhase)Mathf.Min((int)SwayPhase.RightTopple, currentValue + 1);
        }

        ChangePhase(targetPhase);
    }

    /// <summary>
    /// 尝试向中心移动一步
    /// </summary>
    public void TryMoveTowardCenter()
    {
        if (isTransitioning) return;
        if (currentPhase.IsCenterPhase()) return;

        SwayPhase targetPhase = currentPhase.GetNextPhaseTowardCenter();
        ChangePhase(targetPhase);
    }

    /// <summary>
    /// 稳定当前阶段（重置倒计时）
    /// </summary>
    public void StabilizeCurrentPhase()
    {
        if (currentPhase.IsTiltPhase())
        {
            // 重置倾斜阶段的倒计时
            if (tiltCountdownCoroutine != null)
            {
                StopCoroutine(tiltCountdownCoroutine);
            }
            tiltCountdownCoroutine = StartCoroutine(TiltPhaseCountdown());
            
            if (logPhaseChanges)
            {
                Debug.Log($"[SwayController] 稳定当前阶段: {currentPhase.GetChineseName()}，倒计时已重置");
            }
        }
    }

    /// <summary>
    /// 改变阶段
    /// </summary>
    public void ChangePhase(SwayPhase newPhase)
    {
        if (isTransitioning) return;
        if (newPhase == currentPhase) return;

        SwayPhase oldPhase = currentPhase;
        currentPhase = newPhase;

        if (logPhaseChanges)
        {
            Debug.Log($"[SwayController] 阶段转换: {oldPhase.GetChineseName()} → {newPhase.GetChineseName()}");
        }

        // 停止所有现有的协程
        StopAllPhaseCoroutines();

        // 开始转换动画
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        transitionCoroutine = StartCoroutine(TransitionToPhase(newPhase));

        // 根据新阶段启动相应的协程
        if (newPhase.IsCenterPhase())
        {
            StartCenterRandomTimer();
        }
        else if (newPhase.IsTiltPhase())
        {
            tiltCountdownCoroutine = StartCoroutine(TiltPhaseCountdown());
        }
        else if (newPhase.IsTopplePhase())
        {
            toppleDropCoroutine = StartCoroutine(ToppleDropCycle());
        }

        // 触发事件
        OnPhaseChanged?.Invoke(oldPhase, newPhase);
    }

    /// <summary>
    /// 立即切换到倾倒阶段并触发一次掉落
    /// </summary>
    public void InstantTopple(SwayPhase topplePhase)
    {
        if (!topplePhase.IsTopplePhase())
        {
            Debug.LogWarning($"[SwayController] InstantTopple 只能用于倾倒阶段，传入的是: {topplePhase}");
            return;
        }

        if (logPhaseChanges)
        {
            Debug.Log($"[SwayController] 立即倾倒: {topplePhase.GetChineseName()}");
        }

        // 切换到倾倒阶段
        ChangePhase(topplePhase);

        // 立即触发一次掉落
        OnToppleDrop?.Invoke();
    }

    /// <summary>
    /// 转换到目标阶段的动画协程
    /// </summary>
    IEnumerator TransitionToPhase(SwayPhase targetPhase)
    {
        isTransitioning = true;
        
        float startRotation = stackRoot.eulerAngles.z;
        // 处理角度环绕问题
        if (startRotation > 180f) startRotation -= 360f;
        
        float targetRotation = targetPhase.GetRotationAngle();
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = rotationCurve.Evaluate(elapsed / transitionDuration);
            
            float currentRotation = Mathf.Lerp(startRotation, targetRotation, t);
            stackRoot.rotation = Quaternion.Euler(0, 0, currentRotation);
            
            yield return null;
        }

        // 确保最终角度准确
        stackRoot.rotation = Quaternion.Euler(0, 0, targetRotation);
        isTransitioning = false;
    }

    /// <summary>
    /// 居中阶段的随机转换协程
    /// </summary>
    IEnumerator CenterRandomTransition()
    {
        while (currentPhase.IsCenterPhase())
        {
            float waitTime = UnityEngine.Random.Range(centerPhaseMinInterval, centerPhaseMaxInterval);
            currentCountdown = waitTime;

            if (logPhaseChanges)
            {
                Debug.Log($"[SwayController] 居中阶段随机转换倒计时: {waitTime:F1}秒");
            }

            while (currentCountdown > 0)
            {
                currentCountdown -= Time.deltaTime;
                yield return null;
                
                // 如果阶段改变了，退出
                if (!currentPhase.IsCenterPhase()) yield break;
            }

            // 随机选择一个倾斜阶段（不包括倾倒和居中）
            SwayPhase[] tiltPhases = SwayPhaseExtensions.GetTiltPhases();
            SwayPhase randomPhase = tiltPhases[UnityEngine.Random.Range(0, tiltPhases.Length)];
            
            ChangePhase(randomPhase);
            yield break; // 退出协程
        }
    }

    /// <summary>
    /// 倾斜阶段的倒计时协程
    /// </summary>
    IEnumerator TiltPhaseCountdown()
    {
        currentCountdown = tiltPhaseCountdown;
        SwayPhase startPhase = currentPhase;

        if (logPhaseChanges)
        {
            Debug.Log($"[SwayController] 倾斜阶段倒计时开始: {tiltPhaseCountdown}秒");
        }

        while (currentCountdown > 0)
        {
            currentCountdown -= Time.deltaTime;
            yield return null;
            
            // 如果阶段改变了，退出
            if (currentPhase != startPhase) yield break;
        }

        // 倒计时结束，转换到同方向的下一个阶段
        SwayPhase nextPhase = currentPhase.GetNextPhaseInSameDirection();
        
        if (logPhaseChanges)
        {
            Debug.Log($"[SwayController] 倾斜阶段倒计时结束，自动转换到: {nextPhase.GetChineseName()}");
        }
        
        ChangePhase(nextPhase);
    }

    /// <summary>
    /// 倾倒阶段的掉落循环协程
    /// </summary>
    IEnumerator ToppleDropCycle()
    {
        if (logPhaseChanges)
        {
            Debug.Log($"[SwayController] 进入倾倒阶段，开始掉落循环");
        }

        // 第一次掉落：1个物体
        OnToppleDrop?.Invoke();
        
        currentCountdown = toppleFirstDropCountdown;
        
        while (currentCountdown > 0)
        {
            currentCountdown -= Time.deltaTime;
            yield return null;
            
            // 如果阶段改变了，退出
            if (!currentPhase.IsTopplePhase()) yield break;
        }

        // 循环：每次掉落1-2个物体
        while (currentPhase.IsTopplePhase())
        {
            int dropCount = UnityEngine.Random.Range(1, 3); // 1或2个
            
            for (int i = 0; i < dropCount; i++)
            {
                OnToppleDrop?.Invoke();
                
                if (logPhaseChanges)
                {
                    Debug.Log($"[SwayController] 倾倒掉落物体 ({i + 1}/{dropCount})");
                }
            }

            currentCountdown = toppleFirstDropCountdown;
            
            while (currentCountdown > 0)
            {
                currentCountdown -= Time.deltaTime;
                yield return null;
                
                if (!currentPhase.IsTopplePhase()) yield break;
            }
        }
    }

    /// <summary>
    /// 开始居中随机计时器
    /// </summary>
    void StartCenterRandomTimer()
    {
        if (centerRandomCoroutine != null)
        {
            StopCoroutine(centerRandomCoroutine);
        }
        centerRandomCoroutine = StartCoroutine(CenterRandomTransition());
    }

    /// <summary>
    /// 停止所有阶段协程
    /// </summary>
    void StopAllPhaseCoroutines()
    {
        if (centerRandomCoroutine != null)
        {
            StopCoroutine(centerRandomCoroutine);
            centerRandomCoroutine = null;
        }
        
        if (tiltCountdownCoroutine != null)
        {
            StopCoroutine(tiltCountdownCoroutine);
            tiltCountdownCoroutine = null;
        }
        
        if (toppleDropCoroutine != null)
        {
            StopCoroutine(toppleDropCoroutine);
            toppleDropCoroutine = null;
        }
    }

    /// <summary>
    /// 更新调试显示
    /// </summary>
    void UpdateDebugDisplay()
    {
        if (!showDebugInfo) return;
        // 调试信息在OnGUI中显示
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 16;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.white;

        string info = $"当前阶段: {currentPhase.GetChineseName()}\n";
        info += $"旋转角度: {currentPhase.GetRotationAngle()}°\n";
        info += $"转换中: {(isTransitioning ? "是" : "否")}\n";
        
        if (currentCountdown > 0)
        {
            info += $"倒计时: {currentCountdown:F1}秒\n";
        }

        info += $"\n操作提示:\n";
        info += $"← 向左倾斜\n";
        info += $"→ 向右倾斜\n";
        info += $"↓ 回中\n";
        info += $"↑ 稳定（重置倒计时）";

        GUI.Box(new Rect(10, 10, 300, 180), info, style);
    }

    void OnDestroy()
    {
        StopAllPhaseCoroutines();
    }
}

