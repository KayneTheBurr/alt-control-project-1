# UI Slider 阶段显示系统说明

## 📋 概述

**版本**: v1.3.0  
**日期**: 2024-10-13  
**功能**: 通过UI Slider实时显示摇摆倾斜阶段

---

## 🎯 核心功能

### Slider值与阶段映射

| 阶段 | Slider值 | 颜色 | 状态 |
|------|---------|------|------|
| **左倾倒** | 0.00 | 🔴 红色 | 危险 |
| **左倾2** | 0.15 | 🟡 黄色 | 警告 |
| **左倾1** | 0.35 | 🟡 黄色 | 警告 |
| **居中** | 0.50 | 🟢 绿色 | 安全 |
| **右倾1** | 0.75 | 🟡 黄色 | 警告 |
| **右倾2** | 0.85 | 🟡 黄色 | 警告 |
| **右倾倒** | 1.00 | 🔴 红色 | 危险 |

### 特性

- ✅ **只读显示**：Slider不可交互，仅用于显示当前阶段
- ✅ **平滑过渡**：自动匹配 `SwayController` 的过渡时间
- ✅ **颜色反馈**：根据阶段自动改变颜色
- ✅ **实时更新**：监听阶段变化事件，立即响应
- ✅ **自动创建**：使用快速设置工具一键生成完整UI

---

## 🚀 快速开始

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

3. **配置UI选项**
   ```
   ✓ Create Phase Slider = true （默认已勾选）
   ✓ Slider Position = BottomCenter
   ✓ Show Phase Labels = true
   ✓ Show Current Phase Text = true
   ```

4. **执行设置**
   ```
   进入Play模式 → 勾选 setupSystem
   或 右键组件 → "执行快速设置"
   ```

5. **完成！**
   - UI Slider会自动创建在Game视图
   - 随倾斜阶段实时更新

### 方法2：手动创建UI

1. **添加UI设置脚本**
   ```csharp
   GameObject uiSetup = new GameObject("UISetup");
   SwaySliderUISetup setup = uiSetup.AddComponent<SwaySliderUISetup>();
   ```

2. **配置参数**
   ```csharp
   setup.swayController = swayController; // 你的SwayController引用
   setup.position = SwaySliderUISetup.SliderPosition.BottomCenter;
   setup.showPhaseLabels = true;
   setup.showCurrentPhaseText = true;
   ```

3. **创建UI**
   ```csharp
   setup.CreateSliderUI();
   ```

### 方法3：手动设置（完全自定义）

1. **创建Canvas**（如果场景中没有）
   ```
   Hierarchy → UI → Canvas
   ```

2. **创建Slider**
   ```
   Canvas → 右键 → UI → Slider
   ```

3. **添加SwayPhaseSlider脚本**
   ```
   选中Slider → Add Component → SwayPhaseSlider
   ```

4. **配置引用**
   ```
   Sway Controller = 拖入你的SwayController
   ```

5. **调整参数**
   ```
   Use Smooth Transition = true
   Change Color By Phase = true
   ```

---

## 📐 UI布局选项

### 预设位置

通过 `SwaySliderUISetup.SliderPosition` 选择：

| 位置 | 说明 | 适用场景 |
|------|------|----------|
| **TopCenter** | 顶部居中 | 主要UI在底部时 |
| **BottomCenter** ⭐ | 底部居中 | 大多数情况（推荐） |
| **TopLeft** | 左上角 | 紧凑布局 |
| **TopRight** | 右上角 | 紧凑布局 |
| **BottomLeft** | 左下角 | 避开右下角UI |
| **BottomRight** | 右下角 | 避开左下角UI |

### 自定义样式

在 `SwaySliderUISetup.cs` 中修改：

```csharp
// 容器尺寸
float sliderWidth = 400f;   // Slider容器宽度
float sliderHeight = 100f;  // Slider容器高度（包含标签）
float margin = 50f;         // 距离屏幕边缘的间距

// Slider本体尺寸
sliderRect.sizeDelta = new Vector2(360, 20);  // 宽度x高度

// 标签字体
text.fontSize = 10;         // 标签字号
text.color = Color.white;   // 标签颜色

// 当前阶段文本
text.fontSize = 14;         // 文本字号
text.fontStyle = FontStyle.Bold;  // 粗体
```

---

## 🎨 视觉定制

### 颜色主题

在 `SwayPhaseSlider` 组件中设置：

```csharp
// 安全区域颜色（居中）
safeColor = Color.green;

// 警告区域颜色（倾1、倾2）
warningColor = Color.yellow;

// 危险区域颜色（倾倒）
dangerColor = Color.red;
```

### 自定义颜色方案

**清新蓝绿主题**：
```csharp
safeColor = new Color(0.0f, 0.8f, 0.8f);     // 青色
warningColor = new Color(1.0f, 0.7f, 0.0f);  // 橙色
dangerColor = new Color(0.9f, 0.2f, 0.2f);   // 深红
```

