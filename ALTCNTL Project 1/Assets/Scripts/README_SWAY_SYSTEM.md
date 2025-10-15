# 摇摆倾斜系统 (Sway/Tilt System)

> 一个完整的2D物体摇摆、倾斜和堆叠管理系统，不依赖Unity物理引擎

## 📦 包含文件

### 核心系统
- **SwayPhase.cs** - 阶段枚举定义和扩展方法
- **SwayController.cs** - 摇摆控制器（核心逻辑）
- **StackController.cs** - 堆叠物体管理器
- **StackVisualController.cs** - 堆叠视觉控制器（动态偏移）⭐新增

### 工具和演示
- **SwaySystemDemo.cs** - 完整演示和使用示例
- **SwaySystemQuickSetup.cs** - 一键快速设置工具

### UI组件
- **SwayPhaseSlider.cs** - 阶段显示UI Slider⭐最新
- **SwaySliderUISetup.cs** - UI快速创建工具⭐最新

### 文档
- **摇摆系统使用指南.txt** - 详细的中文使用说明
- **堆叠视觉控制说明.txt** - 视觉控制系统详细说明
- **堆叠滑落效果说明.md** - 累积滑落算法详解
- **滑落效果调节指南.txt** - 参数快速参考卡片
- **累积滑落系统更新日志.md** - 更新记录和变更说明
- **UI Slider阶段显示说明.md** - UI Slider详细文档
- **Slider显示快速指南.txt** - UI Slider快速参考
- **玩家输入系统更新说明.md** - 输入系统详细文档
- **输入按键快速参考.txt** - 按键快速参考
- **倾倒阶段回退功能说明.md** - 倾倒回退详细文档
- **倾倒回退快速参考.txt** - 回退功能快速参考
- **长按输入系统说明.md** - 长按和倾倒跨越详细文档⭐最新
- **小键盘输入快速参考.txt** - 小键盘输入快速参考⭐最新
- **README_SWAY_SYSTEM.md** - 本文件

---

## 🎯 UI显示功能 ⭐最新

### SwayPhaseSlider - 阶段实时显示

通过UI Slider可视化当前倾斜阶段，提供清晰的视觉反馈：

**核心特性：**
- 🎚️ **实时显示** - 只读Slider显示当前阶段
- 🌈 **颜色反馈** - 根据危险程度自动变色（绿/黄/红）
- ⏱️ **平滑过渡** - 自动匹配SwayController的过渡时间
- 📋 **阶段标签** - 7个阶段清晰标注
- 📝 **文本显示** - 当前阶段中文名称
- 🚀 **一键创建** - 快速设置工具自动生成

**Slider值映射：**

```
左倾倒(0.00) ← 左倾2(0.15) ← 左倾1(0.35) ← 居中(0.50) → 右倾1(0.75) → 右倾2(0.85) → 右倾倒(1.00)

   🔴           🟡           🟡          🟢          🟡           🟡          🔴
  危险         警告         警告        安全        警告         警告        危险
```

**快速创建：**
```csharp
// 在SwaySystemQuickSetup中
createPhaseSlider = true;  // 勾选即可自动创建

// 或手动创建
SwaySliderUISetup setup = gameObject.AddComponent<SwaySliderUISetup>();
setup.swayController = swayController;
setup.position = SwaySliderUISetup.SliderPosition.BottomCenter;
setup.CreateSliderUI();
```

**效果展示：**
```
┌─────────────────────────────────────────────┐
│            当前阶段：右倾1 (黄色)            │
├─────────────────────────────────────────────┤
│ 左倾倒  左倾2  左倾1   居中   右倾1  右倾2  右倾倒│
│   |      |      |      |      ●     |      | │
│ ══════════════════════████═══════════════════│
│   0    0.15  0.35   0.5    0.75  0.85    1.0│
└─────────────────────────────────────────────┘
         ● = Slider当前位置 (0.75)
         █ = 填充区域 (黄色)
```

