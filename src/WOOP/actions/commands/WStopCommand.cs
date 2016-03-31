using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

namespace WOOP
{
    class WStopCommand : WCommand
    {
        WSimpleAttackingUnit thisAttackingUnit = null;

        override protected void run(WUnit unit)
        {
            if (unit is WSimpleAttackingUnit)
            {
                thisAttackingUnit = (WSimpleAttackingUnit)unit;
            }

            addMA(typeof(WStopMicroAction), null);
        }

        override protected void MAFailed(WUnit unit, WMicroAction action)
        {
            W.core.FatalError("Can't execute stop MA? WTF?");
        }

        override protected void onNextMA(WUnit unit)
        {
            if (currentCommand == this)
            {
                if (thisAttackingUnit != null)
                {
                    WUnit en = findEnemy();
                    if (en != null)
                    {
                        //currentCommand.interrupt();
                        addSubcommand(typeof(WAttackingMoveCommand), en.getPosition(thisAttackingUnit.pos));
                        addSubcommand(typeof(WAttackingMoveCommand), thisAttackingUnit.getPosition());
                    }
                }
            }

            if (actions.Count == 0)
            {
                addMA(typeof(WStopMicroAction), null);
            }
        }

        public override void tick(WUnit unit, int dt, out bool needInterruptMA)
        {
            needInterruptMA = interrupted;
        }

        static int[,] matrix = null; //Если менять размер мира, то это матрицу создавать почище.
        static int checkCntr = 0;
        WUnit findEnemy()
        {
            int pc = 0;
            logl(thisAttackingUnit, "Unit in " + thisAttackingUnit.getPosition() + "; trying to find an enemy...");
            checkCntr++; if (checkCntr == 256) checkCntr = 0;

            List<Point> open = new List<Point>();
            open.Add(thisAttackingUnit.getPosition());
            if (matrix == null) matrix = new int[W.core.world.Width, W.core.world.Height];


            while (open.Count > 0)
            {

                Point p = open[0];
                open.RemoveAt(0);
                matrix[p.X, p.Y] = checkCntr;

                for (int x = p.X - 1; x <= p.X + 1; x++)
                    for (int y = p.Y - 1; y <= p.Y + 1; y++)
                        if (!(x == p.X && y == p.Y))
                        {
                            Point n = new Point(x, y);


                            if (W.core.world.pointInWorld(n))
                                if (thisAttackingUnit.CanPlacedToTerrain(W.core.world.getTerrain(n)))
                                    if (WUtilites.calc2Drange(n, thisAttackingUnit.getPosition()) <= thisAttackingUnit.visibleRange)
                                        if (matrix[x, y] != checkCntr)
                                        {
                                            List<WUnit> unitsInCell = W.core.world.UnitsInWorld(n);
                                            WUnit otherUnit = unitsInCell.Count > 0 ? unitsInCell[0] : null;

                                            if (thisAttackingUnit.isEnemy(otherUnit))
                                            {
                                                logl(thisAttackingUnit, "Enemy found: " + otherUnit.getPosition());
                                                return otherUnit;
                                            }
                                            open.Add(n);
                                            matrix[n.X, n.Y] = checkCntr;
                                            pc++;
                                        }
                        }
            }

            logl(thisAttackingUnit, "no enemy found. PC = " + pc);

            return null;
        }
               
    }
}
