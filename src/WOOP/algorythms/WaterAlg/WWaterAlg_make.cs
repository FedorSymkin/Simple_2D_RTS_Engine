using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Diagnostics;

namespace WOOP
{
    public class WWaterZone : GraphNode
    {
    }

    public class WWaterMacroZone : WWaterZone
    {
        public List<Point> openPoints = new List<Point>();
        public List<WWaterBorder> borders = new List<WWaterBorder>();
        public Point startPoint;
        public int HeuristicSize = 0;
    }

    public class WWaterSpecialMark : WWaterZone
    {
        public List<WWaterZone> BorderZones;
    }

    public class WWaterBorder : WWaterZone
    {
        public List<Point> points = new List<Point>();

        //for astar
        public WWaterBorder astarParent;
        public int astarCost;
        public Point walkPoint;
    }

    public partial class WWaterGraph : Graph
    {
        //main
        public WWaterZone[,] matrix;
        WWaterBorder startStub, endStub;

        //total params
        WWorld world;
        int SideCount;
        WUnit exampleUnit;

        //temp
        List<Point> bufferPoints = new List<Point>();  
        int zonesCtr = 0;

        //Make the map graph where vertices is border zones, edges is moving between border zones
        public void make(WWorld wrld, WUnit exampleUnit, int SideCount)
        {
            Stopwatch timer = Stopwatch.StartNew();

            if (W.core.units.Count > 0) W.core.FatalError("cannot make water graph while existing units"); 

            #if WATER_ALG_BLOCK_DEBUG
                startDebug(DebugMode.make);
            #endif

            //init
            world = wrld;
            bufferPoints.Clear();
            matrix = new WWaterZone[world.Width, world.Height];
            this.exampleUnit = exampleUnit;
            this.SideCount = SideCount;
            zonesCtr = 0;


            makeFirstZones();



            #if WATER_ALG_BLOCK_DEBUG
                BlockingDebug.block("made initial points", 5, false);
            #endif


            //propogate zones
            while (iter());
             
                

            #if WATER_ALG_BLOCK_DEBUG
                BlockingDebug.block("propogate finished", 5, false);
            #endif


            makeAdditionalZones();


            #if WATER_ALG_BLOCK_DEBUG
                BlockingDebug.block("made additional zones", 5, false);
            #endif


            makeBorderZones();


            #if WATER_ALG_BLOCK_DEBUG
                BlockingDebug.block("made border zones", 5, false);
            #endif


            //Cut all macro-zones from graph. After this graph will contains only border zones
            normalizeGraph();


            //Adding non-connected zones for start and end representation (for astar)
            addInitialZones();


            #if WATER_ALG_BLOCK_DEBUG
                this.SaveToPicture("WWaterAlg_debug", true);
                BlockingDebug.block("Make finished", 5, false);
                endDebug();
            #endif

            timer.Stop();
            logo(String.Format("made graph for world size = ({0},{1}). Time = {2} ms", world.Width, world.Height, timer.ElapsedMilliseconds));
        }

        private void makeAdditionalZones()
        {
            for (int x = 0; x < world.Width; x++)
                for (int y = 0; y < world.Height; y++)
                {
                    if (exampleUnit.CanPlacedTo(x, y))
                    {
                        if (matrix[x, y] == null)
                        {
                            #if WATER_ALG_BLOCK_DEBUG
                                BlockingDebug.block("founded empty point: " + new Point(x, y), 2, false);
                            #endif

                            WWaterMacroZone z = makeNewMacroZone(new Point(x,y));
                            while (iterateZone(z));
                        }
                    }
                }
        }

        private void addInitialZones()
        {
            startStub = new WWaterBorder();
            startStub.astarParent = null;
            this.addExistingNode(startStub);

            endStub = new WWaterBorder();
            endStub.astarParent = null;
            this.addExistingNode(endStub);
        }

