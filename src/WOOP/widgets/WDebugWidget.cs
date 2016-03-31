using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Windows.Forms;
using System.Drawing;

namespace WOOP
{
    public interface IWDebugWidget
    {
        Label addLabel(String name);
        void setValue(String name, String value);
    }

    public class WDebugWidget : TableLayoutPanel, IWDebugWidget
    {
        public WDebugWidget()
        {
            addLabel("avg tick time");
            addLabel("max tick time");
            //addLabel("total cmd count");
            addLabel("blocking debug");
            addLabel("pos");
            //addLabel("max path caches");
            //addLabel("max time of move run");
            //addLabel("run command fails (timeout)");
            //addLabel("main dps");
            //addLabel("cell size");
            addLabel("modules perfomance:");

            setValue("blocking debug", "false");
            setValue("modules perfomance:", "OK");


            WTimer tickAvgt = new WTimer(onAvgt, 1000); tickAvgt.start();
            WTimer cmdCnt = new WTimer(onCmdCnt, 1000); cmdCnt.start();
        }

        void onCmdCnt(WTimer sender)
        {
           // IEnumerable<GCHandle> listOfObjectsInHeap = GetListOfObjectsFromHeap();

        }

        void onAvgt(WTimer sender)
        {
            setValue("avg tick time", W.core.averageTickTime.ToString());          
            setValue("run command fails (timeout)", WCommand.runTimeFails.ToString());
            setValue("main dps", W.core.gameField.dps.ToString());
            setValue("cell size", W.core.gameField.cellSize.ToString());
        }

        Dictionary<String, Label> labels = new Dictionary<string, Label>();
        public Label addLabel(String name)
        {
            Label l = new Label();
            this.Controls.Add(l, 0, labels.Count);
            labels.Add(name,l);
            l.Text = name + ": ";
            l.Font = new Font("Arial", 14);

            l.AutoSize = true;
            this.AutoSize = true;

            return l;
        }

        public void setValue(String name, String value)
        {
            Label l = null;
            if (labels.TryGetValue(name, out l))
            {
                l.Text = name + ": " + value;
            }
        }
    }
}
