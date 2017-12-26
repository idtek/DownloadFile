using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DownloadFileNW;

public class DownloadManagerDemo : MonoBehaviour {
    public string[] URL;
    public Text[] text;
    private DownloadManager downloadManager;
    private float lastTime = 0;
    private List<int> IDs;

    void Awake()
    {
        downloadManager = DownloadManagerHelper.GetDonwloadManager();
        downloadManager.MaxDownloadCount = 2;
        IDs = new List<int>();
    }

	// Use this for initialization
	void Start () {
        for (int i = 0; i < URL.Length; i++)
        {
            IDs.Add(downloadManager.AddDownload(URL[i], "D:/", true, HttpVerbType.kHttpVerbGET, Completed));
        }
	}

    void Completed(Download download)
    {
        int id = downloadManager.GetID(download);
        if (id == 0)
        {
            return;
        }
        if (downloadManager.GetErrorMsg(id) == null)
        {
            text[id - 1].text = "  ID:"+ id + "    "+ "下载成功";
        }
        else
        {
            text[id-1].text = "  ID:" + id + "    " + downloadManager.GetErrorMsg(id);
        }
    }

	// Update is called once per frame
	void Update () {
        if (Time.time - lastTime > 1.0f)
        {
            lastTime = Time.time;
            foreach (var item in IDs)
            {
                if (downloadManager.InDownloadQueue(item))
                {
                    if (!downloadManager.IsDone(item))
                    {
                        text[item - 1].text = "  ID:" + item + "    下载速度:" + downloadManager.DownloadSpeed(item) + "KB/S 下载进度:" + downloadManager.PercentageProgress(item);
                    }
                }
                else
                {
                    text[item - 1].text = "  ID:" + item + "    下载空闲中";
                }
                
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            downloadManager.MaxDownloadCount = 5;
        }
        else if (Input.GetMouseButtonDown(1))
        {
            downloadManager.AbortAllDownload();
        }
	}
}
