using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
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
