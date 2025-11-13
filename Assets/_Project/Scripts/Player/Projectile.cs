using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 极简弹丸：直线飞行 + 射线命中检测 + 伤害传递
/// 使用方式：作为预制体，至少带一个小的可视模型（可选加Trail）
///
/// 技术点：
/// - 不用刚体，自己移动，避免物理设置出错
/// - 每帧用 Raycast 检测从上帧到本帧之间是否命中（防穿透）
/// - 命中后在碰撞物体或其父节点上寻找 ShipStats 扣血
/// </summary>

[DisallowMultipleComponent]
public class Projectile : MonoBehaviour
{
    [Header("Basic")]
    public float speed = 200f;
    public float damage = 10f;
    public float lifetime = 5f;
    public float radius = 0.05f;
    public float skinBack = 0.05f;


    [Header("Hit")]
    public LayerMask hitMask = ~0;
    public bool destroyOnHit = true;

    Transform _tf;
    Vector3 _lastPos;
    GameObject _owner;
    float _lifeTimer = 0f;
    public void SetInitialParams(float speed,float damage,GameObject owner)
    {
        this.speed=Mathf.Max(0f,speed);
        this.damage=Mathf.Max(0f,damage);
        this._owner = owner;
    }
    // Start is called before the first frame update
    void Awake()
    {
        _tf = transform;
        _lastPos = _tf.position - _tf.forward * skinBack;
    }

void Start()
    {
        // 把“出生重叠检查”放到 Start（此时 _owner 已经通过 SetInitialParams 赋值）
        var overlapped = Physics.OverlapSphere(
            _tf.position,
            Mathf.Max(radius, 0.02f),
            hitMask,
            QueryTriggerInteraction.Ignore
        );

        if (overlapped != null && overlapped.Length > 0)
        {
            foreach (var c in overlapped)
            {
                // 忽略自己及子节点
                if (_owner && (c.gameObject == _owner || c.transform.IsChildOf(_owner.transform)))
                    continue;

                Vector3 hitPoint = c.ClosestPoint(_tf.position);
                Vector3 hitNormal = (_tf.position - hitPoint).normalized;

                // 直接处理命中（不构造只读 RaycastHit）
                FakeHit(c, hitPoint);

                if (destroyOnHit)
                {
                    Destroy(gameObject);
                    return;
                }
                break;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        float dt=Time.deltaTime;
        _lifeTimer += dt;
        if (_lifeTimer > lifetime)
        {
            Destroy(gameObject);
            return;
        }
        //计算本帧移动
        Vector3 newPos = _tf.position+_tf.forward*speed*dt;
        //从上帧位置到本帧为止做射线检测
        Vector3 seg = newPos - _lastPos;
        float dist=seg.magnitude;
        if (dist > 0f)
        {
            Vector3 dir = seg/dist;

            RaycastHit hit;
            bool hasHit = (radius > 0f)
                ? Physics.SphereCast(_lastPos, radius, dir, out hit, dist, hitMask, QueryTriggerInteraction.Ignore)
                : Physics.Raycast(_lastPos, dir, out hit, dist, hitMask, QueryTriggerInteraction.Ignore);


            if (hasHit)
            {
                //正确：命中“不是自己”才处理；命中自己/子节点则忽略
                bool hitSelf = _owner && (hit.collider.gameObject == _owner ||
                                          hit.collider.transform.IsChildOf(_owner.transform));
                if (!hitSelf)
                {
                    OnHit(hit);
                    if (destroyOnHit)
                    {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
        }
        _tf.position = newPos;  
        _lastPos=_tf.position;
    }
    void OnHit(RaycastHit hit)
    {
        ShipStats stats = hit.collider.GetComponent<ShipStats>();
        if(!stats) stats = hit.collider.GetComponentInParent<ShipStats>();

        if (stats)
        {
            stats.TakeDamage(damage,hit.point);
        }
        //可选命中特效/音效
    }
    void FakeHit(Collider col, Vector3 point)
    {
        ShipStats stats = col.GetComponent<ShipStats>();
        if (!stats) stats = col.GetComponentInParent<ShipStats>();
        if (stats)
        {
            stats.TakeDamage(damage, point);
        }
        // 可以在这里生成命中特效/音效
    }
}
