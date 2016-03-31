using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.IO;
using WOOP;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;
using IMaker;
using System.Threading;

namespace WOOP
{
    class WAI : WModule
    {
        public override void tick(uint dt)
        {
            for (int i = 0; i < W.core.AIControllers.Count; )
            {
                var ctrl = W.core.AIControllers[i];

                if (!ctrl.started) ctrl.start();
                ctrl.tick(dt);

                if (ctrl.finished) W.core.AIControllers.RemoveAt(i);
                else i++;
            }
        }
    }
}
