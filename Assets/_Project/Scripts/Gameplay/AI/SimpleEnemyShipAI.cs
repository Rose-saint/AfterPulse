using UnityEngine;

/// <summary>
/// 最简版敌人飞船 AI（不依赖 ShipController，不用刚体力学）
/// 功能：
/// - 在一定距离内发现玩家后朝玩家飞过去
/// - 接近预定攻击距离后，改为侧向绕圈
/// - 机头始终朝向实际运动方向
/// </summary>
[DisallowMultipleComponent]
public class SimpleEnemyShipAI : MonoBehaviour
{
    public enum AIState
    {
        Idle,       // 什么也不干（或按当前朝向缓慢飞行）
        Approach,   // 朝玩家直线接近
        Orbit       // 侧向绕圈
    }

    [Header("目标")]
    public Transform target;
    [Tooltip("如果 target 为空，则按标签自动寻找")]
    public string playerTag = "Player";

    [Header("距离设置")]
    [Tooltip("发现玩家的最大距离")]
    public float detectRadius = 1000f;
    [Tooltip("期望的攻击距离（大致绕圈半径）")]
    public float orbitDistance = 600f;
    [Tooltip("允许的攻击距离误差带，越大越“宽松”")]
    public float orbitDistanceTolerance = 100f;

    [Header("移动参数")]
    [Tooltip("接近/绕圈的飞行速度（单位：米/秒）")]
    public float moveSpeed = 200f;
    [Tooltip("转向速度：每秒最多能转多少度")]
    public float turnSpeedDeg = 180f;
    [Tooltip("敌人无目标时，是否按当前朝向缓慢前进")]
    public bool idleForwardMove = false;
    public float idleSpeed = 50f;

    [Header("绕圈参数")]
    [Tooltip("绕圈方向：勾选 = 顺时针；不勾选 = 逆时针（从上往下看）")]
    public bool orbitClockwise = true;
    [Tooltip("距离偏大/偏小时，往里/往外修正的强度系数")]
    public float radialAdjustFactor = 0.5f;

    [Header("武器设置")]
    [Tooltip("敌人使用的主炮控制器（如果为空会在子物体里自动查找）")]
    public WeaponFireController weapon;
    [Tooltip("AI 额外攻击间隔（秒），在武器自身射速基础上再加一层节奏控制，可设为 0")]
    public float fireInterval = 0.5f;
    [Tooltip("只有当前方与玩家方向夹角小于该值（度）时才开火")]
    public float maxFireAngle = 20f;

    [Header("调试观察")]
    public AIState currentState;
    public Vector3 debugDesiredMoveDir;

    Transform _tf;
    float _fireTimer = 0f;

