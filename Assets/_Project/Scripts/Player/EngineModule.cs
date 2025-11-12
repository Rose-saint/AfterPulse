using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineModule
{
    public EngineDef def {  get; private set; }

    public EngineModule(EngineDef engineDef)
    {
        def = engineDef; 
    }

    public void ApplyTo(ShipControllerBasic ctrl)
    {
        if (def==null||ctrl==null) return;
        ctrl.maxGear = def.maxGear;
        ctrl.speedPerGear = def.speedPerGear;
        ctrl.boostMul=def.boostMultiplier;
        ctrl.accel=def.accel;
        ctrl.brakeAccel=def.brakeAccel;
        ctrl.pitchSpeed=def.pitchSpeed;
        ctrl.rollSpeed=def.rollSpeed;
        ctrl.yawSpeed=def.yawSpeed;
    }
}
