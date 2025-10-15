using UnityEngine;

/// <summary>
/// 平衡系统设置向导和使用说明
/// 将此组件添加到空GameObject上以快速设置平衡系统
/// </summary>
[AddComponentMenu("Balance System/Setup Wizard")]
public class BalanceSystemSetup : MonoBehaviour
{
    [Header("快速设置")]
    [Tooltip("点击此按钮自动设置场景")]
    public bool autoSetup = false;

    [Header("设置选项")]
    [Tooltip("地面高度")]
    public float groundLevel = -4f;
    
    [Tooltip("创建示例物体")]
    public bool createExampleObjects = true;
    
    [Tooltip("示例物体数量")]
    [Range(1, 5)]
    public int exampleObjectCount = 3;

    [Header("系统说明")]
    [TextArea(20, 30)]
    public string instructions = @"
=== Unity 自定义平衡系统使用说明 ===

【系统组成】

1. BalanceableObject（可平衡物体）
   - 附加到需要平衡模拟的每个物体上
   - 跟踪物体的质心、质量和速度
   - 管理与其他物体的支撑关系

2. BalanceManager（平衡管理器）
   - 场景中只需一个
   - 管理所有物体的平衡计算
   - 自动检测支撑关系并应用物理模拟

3. BalanceCalculator（平衡计算器）
   - 静态工具类
   - 提供质心计算、稳定性检测等功能
   - 可在自定义脚本中直接调用

4. ObjectPlacer（物体放置器）
   - 可选组件
   - 提供运行时放置物体的交互功能

5. BalanceSystemDemo（演示脚本）
   - 可选组件
   - 展示系统的各种使用方法

---

【快速开始】

步骤1：设置场景
1. 创建空GameObject，命名为 ""BalanceSystem""
2. 添加 BalanceManager 组件
3. 设置 groundLevel（地面高度）

步骤2：创建可平衡物体
1. 创建一个Sprite GameObject（例如正方形）
2. 添加 BalanceableObject 组件
3. 配置参数：
   - mass: 物体质量（影响惯性）
   - centerOfMassOffset: 质心偏移（影响倾倒倾向）
   - supportBaseWidth: 支撑基底宽度（影响稳定性）
   - balanceForceMultiplier: 平衡力强度
   - torqueMultiplier: 扭矩强度

步骤3：添加交互（可选）
1. 在 BalanceSystem 上添加 ObjectPlacer 组件
2. 将可平衡物体制作成预制体
3. 将预制体添加到 ObjectPlacer 的 placeablePrefabs 数组
4. 运行游戏，按空格键放置物体

---

【参数调整指南】

物体质量 (mass):
- 较大: 更重，移动慢，难以倾倒
- 较小: 更轻，移动快，容易倾倒
- 推荐: 1.0 - 5.0

质心偏移 (centerOfMassOffset):
- Y为负: 质心偏下，更稳定
- Y为正: 质心偏上，容易倾倒
- X偏移: 会导致物体向一侧倾倒
- 推荐: (0, -0.2) 到 (0, 0.2)

支撑基底宽度 (supportBaseWidth):
- 较大: 稳定性更高，不易倾倒
- 较小: 容易失去平衡
- 应该匹配物体的实际宽度
- 推荐: 与Sprite宽度一致

平衡力系数 (balanceForceMultiplier):
- 较大: 恢复平衡更快，可能震荡
- 较小: 恢复平衡较慢，更平滑
- 推荐: 3.0 - 10.0

扭矩系数 (torqueMultiplier):
- 较大: 旋转效果明显
- 较小: 旋转效果微弱
- 推荐: 1.0 - 3.0

阻尼系数 (dampingFactor):
- 接近1.0: 几乎无阻尼，容易震荡
- 接近0.0: 阻尼很大，快速停止
- 推荐: 0.90 - 0.98

---

【核心功能说明】

1. 自动质心计算
   - 每个物体跟踪自己的质心
   - 堆叠时自动计算组合质心
   - 支持多层嵌套堆叠

2. 支撑检测
   - 自动检测物体之间的支撑关系
   - 维护 objectsBelow 和 objectsAbove 列表
   - 支持地面检测 (isGrounded)

3. 稳定性判断
   - 检查质心是否在支撑基底内
   - 计算稳定性分数（0-1）
   - 质心超出基底时触发倾倒

4. 平衡模拟
   稳定状态：
   - 应用恢复力使质心回到支撑点上方
   - 应用小幅稳定扭矩
   
