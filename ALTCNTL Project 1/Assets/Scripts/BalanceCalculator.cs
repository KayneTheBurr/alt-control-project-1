using UnityEngine;

/// <summary>
/// 包含平衡计算的静态工具类
/// </summary>
public static class BalanceCalculator
{
    /// <summary>
    /// 计算两个物体堆叠时的组合质心
    /// </summary>
    public static Vector2 CalculateStackedCenterOfMass(
        BalanceableObject bottomObject, 
        BalanceableObject topObject)
    {
        float totalMass = bottomObject.mass + topObject.mass;
        Vector2 weightedSum = 
            bottomObject.worldCenterOfMass * bottomObject.mass +
            topObject.worldCenterOfMass * topObject.mass;
        
        return weightedSum / totalMass;
    }

    /// <summary>
    /// 计算多个物体的组合质心
    /// </summary>
    public static Vector2 CalculateCombinedCenterOfMass(
        BalanceableObject[] objects)
    {
        if (objects == null || objects.Length == 0)
            return Vector2.zero;

        float totalMass = 0f;
        Vector2 weightedSum = Vector2.zero;

        foreach (var obj in objects)
        {
            if (obj != null)
            {
                totalMass += obj.mass;
                weightedSum += obj.worldCenterOfMass * obj.mass;
            }
        }

        if (totalMass <= 0f) return Vector2.zero;
        
        return weightedSum / totalMass;
    }

    /// <summary>
    /// 检查质心是否在支撑基底内（稳定性检查）
    /// </summary>
    public static bool IsCenterOfMassSupported(
        Vector2 centerOfMass, 
        BalanceableObject supportObject)
    {
        return supportObject.IsPointWithinSupportBase(centerOfMass);
    }

    /// <summary>
    /// 计算使物体恢复平衡所需的力
    /// </summary>
    public static Vector2 CalculateBalanceForce(
        BalanceableObject targetObject,
        Vector2 desiredCenterOfMass,
        float forceMultiplier)
    {
        Vector2 currentCOM = targetObject.worldCenterOfMass;
        Vector2 displacement = desiredCenterOfMass - currentCOM;
        
        // 使用弹簧力模型
        Vector2 force = displacement * forceMultiplier;
        
        // 添加重力影响
        force.y -= targetObject.mass * targetObject.gravity;
        
        return force;
    }

    /// <summary>
    /// 计算基于质心偏移的扭矩
    /// </summary>
    public static float CalculateTorque(
        BalanceableObject targetObject,
        Vector2 supportPivot,
        float torqueMultiplier)
    {
        Vector2 comPos = targetObject.CalculateCombinedCenterOfMass();
        Vector2 pivotToCOM = comPos - supportPivot;
        
        // 计算力矩臂（水平距离）
        float leverArm = pivotToCOM.x;
        
        // 计算重力产生的扭矩
        float totalMass = targetObject.GetTotalMass();
        float gravityForce = totalMass * targetObject.gravity;
        float torque = -leverArm * gravityForce * torqueMultiplier;
        
        return torque;
    }

    /// <summary>
    /// 找到最近的支撑点（支点）
    /// </summary>
    public static Vector2 FindSupportPivot(
        BalanceableObject targetObject,
        BalanceableObject supportObject)
    {
        Vector2 comPos = targetObject.CalculateCombinedCenterOfMass();
        Vector2 supportRange = supportObject.GetSupportBaseRange();
        
        // 将质心投影到支撑基底上
        float projectedX = Mathf.Clamp(comPos.x, supportRange.x, supportRange.y);
        float pivotY = supportObject.transform.position.y + supportObject.supportBaseWidth / 2f;
        
        return new Vector2(projectedX, pivotY);
    }

    /// <summary>
    /// 计算稳定性分数（0-1，1为完全稳定）
    /// </summary>
    public static float CalculateStabilityScore(
        Vector2 centerOfMass,
        Vector2 supportBaseMin,
        Vector2 supportBaseMax)
    {
        float supportWidth = supportBaseMax.x - supportBaseMin.x;
        if (supportWidth <= 0f) return 0f;

        // 计算质心到支撑基底中心的距离
        float supportCenter = (supportBaseMin.x + supportBaseMax.x) / 2f;
        float distanceFromCenter = Mathf.Abs(centerOfMass.x - supportCenter);
        
        // 归一化距离（0为中心，1为边缘）
        float normalizedDistance = distanceFromCenter / (supportWidth / 2f);
        
        // 如果超出支撑范围，返回负值
        if (centerOfMass.x < supportBaseMin.x || centerOfMass.x > supportBaseMax.x)
        {
            return -normalizedDistance;
        }
        
        // 返回稳定性分数（越接近中心越稳定）
        return 1f - normalizedDistance;
    }

    /// <summary>
    /// 检测两个物体是否在垂直方向上重叠
    /// </summary>
    public static bool IsObjectAbove(
        BalanceableObject upperObject,
        BalanceableObject lowerObject,
        float tolerance = 0.1f)
    {
        float upperBottom = upperObject.transform.position.y - upperObject.supportCheckOffset;
        float lowerTop = lowerObject.transform.position.y + lowerObject.supportCheckOffset;
        
        return upperBottom >= lowerTop - tolerance;
    }

    /// <summary>
    /// 计算物体掉落时的影响力
    /// </summary>
    public static Vector2 CalculateFallImpact(
        BalanceableObject fallingObject,
        float fallHeight)
    {
        // 使用简化的自由落体公式计算冲击力
        float velocity = Mathf.Sqrt(2f * fallingObject.gravity * fallHeight);
        float impactForce = fallingObject.mass * velocity;
        
        return new Vector2(0, -impactForce);
    }
}


