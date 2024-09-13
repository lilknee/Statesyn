using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // 跟随的目标 (角色)
    public Vector3 offset;        // 摄像机与角色之间的偏移量
    public float smoothSpeed = 0.125f; // 平滑跟随的速度
    public float movementThreshold = 0.1f; // 当角色移动距离小于该值时，摄像机不移动

    void Start()
    {
        
        // 保持摄像机的旋转角度不变，不再看向目标

    }


    void LateUpdate()
    {
        // 计算摄像机的目标位置
        
    }
}
