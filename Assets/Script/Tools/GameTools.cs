using UnityEngine;
using System.Collections;

public class GameTools : MonoBehaviour {
    public static GameObject CreateByReourse(string path) {
        GameObject loginUI = Resources.Load("loginUI") as GameObject;
        return Instantiate(loginUI);
    }
	
}
