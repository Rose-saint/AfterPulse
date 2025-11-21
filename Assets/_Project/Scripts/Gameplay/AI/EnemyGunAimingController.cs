using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 敌人炮塔瞄准：
/// - yawPivot 只绕世界 Y 轴旋转
/// - pitchPivot 在本地 X 轴俯仰
/// - 自动对准 target（一般是玩家）
///
/// 不涉及相机和鼠标，只是“朝向玩家”的自动版 GunAiming。
/// </summary>
[DisallowMultipleComponent]
public class EnemyGunAimingController : MonoBehaviour
{
    [Header("Pivot")]
    public Transform yawPiovt;
    public Transform pitchPiovt;
    public Transform muzzle;
    [Header("目标")]
    public Transform target;
    [Header("转速与角度限制")]
    public float yawSpeedDegPerSec = 120f;
    public float pitchSpeedDegPerSec = 90f;
    public float minPitchDeg = -10;
    public float maxPitchDeg = 60;
    [Tooltip("当模型前方是-z就设为-1；是+z，就用+1")]
    public int forwardsign = +1;

    // Start is called before the first frame update
    void Awake()
    {
        if (target==null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player!=null)
                target = player.transform;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target == null || yawPiovt == null || pitchPiovt == null) return;
        Vector3 toTarget= target.position-yawPiovt.position;
        if(toTarget.sqrMagnitude<0.001f)return;
        //先算水平面旋转
        Vector3 flatDir = Vector3.ProjectOnPlane(toTarget,Vector3.up);
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredYaw = Quaternion.LookRotation(flatDir * forwardsign, Vector3.up);
            yawPiovt.rotation = Quaternion.RotateTowards(
                yawPiovt.rotation,
                desiredYaw,
                yawSpeedDegPerSec*Time.deltaTime
                );
        }
        //再算pitch的抬头
        Vector3 worldAimDir =(target.position-pitchPiovt.position).normalized;
        Vector3 localDir=yawPiovt.InverseTransformDirection(worldAimDir);
        float targetPitch = Mathf.Asin(Mathf.Clamp(localDir.y,-1f,1f))*Mathf.Rad2Deg;
        targetPitch=Mathf.Clamp(targetPitch,minPitchDeg,maxPitchDeg);
        Vector3 localAngles=pitchPiovt.localEulerAngles;
        float currentPitch = NormalizeAngle(localAngles.x);
        float newPitch=Mathf.MoveTowards(currentPitch,targetPitch,pitchSpeedDegPerSec*Time.deltaTime);
        localAngles.x = newPitch;
        pitchPiovt.localEulerAngles = localAngles;
    }
    float NormalizeAngle(float angle)
    {
        angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
        return angle;
    }
}
