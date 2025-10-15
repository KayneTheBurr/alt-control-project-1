using UnityEngine;

/// <summary>
/// 演示脚本，展示如何使用平衡系统的各种功能
/// 包含示例代码和使用说明
/// </summary>
public class BalanceSystemDemo : MonoBehaviour
{
    [Header("演示模式")]
    [Tooltip("自动运行演示")]
    public bool autoDemo = false;
    
    [Tooltip("演示间隔时间")]
    public float demoInterval = 2.0f;

    [Header("引用")]
    public BalanceManager balanceManager;
    public ObjectPlacer objectPlacer;

    [Header("演示对象")]
    public GameObject box1;
    public GameObject box2;
    public GameObject box3;

    private float demoTimer = 0f;
    private int demoStep = 0;

    void Start()
    {
        PrintUsageInstructions();
        
        if (balanceManager == null)
        {
            balanceManager = FindObjectOfType<BalanceManager>();
        }
        
        if (objectPlacer == null)
        {
            objectPlacer = FindObjectOfType<ObjectPlacer>();
        }
    }

    void Update()
    {
        if (autoDemo)
        {
            RunAutoDemo();
        }

        // 手动演示快捷键
        if (Input.GetKeyDown(KeyCode.F1))
        {
            DemoStep1_SingleObject();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            DemoStep2_StackedObjects();
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            DemoStep3_UnbalancedStack();
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            DemoStep4_ComplexBalance();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            ResetDemo();
        }
    }

    /// <summary>
    /// 打印使用说明
    /// </summary>
    void PrintUsageInstructions()
    {
        Debug.Log("=== 平衡系统使用说明 ===");
        Debug.Log("基本控制:");
        Debug.Log("  空格键 (Space) - 放置物体");
        Debug.Log("  Tab键 - 切换预制体");
        Debug.Log("  1-9数字键 - 快速选择预制体");
        Debug.Log("");
        Debug.Log("演示功能:");
        Debug.Log("  F1 - 演示：单个物体");
        Debug.Log("  F2 - 演示：堆叠物体");
        Debug.Log("  F3 - 演示：不平衡堆叠");
        Debug.Log("  F4 - 演示：复杂平衡");
        Debug.Log("  R  - 重置演示");
        Debug.Log("");
        Debug.Log("系统特性:");
        Debug.Log("  ✓ 自定义质心跟踪");
        Debug.Log("  ✓ 动态支撑检测");
        Debug.Log("  ✓ 组合质心计算");
        Debug.Log("  ✓ 稳定性判断");
        Debug.Log("  ✓ 自动平衡力/扭矩");
        Debug.Log("========================");
    }

    /// <summary>
    /// 运行自动演示
    /// </summary>
    void RunAutoDemo()
    {
        demoTimer += Time.deltaTime;
        
        if (demoTimer >= demoInterval)
        {
            demoTimer = 0f;
            
            switch (demoStep)
            {
                case 0:
                    DemoStep1_SingleObject();
                    break;
                case 1:
                    DemoStep2_StackedObjects();
                    break;
                case 2:
                    DemoStep3_UnbalancedStack();
                    break;
                case 3:
                    DemoStep4_ComplexBalance();
                    break;
                case 4:
                    ResetDemo();
                    break;
            }
            
            demoStep = (demoStep + 1) % 5;
        }
    }

    /// <summary>
    /// 演示步骤1：单个物体
    /// </summary>
    void DemoStep1_SingleObject()
    {
        Debug.Log("=== 演示1：单个物体下落 ===");
        
        if (objectPlacer != null)
        {
            objectPlacer.PlaceObjectAt(new Vector3(0, 5, 0));
        }
        else if (box1 != null)
        {
            GameObject obj = Instantiate(box1, new Vector3(0, 5, 0), Quaternion.identity);
            BalanceableObject balObj = obj.GetComponent<BalanceableObject>();
            
            if (balObj != null && balanceManager != null)
            {
                balanceManager.RegisterObject(balObj);
                Debug.Log($"物体质量: {balObj.mass}kg");
                Debug.Log($"初始质心: {balObj.worldCenterOfMass}");
            }
        }
    }

    /// <summary>
    /// 演示步骤2：堆叠物体（稳定）
    /// </summary>
    void DemoStep2_StackedObjects()
    {
        Debug.Log("=== 演示2：稳定堆叠 ===");
        
        // 放置底部物体
        if (objectPlacer != null)
        {
            objectPlacer.PlaceObjectAt(new Vector3(0, 1, 0));
        }
        
        // 等待一帧后放置顶部物体
        StartCoroutine(PlaceAfterDelay(new Vector3(0, 2.5f, 0), 0.5f));
    }