   不稳定状态：
   - 应用倾倒扭矩
   - 物体将开始旋转和下落

5. 自由落体
   - 无支撑物体自动下落
   - 着地检测和速度归零

---

【代码示例】

// 创建可平衡物体
GameObject obj = new GameObject(""Box"");
SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
sr.sprite = mySprite;

BalanceableObject balObj = obj.AddComponent<BalanceableObject>();
balObj.mass = 2.0f;
balObj.centerOfMassOffset = new Vector2(0, -0.3f);
balObj.supportBaseWidth = 1.0f;

// 注册到管理器
balanceManager.RegisterObject(balObj);

// 手动计算质心
Vector2 combinedCOM = balObj.CalculateCombinedCenterOfMass();
Debug.Log($""组合质心: {combinedCOM}"");

// 检查稳定性
if (balObj.objectsBelow.Count > 0)
{
    BalanceableObject support = balObj.objectsBelow[0];
    bool isStable = support.IsPointWithinSupportBase(combinedCOM);
    Debug.Log($""是否稳定: {isStable}"");
}

// 获取稳定性报告
string report = balanceManager.GetStabilityReport(balObj);
Debug.Log(report);

---

【调试技巧】

1. 启用调试可视化
   - BalanceableObject.showDebugInfo = true
   - BalanceManager.showDebugInfo = true
   - 在Scene视图中查看：
     * 红色球体: 质心位置
     * 绿色线段: 支撑基底
     * 黄色球体: 支撑点
     * 蓝色连线: 支撑关系

2. 启用日志
   - BalanceManager.logBalanceEvents = true
   - ObjectPlacer.logPlacementEvents = true
   - 查看Console了解系统运行状态

3. 常见问题
   问题: 物体震荡不停
   解决: 增加 dampingFactor（接近1.0）
         减小 balanceForceMultiplier

   问题: 物体反应太慢
   解决: 增加 balanceForceMultiplier
         减小 dampingFactor

   问题: 物体太容易倾倒
   解决: 增加 supportBaseWidth
         降低 centerOfMassOffset.y
         增加 mass

   问题: 支撑检测不准确
   解决: 调整 supportDetectionRange
         检查 supportCheckOffset
         确保 groundLevel 设置正确

---

【高级功能】

1. 自定义平衡逻辑
   继承 BalanceableObject 并重写方法：
   - ApplyBalanceForce()
   - CalculateCombinedCenterOfMass()

2. 使用 BalanceCalculator 工具
   - CalculateStabilityScore(): 获取稳定性数值
   - FindSupportPivot(): 找到支撑点
   - CalculateFallImpact(): 计算掉落冲击

3. 动态调整参数
   在运行时修改参数可实时看到效果：
   balObj.mass = 3.0f;
   balObj.centerOfMassOffset = new Vector2(0.5f, 0);

---

【性能优化】

1. 使用固定时间步长
   - BalanceManager.useFixedTimeStep = true
   - 提供更稳定的物理模拟

2. 限制物体数量
   - 大量物体会增加计算负担
   - 考虑使用对象池

3. 调整检测频率
   - 不需要每帧都检测支撑关系
   - 可以在物体静止时暂停更新

---

【扩展建议】

1. 添加碰撞检测
   - 使用 Collider2D 检测物体接触
   - 更精确的支撑关系判断

2. 添加材质属性
   - 不同材质的摩擦系数
   - 弹性碰撞

3. 添加破坏效果
   - 承载力限制
   - 过载时物体破碎

4. 添加音效
   - 物体碰撞声
   - 倾倒声效

5. 网络同步
   - 同步物体位置和旋转
   - 同步质心和速度

===========================
";

