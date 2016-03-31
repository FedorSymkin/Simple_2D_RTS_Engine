using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using WOOP;
using System.Reflection;

namespace WOOP
{
    public partial class JumpPathFinder : AbstractPathFinder
    {
        #if JPS_BLOCK_DEBUG
        Form dbgForm;
        void startDebug()
        {
            //make debugger
            dbg = new JPSDebugger();
            dbg.alg = this;
            W.core.gameField.graphicDebuggers.Add(dbg);

            //make top-level widget for debug
            dbgForm = dbg.makeSymbolsForm();
            dbgForm.Show();
            dbgForm.Left = 750;

            //pause
            blockDebug("Jump Path Finder execute start", 9);
        }

        void endDebug()
        {
            blockDebug("Jump Path Finder execute end", 9, false);
            dbgForm.Close();
            W.core.gameField.graphicDebuggers.Remove(dbg);
        }

        void blockDebug(String msg, int importance = 0, bool tout = true)
        {
            BlockingDebug.block(msg, importance, tout);
        }   

        private class JPSDebugger : WGraphicsDebugger
        {
            public JumpPathFinder alg;

            Rectangle AreaDiv(Rectangle area, float scale)
            {
                RectangleF res = new RectangleF();
                res.X = (float)area.X + (1 - scale) * (float)area.Width / 2;
                res.Y = (float)area.Y + (1 - scale) * (float)area.Height / 2;
                res.Width = (float)area.Width * scale;
                res.Height = (float)area.Height * scale;

                return new Rectangle((int)res.X, (int)res.Y, (int)res.Width, (int)res.Height);
            }

            public Panel makeSymbol(MethodInfo method)
            {
                //label
                Label L = new Label();
                L.Text = method.Name;
                L.AutoSize = true;

                //image
                PictureBox I = new PictureBox();
               // I.Image = new Bitmap(I.Width, I.Height);
                //WXNAControl g = 
               // using (WXNAControl g = Graphics.FromImage(I.Image))
             //   {
             //       object[] prm = new object[3];
              //       prm[0] = g;
              //      prm[1] = new Point(0, 0);
                //    prm[2] = new Rectangle(new Point(0, 0), W.core.gameField.cellSize);
              //  //   method.Invoke(this, prm);
             //   }

                //parent
                TableLayoutPanel res = new TableLayoutPanel();
                WUtilites.setAnchors(res);
                res.Controls.Add(L, 0, 0);
                //res.Controls.Add(I, 1, 0);
                res.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                res.AutoSize = true;
                return res;
            }

            public Form makeSymbolsForm()
            {
                Form form = new Form();
                TableLayoutPanel panel = new TableLayoutPanel();
                WUtilites.setAnchors(panel);
                panel.AutoSize = true;
                panel.Parent = form;
                panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

                MethodInfo[] methods = GetType().GetMethods();
                foreach (var m in methods)
                {
                    if (m.Name.Contains("drawElement"))
                    {
                        Panel p = makeSymbol(m);
                        panel.Controls.Add(p, 0, panel.RowCount);
                    }
                }

               // form.Left = 500;
               // form.Top = 500;
                form.AutoSize = true;
                form.Text = "JPS Symbols (use importance keys 5-9)";

                form.Location = new Point(500, 500);
                return form;
            }

            //============================================================================
            public void drawElementJumpFrom(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawRectangle(new Pen(Color.Red), AreaDiv(area, 1.0f));
                g.DrawRectangle(new Pen(Color.Red), AreaDiv(area, 0.8f));
                g.DrawRectangle(new Pen(Color.Red), AreaDiv(area, 0.6f));
            }

            public void drawElementNeighbor(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawString("N", new Font("Arial", 12), new SolidBrush(Color.Teal), area);
            }

            public void drawElementNeighborInProcessing(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawString("N", new Font("Arial", 12), new SolidBrush(Color.Teal), area);
                g.DrawRectangle(new Pen(Color.Red), AreaDiv(area, 1.0f));
            }

            public void drawElementOpenPoint(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.FillRectangle(new SolidBrush(Color.Red), AreaDiv(area, 0.5f));
            }

            public void drawElementClosePoint(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.FillRectangle(new SolidBrush(Color.DarkRed), AreaDiv(area, 0.5f));
            }

            public void drawElementCurrentPoint(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawEllipse(new Pen(Color.Red, 3), area);
            }

            public void drawElementCurrentParent(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawEllipse(new Pen(Color.Blue, 3), AreaDiv(area, 0.7f));
            }

            public void drawElementVisitedPoint(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawEllipse(new Pen(Color.Black, 4), AreaDiv(area, 0.2f));
            }

            public void drawElementPointFrom(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawString("FROM", new Font("Arial", 9), new SolidBrush(Color.Red), area);
            }

            public void drawElementPointTo(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                g.DrawString("TO", new Font("Arial", 9), new SolidBrush(Color.Red), area);
            }

            //============================================================================
            void drawNeighbors(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                if (alg.processingNeighbor == cellCoord) drawElementNeighborInProcessing(g, cellCoord, area);
                if (alg.neighborsPts.Contains(cellCoord)) drawElementNeighbor(g, cellCoord, area);
            }

            void drawMatrixPoints(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                JPoint pnt = alg.matrix[cellCoord.X, cellCoord.Y];

                if (pnt != null)
                {
                    if (pnt.state == JPointState.Open) drawElementOpenPoint(g, cellCoord, area);
                    else if (pnt.state == JPointState.Closed) drawElementClosePoint(g, cellCoord, area);
                }

                if (alg.nowProcessig != null)
                if (alg.nowProcessig.pos == cellCoord)
                {
                    drawElementJumpFrom(g, cellCoord, area);
                }

                if (cellCoord == alg.pointFrom) drawElementPointFrom(g, cellCoord, area);
                if (cellCoord == alg.PointTo) drawElementPointTo(g, cellCoord, area);
            } 

            void drawCurrentSnapshotPoints(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                if (alg.curSnapDbg != null)
                {
                    if (alg.curSnapDbg.pos == cellCoord) drawElementCurrentPoint(g, cellCoord, area);
                    if (alg.curSnapDbg.parentPos == cellCoord) drawElementCurrentParent(g, cellCoord, area);
                }
            }

            void drawVisitedPoints(IWXNAControl g, Point cellCoord, Rectangle area)
            {
                if (alg.wasVisited.Contains(cellCoord))
                {
                    drawElementVisitedPoint(g, cellCoord, area);
                }
            }



            override public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                drawNeighbors(g, cellCoord, area);
                drawMatrixPoints(g, cellCoord, area);
                drawCurrentSnapshotPoints(g, cellCoord, area);
                drawVisitedPoints(g, cellCoord, area);
            }


            override public void debugDraw(Object sender, IWXNAControl g, Rectangle area)
            {
                if (alg.resPath.Count > 0)
                {
                    List<Point> points = new List<Point>();
                    foreach (var p in alg.resPath) points.Add(p);
                    points.Insert(0, alg.pointFrom);
                    Point prev = new Point(-1, -1);
                    foreach (var p in points)
                    {
                        bool ok = false;
                        Point scrC = W.core.gameField.WorldToScreenCellCenter(p, ref ok);

                        if (prev.X != -1)
                        {
                            g.DrawLine(new Pen(Color.Yellow), scrC, prev);
                        }

                        prev = scrC;
                    }
                }
            }
        }
#endif
    }
}
