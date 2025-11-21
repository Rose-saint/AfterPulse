using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipControllerBasic : MonoBehaviour
{
    //档位和速度
    public int maxGear = 5;
    public float speedPerGear = 12f;
    public float boostMul = 1.6f;
    //加减速
    public float accel = 25f;
    public float brakeAccel = 40f;
    //转向
    public float pitchSpeed = 70f;
    public float rollSpeed = 120f;
    public float yawSpeed = 100f;
    //手感阻尼
    public float linearDrag = 0.25f;
    public float angularDrag = 0.35f;

    [HideInInspector] public float powerScale = 1f;

    private Rigidbody rb;
    private int gear = 0;
    private Vector3 targetVel;

    //外部只读
    public int CurrentGear=>gear;
    public bool IsBoosting {  get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        rb= GetComponent<Rigidbody>();
        rb.useGravity=false;
        rb.drag=linearDrag;
        rb.angularDrag=angularDrag;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //切换档位
        if (Input.GetKeyDown(KeyCode.LeftControl)) gear=Mathf.Max(0, gear-1);
        if (Input.GetKeyDown(KeyCode.LeftShift)) gear=Mathf.Min(maxGear, gear+1);
        //转向
        float pitch = 0f;
        if (Input.GetKey(KeyCode.W)) pitch = 1f;
        if (Input.GetKey(KeyCode.S)) pitch = -1f;
        float roll = 0f;
        if(Input.GetKey(KeyCode.Q)) roll = 1f;
        if(Input.GetKey(KeyCode.E)) roll = -1f;
        float yaw = 0f;
        if (Input.GetKey(KeyCode.A)) yaw = -1f;
        if (Input.GetKey(KeyCode.D)) yaw = 1f;
        float dt=Time.deltaTime;
        transform.Rotate(pitch*pitchSpeed*dt,yaw*yawSpeed*dt,roll*rollSpeed*dt,Space.Self);
        //目标速度
        IsBoosting = Input.GetKey(KeyCode.Space);
        float speed = gear * speedPerGear;
        if (IsBoosting) speed *=boostMul;
        speed *= Mathf.Clamp01(powerScale);
        targetVel = transform.forward*speed;
    }
    void FixedUpdate()
    {
        //速度逼近的方式前进
        Vector3 dv = targetVel-rb.velocity;
        //判断加减速，微调手感
        bool needBrake = Vector3.Dot(rb.velocity,targetVel)>0f && targetVel.magnitude <rb.velocity.magnitude;
        float maxStep = (needBrake ? brakeAccel : accel)*Time.fixedDeltaTime;
        Vector3 add = Vector3.ClampMagnitude(dv,maxStep);

        rb.AddForce(add,ForceMode.VelocityChange);
    }
}
