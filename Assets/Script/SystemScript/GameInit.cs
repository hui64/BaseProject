using UnityEngine;
using System.Collections;

public class GameInit : MonoBehaviour { 

    void Awake() {
        Debug.Log("初始化-----");
    }
    void Start() {
        Init();
    }
    private void Init() {
                    
    }
    //加载登录界面
    private void LoadLoginUI() {
        GameObject loginUI = GameTools.CreateByReourse("UIPrefab/loginUI");
    }
}
