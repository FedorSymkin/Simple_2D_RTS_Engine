using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Text.RegularExpressions;

namespace WOOP
{
    public partial class WWaterGraph
    {
        List<WWaterBorder> AOpenPoints = new List<WWaterBorder>();
        List<WWaterBorder> AClosedPoints = new List<WWaterBorder>();
        Point pStart;
        Point pEnd;
        WWaterBorder zStart;
        WWaterBorder zEnd;
        int astarCounter = 0;
        Comparison<WWaterBorder> AComp = null;
        List<WWaterBorder> resPath;
        WWaterBorder currentAstarOpenPoint;

        bool EndIsAvailable;
        WWaterBorder NearestFinalZone;
        int NearestFinalZoneRange = -1;


        int DotLogCnt = 0;
        void dotlog(String text)
        {
            SaveToPictureCustomDot(String.Format("MacroPFDot/{0}-{1}", DotLogCnt, text), toAstarDot());
            DotLogCnt++;
        }

        List<WWaterBorder> macroAstar(Point pStart, Point pEnd, out bool destinationSuccess)
        {
            //init
            this.pStart = pStart;
            this.pEnd = pEnd;  
            AOpenPoints.Clear();
            AClosedPoints.Clear();
            astarCounter++;
            if (AComp == null) AComp = new Comparison<WWaterBorder>(ACompFunc);
            resPath = new List<WWaterBorder>();
            NearestFinalZoneRange = -1;
            EndIsAvailable = false;
            DotLogCnt = 0;
            currentAstarOpenPoint = null;
            logl("Macro Astar:_______________");


            #if MACRO_PF_DEBUG
                BlockingDebug.block("WWaterGraph - macro astar", 5, false);
            #endif


            //define start zones, adding start zone
            InitAstar();
            NearestFinalZone = zStart;

            #if MACRO_PF_DEBUG
                dotlog("begin");
                BlockingDebug.block("Macro PF ready for astar", 5, false);
            #endif

            while (AOpenPoints.Count > 0)
            {
                AOpenPoints.Sort(AComp);

                WWaterBorder p = AOpenPoints[0];
                AOpenPoints.RemoveAt(0);
                AClosedPoints.Add(p);

                checkForNearestResult(p);
                procOpenPoint(p);
            }

            if (EndIsAvailable) makePath(ref resPath, zEnd);
            else makePath(ref resPath, NearestFinalZone);

            destinationSuccess = EndIsAvailable;

            #if MACRO_PF_DEBUG
                dotlog("end");
                if (W.core.getConfig("SHOW_MACRO_PF_GRAPH") == "true") LookDot(toAstarDot());
                BlockingDebug.block("Macro Astar finished", 5, false);     
            #endif

            return resPath;
        }

        private void checkForNearestResult(WWaterBorder z)
        {
            if ((z != zStart) && (z != zEnd))
            {
                Point np = findNearestByTwo(z.walkPoint, pEnd, z.points);
                int r = calc2Drange(np, pEnd);
                if ((NearestFinalZoneRange == -1) || (r < NearestFinalZoneRange))
                {
                    NearestFinalZoneRange = r;
                    NearestFinalZone = z;
                }
            }   
        }

        private void InitAstar()
        {
            unconnectFromGraph(startStub);
            unconnectFromGraph(endStub);

            //define start zone
            WWaterZone st = matrix[pStart.X, pStart.Y];
            if (st is WWaterMacroZone)
            {
                zStart = startStub;
                List<WWaterBorder> borders = ((WWaterMacroZone)st).borders;
                foreach (var b in borders) connectNodes(startStub, b, startStub);
                zStart.points.Clear();
                zStart.points.Add(pStart);
            }
            else zStart = ((WWaterBorder)st); 


            //define end zone
            WWaterZone end = matrix[pEnd.X, pEnd.Y];
            if (end is WWaterMacroZone)
            {
                zEnd = endStub;
                List<WWaterBorder> borders = ((WWaterMacroZone)end).borders;
                foreach (var b in borders) connectNodes(endStub, b, endStub);
                zEnd.points.Clear();
                zEnd.points.Add(pEnd);    
            }
            else zEnd = ((WWaterBorder)end);

            if (st == end) connectNodes(zStart, zEnd, st);
     
            //Adding start point
            addPointToAstar(null, zStart);
        }

        void makePath(ref List<WWaterBorder> res, WWaterBorder to)
        {
            res.Clear();

            WWaterBorder z = to;
            while (z != null)
            {
                res.Add(z);
                z = z.astarParent;
            }
        }



        void addPointToAstar(WWaterBorder parent, WWaterBorder p)
        {
            AOpenPoints.Add(p);
            p.astarCost = 0;
            AstarSetParent(parent, p);
        }

        void AstarSetParent(WWaterBorder parent, WWaterBorder p)
        {
            p.astarParent = parent;

            if (parent != null) //not start zone
            {
                if (p != zEnd) p.walkPoint = findNearestByTwo(parent.walkPoint, pEnd, p.points);
                else p.walkPoint = pEnd;

                p.astarCost = parent.astarCost + calc2Drange(parent.walkPoint, p.walkPoint);
            }
            else
            {
                p.walkPoint = pStart;
                p.astarCost = 0;
            }
        }

        bool deepDebug = false;
        void procOpenPoint(WWaterBorder p)
        {
            logl("Processing open point " + p);
            currentAstarOpenPoint = p;

            foreach (var adj in p.adjacent)
            {
                WWaterBorder a = (WWaterBorder)adj.node;

                if (AClosedPoints.Contains(a)) continue;

                if (!AOpenPoints.Contains(a)) //adding
                {
                    addPointToAstar(p, a);
                    logl("Added " + a + " parent = " + p);
                }
                else //reparent
                {
                    logl("Try to reparent " + a + " to " + p);

                    Point nwp = findNearestByTwo(p.walkPoint, pEnd, a.points);
                    int plus = calc2Drange(nwp, p.walkPoint);
                    int Pcost = p.astarCost + plus;
                    logl(String.Format("plus = {0}; a.astarCost = {1}; Pcost = {2}", plus, a.astarCost, Pcost));

                    if (a.astarCost > Pcost)
                    {
                        AstarSetParent(p, a);
                        logl("Reparented " + a + " new parent = " + p);
                    } else logl("no");
                }

                if (a == zEnd) EndIsAvailable = true;
            }

            #if MACRO_PF_DEBUG
            if (deepDebug)
            {
                dotlog("proceccedOpenPoint" + p.index);
                BlockingDebug.block("proceccedOpenPoint " + p.index, 1, false);
            }
            #endif
        }

        int ACompFunc(WWaterBorder z1, WWaterBorder z2)
        {
            return z1.astarCost.CompareTo(z2.astarCost);
        }
    }
}
