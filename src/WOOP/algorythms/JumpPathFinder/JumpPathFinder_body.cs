using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using FibonacciHeapNS;
using System.Drawing;
using System.Diagnostics;
using System.Windows.Forms;
using System.Reflection;

namespace WOOP
{
    public partial class JumpPathFinder : AbstractPathFinder
    {
        private enum JPointState
        {
            Open,
            Closed
        }

        private class JPoint
        {
            public Point pos;

            public uint cost;
            public uint costOfPath;
            public uint heuristic;

            public JPointState state;
            public JPoint parent;

            public FHNode<uint, JPoint> nodeInFibonacciHeap;
        }


        void clear()
        {
            OpenList.clear();
            minRangeValue = -1;
        }

        uint calc2Drange(Point p1, Point p2)
        {
            int dx = (Math.Abs(p2.X - p1.X));
            int dy = (Math.Abs(p2.Y - p1.Y));

            if (dx >= dy) return (uint)dx; else return (uint)dy;
        }

        JPoint initFirstPoint()
        {
            JPoint pt = new JPoint();
            pt.pos = pointFrom;
            defineCost(pt);
            pt.state = JPointState.Open;
            pt.parent = null;

            matrix[pointFrom.X, pointFrom.Y] = pt;

            return pt;
        }

        void testForMinRange(JPoint p)
        {
            if (p.pos.Equals(pointFrom)) return;

            int r = (int)calc2Drange(p.pos, PointTo);

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

        
        bool ProcessingOpenPoint(JPoint p)
        {
            //debug
            #if JPS_BLOCK_DEBUG 
                nowProcessig = p;
                blockDebug("Processing point", 7);
            #endif

            //testForMinRange
            testForMinRange(p);

            //drop point state
            p.state = JPointState.Closed;
            p.nodeInFibonacciHeap = null;

            //get neighbors
            neighbors = getNeighbors(p);
            #if JPS_BLOCK_DEBUG 
                neighborsPts.Clear();
                foreach (MarkedPoint mp in neighbors) neighborsPts.Add(mp.pos);
                blockDebug("Neighbors. Count = )" + neighbors.Count, 7);
            #endif

            //foreach neighbors
            foreach (MarkedPoint Mneighbor in neighbors)
            {
                Point neighbor = Mneighbor.pos;

                //debug
                #if JPS_BLOCK_DEBUG 
                    processingNeighbor = neighbor;
                    blockDebug("Processing Neighbor", 6);
                #endif

                //jump
                Point? j = jumpLoop(p.pos, neighbor);
                if (j != null)
                {
                    Point jpnt = (Point)j;
                    JPoint jumpPoint = matrix[jpnt.X, jpnt.Y];
                    if (jumpPoint == null)
                    {
                        jumpPoint = new JPoint();
                        jumpPoint.pos = jpnt;
                        jumpPoint.parent = p;
                        defineCost(jumpPoint);

                        jumpPoint.state = JPointState.Open;
                        jumpPoint.nodeInFibonacciHeap = OpenList.insertNode(jumpPoint.cost, jumpPoint);
                        
    
                        matrix[jpnt.X, jpnt.Y] = jumpPoint;                       
                        if (jumpPoint.pos.Equals(PointTo))
                        {
                            finalPoint = jumpPoint;
                            return true;
                        }
                    }
                    else
                    {
                        if (jumpPoint.state == JPointState.Open)
                        {
                            if (jumpPoint.cost < p.cost)
                            {
                                jumpPoint.parent = p;
                                defineCost(jumpPoint);

                                FHNode<uint, JPoint> fnode = jumpPoint.nodeInFibonacciHeap;
                                OpenList.changeKey(fnode, jumpPoint.cost);
                            }
                        }
                    }
                }
            }
            return false;
        }

        uint getHeuristic(Point p)
        {
            return calc2Drange(PointTo, p);
        }

        void defineCost(JPoint p)
        {
            if (p.parent == null) p.cost = 0;
            else
            {
                p.costOfPath = p.parent.costOfPath + calc2Drange(p.pos, p.parent.pos);
                p.heuristic = getHeuristic(p.pos);
                p.cost = p.costOfPath + p.heuristic;
            }
        }

        private class MarkedPoint
        {
            public MarkedPoint(Point p, uint m)
            {
                pos = p;
                mark = m;
            }

            public Point pos;
            public uint mark;
        }



        //Добавляет белых соседей вокруг точки, т.е. тех, которых мы будем обрабатывать джампами -
        //с которых будет прыгать
        //И да, здесь проверяется на canPLaced 

        List<MarkedPoint> getNeighbors(JPoint p)
        {
            JPoint tParent = p.parent;
            int tX = p.pos.X;
            int tY = p.pos.Y;
            int tPx, tPy, tDx, tDy;
            List<MarkedPoint> tNeighbors = new List<MarkedPoint>();
            Point pt;

            // directed pruning: can ignore most neighbors, unless forced.
            if (tParent != null)
            {
                tPx = tParent.pos.X;
                tPy = tParent.pos.Y;
                // get the normalized direction of travel
                tDx = (tX - tPx) / Math.Max(Math.Abs(tX - tPx), 1);
                tDy = (tY - tPy) / Math.Max(Math.Abs(tY - tPy), 1);

                // search diagonally
                if (tDx != 0 && tDy != 0)
                {
                    if (isFree(tX, tY + tDy)) //8 is free
                    {                        
                        pt = new Point(tX, tY + tDy); //add P8
                        tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));                
                    }
                    if (isFree(tX + tDx, tY)) //6 is free
                    {

                        pt = new Point(tX + tDx, tY); //add P6
                        tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                    }


                    pt = new Point(tX + tDx, tY + tDy); //add P9
                    tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                    

                    //4 is blocked, 7, 8 is free
                    if (isFree(tX - tDx, tY + tDy))
                    {
                        if (isFree(tX, tY + tDy) && !isFree(tX - tDx, tY))
                        {
                            pt = new Point(tX - tDx, tY + tDy); //add P7
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }
                    }

                    //2 is blocked 3,6 is free
                    if (isFree(tX + tDx, tY - tDy))
                    {
                        if (isFree(tX + tDx, tY) && !isFree(tX, tY - tDy))
                        {
                            pt = new Point(tX + tDx, tY - tDy); //add P3
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }
                    }
                }
                // search horizontally/vertically
                else
                {
                    if (tDx == 0)
                    {
                        if (isFree(tX, tY + tDy))
                        {
                            pt = new Point(tX, tY + tDy);
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }

                        if (isFree(tX + 1, tY + tDy) && !isFree(tX + 1, tY))
                        {
                            pt = new Point(tX + 1, tY + tDy);
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }
                        if (isFree(tX - 1, tY + tDy) && !isFree(tX - 1, tY))
                        {
                            pt = new Point(tX - 1, tY + tDy);
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }
                    }
                    else
                    {
                        if (isFree(tX + tDx, tY))
                        {
                            pt = new Point(tX + tDx, tY);
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }
                        
                        //9 free, 8 blocked
                        if (isFree(tX + tDx, tY + 1) && !isFree(tX, tY + 1))
                        {
                            pt = new Point(tX + tDx, tY + 1);
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }

                        //3 free, 2 blocked
                        if (isFree(tX + tDx, tY - 1) && !isFree(tX, tY - 1))
                        {
                            pt = new Point(tX + tDx, tY - 1);
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }
                        
                    }
                }
            }
            // return all neighbors if no parent
            else
            {
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        if (!(x == 0 && y == 0))
                        {
                            pt = new Point(p.pos.X + x, p.pos.Y + y);
                            tNeighbors.Add(new MarkedPoint(pt, getHeuristic(pt)));
                        }
                    }
            }

