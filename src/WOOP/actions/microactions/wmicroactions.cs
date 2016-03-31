using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

namespace WOOP
{
    public class WAction
    {
        //to override=====================================================
        public virtual void BeginAction(WUnit unit) {}
        public virtual void EndAction(WUnit unit) {}
        public virtual String getDbgPictString(WUnit unit, IWXNAControl g, Rectangle area) { return ""; }
        //================================================================

        public Point? getPointOfAction()
        {
            if (param is Point)
            {
                return (Point)param;
            }
            else if (param is WUnit)
            {
                return ((WUnit)param).getPosition();
            }
            else return null;
        }

        public override String ToString()
        {
            String res = "";

            res = this.GetType().Name;
            res += " param = ";
            if (param != null) res += param.ToString();
            else res += "none";


            return res;
        }


        public object param { set; get; }
        public WCommand parentCommand { get; set; }


        public int nestedLevel()
        {
            int res = 0;

            WCommand c = this.parentCommand;
            while (true)
            {
                if (c == null) break;
                if (!(c is WCommand)) break;
                res++;
                c = c.parentCommand;
            }

            return res;
        }



        protected String LogTag(WUnit unit) 
        {
            if (unit == null) return "action " + this.GetType().Name + ": ";
            else return String.Format("Unit {0} action {1}: ", unit.GetType().Name, this.GetType().Name); 
        }

        protected void logtodo(String text) { W.core.textLogs.TODOLog.log(text); }
        protected void logm(WUnit unit, String text) { W.core.textLogs.MainUnitsLog.log(LogTag(unit) + text); }
        protected void logt(WUnit unit, String text) { W.core.textLogs.TickLog.log(LogTag(unit) + text); }
        protected void loga(WUnit unit, String text) { W.core.textLogs.ActionsLog.log(LogTag(unit) + text); }
        protected void logl(WUnit unit, String text) { W.core.textLogs.AlgLog.log(LogTag(unit) + text); }
        protected void logo(WUnit unit, String text) { W.core.textLogs.OptimizationLog.log(LogTag(unit) + text); }
        protected void logz(WUnit unit, String text) { W.core.textLogs.ZadrotLog.log(LogTag(unit) + text); }
    }

    public class WMicroAction : WAction
    {
        public virtual bool canExecute(WUnit unit) { return true; }
    }
}