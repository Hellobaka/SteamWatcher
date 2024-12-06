using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.SteamAPI
{
    public class Monitors
    {
        public Monitors()
        {
            Instance = this;
        }

        public event Action<string, string> PlayingChanged;

        public static Monitors Instance { get; private set; }

        private Timer CheckTimer { get; set; }

        private bool ElapsedHandling { get; set; }

        private bool FirstFetch { get; set; } = true;

        /// <summary>
        /// steamId, appId
        /// </summary>
        private Dictionary<string, string> Playing { get; set; } = [];

        public void StartCheckTimer()
        {
            if (CheckTimer != null)
            {
                return;
            }
            CheckTimer = new Timer();
            CheckTimer.Interval = AppConfig.QueryInterval * 1000;
            CheckTimer.AutoReset = true;
            CheckTimer.Elapsed -= CheckTimer_Elapsed;
            CheckTimer.Elapsed += CheckTimer_Elapsed;
            CheckTimer.Start();
        }

        public void StopCheckTimer()
        {
            CheckTimer.Stop();
            CheckTimer = null;
        }

        private async void CheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (ElapsedHandling)
            {
                return;
            }
            ElapsedHandling = true;
            try
            {
                StringBuilder sb = new();
                var summary = await GetPlayerSummary.Get(AppConfig.MonitorPlayers);
                if (summary == null)
                {
                    return;
                }
                foreach (var item in summary.players)
                {
                    GetAppInfo.AppInfo info = null;
                    if (Playing.TryGetValue(item.steamid, out var appId) && string.IsNullOrEmpty(item.gameid))
                    {
                        // not playing
                        info = await GetAppInfo.Get(item.gameid);
                        sb.AppendLine(string.Format(AppConfig.ReplyNotPlaying, item.personaname, info.data.name));
                        continue;
                    }
                    info = await GetAppInfo.Get(item.gameid);
                    if (Playing.TryGetValue(item.steamid, out appId))
                    {
                        if (appId != item.gameid)
                        {
                            // playing changed
                            Playing[item.steamid] = item.gameid;
                            sb.AppendLine(string.Format(AppConfig.ReplyPlayingChanged, item.personaname, info.data.name));
                            continue;
                        }
                    }
                    else
                    {
                        // playing
                        Playing.Add(item.steamid, item.gameid);
                        sb.AppendLine(string.Format(AppConfig.ReplyPlaying, item.personaname, info.data.name));
                        continue;
                    }
                }

                if (FirstFetch)
                {
                    FirstFetch = false;
                    return;
                }
                PlayingChanged?.Invoke(sb.ToString(), null);
            }
            catch (Exception ex)
            {
                MainSave.CQLog.Error("监视时钟", ex.Message + ex.StackTrace);
            }
            finally
            {
                ElapsedHandling = false;
            }
        }
    }
}