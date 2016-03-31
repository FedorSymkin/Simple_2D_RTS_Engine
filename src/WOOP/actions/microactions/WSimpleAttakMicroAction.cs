using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    class WSimpleAttakMicroAction : WMicroAction
    {
        public override void BeginAction(WUnit unit)
        {
            loga(unit, "Begin on unit in " + unit.getPosition());

            WUnit target = (WUnit)param;
            if (target != null)
            {
                int px = target.getPosition(unit.pos).X - unit.getPosition().X;
                int py = target.getPosition(unit.pos).Y - unit.getPosition().Y;

                if (px > 0) px = 1; else if (px < 0) px = -1;
                if (py > 0) py = 1; else if (py < 0) py = -1;

                unit.rotateDirection = new Point(px, py);
            }
        }


        public override void EndAction(WUnit unit)
        {
            loga(unit, "End on unit in " + unit.getPosition());

            WUnit target = (WUnit)param;
            //logl(unit, "End of attack MA target = " + target.getPosition());

            if (target != null) 
            {
                if (target.inGame)
                {
                    target.Damage(unit, (int)((WSimpleAttackingUnit)unit).damagePower);
                }
            }
        }

        public override String getDbgPictString(WUnit unit, IWXNAControl g, Rectangle area)
        {
            return "a";
          //  g.DrawString("a", new Font("Arial", 10), new SolidBrush(Color.Black), area.X, area.Y + area.Height / 3);
        }
    }
}
