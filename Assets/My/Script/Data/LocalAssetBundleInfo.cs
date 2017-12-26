using UnityEngine;
using System.IO;
using System;

[Serializable]
public class LocalAssetBundleInfo{
    public AssetBundleInfo assetBundleInfo;
    public bool IsExist;//是否存在
    public string savePath;//项目资源路径+ /AssetBundleFile/场景名/assetBundleFileName
    public string assetBundleFileName;//场景名+版本号

    public LocalAssetBundleInfo(AssetBundleInfo assetBundleInfo, bool isExist = false)
    {
        this.assetBundleInfo = assetBundleInfo;
        this.IsExist = isExist;
        assetBundleFileName = assetBundleInfo.SceneName + assetBundleInfo.CurVersion;
        string DirectoryPath = Application.persistentDataPath + "/AssetBundleFile/" + assetBundleInfo.SceneName;
        if (!Directory.Exists(DirectoryPath))
        {
            Directory.CreateDirectory(DirectoryPath);
        }
        savePath = DirectoryPath + "/" + assetBundleFileName + ".assetbundle";
    }
}

[Serializable]
public class LocalAssetBundleInfoList
{
    public LocalAssetBundleInfo[] LocalAssetBundleInfoLists;

    public LocalAssetBundleInfoList(LocalAssetBundleInfo[] localAssetBundleInfoLists)
    {
        LocalAssetBundleInfoLists = localAssetBundleInfoLists;
    }
}
