using UnityEngine;
using System;

[Serializable]
public class PlayerState : MonoBehaviour
{
    public string playerNAME;
    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;

    // 刚体信息
    public float rigidbodyPosX, rigidbodyPosY, rigidbodyPosZ;
    public float rigidbodyRotX, rigidbodyRotY, rigidbodyRotZ, rigidbodyRotW;

    private Rigidbody playerRigidbody;
    private Vector3 targetPosition;
    public float moveSpeed = 10.0f; // 控制移动速度
    public Quaternion lookRotation;
    public Vector3 moveDirection;
    public float cameraSmoothSpeed = 2.0f; // 控制摄像机的平滑移动速度
    public Vector3 cameraOffset = new Vector3(0f, 12.5f, 0f); // 摄像机的相对偏移
    public Transform cameraTransform; // 摄像机Transform变量
    private Animator animator;


    private void Awake()
    {
        // 获取刚体组件
        playerRigidbody = GetComponent<Rigidbody>();
        targetPosition = transform.position; // 初始化为当前的位置
        animator = GetComponent<Animator>();//获取动画器

    }
    void Start()
    {
        SerializePlayerState();
    }
    //重写一个发送当前位置方法，用于初始化
    public string SerializePlayerState()
    {
        // 获取当前玩家的位置和旋转状态
        getsyncstates();

        string objectName = gameObject.name;

        // 将对象名称与当前位置和旋转信息合并为一个消息
        string message = $"{objectName}:{posX},{posY},{posZ},{rotX},{rotY},{rotZ},{rotW},{rigidbodyPosX},{rigidbodyPosY},{rigidbodyPosZ},{rigidbodyRotX},{rigidbodyRotY},{rigidbodyRotZ},{rigidbodyRotW}";

        // 发送消息
        GameMgr.Instance.socketmgr.SendUDPTo(message);

        return message;
    }


    private void getsyncstates()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        // 同步玩家位置和旋转信息
        posX = transform.position.x;
        posY = transform.position.y;
        posZ = transform.position.z;

        rotX = transform.rotation.x;
        rotY = transform.rotation.y;
        rotZ = transform.rotation.z;
        rotW = transform.rotation.w;

        // 同步刚体位置和旋转信息
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
        // 获取玩家当前位置和旋转状态
        getsyncstates();

        string objectName = gameObject.name;

        // 将对象名称与目标位置和旋转信息合并为一个消息
        string message = $"{objectName}:{targetPosition.x},{targetPosition.y},{targetPosition.z},{targetRotation.x},{targetRotation.y},{targetRotation.z},{targetRotation.w},{rigidbodyPosX},{rigidbodyPosY},{rigidbodyPosZ},{rigidbodyRotX},{rigidbodyRotY},{rigidbodyRotZ},{rigidbodyRotW}";

        // 发送消息
        GameMgr.Instance.socketmgr.SendUDPTo(message);

        return message;
    }

    void Update()
    {
        //调用GameStart单例检测是否可以点击
        if (GameStart.Instance.Gameflag == gameObject.name)
        {
            // 鼠标点击检测
            if (Input.GetMouseButtonDown(0))
            {
                move();
               

            }

            

            // 玩家移动和平滑旋转
        }
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        Vector3 targetCameraPosition = new Vector3(targetPosition.x, cameraOffset.y, targetPosition.z);
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetCameraPosition, cameraSmoothSpeed * Time.deltaTime);
        cameraTransform.rotation = Quaternion.Euler(90, 0, 0);




    }
    //动画改变方法
    private void UpdateAnimation(string animationState)
    {
        // 根据传入的动画状态字符串来更新动画参数
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
            targetPosition = hit.point; // 获取点击位置
            Vector3 direction = (targetPosition - transform.position).normalized;

            // 修改人物朝向目标位置
            lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        }

        if (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            moveDirection = (targetPosition - transform.position).normalized;

            // 同步玩家状态
            
        }
        // 立即同步玩家点击后的目标位置和旋转信息
        SerializePlayerState(targetPosition, lookRotation);
    }

    public void DeserializePlayerState(string data)
    {
        // 拆分字符串，首先获取对象名称和位置数据部分
        Debug.Log(data);
        string[] values = data.Split(',');

        if (values.Length != 14)
        {
            Debug.LogError("位置信息数据格式错误");
            return;
        }

        Vector3 targetPos = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        Quaternion targetRot = new Quaternion(float.Parse(values[3]), float.Parse(values[4]), float.Parse(values[5]), float.Parse(values[6]));

        Vector3 targetRigidbodyPos = new Vector3(float.Parse(values[7]), float.Parse(values[8]), float.Parse(values[9]));
        Quaternion targetRigidbodyRot = new Quaternion(float.Parse(values[10]), float.Parse(values[11]), float.Parse(values[12]), float.Parse(values[13]));

        // 平滑地插值更新位置和旋转信息
        targetPosition = targetPos;
        lookRotation = targetRot;

        if (playerRigidbody != null)
        {
            playerRigidbody.position = Vector3.Lerp(playerRigidbody.position, targetRigidbodyPos, 0.25f);
            playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, targetRigidbodyRot, 0.25f);
        }
    }
}
