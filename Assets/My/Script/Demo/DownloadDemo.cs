using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DownloadFileNW;

public class DownloadDemo : MonoBehaviour {
    public Text PathText;
    public Text MsgText;
    public Text UrlText;
    public GameObject MaxOption;
    public GameObject msgPrefabs;
    Dictionary<int, GameObject> msgs = new Dictionary<int, GameObject>();
    public GameObject content;
    private float lastTime = 0;

    void Awake()
    {
        ChangeMaxCount();
    }

    public void ChangeMaxCount()
    {
        DownloadManagerHelper.GetDonwloadManager().MaxDownloadCount = MaxOption.GetComponent<Dropdown>().value + 1;
    }

    public void Click()
    {
        DownloadManager downloadManager = DownloadManagerHelper.GetDonwloadManager();
        int id = downloadManager.AddDownload(UrlText.text, PathText.text, true, HttpVerbType.kHttpVerbGET, completed);
        msgs[id] = Instantiate(msgPrefabs, content.transform);
        msgs[id].transform.Find("StartOrPause").GetComponent<ButtonEvent>().id = id;
    }

    void completed(int id)
    {
        string msg = DownloadManagerHelper.GetDonwloadManager().GetErrorMsg(id);
        if (msg == null)
        {
            MsgText.text = "  ID:" + id + " 下载完成";
        }
        else
        {
            MsgText.text = "  ID:" + id + " 下载失败.  " + msg;
        }
        Destroy(msgs[id]);
        msgs.Remove(id);
    }

    void Update()
    {
        if (Time.time - lastTime >= 1.0f)
        {
            lastTime = Time.time;
            foreach (var item in msgs)
            {
                DownloadManager downloadManager = DownloadManagerHelper.GetDonwloadManager();
                switch (downloadManager.Status(item.Key))
                {
                    case DownloadStatus.Downloading:
                        item.Value.transform.Find("ProgressBar").GetComponent<Slider>().value = downloadManager.DecimalProgress(item.Key);
                        item.Value.transform.Find("Text").Find("Progress").GetComponent<Text>().text = downloadManager.PercentageProgress(item.Key);
                        item.Value.transform.Find("Text").Find("Speed").GetComponent<Text>().text = downloadManager.DownloadSpeed(item.Key) + "KB/S";
                        break;
                    case DownloadStatus.Waiting:
                        item.Value.transform.Find("Text").Find("Speed").GetComponent<Text>().text = "等待下载中...";
                        break;
                    case DownloadStatus.Queue:
                        item.Value.transform.Find("Text").Find("Speed").GetComponent<Text>().text = "队列中...";
                        break;
                    case DownloadStatus.Pause:
                        item.Value.transform.Find("Text").Find("Speed").GetComponent<Text>().text = "暂停中...";
                        break;
                    case DownloadStatus.PauseWaiting:
                        item.Value.transform.Find("Text").Find("Speed").GetComponent<Text>().text = "暂停等待中...";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
