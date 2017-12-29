using System.Collections.Generic;

namespace DownloadFileNW
{
    /// <summary>
    /// Downloading:下载中(正在下载) Waiting:等待中(即将开始下载,但是下载位置不够) Queue:队列中(还没开始的任务) Pause(暂停的任务) PauseWait(暂停的任务重新开始下载,但是下载位置不够)
    /// </summary>
    public enum DownloadStatus { Downloading,Waiting, Queue,Pause,PauseWaiting}
    public class DownloadManager
    {
        #region 私有字段
        private static DownloadManager instance=null;//单例模式
        private Dictionary<int, Download> Downloads;//保存对Download对象的引用
        private Dictionary<int, string> Errors;//保存下载已经完成的，但是出现错误的Download对象的错误信息
        private Dictionary<int, DownloadStatus> DownloadQueue;//下载队列
        private Dictionary<int, System.Action<int>> Completeds;
        private int MaxCount = 5;//最大下载数量
        private int CurDonwloadCount = 0;//当前正在下载的数量
        private int id = 0;//对ID的计数
        private bool isRange = true;
        #endregion

        #region 属性
        /// <summary>
        /// 最大下载数量,默认为5
        /// </summary>
        public int MaxDownloadCount
        {
            get
            {
                return MaxCount;
            }

            set
            {
                MaxCount = value;
                CheckDownloadQueue();
            }
        }

        /// <summary>
        /// 是否启用断点续传,默认为true
        /// </summary>
        public bool IsRange
        {
            get
            {
                return isRange;
            }

            set
            {
                isRange = value;
            }
        }

