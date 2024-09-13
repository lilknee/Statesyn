using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : PlayerState
{
    public Message message { get; private set; }

    public socketMgr socketmgr { get; private set; }
    public PlayerState playerState { get; private set; }



    private static GameMgr _instance;
    public static GameMgr Instance { get { return _instance; } }
    void Awake()
    {

        _instance = this;
        
        socketmgr = gameObject.AddComponent<socketMgr>();

        socketmgr.OnInit();
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
   
}
