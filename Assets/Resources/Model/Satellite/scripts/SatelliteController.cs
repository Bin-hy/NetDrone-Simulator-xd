using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class SatelliteController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("是否自我旋转")]
    public bool isSpin = false;

    [SerializeField]
    [Tooltip("转速")]
    private float paddleSpinSpeed = 1000f; // 更改变量名，避免与移动速度混淆

    [SerializeField]
    [Tooltip("旋转方向(1=顺时针, -1=逆时针)")]
    private int paddleSpinDirection = 1;

    Transform paddleRotation;
    private void Awake()
    {
        paddleRotation = gameObject.transform;
    }

    void Update()
    {
        // 桨叶旋转逻辑
        if (isSpin)
        {
            float rotationAmount = paddleSpinSpeed * paddleSpinDirection * Time.deltaTime;
            paddleRotation.Rotate(0, rotationAmount, 0, Space.Self);

        }

    }

}
