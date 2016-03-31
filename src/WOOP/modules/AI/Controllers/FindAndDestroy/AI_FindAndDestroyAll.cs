using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.IO;
using WOOP;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;
using IMaker;
using System.Threading;

namespace WOOP
{
    class FindAndDestroyAllController : WAIController
    {
        const int PointsPerTick = 2000;
        const int retryTimeoutTicks = 200;
        const int waitingLatesTout = 600; 
        const double HeuristicKoeff = 50;
        const int maxFragmRate = 8;
        const int normalFragmRate = 6;
        const int waitLatePeriodCalc = 15;


        int state = 0;
        AStar astar = null;
        Point blackPoint;
        bool blackPointFound;
        int tickCounter;
        int microCounter;
        bool waitLates = false;

        double directionAngle;
        bool directionAngleInited; 

        Point lastPoint;    
        bool lastPointInited;




        public FindAndDestroyAllController(WPlayer plr, bool topLevel, bool waitLates)
            : base(plr, topLevel) 
        {
            this.waitLates = waitLates;
        }


        protected override void onStart() 
        {
            if (topLevel)
            {
                units = W.core.units.getUnitsByPredicate(
                    u => ((u.OwnerPlayer == player) && (u is WMovingUnit))
                );
            } //if no topLevel - units list is defined by parent controller

            state = 0;
        }

        protected override void onTick(uint dt)
        {
            switch (state)
            {
                case 0: //init
                    lastPointInited = false;
                    directionAngleInited = false;
                    state = 0x10;
                    break;

                case 0x10: //start astar
                    updateUnits();

                    List<Point> points = new List<Point>();
                    foreach (var unit in units) points.Add(unit.pos);
                    if (points.Count != 0)
                    {
                        blackPointFound = false;
                        if (astar != null) astar.Dispose();
                        astar = new AStar(points, AstarCheckFunc, AstarHeuristic, PointsPerTick);
                        astar.onNewPoint = onAstarNewPoint;
                        // astar.applyDebug();
                        astar.startExecution();
                        state = (astar.finished) ? 0x20 : 0x11;
                    }
                    else state = 0x40;
                    break;

                case 0x11: //continue astar
                    astar.continueExecution();
                    if (astar.finished) state = 0x20;
                    break;

                case 0x20: //try move command
                    if (blackPointFound)
                    {
                        W.core.units.moveUnits(units, blackPoint, null, false, true);
                        tickCounter = 0;
                        state = 0x30;
                    }
                    else state = 0x40;
                    break;

                case 0x30: //wait for goal
                    List<WUnit> uns = W.core.world.UnitsInWorld(blackPoint);
                    if (uns.Count > 0)
                    {
                        WUnit unitInBlackPoint = uns[0];
                        //if some unit from my list has achieved the target
                        if (units.Contains(unitInBlackPoint)) state = 0x31;
                    }
                    if (++tickCounter >= retryTimeoutTicks) state = 0x31; //go next by timeout!
                    break;


                case 0x31: //we are came. Try define next direction and maybe start waiting for late units
                    if (lastPointInited)
                    {
                        directionAngle = WUtilites.angleBetweenPoints(lastPoint, blackPoint);
                        directionAngleInited = true;
                    }

                    lastPoint = blackPoint;
                    lastPointInited = true;


                    //maybe start waiting for late units (if configured)
                    if (waitLates)
                        state = (WUtilites.calcGroupFragmentationRate(units) > maxFragmRate) ? 0x32 : 0x10;
                    else
                        state = 0x10;
                    break;

                case 0x32: //start waiting for late units
                    tickCounter = 0;
                    microCounter = 0;
                    state = 0x33;
                    break;

                case 0x33:
                    tickCounter++;
                    if (++microCounter >= waitLatePeriodCalc)
                    {
                        microCounter = 0;
                        if (WUtilites.calcGroupFragmentationRate(units) <= normalFragmRate) state = 0x10;
                        if (tickCounter >= waitingLatesTout) state = 0x10; //go next by timeout!
                    }
                    break;

                case 0x40: //finish
                    finish();
                    state = 0x41;
                    break;

                case 0x41:
                    break;
            }
        }


        void updateUnits()
        {
            for (int i = 0; i < units.Count; )
            {
                if (units[i].inGame) i++;
                else units.RemoveAt(i);
            }
        }

        bool AstarCheckFunc(int x, int y)
        {
            //TODO!!! Set real move ability of unit! 
            return W.core.world.terrainType(W.core.world.getTerrain(x, y)) == "allowed";
        }

        double AstarHeuristic(int x, int y)
        {
            if (directionAngleInited)
            {
                double ang = WUtilites.angleBetweenPoints(lastPoint, new Point(x, y));

                //ang in radians, abs(ang - directionAngle) -> max 3.14
                return Math.Abs(ang - directionAngle) * HeuristicKoeff;
            } else return 0;
        }

        bool onAstarNewPoint(int x, int y)
        {
            if (!player.mapIsOpen[x, y])
            {
               // BlockingDebug.block("found!!!", 8);
                blackPointFound = true;
                blackPoint = new Point(x, y);
                return false;
            }
            else return true;
        }

        public void applyDebug()
        {
            FindAndDestroyAllDebugger dbg = new FindAndDestroyAllDebugger();
            dbg.alg = this;
            W.core.gameField.graphicDebuggers.Add(dbg);
        }

        public class FindAndDestroyAllDebugger : WGraphicsDebugger
        {
            WUnit median = null;
            Point medianPos;
            public FindAndDestroyAllController alg;
            public override void debugDrawCell(object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                if ((median == null) || (median.pos != medianPos))
                {
                    median = WUtilites.getMedianUnit(alg.units);
                    medianPos = median.pos;
                }

                if (cellCoord == median.pos)
                {
                    g.DrawString("&", new Font("arial", 24), new SolidBrush(Color.Blue), new Point(area.Left, area.Top));
                }
            }
        }
    }
}
