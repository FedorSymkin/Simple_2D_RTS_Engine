using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Windows.Forms;

namespace WOOP
{
    public interface IWMovingUnit
    {
        void onRightClickCommand(Point pos);
        void onRightClickCommand(WUnit OtherUnit);

		//Automatically generated:
		void Move(Point pos, Boolean force);
		void Move(Int32 x, Int32 y, Boolean force);
		Int32 SpeedMsPerCell {get;set;}
		AbstractPathFinder getPathFinder();
	}

    public class WMovingUnit : WUnit, IWMovingUnit
    {
        public bool moveFlag = false;
        public void Move(Point pos, bool force = true) { Move(pos.X, pos.Y, force); }
        public void Move(int x, int y, bool force = true)
        {
            logg("Move to " + new Point(x,y).ToString());
            AddCommand(typeof(WMoveCommand), new Point(x, y), force);
        }

        public int SpeedMsPerCell
        {
            get
            {
                return (int)MsPerMA(typeof(WMoveMicroAction));
            }
            set
            {
                SetMsPerMA(typeof(WMoveMicroAction), (uint)value);
            }
        }

        public virtual AbstractPathFinder getPathFinder()
        {
            return new JumpPathFinder();
        }

        public override void  onRightClickCommand(Point pos)
        {
            this.Move(pos);
        }

        public override void onRightClickCommand(WUnit OtherUnit)
        {
            this.Move(OtherUnit.getPosition());
        }

        public override String getParamStr()
        {
            String res = base.getParamStr();
            res += String.Format("—корость: {0} мс/кл\n", this.SpeedMsPerCell);
            return res;
        }
    }
}
