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
        //string url = "http://d1.music.126.net/dmusic/netease-cloud-music_1.1.0_amd64_deepin.deb";
        //request = new UnityWebRequest(url);
        //request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("Range", "bytes=0-500");
        //request.SendWebRequest().completed += DownloadRangeTest_completed; 
        test();
        //test2();
        
    }

    void test2()
    {
        string url = "https://d1.music.126.net/dmusic/cloudmusicsetup_2_2_3_195673.exe";
        request = new UnityWebRequest(url);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SendWebRequest().completed += DownloadRangeTest_completed1;
    }

    private void DownloadRangeTest_completed1(AsyncOperation obj)
    {
        FileTools.WriteFileBytesAppend("D:/网易云.exe", request.downloadHandler.data);
    }

    private void test()
    {
        string url = "https://d1.music.126.net/dmusic/cloudmusicsetup_2_2_3_195673.exe";
        request = new UnityWebRequest(url);
        DownloadRange downloadRange = new DownloadRange("D:/网易云音乐.exe",request);
        request.downloadHandler = downloadRange;
        string temp = "bytes=" + downloadRange.LastIndex.ToString() + "-";
        Debug.Log(temp);
        request.SetRequestHeader("Range", temp);
        UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
        unityWebRequestAsyncOperation.completed += downloadRange.DownloadRange_completed;
        unityWebRequestAsyncOperation.completed += UnityWebRequestAsyncOperation_completed;
    }

    private void UnityWebRequestAsyncOperation_completed(AsyncOperation obj)
    {
        Debug.Log(request.GetResponseHeader("Content-Range"));
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            request.Abort();
        }
    }
}
