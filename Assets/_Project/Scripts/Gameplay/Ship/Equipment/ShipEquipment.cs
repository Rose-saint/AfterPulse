using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ShipEquipment : MonoBehaviour
{
    [Header("Choose a Loadout")]
    public ShipLoadout loadout;

    public EngineModule Engine {  get; private set; }
    public WeaponDef MainWeapon { get; private set; }

    ShipControllerBasic controller;

    void Awake()
    {
        controller = GetComponent<ShipControllerBasic>();

        if(loadout == null)
        {
            Debug.LogWarning($"{name}:ShipEquipment没有指定Loadout,使用控制器默认参数。");
            return;
        }
        if (loadout.engine != null)
        {
            Engine = new EngineModule(loadout.engine);
            Engine.ApplyTo(controller);
        }
        else
        {
            Debug.LogWarning($"{name}:Loadout未指定EngineDef.");
        }
        if(loadout.mainWeapon != null)
        {
            MainWeapon = loadout.mainWeapon;
        }
    }
}
