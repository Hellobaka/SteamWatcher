using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using me.cqp.luohuaming.SteamWatcher.Sdk.Cqp.Model;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos
{
    public static class CommonHelper
    {
        public static string GetAppImageDirectory()
        {
            var ImageDirectory = Path.Combine(Environment.CurrentDirectory, "data", "image\\");
            return ImageDirectory;
        }

        public static void RemoveNewLine(this StringBuilder stringBuilder)
        {
            if (stringBuilder.Length < Environment.NewLine.Length)
            {
                return;
            }
            stringBuilder.Remove(stringBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="url">网址</param>
        /// <param name="path">目标文件夹</param>
        /// <param name="overwrite">重复时是否覆写</param>
        /// <returns></returns>
        public static bool DownloadFile(string url, string fileName, string path, bool overwrite = false)
        {
            using var http = new HttpClient();
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return false;
                if (!overwrite && File.Exists(Path.Combine(path, fileName))) return true;
                var r = http.GetAsync(url);
                byte[] buffer = r.Result.Content.ReadAsByteArrayAsync().Result;
                Directory.CreateDirectory(path);
                File.WriteAllBytes(Path.Combine(path, fileName), buffer);
                return true;
            }
            catch (Exception e)
            {
                MainSave.CQLog.Warning("下载头像", e);
                return false;
            }
        }
    }
}