    /// <summary>
    /// 演示步骤3：不平衡堆叠
    /// </summary>
    void DemoStep3_UnbalancedStack()
    {
        Debug.Log("=== 演示3：不平衡堆叠（质心偏移） ===");
        
        // 放置底部物体
        if (objectPlacer != null)
        {
            objectPlacer.PlaceObjectAt(new Vector3(0, 1, 0));
        }
        
        // 放置偏移的顶部物体
        StartCoroutine(PlaceAfterDelay(new Vector3(0.6f, 2.5f, 0), 0.5f));
        
        Debug.Log("注意：顶部物体的质心偏移将导致倾倒");
    }

    /// <summary>
    /// 演示步骤4：复杂平衡
    /// </summary>
    void DemoStep4_ComplexBalance()
    {
        Debug.Log("=== 演示4：复杂多层平衡 ===");
        
        if (objectPlacer != null)
        {
            // 底层
            objectPlacer.PlaceObjectAt(new Vector3(0, 1, 0));
            
            // 中层
            StartCoroutine(PlaceAfterDelay(new Vector3(-0.3f, 2.5f, 0), 0.5f));
            StartCoroutine(PlaceAfterDelay(new Vector3(0.3f, 2.5f, 0), 0.6f));
            
            // 顶层
            StartCoroutine(PlaceAfterDelay(new Vector3(0, 4, 0), 1.2f));
        }
        
        Debug.Log("多个物体的组合质心将决定整体稳定性");
    }

    /// <summary>
    /// 延迟放置物体
    /// </summary>
    System.Collections.IEnumerator PlaceAfterDelay(Vector3 position, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (objectPlacer != null)
        {
            objectPlacer.PlaceObjectAt(position);
        }
    }

    /// <summary>
    /// 重置演示
    /// </summary>
    void ResetDemo()
    {
        Debug.Log("=== 重置演示场景 ===");
        
        // 查找并销毁所有动态创建的物体
        BalanceableObject[] allObjects = FindObjectsOfType<BalanceableObject>();
        
        foreach (var obj in allObjects)
        {
            if (balanceManager != null)
            {
                balanceManager.UnregisterObject(obj);
            }
            
            Destroy(obj.gameObject);
        }
        
        demoStep = 0;
        demoTimer = 0f;
    }

    /// <summary>
    /// 显示示例代码（在Inspector中查看脚本）
    /// </summary>
    void ExampleCode()
    {
        // === 示例1：创建一个可平衡物体 ===
        GameObject myObject = new GameObject("MyBalanceableObject");
        BalanceableObject balObj = myObject.AddComponent<BalanceableObject>();
        
        // 设置物理属性
        balObj.mass = 2.0f;
        balObj.centerOfMassOffset = new Vector2(0, -0.5f); // 质心偏下
        balObj.supportBaseWidth = 1.0f;
        
        // 设置平衡参数
        balObj.balanceForceMultiplier = 5.0f;
        balObj.torqueMultiplier = 2.0f;
        balObj.dampingFactor = 0.95f;

        // === 示例2：手动计算质心 ===
        BalanceableObject obj1 = null; // 假设已有对象
        BalanceableObject obj2 = null;
        
        if (obj1 != null && obj2 != null)
        {
            // 计算两个物体的组合质心
            Vector2 combinedCOM = BalanceCalculator.CalculateStackedCenterOfMass(obj1, obj2);
            
            // 检查是否稳定
            bool isStable = BalanceCalculator.IsCenterOfMassSupported(combinedCOM, obj1);
            
            Debug.Log($"组合质心: {combinedCOM}, 是否稳定: {isStable}");
        }

        // === 示例3：检查稳定性分数 ===
        if (balObj != null)
        {
            Vector2 supportRange = balObj.GetSupportBaseRange();
            Vector2 currentCOM = balObj.CalculateCombinedCenterOfMass();
            
            float stability = BalanceCalculator.CalculateStabilityScore(
                currentCOM,
                new Vector2(supportRange.x, 0),
                new Vector2(supportRange.y, 0)
            );
            
            Debug.Log($"稳定性分数: {stability:F2} (1.0 = 完全稳定)");
        }

        // === 示例4：注册到管理器 ===
        if (balanceManager != null && balObj != null)
        {
            balanceManager.RegisterObject(balObj);
            
            // 获取稳定性报告
            string report = balanceManager.GetStabilityReport(balObj);
            Debug.Log(report);
        }
    }
}


