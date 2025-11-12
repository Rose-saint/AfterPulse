using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="Game/Defs/ShipLoadout",fileName ="Loadout_Default")]
public class ShipLoadout : ScriptableObject
{
    public EngineDef engine;
    public WeaponDef mainWeapon;
}
