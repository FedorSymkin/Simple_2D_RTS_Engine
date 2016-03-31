using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace FibonacciHeapNS
{
    public class FibonacciHeap<keyT, valueT>
    {
        public delegate bool compareFuncType(keyT key1, keyT key2);
        public compareFuncType compareFunc;
        public int getCount() { return count; }
        public void clear()
        {
            root.down = null;
            min = null;
            count = 0;
        }

        public FHNode<keyT, valueT> ExtractMin()
        {
            FHNode<keyT, valueT> extractedMin = min;
            if (min != null)
            {
                FHNode<keyT, valueT> temp;

                if (min.right != min)
                {
                    temp = min.right;
                    unConnectNode(min);
                    throwChild(min);
                    count--;
                }
                else
                {
                    unConnectNode(min);
                    throwChild(min);
                    count--;
                    temp = root.down;
                }

                min = temp;
                if (temp != null) consolidate(temp);
            }

            return extractedMin;
        }
        public FHNode<keyT, valueT> insertNode(keyT key, valueT value)
        {
            FHNode<keyT, valueT> node = new FHNode<keyT, valueT>();
            node.left = node;
            node.right = node;
            node.key = key;
            node.value = value;

            connectNode(node, root);
            count++;

            if (min == null) min = node;
            else if (compareFunc(node.key, min.key)) min = node;

            return node;
        }
        public FHNode<keyT, valueT> getMin()
        {
            return min;
        }
        public void delete(FHNode<keyT, valueT> node)
        {
            throwChild(node);
            cutToRoot(node);
            ExtractFromRoot(node);
        }
        public void changeKey(FHNode<keyT, valueT> node, keyT newKey)
        {
            throwChild(node);
            cutToRoot(node);
            node.key = newKey;
            consolidate(node);
        }

        //==========================================================================================

        FHNode<keyT, valueT> min;
        FHNode<keyT, valueT> root = new FHNode<keyT, valueT>();
        int count = 0;

        public FibonacciHeap()
        {
            compareFunc = new compareFuncType(stdCompare);
        }



        void unConnectNode(FHNode<keyT, valueT> node)
        {
            //unconnect from parent
            if (node.up != null)
            {
                if (node.up.down == node)
                {
                    if (node.right != node) node.up.down = node.right; else node.up.down = null;
                }
                if (node.up.degree > 0) node.up.degree--;
                node.up = null;
            }

            //unconnect from sisters
            node.left.right = node.right;
            node.right.left = node.left;

            //now my friends is only me :(
            node.right = node;
            node.left = node;
        }

        void connectNode(FHNode<keyT, valueT> node, FHNode<keyT, valueT> parent)
        {
            //connect to parent
            node.up = parent;
            if (parent.down == null) parent.down = node;
            parent.degree++;

            //my new friends :)
            node.left = parent.down;
            node.right = parent.down.right;

            //connect to sisters
            node.left.right = node;
            node.right.left = node;
        }

        void MoveNode(FHNode<keyT, valueT> node, FHNode<keyT, valueT> newParent)
        {
            if (node == newParent) throw new Exception("Trying to set self-parent");
            unConnectNode(node);
            connectNode(node, newParent);
        }

        bool stdCompare(keyT key1, keyT key2)
        {
            object o1 = (object)key1;
            object o2 = (object)key2;

            if ((o1 is Int32) && (o2 is Int32))
                return (Int32)o1 < (Int32)o2;
            else
                throw new Exception("Cannot compare type " + o1.GetType().Name);
        }

        void throwChild(FHNode<keyT, valueT> node)
        {
            while (node.down != null) MoveNode(node.down, root);
        }

        void consolidate(FHNode<keyT, valueT> startNode)
        {
            if (count >= 0xFFFF) throw new Exception("Fibonacci heap too big (>FFFF)");
            FHNode<keyT, valueT>[] MatchNodesByDegree = new FHNode<keyT, valueT>[16];


            FHNode<keyT, valueT> curr = startNode;
            FHNode<keyT, valueT> stopNode = startNode;
            do
            {
                FHNode<keyT, valueT> willNext = curr.right;

                if (min == null) min = curr;
                else if (compareFunc(curr.key, min.key)) min = curr;

                while (MatchNodesByDegree[curr.degree] != null)
                {
                    FHNode<keyT, valueT> matchNode = MatchNodesByDegree[curr.degree];
                    MatchNodesByDegree[curr.degree] = null;

                    FHNode<keyT, valueT> whoIsLost;
                    FHNode<keyT, valueT> whoIsStay;
                    if (compareFunc(matchNode.key, curr.key)) //matchNode is less
                    {
                        whoIsLost = curr;
                        whoIsStay = matchNode;
                    }
                    else
                    {
                        whoIsLost = matchNode;
                        whoIsStay = curr;
                    }

                    if (min == null) min = whoIsStay;
                    else if (compareFunc(whoIsStay.key, min.key)) min = whoIsStay;


                    if (whoIsLost == stopNode) stopNode = stopNode.right;
                    if (whoIsLost == willNext) willNext = willNext.right;
                    if (whoIsLost == min) min = whoIsStay;


                    MoveNode(whoIsLost, whoIsStay);

                    curr = whoIsStay;
                }
                MatchNodesByDegree[curr.degree] = curr;

                curr = willNext;
            }
            while (curr != stopNode);
        }

        FHNode<keyT, valueT> ExtractFromRoot(FHNode<keyT, valueT> node)
        {
            FHNode<keyT, valueT> extracted = node;
            if (node != null)
            {
                FHNode<keyT, valueT> temp;

                if (node.right != node)
                {
                    temp = node.right;
                    unConnectNode(node);
                    throwChild(node);
                    count--;
                }
                else
                {
                    unConnectNode(node);
                    throwChild(node);
                    count--;
                    temp = root.down;
                }

                if (temp != null) consolidate(temp);
            }

            return extracted;
        }

        void cutToRoot(FHNode<keyT, valueT> node)
        {
            if (node.up != root)
            {
                FHNode<keyT, valueT> wasUp = node.up;

                unConnectNode(node);
                connectNode(node, root);
                if (wasUp.mark)
                {
                    wasUp.mark = false;
                    cutToRoot(wasUp);
                }
                else node.up.mark = true;
            }
        }
    }

    public class FHNode<keyT, valueT>
    {
        public keyT key;
        public valueT value;
        public FHNode<keyT, valueT> up = null;
        public FHNode<keyT, valueT> left;
        public FHNode<keyT, valueT> right;
        public FHNode<keyT, valueT> down = null;
        public int degree = 0;
        public bool mark = false;
    }
}