        private void normalizeGraph()
        {
            logl("normalizeGraph");

            foreach (var z in this.nodes)
            {
                if (z is WWaterMacroZone)
                {
                    WWaterMacroZone wz = ((WWaterMacroZone)z);

                    List<GraphEdge> Adj = new List<GraphEdge>();
                    foreach (var x in wz.adjacent)
                    {
                        Adj.Add(x);
                        wz.borders.Add((WWaterBorder)x.node);
                    }

                    //check all pairs of adjacent elements
                    for (int i = 0; i < Adj.Count - 1; i++)
                        for (int j = i + 1; j < Adj.Count; j++)
                        {
                            WWaterZone a1 = (WWaterZone)Adj[i].node;
                            WWaterZone a2 = (WWaterZone)Adj[j].node;
                            this.unconnectNodes(a1, wz);
                            this.unconnectNodes(a2, wz); 
                            this.connectNodes(a1, a2, z);
                        }
                }
            }
        } 

        private void makeBorderZones()
        {
            foreach (var bp in bufferPoints) //bufferPoints is set of independent marks. Yet.
            {
                WWaterZone zone = matrix[bp.X, bp.Y];

                if (zone != null)
                    if (zone is WWaterSpecialMark)
                    {
                        makeBorderZone(bp);                 
                    }
            }
        }

        void makeBorderZone(Point from)
        {
            

            WWaterSpecialMark thisMark = (WWaterSpecialMark)matrix[from.X, from.Y];
          

            //adding and register new border
            WWaterBorder newBorder = new WWaterBorder();
            matrix[from.X, from.Y] = newBorder;
            this.addExistingNode(newBorder);

            String LS = ("make border zone " + newBorder);
            LS += "; border with: ";
            foreach (var x in thisMark.BorderZones) LS += x.ToString() + " ";
            logl(LS);

            //do propogation of new border
            List<Point> openPt = new List<Point>();
            newBorder.points.Add(from);
            openPt.Add(from);
            while (openPt.Count > 0)
            {
                Point p = openPt[0];
                openPt.RemoveAt(0);

                #if WATER_ALG_BLOCK_DEBUG
                    dbg.currentOpenPoint = p;
                    BlockingDebug.block("Make buffer zone", 1, false);
                #endif

                //for all 2D neighbors
                for (int x = p.X - 1; x <= p.X + 1; x++)
                    for (int y = p.Y - 1; y <= p.Y + 1; y++)
                        if (!(x == p.X && y == p.Y))
                        {
                            Point adj = new Point(x, y);

                            #if WATER_ALG_BLOCK_DEBUG
                                dbg.iteratePoint = adj;
                                BlockingDebug.block("Iterate", 0, false);
                            #endif

                            if (exampleUnit.CanPlacedTo(x, y))
                            {
                                WWaterZone zn = matrix[x, y];
                                if (zn != null)
                                    if (zn is WWaterSpecialMark) //capture only special marks...
                                        if (compareLists(((WWaterSpecialMark)zn).BorderZones, thisMark.BorderZones))//... with same adjacent zones
                                        {
                                            matrix[x, y] = newBorder;
                                            newBorder.points.Add(adj);
                                            openPt.Add(adj);
                                        }
                            }
                        }
            }

            //connecting new border to adjacent zones
            foreach (var z in thisMark.BorderZones) this.connectNodes(newBorder, z);
        }

        WWaterMacroZone makeNewMacroZone(Point p)
        {
            WWaterMacroZone z = new WWaterMacroZone();
            matrix[p.X, p.Y] = z;
            z.openPoints.Add(p);
            z.startPoint = p;
            z.HeuristicSize = 0;
            this.addExistingNode(z);

            return z;
        }

        private void makeFirstZones()
        {
            for (int i = 0; i < SideCount * SideCount; ++i)
            {
                Point? p = getFirstPoint(zonesCtr);
                if (p != null)
                {
                    makeNewMacroZone(p.Value);
                    zonesCtr++;      
                }
            }
        }

