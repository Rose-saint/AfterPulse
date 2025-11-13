using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 标靶：自动设置 Layer/Tag 为 Enemy，并确保有 Collider + ShipStats
/// 放在标靶根物体上即可。
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(ShipStats))]
public class TargetDummy : MonoBehaviour
{
    void Reset()
    {
        gameObject.tag = "Enemy";
        gameObject.layer = LayerMask.NameToLayer("Enemy");

        var col = GetComponent<Collider>();
        col.isTrigger = false;

        var stats = GetComponent<ShipStats>();
        stats.maxHP = 200f;
        stats.currentHP = stats.maxHP;
        stats.destroyOnDeath = false; // 被打空后自动回满，方便测试
    }

    void Awake()
    {
        if (gameObject.layer == 0) gameObject.layer = LayerMask.NameToLayer("Enemy");
        if (!CompareTag("Enemy")) gameObject.tag = "Enemy";
    }
}

