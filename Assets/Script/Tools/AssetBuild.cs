using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class AssetBuild : MonoBehaviour {

    [MenuItem("AssetBundles/Build AssetBundles")]
    static void BuildABs()
    {
        //// Create the array of bundle build details.
        //AssetBundleBuild[] buildMap = new AssetBundleBuild[1];

        //buildMap[0].assetBundleName = "resourse.assetbundle";//打包的资源包名称 随便命名
        //string[] resourcesAssets = new string[4];//此资源包下面有多少文件
        //resourcesAssets[0] = "Assets/Prefab/player.prefab";
        //resourcesAssets[1] = "Assets/LuaScript/GameInit.txt";
        //resourcesAssets[2] = "Assets/Texture/app.png";
        //resourcesAssets[3] = "Assets/Prefab/enemy.prefab";
        //buildMap[0].assetNames = resourcesAssets;
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles");
    }
    void Start() {
        StartCoroutine("LoadAssetBundles");
        //List<string> filePaths = new List<string>();
    } 
    IEnumerator LoadAssetBundles() {
        WWW www = new WWW("file:///" + Application.dataPath + "/AssetBunlds/resourse.assetbundle");
        yield return www;
        print(Application.dataPath);
        if(!string.IsNullOrEmpty(www.error)){
            Debug.Log(www.error);
            yield break;
        }
       // object app = www.assetBundle.LoadAsset("app");
        //Debug.Log(app);
        //yield break;
        GameObject[] gos = www.assetBundle.LoadAllAssets<GameObject>();
        TextAsset[] texts = www.assetBundle.LoadAllAssets<TextAsset>();
        Texture2D[] textures = www.assetBundle.LoadAllAssets<Texture2D>();
        print(texts.Length);
        foreach (GameObject go in gos)
        {
            Debug.Log(go.name);
            Instantiate(go);
            yield return new WaitForFixedUpdate();
        }
        foreach (var text in texts)
        {
            Debug.Log(text.name);
            yield return new WaitForFixedUpdate();
        }
        foreach (var texture in textures)
        {
            Debug.Log(texture.name);
            yield return new WaitForFixedUpdate();

        }
    }
   
}
