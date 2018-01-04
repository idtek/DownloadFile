using UnityEngine;

namespace DownloadFileNW
{
    public class DownloadManagerHelper:MonoBehaviour
    {
        private static DownloadManagerHelper instance = null;

        public static DownloadManager GetDonwloadManager()
        {
            return DownloadManager.Instance;
        }

        public static DownloadManagerHelper GetDownloadManagerHelper()
        {
            if (instance == null)
            {
                instance = (new GameObject("DownloadManagerHelper")).AddComponent<DownloadManagerHelper>();
            }
            return instance;
        }
    }
}
