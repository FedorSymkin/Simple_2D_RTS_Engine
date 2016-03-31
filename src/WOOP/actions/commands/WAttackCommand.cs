using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

namespace WOOP
{
    class WAttackCommand : WCommand
    {
        public WUnit targetUnit { get; private set; }
        private WSimpleAttackingUnit thisUnit;

        Type getAttackMAType(WUnit unit)
        {
            return (unit is WRangeAttackingUnit) ?  typeof(WRangeAttakMicroAction) : typeof(WSimpleAttakMicroAction);
        }

        override protected void run(WUnit unit)
        {
            targetUnit = (WUnit)param;
            thisUnit = (WSimpleAttackingUnit)unit;
            logl(unit, "Attack run. Target = " + targetUnit.getPosition());

            int r = WUtilites.calc2Drange(thisUnit.getPosition(), targetUnit.getPosition(thisUnit.pos));

            if (r <= thisUnit.attackRange)
            {
                logl(unit, "Target in range: " + targetUnit.getPosition());
                addMA(getAttackMAType(thisUnit), targetUnit);
            }
            else
            {
                logl(unit, "Target out of range: " + targetUnit.getPosition());
                addSubcommand(typeof(WMoveCommand), targetUnit.getPosition(thisUnit.pos));
            }
        }

        override protected void MAFailed(WUnit unit, WMicroAction action)
        {
            W.core.FatalError("Attack: WTF??");
        }

        override protected void onNextMA(WUnit unit)
        {
            if ((targetUnit != null) &&(targetUnit.inGame))
            {
                if (currentCommand is WMoveCommand) //check is unit already in attack range
                {
                    int r = WUtilites.calc2Drange(thisUnit.getPosition(), targetUnit.getPosition(thisUnit.pos));
                    if (r <= thisUnit.attackRange)
                    {
                        logl(unit, "I have gone to target: " + targetUnit.getPosition());
                        currentCommand.interrupt();
                        addMA(getAttackMAType(thisUnit), targetUnit);
                    }
                }
                else if (currentCommand == this) //check if unit is not in attack range
                {
                    int r = WUtilites.calc2Drange(thisUnit.getPosition(), targetUnit.getPosition(thisUnit.pos));
                    if (r <= thisUnit.attackRange)
                    {
                        if (this.actionsCount() == 0) addMA(getAttackMAType(thisUnit), targetUnit);
                    }
                    else
                    {
                        logl(unit, "I missed target " + targetUnit.getPosition());
                        addSubcommand(typeof(WMoveCommand), targetUnit.getPosition(thisUnit.pos));
                    }
                }
            }
            else
            {
                logl(unit, "Target not in game");
                interrupt();
            }
        }

        public override String getDbgPictString(WUnit unit, IWXNAControl g, Rectangle area)
        {
            return "A";
            //g.DrawString("A", new Font("Arial", 10), new SolidBrush(Color.Black), area);
        }
    }
}
