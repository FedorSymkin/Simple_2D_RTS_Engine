using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;
using System.Windows.Forms;
using System.Diagnostics;

namespace WOOP
{
    public class DefineGroupsAlg
    {
        //config
        const int actionsPerTick = 2000;
        const int maxRange = 5;
        int groupsCount = 3;

        //total
        List<WUnit> units;
        List<List<WUnit>> result = new List<List<WUnit>>();
        int state = 0;
        int[,] matrix = null;
        int spentActions = 0;

        //for one group
        AStar astar;
        List<WUnit> currentGroup = null;
        int currentMark;
        enum ASTAR_FINISH_TYPE { NeighborFound, TooFar };
        ASTAR_FINISH_TYPE astarFinishType;
        Point centralPoint;


        //debug
        DefineGroupsDebugger debugger = null;
        int debugLevel = 0;
        


        public DefineGroupsAlg(List<WUnit> units, int groupsCount)
        {
            this.units = units;
            this.groupsCount = groupsCount;
            finished = false;
        }

        public List<List<WUnit>> getResult()
        {
            return result;
        }

        public void applyDebug(int level)
        {
            debugLevel = level;

            if (debugLevel >= 1)
            {
                debugger = new DefineGroupsDebugger();
                debugger.alg = this;
                W.core.gameField.graphicDebuggers.Add(debugger);
            }
        }

        ~DefineGroupsAlg()
        {
            if (debugger != null) W.core.gameField.graphicDebuggers.Remove(debugger);
        }

        public bool finished { get; private set; }

        public void startExecution()
        {
            state = 0;
            continueExecution();
        }

        public void continueExecution()
        {
            spentActions = 0;
            while ((spentActions < actionsPerTick) && (!finished))
            {
                exec();
            }
        }

        void exec()
        {
            switch (state)
            {
                case 0: //init 
                    matrix = new int[W.core.world.Width, W.core.world.Height];
                    currentMark = 0;
                    state = 0x10;
                    break;

                case 0x10: //init new group 
                    if (debugLevel >= 2) BlockingDebug.block("DefineGroups: end group", 5, false);
                    if (units.Count > 0)
                    {
                        currentGroup = new List<WUnit>();
                        result.Add(currentGroup);
                        centralPoint = units[0].pos;
                        moveUnitToCurrentGroup(units[0]);
                        currentMark++;

                        spentActions += 2;
                        state = 0x11;

                        if (debugLevel >= 2) BlockingDebug.block("DefineGroups: start group", 5, false);
                    }
                    else state = 0x20; //if no more free units -> go to next stage
                    break;

                case 0x11: //init astar for next unit (centralPoint is already defined)
                    List<Point> points = new List<Point>();
                    points.Add(centralPoint);

                    if (astar != null) astar.Dispose();
                    astar = new AStar(points, AstarCheckFunc, AstarHeuristic, actionsPerTick);
                    if (debugLevel >= 2) astar.applyDebug();
                    astar.onNewPoint = AstarOnNewPoint;
                    astar.onClosePoint = AstarOnClosePoint;
                    astar.startExecution();
                    state = 0x12;
                    break;

                case 0x12: //wait for astar for unit
                    if (!astar.finished) astar.continueExecution();
                    if (astar.finished)
                    {
                        spentActions += astar.spentActions;

                        if (astarFinishType == ASTAR_FINISH_TYPE.NeighborFound) state = 0x11; //OK, astar from next unit within this group
                        else if (astarFinishType == ASTAR_FINISH_TYPE.TooFar) state = 0x10;  //start new group
                    }
                    break;


                case 0x20: //Next stage - TODO

                    state = 0x90;
                    break;

                case 0x90: //finish
                    finished = true;
                    state = 0x91;
                    break;

                case 0x91:
                    break;
            }
        }

        bool AstarCheckFunc(int x, int y)
        {
            //TODO!!! Set real move ability of unit! 
            return (matrix[x, y] != currentMark) && (W.core.world.terrainType(W.core.world.getTerrain(x, y)) == "allowed");
        }

        double AstarHeuristic(int x, int y)
        {
            return 0;
        }

        bool AstarOnNewPoint(int x, int y)
        {
            //check for too far
            if (WUtilites.calc2Drange(new Point(x, y), centralPoint) > maxRange)
            {
                astarFinishType = ASTAR_FINISH_TYPE.TooFar;
                return false;
            }

            //check for found new unit (unit from my list!)
            WUnit unit = W.core.world.UnitInWorld(x, y);
            if (unit != null)
            {
                if (unit.inGame && units.Contains(unit))
                {
                    moveUnitToCurrentGroup(unit);          
                    centralPoint = unit.pos;
                    astarFinishType = ASTAR_FINISH_TYPE.NeighborFound;
                    return false;
                }
            }
            return true;
        }

        bool AstarOnClosePoint(int x, int y)
        {
            matrix[x, y] = currentMark;
            return true;
        }

        void moveUnitToCurrentGroup(WUnit unit)
        {
            currentGroup.Add(unit);
            units.Remove(unit);
        }

        public class DefineGroupsDebugger : WGraphicsDebugger
        {
            public DefineGroupsAlg alg;
            override public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                for (int i = 0; i < alg.result.Count; i++)
                {
                    Color clr = getGroupColor(i);
                    for (int u = 0; u < alg.result[i].Count; u++)
                    {
                        WUnit unit = alg.result[i][u];
                        if (unit.pos == cellCoord)
                        {
                            g.DrawString(i.ToString(), new Font("arial", 14), new SolidBrush(clr), new Point(area.Left, (area.Top + area.Bottom)/2)); 
                        }
                    }
                }

                if (alg.debugLevel >= 2)
                {
                    if (cellCoord == alg.centralPoint)
                    {
                        g.DrawString("()", new Font("arial", 26), new SolidBrush(Color.Silver), new Point(area.Left, area.Top));
                    }

                    if ((alg.matrix != null) && (alg.currentMark > 0) && (alg.matrix[cellCoord.X, cellCoord.Y] == alg.currentMark))
                    {
                        g.DrawString("x", new Font("arial", 10), new SolidBrush(Color.Silver), new Point(area.Left, area.Top));
                    }
                }
            }

            Color getGroupColor(int i)
            {
                switch (i % 5)
                {
                    case 0: return Color.Red;
                    case 1: return Color.LightGreen;
                    case 2: return Color.Blue;
                    case 3: return Color.Yellow;
                    case 4: return Color.Silver;
                }
                return Color.Red;
            }
        }
    }
}