        Point? getFirstPoint(int newIndex)
        {
            //define the start point
            int ix = newIndex % SideCount;
            int iy = newIndex / SideCount;
            int x = (world.Width / SideCount) * ix + (world.Width / (SideCount * 2));
            int y = (world.Height / SideCount) * iy + (world.Height / (SideCount * 2));

            //if cannot placed - check neighbors
            if (!exampleUnit.CanPlacedTo(x, y))
            {
                List<Point> pts = W.core.units.getPointsHeap(new Point(x, y), exampleUnit, 2048); //todo - optimize it
                bool exit = true;
                foreach (var p in pts)
                {
                    if (exampleUnit.CanPlacedTo(p))
                    {
                        x = p.X;
                        y = p.Y;
                        exit = false;
                        break;
                    }
                }
                if (exit) return null;
            }

            return new Point(x,y);
        }


        bool iter()
        {
            #if WATER_ALG_BLOCK_DEBUG
                BlockingDebug.block("Iterate propagating zones", 1, false);
            #endif

            WWaterMacroZone minz = null;
            int sz = -1;
            foreach (var z in nodes)
            {
                WWaterMacroZone m = (WWaterMacroZone)z;
                if (m.openPoints.Count > 0) 
                {
                    if ((sz == -1) || (m.HeuristicSize < sz))
                    {
                        sz = m.HeuristicSize;
                        minz = m;
                    }
                }
            }

            if (minz == null) return false;
            iterateZone(minz);

            return true;
        }

        bool iterateZone(WWaterMacroZone zone)
        {
            bool res = false;

            if (zone.openPoints.Count > 0)
            {
                res = true;
                Point p = zone.openPoints[0];

                #if WATER_ALG_BLOCK_DEBUG
                    dbg.currentOpenPoint = p;
                #endif

                zone.openPoints.RemoveAt(0);
                //For all 2D neighbors
                for (int x = p.X - 1; x <= p.X + 1; x++)
                    for (int y = p.Y - 1; y <= p.Y + 1; y++)
                        if (!(x == p.X && y == p.Y))
                        {
                            #if WATER_ALG_BLOCK_DEBUG
                                dbg.iteratePoint = new Point(x, y);
                                BlockingDebug.block("Iterate one point", 0, false);
                            #endif

                            if (exampleUnit.CanPlacedTo(x, y))
                            {
                                if (matrix[x, y] == null) //only for not visited cells
                                {
                                    Point np = new Point(x, y);
                                    //maybeBorder = true if current neighbor is adjacent with another zone
                                    if (!maybeBorder(np, zone)) 
                                    {
                                        //adding self to cell
                                        zone.openPoints.Add(np);
                                        matrix[x, y] = zone;

                                        int r = calc2Drange(np, zone.startPoint);
                                        if (r > zone.HeuristicSize) zone.HeuristicSize = r;
                                    }
                                }
                            }
                        }
            }

            return res;
        }

        bool maybeBorder(Point p, WWaterZone MotherZone)
        {
            List<WWaterZone> borderZones = new List<WWaterZone>();
            borderZones.Add(MotherZone); //description below

            //for all 2D neighbors
            for (int x = p.X - 1; x <= p.X + 1; x++)
                for (int y = p.Y - 1; y <= p.Y + 1; y++)
                    if (!(x == p.X && y == p.Y))
                    {
                        if (exampleUnit.CanPlacedTo(x, y))
                        {
                            WWaterZone z = matrix[x, y];
                            if (z != null)
                            {
                                //if adjacent is another zone (but SpecialMark)
                                if ((z != MotherZone) && (!(z is WWaterSpecialMark)))
                                {
                                    if (!borderZones.Contains(z)) borderZones.Add(z); //mark another zone
                                }
                            }
                        }
                    }

            //if there are else adjacent zones besides this - make special mark
            if (borderZones.Count > 1)
            {
                if (WZComp == null) WZComp = new Comparison<WWaterZone>(WZCompFunc);
                borderZones.Sort(WZComp); //sort list for compare match in future

                WWaterSpecialMark mark = new WWaterSpecialMark();
                mark.BorderZones = borderZones;
                matrix[p.X, p.Y] = mark;
                bufferPoints.Add(p);
                return true;
            }

            return false;
        }

        Comparison<WWaterZone> WZComp = null;
        int WZCompFunc(WWaterZone z1, WWaterZone z2)
        {
            return z1.index.CompareTo(z2.index);
        }

    }
}
