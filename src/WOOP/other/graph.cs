using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using QuickGraph;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using System.IO;
using System.Diagnostics;
using System.Drawing;

namespace WOOP
{
    public class GraphNode
    {
        public int index;
        public Graph parentGraph;
        public List<GraphEdge> adjacent = new List<GraphEdge>();
        public override String ToString() { return index.ToString(); }
    }

    public struct GraphEdge
    {
        public GraphEdge(GraphNode node, int cost, GraphNode midzone)
        {
            this.node = node;
            this.cost = cost;
            this.midzone = midzone;
        }
        public GraphNode node;
        public int cost;
        public GraphNode midzone;
    }

    public class Graph
    {
        public List<GraphNode> nodes = new List<GraphNode>();

        public GraphNode addNewNode()
        {
            GraphNode node = new GraphNode();
            nodes.Add(node);
            node.parentGraph = this;
            node.index = nodes.Count - 1;

            return node;
        }

        

        public GraphNode addExistingNode(GraphNode node)
        {
            nodes.Add(node);
            node.parentGraph = this;
            node.index = nodes.Count - 1;

            return node;
        }

        public void connectNodes(GraphNode nodeFrom, GraphNode nodeTo, GraphNode midzone = null, int cost = 0)
        {
            nodeFrom.adjacent.Add(new GraphEdge(nodeTo, cost, midzone));
            nodeTo.adjacent.Add(new GraphEdge(nodeFrom, cost, midzone));

            logl("connected " + nodeFrom + " and " + nodeTo);
        }

        public void removeLastNode()
        {
            if (this.nodes.Count > 0)
            {
                GraphNode n = this.nodes.Last();
                foreach (var a in n.adjacent) unconnectNodes(n, a.node);
                nodes.RemoveAt(nodes.Count - 1);
            }
        }

    

        public void unconnectFromGraph(GraphNode n)
        {
            for (int i = n.adjacent.Count - 1; i >= 0; i--) unconnectNodes(n, n.adjacent[i].node);
        }

        int findEdgeIndex(GraphNode centralNode, GraphNode DesirdAdjacent)
        {
            for (int i = 0; i < centralNode.adjacent.Count; ++i)
            {
                if (centralNode.adjacent[i].node == DesirdAdjacent) return i;
            }
            return -1;
        }

        public void unconnectNodes(GraphNode nodeFrom, GraphNode nodeTo)
        {
            int i; 

            i = findEdgeIndex(nodeFrom, nodeTo);
            if (i != -1) nodeFrom.adjacent.RemoveAt(i);

            i = findEdgeIndex(nodeTo, nodeFrom);
            if (i != -1) nodeTo.adjacent.RemoveAt(i);

            logl("unconnected " + nodeFrom + " and " + nodeTo);
        }

        void logl(String text)
        {
            W.core.textLogs.AlgLog.log("Graph: " + text);
        }

        public void clear()
        {
            nodes.Clear();
        }


        public struct NodesPair
        {
            public NodesPair(GraphNode n1, GraphNode n2)
            {
                minNode = n1.index < n2.index ? n1 : n2;
                maxNode = n1.index < n2.index ? n2 : n1;
            }

            public GraphNode minNode;
            public GraphNode maxNode;
        }

        public String toDot()
        {
            HashSet<NodesPair> alr = new HashSet<NodesPair>();

            String res = "";
            res += "graph Graph1 {\n";
            for (int i = 0; i < this.nodes.Count; ++i)
            {
                res += String.Format("{0} [label = \"{1}\"]\n", i, i);
            }

            foreach (var n in nodes)
            {
                foreach (var a in n.adjacent)
                {
                    if (!alr.Contains(new NodesPair(a.node, n)))
                    {
                        res += String.Format("{0}--{1} [label = \"{2}\"]\n", a.node, n, a.cost);
                        alr.Add(new NodesPair(a.node, n));
                    }
                }

            }

            res += "}";

            return res;
        }

        public void LookDot(String dot)
        {
            String f = "look";

            File.WriteAllText(f + ".dot", dot);
            string p = "-Tpng " + "\"" + f + ".dot\" -o " + "\"" + f + ".png\"";

            ProcessStartInfo startInfo = new ProcessStartInfo("C:/Program Files (x86)/Graphviz 2.28/bin/dot.exe", p);
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process.Start(startInfo);

            while (true)
            {
                try
                {
                    System.Diagnostics.Process.Start(f + ".png");
                    break;
                }
                catch
                {
                    System.Threading.Thread.Sleep(500);
                }
            }
        }

        public void SaveToPictureCustomDot(String filenameWithoutExt, String dot)
        {
            File.WriteAllText(filenameWithoutExt + ".dot", dot);

            string p = "-Tpng " + "\"" + filenameWithoutExt + ".dot\" -o " + "\"" + filenameWithoutExt + ".png\"";

            ProcessStartInfo startInfo = new ProcessStartInfo("C:/Program Files (x86)/Graphviz 2.28/bin/dot.exe", p);
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process.Start(startInfo);
        }

        public void SaveToPicture(String filenameWithoutExt, bool view = false)
        {
            string output = this.toDot();
            File.WriteAllText(filenameWithoutExt + ".dot", output);

            string p = "-Tpng " + "\"" + filenameWithoutExt + ".dot\" -o " + "\"" + filenameWithoutExt + ".png\"";

            ProcessStartInfo startInfo = new ProcessStartInfo("C:/Program Files (x86)/Graphviz 2.28/bin/dot.exe", p);
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process.Start(startInfo);

            if (view)
            {
                while (true)
                {
                    try
                    {
                        System.Diagnostics.Process.Start(filenameWithoutExt + ".png");
                        break;
                    }
                    catch
                    {
                        System.Threading.Thread.Sleep(500);
                    }
                }
            }
        }
    }
}