---

## 🎨 视觉增强功能

### StackVisualController - 真实堆叠滑落效果

新增的视觉控制系统采用**累积滑落算法**，让物品在倾斜时产生真实的物理表现：

**核心特性：**
- 🎯 **累积滑落计算** - 每个物品在下方物品基础上继续滑落
- 🎨 **多种偏移模式** - 线性、指数、正弦、自定义曲线
- 🎬 **平滑动画** - 自然的过渡效果
- 📊 **实时位置跟踪** - 获取所有物体的x轴位置
- ⚡ **阶段响应** - 倾斜角度越大，滑落幅度越大

**效果对比：**

```
无视觉控制：           累积滑落效果（右倾2）：
    ▢                         ▢     ← 顶部（累积偏移最大）
    ▢                       ▢       ← 中上（累积滑落）
    ▢          →          ▢         ← 中间（继续滑落）
    ▢                    ▢          ← 中下（轻微滑落）
    ▢                   ▢           ← 底部（无偏移）
```

**快速配置：**
```csharp
StackVisualController visual = gameObject.AddComponent<StackVisualController>();
visual.baseOffsetPerTilt = 0.15f;      // 每级倾斜偏移量
visual.heightMultiplier = 1.0f;        // 高度影响系数
visual.offsetMode = OffsetMode.Exponential;  // 指数偏移
visual.useSmoothMovement = true;       // 启用平滑移动
```

**获取位置信息：**
```csharp
// 获取最低端物体x位置
float baseX = visual.GetBaseObjectXPosition();

// 获取所有物体x位置
float[] allX = visual.GetAllXPositions();

// 获取指定物体x位置
float topX = visual.GetObjectXPosition(4);
```

---

## 🚀 快速开始（3步）

### 方法1：使用快速设置工具（推荐）

1. **创建GameObject**
   ```
   Hierarchy → 右键 → Create Empty
   命名为 "SwaySystem"
   ```

2. **添加快速设置组件**
   ```
   Add Component → SwaySystemQuickSetup
   ```

3. **执行自动设置**
   ```
   方式A: 进入Play模式 → 勾选 setupSystem
   方式B: 右键组件 → "执行快速设置"
   ```

### 方法2：手动设置

1. 创建GameObject "SwaySystem"
2. 添加 `SwayController` 组件
3. 添加 `StackController` 组件
4. 添加 `SwaySystemDemo` 组件（可选）
5. 按 Play 运行

---

## 🎮 控制方式 ⭐最新

### 新版输入系统 (v1.5.0) - 小键盘 + 长按

| 按键 | 功能 | 长按效果 | 说明 |
|------|------|----------|------|
| **小键盘1** | 立即左倾倒 | 1秒后再触发 | 立即进入左倾倒 / 从右倾倒跨越到左倾1 ⚡ |
| **小键盘2** | 向左倾斜 | 1秒后再倾斜 | 逐级向左倾斜一个阶段 |
| **小键盘3** | 向右倾斜 | 1秒后再倾斜 | 逐级向右倾斜一个阶段 |
| **小键盘4** | 立即右倾倒 | 1秒后再触发 | 立即进入右倾倒 / 从左倾倒跨越到右倾1 ⚡ |

**按键布局**（小键盘）：
```
┌────────┬────────┬────────┬────────┐
│   1    │   2    │   3    │   4    │
│ 左倾倒 │ 左倾斜 │ 右倾斜 │ 右倾倒 │
│        │        │        │        │
│⚡跨越  │  逐级  │  逐级  │⚡跨越  │
└────────┴────────┴────────┴────────┘
```

**核心机制**：

1. **长按重复触发** ⏱️
   - 按住任意按键1秒后，会自动再次触发
   - 示例：按住小键盘2 → 0.0秒触发，1.0秒再次触发
   - 可在Inspector中调整触发时间

