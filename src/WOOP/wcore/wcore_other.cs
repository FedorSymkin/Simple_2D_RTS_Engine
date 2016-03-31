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

namespace WOOP
{
    public partial class WCore : IWCore
    {
        Queue<int> avgq = new Queue<int>();
        int avgTT = 0;
        int avgTickTimeCount = 10;
        void updateAvgTickTime(long tickTime)
        {
            avgq.Enqueue((int)tickTime);
            avgTT = (int)avgq.Average();
            if (avgq.Count >= avgTickTimeCount) avgq.Dequeue();
        }

        void TryLaunchInterfaceMaker()
        {
            if (getConfig("ENABLE_INTERFACE_MAKER") == "true") InterfaceMaker.execute(sourceCodePath);
        }

        KeyCapturer keyCapturer;
        DateTime _launchTime;
        Dictionary<String, String> config = new Dictionary<String, String>();
        public List<WTimer> timers = new List<WTimer>();
        String LogTag { get { return ""; } }
    }
}