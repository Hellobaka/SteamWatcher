using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos
{
    public static class CommonHelper
    {
        public static string GetAppImageDirectory()
        {
            if (!string.IsNullOrEmpty(MainSave.AppDirectory))
            {
                // GetAppDirectory() 为 D:\AMN2\data\plugins\<appId>\,上溯两级到框架根目录
                return Path.GetFullPath(Path.Combine(MainSave.AppDirectory, "..", "..", "data", "image"));
            }
            return Path.Combine(Environment.CurrentDirectory, "data", "image");
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
        public static bool DownloadFile(string url, string path, bool overwrite = false)
        {
            using var http = new HttpClient();
            http.Timeout = TimeSpan.FromSeconds(10);
            try
            {
                if (string.IsNullOrWhiteSpace(url)) return false;
                if (!overwrite && File.Exists(path)) return true;
                var r = http.GetAsync(url);
                r.Result.EnsureSuccessStatusCode();
                byte[] buffer = r.Result.Content.ReadAsByteArrayAsync().Result;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, buffer);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MainSave.Logger?.Warn("下载文件", e.ToString());
                return false;
            }
        }

        public static bool IsValidSteamId(string input)
        {
            // 17位纯数字SteamID
            if (input.Length == 17 && long.TryParse(input, out _))
            {
                return true;
            }

            // 好友码（通常较短的数字）
            if (input.Length < 17 && long.TryParse(input, out _))
            {
                return true;
            }

            return false;
        }

        public static string ConvertFriendCodeToSteamId(string friendCode)
        {
            // Steam好友码转换：76561197960265728 + 好友码 = SteamID
            long baseSteamId = 76561197960265728;
            if (long.TryParse(friendCode, out long friendCodeValue))
            {
                return (baseSteamId + friendCodeValue).ToString();
            }
            return null;
        }
    }
}