2. **倾倒阶段跨越** ⚡
   - 在左倾倒时按小键盘4 → 跨越4个阶段到右倾1
   - 在右倾倒时按小键盘1 → 跨越4个阶段到左倾1
   - 这是一个快速救援机制！

3. **倾倒阶段回退** 🔄
   - 在左倾倒时按小键盘2/3 → 回到左倾2
   - 在右倾倒时按小键盘2/3 → 回到右倾2

**使用技巧**：
- 正常游戏：主要使用 **小键盘2** 和 **小键盘3** 键逐级调整
- 快速连续：按住按键可实现1秒间隔的连续操作
- 紧急救援：在倾倒时按相反方向倾倒键，快速跨越到相对安全的位置
- 战略丢弃：主动倾倒减少堆叠高度
- 回退恢复：从倾倒阶段按逐级键可以回退

**详细说明**：
- 输入系统：`玩家输入系统更新说明.md`
- 回退功能：`倾倒阶段回退功能说明.md`
- 长按功能：`长按输入系统说明.md` ⭐最新
- 快速参考：`小键盘输入快速参考.txt` ⭐最新

---

## 📊 7个倾斜阶段

```
左倾倒 ← 左倾2 ← 左倾1 ← 居中 → 右倾1 → 右倾2 → 右倾倒
 -35°     -25°     -15°     0°     15°      25°      35°
  -3       -2       -1       0       1        2        3
```

---

## ⚙️ 系统规则

### 1. 居中阶段 (Center)
- 每隔 **4-8秒** 随机转换
- 随机选择倾斜阶段（不包括倾倒）
- 玩家可手动控制倾斜

### 2. 倾斜阶段 (Tilt1/Tilt2)
- 开始 **5秒** 倒计时
- 倒计时结束 → 自动恶化到下一级
- 按↑键可重置倒计时（稳定）
- 路径：左倾1 → 左倾2 → 左倾倒

### 3. 倾倒阶段 (Topple)
- 立即掉落 **1个** 物体
- 开始 **3秒** 倒计时
- 倒计时结束 → 掉落 **1-2个** 物体
- 循环重复

---

## 🎯 代码示例

### 基础使用

```csharp
// 获取组件
SwayController sway = GetComponent<SwayController>();
StackController stack = GetComponent<StackController>();

// 查询状态
SwayPhase current = sway.CurrentPhase;
bool transitioning = sway.IsTransitioning;
float countdown = sway.CurrentCountdown;
int height = stack.GetStackHeight();
```

### 程序化控制

```csharp
// 改变阶段
sway.ChangePhase(SwayPhase.RightTilt2);

// 安全移动
sway.TryMovePhase(-1);  // 向左
sway.TryMovePhase(1);   // 向右
sway.TryMoveTowardCenter();  // 向中心
sway.StabilizeCurrentPhase(); // 稳定

// 堆叠管理
GameObject obj = stack.AddObjectToStack();
GameObject removed = stack.RemoveTopObject();
stack.RebuildStack(10);
```

### 事件订阅

```csharp
void Start() {
    // 阶段改变事件
    swayController.OnPhaseChanged += (oldPhase, newPhase) => {
        Debug.Log($"阶段变化: {oldPhase} → {newPhase}");
        
        // 播放音效
        if (newPhase.IsTopplePhase()) {
            PlayWarningSound();
        }
    };
    
    // 掉落事件
    swayController.OnToppleDrop += () => {
        Debug.Log("物体掉落！");
        CameraShake();
        PlayDropSound();
    };
}
```

---

## 🔧 参数配置

### SwayController 主要参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| transitionDuration | 1.0秒 | 阶段转换过渡时间 |
| centerPhaseMinInterval | 4.0秒 | 居中最小等待时间 |
| centerPhaseMaxInterval | 8.0秒 | 居中最大等待时间 |
| tiltPhaseCountdown | 5.0秒 | 倾斜阶段倒计时 |
| toppleFirstDropCountdown | 3.0秒 | 倾倒掉落间隔 |

