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
    class WTestModule : WModule
    {
        int i = 0;
        int c = 0;

        public override void tick(uint dt)
        {
            if (++i >= 10)
            {
                i = 0;
                c++;
                System.Console.WriteLine("value = " + c);
            }
        }
    }
}
