using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("非瞄准默认模式")]
    public CursorLockMode defaultLock = CursorLockMode.Confined; // 或 None
    public bool defaultVisible = true;

    void Start()
    {
        Apply(defaultLock, defaultVisible);
    }

    void Update()
    {
        // ESC 立即释放鼠标
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Apply(CursorLockMode.None, true);
        }
    }

    public void Apply(CursorLockMode mode, bool visible)
    {
        Cursor.lockState = mode;
        Cursor.visible = visible;
    }
}
