using UnityEngine;

/// <summary>
/// 摇摆系统演示和使用说明
/// </summary>
public class SwaySystemDemo : MonoBehaviour
{
    [Header("系统说明")]
    [TextArea(30, 50)]
    public string systemDocumentation = @"
=== 摇摆倾斜系统完整说明 ===

【系统概述】
这是一个基于状态机的物体摇摆/倾斜系统，用于模拟一摞2D精灵的倾斜和掉落效果。

【核心组件】

1. SwayPhase（摇摆阶段枚举）
   定义7个倾斜阶段：
   • 左倾倒 (LeftTopple)    -3
   • 左倾2 (LeftTilt2)      -2
   • 左倾1 (LeftTilt1)      -1
   • 居中 (Center)           0  [默认]
   • 右倾1 (RightTilt1)      1
   • 右倾2 (RightTilt2)      2
   • 右倾倒 (RightTopple)    3

2. SwayController（摇摆控制器）
   • 管理阶段转换
   • 处理玩家输入
   • 执行自动倾斜逻辑
   • 管理各种倒计时

3. StackController（堆叠控制器）
   • 管理堆叠物体
   • 处理掉落逻辑
   • 创建和移除物体

---

【系统规则】

1. 阶段转换
   ✓ 所有转换都有1秒过渡动画
   ✓ 转换期间不接受新的输入
   ✓ 使用平滑的旋转曲线

2. 居中阶段（Center）
   • 每隔4-8秒随机触发
   • 随机进入任意倾斜阶段（不包括倾倒和居中）
   • 可选择的阶段：左倾1、左倾2、右倾1、右倾2

3. 倾斜阶段（Tilt1/Tilt2）
   • 进入后开始5秒倒计时
   • 倒计时结束且阶段未改变时：
     → 自动转换到同方向的下一个阶段
   • 左倾1 → 左倾2 → 左倾倒
   • 右倾1 → 右倾2 → 右倾倒

4. 倾倒阶段（Topple）
   • 立即掉落1个物体
   • 开始3秒倒计时
   • 倒计时结束后：
     → 掉落1-2个物体（随机）
     → 重新开始3秒倒计时
     → 循环重复

---

【玩家控制】

默认按键：
  ← (左箭头)  - 向左倾斜一级
  → (右箭头)  - 向右倾斜一级
  ↓ (下箭头)  - 向中心移动一级
  ↑ (上箭头)  - 稳定当前阶段（重置倒计时）

操作逻辑：
• 向左/右：当前阶段值 ±1
• 向中心：向0移动一级
• 稳定：重置当前倾斜阶段的5秒倒计时
• 倾倒阶段：无法通过按键移动

示例：
  居中 + ← = 左倾1
  左倾1 + ← = 左倾2
  左倾2 + → = 左倾1
  左倾1 + ↓ = 居中
  右倾2 + ↑ = 重置倒计时（保持右倾2）

---

【快速设置】

步骤1：创建游戏对象
GameObject stackSystem = new GameObject(""StackSystem"");

步骤2：添加SwayController
SwayController sway = stackSystem.AddComponent<SwayController>();
sway.transitionDuration = 1.0f;
sway.centerPhaseMinInterval = 4f;
sway.centerPhaseMaxInterval = 8f;
sway.tiltPhaseCountdown = 5f;
sway.toppleFirstDropCountdown = 3f;

步骤3：添加StackController
StackController stack = stackSystem.AddComponent<StackController>();
stack.initialStackCount = 5;
stack.objectSpacing = 1.0f;

步骤4：运行游戏
• 系统自动创建堆叠物体
• 从居中阶段开始
• 4-8秒后随机倾斜

---

【自定义物体预制体】

1. 创建Sprite对象
   GameObject obj = new GameObject(""StackBox"");
   SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
   sr.sprite = yourSprite;

2. 设置为预制体
   • 保存到Assets文件夹
   • 拖拽到StackController.objectPrefab

3. 自定义属性
   • 大小、颜色、材质
   • 动画、特效
   • 自定义脚本

---

【高级功能】

1. 事件订阅
   swayController.OnPhaseChanged += (oldPhase, newPhase) => {
       Debug.Log($""阶段改变: {oldPhase} → {newPhase}"");
       // 播放音效、特效等
   };

   swayController.OnToppleDrop += () => {
       Debug.Log(""物体掉落！"");
       // 震动、音效、粒子效果
   };

2. 程序化控制
   // 强制改变阶段
   swayController.ChangePhase(SwayPhase.RightTilt2);
   
   // 添加物体到堆叠
   stackController.AddObjectToStack();
   
   // 移除顶部物体
   GameObject top = stackController.RemoveTopObject();
   
