using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Game/Defs/WeaponDef", fileName = "Weapon_MainGun_Default")]
public class WeaponDef : ScriptableObject
{
    public enum WeaponType { Railgun,Laser,Missile}
    public WeaponType type = WeaponType.Railgun;
    [Header("Basic Stats(not used yet)")]
    public float damage = 10f;
    public float fireRate = 6f;
    public float projectileSpeed = 200f;
    public float energyCostPerShot = 5f;
}
