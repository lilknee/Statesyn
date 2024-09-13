using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public string male;
    public string maleName;
    public GameObject panel;
    public InputField nameInputField; // ��Ϊʹ��InputField
    public GameObject cameraPrefab; // ���ڴ��������Ԥ����
    public Vector3 cameraOffset; // ���������ڽ�ɫ��ƫ��
    public string prefabName; // Ҫ���ص�Ԥ���������
    public string Gameflag;//��Ϸ��ɫ�������
    public static GameStart Instance { get; private set; }//ʹ�õ���ģʽ����ȫ�ֵ���
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���ϣ���ڳ����л�ʱ�����������
        }
        else
        {
            Destroy(gameObject); // ���ʵ���Ѵ��ڣ��������´����Ķ���
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
        // ͨ��Resources.Load����Ԥ����
        Vector3 position = new Vector3((float)8, (float)1.8, (float)14); // ��ʼ��Ϊĳ��λ��
        GameObject prefab = Resources.Load<GameObject>(male);

        if (prefab == null)
        {
            Debug.LogError("Ԥ����δ�ҵ�����ȷ��Ԥ����λ��Resources�ļ����У�����������ȷ��");
            return;
        }

        // ʵ����Ԥ����
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);

        // �޸�Ԥ���������Ϊ��������ּ���Ԥ�����ԭ��
        instance.name = maleName + ":" + prefab.name;
        Gameflag = instance.name;
        Debug.Log(Gameflag);


        // ���һЩ��������� Rigidbody �� Collider
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

        // ����Զ���ű������� PlayerState �ű���
        if (instance.GetComponent<PlayerState>() == null)
        {
            instance.AddComponent<PlayerState>();
            
        }
        PlayerState playerState = instance.GetComponent<PlayerState>();

        Debug.Log($"Ԥ���� {instance.name} �Ѽ��أ���������");

        // �������������������
        
        CameraFollow cameraFollow = cameraPrefab.AddComponent<CameraFollow>();
        cameraOffset = new Vector3((float)0, (float)10, (float)0);
        cameraFollow.offset=cameraOffset;
        // ����ɫ����ֵ��������ĸ���Ŀ��
        playerState.cameraTransform = cameraPrefab.transform;

        
    }
}
