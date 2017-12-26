using System.Collections.Generic;
using UnityEngine;
using DownloadFileNW;

public class JsonManager{

    /// <summary>
    /// 将指定路径的服务器json文件加载为Dictionary<string, Dictionary<string, AssetBundleInfo>>类型
    /// </summary>
    /// <param name="path">json文件的路径</param>
    /// <returns>返回一个二维字典,如果文件不存在则返回null</returns>
    public static Dictionary<string, Dictionary<string, AssetBundleInfo>> ResolutionServerJson(string path)
    {
        if (!FileTools.FileExists(path))
        {
            return null;
        }
        string json = FileTools.ReadFileUTf8(path);
        if (json == null || json.Length==0)
        {
            return null;
        }
        AssetBundleInfoList assetBundleInfoList = JsonUtility.FromJson<AssetBundleInfoList>(json);
        Dictionary<string, Dictionary<string, AssetBundleInfo>> ServerData = new Dictionary<string, Dictionary<string, AssetBundleInfo>>();
        foreach (var item in assetBundleInfoList.ServerAssetBundleList)
        {
            string sceneName = item.SceneName;
            string curVersion = item.CurVersion;
            if (!ServerData.ContainsKey(sceneName))
            {
                ServerData[sceneName] = new Dictionary<string, AssetBundleInfo>();
            }
            ServerData[sceneName][curVersion] = item;
        }
        return ServerData;
    }
    
    /// <summary>
    /// 将指定路径的本地json加载为指定类型
    /// </summary>
    /// <param name="path">json文件的路径</param>
    /// <returns>一个二维字典，如果json文件不存在或者为空，则返回一个空字典(不是Null，而是内容为空)</returns>
    public static Dictionary<string, Dictionary<string, LocalAssetBundleInfo>> ResolutionLocalJson(string path)
    {
        if (!FileTools.FileExists(path))
        {
            return new Dictionary<string, Dictionary<string, LocalAssetBundleInfo>>();
        }
        string json = FileTools.ReadFileUTf8(path);
        if (json == null || json.Length == 0)
        {
            return new Dictionary<string, Dictionary<string, LocalAssetBundleInfo>>();
        }
        LocalAssetBundleInfoList localAssetBundleInfoList = JsonUtility.FromJson<LocalAssetBundleInfoList>(json);
        Dictionary<string, Dictionary<string, LocalAssetBundleInfo>> LocalData = new Dictionary<string, Dictionary<string, LocalAssetBundleInfo>>();
        foreach (var item in localAssetBundleInfoList.LocalAssetBundleInfoLists)
        {
            string sceneName = item.assetBundleInfo.SceneName;
            string curVersion = item.assetBundleInfo.CurVersion;
            if (!LocalData.ContainsKey(sceneName))
            {
                LocalData[sceneName] = new Dictionary<string, LocalAssetBundleInfo>();
            }
            LocalData[sceneName][curVersion] = item;
        }
        return LocalData;
    }

    /// <summary>
    /// 将指定的类型写入到本地json文件中
    /// </summary>
    /// <param name="LocalData">指定的类型</param>
    /// <param name="path">json文件的路径,没有则自动创建,有则覆盖</param>
    public static void WriteLocalJson(Dictionary<string, Dictionary<string, LocalAssetBundleInfo>> LocalData,string path)
    {
        List<LocalAssetBundleInfo> localList = new List<LocalAssetBundleInfo>();
        foreach (var item in LocalData)
        {
            foreach (var item2 in item.Value)
            {
                localList.Add(item2.Value);
            }
        }
        LocalAssetBundleInfoList localAssetBundleInfoList = new LocalAssetBundleInfoList(localList.ToArray());
        string json = JsonUtility.ToJson(localAssetBundleInfoList);
        FileTools.WriteFileUtf8Create(path, json);
    }

}
