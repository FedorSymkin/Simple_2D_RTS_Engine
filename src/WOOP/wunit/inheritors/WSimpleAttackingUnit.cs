using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public interface IWSimpleAttackingUnit
    {
        void onRightClickCommand(Point pos);
        void onRightClickCommand(WUnit OtherUnit);

		//Automatically generated:
		void attack(WUnit target, Boolean force);
		void attackingMove(Point pos, Boolean force);
		Int32 attackRange {get;set;}
		Int32 damagePower {get;set;}
	}

    public class WSimpleAttackingUnit : WMovingUnit, IWSimpleAttackingUnit
    {
        public int attackRange { get; set; }
        public int damagePower { get; set; }

        public virtual int AttackSpeedMsPerCell
        {
            get
            {
                return (int)MsPerMA(typeof(WSimpleAttakMicroAction));
            }
            set
            {
                SetMsPerMA(typeof(WSimpleAttakMicroAction), (uint)value);
            }
        }

        public WSimpleAttackingUnit()
        {
            attackRange = 1;
            damagePower = 20;
            SpeedMsPerCell = 400;
        }

        public void attack(WUnit target, bool force = true)
        {
            logg("Attak to " + target + " at " + target.getPosition());
            AddCommand(typeof(WAttackCommand), target, force);
        }

        public void attackingMove(Point pos, bool force = true)
        {
            AddCommand(typeof(WAttackingMoveCommand), pos, force);
        }

        public override void onRightClickCommand(Point pos)
        {
            this.attackingMove(pos);
        }

        public override void onRightClickCommand(WUnit OtherUnit)
        {
            this.attack(OtherUnit);
        }

        public override String unitName()
        {
            return "Footman";
        }

        public override String getParamStr()
        {
            String res = base.getParamStr();
            res += String.Format("Сила атаки: {0}\n", this.damagePower);
            res += String.Format("Скорость атаки: {0} мс\n", this.AttackSpeedMsPerCell);
            return res;
        }
    }
}