### StackController 主要参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| initialStackCount | 5 | 初始堆叠数量 |
| objectSpacing | 1.0 | 物体垂直间距 |
| objectPrefab | null | 自定义物体预制体 |
| dropForce | 5.0 | 掉落力度 |
| droppedObjectLifetime | 3.0秒 | 掉落物体存活时间 |

### StackVisualController 主要参数 ⭐新增

| 参数 | 默认值 | 说明 |
|------|--------|------|
| baseOffsetPerTilt | 0.15 | 每级倾斜的基础x轴偏移 |
| heightMultiplier | 1.0 | 高度影响系数（0-2） |
| objectSpacing | 1.0 | 物体间距（与StackController一致） |
| baseXPosition | 0.0 | 最低端物体的基准x位置 |
| offsetMode | Exponential | 偏移模式（Linear/Exponential/Sine/Custom） |
| smoothTime | 0.2秒 | 位置调整的平滑时间 |
| useSmoothMovement | true | 是否启用平滑移动 |

#### 累积滑落算法原理 🔬

**传统方法（整体倾斜）：**
- 每个物品独立计算相对底部的偏移
- 结果：看起来像整个堆叠一起旋转

**新算法（累积滑落）：**
- 每个物品计算在前一物品基础上的滑落增量
- 这些增量逐层累积
- 结果：越高的物品累积偏移越大，产生真实的滑落效果

**计算公式：**
```
物品[i]的X位置 = 基准位置 + Σ(第1层到第i层的滑落增量)

其中每层滑落增量 = baseOffsetPerTilt × tiltLevel × HeightFactor(i) × heightMultiplier × direction
```

**示例（10个物品，右倾2，Linear模式）：**
```
层级  归一化高度  高度系数  本层滑落  累积偏移
  0     0.00      0.00     0.00     0.00  ← 底部不动
  1     0.11      0.11     0.02     0.02
  2     0.22      0.22     0.04     0.06
  3     0.33      0.33     0.07     0.13
  4     0.44      0.44     0.09     0.22
  9     1.00      1.00     0.20     1.00  ← 顶部偏移最大
```

---

## 🎚️ 难度预设

在 `SwaySystemQuickSetup` 中选择：

- **简单 (Easy)** - 长倒计时，慢转换，适合新手
- **正常 (Normal)** - 标准参数，平衡的游戏体验
- **困难 (Hard)** - 短倒计时，快转换，高挑战性
- **极难 (Extreme)** - 极限参数，顶级玩家
- **自定义 (Custom)** - 完全自定义所有参数

---

## 🎨 自定义物体预制体

### 使用2D Sprite

```csharp
GameObject obj = new GameObject("StackObject");
SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
sr.sprite = yourSprite;
sr.sortingOrder = 10;

// 保存为预制体
// 拖到 StackController.objectPrefab
```

### 使用3D模型（2D视角）

```csharp
GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
Destroy(obj.GetComponent<Collider>());
obj.transform.localScale = new Vector3(1f, 1f, 0.1f); // 扁平化

// 自定义材质
Renderer renderer = obj.GetComponent<Renderer>();
renderer.material.color = Color.blue;
```

---

## 🐛 调试功能

### 实时调试信息

在Game视图左上角显示：
- ✓ 当前阶段名称
- ✓ 旋转角度
- ✓ 转换状态
- ✓ 倒计时
- ✓ 堆叠高度

启用方式：
```csharp
swayController.showDebugInfo = true;
stackController.showDebugInfo = true;
swayController.logPhaseChanges = true;
stackController.logDropEvents = true;
```

### 右键测试菜单

在 `SwaySystemDemo` 组件上右键：
- 测试：强制左倾倒
- 测试：强制右倾倒
- 测试：回到居中
- 测试：添加10个物体
- 测试：清空堆叠
- 测试：重建堆叠
- 测试：模拟游戏流程

---

## 💡 游戏设计建议

### 计分系统示例

