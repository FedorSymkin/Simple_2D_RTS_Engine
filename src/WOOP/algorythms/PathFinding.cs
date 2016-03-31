using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;
using System.Windows.Forms;
using System.Diagnostics;
using FibonacciHeapNS;

namespace WOOP
{
    public abstract class AbstractPathFinder
    {
        public abstract List<Point> execute(WUnit unit, Point pointFrom, Point PointTo, out bool destinationSuccess);

        protected virtual String LogTag(WUnit unit)
        {
            if (unit == null) return this.GetType().Name + ": ";
            else return String.Format("Unit {0} - {1}: ", unit.GetType().Name, this.GetType().Name);
        }
        protected virtual void logo(WUnit unit, String text) { W.core.textLogs.OptimizationLog.log(LogTag(unit) + text); }
    }

    public class AStarPathFinder : AbstractPathFinder
    {
        FibonacciHeap<double, AStarPoint> OpenList = new FibonacciHeap<double, AStarPoint>();

        AStarPoint[,] matrix;
        Point pointFrom;
        Point PointTo;
        WUnit unit;
        static Point LookedPoint;
        AStarPoint minRangePoint;
        int minRangeValue = -1;

        public bool usePermissions = false;
        public int maxPointsCount;
        public int pCount = 0;

        public enum AStarPointState
        {
            Open,
            Closed
        }

        public AStarPathFinder()
        {
            OpenList.compareFunc = new FibonacciHeap<double, AStarPoint>.compareFuncType(CompareAStarPoints);
        }

        public class AStarPoint
        {
            public Point pos;
            public double F;
            public double H;
            public double G;
            public AStarPointState state;
            public AStarPoint parent;
        }



        void clear()
        {
            OpenList.clear();
            minRangeValue = -1;
        }

        int calc2Drange(Point p1, Point p2)
        {
            int dx = (Math.Abs(p2.X - p1.X));
            int dy = (Math.Abs(p2.Y - p1.Y));

            if (dx >= dy) return dx; else return dy;
        }

        double getH(Point p)
        {
            return (Math.Abs(p.X - PointTo.X) + Math.Abs(p.Y - PointTo.Y));
        }

        double getCost(AStarPoint pt1, AStarPoint pt2)
        {
            Point p1 = pt1.pos;
            Point p2 = pt2.pos;

            if ((p1.X != p2.X) && (p1.Y != p2.Y)) return 14;
            else return 10;
        }

        AStarPoint initFirstPoint()
        {
            AStarPoint pt = new AStarPoint();
            pt.pos = pointFrom;
            pt.G = 0;
            pt.H = getH(pointFrom);
            pt.F = pt.G + pt.H;
            pt.state = AStarPointState.Open;
            pt.parent = null;

            matrix[pointFrom.X, pointFrom.Y] = pt; 

            return pt;
        }

        void testForMinRange(AStarPoint p)
        {
            if (p.pos.Equals(pointFrom)) return;

            int r = calc2Drange(p.pos, PointTo);

            if ((minRangeValue == -1))
            {
                this.minRangePoint = p;
                minRangeValue = r;
                return;
            }

            if (r < minRangeValue)
            {
                this.minRangePoint = p;
                minRangeValue = r;
            }
        }

        AStarPoint finalPoint;
        bool ProcessingOpenPoint()
        {
            AStarPoint p = OpenList.ExtractMin().value;
            testForMinRange(p);

            p.state = AStarPointState.Closed;

            for (int x = p.pos.X - 1; x <= p.pos.X + 1; x++)
            for (int y = p.pos.Y - 1; y <= p.pos.Y + 1; y++)
            if ((x != p.pos.X) || (y != p.pos.Y))
            {
                if (!W.core.world.pointInWorld(x, y)) continue;

                LookedPoint.X = x;
                LookedPoint.Y = y;


                AStarPoint adjacent = matrix[x, y];

                if (unit.CanPlacedTo(x, y))
                {
                    if (adjacent != null)
                    {
                        if (adjacent.state == AStarPointState.Open)
                        {
                            if (adjacent.G < p.G)
                            {
                                adjacent.parent = p;
                                adjacent.G = p.G + getCost(p, adjacent);
                                adjacent.F = adjacent.G + adjacent.H;
                            }
                        }
                    }
                    else //adding a new point
                    {
                        if (usePermissions)
                        {
                            pCount++;
                            if (pCount > maxPointsCount) return false;
                        }

                            AStarPoint child = new AStarPoint();
                            child.pos = new Point(x, y);
                            child.parent = p;
                            child.G = p.G + getCost(p, child);
                            child.H = getH(child.pos);
                            child.F = child.G + child.H;
                            child.state = AStarPointState.Open;

                            matrix[x, y] = child;
                            OpenList.insertNode(child.F, child);

                            if (child.pos.Equals(PointTo))
                            {
                                finalPoint = child;
                                return true;
                            }
                    }
                }
            }

            return false;
        }

        bool CompareAStarPoints(double p1, double p2)
        {
            return p1 <= p2;
        }

        public void MakePath(ref List<Point> res, AStarPoint finP)
        {
            if (finP != null)
            {
                AStarPoint p = finP;
                while (p.parent != null)
                {
                    res.Insert(0, p.pos);
                    p = p.parent;
                }
            }
        }


        int maxOpenCount = 0;
        int startRange;
        public override List<Point> execute(WUnit unit, Point pointFrom, Point PointTo, out bool destinationSuccess)
        {
            Stopwatch timer = Stopwatch.StartNew();
            startRange = calc2Drange(pointFrom,PointTo);
            destinationSuccess = false;
            maxOpenCount = 0;
            pCount = 0;

            List<Point> res = new List<Point>();
            this.pointFrom = pointFrom;
            this.PointTo = PointTo;
            this.unit = unit;
            clear();
            matrix = new AStarPoint[W.core.world.Width, W.core.world.Height];

            AStarPoint fpt = initFirstPoint();
            OpenList.insertNode(fpt.F, fpt);

            while (OpenList.getCount() > 0)
            {
                if (OpenList.getCount() > maxOpenCount) maxOpenCount = OpenList.getCount();

                if (ProcessingOpenPoint())
                {
                    destinationSuccess = true;
                    MakePath(ref res, finalPoint);
                    break;
                }
            }

            //if we can't find path to destination point -> we will make path to nearest point
            if (!destinationSuccess)
            {
                MakePath(ref res, minRangePoint);
            }


            timer.Stop();
            logo(unit, String.Format("AStar executed. Range = {0}; Max open points count = {1}; time = {2} ms", startRange, maxOpenCount, timer.ElapsedMilliseconds));

            return res;
        }

    }

    public class WAStarTestUnit : WMovingUnit
    {
        public override AbstractPathFinder getPathFinder()
        {
            return new AStarPathFinder();
        }
    }
}
