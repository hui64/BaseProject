using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class FileManager
{
    public static readonly string VersionPath = "version.txt";                          //版本文件名
    public static readonly string LocalPath = "Assets/Test/";                           //热跟文件夹
    public static readonly string SeverURL = "";                                        //资源服务器地址  
    public void Init()
    {

    }
    //根据文件名获取本地总路径
    public static string GetFullFilePath(string fileName)
    {
        Debug.Log(LocalPath + StringTool.Trim(fileName));
        return LocalPath + StringTool.Trim(fileName);
    }
    //根据文件名获取服务器地址
    public static string GetFullURL(string fileName)
    {
        return SeverURL + StringTool.Trim(fileName);
    }
    //获取该路径下的所有文件路径名
    public static void GetFilePathsAndMD5(string fullPath, Dictionary<string, string> versions)
    {
        //获取路径下的所有文件
        DirectoryInfo Dir = new DirectoryInfo(fullPath);
        //获取所有子路径
        foreach (DirectoryInfo info in Dir.GetDirectories())
        {
            GetFilePathsAndMD5(info.Name, versions);
        }
        //获取所有子文件
        foreach (FileInfo info in Dir.GetFiles())
        {
            //if (info.Name.EndsWith(".assetsbundle"))
            {
                Debug.Log(info.FullName);
                versions.Add(info.FullName, GetMD5(info.FullName));
            }
        }
    }
    //获取文件的MD5
    public static string GetMD5(string fileName)
    {
        FileStream file = new FileStream(fileName, FileMode.Open);
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] retVal = md5.ComputeHash(file);
        file.Close();
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < retVal.Length; i++)
        {
            sb.Append(retVal[i].ToString("x2"));
        }
        return sb.ToString();
    }
    //写入map
    public static void WriteVersion(string fullPath, Dictionary<string, string> pathAndMD5)
    {
        StringBuilder myString = new StringBuilder();
        foreach (var item in pathAndMD5)
        {
            myString.Append(item.Key).Append(",").Append(item.Value).Append("\n");
            FileStream stream = new FileStream(fullPath, FileMode.Create);
            byte[] data = Encoding.UTF8.GetBytes(myString.ToString());
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();
        }
    }
}

