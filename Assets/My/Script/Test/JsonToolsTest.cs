using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JsonToolsTest : MonoBehaviour {
    void Start () {
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
        {
            testWriteLocal();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            testReadLocal();
        }
        else if (Input.GetMouseButtonDown(2))
        {
            testReadServer();
        }
	}

    void testReadLocal()
    {
        foreach (var item in JsonManager.ResolutionLocalJson("D:/UnityProject/DownloadFile/Assets/My/Resources/Data/Json/LocalJson.json"))
        {
            foreach (var item2 in item.Value)
            {
                Debug.Log(item.Key + " " + item2.Key + " " + item2.Value.assetBundleFileName);
            }
        }
    }

    void testWriteLocal()
    {
        Dictionary<string, Dictionary<string, LocalAssetBundleInfo>> LocalData = new Dictionary<string, Dictionary<string, LocalAssetBundleInfo>>();
        for (int i = 1; i <= 5; i++)
        {
            LocalData.Add("场景" + i, new Dictionary<string, LocalAssetBundleInfo>());
            for (int j = 1; j <= 3; j++)
            {
                LocalData["场景" + i]["版本" + j] = new LocalAssetBundleInfo(new AssetBundleInfo("场景" + i, "版本" + j,"Baidu"));
            }
        }
        JsonManager.WriteLocalJson(LocalData, "D:/UnityProject/DownloadFile/Assets/My/Resources/Data/Json/LocalJson.json");
    }

    void testReadServer()
    {
        foreach (var item in JsonManager.ResolutionServerJson("D:/UnityProject/DownloadFile/Assets/My/Resources/Data/Json/ServerJson.json"))
        {
            foreach (var item2 in item.Value)
            {
                Debug.Log(item.Key + " " + item2.Key + " " + item2.Value.Url);
            }
        }
    }

}
