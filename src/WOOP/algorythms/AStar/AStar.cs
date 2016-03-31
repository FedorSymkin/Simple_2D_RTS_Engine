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
    public class AStar : IDisposable
    {
        public delegate void AStarFinishCallback();
        public delegate bool AStarPointCallback(int x, int y);
        public delegate double AStarHeuristicCallback(int x, int y);

        //input interface=================================================
        public List<Point> initialPoints;        
        public AStarPointCallback checkPointFunction;
        public AStarHeuristicCallback heuristicFunction;
        public bool useBlockDebug = false;
        public int maxPointsPerTick = 0;

        public AStar(   List<Point> initialPoints, 
                        AStarPointCallback checkPointFunction,
                        AStarHeuristicCallback heuristicFunction,
                        int maxPointsPerTick = 0) 
        { 
            this.initialPoints = initialPoints;


            this.checkPointFunction = checkPointFunction != null ? 
                                      checkPointFunction : new AStarPointCallback(AlyawsTrueCheckFunc);

            this.heuristicFunction = heuristicFunction != null ?
                                        heuristicFunction : new AStarHeuristicCallback(NonHeuristicFunc);

            this.maxPointsPerTick = maxPointsPerTick;
            finished = false;

            OpenList.compareFunc = new FibonacciHeap<double, AStarPoint>.compareFuncType(CompareAStarPoints);
        }

        AStarGraphicsDebugger debugger = null;
        public void applyDebug() 
        {
            useBlockDebug = true;
            debugger = new AStarGraphicsDebugger();
            debugger.astar = this;
            W.core.gameField.graphicDebuggers.Add(debugger);
        }

        public void Dispose()
        {
            if (debugger != null) W.core.gameField.graphicDebuggers.Remove(debugger);
        }

        //output interface=================================================
        public AStarPointCallback onNewPoint;
        public AStarPointCallback onClosePoint;
        public AStarFinishCallback onFinishAll;
        public bool finished {get; private set;}
        AStarPoint getMatrixPoint(int x, int y)
        {
            return matrix[x, y];
        }
        public int spentActions { get; private set; }



        //run & private=================================================
        AStarPoint[,] matrix;
        FibonacciHeap<double, AStarPoint> OpenList = new FibonacciHeap<double, AStarPoint>();

        public void startExecution()
        {
            spentActions = 0;

            if (initialPoints.Count == 0)
            {
                W.core.textLogs.AlgLog.log("error: cannot run A Star without start point");
                finished = true;
                return;
            }

            matrix = new AStarPoint[W.core.world.Width, W.core.world.Height];
            OpenList.clear();
            if (!initFirstPoints())
            {
                finished = true;
                return;
            }

            continueExecution();
        }

        private int currentItersCount = 0;
        public void continueExecution()
        {
            currentItersCount = 0;

            while (OpenList.getCount() > 0)
            {
                if (ProcessingOpenPoint())
                {
                    finished = true;
                    return;
                }

                //return, but not finish (need call continueExecution again)
                spentActions++;
                if ((maxPointsPerTick > 0) && (++currentItersCount >= maxPointsPerTick)) return;
            }

            if (onFinishAll != null) onFinishAll();
            finished = true;
        }

        bool ProcessingOpenPoint()
        {
            AStarPoint p = OpenList.ExtractMin().value;
            if (useBlockDebug)
            {
                currentPoint = p;
                BlockingDebug.block("AStar ProcessingOpenPoint", 0, false);
            }

            p.state = AStarPointState.Closed;
            if (onClosePoint != null)
            {
                if (!onClosePoint(p.pos.X, p.pos.Y)) return true;
            }

            for (int x = p.pos.X - 1; x <= p.pos.X + 1; x++)
                for (int y = p.pos.Y - 1; y <= p.pos.Y + 1; y++)
                    if ((x != p.pos.X) || (y != p.pos.Y))
                    {
                        if (!W.core.world.pointInWorld(x, y)) continue;

                        AStarPoint adjacent = matrix[x, y];
                        if (checkPointFunction(x,y))
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
                                AStarPoint child = new AStarPoint();
                                child.pos = new Point(x, y);
                                child.parent = p;
                                child.G = p.G + getCost(p, child);
                                child.H = heuristicFunction(child.pos.X, child.pos.Y);
                                child.F = child.G + child.H;
                                child.state = AStarPointState.Open;

                                matrix[x, y] = child;
                                OpenList.insertNode(child.F, child);

                                if (onNewPoint != null)
                                {
                                    if (!onNewPoint(x, y)) return true;
                                }
                            }
                        }
                    }

            return false;
        }

        double getCost(AStarPoint pt1, AStarPoint pt2)
        {
            Point p1 = pt1.pos;
            Point p2 = pt2.pos;

            if ((p1.X != p2.X) && (p1.Y != p2.Y)) return 14;
            else return 10;
        }

        bool CompareAStarPoints(double p1, double p2)
        {
            return p1 <= p2;
        }

        bool AlyawsTrueCheckFunc(int x, int y)
        {
            return true;
        }

        double NonHeuristicFunc(int x, int y)
        {
            return 0;
        }
  
        bool initFirstPoints()
        {
            foreach (var userPoint in initialPoints)
            {
                if (onNewPoint != null)
                {
                    if (!onNewPoint(userPoint.X, userPoint.Y)) return false;
                }

                AStarPoint pt = new AStarPoint();
                pt.pos = userPoint;
                pt.G = 0;
                pt.H = heuristicFunction(userPoint.X, userPoint.Y);
                pt.F = pt.G + pt.H;
                pt.state = AStarPointState.Open;
                pt.parent = null;

                matrix[userPoint.X, userPoint.Y] = pt;
                OpenList.insertNode(pt.F, pt);      
            }

            return true;
        }
        
        public enum AStarPointState
        {
            Open,
            Closed
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

        //graphic debug=================================================
        AStarPoint currentPoint = null;
        public class AStarGraphicsDebugger : WGraphicsDebugger
        {
            public AStar astar;

            override public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                AStarPoint p = astar.matrix[cellCoord.X, cellCoord.Y];
                if (p != null)
                {
                    if (astar.currentPoint == p)
                    {
                        g.DrawString("*",
                            new Font("arial", 34),
                            new SolidBrush(Color.Yellow),
                            new Point(area.Left, area.Top)
                            );
                    }
                    else switch (p.state)
                    {
                        case AStarPointState.Open:
                            g.DrawString("" + Math.Round(p.F, 1),
                                            new Font("arial", 10),
                                            new SolidBrush(Color.Red),
                                            new Point(area.Left, area.Top)
                                            );
                            break;

                        case AStarPointState.Closed:
                            g.DrawString("x",
                                            new Font("arial", 14),
                                            new SolidBrush(Color.Black),
                                            new Point(area.Left, area.Top)
                                            );
                            break;
                    }
                }
            }
        }
    }


    public class AStarTest
    {
        public void run()
        {
            List<Point> points = new List<Point>();
            points.Add(new Point(4, 10));
            points.Add(new Point(12, 3));
            points.Add(new Point(4, 15));
            points.Add(new Point(0, 0));

            W.core.textLogs.ZadrotLog.log("AStar_Test started");
            AStar astar = new AStar(points, checkFunc, Heu, 400);
            astar.onNewPoint = onNewPoint;
            astar.onFinishAll = onFinishAll;
            astar.applyDebug();

            astar.startExecution();
            while (!astar.finished)
            {
                BlockingDebug.block("b", 9, false);
                astar.continueExecution();
            }
        }

        public double Heu(int x, int y)
        {
            return 0;
        }

        public bool checkFunc(int x, int y)
        {
            return W.core.world.terrainType( W.core.world.getTerrain(x,y) ) == "allowed";
        }

        public bool onNewPoint(int x, int y)
        {
           /* if ((x == 5) && (y == 6))
            {
                return false;
            }*/
            return true;
        }

        public void onFinishAll()
        {
            W.core.textLogs.ZadrotLog.log("AStar_Test finished");
        }
    }
}
