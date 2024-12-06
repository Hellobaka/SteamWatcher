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
        /// steamId, (appId, name)
        /// </summary>
        public static Dictionary<string, (string, string)> Playing { get; set; } = [];

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
            CheckTimer_Elapsed(null, null);
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
                    if (Playing.TryGetValue(item.steamid, out var playing) && string.IsNullOrEmpty(item.gameid) && item.gameid != playing.Item1)
                    {
                        // not playing
                        sb.AppendLine(string.Format(AppConfig.ReplyNotPlaying, item.personaname, playing.Item2));
                        Playing.Remove(item.steamid);
                        continue;
                    }
                    if (Playing.TryGetValue(item.steamid, out playing))
                    {
                        if (playing.Item1 != item.steamid)
                        {
                            // playing changed
                            Playing[item.steamid] = (item.steamid, item.gameextrainfo);
                            sb.AppendLine(string.Format(AppConfig.ReplyPlayingChanged, item.personaname, item.gameextrainfo));
                            continue;
                        }
                    }
                    else
                    {
                        // playing
                        if (!string.IsNullOrEmpty(item.gameid))
                        {
                            Playing.Add(item.steamid, (item.steamid, item.gameextrainfo));
                            sb.AppendLine(string.Format(AppConfig.ReplyPlaying, item.personaname, item.gameextrainfo));
                            continue;
                        }
                    }
                }

                if (FirstFetch)
                {
                    FirstFetch = false;
                    return;
                }
                var result = sb.ToString();
                if (!string.IsNullOrEmpty(result))
                {
                    PlayingChanged?.Invoke(sb.ToString(), null);
                }
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