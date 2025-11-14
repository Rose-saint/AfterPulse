using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 读取当前档位/加力状态 → 计算本帧引擎能耗 → 向 ShipEnergySystem 申请能量；
/// 同时把能量系统的功率缩放写回控制器（powerScale）。
/// </summary>
[RequireComponent(typeof(ShipControllerBasic))]
[DisallowMultipleComponent]
public class EngineEnergyConsumer : MonoBehaviour
{
    public ShipEnergySystem energySystem;
    public ShipEquipment equipment;

    ShipControllerBasic _ctrl;//为什么有下划线？

    void Awake()
    {
        _ctrl = GetComponent<ShipControllerBasic>();
        if(!energySystem) energySystem = GetComponent<ShipEnergySystem>();
        if(!equipment) equipment = GetComponent<ShipEquipment>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!energySystem||equipment?.Engine?.def==null) return;//没有能量系统或引擎就直接返回
        
        float dt=Time.deltaTime;
        var engDef = equipment.Engine.def;

        //档位固定负载
        float baseLoad = 0f;
        //只要档位>0就有巡航耗能
        if (_ctrl.CurrentGear > 0)
        {
            baseLoad = engDef.cruiseEnergyPerSec*_ctrl.CurrentGear;
        }
        energySystem.SetEngineBaseLoad(baseLoad);
        float overheatThisFrame = 0f;
        //开加力额外耗能
        if (_ctrl.IsBoosting)
            overheatThisFrame = engDef.boostEnergyPerSec * dt;
        if (overheatThisFrame > 0f)
        {
            energySystem.AddEngineLoad(overheatThisFrame);
        }
        //功率缩放反馈给控制器
        _ctrl.powerScale = energySystem.PowerScale;
    }
}
