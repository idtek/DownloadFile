using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

namespace DownloadFileNW
{
    public class DownloadRange : DownloadHandlerScript
    {
        private string Path;
        private string ConfigPath;
        private bool IsFirst = true;
        private ulong DataSize = 0;
        private ulong lastIndex = 0;
        private int count = 0;
        private FileStream stream;
        private bool IsCompleted = false;

        public ulong LastIndex
        {
            get
            {
                return lastIndex;
            }
            private set
            {
                lastIndex = value;
            }
        }

        public DownloadRange(string path,UnityWebRequest request)
        {
            Path = path;
            ConfigPath = Path + ".config";
            if (FileTools.FileExists(ConfigPath))
            {
                LastIndex = ulong.Parse(FileTools.ReadFileUTf8(ConfigPath));
            }
            stream = new FileStream(path, FileMode.Append, FileAccess.Write);
        }

        public void DownloadRange_completed(AsyncOperation obj)
        {
            Debug.LogError("结束了");
            LastIndex += DataSize;
            if (IsCompleted)
            {
                if (FileTools.FileExists(ConfigPath))
                {
                    FileTools.DeleteFile(ConfigPath);
                }
            }
            else
            {
                FileTools.WriteFileUtf8Create(ConfigPath, LastIndex.ToString());
            }
            Debug.Log(DataSize);
            stream.Close();
        }

        protected override void CompleteContent()
        {
            IsCompleted = true;
        }

        protected override float GetProgress()
        {
            return base.GetProgress();
        }

        protected override void ReceiveContentLength(int contentLength)
        {
            base.ReceiveContentLength(contentLength);
        }

        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            DataSize = DataSize + (ulong)dataLength;

            Debug.Log(++count);
            stream.Write(data, 0, dataLength);
            Debug.Log(dataLength);
            Debug.Log(count+"\n---------------------------");
            return true;
        }
    }
}
