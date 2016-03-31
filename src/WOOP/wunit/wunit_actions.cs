using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;

namespace WOOP
{
    public partial interface IWUnit
    {
        WMicroAction currentMA { get;  }
        WCommand currentCmd { get;  }
        void AddCommand(Type commandType, object Param, bool force = false);
        void tickActions(uint dt);

        uint MsPerMA(Type MAType);
        void SetMsPerMA(Type MAType, uint ms);
        

		//Automatically generated:
		String getMAInfo();
	}
    public partial class WUnit : IWUnit
    {
        //May be overriden================================
        protected virtual uint DefaultMsPerMA() { return 450; }



        //Implementation==================================
        public WMicroAction currentMA { get; private set; }
        public WCommand currentCmd { get; private set; }
        List<WCommand> commands = new List<WCommand>();

        public int commandsCount()          {return commands.Count + 1;}
        public WCommand getCommand(int i)   
        {
            if (i == 0) return currentCmd;
            else return commands[i - 1];
        }

        public void AddCommand(Type commandType, object Param, bool force = false)
        {
            //Clean old commands
            if (force)
            {
                if (commands.Count > 1) commands.RemoveRange(0, commands.Count - 1); //remove all but first (current)
                if (currentCmd != null) currentCmd.interrupt();
            }

            //Adding a command
            WCommand cmd = (WCommand)Activator.CreateInstance(commandType);
            cmd.param = Param;
            cmd.parentCommand = null;
            commands.Add(cmd);

            logm(String.Format("Added command: {0}, param = {1}, force = {2}", commandType, Param, force));
        }

        uint MATmr = 0;
        public void tickActions(uint dt)
        {
            if (currentMA == null) initCurrentMA();

            uint t = MsPerMA(currentMA.GetType());
            logt(String.Format("MsPerMA = {0}",t));

            MATmr += dt;
            if (MATmr >= t)
            {
                NextMA();
            } 
            else 
            { 
                //Check for force interrupt of micro action
                bool needInterrupt = false;
                currentCmd.tick(this, (int)dt, out needInterrupt);

                if ((needInterrupt) && (!(currentMA is WMoveMicroAction)))
                {
                    loga("MA interrupted");
                    NextMA();
                }
            }
        }

        void initCurrentMA()
        {
            logm("initCurrentMA");
            currentMA = defineNextMA();
            MATmr = 0;
            currentMA.BeginAction(this);
        }


        void NextMA()
        {
            logt("NextMA");
            currentMA.EndAction(this);
            currentMA = defineNextMA();
            MATmr = 0;
            currentMA.BeginAction(this);

            if (Actions_debug == true)
            {
                if (W.core.textDebugWindow.unit == this)
                {
                    W.core.textDebugWindow.textBox.Text = getMAInfo();
                }
            }
        }

        WMicroAction defineNextMA()
        {
            WMicroAction res;
            res = currentCmd.nextMA(this);

            while (res == null)
            {
                NextCmd();
                res = currentCmd.nextMA(this);
            }
            return res;
        }

        void NextCmd()
        {
            logt("NextCmd");
            currentCmd = defineNextCmd();
        }

        void initCurrentCmd()
        {
            logm("initCurrentCmd");
            NextCmd();
        }

        WCommand defineNextCmd()
        {
            WCommand res = null;

            if (commands.Count > 0) res = dequeueCommand();
            else
            {
                AddDefaultCommand();
                if (commands.Count > 0) res = dequeueCommand();
                else logm("AddDefaultCommand not added a command!");
            }

            if (res == null) logm("strange error: defineNextCommand returns null");
            return res;
        }

        WCommand dequeueCommand()
        {
            WCommand res = commands[0];
            commands.RemoveAt(0);

            return res;
        }


        Dictionary<Type, uint> MsPerMADict = new Dictionary<Type, uint>();
        public uint MsPerMA(Type MAType)
        {
            uint res;

            if (!MsPerMADict.TryGetValue(MAType, out res))
            {
                res = DefaultMsPerMA();
            }

            if (res <= 0) 
            {
                logm(String.Format("Error: speed of MicroAction {0} <=0. Using minimum.", MAType));
                res = 1;
            }

            return res;
        }



        public void SetMsPerMA(Type MAType, uint ms)
        {
            MsPerMADict[MAType] = ms;
        }

        public String getMAInfo()
        {
            String res = "";

            res += "Current command: \n";
            res += this.currentCmd.ToString() + "\n\n";

            foreach (var c in this.commands)
            {
                res += c.ToString() + "\n\n";      
            }

            return res;
        }
    }
}
