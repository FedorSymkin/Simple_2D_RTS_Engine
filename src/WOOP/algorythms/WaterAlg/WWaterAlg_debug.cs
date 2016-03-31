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
        WaterDebugger dbg;

        enum DebugMode
        {
            make,
            astar
        }

        void startDebug(DebugMode mode)
        {
            dbg = new WaterDebugger();
            dbg.mode = mode;
            dbg.alg = this;
            W.core.gameField.graphicDebuggers.Add(dbg);
            W.core.miniMap.graphicDebuggers.Add(dbg);
        }

        void endDebug()
        {
            W.core.gameField.graphicDebuggers.Remove(dbg);
            W.core.miniMap.graphicDebuggers.Remove(dbg);
        }

        private class WaterDebugger : WGraphicsDebugger
        {
            public WWaterGraph alg;
            public DebugMode mode;
            public Point? iteratePoint = null;
            public Point? currentOpenPoint = null;
            public List<Point> resPointPath = new List<Point>();
            
            Rectangle AreaDiv(Rectangle area, float scale)
            {
                RectangleF res = new RectangleF();
                res.X = (float)area.X + (1 - scale) * (float)area.Width / 2;
                res.Y = (float)area.Y + (1 - scale) * (float)area.Height / 2;
                res.Width = (float)area.Width * scale;
                res.Height = (float)area.Height * scale;

                return new Rectangle((int)res.X, (int)res.Y, (int)res.Width, (int)res.Height);
            }

            Color getColorFromIndex(int index)
            {
                uint v = (uint)index * 40200;
                v |= 0xFF7F007F;

                return Color.FromArgb((int)v);
            }

            void drawMatrix(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                WWaterZone z = alg.matrix[cellCoord.X, cellCoord.Y];
                if (z != null)
                {
                    bool drawNumber = false;
                    bool drawRect = false;
                    bool drawMark = false;
                    bool little = false;
                   

                    if (z is WWaterMacroZone) 
                    {
                        drawRect = true;
                        drawNumber = true;
                        drawMark = false;
                        little = ((WWaterMacroZone)z).openPoints.Contains(cellCoord);
                    }
                    else if (z is WWaterBorder)
                    {
                        drawRect = true;
                        drawNumber = true;
                        drawMark = false;
                        little = false;
                    }
                    else if (z is WWaterSpecialMark)
                    {
                        drawNumber = false;
                        drawRect = false;
                        drawMark = true;
                        little = false;
                    }

                    
                    if (drawRect)
                    {
                        Color c = getColorFromIndex(z.index);

                        float f = little ? 0.3f : 0.65f;
                        if (sender == W.core.gameField) g.FillRectangle(new SolidBrush(c), AreaDiv(area, f));
                        else g.FillRectangle(new SolidBrush(c), area.X, area.Y, area.Width + 1, area.Height + 1);
                    }

                    if (drawMark)
                    {
                        g.FillRectangle(new SolidBrush(Color.White), AreaDiv(area, 0.9f));

                        WWaterSpecialMark mark = (WWaterSpecialMark)z;
                        int cnt = mark.BorderZones.Count;
                        if (sender == W.core.gameField)
                        {
                            for (int i = 0; i < cnt; i++)
                            {
                                Rectangle rect = AreaDiv(area, 0.9f);

                                rect.X += i * (rect.Width / cnt);
                                rect.Y += i * (rect.Height / cnt);

                                rect.Width /= cnt;
                                rect.Height /= cnt;

                                Color c = getColorFromIndex(mark.BorderZones[i].index);

                                g.FillRectangle(new SolidBrush(c), rect);
                            } 
                        }
                        else
                        {
                            g.FillRectangle(new SolidBrush(Color.White), area.X, area.Y, area.Width + 1, area.Height + 1);
                        }
                    }

                    if (drawNumber)
                    {
                        if (sender == W.core.gameField)
                        g.DrawString(z.index.ToString(), new Font("Arial", 14), new SolidBrush(Color.Black), new Point(area.X , area.Y ));                    
                    }
                }
            }

            void DrawPoints(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                if (mode == DebugMode.make)
                {
                    if (iteratePoint != null)
                        if (iteratePoint.Value == cellCoord)
                            g.DrawEllipse(new Pen(Color.Red, 6), AreaDiv(area, 0.4f));

                    if (currentOpenPoint != null)
                        if (currentOpenPoint.Value == cellCoord)
                            g.DrawEllipse(new Pen(Color.Blue, 2), area);
                }
                else if (mode == DebugMode.astar)
                {
                    if (alg.pStart == cellCoord)
                        g.DrawString("FROM", new Font("Arial", 10), new SolidBrush(Color.Red), area);

                    if (alg.pEnd == cellCoord)
                        g.DrawString("TO", new Font("Arial", 10), new SolidBrush(Color.Red), area);

                    if (resPointPath != null)
                    if (resPointPath.Contains(cellCoord))
                        g.DrawString("O", new Font("Arial", 18), new SolidBrush(Color.Blue), area);    
                }
            }

            void drawResPath(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                if (resPointPath != null)
                for (int i = 1; i < resPointPath.Count; i++)
                {
                    Point p1 = resPointPath[i - 1];
                    Point p2 = resPointPath[i];

                    Point p1Scr = new Point(p1.X * (int)W.core.miniMap.pixelPerPoint, p1.Y * (int)W.core.miniMap.pixelPerPoint);
                    Point p2Scr = new Point(p2.X * (int)W.core.miniMap.pixelPerPoint, p2.Y * (int)W.core.miniMap.pixelPerPoint);

                    g.DrawLine(new Pen(Color.Yellow, 3), p1Scr, p2Scr);
                }
            }

            override public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
            {
                drawMatrix(sender, g, cellCoord, area);
                DrawPoints(sender, g, cellCoord, area);

                if (mode == DebugMode.astar)
                if (sender == W.core.miniMap)
                {
                    if (cellCoord.X == W.core.world.Width - 1 && cellCoord.Y == W.core.world.Height - 1)
                    {
                        drawResPath(sender, g, cellCoord, area);
                    }
                }
            }
        }
    }
}
