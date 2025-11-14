using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   // 使用 TextMeshProUGUI

/// <summary>
/// 调试用 HUD，在屏幕上显示飞船的各种运行数据。
/// 只用于开发阶段，后期可以禁用或删除。
/// </summary>
public class ShipDebugHUD : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("用于显示调试文字的 TextMeshProUGUI")]
    public TextMeshProUGUI textUI;

    [Header("目标飞船（不填则自动找挂在同一物体或父物体上的组件）")]
    public ShipControllerBasic controller;
    public ShipEnergySystem energySystem;
    public ShipStats shipStats;
    public ShipEquipment shipEquipment;
    public Rigidbody shipRigidbody;
    public WeaponFireController weaponController;


    [Header("调试设置")]
    [Tooltip("是否一开始就显示调试信息")]
    public bool startVisible = true;
    [Tooltip("切换显示/隐藏的按键")]
    public KeyCode toggleKey = KeyCode.F3;

    bool _visible;

    void Awake()
    {
        _visible = startVisible;

        // 自动寻找引用（如果没手动拖）
        if (!weaponController) weaponController = FindObjectOfType<WeaponFireController>();
        if (!controller) controller = FindObjectOfType<ShipControllerBasic>();
        if (!energySystem && controller) energySystem = controller.GetComponent<ShipEnergySystem>();
        if (!shipStats && controller) shipStats = controller.GetComponent<ShipStats>();
        if (!shipEquipment && controller) shipEquipment = controller.GetComponent<ShipEquipment>();
        if (!shipRigidbody && controller) shipRigidbody = controller.GetComponent<Rigidbody>();

        if (!textUI)
        {
            Debug.LogWarning($"{name}: ShipDebugHUD 没有关联 TextMeshProUGUI，记得在 Canvas 里拖一个过来。");
        }
        else
        {
            textUI.enableWordWrapping = false;
            textUI.alignment=TMPro.TextAlignmentOptions.TopLeft;
        }
    }

    void Update()
    {
        // 切换显示 / 隐藏
        if (Input.GetKeyDown(toggleKey))
        {
            _visible = !_visible;
            if (textUI) textUI.gameObject.SetActive(_visible);
        }

        if (!_visible || !textUI) return;

        textUI.text = BuildDebugText();
    }

    string BuildDebugText()
    {
        // 用简单的字符串拼接就行，调试 UI 不在乎 GC
        string s = "";

        // 1. 基础运动 / 控制状态
        if (controller)
        {
            s += $"[Ship]\n";
            s += $"  Gear: {controller.CurrentGear}\n";
            s += $"  Boost: {(controller.IsBoosting ? "ON" : "OFF")}\n";
        }

        if (shipRigidbody)
        {
            float speed = shipRigidbody.velocity.magnitude;
            s += $"  Speed: {speed:F1} m/s\n";
        }

        // 2. 舰船生命值
        if (shipStats)
        {
            s += $"\n[Hull]\n";
            s += $"  HP: {shipStats.currentHP:F0} / {shipStats.maxHP:F0}\n";
        }

        // 3. 能量 / 负载系统
        if (energySystem)
        {
            float total = energySystem.TotalLoad;
            float pe = energySystem.LoadPercent * 100f;

            s += $"\n[Energy / Load]\n";
            s += $"  Max Load: {energySystem.maxLoad:F1}\n";
            s += $"  Engine Load: {energySystem.engineLoad:F1}\n";
            s += $"  Weapon Load: {energySystem.weaponLoad:F1}\n";
            s += $"  System Load: {energySystem.systemLoad:F1}\n";
            s += $"  Total Load: {total:F1}  ({pe:F0}%)\n";
            s += $"  Overload: {(energySystem.IsOverload ? "YES" : "NO")}\n";
            s += $"  PowerScale: {energySystem.PowerScale:F2}\n";
        }
        // 4. 实时射速（受过载影响）
        if (weaponController)
        {
            s += $"\n[Runtime]\n";
            s += $"  FireRate(Current): {weaponController.currentFireRate:F2} shots/s\n";
        }

        // 5. 武器信息
        if (shipEquipment)
        {
            var w = shipEquipment.MainWeapon;
            s += $"\n[Weapon]\n";
            if (w)
            {
                s += $"  Name: {w.name}\n";
                s += $"  Type: {w.type}\n";
                s += $"  Damage: {w.damage:F1}\n";
                s += $"  FireRate: {w.fireRate:F1} shots/s\n";
                s += $"  ProjectileSpeed: {w.projectileSpeed:F1}\n";
                s += $"  Load per Shot: {w.energyCostPerShot:F1}\n";
            }
            else
            {
                s += $"  (No WeaponDef in ShipEquipment.MainWeapon)\n";
            }
        }
        

        return s;
    }
}
