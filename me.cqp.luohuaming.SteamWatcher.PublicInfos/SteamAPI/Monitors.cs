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

        public event Action<List<MonitorNoticeItem>> PlayingChanged;

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
                List<MonitorNoticeItem> notices = [];
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
                        notices.Add(new MonitorNoticeItem
                        {
                            GameName = playing.Item2,
                            NoticeType = NoticeType.NotPlayed,
                            SteamID = item.steamid,
                            PlayerName = item.personaname,
                            AvatarUrl = item.avatarfull
                        });
                        Playing.Remove(item.steamid);
                        continue;
                    }
                    if (Playing.TryGetValue(item.steamid, out playing))
                    {
                        if (playing.Item1 != item.steamid)
                        {
                            // playing changed
                            Playing[item.steamid] = (item.steamid, item.gameextrainfo);
                            notices.Add(new MonitorNoticeItem
                            {
                                GameName = item.gameextrainfo,
                                NoticeType = NoticeType.PlayChanged,
                                SteamID = item.steamid,
                                PlayerName = item.personaname,
                                AvatarUrl = item.avatarfull
                            });
                            continue;
                        }
                    }
                    else
                    {
                        // playing
                        if (!string.IsNullOrEmpty(item.gameid))
                        {
                            Playing.Add(item.steamid, (item.steamid, item.gameextrainfo));
                            notices.Add(new MonitorNoticeItem
                            {
                                GameName = item.gameextrainfo,
                                NoticeType = NoticeType.Playing,
                                SteamID = item.steamid,
                                PlayerName = item.personaname,
                                AvatarUrl = item.avatarfull
                            });
                            continue;
                        }
                    }
                }

                if (FirstFetch)
                {
                    FirstFetch = false;
                    return;
                }
                if (notices.Count > 0)
                {
                    PlayingChanged?.Invoke(notices);
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