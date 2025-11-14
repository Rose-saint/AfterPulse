using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 舰船能量 / 负载系统（简化版）
/// - 不再用“能量 - 消耗”的电池模型；
/// - 使用“最大负载 + 当前负载”的模型：
///   * 引擎负载：档位 / 加力 持续叠加
///   * 武器负载：每次开火瞬间叠加，然后缓慢冷却
///   * 其他系统负载：以后可以加雷达 / 护盾等
/// - 当总负载 >= 最大负载时进入过载，功率下降一段时间。
/// </summary>
[DisallowMultipleComponent]
public class ShipEnergySystem : MonoBehaviour
{
    [Header("基础负载容量")]
    [Tooltip("没有引擎时的默认最大负载")]
    public float baseMaxLoad = 100f;
    [Header("冷却速度")]
    [Tooltip("引擎【过热负载】每秒自然减少多少（不影响固定档位负载）")]
    public float engineLoadCoolRate = 5f;
    [Tooltip("武器负载每秒自然减少多少")]
    public float weaponLoadCoolRate = 4f;
    [Tooltip("其他系统负载每秒自然减少多少")]
    public float systemLoadCoolRate = 3f;
    [Header("过载设置")]
    [Tooltip("过载状态下的功率缩放（0~1），例如 0.6 表示只保留 60% 性能")]
    public float overloadPowerScale = 0.6f;
    [Tooltip("进入过载后持续多久（秒）")]
    public float overloadCooldown = 5.0f;
    [Tooltip("达到这个百分比开始警告（仅给 UI 使用，可选）")]
    public float warningPercent = 0.8f;

    //最大负载
    public float maxLoad;
    [Header("调试观察")]
    public float engineBaseLoad;
    public float engineLoad;
    public float weaponLoad;
    public float systemLoad;
    //总负载与百分比
    public float TotalLoad => Mathf.Max(0f, engineBaseLoad+engineLoad + weaponLoad + systemLoad);
    public float LoadPercent => maxLoad <= 0f ? 0f : Mathf.Clamp01(TotalLoad / maxLoad);
    //是否处于过载状态
    public bool IsOverload => _overTimer > 0f;
    public float PowerScale => IsOverload ? overloadPowerScale : 1f;

    float _overTimer;
    ShipEquipment _equip;

    public void ApplyEngineEnergySpecs()
    {
        var eng = _equip?.Engine?.def;
        if (eng != null && eng.energyCapacity > 0f)
        {
            maxLoad = eng.energyCapacity;
        }
        else
        {
            maxLoad = Mathf.Max(1f, baseMaxLoad);
        }

        engineBaseLoad = 0f;
        engineLoad = 0f;
        weaponLoad = 0f;
        systemLoad = 0f;
    }
    public void SetEngineBaseLoad(float baseLoad)
    {
        engineBaseLoad = Mathf.Clamp(baseLoad, 0f, Mathf.Max(1f,maxLoad));
        CheckOverload();
    }
    public void AddEngineLoad(float amountThisFrame)
    {
        if(amountThisFrame <= 0f) return;
        if (IsOverload) return;

        engineLoad += amountThisFrame;
        ClampLoads();
        CheckOverload();
    }
    public void AddWeaponLoad(float amount)
    {
        if (amount <= 0f) return;
        if(IsOverload)return;
        weaponLoad+= amount;
        ClampLoads();
        CheckOverload();
    }
    public void AddSystemLoad(float amount)
    {
        if (amount <= 0f) return;
        if (IsOverload) return;
        systemLoad += amount;
        ClampLoads();
        CheckOverload();
    }
    void Awake()
    {
        _equip =GetComponent<ShipEquipment>();
        ApplyEngineEnergySpecs();
    }
    void Update()
    {
        float dt = Time.deltaTime;
        if (IsOverload)
        {
            _overTimer-=dt;
            if(_overTimer <= 0f)
            {
                _overTimer = 0f;
            }
        }
        coolDownLoads(dt);
        ClampLoads();
    }
    void coolDownLoads(float dt)
    {
        if(dt<= 0f) return;
        float engineCool = engineLoadCoolRate;
        float weaponCool = weaponLoadCoolRate;
        float systemCool = systemLoadCoolRate;
        if (IsOverload)
        {
            engineCool *= 1.5f;
            weaponCool *= 1.5f;
            systemCool *= 1.5f;
        }
        engineLoad -= engineCool * dt;
        weaponLoad -= weaponCool * dt;
        systemLoad -= systemCool * dt;
    }
    void ClampLoads()
    {
        float max = Mathf.Max(1f, maxLoad);
        engineLoad = Mathf.Clamp(engineLoad, 0f, max);
        weaponLoad = Mathf.Clamp(weaponLoad, 0f, max);
        systemLoad = Mathf.Clamp(systemLoad, 0f, max);
    }
    void CheckOverload()
    {
        if (IsOverload) return;
        if (TotalLoad >= maxLoad)
        {
            _overTimer = overloadCooldown;
        }
    }
}
/*[DisallowMultipleComponent]
public class ShipEnergySystem : MonoBehaviour
{
    [Header("Overload")]
    public float overloadPowerScale = 0.6f;
    public float overloadCooldown = 2.0f;
    [HideInInspector] public float energyMax;
    [HideInInspector] public float energy;
    public bool IsOverload => _overTimer > 0f;
    public float PowerScale => IsOverload ? overloadPowerScale : 1f;
    float _overTimer;
    ShipEquipment _equip;

    public void ApplyEngineEnergySpecs() 
    {
        var eng = _equip?.Engine?.def;//?什么意思？
        if (eng != null)
        {
            energyMax = Mathf.Max(1f, eng.energyCapacity);
            energy = energyMax;
        }
        else
        {
            energyMax = 100f;
            energy = energyMax;
        }
    }
    public bool TryConsume(float amountThisFrame)
    {
        if (amountThisFrame <= 0f) return true;
        if (IsOverload) return false;
        if (energy >= amountThisFrame)
        {
            energy -= amountThisFrame;
            return true;
        }
        else
        {
            //能量不足，进入过载
            _overTimer = overloadCooldown;
            energy = 0f;
            return false;
        }
    }
    public void Recharge(float amount)
    {
        energy = Mathf.Min(energyMax,energy+amount);
    }

    void Awake()
    {
        _equip = GetComponent<ShipEquipment>();
        ApplyEngineEnergySpecs();
    }
    void Update()
    {
        if (IsOverload)
        {
            _overTimer -= Time.deltaTime;
            if (_overTimer <= 0f) _overTimer = 0f;
        }
    }

}
*/