```csharp
public class ScoreManager : MonoBehaviour {
    private int score = 0;
    
    void Start() {
        swayController.OnPhaseChanged += OnPhaseChanged;
        swayController.OnToppleDrop += OnDrop;
        
        InvokeRepeating("AddSurvivalScore", 1f, 1f);
    }
    
    void OnPhaseChanged(SwayPhase old, SwayPhase newPhase) {
        if (newPhase == SwayPhase.Center) {
            score += 30; // 回到居中奖励
        }
    }
    
    void OnDrop() {
        score -= 100; // 掉落惩罚
    }
    
    void AddSurvivalScore() {
        score += 10; // 每秒存活
    }
}
```

### 难度递增

```csharp
void IncreaseDifficulty() {
    swayController.tiltPhaseCountdown -= 0.5f;
    swayController.transitionDuration -= 0.05f;
    swayController.centerPhaseMaxInterval -= 1f;
}
```

### 道具系统

```csharp
// 时间减速道具
void SlowTimeItem() {
    swayController.tiltPhaseCountdown += 3f;
}

// 自动居中道具
void AutoCenterItem() {
    swayController.ChangePhase(SwayPhase.Center);
}

// 稳定护盾
void StabilizeItem() {
    swayController.StabilizeCurrentPhase();
}
```

---

## 🎚️ UI Slider API

### 基础使用

```csharp
// 获取Slider组件
SwayPhaseSlider slider = FindObjectOfType<SwayPhaseSlider>();

// 获取当前显示值
float currentValue = slider.GetCurrentValue();
Debug.Log($"Slider当前值: {currentValue}");  // 0.0 - 1.0

// 获取目标值
float targetValue = slider.GetTargetValue();
Debug.Log($"Slider目标值: {targetValue}");

// 手动设置值（测试用）
slider.SetSliderValue(0.75f);  // 设置为右倾1位置
```

### 自定义颜色

```csharp
// 运行时修改颜色
SwayPhaseSlider slider = FindObjectOfType<SwayPhaseSlider>();

slider.safeColor = Color.cyan;                      // 安全区域（居中）
slider.warningColor = new Color(1f, 0.5f, 0f);      // 警告区域（倾1/倾2）
slider.dangerColor = Color.magenta;                 // 危险区域（倾倒）

// 如需立即刷新颜色
SwayController sway = FindObjectOfType<SwayController>();
slider.UpdateSliderColor(sway.CurrentPhase);
```

### 创建自定义UI

```csharp
// 手动创建Slider UI
SwaySliderUISetup setup = gameObject.AddComponent<SwaySliderUISetup>();

setup.swayController = swayController;
setup.position = SwaySliderUISetup.SliderPosition.BottomCenter;
setup.sliderWidth = 400f;
setup.sliderHeight = 100f;
setup.showPhaseLabels = true;
setup.showCurrentPhaseText = true;

setup.CreateSliderUI();
```

### 监听Slider更新

```csharp
public class SliderMonitor : MonoBehaviour {
    private SwayPhaseSlider slider;
    private SwayController swayController;
    
    void Start() {
        slider = FindObjectOfType<SwayPhaseSlider>();
        swayController = FindObjectOfType<SwayController>();
        
        // 监听阶段变化
        swayController.OnPhaseChanged += OnPhaseChanged;
    }
    
    void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase) {
        float sliderValue = slider.GetTargetValue();
        Debug.Log($"阶段改变: {newPhase.GetChineseName()}, Slider目标值: {sliderValue}");
        
        // 根据Slider值执行操作
        if (sliderValue <= 0.15f || sliderValue >= 0.85f) {
            Debug.LogWarning("接近危险区域！");
        }
    }
}
```

---

## 🎭 视觉和音效增强

### UI倒计时

```csharp
public class CountdownUI : MonoBehaviour {
    public Image progressBar;
    public Text countdownText;
    
    void Update() {
        float countdown = swayController.CurrentCountdown;
        float max = swayController.tiltPhaseCountdown;
        
        progressBar.fillAmount = countdown / max;
        countdownText.text = countdown.ToString("F1") + "s";
        
        // 危险警告
        if (countdown < 1f) {
            progressBar.color = Color.red;
        }
    }
}
```

