using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public interface IWBuilding
    {

    }

    public class WBuilding : WUnit, IWBuilding
    {
        public override string unitName()
        {
            return "Здание";
        }

        public override Rectangle getDrawRect(Point screenPoint)
        {
            Size cellSize = W.core.gameField.cellSize;

            return new Rectangle(
                (getPosition().X - screenPoint.X) * cellSize.Width,
                (getPosition().Y - screenPoint.Y) * cellSize.Height,
                cellSize.Width * width,
                cellSize.Height * height
                );
        }

        List<Point> _positions = new List<Point>();

        public override void init()
        {
            base.init();
            width = 3;
            height = 3;
            maxHitPoints = 5000;
            hitPoints = maxHitPoints;
        }


        public int width{ set; get; }
        public int height{ set; get; }

        List<Point> makeNewPositions(Point TopLeft)
        {
            List<Point> res = new List<Point>();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    res.Add(new Point(TopLeft.X + x, TopLeft.Y + y));
                }

            return res;
        }

        public override bool inScreen()
        {
            Point ps = getPosition(W.core.players.watchPlayer.centerOFScreen);
            return W.core.viewRect.Contains(ps) && (W.core.players.watchPlayer.mapIsOpen[ps.X, ps.Y]);
        }


        public override void ClearWorldFromMe()             
        {
            foreach (var p in _positions) ClearWorldCellFromMe(p);
        }

        public bool CanBuildingPlacedTo(Point pos)
        {
            List<Point> PosArray = makeNewPositions(pos);

            foreach (var ip in PosArray) if (!this.CanPlacedTo(ip)) return false;

            return true;
        }

        public override void setPosition(Point value)
        {          
            bool ok = true;
            List<Point> newPositions = makeNewPositions(value);

            foreach (var ip in newPositions)
            {
                if (!this.CanPlacedTo(ip))
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
            {
                foreach (var ip in _positions) ClearWorldCellFromMe(ip);
                _positions = newPositions;
                foreach (var ip in newPositions) SetMeToWorldCell(ip);
                See();
                loga("Building position changed to " + value);
            }
            else
            {
                logm("Error: can't placed building to " + value);
            }
        }

        public override Point getPosition(Point? neareastTo = null)
        {
            if (neareastTo != null)
            {
                int mr = -1;
                Point p = new Point(0, 0);
                foreach (var ip in _positions)
                {
                    int r = WUtilites.calc2Drange(neareastTo.Value, ip);
                    if ((r < mr) || (mr == -1))
                    {
                        p = ip;
                        mr = r;
                    }
                }

                return p;
            }
            else if (_positions.Count > 0)
            {
                return _positions.First();
            }
            else
            {
                return new Point(0,0);
            }
        }
    }
}