using System.Collections;
using UnityEngine;
#if UNITY_5_4_OR_NEWER
using UnityEngine.Networking;
#else
using UnityEngine.Experimental.Networking;
#endif

namespace DownloadFileNW
{
    /// <summary>
    /// 发起Http请求时的方法
    /// </summary>
    public enum HttpVerbType { kHttpVerbCREATE, kHttpVerbDELETE, kHttpVerbGET, kHttpVerbHEAD, kHttpVerbPOST, kHttpVerbPUT }

    public class Download
    {
#region 事件
        /// <summary>
        /// 下载结束时执行该事件，请注意，对Download对象的所有操作请在该事件里或该事件执行之前，
        /// 当该事件执行结束后，该Download对象资源将被释放，此时请不要在访问该Download对象
        /// </summary>
        public event System.Action<Download> Completed;
#endregion

#region 属性

        /// <summary>
        /// 该属性反应了“一段时间”内的平均下载速度(KB/S),这个“一段时间”的长度取决于你调用该属性的间隔(频率)
        /// </summary>
        public float DownloadSpeed
        {
            get
            {
                return DownloadHandlerRange.Speed;
            }
        }

        /// <summary>
        /// 小数进度[0,1]之间的一个数
        /// </summary>
        public float DecimalProgress
        {
            get
            {
                return DownloadHandlerRange.DownloadProgress;
            }
        }

        /// <summary>
        /// 百分比进度，格式:00.00%
        /// </summary>
        public string PercentageProgress
        {
            get
            {
                int intProgress = (int)(DecimalProgress * 10000);
                return (intProgress / 100).ToString() + "." + (intProgress % 100).ToString() + "%";
            }
        }

        /// <summary>
        /// Note:该属性只能在下载开始后的一帧之后开始调用，如果在刚刚调用StartDownload紧接着在同一帧调用该属性将返回
        /// Null，而且在下载已经完成后调用该属性将返回Null(也即是Completed事件执行完毕后)
        /// </summary>
        public long FileSize
        {
            get
            {
                return fileSize;
            }
        }

        /// <summary>
        /// 判断下载是否已经完成，不管下载成功与否，只要该属性返回True就意味着下载已经完成，此时该对象就不应该再被使用
        /// </summary>
        public bool IsDone
        {
            get
            {
                return Is_Done;
            }

            private set
            {
                Is_Done = value;
            }
        }

        /// <summary>
        /// 判断是否存在系统错误
        /// </summary>
        public bool IsSystemError
        {
            get
            {
                return isSystemError;
            }

            private set
            {
                isSystemError = value;
            }
        }

        /// <summary>
        /// 如果存在系统错误，则该属性返回系统错误的信息
        /// </summary>
        public string SystemErrorMsg
        {
            get
            {
                return systemErrorMsg;
            }

            private set
            {
                systemErrorMsg = value;
            }
        }

        /// <summary>
        /// 判断是否存在Http连接错误
        /// </summary>
        public bool IsHttpError
        {
            get
            {
                return isHttpError;
            }

            private set
            {
                isHttpError = value;
            }
        }

        /// <summary>
        /// 如果存在Http连接错误,该属性返回Http状态码
        /// </summary>
        public long HttpErrorCode
        {
            get
            {
                return httpErrorCode;
            }

            private set
            {
                httpErrorCode = value;
            }
        }

        /// <summary>
        /// 当前下载是否处于暂停状态,暂停为true
        /// </summary>
        public bool IsPause
        {
            get
            {
                return isPause;
            }

            private set
            {
                isPause = value;
            }
        }

#endregion

#region 构造方法

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">要下载的文件的URL</param>
        /// <param name="savePath">要将下载文件保存的路径</param>
        /// <param name="isDelete">如果指定目录下存在同名文件，当该值为true时将删除同名文件，为false时则删除本次下载的文件</param>
        /// <param name="httpVerbType">向服务器发起请求的方法，默认为Get</param>
        /// <param name="saveName">下载文件名称,不指定该值时,将根据HttpResponse头信息或URL来决定文件名称</param>
        public Download(string url, string savePath, bool isDelete = false, HttpVerbType httpVerbType = HttpVerbType.kHttpVerbGET, string saveName = null)
        {
            URL = url;
            SavePath = savePath;
            SaveName = saveName;
            IsDelete = isDelete;
            switch (httpVerbType)
            {
                case HttpVerbType.kHttpVerbCREATE:
                    Method = UnityWebRequest.kHttpVerbCREATE;
                    break;
                case HttpVerbType.kHttpVerbDELETE:
                    Method = UnityWebRequest.kHttpVerbDELETE;
                    break;
                case HttpVerbType.kHttpVerbGET:
                    Method = UnityWebRequest.kHttpVerbGET;
                    break;
                case HttpVerbType.kHttpVerbHEAD:
                    Method = UnityWebRequest.kHttpVerbHEAD;
                    break;
                case HttpVerbType.kHttpVerbPOST:
                    Method = UnityWebRequest.kHttpVerbPOST;
                    break;
                case HttpVerbType.kHttpVerbPUT:
                    Method = UnityWebRequest.kHttpVerbPUT;
                    break;
                default:
                    break;
            }
        }

#endregion

#region 公共方法

