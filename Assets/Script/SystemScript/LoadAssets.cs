using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using System.IO;

public class LoadAssets: BaseMonoBehaviour{

    public static Dictionary<string, string> LocalVersion;
    public static Dictionary<string, string> SeverVersion;
    public static bool IsCanUpDateVersion = true;
    private List<string> NeedDownLoadFiles;

    public void Init() {
        LocalVersion = new Dictionary<string, string>();
        SeverVersion = new Dictionary<string, string>();
        NeedDownLoadFiles = new List<string>();  
    }
    void Start() {
        StartCoroutine(DownLoad01());
    }
    private void DownLocalVersion() {
        StartCoroutine(DownLoad(FileManager.GetFullFilePath(FileManager.VersionPath), delegate (WWW www)
        {
            //保存本地version
            ParseVersionPath(www.text, LocalVersion);
            //加载服务端version配置  
            StartCoroutine(this.DownLoad(FileManager.GetFullURL(FileManager.VersionPath), delegate (WWW serverVersion)
            {
                //保存服务端version  
                ParseVersionPath(serverVersion.text, SeverVersion
                    );
                //计算出需要重新加载的资源  
                CompareVersion();
                //加载需要更新的资源  
                DownLoadRes();
            }));

        }));
    }
    //依次加载需要更新的资源  
    private void DownLoadRes()
    {
        if (NeedDownLoadFiles.Count == 0)
        {
            UpdateLocalVersionPath();
            return;
        }

        string file = NeedDownLoadFiles[0];
        NeedDownLoadFiles.RemoveAt(0);

        StartCoroutine(this.DownLoad(FileManager.GetFullURL(file), delegate (WWW w)
        {
            //将下载的资源替换本地旧的资源  
            ReplaceLocalRes(file, w.bytes);
            DownLoadRes();
        }));
    }
    //更新本地的version配置  
    private void UpdateLocalVersionPath()
    {
        if (IsCanUpDateVersion)
        {
            StringBuilder versions = new StringBuilder();
            foreach (var item in SeverVersion)
            {
                versions.Append(item.Key).Append(",").Append(item.Value).Append("\n");
            }

            FileStream stream = new FileStream(FileManager.GetFullFilePath(FileManager.VersionPath), FileMode.Create);
            byte[] data = Encoding.UTF8.GetBytes(versions.ToString());
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();
        }
        //加载显示对象  
        StartCoroutine(Show());
    }
    private void ReplaceLocalRes(string fileName, byte[] data)
    {
        FileStream stream = new FileStream(fileName, FileMode.Create);
        stream.Write(data, 0, data.Length);
        stream.Flush();
        stream.Close();
    }
    private void CompareVersion()
    {
        foreach (var version in SeverVersion)
        {
            string fileName = version.Key;
            string serverMd5 = version.Value;
            //新增的资源  
            if (!LocalVersion.ContainsKey(fileName))
            {
                NeedDownLoadFiles.Add(fileName);
            }
            else
            {
                //需要替换的资源  
                string localMd5;
                LocalVersion.TryGetValue(fileName, out localMd5);
                if (!serverMd5.Equals(localMd5))
                {
                    NeedDownLoadFiles.Add(fileName);
                }
            }
        }
        //本次有更新，同时更新本地的version.txt  
        IsCanUpDateVersion = NeedDownLoadFiles.Count > 0;
    }
    //显示资源  
    private IEnumerator Show()
    {
        WWW asset = new WWW(FileManager.GetFullURL("cube.assetbundle"));
        yield return asset;
        AssetBundle bundle = asset.assetBundle;
        Instantiate(bundle.LoadAsset("Cube"));
        bundle.Unload(false);
    }
    private void ParseVersionPath(string content, Dictionary<string, string> dict)
    {
        if (content == null || content.Length == 0)
        {
            return;
        }
        string[] items = content.Split(new char[] { '\n' });
        foreach (string item in items)
        {
            string[] info = item.Split(new char[] { ',' });
            if (info != null && info.Length == 2)
            {
                dict.Add(info[0], info[1]);
            }
        }
    }
    private IEnumerator DownLoad(string url, HandleFinishDownload finishFun)
    {
        WWW www = new WWW(url);
        yield return www;
        if (finishFun != null)
        {
            finishFun(www);
        }
        www.Dispose();
    }
    private IEnumerator DownLoad01() {
        //print("=========");
        WWWForm info = new WWWForm();
        WWW versionWWW = new WWW(@"file:///E:/BaseProject/Assets/Test/version.txt");
        yield return versionWWW;
        print(versionWWW.text);
        info.AddBinaryData("aa", versionWWW.bytes,"aa.txt");
        WWW www = new WWW("http://192.168.0.141:8080/assets/upload/", info);
        yield return www;
        //print(www);
        //WWW www01 = new WWW("http://192.168.0.141:8080/assets/download?filename=aa.dat");
        //yield return www01;
        //print(www01.text);
        //print(www01.assetBundle.name);
        // print(www.text);
        // print(www.assetBundle.name);

    }
    public delegate void HandleFinishDownload(WWW www);
}
