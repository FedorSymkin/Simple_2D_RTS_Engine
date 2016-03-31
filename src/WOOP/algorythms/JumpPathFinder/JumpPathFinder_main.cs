using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using WOOP;
using System.Reflection;
using FibonacciHeapNS;
using System.Diagnostics;

namespace WOOP
{
    public partial class JumpPathFinder : AbstractPathFinder
    {
        #if JPS_BLOCK_DEBUG
            JPoint nowProcessig;        
            List<Point> wasVisited = new List<Point>();
            JPSDebugger dbg;
            Point processingNeighbor = new Point(-1,-1);
            JumpSnapshot curSnapDbg;
            List<Point> neighborsPts = new List<Point>();
        #endif

        //total lists
        FibonacciHeap<uint, JPoint> OpenList = new FibonacciHeap<uint, JPoint>();
        JPoint[,] matrix;
        byte[,] freeMatrix;
        List<Point> resPath = new List<Point>();
        List<MarkedPoint> neighbors = new List<MarkedPoint>();
        

        //total var
        JPoint finalPoint;     
        WUnit unit;
        Point pointFrom;
        Point PointTo;
        JPoint minRangePoint;
        int minRangeValue = -1;

        //For macro mode
        public bool macroMode = false;
        public WWaterZone zoneFrom;
        public WWaterZone zoneMid;
        public WWaterZone zoneTo; 
   
        
        //additiponal debug var (no blocking)
        int maxOpenCount = 0;
        int startRange;
        //===========================================================================



        public JumpPathFinder()
        {
            OpenList.compareFunc = new FibonacciHeap<uint, JPoint>.compareFuncType(CompareJPoints);
            MarkedPointComp = new Comparison<MarkedPoint>(CompMarkedPoints);
        }


        public override List<Point> execute(WUnit unit, Point pointFrom, Point PointTo, out bool destinationSuccess)
        {
            //Init params
            destinationSuccess = false;
            this.pointFrom = pointFrom;
            this.PointTo = PointTo;
            this.unit = unit;
            clear();
            matrix = new JPoint[W.core.world.Width, W.core.world.Height];
            _W = W.core.world.Width;
            _H = W.core.world.Height;
            makeFreeMatrix();
            this.macroMode = macroMode;


            //Init debug
            Stopwatch timer = Stopwatch.StartNew();
            startRange = (int)calc2Drange(pointFrom, PointTo);
            #if JPS_BLOCK_DEBUG
                startDebug();
            #endif

            //Init first point
            JPoint fpt = initFirstPoint();
            fpt.nodeInFibonacciHeap = OpenList.insertNode(fpt.cost, fpt);

            //Body
            while (OpenList.getCount() > 0)
            {
                if (OpenList.getCount() > maxOpenCount) maxOpenCount = OpenList.getCount();

                #if JPS_BLOCK_DEBUG 
                    blockDebug("Before iteration; OpenList count = " + OpenList.getCount(), 8); 
                #endif
                if (ProcessingOpenPoint(OpenList.ExtractMin().value))
                {
                    destinationSuccess = true;
                    MakePath(ref resPath, finalPoint);
                    break;
                }
            }
            if (!destinationSuccess) MakePath(ref resPath, minRangePoint); //if we can't find path to destination point -> we will make path to nearest point


            //debug
            #if JPS_BLOCK_DEBUG  
                endDebug();
            #endif
            timer.Stop();
            logo(unit, String.Format("J executed. Range = {0}; Max open points count = {1}; time = {2} ms", startRange, maxOpenCount, timer.ElapsedMilliseconds));
            

            //return
            return resPath;
        }
    }

    public class WJPSTestUnit : WMovingUnit
    {
        public override AbstractPathFinder getPathFinder()
        {
            return new JumpPathFinder();
        }
    }
}
