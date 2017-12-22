using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DownloadFileNW
{
    public class DownloadManager : MonoBehaviour
    {
        Dictionary<int, Download> Downloads;
        Dictionary<int, Dictionary<string, string>> UrlAndName;
        private HttpVerbType httpVerbType = HttpVerbType.kHttpVerbGET;//发起Http请求的方法
        private bool isDelete = false;//是否覆盖文件

        /// <summary>
        /// 设置是否覆盖掉同路径下的同名文件，默认不覆盖
        /// </summary>
        public bool IsDelete
        {
            get
            {
                return isDelete;
            }

            set
            {
                isDelete = value;
            }
        }

        /// <summary>
        /// 设置发起Http请求的方法,默认Get方法
        /// </summary>
        public HttpVerbType HttpVerbType
        {
            get
            {
                return httpVerbType;
            }

            set
            {
                httpVerbType = value;
            }
        }

        void Awake()
        {
            Downloads = new Dictionary<int, Download>();
            UrlAndName = new Dictionary<int, Dictionary<string, string>>();
        }

        void AddDownload()
        {

        }

    }
}
