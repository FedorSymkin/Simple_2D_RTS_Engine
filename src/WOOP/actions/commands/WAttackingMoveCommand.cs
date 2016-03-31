using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

namespace WOOP
{
    public class WAttackingMoveCommand : WCommand
    {
        WSimpleAttackingUnit thisUnit;
        WUnit currentTarget;
        Point targetPoint;

        static int[,] matrix = null; //Если менять размер мира, то это матрицу создавать почище.
        static int checkCntr = 0;
        WUnit findEnemy()
        {
            int pc = 0;
            logl(thisUnit, "Unit in " + thisUnit.getPosition() + "; trying to find an enemy...");
            checkCntr++; if (checkCntr == 256) checkCntr = 0; 

            List<Point> open = new List<Point>();
            open.Add(thisUnit.getPosition());
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
                            Point n = new Point(x,y);
                            

                            if (W.core.world.pointInWorld(n))
                            if (thisUnit.CanPlacedToTerrain(W.core.world.getTerrain(n)))
                            if (WUtilites.calc2Drange(n, thisUnit.getPosition()) <= thisUnit.visibleRange)
                            if (matrix[x, y] != checkCntr)  
                            {
                                List<WUnit> unitsInCell = W.core.world.UnitsInWorld(n);
                                WUnit otherUnit = unitsInCell.Count > 0 ? unitsInCell[0] : null;

                                if (thisUnit.isEnemy(otherUnit))
                                {
                                    logl(thisUnit, "Enemy found: " + otherUnit.getPosition());
                                    return otherUnit;
                                }
                                open.Add(n);
                                matrix[n.X, n.Y] = checkCntr;
                                pc++;
                            }
                    }
            }

            logl(thisUnit, "no enemy found. PC = "+pc);

            return null;
        }

        bool canAttackTargetRightNow()
        {
            if (currentTarget != null)
                if (currentTarget.inGame)
                {
                    if (WUtilites.calc2Drange(thisUnit.getPosition(), currentTarget.getPosition(thisUnit.pos)) <= thisUnit.attackRange)
                        return true;
                }

            return false;
        }

        void check()
        {
            if (currentCommand == this)
            {
                addSubcommand(typeof(WMoveCommand), targetPoint);
            }
            else if (currentCommand is WMoveCommand)
            {
                logl(thisUnit, "Moving... ");
                WUnit enemy = findEnemy();
                if (enemy != null)
                {
                    logl(thisUnit, "Run attack to unit at" + enemy.getPosition());
                    currentCommand.interrupt();
                    addSubcommand(typeof(WAttackCommand), enemy);
                    addSubcommand(typeof(WMoveCommand), targetPoint);
                    currentTarget = enemy;
                }
            }
            else if (currentCommand is WAttackCommand)
            {
                if (!canAttackTargetRightNow())
                {
                    for (int x = -1; x <= 1; x++)
                        for (int y = -1; y <= 1; y++)
                            if (!((x == 0) && (y == 0)))
                                if (W.core.world.pointInWorld(x, y))
                                {
                                    List<WUnit> uns = W.core.world.UnitsInWorld(thisUnit.getPosition().X + x, thisUnit.getPosition().Y + y);
                                    if (uns.Count > 0)
                                    {
                                        WUnit other = uns[0];
                                        if (thisUnit.isEnemy(other))
                                        {
                                            addSubcommand(typeof(WAttackCommand), other, 0);
                                            currentTarget = other;
                                        }
                                    }
                                }
                }
            }
        }

        override protected void run(WUnit unit)
        {
            thisUnit = (WSimpleAttackingUnit)unit;
            targetPoint = (Point)param;

            logl(thisUnit, "run: " + targetPoint);

            check();
        }

        override protected void onNextMA(WUnit unit)
        {
            check();
        }

        public override String getDbgPictString(WUnit unit, IWXNAControl g, Rectangle area)
        {
            return "M!";
            //g.DrawString("  AM", new Font("Arial", 10), new SolidBrush(Color.Black), area);
        }
    }
}