        /// <summary>
        /// 开始下载文件
        /// </summary>
        /// <param name="isRange">是否采用断点续传的方式进行下载</param>
        public void StartDownload(bool isRange = true)
        {
            if (IsDone)
            {
                return;
            }
            if (URL == null || "".Equals(URL))
            {
                IsSystemError = true;
                SystemErrorMsg = "URL不能为空";
                CompletedFinally();
                return;
            }
            if (SavePath == null || "".Equals(SavePath))
            {
                IsSystemError = true;
                SystemErrorMsg = "保存路径不能为空";
                CompletedFinally();
                return;
            }
            UnityWebRequest = new UnityWebRequest(URL, Method);
            IsRange = isRange;
            //如果下载目录不存在，则创建目录及其父目录
            if (!FileTools.DirectoryExists(SavePath))
            {
                try
                {
                    FileTools.CreateDirectory(SavePath);
                }
                catch (System.UnauthorizedAccessException)
                {
                    IsSystemError = true;
                    SystemErrorMsg = "没有权限";
                    CompletedFinally();
                    return;
                }
                catch (System.IO.IOException)
                {
                    IsSystemError = true;
                    SystemErrorMsg = "目录路径有误";
                    CompletedFinally();
                    return;
                }
                catch (System.Exception e)
                {
                    IsSystemError = true;
                    SystemErrorMsg = e.Message;
                    CompletedFinally();
                    return;
                }
            }
            //如果没有设置文件名称，则通过URL来确定临时文件名称
            string tempName = null;
            try
            {
                tempName = SaveName == null ? FileTools.GetFileName(URL).Split('?')[0] : SaveName;
            }
            catch (System.ArgumentException)
            {
                //如果无法通过URL获取到下载名称,则通过HashCode来确定名称
                tempName = ((uint)URL.GetHashCode()).ToString();
            }
            TempFilePath = FileTools.PathCombine(SavePath, tempName);
            TempFilePath += ".temp";
            try
            {
                if (!IsRange && FileTools.FileExists(TempFilePath))
                {
                    FileTools.DeleteFile(TempFilePath);
                }
            }
            catch (System.Exception e)
            {
                IsSystemError = true;
                SystemErrorMsg = e.Message;
                CompletedFinally();
                return;
            }
            try
            {
                DownloadHandlerRange = new DownloadHandlerRange(TempFilePath, UnityWebRequest);
                DownloadHandlerRange.StartDownloadEvent += StartDownloadCallBack;
            }
            catch (System.IO.IOException)
            {
                IsSystemError = true;
                SystemErrorMsg = "文件正在被下载";
                CompletedFinally();
                return;
            }
            catch (System.Exception e)
            {
                IsSystemError = true;
                SystemErrorMsg = e.Message;
                CompletedFinally();
                return;
            }
            UnityWebRequest.downloadHandler = DownloadHandlerRange;
#if UNITY_2017_2_OR_NEWER
            UnityWebRequest.SendWebRequest().completed += Download_completed;
#else
            UnityWebRequest.Send();
            DownloadManagerHelper.GetDownloadManagerHelper().StartCoroutine(JudgmentCompleted());
#endif
        }

        /// <summary>
        /// 暂停开关,传入true将暂停下载,传入false将继续下载
        /// </summary>
        /// <param name="on">true暂停,false继续下载</param>
        public void PauseSwitch(bool on)
        {
            if (!IsPause && on)
            {
                IsPause = on;
                Abort();
            }
            else if (IsPause && !on)
            {
                IsPause = on;
                StartDownload(IsRange);
            }
        }

        /// <summary>
        /// 中止下载
        /// </summary>
        public void Abort()
        {
            if (IsDone)
            {
                return;
            }
            UnityWebRequest.Abort();
        }

#endregion

#region 私有字段

