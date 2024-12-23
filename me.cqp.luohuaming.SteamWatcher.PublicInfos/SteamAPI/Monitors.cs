using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static me.cqp.luohuaming.SteamWatcher.PublicInfos.MonitorNoticeItem;

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
        /// steamId, MonitorItem
        /// </summary>
        public static Dictionary<string, MonitorItem> Playing { get; set; } = [];

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
                if (AppConfig.MonitorPlayers.Count == 0)
                {
                    return;
                }
                var summary = await GetPlayerSummary.Get(AppConfig.MonitorPlayers);
                if (summary == null)
                {
                    return;
                }
                foreach (var item in summary.players)
                {
                    if (Playing.TryGetValue(item.steamid, out var playing)
                        && string.IsNullOrEmpty(item.gameid)
                        && item.gameid != playing.AppId)
                    {
                        // not playing
                        notices.Add(new MonitorNoticeItem
                        {
                            GameName = playing.GameName,
                            NoticeType = NoticeType.NotPlayed,
                            SteamID = item.steamid,
                            PlayerName = item.personaname,
                            AvatarUrl = item.avatarfull,
                            Extra = AppConfig.EnableSessionDurationNotice ? $"，游玩时间 {(DateTime.Now - playing.StartTime).TotalMinutes:f1} 分钟" : ""
                        });
                        Playing.Remove(item.steamid);
                        continue;
                    }
                    if (Playing.TryGetValue(item.steamid, out playing))
                    {
                        if (playing.AppId != item.gameid)
                        {
                            // playing changed
                            playing.AppId = item.gameid;
                            playing.GameName = item.gameextrainfo;
                            playing.StartTime = DateTime.Now;
                            playing.Achievements = await FetchPlayerAchievementList(item.steamid, item.gameid);

                            notices.Add(new MonitorNoticeItem
                            {
                                GameName = item.gameextrainfo,
                                NoticeType = NoticeType.PlayChanged,
                                SteamID = item.steamid,
                                PlayerName = item.personaname,
                                AvatarUrl = item.avatarfull
                            });
                        }
                        if (AppConfig.EnableAchievementNotice)
                        {
                            var achievements = await FetchPlayerAchievementList(item.steamid, item.gameid);
                            if (achievements != null)
                            {
                                foreach (var achievement in achievements)
                                {
                                    // get achievement
                                    if (playing.Achievements.Any(x => x.apiname != achievement.apiname))
                                    {
                                        var achievementDetail = await GetAppAchievements.Get(item.gameid, achievement.apiname);
                                        if (achievementDetail != null)
                                        {
                                            var notice = new MonitorNoticeItem
                                            {
                                                AchievementDescription = achievementDetail.description,
                                                NoticeType = NoticeType.GetAchievement,
                                                AppID = item.gameid,
                                                AchievementID = achievementDetail.name,
                                                AchievementName = achievementDetail.displayName,
                                                AvatarUrl = achievementDetail.icon,
                                                PlayerName = item.personaname
                                            };
                                            var percent = await GetGlobalAchievementStat.Get(item.gameid, achievement.apiname);
                                            notice.Extra = percent > 0 ? $"全球解锁率：{percent:f1}%" : "";
                                            notices.Add(notice);
                                        }
                                    }
                                }
                            }
                            playing.Achievements = achievements;
                        }
                    }
                    else
                    {
                        // playing
                        if (!string.IsNullOrEmpty(item.gameid))
                        {
                            if (item.personastate == 0 && AppConfig.HideIfOfflineStatus)
                            {
                                continue;
                            }
                            if (AppConfig.GameNameFilter.Any(item.gameextrainfo.Contains))
                            {
                                continue;
                            }
                            Playing.Add(item.steamid, new MonitorItem
                            {
                                AppId = item.gameid,
                                GameName = item.gameextrainfo,
                                StartTime = DateTime.Now,
                                SteamId = item.steamid,
                                Achievements = await FetchPlayerAchievementList(item.steamid, item.gameid),
                            });
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
                foreach (var item in notices.Where(x => x.NoticeType == NoticeType.Playing))
                {
                    if (item.DownloadAvatar())
                    {
                        item.ImagePath = item.Draw();
                    }
                }
                if (notices.Count > 0)
                {
                    PlayingChanged?.Invoke(notices);
                }
            }
            catch (Exception ex)
            {
                MainSave.CQLog?.Error("监视时钟", ex.Message + ex.StackTrace);
            }
            finally
            {
                ElapsedHandling = false;
            }
        }

        private async Task<GetPlayerAchievement.Achievement[]> FetchPlayerAchievementList(string steamid, string gameid)
        {
            try
            {
                if (!AppConfig.EnableAchievementNotice)
                {
                    return [];
                }
                var achievements = await GetPlayerAchievement.Get(steamid, gameid);
                if (achievements != null)
                {
                    return achievements.playerstats.achievements;
                }
                else
                {
                    return [];
                }
            }
            catch (Exception ex)
            {
                MainSave.CQLog?.Error("获取成就列表", ex.Message + ex.StackTrace);
                return [];
            }
        }
    }
}