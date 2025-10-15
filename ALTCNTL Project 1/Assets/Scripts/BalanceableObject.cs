using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 表示一个可平衡的物体，跟踪其质心、质量和支撑关系
/// </summary>
public class BalanceableObject : MonoBehaviour
{
    [Header("物理属性")]
    [Tooltip("物体的质量（千克）")]
    public float mass = 1.0f;
    
    [Tooltip("相对于物体中心的质心偏移")]
    public Vector2 centerOfMassOffset = Vector2.zero;
    
    [Header("平衡参数")]
    [Tooltip("平衡力的强度系数")]
    public float balanceForceMultiplier = 5.0f;
    
    [Tooltip("扭矩系数")]
    public float torqueMultiplier = 2.0f;
    
    [Tooltip("阻尼系数（防止震荡）")]
    public float dampingFactor = 0.95f;
    
    [Tooltip("重力加速度")]
    public float gravity = 9.81f;
    
    [Header("支撑检测")]
    [Tooltip("物体的支撑基底宽度")]
    public float supportBaseWidth = 1.0f;
    
    [Tooltip("支撑检测的垂直偏移")]
    public float supportCheckOffset = 0.1f;

    [Header("调试")]
    public bool showDebugInfo = true;
    public Color debugCenterOfMassColor = Color.red;
    public Color debugSupportBaseColor = Color.green;

    // 运行时数据
    [HideInInspector] public Vector2 worldCenterOfMass;
    [HideInInspector] public Vector2 velocity;
    [HideInInspector] public float angularVelocity;
    [HideInInspector] public List<BalanceableObject> objectsBelow = new List<BalanceableObject>();
    [HideInInspector] public List<BalanceableObject> objectsAbove = new List<BalanceableObject>();
    [HideInInspector] public bool isGrounded = false;
    [HideInInspector] public Vector2 supportPivot;
    
    private bool isInitialized = false;

    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (isInitialized) return;
        
        UpdateWorldCenterOfMass();
        velocity = Vector2.zero;
        angularVelocity = 0f;
        isInitialized = true;
    }

    /// <summary>
    /// 更新世界坐标系中的质心位置
    /// </summary>
    public void UpdateWorldCenterOfMass()
    {
        worldCenterOfMass = (Vector2)transform.position + centerOfMassOffset;
    }

    /// <summary>
    /// 计算包含上方所有物体的组合质心
    /// </summary>
    public Vector2 CalculateCombinedCenterOfMass()
    {
        float totalMass = mass;
        Vector2 weightedSum = worldCenterOfMass * mass;

        foreach (var obj in objectsAbove)
        {
            if (obj != null)
            {
                Vector2 objCombinedCOM = obj.CalculateCombinedCenterOfMass();
                float objTotalMass = obj.GetTotalMass();
                
                weightedSum += objCombinedCOM * objTotalMass;
                totalMass += objTotalMass;
            }
        }

        return weightedSum / totalMass;
    }

    /// <summary>
    /// 获取包含上方所有物体的总质量
    /// </summary>
    public float GetTotalMass()
    {
        float totalMass = mass;
        foreach (var obj in objectsAbove)
        {
            if (obj != null)
            {
                totalMass += obj.GetTotalMass();
            }
        }
        return totalMass;
    }

    /// <summary>
    /// 获取物体的支撑基底范围（最左和最右点）
    /// </summary>
    public Vector2 GetSupportBaseRange()
    {
        float halfWidth = supportBaseWidth / 2f;
        float baseY = transform.position.y - supportCheckOffset;
        
        return new Vector2(
            transform.position.x - halfWidth,
            transform.position.x + halfWidth
        );
    }

    /// <summary>
    /// 检查给定的质心是否在支撑基底内
    /// </summary>
    public bool IsPointWithinSupportBase(Vector2 point)
    {
        Vector2 range = GetSupportBaseRange();
        return point.x >= range.x && point.x <= range.y;
    }

    /// <summary>
    /// 应用平衡力和扭矩
    /// </summary>
    public void ApplyBalanceForce(Vector2 force, float torque)
    {
        // 应用线性力
        Vector2 acceleration = force / mass;
        velocity += acceleration * Time.deltaTime;
        
        // 应用阻尼
        velocity *= dampingFactor;
        
        // 更新位置
        transform.position += (Vector3)(velocity * Time.deltaTime);
        
        // 应用扭矩（转换为角加速度）
        float angularAcceleration = torque / mass;
        angularVelocity += angularAcceleration * Time.deltaTime;
        
        // 应用角阻尼
        angularVelocity *= dampingFactor;
        
        // 更新旋转
        float rotationChange = angularVelocity * Time.deltaTime * Mathf.Rad2Deg;
        transform.Rotate(0, 0, rotationChange);
        
        // 更新质心
        UpdateWorldCenterOfMass();
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;

        // 绘制质心
        Gizmos.color = debugCenterOfMassColor;
        Vector2 comPos = Application.isPlaying ? worldCenterOfMass : 
                         (Vector2)transform.position + centerOfMassOffset;
        Gizmos.DrawSphere(comPos, 0.1f);
        Gizmos.DrawLine(transform.position, comPos);

        // 绘制支撑基底
        Gizmos.color = debugSupportBaseColor;
        Vector2 range = GetSupportBaseRange();
        float baseY = transform.position.y - supportCheckOffset;
        Vector3 leftPoint = new Vector3(range.x, baseY, 0);
        Vector3 rightPoint = new Vector3(range.y, baseY, 0);
        Gizmos.DrawLine(leftPoint, rightPoint);
        Gizmos.DrawSphere(leftPoint, 0.05f);
        Gizmos.DrawSphere(rightPoint, 0.05f);

        // 如果有支撑点，绘制它
        if (Application.isPlaying && !isGrounded && objectsBelow.Count > 0)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(supportPivot, 0.08f);
        }
    }
}


