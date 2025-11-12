using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName ="Game/Defs/EngineDef",fileName ="Engine_Default")]

public class EngineDef : ScriptableObject
{
    [Header("Speed&Gears")]
    public int maxGear = 5;
    public float speedPerGear = 12f;
    public float boostMultiplier = 1.6f;
    [Header("Acceleration")]
    public float accel = 25f;
    public float brakeAccel = 40f;
    [Header("Angular Speeds")]
    public float pitchSpeed = 40f;
    public float rollSpeed = 60f;
    public float yawSpeed = 40f;
    [Header("Energy")]
    public float cruiseEnergyPerSec = 2f;
    public float boostEnergyPerSec = 6f;
    public float energyCapacity = 100f;
}
