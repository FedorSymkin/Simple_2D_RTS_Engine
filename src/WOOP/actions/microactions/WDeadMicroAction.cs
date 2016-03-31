using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;

namespace WOOP
{
    class WDeadMicroAction : WMicroAction
    {
        public override void BeginAction(WUnit unit)
        {
            loga(unit, "Begin on unit in " + unit.getPosition());
            unit.unselectAll();
        }
    }
}
