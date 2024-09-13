using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public string male;
    public string maleName;
    public GameObject panel;
    public InputField nameInputField; // 改为使用InputField
    public GameObject cameraPrefab; // 用于创建相机的预制体
    public Vector3 cameraOffset; // 摄像机相对于角色的偏移
    public string prefabName; // 要加载的预制体的名字
    public string Gameflag;//游戏角色姓名检测
    public static GameStart Instance { get; private set; }//使用单例模式，让全局调用
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 如果希望在场景切换时保留这个对象
        }
        else
        {
            Destroy(gameObject); // 如果实例已存在，则销毁新创建的对象
        }
    }

    public void changesex1()
    {
        male = "MaleCharacterPBR";
    }
    public void changesex2()
    {
        male = "FemaleCharacterPBR";
    }
    public void startgame()
    {
        maleName = nameInputField.text;
        panel.SetActive(false);
        LoadPrefabAtPosition();
    }

    public void LoadPrefabAtPosition()
    {
        // 通过Resources.Load加载预制体
        Vector3 position = new Vector3((float)8, (float)1.8, (float)14); // 初始化为某个位置
        GameObject prefab = Resources.Load<GameObject>(male);

        if (prefab == null)
        {
            Debug.LogError("预制体未找到，请确保预制体位于Resources文件夹中，并且名称正确！");
            return;
        }

        // 实例化预制体
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);

        // 修改预制体的名字为输入的名字加上预制体的原名
        instance.name = maleName + ":" + prefab.name;
        Gameflag = instance.name;
        Debug.Log(Gameflag);


        // 添加一些组件，例如 Rigidbody 和 Collider
        if (instance.GetComponent<Rigidbody>() == null)
        {
            instance.AddComponent<Rigidbody>();
        }

        if (instance.GetComponent<BoxCollider>() == null)
        {
            instance.AddComponent<BoxCollider>();
            BoxCollider boxCollider = instance.GetComponent<BoxCollider>();

            boxCollider.center = new Vector3(0, 0.36f, 0);
            boxCollider.size = new Vector3(0.3f, 0.5f, 0.5f);
        }

        // 添加自定义脚本（例如 PlayerState 脚本）
        if (instance.GetComponent<PlayerState>() == null)
        {
            instance.AddComponent<PlayerState>();
            
        }
        PlayerState playerState = instance.GetComponent<PlayerState>();

        Debug.Log($"预制体 {instance.name} 已加载，并添加组件");

        // 创建并设置摄像机跟随
        
        CameraFollow cameraFollow = cameraPrefab.AddComponent<CameraFollow>();
        cameraOffset = new Vector3((float)0, (float)10, (float)0);
        cameraFollow.offset=cameraOffset;
        // 将角色对象赋值给摄像机的跟随目标
        playerState.cameraTransform = cameraPrefab.transform;

        
    }
}
