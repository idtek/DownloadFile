using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;

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

        public DownloadHandlerRange(string path,UnityWebRequest request)
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

        protected override void ReceiveContentLength(int contentLength)
        {
            TotalFileSize = contentLength;
        }

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
