using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;

public class Build{
    [MenuItem("Build/生成Version")]
    static void BuildVersion()
    {
        Dictionary<string, string> fileData = new Dictionary<string, string>();
        FileManager.GetFilePathsAndMD5(FileManager.LocalPath, fileData);
        FileManager.WriteVersion(FileManager.GetFullFilePath(FileManager.VersionPath), fileData);
        AssetDatabase.Refresh();
        // 从工程中将该模型读出来
        GameObject tar = AssetDatabase.LoadAssetAtPath("Assets/Test/picture.jpg", typeof(Texture2D)) as GameObject;
        Debug.Log(tar);
        // 将这个模型创建为Prefab
        GameObject prefab = PrefabUtility.CreatePrefab("Assets/Test/version.prefab",tar);
    }

    [MenuItem("Custom Editor/Create AssetBunldes Main")]
    static void CreateAssetBunldesMain()
    {
        //获取在Project视图中选择的所有游戏对象
        Object[] SelectedAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

        //遍历所有的游戏对象
        foreach (Object obj in SelectedAsset)
        {
            string sourcePath = AssetDatabase.GetAssetPath(obj);
            //本地测试：建议最后将Assetbundle放在StreamingAssets文件夹下，如果没有就创建一个，因为移动平台下只能读取这个路径
            //StreamingAssets是只读路径，不能写入
            //服务器下载：就不需要放在这里，服务器上客户端用www类进行下载。
            string targetPath = Application.dataPath + "/StreamingAssets/" + obj.name + ".assetbundle";
            if (BuildPipeline.BuildAssetBundle(obj, null, targetPath, BuildAssetBundleOptions.CollectDependencies))
            {
                Debug.Log(obj.name + "资源打包成功");
            }
            else
            {
                Debug.Log(obj.name + "资源打包失败");
            }
        }
        //刷新编辑器
        AssetDatabase.Refresh();
    }
}
