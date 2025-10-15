using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理所有可平衡物体的平衡模拟
/// </summary>
public class BalanceManager : MonoBehaviour
{
    [Header("全局设置")]
    [Tooltip("地面Y坐标")]
    public float groundLevel = 0f;
    
    [Tooltip("自动检测物体支撑关系")]
    public bool autoDetectSupport = true;
    
    [Tooltip("支撑检测范围")]
    public float supportDetectionRange = 2.0f;
    
    [Header("物理更新")]
    [Tooltip("使用固定时间步长")]
    public bool useFixedTimeStep = true;
    
    [Tooltip("固定时间步长（秒）")]
    public float fixedTimeStep = 0.02f;
    
    [Header("调试")]
    public bool showDebugInfo = true;
    public bool logBalanceEvents = false;

    private List<BalanceableObject> allObjects = new List<BalanceableObject>();
    private float accumulatedTime = 0f;

    void Start()
    {
        RegisterAllObjects();
        
        if (autoDetectSupport)
        {
            DetectAllSupportRelationships();
        }

        if (logBalanceEvents)
        {
            Debug.Log($"[BalanceManager] 初始化完成，管理 {allObjects.Count} 个物体");
        }
    }

    void Update()
    {
        if (useFixedTimeStep)
        {
            accumulatedTime += Time.deltaTime;
            
            while (accumulatedTime >= fixedTimeStep)
            {
                UpdateBalanceSimulation(fixedTimeStep);
                accumulatedTime -= fixedTimeStep;
            }
        }
        else
        {
            UpdateBalanceSimulation(Time.deltaTime);
        }
    }

    /// <summary>
    /// 注册场景中的所有可平衡物体
    /// </summary>
    void RegisterAllObjects()
    {
        allObjects.Clear();
        BalanceableObject[] objects = FindObjectsOfType<BalanceableObject>();
        
        foreach (var obj in objects)
        {
            obj.Initialize();
            allObjects.Add(obj);
        }

        if (logBalanceEvents)
        {
            Debug.Log($"[BalanceManager] 注册了 {allObjects.Count} 个可平衡物体");
        }
    }

    /// <summary>
    /// 手动注册一个新物体
    /// </summary>
    public void RegisterObject(BalanceableObject obj)
    {
        if (!allObjects.Contains(obj))
        {
            obj.Initialize();
            allObjects.Add(obj);
            
            if (autoDetectSupport)
            {
                UpdateObjectSupport(obj);
            }

            if (logBalanceEvents)
            {
                Debug.Log($"[BalanceManager] 注册新物体: {obj.name}");
            }
        }
    }

    /// <summary>
    /// 注销物体
    /// </summary>
    public void UnregisterObject(BalanceableObject obj)
    {
        if (allObjects.Contains(obj))
        {
            // 移除与其他物体的支撑关系
            foreach (var other in allObjects)
            {
                other.objectsAbove.Remove(obj);
                other.objectsBelow.Remove(obj);
            }
            
            allObjects.Remove(obj);

            if (logBalanceEvents)
            {
                Debug.Log($"[BalanceManager] 注销物体: {obj.name}");
            }
        }
    }

    /// <summary>
    /// 检测所有物体的支撑关系
    /// </summary>
    void DetectAllSupportRelationships()
    {
        foreach (var obj in allObjects)
        {
            UpdateObjectSupport(obj);
        }
    }

