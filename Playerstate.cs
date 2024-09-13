using UnityEngine;
using System;

[Serializable]
public class PlayerState : MonoBehaviour
{
    public string playerNAME;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;

    // ������Ϣ
    public float rigidbodyPosX, rigidbodyPosY, rigidbodyPosZ;
    public float rigidbodyRotX, rigidbodyRotY, rigidbodyRotZ, rigidbodyRotW;

    private Rigidbody playerRigidbody;
    private Vector3 targetPosition;
    public float moveSpeed = 10.0f; // �����ƶ��ٶ�
    public Quaternion lookRotation;
    public Vector3 moveDirection;
    public float cameraSmoothSpeed = 2.0f; // �����������ƽ���ƶ��ٶ�
    public Vector3 cameraOffset = new Vector3(0f, 12.5f, 0f); // ����������ƫ��
    public Transform cameraTransform; // �����Transform����
    private Animator animator;


    private void Awake()
    {
        // ��ȡ�������
        playerRigidbody = GetComponent<Rigidbody>();
        targetPosition = transform.position; // ��ʼ��Ϊ��ǰ��λ��
        animator = GetComponent<Animator>();//��ȡ������

    }
    void Start()
    {
        SerializePlayerState();
    }
    //��дһ�����͵�ǰλ�÷��������ڳ�ʼ��
    public string SerializePlayerState()
    {
        // ��ȡ��ǰ��ҵ�λ�ú���ת״̬
        getsyncstates();

        string objectName = gameObject.name;

        // �����������뵱ǰλ�ú���ת��Ϣ�ϲ�Ϊһ����Ϣ
        string message = $"{objectName}:{posX},{posY},{posZ},{rotX},{rotY},{rotZ},{rotW},{rigidbodyPosX},{rigidbodyPosY},{rigidbodyPosZ},{rigidbodyRotX},{rigidbodyRotY},{rigidbodyRotZ},{rigidbodyRotW}";

        // ������Ϣ
        GameMgr.Instance.socketmgr.SendUDPTo(message);

        return message;
    }


    private void getsyncstates()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        // ͬ�����λ�ú���ת��Ϣ
        posX = transform.position.x;
        posY = transform.position.y;
        posZ = transform.position.z;

        rotX = transform.rotation.x;
        rotY = transform.rotation.y;
        rotZ = transform.rotation.z;
        rotW = transform.rotation.w;

        // ͬ������λ�ú���ת��Ϣ
        rigidbodyPosX = playerRigidbody.position.x;
        rigidbodyPosY = playerRigidbody.position.y;
        rigidbodyPosZ = playerRigidbody.position.z;

        rigidbodyRotX = playerRigidbody.rotation.x;
        rigidbodyRotY = playerRigidbody.rotation.y;
        rigidbodyRotZ = playerRigidbody.rotation.z;
        rigidbodyRotW = playerRigidbody.rotation.w;
    }

    public string SerializePlayerState(Vector3 targetPosition, Quaternion targetRotation)
    {
        // ��ȡ��ҵ�ǰλ�ú���ת״̬
        getsyncstates();

        string objectName = gameObject.name;

        // ������������Ŀ��λ�ú���ת��Ϣ�ϲ�Ϊһ����Ϣ
        string message = $"{objectName}:{targetPosition.x},{targetPosition.y},{targetPosition.z},{targetRotation.x},{targetRotation.y},{targetRotation.z},{targetRotation.w},{rigidbodyPosX},{rigidbodyPosY},{rigidbodyPosZ},{rigidbodyRotX},{rigidbodyRotY},{rigidbodyRotZ},{rigidbodyRotW}";

        // ������Ϣ
        GameMgr.Instance.socketmgr.SendUDPTo(message);

        return message;
    }

    void Update()
    {
        //����GameStart��������Ƿ���Ե��
        if (GameStart.Instance.Gameflag == gameObject.name)
        {
            // ��������
            if (Input.GetMouseButtonDown(0))
            {
                move();
               

            }

            

            // ����ƶ���ƽ����ת
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        Vector3 targetCameraPosition = new Vector3(targetPosition.x, cameraOffset.y, targetPosition.z);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetCameraPosition, cameraSmoothSpeed * Time.deltaTime);
        cameraTransform.rotation = Quaternion.Euler(90, 0, 0);




    }
    //�����ı䷽��
    private void UpdateAnimation(string animationState)
    {
        // ���ݴ���Ķ���״̬�ַ��������¶�������
        switch (animationState)
        {
            case "MoveFWD_Normal_RM_SwordAndShield":
                animator.SetBool("MoveFWD_Normal_RM_SwordAndShield", true);
                
                break;
            
            case "Run":
                animator.SetBool("IsRunning", true);
                break;
            default:
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsRunning", false);
                break;
        }
    }


    public void move()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            targetPosition = hit.point; // ��ȡ���λ��
            Vector3 direction = (targetPosition - transform.position).normalized;

            // �޸����ﳯ��Ŀ��λ��
            lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }

        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            moveDirection = (targetPosition - transform.position).normalized;

            // ͬ�����״̬
            
        }
        // ����ͬ����ҵ�����Ŀ��λ�ú���ת��Ϣ
        SerializePlayerState(targetPosition, lookRotation);
    }

    public void DeserializePlayerState(string data)
    {
        // ����ַ��������Ȼ�ȡ�������ƺ�λ�����ݲ���
        Debug.Log(data);
        string[] values = data.Split(',');

        if (values.Length != 14)
        {
            Debug.LogError("λ����Ϣ���ݸ�ʽ����");
            return;
        }

        Vector3 targetPos = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        Quaternion targetRot = new Quaternion(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));

        Vector3 targetRigidbodyPos = new Vector3(float.Parse(values[7]), float.Parse(values[8]), float.Parse(values[9]));
        Quaternion targetRigidbodyRot = new Quaternion(float.Parse(values[10]), float.Parse(values[11]), float.Parse(values[12]), float.Parse(values[13]));

        // ƽ���ز�ֵ����λ�ú���ת��Ϣ
        targetPosition = targetPos;
        lookRotation = targetRot;

        if (playerRigidbody != null)
        {
            playerRigidbody.position = Vector3.Lerp(playerRigidbody.position, targetRigidbodyPos, 0.25f);
            playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, targetRigidbodyRot, 0.25f);
        }
    }
}
