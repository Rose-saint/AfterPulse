using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 炮塔瞄准控制：YawPivot 水平、PitchPivot 俯仰；基于当前有效相机（主相机或Aim相机）
/// 将炮口(Muzzle)朝向鼠标指向点。保证与现有发射脚本解耦。
/// </summary>

public class GunAimingController : MonoBehaviour
{
    [Header("Pivot引用")]
    public Transform yawPivot;
    public Transform pitchPivot;
    public Transform muzzle;

    [Header("相机")]
    public Camera mainCamera;
    public Camera aimCamera;
    public bool holdToAim = true;

    [Header("瞄准参数")]
    public LayerMask aimMask = ~0;
    public float maxAimDistance = 2000f;
    public float yawSpeedDegPerSec = 360f;
    public float pitchSpeedDegPerSec = 270f;
    public float minPitchDeg = -10f;
    public float maxPitchDeg = 60f;
    [Header("FOV过渡")]
    public bool smoothFov=true;
    public float fovLerpSpeed = 8f;
    [Header("方向修正")]
    public bool invertPitch = true;
    [Tooltip("如果炮管前向是-z，就把它设为-1")]
    public int forwardSign = +1;
    [Header("瞄准射线策略")]
    public bool useMuzzleRayWhenAiming = true;  // 开镜时改用炮口射线
    public float minValidHitDist = 0.3f;        // 过滤离相机/炮口太近的命中
    [Header("ADS 鼠标位移模式")]
    public bool useMouseLookWhenAiming = true;  // 开镜时启用鼠标位移控制
    public float normalSensitivity = 1.0f;
    public float aimSensitivity = 0.5f;
    public bool invertY = false;     // 需要反转抬压就开

    // 内部缓存：当前灵敏度
    float CurrentSensitivity => isAiming ? aimSensitivity : normalSensitivity;

    //内部
    private bool isAiming;
    private float targetMainFov;
    private float targetAimFov;
    private void HandleAimToggle()
    {
        if (holdToAim)
        {
            isAiming = Input.GetMouseButton(1);
        }
        else
        {
            if (Input.GetMouseButtonDown(1)) isAiming = !isAiming;
        }
        if (aimCamera == null || mainCamera == null) return;

        aimCamera.enabled = isAiming;
        mainCamera.enabled = !isAiming;
        var cm = FindObjectOfType<CursorManager>();
        if (cm != null)
        {
            if (isAiming)
                cm.Apply(CursorLockMode.Locked, false);   // 开镜锁定并隐藏
            else
                cm.Apply(CursorLockMode.Confined, true);  // 退镜解锁并可见（或 None）
        }
        if (useMouseLookWhenAiming && isAiming)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Confined; // 或 None
            Cursor.visible = true;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isAiming = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private Camera GetActiveCamera()
    {
        if(isAiming&&aimCamera!=null)return aimCamera;
        return mainCamera != null?mainCamera:Camera.main;
    }

    private Vector3 ComputeAimPoint(Camera cam)
    {
        // 选用哪根射线：开镜且启用选项 → 用炮口射线；否则用相机射线
        Vector3 rayOrigin, rayDir;

        if (isAiming && useMuzzleRayWhenAiming && muzzle != null)
        {
            rayOrigin = muzzle.position;
            rayDir = (cam != null ? cam.transform.forward : muzzle.forward);
        }
        else
        {
            if (cam == null)
                return (muzzle ? muzzle.position : transform.position) + transform.forward * maxAimDistance;
            Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
            rayOrigin = camRay.origin;
            rayDir = camRay.direction;
        }

        // 拿所有命中，滤掉“太近”和“命中自己root”的，再取最近有效命中
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, rayDir, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore);
        if (hits != null && hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            Transform selfRoot = yawPivot ? yawPivot.root : transform.root;

            foreach (var h in hits)
            {
                if (h.distance < minValidHitDist) continue;              // 太近（通常是自身附近），忽略
                if (selfRoot != null && h.transform.root == selfRoot) continue; // 命中自己，忽略
                return h.point;
            }
        }

        // 没有有效命中，取远点
        return rayOrigin + rayDir * maxAimDistance;
    }


