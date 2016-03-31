using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public class WGraphicsDebugger
    {
        virtual public void debugDraw(Object sender, IWXNAControl g, Rectangle area) { }
        virtual public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area) { }

        public bool enabled;
        public String name { get { return this.GetType().Name; } }

        public WGraphicsDebugger()
        {
            enabled = true;

            String configKey = "Disable_" + name;
            
            if ((W.core.getConfig(configKey) == "true"))
            {
                enabled = false;
                W.core.textLogs.CoreLog.log("IWXNAControl debugger " + name + " disabled");
            }
        }
    }
   
    //===========================================================================================
    public class RectDebugger : WGraphicsDebugger
    {
        override public void debugDraw(Object sender, IWXNAControl g, Rectangle area) 
        {
            Rectangle r = new Rectangle(area.X, area.Y, area.Width, area.Height);
            g.DrawRectangle(new Pen(Color.Red, 3), r); 
        }
    }
}

