using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace DownloadFileNW
{
    public class DownloadHandlerRange : DownloadHandlerScript
    {
        private string Path;
        private FileStream FileStream;
        private UnityWebRequest UnityWebRequest;
        private long LocalFileSize = 0;
        private long TotalFileSize = 0;
        private long CurFileSize = 0;

        //使用1MB的缓存,在补丁2017.2.1p1中对DownloadHandlerScript的优化中,目前最大传入数据量也仅仅是1024*1024,再多也没用
        public DownloadHandlerRange(string path,UnityWebRequest request):base(new byte[1024*1024])
        {
            Path = path;
            FileStream = new FileStream(Path, FileMode.Append, FileAccess.Write);
            if (FileTools.FileExists(Path))
            {
                LocalFileSize = new FileInfo(Path).Length;
            }
            UnityWebRequest = request;
            UnityWebRequest.SetRequestHeader("Range", "bytes=" + LocalFileSize + "-");
        }

        new public void Dispose()
        {
            if (FileStream != null)
            {
                FileStream.Close();
                FileStream = null;
            }
        }

        protected override void CompleteContent()
        {
            base.CompleteContent();
            if (FileStream != null)
            {
                FileStream.Flush();
                FileStream.Close();
                FileStream = null;
            }
        }

        protected override byte[] GetData()
        {
            return null;
        }

        protected override float GetProgress()
        {
            return TotalFileSize == 0 ? 0 : ((float)(CurFileSize + LocalFileSize)) / (TotalFileSize + LocalFileSize);
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
        }

        private int last = 0;
        //在2017.3.0(包括该版本)以下的正式版本中存在一个性能上的问题
        //该回调方法有性能上的问题,每次传入的数据量最大不会超过65536(2^16)个字节,不论缓存区有多大
        //在下载速度中的体现,大约相当于每秒下载速度不会超过3.8MB/S
        //这个问题在 "补丁2017.2.1p1" 版本中被优化(2017.12.21发布)(https://unity3d.com/cn/unity/qa/patch-releases/2017.2.1p1)
        //(965165) - Web: UnityWebRequest: improve performance for DownloadHandlerScript.
        //优化后,每次传入数据量最大不会超过1048576(2^20)个字节(1MB),基本满足下载使用
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if (data == null || dataLength < 1)
            {
                return false;
            }
            FileStream.Write(data, 0, dataLength);
            FileStream.Flush();
            CurFileSize += dataLength;
            return true;
        }
    }
}
