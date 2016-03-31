using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;

namespace WOOP
{
    public partial interface IWUnit
    {
    }

    public partial class WUnit : IWUnit
    {
        String LogTag { get { return String.Format("Unit {0}: ", this.GetType().ToString()); } }
        protected void logm(String text) { W.core.textLogs.MainUnitsLog.log(LogTag + text); }
        protected void logg(String text) { W.core.textLogs.GameUnitsLog.log(LogTag + text); }
        protected void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
        protected void loga(String text) { W.core.textLogs.ActionsLog.log(LogTag + text); }
    }
}
