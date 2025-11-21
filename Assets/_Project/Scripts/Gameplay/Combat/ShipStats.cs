using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShipStats : MonoBehaviour
{
    [Header("Hull")]
    public float maxHP = 100f;
    public float currentHP = 100f;
    [Header("DamagePopup")]
    public GameObject damagePopupPrefab;
    public bool destroyOnDeath = true;
    // Start is called before the first frame update
    void Awake()
    {
        currentHP = Mathf.Clamp(currentHP,0, maxHP);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TakeDamage(float amount)
    {
        TakeDamage(amount,transform.position);
       /* if(amount<=0f)return;
        currentHP=Mathf.Max(0f,currentHP-amount);
        if(currentHP <= 0f)
        {
            Destroy(gameObject);
        }*/
    }
    public void TakeDamage(float amount,Vector3 worldHitPoint)
    {
        if (amount <= 0f) return;
        currentHP = Mathf.Max(0f, currentHP - amount);
        //伤害字体显示
        if (damagePopupPrefab)
        {
            Vector3 camDir = Camera.main ? (Camera.main.transform.forward) : Vector3.up;
            Vector3 spawnPos = worldHitPoint + camDir * 0.05f;   // 0.03~0.1f 自己试

            var go = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            var popup = go.GetComponent<DamagePopup>();
            if (popup) popup.SetNumber(amount);
        }
        if (currentHP <= 0f)
        {
            /*if (destroyOnDeath)
            {
                //TODO：爆炸特效和音效
                Destroy(gameObject);
            }
            else 
            {
                StartCoroutine(ResetHPAfter(1.0f));
            }*/
            StartCoroutine(ResetHPAfter(1.0f));
        }
    }
    System.Collections.IEnumerator ResetHPAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentHP = maxHP;
    }
}
