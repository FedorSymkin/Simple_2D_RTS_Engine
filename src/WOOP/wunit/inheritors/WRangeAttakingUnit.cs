using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public interface IWRangeAttackingUnit
    {
    }

    public class WRangeAttackingUnit : WSimpleAttackingUnit, IWRangeAttackingUnit
    {
        public ShellTextures shellTextures { get; private set; }
        public float shellSpeedMsPerCell { get; set; }
        public bool useAnimatedShell { get; set; }

        public override int AttackSpeedMsPerCell
        {
            get
            {
                return (int)MsPerMA(typeof(WRangeAttakMicroAction));
            }
            set
            {
                SetMsPerMA(typeof(WRangeAttakMicroAction), (uint)value);
            }
        }

        public override void init()
        {
            base.init();
            shellTextures = W.core.shells.getTextures(this.GetType());

            attackRange = 6;
            shellSpeedMsPerCell = 80;
            AttackSpeedMsPerCell = 900;
            useAnimatedShell = true;
        }

        public override String unitName()
        {
            return "Archer";
        }

        public override String getParamStr()
        {
            String res = base.getParamStr();
            res += String.Format("Дальность атаки: {0} кл\n", this.attackRange);
            return res;
        }
    }
}