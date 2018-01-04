using UnityEngine.Networking;
using System.IO;

namespace DownloadFileNW
{
    public class DownloadHandlerRange : DownloadHandlerScript
    {
        public event System.Action StartDownloadEvent;

        #region 属性
        /// <summary>
        /// 下载速度,单位:KB/S 保留两位小数
        /// </summary>
        public float Speed
        {
            get
            {
                return ((int)(DownloadSpeed / 1024 * 100)) / 100.0f;
            }
        }

        /// <summary>
        /// 文件的总大小
        /// </summary>
        public long FileSize
        {
            get
            {
                return TotalFileSize;
            }
        }

        /// <summary>
        /// 下载进度[0,1]
        /// </summary>
        public float DownloadProgress
        {
            get
            {
                return GetProgress();
            }
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 使用1MB的缓存,在补丁2017.2.1p1中对DownloadHandlerScript的优化中,目前最大传入数据量也仅仅是1024*1024,再多也没用
        /// </summary>
        /// <param name="path">文件保存的路径</param>
        /// <param name="request">UnityWebRequest对象,用来获文件大小,设置断点续传的请求头信息</param>
        public DownloadHandlerRange(string path,UnityWebRequest request):base(new byte[1024*1024])
        {
            Path = path;
            FileStream = new FileStream(Path, FileMode.Append, FileAccess.Write);
            UnityWebRequest = request;
            LocalFileSize = FileTools.FileSize(Path);
            CurFileSize = LocalFileSize;
            UnityWebRequest.SetRequestHeader("Range", "bytes=" + LocalFileSize + "-");
        }

        /// <summary>
        /// 下载完成后清理资源
        /// </summary>
        new public void Dispose()
        {
            DownloadSpeed = 0.0f;
            if (FileStream != null)
            {
                FileStream.Flush();
                FileStream.Dispose();
                FileStream = null;
            }
        }
        #endregion

        #region 私有继承的方法
        /// <summary>
        /// 下载完成后清理资源
        /// </summary>
        protected override void CompleteContent()
        {
            base.CompleteContent();
            DownloadSpeed = 0.0f;
            if (FileStream != null)
            {
                FileStream.Flush();
                FileStream.Dispose();
                FileStream = null;
            }
        }

        protected override byte[] GetData()
        {
            return null;
        }

        /// <summary>
        /// 得到下载进度
        /// </summary>
        /// <returns></returns>
        protected override float GetProgress()
        {
            return TotalFileSize == 0 ? 0 : ((float)CurFileSize) /TotalFileSize;
        }

        protected override string GetText()
        {
            return null;
        }

        //Note:当下载的文件数据大于2G时,该int类型的参数将会数据溢出,所以先自己通过响应头来获取长度,获取不到再使用参数的方式
        protected override void ReceiveContentLength(int contentLength)
        {
            string contentLengthStr = UnityWebRequest.GetResponseHeader("Content-Length");
            if (contentLengthStr != null)
            {
                TotalFileSize = long.Parse(contentLengthStr);
            }
            else
            {
                TotalFileSize = contentLength;
            }
            //这里拿到的下载大小是待下载的文件大小,需要加上本地已下载文件的大小才等于总大小
            TotalFileSize += LocalFileSize;
            LastTime = UnityEngine.Time.time;
            LastDataSize = CurFileSize;
            if (StartDownloadEvent != null)
            {
                StartDownloadEvent();
            }
        }

        //在2017.3.0(包括该版本)以下的正式版本中存在一个性能上的问题
        //该回调方法有性能上的问题,每次传入的数据量最大不会超过65536(2^16)个字节,不论缓存区有多大
        //在下载速度中的体现,大约相当于每秒下载速度不会超过3.8MB/S
        //这个问题在 "补丁2017.2.1p1" 版本中被优化(2017.12.21发布)(https://unity3d.com/cn/unity/qa/patch-releases/2017.2.1p1)
        //(965165) - Web: UnityWebRequest: improve performance for DownloadHandlerScript.
        //优化后,每次传入数据量最大不会超过1048576(2^20)个字节(1MB),基本满足下载使用
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength == 0)
            {
                return false;
            }
            FileStream.Write(data, 0, dataLength);
            CurFileSize += dataLength;
            //统计下载速度
            if (UnityEngine.Time.time - LastTime >= 1.0f)
            {
                DownloadSpeed = (CurFileSize - LastDataSize) / (UnityEngine.Time.time - LastTime);
                LastTime = UnityEngine.Time.time;
                LastDataSize = CurFileSize;
            }
            return true;
        }
        
        ~DownloadHandlerRange()
        {
            if (FileStream != null)
            {
                FileStream.Flush();
                FileStream.Dispose();
                FileStream = null;
            }
        }
        #endregion

        #region 私有字段
        private string Path;//文件保存的路径
        private FileStream FileStream;
        private UnityWebRequest UnityWebRequest;
        private long LocalFileSize = 0;//本地已经下载的文件的大小
        private long TotalFileSize = 0;//文件的总大小
        private long CurFileSize = 0;//当前的文件大小
        private float LastTime = 0;//用作下载速度的时间统计
        private float LastDataSize = 0;//用来作为下载速度的大小统计
        private float DownloadSpeed = 0;//下载速度,单位:Byte/S
        #endregion
    }
}
