using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public partial class WWaterGraph : Graph
    {
        void logl(String text)
        {
            W.core.textLogs.AlgLog.log("WaterAlg: " + text);
        }

        void logo(String text)
        {
            W.core.textLogs.OptimizationLog.log("WaterAlg: " + text);
        }

        public GraphEdge? findAdjacent(GraphNode node, GraphNode adj)
        {
            foreach (var x in node.adjacent)
                if (x.node == adj) return x;

            return null;
        }

        bool compareLists(List<WWaterZone> List1, List<WWaterZone> List2)
        {
            if (List1.Count != List2.Count) return false;

            for (int i = 0; i < List1.Count; i++)
                if (List1[i] != List2[i]) return false;

            return true;
        }

        int calc2Drange(Point p1, Point p2)
        {
            double dx = (Math.Abs(p2.X - p1.X));
            double dy = (Math.Abs(p2.Y - p1.Y));

            double r = Math.Sqrt(dx * dx + dy * dy);

            return (int)Math.Round(r);
        }

        //find point in border by condition: Range(searched point, p1) + Range(searched point, p2) = min
        Point findNearestByTwo(Point p1, Point p2, List<Point> border)
        {
            bool started = false;
            int minSum = 0;
            Point res = border[0];

            foreach (Point b in border)
            {
                int r1 = calc2Drange(p1, b);
                int r2 = calc2Drange(p2, b);
                int r = r1 + r2;

                if ((minSum > r) || (!started))
                {
                    started = true;
                    minSum = r;
                    res = b;
                }
            }

            return res;
        }

        String toAstarDot()
        {
            HashSet<NodesPair> alr = new HashSet<NodesPair>();

            String res = "";
            res += "graph Graph1 {\n";
            for (int i = 0; i < this.nodes.Count; ++i)
            {
                String cost = "";
                String Mark = "";

                if (nodes[i] is WWaterBorder) cost = ((WWaterBorder)nodes[i]).astarCost.ToString();
                if (nodes[i] == currentAstarOpenPoint) Mark += " -----> ";
                if (resPath.Contains(nodes[i])) Mark += "PATH ";

                if (nodes[i] == zStart) Mark += "START ";
                if (nodes[i] == zEnd) Mark += "END ";

                String cstr = "black";
                if (AOpenPoints.Contains(nodes[i])) cstr = "red";
                if (AClosedPoints.Contains(nodes[i])) cstr = "blue";

                res += String.Format("{0} [label = \"{1}{2}({3})\"] [color = {4}]\n", i, Mark, i, cost, cstr);
            }

            foreach (var n in nodes)
            {
                foreach (var a in n.adjacent)
                {
                    if (!alr.Contains(new NodesPair(a.node, n)))
                    {
                        res += String.Format("{0}--{1}\n", a.node, n);
                        alr.Add(new NodesPair(a.node, n));
                    }
                }

            }

            res += "}";

            return res;
        }
    }
}
