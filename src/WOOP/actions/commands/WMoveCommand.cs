using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Diagnostics;
using WOOP;

namespace WOOP
{
    public class WMoveCommand : WCommand, IDisposable
    {
        static bool? useMacroPathFinding = null;
        static bool? usePathCaching = null;
        Point? firstTargetPoint = null;

        WMoveCommandDebugger dbg;
        public WUnit DbgParentUnit;
        override public void BeginAction(WUnit unit)
        {
            if (W.core.getConfig("ENABLE_CMDMOVE_DEBUG") == "true")
            {
                dbg = new WMoveCommandDebugger();
                dbg.cmd = this;
                DbgParentUnit = unit;
                W.core.gameField.graphicDebuggers.Add(dbg);
            }
            ((WMovingUnit)unit).moveFlag = true;
        }



        public override String getDbgPictString(WUnit unit, IWXNAControl g, Rectangle area)
        {
            return "M";
            //g.DrawString("M", new Font("Arial", 10), new SolidBrush(Color.Black), area);
        }


        public override void EndAction(WUnit unit)
        {
            if (W.core.getConfig("ENABLE_CMDMOVE_DEBUG") == "true")
            {
                W.core.gameField.graphicDebuggers.Remove(dbg);
            }
            ((WMovingUnit)unit).moveFlag = false;
        }

        public override void EndActionInterrupted(WUnit unit)
        {
            if (W.core.getConfig("ENABLE_CMDMOVE_DEBUG") == "true")
            {
                W.core.gameField.graphicDebuggers.Remove(dbg);
            }
            ((WMovingUnit)unit).moveFlag = false;
        }

        List<uint> RecentPathsCRCs = new List<uint>();
        static int resetAfter = -1;
        override protected void run(WUnit unit)
        {
            Stopwatch timer = Stopwatch.StartNew();
            logo(unit, "");


            #if RETARGET_DEBUG
                        startRetargetDebug();
            #endif
            bool dp = defineNewTargetPoint(unit);
            #if RETARGET_DEBUG
                        endRetargetDebug();
            #endif


            if (!dp)
            {
                logl(unit, "Error: cannot spire");
                runStub(unit);
                return;
            }

            if ((Point)param == unit.getPosition())
            {
                logl(unit, "unit already in this position");
                addMA(typeof(WStopMicroAction), null);
                return;
            }

            if (useMacroPathFinding == null) useMacroPathFinding = W.core.getConfig("USE_MACRO_PF") == "true";
            if (usePathCaching == null) usePathCaching = W.core.getConfig("USE_PATH_CACHING") == "true";
            
            bool bigRange = (calc2Drange(unit.getPosition(), (Point)param) >= 120 );
            bool usePathCachingNow = usePathCaching.Value && bigRange;

            logl(unit, "Move run");
            initFlags();


            //Getting points====================================
            List<Point> points = null;
            bool targetAchieved = false;

            if (usePathCachingNow == true)
            {
                Stopwatch timer1 = Stopwatch.StartNew();
                bool ok;
                points = getPathCachingPoints(unit, unit.getPosition(), (Point)param, out ok);
                targetAchieved = false;
                timer1.Stop();
                logo(unit, String.Format("Getting cached path result = {0}) time = {1}", ok, timer1.ElapsedMilliseconds));
            }

            if (points == null)
            {
                if ((useMacroPathFinding == true) && (bigRange))
                {
                    points = macroPathFinding(unit, unit.getPosition(), (Point)param, out targetAchieved);
                }
                else points = ((WMovingUnit)unit).getPathFinder().execute(unit, unit.getPosition(), (Point)param, out targetAchieved);                
            }

            if (usePathCachingNow == true) makePathCache(unit, points);
           //==============================================

            logl(unit, "PathFinding executed, targetAchieved = " + targetAchieved);
            foreach (var p in points)
            {
                logl(unit, "ALGPOINT = " + p.ToString());
                addMA(typeof(WMoveMicroAction), p);
            }

            if (points.Count == 0)
            {
                logl(unit, "Cant define points to pathfinding. Pause");
                addMA(typeof(WStopMicroAction), null);
            }
            

            

            
            if (!targetAchieved)
            {
                logl(unit, "targetAchieved = false, rerun added");

                uint crc = CRC32.calc(points);
                if (RecentPathsCRCs.Contains(crc))
                {
                    logl(unit, "path already has been");
                    shelfLifeTmrEnabled = true;
                }
                else RecentPathsCRCs.Add(crc);

                addMA(typeof(WRerunMicroAction), null);
            }
            else
            {
                shelfLife = 0;
                shelfLifeTmrEnabled = false;
                RecentPathsCRCs.Clear();
            }

            //rerun after 10 moves - it nessesary for avoid "stupid unit". Can be switched off if PathFinding is hard
            if (resetAfter == -1) resetAfter = Convert.ToInt32(W.core.getConfig("RESET_PF_AFTER"));
            if (resetAfter > 0)
            {
                if (actions.Count > resetAfter)
                {
                    actions.RemoveRange(resetAfter, actions.Count - resetAfter);
                    addMA(typeof(WRerunMicroAction), null);
                }
            }            
           
            


            timer.Stop();
            if (maxTimeOfRun < timer.ElapsedMilliseconds)
            {
                maxTimeOfRun = (int)timer.ElapsedMilliseconds;
                W.core.debugWidget.setValue("max time of move run", maxTimeOfRun.ToString());
            }
            logo(unit, "run move command time = " + timer.ElapsedMilliseconds + " ms");
        }