### 相机震动

```csharp
IEnumerator CameraShake(float duration = 0.3f, float magnitude = 0.2f) {
    Vector3 originalPos = Camera.main.transform.position;
    
    while (duration > 0) {
        Camera.main.transform.position = originalPos + 
            Random.insideUnitSphere * magnitude;
        duration -= Time.deltaTime;
        yield return null;
    }
    
    Camera.main.transform.position = originalPos;
}
```

### 音效管理

```csharp
public class SoundManager : MonoBehaviour {
    public AudioClip tiltSound;
    public AudioClip warningSound;
    public AudioClip dropSound;
    public AudioClip successSound;
    
    void OnPhaseChanged(SwayPhase old, SwayPhase newPhase) {
        if (newPhase.IsTiltPhase()) {
            PlaySound(tiltSound);
        } else if (newPhase.IsTopplePhase()) {
            PlaySound(warningSound);
        } else if (newPhase.IsCenterPhase()) {
            PlaySound(successSound);
        }
    }
    
    void OnDrop() {
        PlaySound(dropSound);
    }
    
    void PlaySound(AudioClip clip) {
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }
}
```

---

## 📈 性能优化

### 对象池实现

```csharp
public class ObjectPool : MonoBehaviour {
    public GameObject prefab;
    private Queue<GameObject> pool = new Queue<GameObject>();
    
    public GameObject Get() {
        if (pool.Count > 0) {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        return Instantiate(prefab);
    }
    
    public void Return(GameObject obj) {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

---

## ❓ 常见问题

**Q: 如何禁用自动倾斜？**
```csharp
swayController.centerPhaseMaxInterval = Mathf.Infinity;
```

**Q: 如何改变旋转角度？**  
修改 `SwayPhase.cs` 中的 `GetRotationAngle()` 方法

**Q: 如何自定义按键？**  
在 `SwayController` Inspector 中修改 Key 参数

**Q: 物体看不见怎么办？**  
检查相机位置和 SwaySystem GameObject 的位置

**Q: 如何添加网络多人？**  
同步 `currentPhase` 变量，使用 RPC 调用 `ChangePhase()`

---

## 📝 系统架构

```
SwaySystem (GameObject)
├── SwayController (控制器)
│   ├── 状态管理
│   ├── 输入处理
│   ├── 倒计时逻辑
│   └── 事件分发
├── StackController (堆叠管理)
│   ├── 物体创建
│   ├── 掉落处理
│   └── StackRoot (Transform)
│       ├── StackObject_0 (x偏移: 0)
│       ├── StackObject_1 (x偏移: 小)
│       ├── StackObject_2 (x偏移: 中)
│       └── StackObject_3 (x偏移: 大) ← 越高偏移越大
├── StackVisualController (视觉控制) ⭐新增
│   ├── 偏移计算
│   ├── 位置跟踪
│   └── 平滑动画
└── SwaySystemDemo (演示，可选)
```

---

## 🔗 依赖关系

- **无外部依赖** - 纯C#和Unity基础API
- **Unity版本** - 2019.4 或更高
- **兼容平台** - 所有平台（PC, Mobile, WebGL等）

---

## 📄 许可证

此系统为教育和开发用途创建。  
您可以自由使用、修改和分发。

---

## 🤝 贡献

如发现问题或有改进建议，欢迎反馈！

---

## 📚 相关资源

- Unity官方文档: https://docs.unity3d.com/
- C#协程指南: https://docs.unity3d.com/Manual/Coroutines.html
- 状态机模式: https://gameprogrammingpatterns.com/state.html

---

**版本**: 1.0  
**创建日期**: 2025-10-13  
**最后更新**: 2025-10-13

---

祝您使用愉快！🎉

