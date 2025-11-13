using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/// <summary>
/// 简易伤害飘字：世界空间上浮+淡出，自动销毁
/// 用法：做成预制体，挂 TextMeshPro（3D 版：TextMeshPro）或 TextMeshPro - Text (UI)+World Space Canvas 都行
/// </summary>
[DisallowMultipleComponent]
public class DamagePopup : MonoBehaviour
{
    public float riseSpeed = 1.2f;
    public float lifetime = 0.8f;
    public Vector3 randomXZ = new Vector3 (0.2f, 0, 0.2f);
    public float fadeStart = 0.4f;
    public string numberFormat = "F0";

    TextMeshPro _tmp;
    Transform _tf;
    Color _baseColor;
    float _timer;
    float _amount=0f;

    public void SetNumber(float amount)
    {
        _amount = amount;
        if (_tmp) _tmp.text = amount.ToString(numberFormat);
    }
    // Start is called before the first frame update
    void Awake()
    {
        _tf = transform;
        _tmp = GetComponent<TextMeshPro>();
        if (!_tmp)
        {
            Debug.LogWarning($"{name}: DamagePopup 需要 TextMeshPro 组件（3D 版）。");
            enabled = false;
            return;
        }
        _baseColor = _tmp.color;
        //初始随机漂移
        Vector3 off = new Vector3 (
            Random.Range(-randomXZ.x,randomXZ.x),
            0f,
            Random.Range(-randomXZ.z,randomXZ.z)
            );
        _tf.position += off;
    }

void Start()
    {
        if (_tmp&&string.IsNullOrEmpty(_tmp.text))_tmp.text = "10";
    }
    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        _timer += dt;
        if (Camera.main)
        {
            _tf.rotation= Quaternion.LookRotation(_tf.position-Camera.main.transform.position);
        }
        _tf.position += Vector3.up * riseSpeed * dt;
        if (_timer >= fadeStart)
        {
            float t = Mathf.InverseLerp(fadeStart,lifetime,_timer);
            Color c = _baseColor;
            c.a = Mathf.Lerp(1f,0f,t);
            _tmp.color = c;
        }
        if (_timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