        private void endRetargetDebug()
        {
            W.core.gameField.graphicDebuggers.Remove(retargetDbg);
        }

        WMoveRetargetDebugger retargetDbg;
        private void startRetargetDebug()
        {
            retargetDbg = new WMoveRetargetDebugger();
            retargetDbg.cmd = this;
            W.core.gameField.graphicDebuggers.Add(retargetDbg);
        }
        static int maxTimeOfRun = 0;


        enum SpireMode
        {
            Normal,
            TerrainBlock
        }

        Point? spire(WUnit unit, Point target, SpireMode mode)
        {
            bool[,] matrix = new bool[W.core.world.Width, W.core.world.Height];
            List<Point> open = new List<Point>();

            #if RETARGET_DEBUG
            retargetDbg.matrix = matrix;
            retargetDbg.open = open;
            retargetDbg.unit = unit;
            BlockingDebug.block("Retarget spire", 5, false);
            #endif

            
            open.Add(target);           
            Point? ret = null;
            int minRng = 0;
            while (open.Count > 0)
            {
                Point pt = open.ElementAt(0);
                open.RemoveAt(0);
                matrix[pt.X, pt.Y] = true;

                #if RETARGET_DEBUG
                retargetDbg.currentProcessedPoint = pt;
                BlockingDebug.block("Processing point", 1, false);
                #endif

                for (int x = pt.X - 1; x <= pt.X + 1; ++x)
                    for (int y = pt.Y - 1; y <= pt.Y + 1; ++y)
                        if (!(x == pt.X && y == pt.Y))
                            if ((x - pt.X)*(y - pt.Y) == 0) //cross propogation (x4 instead x8)
                        {
                            #if RETARGET_DEBUG
                            retargetDbg.currentNeighbor = new Point(x,y);
                            BlockingDebug.block("Processing neighbor", 0, false);
                            #endif

                            if (W.core.world.pointInWorld(x,y))
                            if (!matrix[x, y])
                            {
                                bool ok = true;
                                if (mode == SpireMode.TerrainBlock)
                                {
                                    if (!unit.CanPlacedToTerrain(W.core.world.getTerrain(x, y))) ok = false;
                                }

                                if (ok)
                                {
                                    Point p = new Point(x, y);

                                    if (unit.CanPlacedTo(p))
                                    {
                                        if (ret == null)
                                        {
                                            ret = p;
                                            minRng = calc2Drange(unit.getPosition(), p);
                                        }
                                        else
                                        {
                                            int r = calc2Drange(unit.getPosition(), p);
                                            if (r < minRng)
                                            {
                                                if (r != 0) //do not return self cell
                                                {
                                                    minRng = r;
                                                    ret = p;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //other unit in p   
                                        List<WUnit> unitsInCell = W.core.world.UnitsInWorld(p);
                                        if (unitsInCell.Count > 0)
                                        {
                                            WUnit otherUnit = unitsInCell.ElementAt(0);
                                            if (otherUnit is WMovingUnit)
                                            {
                                                //only for staying units
                                                if (!((WMovingUnit)otherUnit).moveFlag) open.Add(p); 
                                            }
                                            else open.Add(p); //or non-moving units
                                        }   
                                    }
                                }
                            }

                            #if RETARGET_DEBUG
                            retargetDbg.retPoint = ret;
                            #endif
                        }
            }

            #if RETARGET_DEBUG
            BlockingDebug.block("End of retarget spire", 6, false);
            #endif

            return ret;
        }

        private bool defineNewTargetPoint(WUnit unit)
        {
            Point target;
            if (firstTargetPoint == null) target = (Point)param;
            else target = firstTargetPoint.Value;

            if (!unit.CanPlacedTo(target))
            {
                logl(unit, "can't placed to target");

                if (!unit.CanPlacedToTerrain(W.core.world.getTerrain(target))) 
                {
                    logl(unit, "because of terrain");

                    Point? pt = spire(unit, target, SpireMode.Normal);
                    if (pt != null) logl(unit, "spire = " + pt.Value); else logl(unit, "spire = null");

                    if (pt == null) return false;
                    param = pt.Value;
                    firstTargetPoint = null;
                }
                else
                {
                    logl(unit, "because of unit");

                    firstTargetPoint = (Point)param;
                    Point? pt = spire(unit, target, SpireMode.TerrainBlock);
                    if (pt != null) logl(unit, "spire = " + pt.Value); else logl(unit, "spire = null");
                    if (pt == null) return false;
                    param = pt.Value;
                }              
            }

            logl(unit, "new target = " + param);
            if (firstTargetPoint != null) logl(unit, "first Point = " + firstTargetPoint.Value); else logl(unit, "first Point = null");
            return true;
        }

        private void makePathCache(WUnit unit, List<Point> points)
        {
            Stopwatch timer = Stopwatch.StartNew();

            if (PathCache.radius == -1)
            {
                PathCache.radius = Convert.ToInt32(W.core.getConfig("PATH_CACHING_RADIUS"));
                PathCache.radiusSq2 = PathCache.radius * PathCache.radius;
            }

            PathCache p = W.core.units.pathCaching.createPathCache(points, 0, points.Count - 1, PathCache.radius);
            bool ok = (p != null);
            logo(unit, String.Format("Made path cache result = {0}) time = {1}", ok, timer.ElapsedMilliseconds));

        }

        int calc2Drange(Point p1, Point p2)
        {
            int dx = (Math.Abs(p2.X - p1.X));
            int dy = (Math.Abs(p2.Y - p1.Y));

            if (dx >= dy) return dx; else return dy;
        }

        private List<Point> getPathCachingPoints(WUnit unit, Point from, Point to, out bool ok)
        {
            

            logl(unit, "start getting cached path");

            ok = false;
            if (calc2Drange(from, to) < PathCache.radius * 2) 
            {
                logl(unit, "path caching failed: path too short");
                return null; 
            }

            foreach (var pc in W.core.units.pathCaching.cache)
            {
                if (pc.isGood(from, to))
                {
                    logl(unit, String.Format("founded good path: from {0} to {1} initPoint {2}", pc.from, pc.to, pc.initPoint));

                    AStarPathFinder pf = new AStarPathFinder();
                    pf.usePermissions = true;
                    pf.maxPointsCount = PathCache.radiusSq2 * 2 + 10;

                    logl(unit, String.Format("Executing local astar from {0} to {1}", from, pc.initPoint)); 
                    List<Point> firstPath = pf.execute(unit, from, pc.initPoint, out ok);

                    if (ok)
                    {
                        logl(unit, "local astar executed. Path caching success."); 
                        List<Point> res = new List<Point>();
                        foreach (var x in firstPath) res.Add(x);
                        foreach (var x in pc.points) res.Add(x);
                        return res;
                    }
                    else
                    {
                        logl(unit, "path caching failed: Astar too long"); 
                        return null;
                    }
                }
            }

            logl(unit, "no good paths found"); 
            return null;
        }

        static int PFcount = -1;
        private List<Point> macroPathFinding(WUnit unit, Point from, Point to, out bool targetAchieved)
        {
            logl(unit, "macroPathFinding");

            targetAchieved = false;
            bool founded = false;
            List<Point> MacroPoints = W.core.world.graph.findMacroPath(unit, from, to, out founded);

            List<Point> res = new List<Point>();
            if (PFcount == -1) PFcount = Convert.ToInt32(W.core.getConfig("MICRO_PF_COUNT"));
            logl(unit, "count of used parts = " + PFcount);
            for (int i = 1; (i < MacroPoints.Count) && (i < PFcount + 1); i++)
            {
                logl(unit, String.Format("local path from {0} to {1}...", MacroPoints[i - 1], MacroPoints[i]));

                AbstractPathFinder pf = ((WMovingUnit)unit).getPathFinder();
                if (pf is JumpPathFinder)
                {
                    Point localFrom = MacroPoints[i - 1];
                    Point localTo = MacroPoints[i];

                    ((JumpPathFinder)pf).macroMode = true;
                    WWaterZone zoneFrom = W.core.world.graph.matrix[localFrom.X, localFrom.Y];
                    WWaterZone zoneTo = W.core.world.graph.matrix[localTo.X, localTo.Y];
                    WWaterZone zoneMid = null;

                    if (zoneFrom is WWaterMacroZone)
                    {
                        zoneMid = zoneFrom;
                    }
                    else if (zoneTo is WWaterMacroZone)
                    {
                        zoneMid = zoneTo;
                    }
                    else
                    {
                        GraphEdge? ed = W.core.world.graph.findAdjacent(zoneFrom, zoneTo);
                        if (ed != null) zoneMid = (WWaterZone)ed.Value.midzone;
                        else W.core.FatalError("WVC: WTF??");
                    }
                       
                    ((JumpPathFinder)pf).zoneFrom = zoneFrom;
                    ((JumpPathFinder)pf).zoneFrom = zoneTo;
                    ((JumpPathFinder)pf).zoneMid = zoneMid;
                }
                List<Point> localPath = pf.execute(unit, MacroPoints[i - 1], MacroPoints[i], out founded);

                if (!founded)
                {
                    logl(unit, "localPath not founded. Using global PF");
                    res = ((WMovingUnit)unit).getPathFinder().execute(unit, from, to, out targetAchieved);
                    return res;
                }
                
                if (res.Count > 0) res.RemoveAt(res.Count - 1);
                foreach (var x in localPath) res.Add(x);

                if (MacroPoints[i] == to) targetAchieved = true;
            }

            return res;
        }


        void initFlags()
        {
            logl(null, "initFlags");
            LocalFailsCount = 0;
            LocalWaiting = false;
        }

        int LocalFailsCount;
        bool LocalWaiting;
        override protected void MAFailed(WUnit unit, WMicroAction action)
        {
            if (action is WMoveMicroAction)
            {
                LocalFailsCount++;
                LocalWaiting = true;

                logl(unit, "MoveMAFailed, LocalFailsCount = " + LocalFailsCount);

                if (LocalFailsCount < 3)
                {
                    logl(unit, "adding stop");
                    addMA(typeof(WStopMicroAction), null, 0);
                }
                else
                {
                    logl(unit, "rerun");
                    actions.Clear();
                    run(unit);
                }
            }
            else W.core.FatalError("WTF??,");
        }

        override protected void MASuccess(WUnit unit, WMicroAction action)
        {
            if (action is WMoveMicroAction)
            {
                logl(unit, "Move MASuccess");
                LocalFailsCount = 0;
                LocalWaiting = false;
            }
        }   

        override protected void onNextMA(WUnit unit)
        {
           
        }

        int shelfLife = 0;
        bool shelfLifeTmrEnabled = false;
        public override void tick(WUnit unit, int dt, out bool needInterruptMA)
        {
            needInterruptMA = false;
            if (LocalWaiting)
            {
                WMoveMicroAction mma = getFirstMoveMA();

                if (mma != null)
                if (mma.canExecute(unit))
                {
                    logl(unit, "needInterruptMA = true");
                    needInterruptMA = true;
                }
            }

            if (shelfLifeTmrEnabled)
            {
                shelfLife += dt;
                if (shelfLife >= 500)
                {
                    shelfLifeTmrEnabled = false;
                    shelfLife = 0;
                    logl(unit, "interrupted by shelfLife");
                    interrupt();
                }
            }

            if (wasBlockedByTime()) needInterruptMA = true;   
        }

        //=========internal===================================================
        WMoveMicroAction getFirstMoveMA()
        {
            foreach (WMicroAction a in actions)
            {
                if (a is WMoveMicroAction) return (WMoveMicroAction)a;
            }
            logl(null, "Error: getFirstMoveMA returned null");
            return null;
        }

        public void Dispose()
        {
            logm(null, "Dispose");   
        }




        class WMoveRetargetDebugger : WGraphicsDebugger
        {
            public WMoveCommand cmd;

            public bool[,] matrix;
            public List<Point> open;
            public Point? currentProcessedPoint;
            public Point? currentNeighbor;
            public Point? retPoint;
            public WUnit unit;

            Rectangle AreaDiv(Rectangle area, float scale)
            {
                RectangleF res = new RectangleF();
                res.X = (float)area.X + (1 - scale) * (float)area.Width / 2;
                res.Y = (float)area.Y + (1 - scale) * (float)area.Height / 2;
                res.Width = (float)area.Width * scale;
                res.Height = (float)area.Height * scale;

                return new Rectangle((int)res.X, (int)res.Y, (int)res.Width, (int)res.Height);
            }

            override public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {


                if (currentNeighbor != null)
                if (cellCoord == currentNeighbor)
                    g.FillRectangle(new SolidBrush(Color.Blue), AreaDiv(area, 0.3f));

                if (currentProcessedPoint != null)
                if (cellCoord == currentProcessedPoint)
                    g.FillEllipse(new SolidBrush(Color.Red), AreaDiv(area, 0.7f));


                if (open.Contains(cellCoord))
                    g.DrawEllipse(new Pen(Color.Red, 3), AreaDiv(area, 0.7f));

                if (matrix[cellCoord.X, cellCoord.Y])
                    g.DrawRectangle(new Pen(Color.Black, 3), AreaDiv(area, 0.8f));

                if (cmd.firstTargetPoint != null)
                    if (cmd.firstTargetPoint.Value == cellCoord)
                        g.DrawString("F", new Font("Arial", 18), new SolidBrush(Color.Red), area);

                if (retPoint != null)
                    if (cellCoord == retPoint.Value)
                        g.FillEllipse(new SolidBrush(Color.Yellow), AreaDiv(area, 0.9f));

                if (unit.getPosition() == cellCoord)
                    g.DrawRectangle(new Pen(Color.Purple, 3), AreaDiv(area, 0.9f));
            }
        }
    }



    public class WMoveCommandDebugger : WGraphicsDebugger
    {
        public WMoveCommand cmd;

        override public void debugDraw(Object sender, IWXNAControl g, Rectangle area)
        {
        }

        override public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
        {
            if (cmd.DbgParentUnit.isSelected())
            {
                for (int i = 0; i < cmd.actionsCount(); i++)
                {
                    WAction a = cmd.getAction(i);
                    if (a is WMoveMicroAction)
                    {
                        Point prm = (Point)((WMoveMicroAction)a).param;
                        if (prm == cellCoord)
                        {
                           g.DrawEllipse(new Pen(Color.Red, 4), area);
                        }
                    }
                }

                Point cmdParam = (Point)cmd.param;
                if (cellCoord == cmdParam)
                {
                    g.DrawEllipse(new Pen(Color.Yellow, 4), area);
                }
            }
        }
    }
}
