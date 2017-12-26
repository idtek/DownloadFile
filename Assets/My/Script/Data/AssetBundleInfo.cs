using System;

[Serializable]
public class AssetBundleInfo {
    public string SceneName;//场景的名称
    public string CurVersion;//当前版本
    public string Url;//下载链接

    public AssetBundleInfo(string sceneName, string curVersion, string url)
    {
        SceneName = sceneName;
        CurVersion = curVersion;
        Url = url;
    }
}

[Serializable]
public class AssetBundleInfoList
{
    public AssetBundleInfo[] ServerAssetBundleList;
}
