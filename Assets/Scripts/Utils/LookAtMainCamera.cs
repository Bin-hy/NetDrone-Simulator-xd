using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtMainCamera : MonoBehaviour
{
    private Transform mainCameraTransform;
    [SerializeField] private bool isSpinY = false;
    void Start()
    {
        // 找到主摄像机
        mainCameraTransform = Camera.main.transform;
        if (mainCameraTransform == null)
        {
            Debug.LogError("主摄像机未找到！请确保场景中有一个带有 'MainCamera' 标签的摄像机。");
        }
    }
    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // 使血条的 Y 轴始终朝向摄像机的 Y 轴
            // 这里我们只绕Y轴旋转，保持血条的垂直方向不变
            Vector3 lookDirection = mainCameraTransform.position - transform.position;
            if(!isSpinY)
                lookDirection.y = 0; // 忽略Y轴差异，防止倾斜
            transform.rotation = Quaternion.LookRotation(-lookDirection); // 反向旋转，使 正面朝向摄像机
        }
    }
}
