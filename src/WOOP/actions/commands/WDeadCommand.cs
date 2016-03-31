using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;

namespace WOOP
{
    class WDeadCommand : WCommand
    {
        override protected void run(WUnit unit)
        {
            addMA(typeof(WDeadMicroAction), null);
            addMA(typeof(WDecayMicroAction), null);
        }
    }
}