        private UnityWebRequest UnityWebRequest = null;
        private string URL = null;//下载文件的地址
        private string SavePath = null;//保存的路径
        private string SaveName = null;//保存的文件名
        private string FilePath;//下载文件的路径
        private string TempFilePath;//下载临时文件的路径
        private long fileSize = 0;//文件的大小
        private bool IsDelete = false;//是否覆盖文件
        private bool Is_Done = false;//是否完成
        private bool isSystemError = false;//是否有系统错误
        private string systemErrorMsg = "";//系统错误时的消息
        private bool isHttpError = false;//是否有Http连接错误
        private long httpErrorCode = -1;//出现Http错误时，该值代表着Http状态码
        private bool IsRange = true;//是否采用断点续传
        private string Method;//发起Http请求的方法
        private DownloadHandlerRange DownloadHandlerRange = null;
        private bool isPause = false;
#endregion

#region 私有方法

        /// <summary>
        /// 得到响应头信息时被回调
        /// </summary>
        private void StartDownloadCallBack()
        {
            fileSize = DownloadHandlerRange.FileSize;
        }

        /// <summary>
        /// Http连接结束时，判断是否出现Http错误或者系统错误，并且清除资源
        /// </summary>
        /// <param name="obj"></param>
        private void Download_completed(AsyncOperation obj)
        {
            bool isError = false;
            if (!IsPause)
            {
#if UNITY_2017_1_OR_NEWER
                if (UnityWebRequest.isHttpError)
                {
                    isError = true;
                    IsHttpError = true;
                    HttpErrorCode = UnityWebRequest.responseCode;
                }
                if (UnityWebRequest.isNetworkError)
                {
                    isError = true;
                    IsSystemError = true;
                    SystemErrorMsg = UnityWebRequest.error;
                }
#else
                if (UnityWebRequest.isError)
                {
                    isError = true;
                    IsSystemError = true;
                    SystemErrorMsg = UnityWebRequest.error;
                }
#endif

            }
            if (!isError && !IsPause)
            {
                RenameFile();
            }
            CompletedFinally();
        }

        /// <summary>
        /// 下载结束后，不管成功与否都要执行的操作
        /// </summary>
        private void CompletedFinally()
        {
            if (Completed != null && !IsPause)
            {
                Completed(this);
            }
            Dispose();
            if (DownloadHandlerRange != null)
            {
                DownloadHandlerRange.Dispose();
            }
            UnityWebRequest = null;
            if (!IsPause)
            {
                IsDone = true;
            }
            if (!IsRange)
            {
                FileTools.DeleteFile(TempFilePath);
            }
        }

        /// <summary>
        /// 清除资源
        /// </summary>
        private void Dispose()
        {
            if (UnityWebRequest != null)
            {
                UnityWebRequest.Dispose();
            }
        }

        /// <summary>
        /// 将文件的临时文件名修改成正式文件名
        /// </summary>
        private void RenameFile()
        {
            string fileName = FileTools.GetFileNameWithoutExtension(TempFilePath);
            //如何调用者没有设置保存文件的名称，则去查找服务器响应头信息里面是否包含文件名信息
            if (SaveName == null)
            {
                string contentDisposition = UnityWebRequest.GetResponseHeader("Content-Disposition");
                if (contentDisposition != null)
                {
                    foreach (var item in contentDisposition.Split(';'))
                    {
                        if (item.IndexOf("filename") != -1)
                        {
                            fileName = item.Split('"')[1];
                        }
                    }
                }
            }
            //将新名字应用到文件路径上
            FilePath = FileTools.PathCombine(FileTools.GetDirectoryName(TempFilePath), fileName);
            //如果指定路径已经存在新名字了，则根据IsDelete的值来决定是覆盖还是报错
            if (FileTools.FileExists(FilePath))
            {
                if (IsDelete)
                {
                    try
                    {
                        FileTools.DeleteFile(FilePath);
                    }
                    catch (System.IO.IOException)
                    {
                        if (!IsSystemError)
                        {
                            IsSystemError = true;
                            SystemErrorMsg = "同路径出现同名文件,而且该文件被其他程序占用无法删除";
                        }
                        return;
                    }
                }
                else
                {
                    if (!IsSystemError)
                    {
                        IsSystemError = true;
                        SystemErrorMsg = "目标目录已经存在同名文件!";
                    }
                    try
                    {
                        FileTools.DeleteFile(TempFilePath);
                    }
                    catch (System.Exception e)
                    {
                        if (!IsSystemError)
                        {
                            IsSystemError = true;
                            SystemErrorMsg = e.Message;
                        }
                    }
                    return;
                }
            }
            else if (FileTools.DirectoryExists(FilePath))
            {
                if (!IsSystemError)
                {
                    IsSystemError = true;
                    SystemErrorMsg = "目标目录已经存在同名文件夹!";
                }
                return;
            }
            FileTools.RenameFile(TempFilePath, fileName);
        }

#if !UNITY_2017_2_OR_NEWER
        IEnumerator JudgmentCompleted()
        {
            while (!UnityWebRequest.isDone)
            {
                yield return 0;
            }
            Download_completed(null);
        }
#endif
        #endregion
    }
}
