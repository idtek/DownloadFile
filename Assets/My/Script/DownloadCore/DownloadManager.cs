using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownloadFileNW
{
    public class DownloadManager : MonoBehaviour
    {
        Dictionary<int, Download> Downloads;//保存对Download对象的引用
        Dictionary<int, string[]> UrlAndName;//保存正在下载的Url,下载地址,名字
        Dictionary<int, string> Errors;//保存下载已经完成的，但是出现错误的Download对象的错误信息
        int Id = 0;

        void Awake()
        {
            Downloads = new Dictionary<int, Download>();
            UrlAndName = new Dictionary<int, string[]>();
            Errors = new Dictionary<int, string>();
        }

        /// <summary>
        /// 添加一项下载任务到下载管理器中
        /// </summary>
        /// <param name="url">要下载的文件的URL</param>
        /// <param name="savePath">要下载文件保存的路径</param>
        /// <param name="isDelete">是否覆盖同路径下同名文件,默认不覆盖</param>
        /// <param name="httpVerbType">向服务器发起请求的方法，默认为Get</param>
        /// <param name="completed">完成下载后回调事件</param>
        /// <param name="saveName">下载文件名称,不指定该值时,将根据HttpResponse头信息或URL来决定文件名称</param>
        /// <returns>返回该下载任务的ID号</returns>
        public int AddDownload(string url, string savePath, bool isDelete = false, HttpVerbType httpVerbType = HttpVerbType.kHttpVerbGET, System.Action<Download> completed = null, string saveName = null)
        {
            Id++;
            foreach (var item in UrlAndName)
            {
                if (saveName == null)
                {
                    if (item.Value[0] == url && item.Value[1] == savePath)
                    {
                        string msg = "该文件已经在下载了，请勿重复下载";
                        Debug.LogError(msg);
                        Errors.Add(Id, msg);
                        return Id;
                    }
                }
                else
                {
                    if (item.Value[2] == saveName && item.Value[1] == savePath)
                    {
                        string msg = "该文件已经在下载了，请勿重复下载";
                        Debug.LogError(msg);
                        Errors.Add(Id, msg);
                        return Id;
                    }
                }
            }
            Download download = new Download(url, savePath, isDelete, httpVerbType, saveName);
            download.Completed += DownloadCompletedError;
            if (completed!=null)
            {
                download.Completed += completed;
            }
            download.Completed += DownloadCompletedClean;
            download.StartDownload();
            Downloads.Add(Id, download);
            UrlAndName.Add(Id, new string[3] { url,savePath,saveName});
            return Id;
        }

        /// <summary>
        /// 获取指定ID的下载器的下载速度
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>下载速度，如果指定ID对应的下载器不存在时返回0</returns>
        public float DownloadSpeed(int id)
        {
            if (Downloads.ContainsKey(id))
            {
                return Downloads[id].DownloadSpeed;
            }
            return 0;
        }

        /// <summary>
        /// 获取指定ID的下载器的下载进度，进度[0,1]之间的一个数
        /// </summary>
        /// <param name="ID">ID值</param>
        /// <returns>下载进度，如果指定ID对应的下载器不存在时返回1</returns>
        public float DecimalProgress(int id)
        {
            if (Downloads.ContainsKey(id))
            {
                return Downloads[id].DecimalProgress;
            }
            return 1;
        }

        /// <summary>
        /// 百分比进度，格式:00.00%
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>下载进度，如果指定ID对应的下载器不存在时返回100.00%</returns>
        public string PercentageProgress(int id)
        {
            if (Downloads.ContainsKey(id))
            {
                return Downloads[id].PercentageProgress;
            }
            return "100.00%";
        }

        /// <summary>
        /// 判断指定ID对应的下载器是否下载完成
        /// </summary>
        /// <param name="id">ID值</param>
        /// <returns>完成返回true,如果没有找到对应下载器也返回true</returns>
        public bool IsDone(int id)
        {
            if (Downloads.ContainsKey(id))
            {
                return Downloads[id].IsDone;
            }
            return true;
        }

        /// <summary>
        /// 根据Download对象得到对应的ID值，返回0，代表没有找到
        /// </summary>
        /// <param name="download">Download对象</param>
        /// <returns>ID值</returns>
        public int GetID(Download download)
        {
            foreach (var item in Downloads)
            {
                if (item.Value == download)
                {
                    return item.Key;
                }
            }
            return 0;
        }

        /// <summary>
        /// 得到对应ID的下载器下载错误信息
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
        /// 下载结束后进行资源清理
        /// </summary>
        /// <param name="download"></param>
        private void DownloadCompletedClean(Download download)
        {
            int count = GetID(download);
            Downloads.Remove(count);
            UrlAndName.Remove(count);
        }
    }
}
