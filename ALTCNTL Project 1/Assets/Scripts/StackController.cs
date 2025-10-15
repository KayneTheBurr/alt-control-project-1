using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 堆叠控制器 - 管理堆叠物体和掉落
/// </summary>
public class StackController : MonoBehaviour
{
    [Header("堆叠设置")]
    [Tooltip("堆叠的物体列表（从下到上）- 由代码自动管理")]
    [HideInInspector] // 隐藏以防止手动编辑
    public List<GameObject> stackedObjects = new List<GameObject>();
    
    [Tooltip("物体预制体（用于添加新物体）")]
    public GameObject objectPrefab;
    
    [Tooltip("初始堆叠数量")]
    public int initialStackCount = 5;
    
    [Tooltip("物体间距")]
    public float objectSpacing = 1.0f;
    
    [Tooltip("堆叠起始位置（最底部物品的位置）")]
    public Vector3 stackStartPosition = new Vector3(-4f, -3.4f, 3.4f);

    [Header("掉落设置")]
    [Tooltip("掉落时的力度")]
    public float dropForce = 5.0f;
    
    [Tooltip("掉落时的扭矩")]
    public float dropTorque = 2.0f;
    
    [Tooltip("掉落物体的生命周期（秒）")]
    public float droppedObjectLifetime = 3.0f;
    
    [Tooltip("掉落方向偏移")]
    public float dropDirectionOffset = 1.0f;

    [Header("引用")]
    public SwayController swayController;
    public StackVisualController visualController;

    [Header("调试")]
    public bool showDebugInfo = true;
    public bool logDropEvents = true;

    private Transform stackRoot;

    // 公开stackRoot供其他组件访问
    public Transform StackRoot => stackRoot;

    void Start()
    {
        if (swayController == null)
        {
            swayController = GetComponent<SwayController>();
        }

        if (visualController == null)
        {
            visualController = GetComponent<StackVisualController>();
        }

        // 创建堆叠根节点
        GameObject rootObj = new GameObject("StackRoot");
        rootObj.transform.SetParent(transform);
        rootObj.transform.localPosition = stackStartPosition;
        stackRoot = rootObj.transform;
        
        Debug.Log($"[StackController] StackRoot 创建于: {transform.name}");
        Debug.Log($"[StackController] StackRoot 本地位置: {stackRoot.localPosition}");
        Debug.Log($"[StackController] StackRoot 世界位置: {stackRoot.position}");

        // 将堆叠根节点设置给SwayController
        if (swayController != null)
        {
            swayController.stackRoot = stackRoot;
        }

        // 创建初始堆叠
        CreateInitialStack();

        // 订阅掉落事件
        if (swayController != null)
        {
            swayController.OnToppleDrop += OnToppleDrop;
        }

        // 通知视觉控制器刷新位置
        if (visualController != null)
        {
            visualController.RefreshPositions();
        }
    }

    /// <summary>
    /// 创建初始堆叠
    /// </summary>
    void CreateInitialStack()
    {
        if (initialStackCount <= 0)
        {
            Debug.LogWarning("[StackController] initialStackCount 为 0，不会创建物体！");
            return;
        }

        // 检查初始状态
        int initialCount = stackedObjects != null ? stackedObjects.Count : 0;
        Debug.Log($"[StackController] 初始状态检查：stackedObjects 中已有 {initialCount} 个物体");
        
        if (initialCount > 0)
        {
            Debug.LogWarning($"[StackController] 警告：在CreateInitialStack之前，stackedObjects已经有 {initialCount} 个物体！将清空后重新创建。");
            stackedObjects.Clear();
        }

        Debug.Log($"[StackController] 开始创建 {initialStackCount} 个堆叠物体...");
        
        for (int i = 0; i < initialStackCount; i++)
        {
            GameObject obj = AddObjectToStack();
            if (obj != null)
            {
                Debug.Log($"[StackController] 创建物体 {i}: {obj.name} 在位置 {obj.transform.position}");
            }
            else
            {
                Debug.LogError($"[StackController] 创建物体 {i} 失败！");
            }
        }

        Debug.Log($"[StackController] ✓ 堆叠创建完成: {stackedObjects.Count}/{initialStackCount} 个物体");
        Debug.Log($"[StackController] 物体父对象: {(stackRoot != null ? stackRoot.name : "null")}");
        
        if (stackedObjects.Count != initialStackCount)
        {
            Debug.LogError($"[StackController] 警告：实际创建数量 ({stackedObjects.Count}) 与预期 ({initialStackCount}) 不符！");
        }
    }

