using UnityEngine;
using UnityEngine.UI;
using DownloadFileNW;

public class DownloadDemo : MonoBehaviour {
    public Text text;
    Download download;
    float lastTime = 0.0f;
    Download download2 = null;

    void Awake()
    {
        string url = "https://d1.music.126.net/dmusic/cloudmusicsetup_2_2_3_195673.exe";
        download = new Download(url, "D:/", true, HttpVerbType.kHttpVerbGET);
        download.Completed += Download_Completed;
        download.StartDownload();
        //download2 = new Download(url, "D:/", true, HttpVerbType.kHttpVerbGET);
        //download2.StartDownload();
    }

    private void Download_Completed()
    {
        string msg = null;
        if (download.IsHttpError)
        {
            msg = "Http连接错误,状态码:"+ download.HttpErrorCode+"\n";
        }
        if (download.IsSystemError)
        {
            msg += "系统错误:" + download.SystemErrorMsg +"\n";
        }
        text.text = msg + "下载完成！用时:" + (Time.time);
    }

    void Update () {
        if (!download.IsDone && Time.time - lastTime > 1.0f)
        {
            lastTime = Time.time;
            text.text = "文件大小:" + download.FileSize + "Byte\n下载进度:" + download.PercentageProgress + "\n下载速度:" + download.DownloadSpeed + "KB/S";
        }
        if (Input.GetMouseButtonDown(0) && download2!=null)
        {
            string msg = null;
            if (download2.IsHttpError)
            {
                msg = "Http连接错误,状态码:" + download2.HttpErrorCode + "\n";
            }
            if (download2.IsSystemError)
            {
                msg += "系统错误:" + download2.SystemErrorMsg + "\n";
            }
            text.text = msg + "下载完成！用时:" + (Time.time);
        }
        if (Input.GetMouseButtonDown(1))
        {
            download.Abort();
        }
    }
}
