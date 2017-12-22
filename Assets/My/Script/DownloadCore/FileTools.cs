using System;
using System.IO;
using UnityEngine;

namespace DownloadFileNW
{
    public static class FileTools
    {
        /// <summary>
        /// 判断指定路径的目录是否存在
        /// </summary>
        /// <param name="path">要测试的路径。</param>
        /// <returns>如果 path 指向现有目录，则为 true；如果该目录不存在或者在尝试确定指定目录是否存在时出错，则为 false。</returns>
        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// 判断指定的文件是否存在。
        /// </summary>
        /// <param name="path">要检查的文件。</param>
        /// <returns>如果调用方具有要求的权限并且 true 包含现有文件的名称，则为 path；否则为 false。 如果 false 为 path（一个无效路径或零长度字符串）,则此方法也将返回 null。 如果调用方不具有读取指定文件所需的足够权限，则不引发异常并且该方法返回 false，这与 path 是否存在无关。</returns>
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        /// <summary>
        /// 返回指定路径字符串的文件名和扩展名。
        /// </summary>
        /// <param name="path">从中获取文件名和扩展名的路径字符串。</param>
        /// <returns>path 中最后一个目录字符后的字符。 如果 path 的最后一个字符是目录或卷分隔符，则此方法返回 String.Empty。 如果 path 为 null，则此方法返回 null。</returns>
        public static string GetFileName(string path)
        {
            try
            {
                return Path.GetFileName(path);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 返回不具有扩展名的指定路径字符串的文件名。
        /// </summary>
        /// <param name="path">文件的路径。</param>
        /// <returns>返回的字符串 GetFileName, ，减去最后的句点 （.） 和之后的所有字符。</returns>
        public static string GetFileNameWithoutExtension(string path)
        {
            try
            {
                return Path.GetFileNameWithoutExtension(path);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw e;
            }
        }

        public static string GetDirectoryName(string path)
        {
            try
            {
                return Path.GetDirectoryName(path);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 在指定路径中创建所有目录和子目录，除非它们已经存在。
        /// </summary>
        /// <param name="path">要创建的目录。</param>
        public static void CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 将两个字符串组合成一个路径。
        /// </summary>
        /// <param name="path1">要组合的第一个路径。</param>
        /// <param name="path2">要组合的第二个路径。</param>
        /// <returns>已组合的路径。 如果指定的路径之一是零长度字符串，则该方法返回其他路径。 如果 path2 包含绝对路径，则该方法返回 path2。</returns>
        public static string PathCombine(string path1,string path2)
        {
            try
            {
                return Path.Combine(path1, path2);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw e;
            }
        }

        /// <summary>
        /// 删除指定的文件。
        /// </summary>
        /// <param name="path">要删除的文件的名称。 该指令不支持通配符。</param>
        public static void DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw e;
            }
        }

        public static void RenameFile(string path,string newName)
        {
            try
            {
                File.Move(path, Path.Combine(GetDirectoryName(path), newName));
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                throw e;
            }
        }
    }
}
