using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Reflection;

namespace WOOP
{
    public interface IWTextLog
    {
        String logName {  get; }
        bool enabledToFile { set; get; }
        bool enabledToConsole { set; get; }

        void log(String text);
	
		//Automatically generated:
		void createTotalFile();
		void Dispose();
	}

    public class WTextLog : IWTextLog, IDisposable
    {
        //PUBLIC===========================================================
        public String logName { private set; get; }
        public bool enabledToFile { set; get; }
        public bool enabledToConsole { set; get; }
        public void log(String text)
        {
            String ts = DateTime.Now.TimeOfDay.ToString().Replace('.', ',');
            if (enabledToFile)
            {
                file.WriteLine(String.Format("({0})\t{1}", ts, text));
                file.Flush();

                if (W.core.getConfig("SAVE_TOTAL_LOG") == "true")
                {
                    totalFile.WriteLine(String.Format("({0})\t{1}:\t\t\t{2}", ts, logName, text));
                    totalFile.Flush();
                }
            }
            if (enabledToConsole)
            {
                System.Console.WriteLine(String.Format("({0})\t{1}:\t\t\t{2}", ts, logName, text));
            }
        }
        //=================================================================


        StreamWriter file;
        static StreamWriter totalFile;
        String GetFileName() {return String.Format("{0}/{1}.log", W.core.textLogsDir, logName);}
        String GetTotalFileName() { return String.Format("{0}/logs/textLogs/!total.log", W.core.path); }
        
        
        void createFile()
        {
            try
            {
                file = new StreamWriter(GetFileName(), false);
                log("log created");
            }
            catch
            {
                W.core.FatalError("Cannot open log file '" + GetFileName() + "'");
            }   
        }

        public void createTotalFile()
        {
            if (totalFile == null)
            {
                try
                {
                    totalFile = new StreamWriter(GetTotalFileName(), false);
                }
                catch
                {
                    W.core.FatalError("Cannot open log file '" + GetTotalFileName() + "'");
                }
            }
        }



        public WTextLog(String name)
        {
            enabledToFile = true;
            enabledToConsole = true;
            logName = name;
            createTotalFile();
            createFile();       
        }

        public void Dispose()
        {
            file.Close();
        }
    }

    public class WTextLogs
    {
        public WTextLog MainWorldLog = new WTextLog("MainWorldLog");
        public WTextLog MainUnitsLog = new WTextLog("MainUnitsLog");    
        public WTextLog TexturesLog = new WTextLog("TexturesLog");
        public WTextLog MainPlayersLog = new WTextLog("MainPlayersLog");
        public WTextLog GameFieldLog = new WTextLog("GameFieldLog");
        public WTextLog CoreLog = new WTextLog("CoreLog");
        public WTextLog TODOLog = new WTextLog("TODOLog");
        public WTextLog GameUnitsLog = new WTextLog("GameUnitsLog");
        public WTextLog WidgetsLog = new WTextLog("WidgetsLog");
        public WTextLog TickLog = new WTextLog("TickLog");
        public WTextLog ActionsLog = new WTextLog("ActionsLog");
        public WTextLog UserEventLog = new WTextLog("UserEventLog");
        public WTextLog AlgLog = new WTextLog("AlgLog");
        public WTextLog OptimizationLog = new WTextLog("OptimizationLog");
        public WTextLog ZadrotLog = new WTextLog("ZadrotLog");
        public WTextLog AILog = new WTextLog("AILog");
        

        //=====================================================================================
        public void applyConfig()
        {
            if (logsList == null) defineLogsList();

            //Default logs state
            if (W.core.getConfig("LOGS_BY_DEFAULT") == "dis")
            {
                foreach (KeyValuePair<String, WTextLog> pair in logsList)
                {
                    pair.Value.enabledToConsole = false;
                    pair.Value.enabledToFile = false;  
                }
                System.Console.WriteLine("Logs by default disabled");
            }


            //If disable/enable some logs
            foreach (KeyValuePair<String, WTextLog> pair in logsList)
            {
                String configKey = "EnableLog_" + pair.Key;
                if (W.core.getConfig(configKey) == "true")
                {
                    pair.Value.enabledToConsole = true;
                    pair.Value.enabledToFile = true;

                    Console.WriteLine("log " + pair.Key + " enabled");
                }
                else
                {
                    pair.Value.enabledToConsole = false;
                    pair.Value.enabledToFile = false;

                    Console.WriteLine("log " + pair.Key + " disabled");
                }
            }
        }


        Dictionary<String, WTextLog> logsList = null;
        void defineLogsList()
        {
            logsList = new Dictionary<String, WTextLog>();
            MemberInfo[] members = this.GetType().GetMembers();

            foreach (MemberInfo m in members)
            {
                if ((m.MemberType == MemberTypes.Field) && (m.ToString().Contains("WOOP.WTextLog ")))
                {
                    WTextLog log = (WTextLog)this.GetType().GetField(m.Name).GetValue(this);
                    logsList.Add(log.logName, log);
                }
            }
        }

        public WTextLog logByName(String name)
        {
            if (logsList == null) defineLogsList();

            WTextLog res = null;
            logsList.TryGetValue(name, out res);
            return res;
        }
    }
}
