using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;

namespace WOOP
{
    class WStopMicroAction : WMicroAction
    {
        public override void BeginAction(WUnit unit)
        {
            loga(unit, "Begin on unit in " + unit.getPosition());
        }

        public override void EndAction(WUnit unit)
        {
            loga(unit, "End on unit in " + unit.getPosition());
        }

        public override bool canExecute(WUnit unit)
        { 
            return true; 
        }       
    }
}
