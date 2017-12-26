using UnityEngine;
using UnityEngine.Networking;

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
                if (IsDone)
                {
                    return 0;
                }
                ulong nowDataSize = UnityWebRequest.downloadedBytes;
                float nowTime = Time.time;
                if (nowTime - LastTime == 0)
                {
                    return LastSpeed;
                }
                float speed = (nowDataSize - LastDataSize) / 1024 / (nowTime - LastTime);
                if (double.IsPositiveInfinity(speed))
                {
                    return LastSpeed;
                }
                LastDataSize = nowDataSize;
                LastTime = nowTime;
                LastSpeed = speed;
                return ((int)(speed * 100))/100;
            }
        }

        /// <summary>
        /// 小数进度[0,1]之间的一个数
        /// </summary>
        public float DecimalProgress
        {
            get
            {
                if (IsDone)
                {
                    return 1.0f;
                }
                if (UnityWebRequest.downloadProgress < 0)
                {
                    return 0;
                }
                return UnityWebRequest.downloadProgress;
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
        public string FileSize
        {
            get
            {
                if (IsDone)
                {
                    return null;
                }
                return UnityWebRequest.GetResponseHeader("Content-Length");
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

        #endregion

        #region 构造方法

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="url">要下载的文件的URL</param>
        /// <param name="savePath">要将下载文件保存的路径</param>
        /// <param name="isDelete">如果指定目录下存在同名文件，当该值为true时将删除同名文件，为false时则报错</param>
        /// <param name="httpVerbType">向服务器发起请求的方法，默认为Get</param>
        /// <param name="saveName">下载文件名称,不指定该值时,将根据HttpResponse头信息或URL来决定文件名称</param>
        public Download(string url, string savePath, bool isDelete = false, HttpVerbType httpVerbType = HttpVerbType.kHttpVerbGET, string saveName = null)
        {
            URL = url;
            SavePath = savePath;
            SaveName = saveName;
            IsDelete = isDelete;
            HttpVerbType = httpVerbType;
            string method = null;
            switch (httpVerbType)
            {
                case HttpVerbType.kHttpVerbCREATE:
                    method = UnityWebRequest.kHttpVerbCREATE;
                    break;
                case HttpVerbType.kHttpVerbDELETE:
                    method = UnityWebRequest.kHttpVerbDELETE;
                    break;
                case HttpVerbType.kHttpVerbGET:
                    method = UnityWebRequest.kHttpVerbGET;
                    break;
                case HttpVerbType.kHttpVerbHEAD:
                    method = UnityWebRequest.kHttpVerbHEAD;
                    break;
                case HttpVerbType.kHttpVerbPOST:
                    method = UnityWebRequest.kHttpVerbPOST;
                    break;
                case HttpVerbType.kHttpVerbPUT:
                    method = UnityWebRequest.kHttpVerbPUT;
                    break;
                default:
                    break;
            }
            UnityWebRequest = new UnityWebRequest(URL, method);
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
            IsRange = isRange;
            //如果下载目录不存在，则创建目录及其父目录
            if (!FileTools.DirectoryExists(SavePath))
            {
                FileTools.CreateDirectory(SavePath);
            }
            //如果没有设置文件名称，则通过URL来确定文件名称
            TempFilePath = FileTools.PathCombine(SavePath, SaveName == null ? FileTools.GetFileName(URL).Split('?')[0] : SaveName);
            TempFilePath += ".temp";
            //Note:这里可能会抛出ArgumentException: Failed to create file,通常意味着你试图开着两个Download将同一个
            //文件下载到同一个路径下，它们的名字冲突所引起的
            DownloadHandler downloadHandler = null;
            try
            {
                if (IsRange)
                {
                    DownloadHandlerRange = new DownloadHandlerRange(TempFilePath, UnityWebRequest);
                    downloadHandler = DownloadHandlerRange;
                }
                else
                {
                    DownloadHandlerFile downloadHandlerFile = new DownloadHandlerFile(TempFilePath);
                    downloadHandlerFile.removeFileOnAbort = true;
                    downloadHandler = downloadHandlerFile; ;
                }
            }
            catch (System.ArgumentException e)
            {
                Debug.LogError(e.Message);
                if (!IsSystemError)
                {
                    IsSystemError = true;
                    SystemErrorMsg = "文件名冲突,指定路径的临时文件正在被其他程序占用";
                }
                CompletedFinally();
                return;
            }
            UnityWebRequest.downloadHandler = downloadHandler;
            UnityWebRequest.SendWebRequest().completed += Download_completed;
            LastDataSize = 0;
            LastTime = Time.time;
            LastSpeed = 0.0f;
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
        private HttpVerbType HttpVerbType;//发起Http请求的方法
        private string FilePath;//下载文件的路径
        private string TempFilePath;//下载临时文件的路径
        private bool IsDelete = false;//是否覆盖文件
        private bool Is_Done = false;//是否完成
        private bool isSystemError = false;//是否有系统错误
        private string systemErrorMsg = "";//系统错误时的消息
        private bool isHttpError = false;//是否有Http连接错误
        private long httpErrorCode = -1;//出现Http错误时，该值代表着Http状态码
        private bool IsRange = true;//是否采用断点续传
        private DownloadHandlerRange DownloadHandlerRange = null;
        private ulong LastDataSize;//以下用来某段时间的平均下载速度
        private float LastTime;
        private float LastSpeed;

        #endregion

        #region 私有方法

        /// <summary>
        /// Http连接结束时，判断是否出现Http错误或者系统错误，并且清除资源
        /// </summary>
        /// <param name="obj"></param>
        private void Download_completed(AsyncOperation obj)
        {
            bool isError = false;
            try
            {
                if (UnityWebRequest.isHttpError)
                {
                    isError = true;
                    Debug.LogError("下载失败,Http错误,错误码:" + UnityWebRequest.responseCode.ToString());
                    IsHttpError = true;
                    HttpErrorCode = UnityWebRequest.responseCode;
                }
                if (UnityWebRequest.isNetworkError)
                {
                    isError = true;
                    Debug.LogError("下载失败,系统错误,错误信息:" + UnityWebRequest.error);
                    IsSystemError = true;
                    SystemErrorMsg = UnityWebRequest.error;
                }
                if (isError)
                {
                    return;
                }
            }
            finally
            {
                if (!isError)
                {
                    RenameFile();
                }
                CompletedFinally();
            }

        }

        /// <summary>
        /// 下载结束后，不管成功与否都要执行的操作
        /// </summary>
        private void CompletedFinally()
        {
            if (Completed != null)
            {
                Completed(this);
            }
            Dispose();
            if (DownloadHandlerRange != null)
            {
                DownloadHandlerRange.Dispose();
            }
            UnityWebRequest = null;
            IsDone = true;
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
            UnityWebRequest.Dispose();
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
                    catch (System.IO.IOException e)
                    {
                        if (!IsSystemError)
                        {
                            IsSystemError = true;
                            SystemErrorMsg = "同路径出现同名文件,而且该文件被其他程序占用无法删除";
                        }
                        Debug.LogError(SystemErrorMsg);
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
                    Debug.LogError(SystemErrorMsg);
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
                Debug.LogError(SystemErrorMsg);
                return;
            }
            FileTools.RenameFile(TempFilePath, fileName);
        }
        #endregion
    }
}