        /// <summary>
        /// 得到DownloadManager实例
        /// </summary>
        public static DownloadManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DownloadManager();
                }
                return instance;
            }
        }
        #endregion

        #region 构造方法
        private DownloadManager()
        {
            Downloads = new Dictionary<int, Download>();
            Errors = new Dictionary<int, string>();
            DownloadQueue = new Dictionary<int, DownloadStatus>();
            Completeds = new Dictionary<int, System.Action<int>>();
        }
        #endregion

        /// <summary>
        /// 添加一项下载任务到下载管理器中,该下载任务将处于队列中状态,调用StartDownload将变为等待中
        /// </summary>
        /// <param name="url">要下载的文件的URL</param>
        /// <param name="savePath">要下载文件保存的路径</param>
        /// <param name="isDelete">如果下载的文件已经存在,是否覆盖,默认不覆盖</param>
        /// <param name="httpVerbType">向服务器发起请求的方法，默认为Get</param>
        /// <param name="completed">完成下载后回调事件</param>
        /// <param name="saveName">下载文件名称,不指定该值时,将根据HttpResponse头信息或URL来决定文件名称</param>
        /// <returns>返回该下载任务的ID号</returns>
        public int AddDownload(string url, string savePath, bool isDelete = false, HttpVerbType httpVerbType = HttpVerbType.kHttpVerbGET, System.Action<int> completed = null, string saveName = null)
        {
            id++;
            Download download = new Download(url, savePath, isDelete, httpVerbType, saveName);
            download.Completed += DownloadCompletedError;
            if (completed != null)
            {
                Completeds[id] = completed;
                download.Completed += UserCompleted;
            }
            download.Completed += DownloadCompletedClean;
            Downloads.Add(id, download);
            DownloadQueue.Add(id, DownloadStatus.Queue);
            return id;
        }

        /// <summary>
        /// 将指定ID的下载任务开始下载,下载任务的状态由Queue变为Waiting
        /// </summary>
        /// <param name="id"></param>
        public void StartDownload(int id)
        {
            if (InDownloadQueue(id) && DownloadQueue[id]==DownloadStatus.Queue)
            {
                DownloadQueue[id] = DownloadStatus.Waiting;
                CheckDownloadQueue();
            }
        }

        /// <summary>
        /// 获取指定ID的下载任务的下载速度
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>下载速度，如果指定ID对应的下载器不存在时返回-1</returns>
        public float DownloadSpeed(int id)
        {
            if (InDownloadQueue(id))
            {
                return Downloads[id].DownloadSpeed;
            }
            return -1;
        }

        /// <summary>
        /// 获取指定ID的下载任务的下载进度，进度[0,1]之间的一个数
        /// </summary>
        /// <param name="ID">ID值</param>
        /// <returns>下载进度，如果指定ID对应的下载器不存在时返回-1</returns>
        public float DecimalProgress(int id)
        {
            if (InDownloadQueue(id))
            {
                return Downloads[id].DecimalProgress;
            }
            return -1;
        }

        /// <summary>
        /// 百分比进度，格式:00.00%
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>下载进度，如果指定ID对应的下载器不存在时返回null</returns>
        public string PercentageProgress(int id)
        {
            if (InDownloadQueue(id))
            {
                return Downloads[id].PercentageProgress;
            }
            return null;
        }

        /// <summary>
        /// 得到对应ID的下载任务下载错误信息,没有错误返回null
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>返回错误信息，如果没有错误则返回Null</returns>
        public string GetErrorMsg(int id)
        {
            if (Errors.ContainsKey(id))
            {
                return Errors[id];
            }
            return null;
        }

        /// <summary>
        /// 删除指定ID所对应的下载任务(包括未开始但存在下载列表中的)
        /// </summary>
        /// <param name="id">指定ID</param>
        public void DeleteDownload(int id)
        {
            if (InDownloadQueue(id))
            {
                if (DownloadQueue[id] == DownloadStatus.Downloading)
                {
                    Downloads[id].Abort();
                }
                else
                {
                    Downloads.Remove(id);
                    DownloadQueue.Remove(id);
                }
            }
        }

        /// <summary>
        /// 删除所有下载任务(包括未开始但存在下载列表中的)
        /// </summary>
        public void DeleteAllDownload()
        {
            List<int> downloading = new List<int>();
            List<int> downloadWait = new List<int>();
            foreach (var item in DownloadQueue)
            {
                if (item.Value == DownloadStatus.Downloading)
                {
                    downloading.Add(item.Key);
                }
                else
                {
                    downloadWait.Add(item.Key);
                }
            }
            foreach (var item in downloadWait)
            {
                DeleteDownload(item);
            }
            foreach (var item in downloading)
            {
                DeleteDownload(item);
            }
        }

        /// <summary>
        /// 暂停开关,传入true将暂停下载,传入false将继续下载
        /// </summary>
        /// <param name="on">true暂停,false继续下载</param>
        public void PauseDownload(int id , bool on)
        {
            if (InDownloadQueue(id))
            {
                if (DownloadQueue[id] == DownloadStatus.Downloading && on)
                {
                    DownloadQueue[id] = DownloadStatus.Pause;
                    Downloads[id].PauseSwitch(on);
                    CurDonwloadCount--;
                    CheckDownloadQueue();
                }
                if (DownloadQueue[id] == DownloadStatus.Pause && !on)
                {
                    DownloadQueue[id] = DownloadStatus.PauseWaiting;
                    CheckDownloadQueue();
                }
            }
        }

        /// <summary>
        /// 获得指定ID下载下载任务的状态,下载中,等待中,队列中,暂停中,暂停等待中
        /// </summary>
        /// <param name="id">指定的ID</param>
        /// <returns>DownloadStatus,指定ID不存在时返回-1</returns>
        public DownloadStatus Status(int id)
        {
            if (InDownloadQueue(id))
            {
                return DownloadQueue[id];
            }
            return (DownloadStatus) (-1);
        }

        /// <summary>
        /// 判断指定ID的下载任务是否存在下载列表中
        /// </summary>
        /// <param name="id">指定的ID</param>
        /// <returns>存在返回true,不存在返回false</returns>
        private bool InDownloadQueue(int id)
        {
            return DownloadQueue.ContainsKey(id);
        }

        /// <summary>
        /// 根据Download对象得到对应的ID值，返回-1，代表没有找到
        /// </summary>
        /// <param name="download">Download对象</param>
        /// <returns>ID值</returns>
        private int GetID(Download download)
        {
            foreach (var item in Downloads)
            {
                if (item.Value == download)
                {
                    return item.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// 下载结束后进行错误处理
        /// </summary>
        /// <param name="download"></param>
        private void DownloadCompletedError(Download download)
        {
            int count = GetID(download);
            string msg = null;
            if (download.IsHttpError)
            {
                msg = "Http错误,错误状态码:" + download.HttpErrorCode.ToString();
            }
            if (download.IsSystemError)
            {
                msg = msg == null ? "系统错误，错误信息:" + download.SystemErrorMsg : msg + "|" + "系统错误，错误信息:" + download.SystemErrorMsg;
            }
            if (msg != null)
            {
                Errors.Add(count, msg);
            }
        }

        /// <summary>
        /// 下载结束后对调用者进行回调
        /// </summary>
        /// <param name="download"></param>
        private void UserCompleted(Download download)
        {
            int id = GetID(download);
            Completeds[id](id);
            Completeds.Remove(id);
        }

        /// <summary>
        /// 下载结束最后进行资源清理
        /// </summary>
        /// <param name="download"></param>
        private void DownloadCompletedClean(Download download)
        {
            int id = GetID(download);
            Downloads.Remove(id);
            DownloadQueue.Remove(id);
            CurDonwloadCount--;
            CheckDownloadQueue();//下载完成后检测下载列表中是否有未开始的下载任务
        }

        /// <summary>
        /// 如果当前下载列表中有还未开始的下载任务,且还有空置的下载位置则启动新的下载
        /// </summary>
        private void CheckDownloadQueue()
        {
            if (CurDonwloadCount != DownloadQueue.Count && CurDonwloadCount < MaxDownloadCount)
            {
                List<int> needDownloadId = new List<int>();
                foreach (var item in DownloadQueue)
                {
                    if (item.Value == DownloadStatus.Waiting || item.Value == DownloadStatus.PauseWaiting)
                    {
                        CurDonwloadCount++;
                        needDownloadId.Add(item.Key);
                        if (CurDonwloadCount >= MaxDownloadCount)
                        {
                            break;
                        }
                    }
                }
                foreach (var item in needDownloadId)
                {
                    if (DownloadQueue[item] == DownloadStatus.Waiting)
                    {
                        DownloadQueue[item] = DownloadStatus.Downloading;
                        Downloads[item].StartDownload(IsRange);
                    }
                    else if (DownloadQueue[item] == DownloadStatus.PauseWaiting)
                    {
                        DownloadQueue[item] = DownloadStatus.Downloading;
                        Downloads[item].PauseSwitch(false);
                    }
                }
            }
            else if (CurDonwloadCount > MaxDownloadCount)
            {
                List<int> needPauseId = new List<int>();
                foreach (var item in DownloadQueue)
                {
                    if (item.Value == DownloadStatus.Downloading)
                    {
                        needPauseId.Add(item.Key);
                    }
                }
                int idleCount = CurDonwloadCount - MaxDownloadCount;
                for (int i = needPauseId.Count - 1, j = 0; j < idleCount && i>=0; j++, i--)
                {
                    CurDonwloadCount--;
                    Downloads[needPauseId[i]].PauseSwitch(true);
                    DownloadQueue[needPauseId[i]] = DownloadStatus.PauseWaiting;
                }
            }
        }
    }
}
