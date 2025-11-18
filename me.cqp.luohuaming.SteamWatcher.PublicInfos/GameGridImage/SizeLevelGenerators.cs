using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace me.cqp.luohuaming.SteamWatcher.PublicInfos.GameGridImage
{
    public static class SizeLevelGenerators
    {
        private static double SafeLog(double t, double eps = 1e-3) => Math.Log(t + eps);

        /// <summary>
        /// 在 log(playtime) 空间做分位数切分，返回每个元素的 SizeLevel（1..levelCount，越大表示越大的封面）
        /// </summary>
        public static List<int> ComputeLogQuantileLevels(List<double> playTimes, int levelCount, double eps = 1e-3)
        {
            int n = playTimes.Count;
            if (n == 0) return [];

            // sort by log-value desc and remember original index
            var sorted = playTimes
                .Select((t, i) => new { Time = t, Index = i, Log = SafeLog(t, eps) })
                .OrderByDescending(x => x.Log)
                .ToList();

            // compute rank-based bucket on sorted list but using quantiles in log space
            // For each position i, compute quantile = i / (n-1)
            // Map quantile -> level by non-linear mapping: use power to bias head
            // Simpler: we map position to level via floor((i / n) * levelCount) but with power transform alpha < 1 to expand head
            double alpha = 0.3; // alpha in (0,1) : smaller -> more levels in head. 调参可以改成 0.4~0.7
            List<int> levelsSorted = [.. new int[n]];

            for (int i = 0; i < n; i++)
            {
                double q = (double)i / Math.Max(1, n - 1); // 0..1, i=0 is largest playtime
                                                           // apply power transform to q to concentrate more levels at head (small q)
                double qTransformed = Math.Pow(q, alpha);
                int level = levelCount - (int)(qTransformed * levelCount); // levelCount..1
                if (level < 1) level = 1;
                if (level > levelCount) level = levelCount;
                levelsSorted[i] = level;
            }

            // restore original ordering
            List<int> result = [.. new int[n]];
            for (int i = 0; i < n; i++)
            {
                int origIndex = sorted[i].Index;
                result[origIndex] = levelsSorted[i];
            }

            return result;
        }
    }
}