    /// <summary>
    /// 添加物体到堆叠顶部
    /// </summary>
    public GameObject AddObjectToStack()
    {
        GameObject newObject;
        
        if (objectPrefab != null)
        {
            newObject = Instantiate(objectPrefab, stackRoot);
            
            // 移除预制体上可能存在的物理组件（防止自动下落）
            Rigidbody rb = newObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log($"[StackController] 移除预制体上的 Rigidbody 组件");
                Destroy(rb);
            }
            
            Rigidbody2D rb2d = newObject.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                Debug.Log($"[StackController] 移除预制体上的 Rigidbody2D 组件");
                Destroy(rb2d);
            }
            
            // 移除碰撞器
            Collider col = newObject.GetComponent<Collider>();
            if (col != null)
            {
                Debug.Log($"[StackController] 移除预制体上的 Collider 组件");
                Destroy(col);
            }
            
            Collider2D col2d = newObject.GetComponent<Collider2D>();
            if (col2d != null)
            {
                Debug.Log($"[StackController] 移除预制体上的 Collider2D 组件");
                Destroy(col2d);
            }
            
            // 移除可能已存在的掉落动画组件（仅在倾倒时才应该有）
            DropAnimation dropAnim = newObject.GetComponent<DropAnimation>();
            if (dropAnim != null)
            {
                Debug.Log($"[StackController] 移除预制体上的 DropAnimation 组件");
                Destroy(dropAnim);
            }
        }
        else
        {
            // 如果没有预制体，创建一个简单的立方体
            newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            newObject.transform.SetParent(stackRoot);
            
            // 移除碰撞器（我们不使用物理引擎）
            Destroy(newObject.GetComponent<Collider>());
            
            // 移除可能的物理组件（防止自动下落）
            Rigidbody rb = newObject.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);
            
            Rigidbody2D rb2d = newObject.GetComponent<Rigidbody2D>();
            if (rb2d != null) Destroy(rb2d);
            
            // 移除可能的掉落动画组件
            DropAnimation dropAnim = newObject.GetComponent<DropAnimation>();
            if (dropAnim != null) Destroy(dropAnim);
            
            // 设置2D外观
            newObject.transform.localScale = new Vector3(1f, 1f, 0.1f);
            
            // 随机颜色
            Renderer renderer = newObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = new Color(
                    Random.Range(0.3f, 1f),
                    Random.Range(0.3f, 1f),
                    Random.Range(0.3f, 1f)
                );
            }
        }

        newObject.name = $"StackObject_{stackedObjects.Count}";
        
        // 设置位置（初始位置，会被StackVisualController调整）
        float yPosition = stackedObjects.Count * objectSpacing;
        newObject.transform.localPosition = new Vector3(0, yPosition, 0);
        
        // 确保物体可见（设置layer和tag）
        newObject.layer = gameObject.layer;
        
        // 验证父对象设置正确
        if (newObject.transform.parent != stackRoot)
        {
            Debug.LogError($"[StackController] 错误：{newObject.name} 的父对象不是 StackRoot！强制设置。");
            newObject.transform.SetParent(stackRoot);
            newObject.transform.localPosition = new Vector3(0, yPosition, 0);
        }
        
        stackedObjects.Add(newObject);

        if (logDropEvents)
        {
            Debug.Log($"[StackController] ✓ 添加物体: {newObject.name}");
            Debug.Log($"  - 父对象: {newObject.transform.parent.name}");
            Debug.Log($"  - 本地位置: {newObject.transform.localPosition}");
            Debug.Log($"  - 世界位置: {newObject.transform.position}");
            Debug.Log($"  - 有物理组件: Rigidbody={newObject.GetComponent<Rigidbody>() != null}, Rigidbody2D={newObject.GetComponent<Rigidbody2D>() != null}");
        }

        // 通知视觉控制器刷新位置
        if (visualController != null)
        {
            visualController.RefreshPositions();
        }
        
        return newObject;
    }

    /// <summary>
    /// 从堆叠顶部移除物体
    /// </summary>
    public GameObject RemoveTopObject()
    {
        if (stackedObjects.Count == 0)
        {
            if (logDropEvents)
            {
                Debug.LogWarning("[StackController] 堆叠为空，无法移除物体");
            }
            return null;
        }

        int topIndex = stackedObjects.Count - 1;
        GameObject topObject = stackedObjects[topIndex];
        stackedObjects.RemoveAt(topIndex);

        // 通知视觉控制器刷新位置
        if (visualController != null)
        {
            visualController.RefreshPositions();
        }

        return topObject;
    }

    /// <summary>
    /// 倾倒掉落事件处理
    /// </summary>
    void OnToppleDrop()
    {
        if (stackedObjects.Count == 0)
        {
            if (logDropEvents)
            {
                Debug.LogWarning("[StackController] 堆叠已空，无法掉落物体");
            }
            return;
        }

        GameObject droppedObject = RemoveTopObject();
        
        if (droppedObject == null) return;

        // 保存当前世界位置和旋转
        Vector3 worldPosition = droppedObject.transform.position;
        Quaternion worldRotation = droppedObject.transform.rotation;

        if (logDropEvents)
        {
            Debug.Log($"[StackController] 掉落物体: {droppedObject.name}");
            Debug.Log($"  - 掉落位置: {worldPosition}");
            Debug.Log($"  - 剩余物体: {stackedObjects.Count}");
        }

        // 将物体从堆叠根节点移出，保持世界位置
        droppedObject.transform.SetParent(transform.parent, true); // worldPositionStays = true
        
        // 确保位置正确（以防万一）
        droppedObject.transform.position = worldPosition;
        droppedObject.transform.rotation = worldRotation;
        
        // 应用掉落效果
        ApplyDropEffect(droppedObject);

        // 设置自动销毁
        Destroy(droppedObject, droppedObjectLifetime);
    }

    /// <summary>
    /// 应用掉落效果
    /// </summary>
    void ApplyDropEffect(GameObject obj)
    {
        // 添加简单的掉落动画组件
        DropAnimation dropAnim = obj.AddComponent<DropAnimation>();
        
        // 计算掉落方向（基于当前摇摆方向）
        float direction = 0f;
        if (swayController != null)
        {
            direction = swayController.CurrentPhase.GetDirection();
        }
        
        // 设置掉落参数（从当前位置开始）
        dropAnim.dropVelocity = new Vector2(direction * dropForce, -dropForce * 0.5f);
        dropAnim.angularVelocity = Random.Range(-dropTorque, dropTorque) * 100f;
        dropAnim.gravity = 9.81f;
        
        if (logDropEvents)
        {
            Debug.Log($"[StackController] 掉落动画设置:");
            Debug.Log($"  - 初始速度: {dropAnim.dropVelocity}");
            Debug.Log($"  - 角速度: {dropAnim.angularVelocity}");
            Debug.Log($"  - 方向: {(direction < 0 ? "左" : direction > 0 ? "右" : "中")}");
        }
    }

    /// <summary>
    /// 获取堆叠高度
    /// </summary>
    public int GetStackHeight()
    {
        return stackedObjects.Count;
    }

    /// <summary>
    /// 清空堆叠
    /// </summary>
    public void ClearStack()
    {
        foreach (var obj in stackedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        stackedObjects.Clear();
    }

    /// <summary>
    /// 重建堆叠
    /// </summary>
    public void RebuildStack(int count)
    {
        ClearStack();
        
        for (int i = 0; i < count; i++)
        {
            AddObjectToStack();
        }
    }

    void OnGUI()
    {
        if (!showDebugInfo) return;

        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.fontSize = 14;
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = Color.yellow;

        string info = $"堆叠高度: {stackedObjects.Count}";

        GUI.Box(new Rect(10, 200, 200, 30), info, style);
    }

    void OnDestroy()
    {
        if (swayController != null)
        {
            swayController.OnToppleDrop -= OnToppleDrop;
        }
    }
}

/// <summary>
/// 掉落动画组件 - 简单的非物理掉落效果
/// </summary>
public class DropAnimation : MonoBehaviour
{
    public Vector2 dropVelocity = Vector2.zero;
    public float angularVelocity = 0f;
    public float gravity = 9.81f;
    public float damping = 0.98f;

    void Update()
    {
        // 应用重力
        dropVelocity.y -= gravity * Time.deltaTime;
        
        // 更新位置
        transform.position += (Vector3)dropVelocity * Time.deltaTime;
        
        // 更新旋转
        transform.Rotate(0, 0, angularVelocity * Time.deltaTime);
        
        // 应用阻尼
        dropVelocity *= damping;
        angularVelocity *= damping;
    }
}

