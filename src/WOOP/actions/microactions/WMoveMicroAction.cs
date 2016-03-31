using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public class WMoveMicroAction : WMicroAction
    {
        bool error;
        public override void BeginAction(WUnit unit)
        {
            loga(unit, "Begin on unit in " + unit.getPosition());
            error = false;
            beginPoint = unit.getPosition();

            if ((direction.X == 0) && (direction.Y == 0))
            {
                logm(unit, "Error: no direction");
                error = true;
            }
            else if (!(unit is WMovingUnit))
            {
                logm(unit, "Error: unit is not moving unit");
                error = true;
            }
            else
            {            
                ((WMovingUnit)unit).shiftDirection = direction;
                ((WMovingUnit)unit).rotateDirection = direction;
                unit.SetMeToWorldCell(endPoint);
            }
        }

        public override void EndAction(WUnit unit)
        {
            if (!error)
            {
                loga(unit, "End on unit in " + unit.getPosition());

                unit.setPosition(endPoint);
                ((WMovingUnit)unit).shiftDirection = new Point(0,0);
            }
        }

        public override bool canExecute(WUnit unit)
        {
            beginPoint = unit.getPosition();
            return unit.CanPlacedTo(endPoint);
        }

        Point paramPoint { get { return (Point)this.param; } }
        Point beginPoint;
        Point direction
        {
            get
            {
                Point res = new Point(0, 0);
                if (paramPoint.X > beginPoint.X) res.X = 1;
                else if (paramPoint.X < beginPoint.X) res.X = -1;
                else res.X = 0;

                if (paramPoint.Y > beginPoint.Y) res.Y = 1;
                else if (paramPoint.Y < beginPoint.Y) res.Y = -1;
                else res.Y = 0;

                return res;
            }
        }

        Point endPoint
        {
            get
            {
                Point res = beginPoint;
                res.Offset(direction);
                return res;
            }
        }

        public override String getDbgPictString(WUnit unit, IWXNAControl g, Rectangle area)
        {
            return "g";
            //g.DrawString("g", new Font("Arial", 10), new SolidBrush(Color.Black), area.X + area.Height/2, area.Y + area.Height/3);
        }
    }
}
