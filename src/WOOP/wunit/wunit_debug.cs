using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

public class WUnitActionDbg : WGraphicsDebugger
{
    public WUnit unit;

    override public void debugDraw(Object sender, IWXNAControl g, Rectangle area)
    {

    }

    override public void debugDrawCell(Object sender, IWXNAControl g, Point cellCoord, Rectangle area)
    {
        String pict = "";

        for (int i = 0; i < unit.commandsCount(); ++i)
        {
            processingAct(ref pict, unit.getCommand(i), g, cellCoord, area);
        }

        if (pict != "")
        {
            g.DrawEllipse(new Pen(Color.Black, 2), WUtilites.AreaDiv(area, 0.1f));
            g.DrawString(pict, new Font("Arial", 8), new SolidBrush(Color.Black), area);
        }
    }

    void processingAct(ref String pict, WAction act, IWXNAControl g, Point cellCoord, Rectangle area)
    {
        Point? p = act.getPointOfAction();

        if (p != null)
        {
            if (p == cellCoord)
            {
                pict += act.getDbgPictString(unit, g, area);
            }
        }

        if (act is WCommand)
        {
            WCommand cmd = (WCommand)act;
            for (int i = 0; i < cmd.actionsCount(); i++)
            {
                processingAct(ref pict, cmd.getAction(i), g, cellCoord, area);
            }
        }
    }
}