**暗黑主题**：
```csharp
safeColor = new Color(0.3f, 0.9f, 0.3f);     // 亮绿
warningColor = new Color(1.0f, 0.8f, 0.0f);  // 金色
dangerColor = new Color(1.0f, 0.0f, 0.3f);   // 品红
```

### 添加特效

在 Slider 的 Fill Image 上添加组件：

```csharp
// 添加阴影
Shadow shadow = fillImage.gameObject.AddComponent<Shadow>();
shadow.effectDistance = new Vector2(2, -2);
shadow.effectColor = new Color(0, 0, 0, 0.5f);

// 添加外发光
Outline outline = fillImage.gameObject.AddComponent<Outline>();
outline.effectDistance = new Vector2(1, 1);
outline.effectColor = Color.white;
```

---

## ⚙️ 核心脚本说明

### SwayPhaseSlider.cs

**作用**：连接 `SwayController` 和 UI Slider，实时更新显示

**核心功能**：
- 监听 `OnPhaseChanged` 事件
- 计算对应的Slider值
- 平滑过渡到目标值
- 根据阶段改变颜色

**重要方法**：

```csharp
// 获取阶段对应的Slider值
float GetSliderValueForPhase(SwayPhase phase)

// 更新Slider颜色
void UpdateSliderColor(SwayPhase phase)

// 更新过渡速度（匹配SwayController）
void UpdateTransitionSpeed()
```

### SwaySliderUISetup.cs

**作用**：快速创建完整的UI层次结构

**核心功能**：
- 自动创建Canvas（如果不存在）
- 创建Slider及其子元素
- 创建阶段标签
- 创建当前阶段文本显示
- 配置SwayPhaseSlider组件

**使用方法**：

```csharp
// 在Inspector中
[ContextMenu("创建Slider UI")]
public void CreateSliderUI()

// 在代码中
SwaySliderUISetup setup = gameObject.AddComponent<SwaySliderUISetup>();
setup.swayController = mySwayController;
setup.CreateSliderUI();
```

### SwayPhaseTextUpdater.cs

**作用**：实时更新"当前阶段"文本显示

**功能**：
- 监听阶段变化
- 更新文本内容为中文阶段名
- 根据阶段改变文本颜色

---

## 🔧 参数配置

### SwayPhaseSlider参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `swayController` | null | SwayController引用（必须） |
| `useSmoothTransition` | true | 是否启用平滑过渡 |
| `transitionSpeed` | 自动 | 过渡速度（自动匹配SwayController） |
| `changeColorByPhase` | true | 是否根据阶段改变颜色 |
| `safeColor` | Green | 安全区域颜色 |
| `warningColor` | Yellow | 警告区域颜色 |
| `dangerColor` | Red | 危险区域颜色 |
| `showDebugInfo` | false | 显示调试信息 |

### SwaySliderUISetup参数

| 参数 | 默认值 | 说明 |
|------|--------|------|
| `position` | BottomCenter | Slider位置 |
| `sliderWidth` | 400 | Slider宽度 |
| `sliderHeight` | 30 | Slider高度 |
| `margin` | 50 | 边缘间距 |
| `showPhaseLabels` | true | 显示阶段标签 |
| `showCurrentPhaseText` | true | 显示当前阶段文本 |

---

## 📊 工作原理

### 事件流程

```
1. SwayController 阶段改变
   ↓
2. 触发 OnPhaseChanged 事件
   ↓
3. SwayPhaseSlider 接收事件
   ↓
4. 计算目标Slider值
   ↓
5. 更新Slider颜色
   ↓
6. 平滑过渡到目标值（Update循环）
   ↓
7. SwayPhaseTextUpdater 更新文本
```

### 平滑过渡算法

```csharp
// 在Update()中每帧执行
currentValue = Mathf.SmoothDamp(
    currentValue,           // 当前值
    targetValue,            // 目标值
    ref transitionVelocity, // 速度引用
    1f / transitionSpeed    // 平滑时间
);

slider.value = currentValue;
```

**速度自动匹配**：
```csharp
transitionSpeed = 1f / swayController.transitionDuration;
```
这确保Slider的过渡时间与阶段转换时间一致！

---

## 🎮 使用示例

### 示例1：基础使用

```csharp
// 在Play模式下运行
SwayController sway = GetComponent<SwayController>();

// 改变阶段
sway.TryMovePhase(1);  // 向右倾斜

// Slider会自动更新显示！
```

### 示例2：获取Slider状态

```csharp
SwayPhaseSlider slider = FindObjectOfType<SwayPhaseSlider>();

// 获取当前显示值
float current = slider.GetCurrentValue();
Debug.Log($"Slider当前值: {current}");

// 获取目标值
float target = slider.GetTargetValue();
Debug.Log($"Slider目标值: {target}");
```

### 示例3：动态修改颜色

