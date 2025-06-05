using UnityEngine;

/// <summary>
/// 使得 gameobject 跟随 并且朝向用户
/// </summary>
public class FollowAndFacePlayer : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // 玩家的Transform组件
    [SerializeField] private float moveSpeed = 3f;      // 移动速度
    [SerializeField] private float rotationSpeed = 5f; // 旋转速度
    [SerializeField] private float stoppingDistance = 1f; // 停止距离

    private void Update()
    {
        // 检查是否设置了玩家引用
        if (playerTransform == null)
        {
            Debug.LogWarning("Player transform reference is not set!");
            return;
        }

        // 计算与玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 如果距离大于停止距离，则移动和旋转
        if (distanceToPlayer > stoppingDistance)
        {
            // 移动逻辑
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;

            // 旋转逻辑 - 使对象朝向玩家
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}