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
        void init()
        {
            SystemInit(parentControl, path, sourceCodePath, tickTime);
            TryLaunchInterfaceMaker();
            WidgetsInit(parentControl, path);
            SingletonsInit(parentControl, path);         
            OtherInit();
            GamePartInit();
            world.loadFromFile(String.Format("{0}/maps/testMap.bmp", W.core.path));
            modulesInit();

            logm("WCore init success");
            nowDebug();

            OnInit();
        }

        void SystemInit(Control parent, String path, String sourcesPath, uint tickIntervalMs)
        {
            exitFlag = false;
            paused = false;
            loadConfig();
            _launchTime = DateTime.Now;

            System.Console.WriteLine("textLogsDir = " + textLogsDir + "\n");
            Directory.CreateDirectory(textLogsDir);
            textLogs = new WTextLogs();
            textLogs.applyConfig();

            keyCapturer = new KeyCapturer();

            debugInputKeys = new DebugInputKeys();
            debugInputKeys.init();
            modules = new List<WModule>();

            BlockingDebug.init();

            logm("System init success");
            logm("launchTime = " + launchTime);
        }

        void WidgetsInit(Control parent, String path)
        {
            mainWidget = new WMainWidget();
            mainWidget.Parent = parent;
            mainWidget.Top = 0;
            mainWidget.Left = 0;
            mainWidget.Width = parent.Width;
            mainWidget.Height = parent.Height;
            mainWidget.Show();



            gameField = mainWidget.gameField;
            miniMap = mainWidget.minimap;
            panel = mainWidget.panel;
            debugWidget = mainWidget.debugWidget;

            logm("Widgets init success");
        }

        void GamePartInit()
        {
            AIControllers = new List<WAIController>(); logm("AIControllers inited");
        }

        void SingletonsInit(Control parent, String path)
        {
            units = new WUnits(); logm("Units inited");
            players = new WPlayers(); logm("Players inited");
            world = new WWorld(); logm("World inited");
            shells = new WShells(); logm("shells inited");            
            logm("Singletons init success");
        }

        void loadConfig()
        {
            StreamReader streamReader = new StreamReader(String.Format("{0}/config.cpp", W.core.path));
            while (!streamReader.EndOfStream)
            {
                String s = streamReader.ReadLine();
                String key = Regex.Split(s, "=").First().Trim();
                String value = Regex.Split(s, "=").Last().Trim();
                if (!config.ContainsKey(key))
                {
                    if (key.IndexOf("//") != 0) config.Add(key, value);
                }
            }
            streamReader.Close();
        }

        private void OtherInit()
        {
            if (getConfig("SHOW_DEBUG_COORD") == "true")
            {
                W.core.registerEventHandlerItem(typeof(WMouseMoveEvent), new WGameEventHandler(DebugMouseMove));
            }
        }

        private void modulesInit()
        {
            registerModule(new WAI());
        }
    }
}