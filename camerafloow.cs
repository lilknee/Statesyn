using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // �����Ŀ�� (��ɫ)
    public Vector3 offset;        // ��������ɫ֮���ƫ����
    public float smoothSpeed = 0.125f; // ƽ��������ٶ�
    public float movementThreshold = 0.1f; // ����ɫ�ƶ�����С�ڸ�ֵʱ����������ƶ�

    void Start()
    {
        
        // �������������ת�ǶȲ��䣬���ٿ���Ŀ��

    }


    void LateUpdate()
    {
        // �����������Ŀ��λ��
        
    }
}
