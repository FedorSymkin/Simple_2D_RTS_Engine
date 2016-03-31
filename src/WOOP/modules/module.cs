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
    public interface IWModule
    {
    }

    public abstract class WModule : IWModule
    {
        public bool started { set; get; }
        public void start() {started = true;}
        public void stop() {started = false;}
        public abstract void tick(uint dt);
    }
}
