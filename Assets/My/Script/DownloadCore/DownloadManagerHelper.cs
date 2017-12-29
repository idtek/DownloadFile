namespace DownloadFileNW
{
    public class DownloadManagerHelper
    {
        private static DownloadManager DownloadManager = null;
        public static int MaxDownloadCount
        {
            get
            {
                return DownloadManager.MaxDownloadCount;
            }
            set
            {
                DownloadManager.MaxDownloadCount = value;
            }
        }

        public static DownloadManager GetDonwloadManager()
        {
            return DownloadManager.Instance;
        }

    }
}