```csharp
SwayPhaseSlider slider = FindObjectOfType<SwayPhaseSlider>();

// 运行时修改颜色
slider.safeColor = Color.cyan;
slider.warningColor = new Color(1f, 0.5f, 0f);
slider.dangerColor = Color.magenta;

// 立即刷新
slider.OnPhaseChanged(SwayPhase.Center, SwayPhase.Center);
```

---

## 🐛 故障排除

### 问题1：Slider不显示

**原因**：
- Canvas可能不在场景中
- Slider可能在屏幕外

**解决方案**：
```csharp
// 检查Canvas
Canvas canvas = FindObjectOfType<Canvas>();
if (canvas == null)
{
    Debug.LogError("场景中没有Canvas！");
}

// 检查Slider位置
SwayPhaseSlider slider = FindObjectOfType<SwayPhaseSlider>();
RectTransform rect = slider.GetComponent<RectTransform>();
Debug.Log($"Slider位置: {rect.anchoredPosition}");
```

### 问题2：Slider不更新

**原因**：
- `swayController` 引用未设置
- 事件未订阅

**解决方案**：
```csharp
SwayPhaseSlider slider = FindObjectOfType<SwayPhaseSlider>();

if (slider.swayController == null)
{
    slider.swayController = FindObjectOfType<SwayController>();
    Debug.Log("已自动设置SwayController引用");
}
```

### 问题3：过渡不平滑

**原因**：
- `useSmoothTransition` 被禁用
- `transitionSpeed` 过大

**解决方案**：
```csharp
slider.useSmoothTransition = true;
slider.UpdateTransitionSpeed();  // 重新同步过渡速度
```

### 问题4：颜色不变

**原因**：
- `changeColorByPhase` 被禁用
- Fill Image引用丢失

**解决方案**：
```csharp
slider.changeColorByPhase = true;

// 检查Fill Image
if (slider.GetComponent<Slider>().fillRect == null)
{
    Debug.LogError("Slider的fillRect未设置！");
}
```

---

## 💡 高级技巧

### 技巧1：添加音效反馈

```csharp
public class SliderAudioFeedback : MonoBehaviour
{
    public AudioClip phaseChangeSound;
    private AudioSource audioSource;
    private SwayController swayController;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        swayController = FindObjectOfType<SwayController>();
        swayController.OnPhaseChanged += OnPhaseChanged;
    }

    void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase)
    {
        audioSource.PlayOneShot(phaseChangeSound);
    }
}
```

### 技巧2：添加震动反馈

```csharp
void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase)
{
    // 倾倒阶段强震动
    if (newPhase == SwayPhase.LeftTopple || newPhase == SwayPhase.RightTopple)
    {
        Handheld.Vibrate();  // 移动设备震动
    }
}
```

### 技巧3：添加粒子特效

```csharp
// 在Slider上添加粒子系统
ParticleSystem particles = sliderObject.AddComponent<ParticleSystem>();

// 阶段改变时播放
void OnPhaseChanged(SwayPhase oldPhase, SwayPhase newPhase)
{
    if (newPhase == SwayPhase.LeftTopple || newPhase == SwayPhase.RightTopple)
    {
        particles.Play();
    }
}
```

### 技巧4：数据持久化

```csharp
// 保存Slider配置
[System.Serializable]
public class SliderConfig
{
    public bool useSmoothTransition = true;
    public bool changeColorByPhase = true;
    public Color safeColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
}

// 保存
string json = JsonUtility.ToJson(config);
PlayerPrefs.SetString("SliderConfig", json);

// 加载
string json = PlayerPrefs.GetString("SliderConfig");
SliderConfig config = JsonUtility.FromJson<SliderConfig>(json);
```

---

## 📚 相关文件

### 核心脚本
- `SwayPhaseSlider.cs` - Slider显示逻辑
- `SwaySliderUISetup.cs` - UI创建工具
- `SwayPhase.cs` - 阶段定义
- `SwayController.cs` - 摇摆控制

### 文档
- `UI Slider阶段显示说明.md` - 本文件
- `README_SWAY_SYSTEM.md` - 完整系统文档
- `摇摆系统使用指南.txt` - 基础使用指南

---

## 🎯 最佳实践

1. **总是使用快速设置工具**：避免手动创建UI的复杂性
2. **保持Slider不可交互**：Slider仅用于显示，不应接受用户输入
3. **启用颜色反馈**：帮助玩家快速识别危险状态
4. **匹配过渡时间**：确保UI响应与游戏逻辑同步
5. **适当的位置选择**：避免遮挡重要游戏元素

---

## 📝 版本历史

**v1.3.0** (2024-10-13)
- ✅ 初始发布
- ✅ 基础Slider显示功能
- ✅ 自动过渡同步
- ✅ 颜色反馈系统
- ✅ 快速设置工具

---

## 🔮 未来计划

- 🔲 自定义UI皮肤系统
- 🔲 动画效果增强
- 🔲 多语言支持
- 🔲 触觉反馈集成
- 🔲 VR/AR支持

---

**更新日期**: 2024-10-13  
**版本**: v1.3.0  
**维护**: Sway System Team