    /// <summary>
    /// 更新单个物体的支撑关系
    /// </summary>
    void UpdateObjectSupport(BalanceableObject obj)
    {
        obj.objectsBelow.Clear();
        obj.objectsAbove.Clear();
        obj.isGrounded = false;

        // 检查是否在地面上
        if (obj.transform.position.y <= groundLevel + obj.supportCheckOffset)
        {
            obj.isGrounded = true;
            return;
        }

        // 检查与其他物体的支撑关系
        foreach (var other in allObjects)
        {
            if (other == obj) continue;

            float verticalDistance = obj.transform.position.y - other.transform.position.y;
            float horizontalDistance = Mathf.Abs(obj.transform.position.x - other.transform.position.x);

            // 如果在检测范围内
            if (Mathf.Abs(verticalDistance) < supportDetectionRange && 
                horizontalDistance < supportDetectionRange)
            {
                // 判断上下关系
                if (verticalDistance > 0)
                {
                    // obj 在 other 上方
                    if (!obj.objectsBelow.Contains(other))
                    {
                        obj.objectsBelow.Add(other);
                    }
                    if (!other.objectsAbove.Contains(obj))
                    {
                        other.objectsAbove.Add(obj);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 更新平衡模拟
    /// </summary>
    void UpdateBalanceSimulation(float deltaTime)
    {
        // 首先更新所有物体的质心
        foreach (var obj in allObjects)
        {
            obj.UpdateWorldCenterOfMass();
        }

        // 然后计算并应用平衡力
        foreach (var obj in allObjects)
        {
            if (obj.isGrounded)
            {
                // 接地物体不需要平衡模拟
                continue;
            }

            if (obj.objectsBelow.Count == 0)
            {
                // 没有支撑物体，应用自由落体
                ApplyFreeFall(obj);
            }
            else
            {
                // 有支撑物体，计算平衡
                SimulateBalance(obj);
            }
        }

        // 更新支撑关系（可选，如果物体移动较多）
        if (autoDetectSupport)
        {
            DetectAllSupportRelationships();
        }
    }

    /// <summary>
    /// 应用自由落体
    /// </summary>
    void ApplyFreeFall(BalanceableObject obj)
    {
        Vector2 gravityForce = new Vector2(0, -obj.mass * obj.gravity);
        obj.ApplyBalanceForce(gravityForce, 0);

        // 检查是否着地
        if (obj.transform.position.y <= groundLevel)
        {
            obj.transform.position = new Vector3(
                obj.transform.position.x,
                groundLevel,
                obj.transform.position.z
            );
            obj.velocity = Vector2.zero;
            obj.angularVelocity = 0f;
            obj.isGrounded = true;

            if (logBalanceEvents)
            {
                Debug.Log($"[BalanceManager] {obj.name} 已着地");
            }
        }
    }

    /// <summary>
    /// 模拟物体的平衡
    /// </summary>
    void SimulateBalance(BalanceableObject obj)
    {
        // 获取主要支撑物体（假设是最接近的一个）
        BalanceableObject primarySupport = FindPrimarySupportObject(obj);
        
        if (primarySupport == null)
        {
            ApplyFreeFall(obj);
            return;
        }

        // 计算组合质心
        Vector2 combinedCOM = obj.CalculateCombinedCenterOfMass();
        
        // 找到支撑点
        obj.supportPivot = BalanceCalculator.FindSupportPivot(obj, primarySupport);
        
        // 检查质心是否在支撑基底内
        bool isSupported = primarySupport.IsPointWithinSupportBase(combinedCOM);
        
        if (!isSupported)
        {
            // 不稳定，应用倾倒力和扭矩
            ApplyTippingForces(obj, primarySupport, combinedCOM);
        }
        else
        {
            // 稳定，应用恢复力维持平衡
            ApplyRestoringForces(obj, primarySupport, combinedCOM);
        }
    }

    /// <summary>
    /// 找到主要支撑物体
    /// </summary>
    BalanceableObject FindPrimarySupportObject(BalanceableObject obj)
    {
        if (obj.objectsBelow.Count == 0) return null;
        
        // 返回最接近的支撑物体
        BalanceableObject closest = null;
        float minDistance = float.MaxValue;
        
        foreach (var support in obj.objectsBelow)
        {
            if (support == null) continue;
            
            float distance = Vector2.Distance(
                obj.transform.position,
                support.transform.position
            );
            
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = support;
            }
        }
        
        return closest;
    }

    /// <summary>
    /// 应用倾倒力（当物体不稳定时）
    /// </summary>
    void ApplyTippingForces(
        BalanceableObject obj, 
        BalanceableObject support,
        Vector2 combinedCOM)
    {
        // 计算扭矩
        float torque = BalanceCalculator.CalculateTorque(
            obj, 
            obj.supportPivot, 
            obj.torqueMultiplier
        );
        
        // 应用重力
        Vector2 gravityForce = new Vector2(0, -obj.mass * obj.gravity);
        
        obj.ApplyBalanceForce(gravityForce, torque);

        if (logBalanceEvents)
        {
            Debug.Log($"[BalanceManager] {obj.name} 不稳定，扭矩: {torque:F2}");
        }
    }

    /// <summary>
    /// 应用恢复力（维持平衡）
    /// </summary>
    void ApplyRestoringForces(
        BalanceableObject obj,
        BalanceableObject support,
        Vector2 combinedCOM)
    {
        // 计算目标位置（支撑点正上方）
        Vector2 desiredCOM = new Vector2(
            obj.supportPivot.x,
            combinedCOM.y
        );
        
        // 计算恢复力
        Vector2 restoringForce = BalanceCalculator.CalculateBalanceForce(
            obj,
            desiredCOM,
            obj.balanceForceMultiplier
        );
        
        // 计算小幅度的稳定扭矩
        float stabilizingTorque = BalanceCalculator.CalculateTorque(
            obj,
            obj.supportPivot,
            obj.torqueMultiplier * 0.5f
        );
        
        obj.ApplyBalanceForce(restoringForce, stabilizingTorque);
    }

    /// <summary>
    /// 获取稳定性报告（用于调试）
    /// </summary>
    public string GetStabilityReport(BalanceableObject obj)
    {
        if (obj.isGrounded)
        {
            return $"{obj.name}: 已着地（稳定）";
        }

        if (obj.objectsBelow.Count == 0)
        {
            return $"{obj.name}: 无支撑（下落中）";
        }

        Vector2 combinedCOM = obj.CalculateCombinedCenterOfMass();
        BalanceableObject primarySupport = FindPrimarySupportObject(obj);
        
        if (primarySupport == null)
        {
            return $"{obj.name}: 支撑物体丢失";
        }

        Vector2 supportRange = primarySupport.GetSupportBaseRange();
        float stability = BalanceCalculator.CalculateStabilityScore(
            combinedCOM,
            new Vector2(supportRange.x, 0),
            new Vector2(supportRange.y, 0)
        );

        return $"{obj.name}: 稳定性 {stability:F2} (质心: {combinedCOM}, 支撑: {primarySupport.name})";
    }

    /// <summary>
    /// 在场景视图中显示调试信息
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // 绘制地面线
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            new Vector3(-100, groundLevel, 0),
            new Vector3(100, groundLevel, 0)
        );

        // 绘制支撑关系
        if (Application.isPlaying)
        {
            foreach (var obj in allObjects)
            {
                if (obj == null) continue;

                foreach (var supportObj in obj.objectsBelow)
                {
                    if (supportObj == null) continue;

                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(
                        obj.transform.position,
                        supportObj.transform.position
                    );
                }
            }
        }
    }
}


