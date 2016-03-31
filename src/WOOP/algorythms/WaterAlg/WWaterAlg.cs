using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Diagnostics;

namespace WOOP
{
    public partial class WWaterGraph : Graph
    {
        public List<Point> findMacroPath(WUnit unit, Point pointFrom, Point PointTo, out bool destinationSuccess)
        {
            Stopwatch timer = Stopwatch.StartNew();

            #if MACRO_PF_DEBUG
                startDebug(DebugMode.astar);
                dbg.resPointPath = null;
            #endif

            List<Point> res = new List<Point>();
            List<WWaterBorder> zones = macroAstar(pointFrom, PointTo, out destinationSuccess);

            for (int i = zones.Count - 1; i >= 0; --i) res.Add(zones[i].walkPoint);

            #if MACRO_PF_DEBUG
                dbg.resPointPath = res;
                BlockingDebug.block("macro Path findinf finished ", 6, false);

                if (W.core.getConfig("STAY_MACRO_MAP") != "true") endDebug();            
            #endif



            timer.Stop();
            logo("findMacroPath time = " + timer.ElapsedMilliseconds + " ms");
            return res;
        }

    } 

}
