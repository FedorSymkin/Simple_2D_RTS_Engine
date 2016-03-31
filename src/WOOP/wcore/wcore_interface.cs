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
    public delegate void OnInitHandler();
    public delegate void OnTickHandler(uint dt);

    public partial interface IWCore
    {
        //SYSTEM=========================================
        uint tickTime { get; }
        uint redrawTime { get; }     
        String path { set; get; }
        String sourceCodePath { set; get; }
        int averageTickTime { get; }
        bool exitFlag { get; }
        Boolean paused { get; }
        Control parentControl { get; }
        OnInitHandler OnInit { get; set; }
        OnTickHandler OnTick { get; set; }
        
        void FatalError(String text);
        void exit();
        void pause();
        void unpause();


        //SINGLETONS=========================================
        WMainWidget mainWidget { get; }
        WUnits units { get;  }
        WPlayers players { get;  }
        WWorld world { get;  }
        WDebugWidget debugWidget { get; }

        //GAMEPART=========================================
        Point screenPoint { set; get; }
        Rectangle viewRect { get; }
        List<WAIController> AIControllers { get; }
        void addAI(WAIController controller);


        //OTHER========================================= 
        String textLogsDir { get; }
        WTextLogs textLogs { get; }
        

        String launchTime { get; }
        String getConfig(String key);
        void logm(String text);
        void logt(String text);
        DebugInputKeys debugInputKeys { get; }
        Int32 getCurrentElapsedTick();
        int maxTickTime { get; }
        void clearMaxTickTime();
	}

    public partial class WCore : IWCore
    {
        //SINGLETONS=========================================
        public WMainWidget mainWidget { get; private set; }
        public WUnits units { get; private set; }
        public WPlayers players { get; private set; }
        public WWorld world { get; private set; }
        public WDebugWidget debugWidget { get; private set;}
        public WShells shells { get; private set; }


        //SOME POINTERS FOR convenience======================
        public WGameField gameField;
        public WMiniMap miniMap;
        public WPanel panel;


        //SYSTEM===============================================
        public uint tickTime { get; private set; }
        public uint tickTimeUseful { get { return (tickTime * 90) / 100; } }
        public uint redrawTime { get; private set; }
        public String path { set; get; }
        public String sourceCodePath { set; get; }
        public int averageTickTime { get { return avgTT; } }
        public bool exitFlag { get; private set; }
        public bool paused { get; private set; }
        public void pause() { paused = true; }
        public void unpause() { paused = false; }
        public Control parentControl { get; private set; }
        public OnInitHandler OnInit { get; set; }
        public OnTickHandler OnTick { get; set; }
        public List<WModule> modules { get; set; }


        public void FatalError(String text)
        {
            MessageBox.Show("Fatal error: " + text + ". Application will terminate");
            exit();
        }

        public void exit()
        {
            exitFlag = true;
            if (mainWidget != null) mainWidget.Dispose();
        }

        public WCore(Control parent, String path, String sourcesPath, uint tickIntervalMs, uint redrawIntervalMs)
        {
            this.parentControl = parent;
            this.path = path;
            this.sourceCodePath = sourcesPath;
            this.tickTime = tickIntervalMs;
            this.redrawTime = redrawIntervalMs;

            Application.Idle += new EventHandler(main);
        }


        //GAMEPART===============================================
        public Point screenPoint
        {
            get { return W.core.players.humanPlayer.screenPoint; }
            set { W.core.players.humanPlayer.screenPoint = value; }
        }

        public Rectangle viewRect
        {
            get
            {
                Rectangle res = new Rectangle(screenPoint, W.core.gameField.CellsInField);
                W.core.world.correctUnboundsRect(ref res);
                return res;
            }
        }

        public List<WAIController> AIControllers { get; private set; }

        public void addAI(WAIController controller)
        {
            AIControllers.Add(controller);
        }



        //OTHER===============================================   
        public DebugInputKeys debugInputKeys { get; private set; }
        public int maxTickTime { get; private set; }
        public void clearMaxTickTime() { maxTickTime = 0; } 
        public int getCurrentElapsedTick() { return (int)tmrTick.ElapsedMilliseconds; }
        public String launchTime {get {return _launchTime.ToString();}}
        public WTextLogs textLogs { private set; get; }
        public void logm(String text) { W.core.textLogs.CoreLog.log(LogTag + text); }
        public void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
        public String textLogsDir
        {
            get
            {
                if ((getConfig("CREATE_NEW_LOGDIR") == "true"))
                    return String.Format("{0}/logs/{1}/textLogs", path, _launchTime.ToString().Replace(':', '-'));
                else
                    return String.Format("{0}/logs/textLogs", path);
            }
        }

        public String getConfig(String key)
        {
            String v = "";
            config.TryGetValue(key, out v);
            return v;
        }

        
        public void registerModule(WModule module, bool started = true)
        {
            modules.Add(module);
            if (started) module.start();
        }
    }

    public static class W
    {
        static public WCore core;
    }
}