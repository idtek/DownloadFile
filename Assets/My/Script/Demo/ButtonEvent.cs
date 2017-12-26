using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DownloadFileNW;

public class ButtonEvent : MonoBehaviour {
    public int id;
    private bool isPause = false;
    private bool isStart = false;

    public void onClick()
    {
        if (!isStart)
        {
            isStart = true;
            DownloadManagerHelper.GetDonwloadManager().StartDownload(id);
            return;
        }
        if (isPause)
        {
            isPause = false;
            transform.Find("Text").GetComponent<Text>().text = "暂停";
            DownloadManagerHelper.GetDonwloadManager().PauseDownload(id, isPause);
        }
        else
        {
            isPause = true;
            transform.Find("Text").GetComponent<Text>().text = "开始";
            DownloadManagerHelper.GetDonwloadManager().PauseDownload(id, isPause);
        }
    }
}