            tNeighbors.Sort(MarkedPointComp);
            return tNeighbors;
        }

        Comparison<MarkedPoint> MarkedPointComp = null;
        int CompMarkedPoints(MarkedPoint p1, MarkedPoint p2)
        {
            return p1.mark.CompareTo(p2.mark);
        }        

        bool CompareJPoints(uint p1, uint p2)
        {
            return p1 <= p2;
        }

        void MakePathGroup(ref List<Point> res, JPoint p1, JPoint p2)
        {
            int dx = (p2.pos.X - p1.pos.X) / Math.Max(Math.Abs(p2.pos.X - p1.pos.X), 1);
            int dy = (p2.pos.Y - p1.pos.Y) / Math.Max(Math.Abs(p2.pos.Y - p1.pos.Y), 1);

            int x = p1.pos.X;
            int y = p1.pos.Y;
            while (true)
            {
                if ((p2.pos.X == x) && (p2.pos.Y == y)) break;
                res.Insert(0, new Point(x, y));
                x += dx;
                y += dy;
            }
        }

        void MakePath(ref List<Point> res, JPoint finP)
        {
            if (finP != null)
            {
                JPoint p = finP;
                while (p.parent != null)
                {
                    MakePathGroup(ref res, p, p.parent);
                    p = p.parent;
                }
            }
        }


        void makeFreeMatrix()
        {
            freeMatrix = new byte[_W, _H];
        }

        int _W;
        int _H;
        bool isFree(int x, int y)
        {
            if (x < 0) return false;
            if (y < 0) return false;
            if (x >= _W) return false;
            if (y >= _H) return false;

            if (macroMode)
            {
                WWaterZone z = W.core.world.graph.matrix[x, y];
                if ((z != zoneFrom) && (z != zoneMid) && (z != zoneTo)) return false;
            }

            if (freeMatrix[x, y] == 0) freeMatrix[x, y] = unit.CanPlacedTo(x, y) ? (byte)1 : (byte)2;   
            return freeMatrix[x, y] == 1;
        }

        bool isFree(Point p) { return isFree(p.X, p.Y); }
    }
}
