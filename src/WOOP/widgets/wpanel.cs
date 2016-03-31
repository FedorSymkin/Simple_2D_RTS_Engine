using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using WOOP;
using System.Runtime.InteropServices;

namespace WOOP
{
    public interface IWPanel
    {
        void setupForUnits(List<WUnit> units);
        void tick(uint dt);
        List<WGraphicsDebugger> graphicDebuggers { get; }
    }

    public class WPanel : Panel, IWPanel
	{
        public List<WGraphicsDebugger> graphicDebuggers { get; private set; }
        public Bitmap panelBackgruond { get; private set; }
        TableLayoutPanel content = new TableLayoutPanel();

        public WPanel()
        {
            this.BorderStyle = BorderStyle.FixedSingle;
            graphicDebuggers = new List<WGraphicsDebugger>();

            panelBackgruond = new Bitmap(W.core.path + "/textures/background/panel.bmp");
            this.BackgroundImage = panelBackgruond;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            
            content.Parent = this;
            content.Dock = DockStyle.Fill;
            content.BackgroundImage = panelBackgruond;
            content.BackgroundImageLayout = ImageLayout.Stretch;

            this.Height = 400;
        }

		public void tick(uint dt)
		{
            
		}

        void debugRender(IWXNAControl g)
        {
            foreach (WGraphicsDebugger dbg in graphicDebuggers)
            {
                if (dbg.enabled)
                {
                    dbg.debugDraw(this, g, this.DisplayRectangle);
                }
            }
        }

        class UnitsByType : Dictionary<Type, List<WUnit>>{}

        UnitsByType sortUnits(List<WUnit> units)
        {
            UnitsByType res = new UnitsByType();
            foreach (var u in units)
            {
                List<WUnit> L;
                if (!res.TryGetValue(u.GetType(), out L))
                {
                    L = new List<WUnit>();
                    res.Add(u.GetType(), L);
                }
                L.Add(u);
            }

            return res;
        }


        WUnitWidget makeUnitWidget(List<WUnit> units)
        {
            WUnitWidget w = units[0].createPanelGui(units);
            return w;
        }

        public void setupForUnits(List<WUnit> units)
		{
            clear();          
            int a = 0;
            if (units.Count > 0)
            {
                UnitsByType sortedUnits = sortUnits(units);
                foreach (var L in sortedUnits)
                {
                    WUnitWidget w = L.Value[0].createPanelGui(units);
                    w.Parent = content;   
                    w.Dock = DockStyle.Top;
                    w.Height += w.Margin.All;
                    a++;
                }
            }
		}


        public void clear()
        {
            content.Controls.Clear();
        }


        protected override CreateParams CreateParams
        {
            get
            {
                // Activate double buffering at the form level.  All child controls will be double buffered as well.
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;   // WS_EX_COMPOSITED
                return cp;
            }
        }

        String LogTag { get { return "Panel: "; } }
        void logm(String text) { W.core.textLogs.WidgetsLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
    }
}