    void OnValidate()
    {
        if (autoSetup && Application.isPlaying)
        {
            PerformAutoSetup();
            autoSetup = false;
        }
    }

    [ContextMenu("执行自动设置")]
    void PerformAutoSetup()
    {
        Debug.Log("=== 开始自动设置平衡系统 ===");

        // 1. 查找或创建 BalanceManager
        BalanceManager manager = FindObjectOfType<BalanceManager>();
        
        if (manager == null)
        {
            GameObject managerObj = new GameObject("BalanceManager");
            manager = managerObj.AddComponent<BalanceManager>();
            Debug.Log("[Setup] 创建了 BalanceManager");
        }
        
        manager.groundLevel = groundLevel;
        manager.showDebugInfo = true;
        manager.logBalanceEvents = true;

        // 2. 创建示例物体（如果需要）
        if (createExampleObjects)
        {
            CreateExampleObjects();
        }

        // 3. 查找或创建 ObjectPlacer
        ObjectPlacer placer = FindObjectOfType<ObjectPlacer>();
        
        if (placer == null)
        {
            GameObject placerObj = new GameObject("ObjectPlacer");
            placer = placerObj.AddComponent<ObjectPlacer>();
            Debug.Log("[Setup] 创建了 ObjectPlacer");
        }

        Debug.Log("=== 自动设置完成 ===");
        Debug.Log("提示: 请在 ObjectPlacer 中添加预制体以启用物体放置功能");
    }

    void CreateExampleObjects()
    {
        for (int i = 0; i < exampleObjectCount; i++)
        {
            // 创建GameObject
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = $"ExampleBox_{i + 1}";
            
            // 移除3D碰撞器（因为我们是2D系统）
            Destroy(obj.GetComponent<Collider>());
            
            // 设置位置
            float xPos = (i - exampleObjectCount / 2f) * 2f;
            obj.transform.position = new Vector3(xPos, groundLevel + 3f, 0);
            
            // 设置缩放（扁平化用于2D）
            obj.transform.localScale = new Vector3(1f, 1f, 0.1f);
            
            // 添加 BalanceableObject
            BalanceableObject balObj = obj.AddComponent<BalanceableObject>();
            balObj.mass = 1.0f + i * 0.5f;
            balObj.supportBaseWidth = 1.0f;
            balObj.showDebugInfo = true;
            
            // 随机颜色
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f)
                );
            }
            
            Debug.Log($"[Setup] 创建示例物体: {obj.name}");
        }
    }

    [ContextMenu("打印系统状态")]
    void PrintSystemStatus()
    {
        Debug.Log("=== 平衡系统状态 ===");
        
        BalanceManager manager = FindObjectOfType<BalanceManager>();
        if (manager != null)
        {
            Debug.Log($"✓ BalanceManager 存在");
            Debug.Log($"  - 地面高度: {manager.groundLevel}");
            Debug.Log($"  - 自动检测支撑: {manager.autoDetectSupport}");
        }
        else
        {
            Debug.LogWarning("✗ 未找到 BalanceManager");
        }

        BalanceableObject[] objects = FindObjectsOfType<BalanceableObject>();
        Debug.Log($"✓ 找到 {objects.Length} 个可平衡物体");
        
        foreach (var obj in objects)
        {
            Debug.Log($"  - {obj.name}: 质量={obj.mass}, 质心偏移={obj.centerOfMassOffset}");
        }

        ObjectPlacer placer = FindObjectOfType<ObjectPlacer>();
        if (placer != null)
        {
            Debug.Log($"✓ ObjectPlacer 存在");
            Debug.Log($"  - 预制体数量: {placer.placeablePrefabs?.Length ?? 0}");
        }
        else
        {
            Debug.Log("- ObjectPlacer 未找到（可选组件）");
        }

        Debug.Log("===================");
    }
}


