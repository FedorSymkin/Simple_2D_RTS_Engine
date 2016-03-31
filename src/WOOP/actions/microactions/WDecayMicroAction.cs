using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;

namespace WOOP
{
    class WDecayMicroAction : WMicroAction
    {
        public override void EndAction(WUnit unit)
        {
            loga(unit, "End on unit in " + unit.getPosition());
            unit.RemoveLater();
        }
    }
}
