using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WOOP;


namespace WOOP
{
    public interface IWMainWidget
    {
        WGameField gameField { get;  }
        WMiniMap minimap { get;}
        WPanel panel { get;  }
        WDebugWidget debugWidget { get; }

        void render();

		//Automatically generated:
		void layouting();
	}

    public class WMainWidget : Panel, IWMainWidget
    {
        public WGameField gameField { get; private set; }
        public WMiniMap minimap { get; private set; }
        public WPanel panel { get; private set; }
        public WDebugWidget debugWidget { get; private set; }

        TableLayoutPanel container;
        TableLayoutPanel PanelMap;


        public WMainWidget()
        {
            this.BorderStyle = BorderStyle.FixedSingle;

            gameField = new WGameField();
            minimap = new WMiniMap();
            panel = new WPanel();
            debugWidget = new WDebugWidget();

            logm("mainWidget children created");

            layouting();

            RectDebugger rdbg = new RectDebugger();
            gameField.graphicDebuggers.Add(rdbg);
            minimap.graphicDebuggers.Add(rdbg);
            panel.graphicDebuggers.Add(rdbg);
         }

        public void layouting()
        {
            container = new TableLayoutPanel();
            PanelMap = new TableLayoutPanel();
            container.Parent = this;

            PanelMap.Controls.Add(panel, 0, 0);
            PanelMap.Controls.Add(minimap, 0, 1);

            container.Controls.Add(debugWidget, 0, 0);
            container.Controls.Add(gameField, 0, 1);
            container.Controls.Add(PanelMap, 1, 1);

            Panel p = new Panel(); container.Controls.Add(p, 0, 2);
            gameField.MinimumSize = new Size(800,800);
            
            
            //Какие-то русские символы
            WUtilites.setAnchors(this);
            WUtilites.setAnchors(container);
            WUtilites.setAnchors(PanelMap);
            WUtilites.setAnchors(gameField);
            WUtilites.setAnchors(minimap);
            WUtilites.setAnchors(panel);

            panel.MinimumSize = new Size(panel.Width, 500);

            logm("mainWidget layouting done");
        }


        public void render()
        {
            gameField.Invalidate();
            minimap.Invalidate();

            //gameField.render();
            //minimap.render();
            //panel.render();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            W.core.exit();
            base.OnHandleDestroyed(e);
        }



        String LogTag { get { return ""; } }
        void logm(String text) { W.core.textLogs.WidgetsLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
    }




}