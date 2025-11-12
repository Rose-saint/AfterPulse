using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipStats : MonoBehaviour
{
    [Header("Hull")]
    public float maxHP = 100f;
    public float currentHP = 100f;
    // Start is called before the first frame update
    void Awake()
    {
        currentHP = Mathf.Clamp(currentHP,0, maxHP);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
