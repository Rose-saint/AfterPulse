using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 主炮发射控制（Railgun）
/// - 左键点击发射
/// - 读取 ShipEquipment.MainWeapon（fireRate, damage, projectileSpeed, energyCostPerShot）
/// - 扣 ShipEnergySystem 的能量（不足进入过载则不发射）
/// - 生成 Projectile 弹丸并赋初速度
/// - 简单冷却（fireRate）
///
/// 说明：激光/导弹后续再扩展，这里只实现 Railgun 的实体弹丸。
/// </summary>

[DisallowMultipleComponent]
public class WeaponFireController : MonoBehaviour
{
    [Header("References")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public ShipEquipment equipment;
    public ShipEnergySystem energySystem;

    [Header("Input")]
    public KeyCode firekey = KeyCode.Mouse0;

    [Header("Fallback(当武器为空时的兜底)")]
    public float fallbackdamage = 10f;
    public float fallbackFireRate = 6f;
    public float fallbackProjectileSpeed = 200f;
    public float fallbackEnergyCost = 5f;
    [Header("SPawn")]
    public float spawnForwardOffset = 0.25f;

    float _cooldownTimer = 0f;
    // Start is called before the first frame update
    void Awake()
    {
        if(!equipment)equipment = GetComponentInParent<ShipEquipment>();
        if(!energySystem) energySystem = GetComponentInParent<ShipEnergySystem>();
        if (!muzzle)
        {
            Debug.LogWarning($"{name}:WeaponFireController未指定muzzle，请在Inspector拖拽 Muzzle 节点。");
        }
        if (!projectilePrefab)
        {
            Debug.LogWarning($"{name}:WeaponFireController未指定projectilePrefab，请在Inspector中设置。");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(_cooldownTimer > 0f)_cooldownTimer-= Time.deltaTime;
        if (Input.GetKey(firekey)) { TryFire(); }

    }
    void TryFire()
    {
        if (_cooldownTimer>0f)return;
        if(!muzzle||!projectilePrefab)return;
        //读取武器定义
        var def=equipment?equipment.MainWeapon:null;
        float damage =def?def.damage:fallbackdamage;
        float fireRate = def?def.fireRate:fallbackFireRate;
        float speed=def?def.projectileSpeed:fallbackProjectileSpeed;
        float energyCost=def?def.energyCostPerShot:fallbackEnergyCost;
        //Railgun只在能量足够时开火
        if (energySystem && energyCost > 0f)
        {
            bool ok = energySystem.TryConsume(energyCost);
            if (!ok) return;
        }
        bool shooterIsPlayer = CompareTag("Player");
        int layerProj = LayerMask.NameToLayer(shooterIsPlayer ? "Projectile_Player" : "Projectile_Enemy");
        LayerMask hitMask = LayerMask.GetMask(
            shooterIsPlayer ? new string[] { "Enemy", "Environment" }
                            : new string[] { "Player", "Environment" }
                            );
        //生成弹丸
        GameObject go = Instantiate(projectilePrefab,
            muzzle.position+muzzle.forward*spawnForwardOffset,
            muzzle.rotation);
        //设置弹丸的Layer/tag
        if(layerProj>=0) go.layer = layerProj;
        go.tag = shooterIsPlayer ? "Projectile_Player" : "Projectile_Enemy";

        var proj = go.GetComponent<Projectile>();
        if (proj)
        {
            proj.SetInitialParams(
                speed: speed,
                damage: damage,
                owner: this.gameObject
                );
            proj.hitMask = hitMask;
        }

        float cd = (fireRate > 0f) ? (1f / fireRate) : 0.2f;
        _cooldownTimer = cd;


    }

}