   // 重建堆叠
   stackController.RebuildStack(10);

3. 动态调整参数
   // 改变倒计时时间
   swayController.tiltPhaseCountdown = 3f;
   
   // 改变随机间隔
   swayController.centerPhaseMinInterval = 2f;
   swayController.centerPhaseMaxInterval = 6f;

---

【可视化调试】

1. SwayController调试信息
   • 当前阶段名称
   • 旋转角度
   • 是否在转换中
   • 当前倒计时

2. StackController调试信息
   • 当前堆叠高度
   • 物体列表

3. Scene视图
   • 观察旋转动画
   • 查看掉落效果
   • 监控堆叠变化

---

【参数调整指南】

transitionDuration（转换时长）
  • 默认: 1.0秒
  • 较小: 快速转换，反应灵敏
  • 较大: 缓慢转换，更优雅
  • 推荐: 0.5 - 2.0秒

centerPhaseMinInterval/MaxInterval（居中随机间隔）
  • 默认: 4-8秒
  • 较小: 频繁倾斜，挑战性高
  • 较大: 平静期长，容易应对
  • 推荐: 3-10秒

tiltPhaseCountdown（倾斜倒计时）
  • 默认: 5秒
  • 较小: 快速恶化，紧张刺激
  • 较大: 充足时间，容易稳定
  • 推荐: 3-8秒

toppleFirstDropCountdown（倾倒掉落间隔）
  • 默认: 3秒
  • 较小: 快速连续掉落
  • 较大: 有时间救援
  • 推荐: 2-5秒

objectSpacing（物体间距）
  • 默认: 1.0
  • 应匹配物体高度
  • 影响视觉效果
  • 推荐: 0.8 - 1.5

---

【游戏设计建议】

1. 难度曲线
   • 初期：长倒计时，慢速转换
   • 中期：标准参数
   • 后期：短倒计时，快速转换

2. 计分系统
   • 每秒存活 +10分
   • 成功稳定 +50分
   • 物体掉落 -100分
   • 堆叠高度奖励

3. 强化道具
   • 时间减速：延长倒计时
   • 自动居中：立即回到中心
   • 稳定护盾：免疫一次倾倒
   • 堆叠增高：添加物体

4. 视觉反馈
   • 倒计时UI（进度条、数字）
   • 阶段颜色指示
   • 震动效果（倾倒时）
   • 粒子特效（掉落时）

5. 音效设计
   • 倾斜音（whoosh）
   • 警告音（倒计时最后1秒）
   • 掉落音（crash）
   • 稳定成功音（chime）

---

【扩展功能实现】

1. 多堆叠系统
   • 同时管理多个堆叠
   • 堆叠间相互影响
   • 连锁倾倒效果

2. 物理碰撞
   • 掉落物体碰撞检测
   • 撞击其他堆叠
   • 反弹效果

3. 特殊物体
   • 重物体：加速倾斜
   • 轻物体：减缓倾斜
   • 粘性物体：延长倒计时
   • 炸弹物体：掉落时爆炸

4. 环境因素
   • 风力：随机推力
   • 地震：随机震动
   • 重力变化

---

【性能优化】

1. 对象池
   • 重用掉落的物体
   • 减少Instantiate/Destroy调用

2. 协程优化
   • 避免每帧创建新协程
   • 使用WaitForSeconds缓存

3. 批量操作
   • 合批同类操作
   • 延迟非关键更新

---

【常见问题】

Q: 如何禁用自动倾斜？
A: 设置 centerPhaseMaxInterval = Mathf.Infinity

Q: 如何让堆叠永不倾倒？
A: 在到达倾倒前强制回中心：
   if (phase == SwayPhase.LeftTilt2 || phase == SwayPhase.RightTilt2) {
       swayController.ChangePhase(SwayPhase.Center);
   }

Q: 如何自定义掉落方向？
A: 修改 StackController.ApplyDropEffect() 方法

Q: 如何添加网络多人？
A: 同步 currentPhase 和 stackedObjects 数组
   使用RPC调用 ChangePhase() 方法

---

【调试技巧】

1. 慢动作测试
   Time.timeScale = 0.5f; // 半速
   Time.timeScale = 0.1f; // 十分之一速

2. 强制阶段
   [ContextMenu(""测试左倾倒"")]
   void TestLeftTopple() {
       swayController.ChangePhase(SwayPhase.LeftTopple);
   }

3. 日志筛选
   启用 logPhaseChanges 和 logDropEvents
   在Console中筛选 [SwayController] 和 [StackController]

4. 可视化调试
   在Scene视图中观察
   启用showDebugInfo显示实时数据

===========================
";

