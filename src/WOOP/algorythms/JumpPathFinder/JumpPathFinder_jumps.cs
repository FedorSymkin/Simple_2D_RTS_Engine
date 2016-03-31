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
        private class JumpSnapshot
        {
            public int iX;
            public int iY;
            public int iPx;
            public int iPy;
            public int tDx;
            public int tDy;
            public Point? jx;
            public Point? jy;
            public int stage;
            public JumpSnapshot()
            {

                iX = 0;
                iY = 0;
                iPx = 0;
                iPy = 0;
                tDx = 0;
                tDy = 0;
                jx = null;
                jy = null;
                stage = 0;
            }

#if JPS_BLOCK_DEBUG
            public Point pos { get { return new Point(iX, iY); } set { iX = value.X; iY = value.Y; } }
            public Point parentPos { get { return new Point(iPx, iPy); } set { iPx = value.X; iPy = value.Y; } }
            public Point D { get { return new Point(tDx, tDy); } set { tDx = value.X; tDy = value.Y; } }
#endif

            /*
            * Diagonal designations (path is p1 -> p5 (other paths is symmetric if use Dx/Dy instead specific values))
                p1      p2      p3
            *   p4      p5=pos  p6
            *   p7      p8      p9
            */
            public Point p1D { get { return new Point(iX - tDx, iY - tDy); } }
            public Point p2D { get { return new Point(iX, iY - tDy); } }
            public Point p3D { get { return new Point(iX + tDx, iY - tDy); } }
            public Point p4D { get { return new Point(iX - tDx, iY); } }
            public Point p5D { get { return new Point(iX, iY); } }
            public Point p6D { get { return new Point(iX + tDx, iY); } }
            public Point p7D { get { return new Point(iX - tDx, iY + tDy); } }
            public Point p8D { get { return new Point(iX, iY + tDy); } }
            public Point p9D { get { return new Point(iX + tDx, iY + tDy); } }

            /*
 * Horizontal designations (path is p4 -> p5) (other paths is symmetric))
    p1      p2      p3
 *  p4      p5=pos  p6
 *  p7      p8      p9
 */
            public Point p1H { get { return new Point(iX - tDx, iY - 1); } }
            public Point p2H { get { return new Point(iX, iY - 1); } }
            public Point p3H { get { return new Point(iX + tDx, iY - 1); } }
            public Point p4H { get { return new Point(iX - tDx, iY); } }
            public Point p5H { get { return new Point(iX, iY); } }
            public Point p6H { get { return new Point(iX + tDx, iY); } }
            public Point p7H { get { return new Point(iX - tDx, iY + 1); } }
            public Point p8H { get { return new Point(iX, iY + 1); } }
            public Point p9H { get { return new Point(iX + tDx, iY + 1); } }


            /*
* Vertical designations (path is p2 -> p5)(other paths is symmetric))
   p1      p2      p3
*  p4      p5=pos  p6
*  p7      p8      p9
*/
            public Point p1V { get { return new Point(iX - 1, iY - tDy); } }
            public Point p2V { get { return new Point(iX, iY - tDy); } }
            public Point p3V { get { return new Point(iX + 1, iY - tDy); } }
            public Point p4V { get { return new Point(iX - 1, iY); } }
            public Point p5V { get { return new Point(iX, iY); } }
            public Point p6V { get { return new Point(iX + 1, iY); } }
            public Point p7V { get { return new Point(iX - 1, iY + tDy); } }
            public Point p8V { get { return new Point(iX, iY + tDy); } }
            public Point p9V { get { return new Point(iX + 1, iY + tDy); } }

            public bool isDiagonal { get { return tDx != 0 && tDy != 0; } }
            public bool isVertical { get { return tDy != 0; } }
            public bool isHorizontal { get { return tDx != 0; } }
        }


        /*Эта функция возвращает точки цели прыжка. Вызывается для всех открытых точек. 
         *Это основное отличие от А-стара. Т.е. здесь следующая открытая точка возвращается в результате
         *прыжка, а не просмотра соседей.
        
         * Реализует мнимую рекурсию - использует локальный стек таким образом, что создаётся "ощущение
         * текущей функции и рекурсии"
         Работа функции разбита на stages, где каждый следующий stage будет выполнен только после того,
         как "поток выполнения" вернутся на текущую как бы "функцию". Есть переменная retVal, в котрой
         хранится значение, возвращаемое "мнимой функцией"*/
        //parentPoint, thisPoint - в качестве параметра любого джампа берётся не только точка, 
        //от которой прыжок, но и движение от одной точки к другой (его направление)

        Point? jumpHohizontal(Point parentPoint, Point thisPoint)
        {
            int iX;
            int iY;
            int iPx;
            int iPy;
            int tDx;

            //Инициализация первой точки
            iX = thisPoint.X; iY = thisPoint.Y;
            iPx = parentPoint.X; iPy = parentPoint.Y;

            
            //цикл основной
            while (true)
            {
#if JPS_BLOCK_DEBUG
                JumpSnapshot curSnap = new JumpSnapshot();
                curSnap.iX = iX;
                curSnap.iY = iY;
                curSnap.iPx = iPx;
                curSnap.iPy = iPy;

                curSnapDbg = curSnap;
                wasVisited.Add(new Point(curSnap.iX, curSnap.iY));
                blockDebug("Jump iteration", 5);
#endif

                //проверка на доступность точки и на то, является ли данная точка целевой
                if (!isFree(iX, iY))
                {
                    return null;
                }
                else if (PointTo.X == iX && PointTo.Y == iY)
                {
                    return new Point(iX, iY);
                }

                tDx = iX - iPx;


             // if ((isFree(      p9H       ) && !isFree(    p8H   )) || (isFree(   p3H          ) && !isFree(    p2H   )))
                if ((isFree(iX + tDx, iY + 1) && !isFree(iX, iY + 1)) || (isFree(iX + tDx, iY - 1) && !isFree(iX, iY - 1)))
                {
                    return new Point(iX, iY);
                }

                iPx = iX;
                iX = iX + tDx;
            }
        }

        Point? jumpVertical(Point parentPoint, Point thisPoint)
        {
            int iX;
            int iY;
            int iPx;
            int iPy;
            int tDy;

            //Инициализация первой точки
            iX = thisPoint.X; iY = thisPoint.Y;
            iPx = parentPoint.X; iPy = parentPoint.Y;

            //цикл основной
            while (true)
            {
#if JPS_BLOCK_DEBUG
                JumpSnapshot curSnap = new JumpSnapshot();
                curSnap.iX = iX;
                curSnap.iY = iY;
                curSnap.iPx = iPx;
                curSnap.iPy = iPy;

                curSnapDbg = curSnap;
                wasVisited.Add(new Point(iX, iY));
                blockDebug("Jump iteration", 5);
#endif
                //проверка на доступность точки и на то, является ли данная точка целевой
                if (!isFree(iX, iY))
                {
                    return null;
                }
                else if (PointTo.X == iX && PointTo.Y == iY)
                {
                    return new Point(iX, iY);
                }

                tDy = iY - iPy;


                //if ((isFree(p7V) &&            !isFree(p4V))        || (isFree(p9V)              && !isFree(p6V))
                if ((isFree(iX - 1, iY + tDy) && !isFree(iX - 1, iY)) || (isFree(iX + 1, iY + tDy) && !isFree(iX + 1, iY)))
                {
                    return new Point(iX, iY);
                }

                iPy = iY;
                iY = iY + tDy;
            }
        }

        Point? jumpDiagonal(Point parentPoint, Point thisPoint)
        {
            int iX;
            int iY;
            int iPx;
            int iPy;
            int tDx;
            int tDy;

            //Инициализация первой точки
            iX = thisPoint.X; iY = thisPoint.Y;
            iPx = parentPoint.X; iPy = parentPoint.Y;

            while (true)
            {
#if JPS_BLOCK_DEBUG
                JumpSnapshot curSnap = new JumpSnapshot();
                curSnap.iX = iX;
                curSnap.iY = iY;
                curSnap.iPx = iPx;
                curSnap.iPy = iPy;

                curSnapDbg = curSnap;
                wasVisited.Add(new Point(curSnap.iX, curSnap.iY));
                blockDebug("Jump iteration", 5);
#endif
                if (!isFree(iX, iY))
                {
                    return null;
                }
                else if (PointTo.X == iX && PointTo.Y == iY)
                {
                    return new Point(iX, iY);
                }

                tDx = iX - iPx;
                tDy = iY - iPy;


                //if ((isFree(p7D)              && !isFree(p4D))          || (isFree(p3D)                && !isFree(p2D)))
                if ((isFree(iX - tDx, iY + tDy) && !isFree(iX - tDx, iY)) || (isFree(iX + tDx, iY - tDy) && !isFree(iX, iY - tDy)))
                {
                    return new Point(iX, iY);
                }

                Point? ret;
                ret = jumpHohizontal(new Point(iX, iY), new Point(iX + tDx, iY));
                if (ret != null) return new Point(iX, iY);

                ret = jumpVertical(new Point(iX, iY), new Point(iX, iY + tDy));
                if (ret != null) return new Point(iX, iY);

                iPx = iX;
                iPy = iY;
                iX = iX + tDx; 
                iY = iY + tDy;
            }
        }


        Point? jumpLoop(Point parentPoint, Point thisPoint)
        {
            int dx = thisPoint.X - parentPoint.X;
            int dy = thisPoint.Y - parentPoint.Y;

            if ((dx != 0) && (dy != 0)) return jumpDiagonal(parentPoint, thisPoint);
            else if (dx != 0) return jumpHohizontal(parentPoint, thisPoint);
            else return jumpVertical(parentPoint, thisPoint);
        }
    }
}
