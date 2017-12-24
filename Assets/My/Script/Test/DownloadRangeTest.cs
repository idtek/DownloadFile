using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using DownloadFileNW;

public class DownloadRangeTest : MonoBehaviour {
    UnityWebRequest request;
    private void Awake()
    {
        test();
    }

    void test()
    {
        string url = "https://d1.music.126.net/dmusic/cloudmusicsetup_2_2_3_195673.exe";
        request = new UnityWebRequest(url);
        DownloadRange download = new DownloadRange("D:/网易云.exe",request);
        request.downloadHandler = download;
        request.SendWebRequest().completed += DownloadRangeTest_completed1;
    }

    private void DownloadRangeTest_completed1(AsyncOperation obj)
    {
        Debug.Log("下载结束");
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (!request.isDone)
        {
            Debug.Log(request.downloadProgress);
        }
        if (Input.GetMouseButtonDown(0))
        {
            request.Abort();
        }
    }
}