    private void ApplyYawPitch(Vector3 aimPoint,Camera activeCam)
    {
        if (yawPivot == null || pitchPivot == null) return;
        
        //计算水平朝向
        Vector3 fromYaw = yawPivot.position;
        Vector3 dirToAim = (aimPoint - fromYaw);
        Vector3 flatDir = Vector3.ProjectOnPlane(dirToAim, yawPivot.up);
        if (flatDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetYawRot = Quaternion.LookRotation(flatDir.normalized, yawPivot.up);
            yawPivot.rotation = Quaternion.RotateTowards(
                yawPivot.rotation, targetYawRot,
                yawSpeedDegPerSec * Time.deltaTime
            );
        }
        //计算俯仰
        Vector3 localDir = yawPivot.InverseTransformDirection(aimPoint - pitchPivot.position);
        localDir.z *= forwardSign;

        float rawPitch = Mathf.Atan2(localDir.y,Mathf.Max(0.0001f,localDir.z))*Mathf.Rad2Deg;
        if(invertPitch) rawPitch=-rawPitch;
        float clampedPitch=Mathf.Clamp(rawPitch,minPitchDeg,maxPitchDeg);

        Quaternion currentLocal=pitchPivot.localRotation;
        Quaternion targetLocal=Quaternion.Euler(clampedPitch,0f,0f);
        pitchPivot.localRotation = Quaternion.RotateTowards(
            currentLocal,
            targetLocal,
            pitchSpeedDegPerSec * Time.deltaTime
            );
    }
    
    private void SmoothFovIfNeeded()
    {
        if(!smoothFov) return;
        Camera active =GetActiveCamera();
        if(active==null)return;
        float target = active==aimCamera?targetAimFov:targetMainFov;
        active.fieldOfView=Mathf.Lerp(active.fieldOfView,target,1f-Mathf.Exp(-fovLerpSpeed*Time.deltaTime));
    }


    void Awake()
    {
        if(mainCamera==null)mainCamera = Camera.main;
        if(aimCamera!=null) aimCamera.enabled = false;
        if (mainCamera != null)
        {
            targetMainFov = mainCamera.fieldOfView;
        }
        if (aimCamera != null)
        {
            targetAimFov = aimCamera.fieldOfView;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleAimToggle();
        Camera activeCam=GetActiveCamera();
        if (useMouseLookWhenAiming && isAiming)
        {
            // 1) 用鼠标增量直接旋转炮塔
            float mx = Input.GetAxisRaw("Mouse X") * CurrentSensitivity;
            float my = Input.GetAxisRaw("Mouse Y") * CurrentSensitivity * (invertY ? 1f : -1f);
            ApplyMouseDeltaYawPitch(mx, my);

            // 2) 瞄准点就用中心射线的远点（或做一次中心 Raycast）
            Vector3 aimPoint = activeCam.transform.position + activeCam.transform.forward * maxAimDistance;

            // 3) 用当前朝向微调到 aimPoint（可选，保持一致性）
            ApplyYawPitch(aimPoint, activeCam);
        }
        else
        {
            // 鼠标坐标瞄准（原有流程）
            Vector3 aimPoint = ComputeAimPoint(activeCam);
            ApplyYawPitch(aimPoint, activeCam);
        }
        SmoothFovIfNeeded();
    }
    private void ApplyMouseDeltaYawPitch(float mx, float my)
    {
        if (yawPivot == null || pitchPivot == null) return;

        // 水平：绕自身Up旋转（避免滚转带来的世界Up误差）
        yawPivot.Rotate(yawPivot.up, mx * yawSpeedDegPerSec * Time.deltaTime, Space.World);

        // 俯仰：在本地X轴上累积并限幅
        // 取当前俯仰角（-180~180），再加增量
        float currentPitch = Mathf.DeltaAngle(0f, pitchPivot.localEulerAngles.x);
        float targetPitch = Mathf.Clamp(currentPitch + my * pitchSpeedDegPerSec * Time.deltaTime,
                                         minPitchDeg, maxPitchDeg);
        pitchPivot.localRotation = Quaternion.Euler(targetPitch, 0f, 0f);
    }

}
