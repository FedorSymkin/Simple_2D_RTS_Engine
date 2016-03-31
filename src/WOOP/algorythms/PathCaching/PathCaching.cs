using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public class PathCachingSingleton
    {
        public List<PathCache> cache = new List<PathCache>();

        public void tick(int dt)
        {
            int i = 0;
            while (i < cache.Count)
            {
                cache[i].lifeTime -= dt;
                if (cache[i].lifeTime <= 0)
                {
                    cache.RemoveAt(i);
                    logl("patch cache " + i + " removed. New size = " + cache.Count);
                }
                else i++;
            }
        }

        static int cacheLifeTime = -1;
        static int maxCount = -1;
        int maxPathCaches = 0;
        public PathCache createPathCache(List<Point> path, int from, int to, int initPoint)
        {
            logl("make path cache");

            if (maxCount == -1) maxCount = Convert.ToInt32(W.core.getConfig("MAX_PATHCACHES_COUNT"));

            if (cache.Count < maxCount)
            {
                if (initPoint + 1 < path.Count)
                {
                    PathCache c = new PathCache();
                    c.from = path[from];
                    c.to = path[to];
                    c.initPoint = path[initPoint];
                    for (int i = initPoint + 1; i < path.Count; ++i) c.points.Add(path[i]);

                    if (cacheLifeTime == -1) cacheLifeTime = Convert.ToInt32(W.core.getConfig("PATH_CACHE_LIFETIME"));
                    c.lifeTime = cacheLifeTime;
                    this.cache.Add(c);

                    if (maxPathCaches < cache.Count)
                    {
                        maxPathCaches = cache.Count;
                        W.core.debugWidget.setValue("max path caches", maxPathCaches.ToString());
                    }

                    logl(String.Format("added path cache: from {0} to {1} initPoint {2}", c.from, c.to, c.initPoint));
                    return c;
                }
                else
                {
                    logl("cannot create path cache: path too short");
                    return null;
                }
            }
            else
            {
                logl("cannot create path cache: cache overflow");
                return null;
            }
        }

        void logl(String text)
        {
            W.core.textLogs.AlgLog.log("PathCaching: " + text);
        }
    }

    public class PathCache
    {
        public Point from;
        public Point to;
        public Point initPoint;
        public List<Point> points = new List<Point>();
        public int lifeTime;

        int calc2Drange(Point p1, Point p2)
        {
            int dx = (Math.Abs(p2.X - p1.X));
            int dy = (Math.Abs(p2.Y - p1.Y));

            if (dx >= dy) return dx; else return dy;
        }

        public static int radius = -1;
        public static int radiusSq2 = -1;
        public bool isGood(Point pFrom, Point pTo)
        {
            if (radius == -1)
            {
                radius = Convert.ToInt32(W.core.getConfig("PATH_CACHING_RADIUS"));
                radiusSq2 = radius * radius;
            }

            return ((calc2Drange(pFrom, from) <= radius) && (calc2Drange(pTo, to) <= radius));     
        }
    }
}
