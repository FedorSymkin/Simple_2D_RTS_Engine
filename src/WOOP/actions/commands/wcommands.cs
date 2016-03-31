using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

namespace WOOP
{
    public interface IWCommand
    {
        WAction getAction(int i);

        WMicroAction nextMA(WUnit unit);    
        void tick(WUnit unit, int dt, out bool needInterruptMA);
        void interrupt(); //If called, command will return null MA as soon as possible. If possible it will interrupt current MA
        bool interrupted { get; }
	
		//Automatically generated:
		void BeginAction(WUnit unit);
		void EndAction(WUnit unit);
		Int32 actionsCount();
		void EndActionInterrupted(WUnit unit);
		String ToString();
		WCommand currentCommand {get;}
	}

    public class WCommand : WAction, IWCommand
	{
        //This callbacks are must override=======================================================
        virtual protected void run(WUnit unit) { }  //called at start/rerun
        virtual protected void MAFailed(WUnit unit, WMicroAction action) { } //called if can't execute MA

        //This callbacks are may be overridden (defined behavior by default)==================
        virtual protected void MASuccess(WUnit unit, WMicroAction action) { } //called if can execute MA
        override public void BeginAction(WUnit unit) { } //called once at begin command
        override public void EndAction(WUnit unit) { }   //called once at end command
        virtual public void EndActionInterrupted(WUnit unit) { } //called once at end command via interrupt
        virtual public void tick(WUnit unit, int dt, out bool needInterruptMA) { needInterruptMA = false; }  //called every tick. If needInterruptMA = true, nextMA() will called immediately
        virtual protected void onNextMA(WUnit unit) { } //called every nextMA. May used for checking command
        virtual protected void runStub(WUnit unit) { addMA(typeof(WStopMicroAction), null); addMA(typeof(WRerunMicroAction), null); }
         
        //===================================================================
        public void interrupt() { interrupted = true; logm(null, "interrupted command"); }
        public bool interrupted { get; private set; }
        protected List<WAction> actions = new List<WAction>();
        public WCommand currentCommand { get; private set; }
        
        public WCommand()
        {
            interrupted = false;
            currentCommand = this;
        }

        public override String ToString()
        {
            String res = base.ToString();

            String newStr = "\n";
            int n = nestedLevel();
            for (int i = 0; i < n; i++) newStr = newStr + "\t";

            if (this.actions.Count > 0)
            {
                res += newStr + "Subactions:" + newStr;
                foreach (var c in this.actions)
                {
                    res += "\t" + c.ToString() + newStr;
                }
            }
            else res += newStr + "Subactions: none" + newStr;

            String sep = newStr + "-----------------------" + newStr;
            res = sep + res + sep;
            return res;
        }
        
        public WAction getAction(int i)
        {
            logtodo("WCommand: Check for correct ");
            return actions[i];        
        }

        public int actionsCount() { return actions.Count; }

        protected WCommand addSubcommand(Type CommandType, object param, int position = -1)
        {
            WCommand res = (WCommand)Activator.CreateInstance(CommandType);
            if (actions.Count == 0) currentCommand = res;
            res.param = param;
            res.parentCommand = this;
            if (position == -1) actions.Add(res); else actions.Insert(0, res);
            return res;
        }

        protected WMicroAction addMA(Type MAType, object param, int position = -1)
        {
            if (actions.Count == 0) currentCommand = this;

            WMicroAction res = (WMicroAction)Activator.CreateInstance(MAType);
            res.param = param;
            res.parentCommand = this;
            if (position == -1) actions.Add(res); else actions.Insert(0, res);
            return res;
        }


        bool? UseTimeLimits = null;
        int maxTickLimit;
        private bool blockedByTime = false;
        protected bool wasBlockedByTime() { return blockedByTime; }
        bool timeAllowsRun(WUnit unit)
        {
            if (UseTimeLimits == null)
            {
                UseTimeLimits = W.core.getConfig("USE_TIME_LIMITS") == "true";
                maxTickLimit = Convert.ToInt32(W.core.getConfig("MAX_TICK_LIMIT"));
            }

            if (UseTimeLimits == true)
            {
                bool res = (W.core.getCurrentElapsedTick() <= maxTickLimit);
                blockedByTime = !res;
                return res;
            }
            else 
            {
                blockedByTime = false;
                return true;
            }
        }

        public static int runTimeFails = 0;
        void tryRun(WUnit unit)
        {
            if (timeAllowsRun(unit))
            {
                run(unit);
            }
            else
            {
                runTimeFails++;
                runStub(unit);
            }
        }

        protected bool wasRunning = false;
        public WMicroAction nextMA(WUnit unit)
        {
            logt(unit,"Command next MA");

            if (interrupted)
            {
                EndActionInterrupted(unit);
                return null;
            }

            if (!this.wasRunning)
            {
                this.BeginAction(unit);
                tryRun(unit);
                wasRunning = true;
            }


            if (actions.Count > 0)
            {
                if (actions[0] is WMicroAction) currentCommand = this; else currentCommand = (WCommand)actions[0];
            }
            onNextMA(unit);

            if (actions.Count == 0)
            {
                this.EndAction(unit);
                return null;
            }
            else 
            {
                WAction act = actions[0];
                if (act is WMicroAction)
                {
                    currentCommand = this;
                    WMicroAction mact = (WMicroAction)act;
                    if (mact.canExecute(unit))
                    {
                        this.MASuccess(unit, mact);
                        actions.RemoveAt(0);

                        if (mact is WRerunMicroAction)
                        {
                            logl(unit, "Rerun command");
                            actions.Clear();
                            tryRun(unit);
                            return nextMA(unit);  //danger! May be recursively infinity
                        }
                        return (WMicroAction)act;
                    }
                    else
                    {
                        this.MAFailed(unit, mact);
                        return nextMA(unit); //danger! May be recursively infinity
                    }
                }
                else
                {
                    WCommand cmd = (WCommand)actions[0];
                    currentCommand = cmd;
                    WMicroAction res = cmd.nextMA(unit);
                    if (res != null) return res;
                    else
                    {
                        actions.RemoveAt(0);
                        return nextMA(unit);
                    }
                }
            }
        }
    } 
}