    void Awake()
    {
        _tf = transform;

        if (target == null && !string.IsNullOrEmpty(playerTag))
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null) target = p.transform;
        }

        if (!weapon)
        {
            weapon = GetComponent<WeaponFireController>();
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        _fireTimer += dt;

        // 1）如果没有目标
        if (target == null)
        {
            currentState = AIState.Idle;

            if (idleForwardMove)
            {
                Vector3 dir = _tf.forward;
                _tf.position += dir * idleSpeed * dt;
            }
            return;
        }

        Vector3 toTarget = target.position - _tf.position;
        float dist = toTarget.magnitude;

        // 2）根据距离决定当前状态
        if (dist > detectRadius)
        {
            currentState = AIState.Idle;
        }
        else
        {
            float minOrbit = orbitDistance - orbitDistanceTolerance;
            float maxOrbit = orbitDistance + orbitDistanceTolerance;

            if (dist > maxOrbit)
            {
                // 远离攻击圈 → 接近玩家
                currentState = AIState.Approach;
            }
            else
            {
                // 到达攻击圈范围内 → 环绕
                currentState = AIState.Orbit;
            }
        }

        // 3）根据状态计算“本帧想要的移动方向”
        Vector3 desiredMoveDir = _tf.forward; // 默认按当前朝向
        switch (currentState)
        {
            case AIState.Idle:
                {
                    if (idleForwardMove)
                    {
                        desiredMoveDir = _tf.forward;
                    }
                    else
                    {
                        // 不动：直接 return
                        return;
                    }
                    break;
                }

            case AIState.Approach:
                {
                    if (dist > 0.01f)
                        desiredMoveDir = toTarget.normalized; // 直接朝玩家方向飞
                    break;
                }

            case AIState.Orbit:
                {
                    desiredMoveDir = ComputeOrbitDirection(toTarget, dist);
                    break;
                }
        }

        // 4）沿“期望移动方向”位移
        if (desiredMoveDir.sqrMagnitude > 0.0001f)
        {
            desiredMoveDir.Normalize();
            debugDesiredMoveDir = desiredMoveDir;

            _tf.position += desiredMoveDir * moveSpeed * dt;

            // 5）机头旋转对齐运动方向（禁止滚转，只允许左右转+抬头）
            Quaternion targetRot = Quaternion.LookRotation(desiredMoveDir, Vector3.up);
            _tf.rotation = Quaternion.RotateTowards(
                _tf.rotation,
                targetRot,
                turnSpeedDeg * dt
            );
        }
        TryAutoFire(toTarget, dist);

    }

    /// <summary>
    /// 计算在攻击距离附近时的“绕圈移动方向”
    /// - 在水平面上绕圈
    /// - 距离略偏大/略偏小时，稍微往里/往外修正
    /// </summary>
    Vector3 ComputeOrbitDirection(Vector3 toTarget, float dist)
    {
        // 1）只在水平面考虑“朝向玩家”的方向（避免飞到头顶）
        Vector3 flatToTarget = Vector3.ProjectOnPlane(toTarget, Vector3.up);
        if (flatToTarget.sqrMagnitude < 0.001f)
        {
            // 极端情况：玩家正好在正上/正下方，就先不乱动
            return _tf.forward;
        }
        Vector3 radialDir = flatToTarget.normalized; // 水平面指向玩家

        // 2）计算绕圈的切线方向：与半径垂直
        Vector3 tangent;
        if (orbitClockwise)
        {
            // 顺时针（从上往下看）
            tangent = Vector3.Cross(Vector3.up, radialDir);
        }
        else
        {
            // 逆时针
            tangent = Vector3.Cross(radialDir, Vector3.up);
        }
        tangent.Normalize();

        // 3）距离偏大/偏小时，稍微往里/往外修一点
        float minOrbit = orbitDistance - orbitDistanceTolerance;
        float maxOrbit = orbitDistance + orbitDistanceTolerance;

        Vector3 radialAdjust = Vector3.zero;
        if (dist > maxOrbit)
        {
            // 略远 → 稍微往里飞一点
            radialAdjust = radialDir * radialAdjustFactor;
        }
        else if (dist < minOrbit)
        {
            // 略近 → 稍微往外飞一点
            radialAdjust = -radialDir * radialAdjustFactor;
        }

        // 4）合成最终移动方向：切线为主，半径修正为辅
        Vector3 moveDir = tangent + radialAdjust;
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            moveDir = tangent;
        }

        // 仍只保持在水平面（XZ）上运动
        moveDir = Vector3.ProjectOnPlane(moveDir, Vector3.up);
        return moveDir.normalized;
    }
    /// <summary>
    /// 自动开火：间隔 + 角度限制
    /// </summary>
    void TryAutoFire(Vector3 toTarget,float dist)
    {
        if(!weapon) return;
        if(currentState==AIState.Idle) return;
        if(dist>detectRadius) return;

        Vector3 dirToTarget = toTarget.sqrMagnitude > 0.0001f
            ?toTarget.normalized
            : _tf.forward;

        float angle = Vector3.Angle(_tf.forward, dirToTarget);
        if(angle>maxFireAngle) return;
        if(fireInterval > 0f&&_fireTimer<fireInterval)return;
        weapon.TryFire();
        _fireTimer = 0f;
    }
}