    [Header("快速测试")]
    [Tooltip("点击测试左倾倒")]
    public bool testLeftTopple = false;
    
    [Tooltip("点击测试右倾倒")]
    public bool testRightTopple = false;
    
    [Tooltip("点击回到居中")]
    public bool testCenter = false;
    
    [Tooltip("点击添加5个物体")]
    public bool testAddObjects = false;

    private SwayController swayController;
    private StackController stackController;

    void Start()
    {
        swayController = GetComponent<SwayController>();
        stackController = GetComponent<StackController>();

        if (swayController == null)
        {
            Debug.LogError("[SwaySystemDemo] 未找到 SwayController 组件！");
        }

        if (stackController == null)
        {
            Debug.LogError("[SwaySystemDemo] 未找到 StackController 组件！");
        }

        // 订阅事件
        if (swayController != null)
        {
            swayController.OnPhaseChanged += OnPhaseChanged;
            swayController.OnToppleDrop += OnToppleDrop;
        }

        PrintUsageInstructions();
    }

    void OnValidate()
    {
        if (!Application.isPlaying) return;

        if (testLeftTopple)
        {
            testLeftTopple = false;
            if (swayController != null)
            {
                swayController.ChangePhase(SwayPhase.LeftTopple);
            }
        }

        if (testRightTopple)
        {
            testRightTopple = false;
            if (swayController != null)
            {
                swayController.ChangePhase(SwayPhase.RightTopple);
            }
        }

        if (testCenter)
        {
            testCenter = false;
            if (swayController != null)
            {
                swayController.ChangePhase(SwayPhase.Center);
            }
        }

        if (testAddObjects)
        {
            testAddObjects = false;
            if (stackController != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    stackController.AddObjectToStack();
                }
            }
        }
    }

    void PrintUsageInstructions()
    {
        Debug.Log("=== 摇摆倾斜系统使用说明 ===");
        Debug.Log("【控制按键】");
        Debug.Log("  ← (左箭头) - 向左倾斜");
        Debug.Log("  → (右箭头) - 向右倾斜");
        Debug.Log("  ↓ (下箭头) - 向中心移动");
        Debug.Log("  ↑ (上箭头) - 稳定当前阶段");
        Debug.Log("");
        Debug.Log("【系统阶段】");
        Debug.Log("  左倾倒 ← 左倾2 ← 左倾1 ← 居中 → 右倾1 → 右倾2 → 右倾倒");
        Debug.Log("");
        Debug.Log("【自动行为】");
        Debug.Log("  • 居中：4-8秒后随机倾斜");
        Debug.Log("  • 倾斜：5秒后自动恶化");
        Debug.Log("  • 倾倒：3秒循环掉落物体");
        Debug.Log("========================");
    }

    void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase)
    {
        Debug.Log($"[Demo] 阶段转换: {oldPhase.GetChineseName()} → {newPhase.GetChineseName()}");
        
        // 这里可以添加音效、特效等
        // PlaySound(newPhase);
        // TriggerVisualEffect(newPhase);
    }

    void OnToppleDrop()
    {
        Debug.Log("[Demo] 物体掉落！");
        
        // 这里可以添加震动、音效、粒子效果等
        // CameraShake();
        // PlayDropSound();
        // SpawnParticles();
    }

    [ContextMenu("测试：强制左倾倒")]
    void ForceLeftTopple()
    {
        if (swayController != null)
        {
            swayController.ChangePhase(SwayPhase.LeftTopple);
        }
    }

    [ContextMenu("测试：强制右倾倒")]
    void ForceRightTopple()
    {
        if (swayController != null)
        {
            swayController.ChangePhase(SwayPhase.RightTopple);
        }
    }

    [ContextMenu("测试：回到居中")]
    void ForceCenter()
    {
        if (swayController != null)
        {
            swayController.ChangePhase(SwayPhase.Center);
        }
    }

    [ContextMenu("测试：添加10个物体")]
    void Add10Objects()
    {
        if (stackController != null)
        {
            for (int i = 0; i < 10; i++)
            {
                stackController.AddObjectToStack();
            }
        }
    }

    [ContextMenu("测试：清空堆叠")]
    void ClearStack()
    {
        if (stackController != null)
        {
            stackController.ClearStack();
        }
    }

    [ContextMenu("测试：重建堆叠")]
    void RebuildStack()
    {
        if (stackController != null)
        {
            stackController.RebuildStack(5);
        }
    }

    void OnDestroy()
    {
        if (swayController != null)
        {
            swayController.OnPhaseChanged -= OnPhaseChanged;
            swayController.OnToppleDrop -= OnToppleDrop;
        }
    }
}

