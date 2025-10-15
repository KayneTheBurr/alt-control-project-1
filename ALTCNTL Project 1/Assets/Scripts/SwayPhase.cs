using UnityEngine;

/// <summary>
/// 摇摆阶段枚举
/// </summary>
public enum SwayPhase
{
    LeftTopple = -3,    // 左倾倒
    LeftTilt2 = -2,     // 左倾2
    LeftTilt1 = -1,     // 左倾1
    Center = 0,         // 居中（默认）
    RightTilt1 = 1,     // 右倾1
    RightTilt2 = 2,     // 右倾2
    RightTopple = 3     // 右倾倒
}

/// <summary>
/// 摇摆阶段扩展方法
/// </summary>
public static class SwayPhaseExtensions
{
    /// <summary>
    /// 检查是否为倾倒阶段
    /// </summary>
    public static bool IsTopplePhase(this SwayPhase phase)
    {
        return phase == SwayPhase.LeftTopple || phase == SwayPhase.RightTopple;
    }

    /// <summary>
    /// 检查是否为居中阶段
    /// </summary>
    public static bool IsCenterPhase(this SwayPhase phase)
    {
        return phase == SwayPhase.Center;
    }

    /// <summary>
    /// 检查是否为倾斜阶段（不包括倾倒和居中）
    /// </summary>
    public static bool IsTiltPhase(this SwayPhase phase)
    {
        return !phase.IsTopplePhase() && !phase.IsCenterPhase();
    }

    /// <summary>
    /// 获取倾斜方向（-1=左，0=居中，1=右）
    /// </summary>
    public static int GetDirection(this SwayPhase phase)
    {
        if ((int)phase < 0) return -1;
        if ((int)phase > 0) return 1;
        return 0;
    }

    /// <summary>
    /// 获取同方向的下一个阶段
    /// </summary>
    public static SwayPhase GetNextPhaseInSameDirection(this SwayPhase phase)
    {
        int direction = phase.GetDirection();
        int currentLevel = Mathf.Abs((int)phase);
        
        // 如果已经是倾倒阶段，保持不变
        if (currentLevel >= 3) return phase;
        
        // 移动到下一个阶段
        return (SwayPhase)(direction * (currentLevel + 1));
    }

    /// <summary>
    /// 获取向中心移动一步的阶段
    /// </summary>
    public static SwayPhase GetNextPhaseTowardCenter(this SwayPhase phase)
    {
        if (phase.IsCenterPhase()) return phase;
        
        int direction = phase.GetDirection();
        int currentLevel = Mathf.Abs((int)phase);
        
        // 移动到更接近中心的阶段
        return (SwayPhase)(direction * (currentLevel - 1));
    }

    /// <summary>
    /// 获取阶段的旋转角度
    /// </summary>
    public static float GetRotationAngle(this SwayPhase phase)
    {
        switch (phase)
        {
            case SwayPhase.LeftTopple: return -35f;  // 修改：-45° → -35°
            case SwayPhase.LeftTilt2: return -25f;   // 修改：-30° → -25°
            case SwayPhase.LeftTilt1: return -15f;
            case SwayPhase.Center: return 0f;
            case SwayPhase.RightTilt1: return 15f;
            case SwayPhase.RightTilt2: return 25f;   // 修改：30° → 25°
            case SwayPhase.RightTopple: return 35f;  // 修改：45° → 35°
            default: return 0f;
        }
    }

    /// <summary>
    /// 获取阶段的中文名称
    /// </summary>
    public static string GetChineseName(this SwayPhase phase)
    {
        switch (phase)
        {
            case SwayPhase.LeftTopple: return "左倾倒";
            case SwayPhase.LeftTilt2: return "左倾2";
            case SwayPhase.LeftTilt1: return "左倾1";
            case SwayPhase.Center: return "居中";
            case SwayPhase.RightTilt1: return "右倾1";
            case SwayPhase.RightTilt2: return "右倾2";
            case SwayPhase.RightTopple: return "右倾倒";
            default: return "未知";
        }
    }

    /// <summary>
    /// 获取所有倾斜阶段（不包括倾倒和居中）
    /// </summary>
    public static SwayPhase[] GetTiltPhases()
    {
        return new SwayPhase[]
        {
            SwayPhase.LeftTilt2,
            SwayPhase.LeftTilt1,
            SwayPhase.RightTilt1,
            SwayPhase.RightTilt2
        };
    }
}

