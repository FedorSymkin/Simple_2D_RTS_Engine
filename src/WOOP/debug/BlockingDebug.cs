using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Windows.Forms;
using System.Diagnostics;

namespace WOOP
{
    public static class BlockingDebug
    {
        static int blockTout = 0;
        public static void init()
        {
            W.core.registerEventHandlerItem(typeof(WKeyDownEvent), new WGameEventHandler(onUserKeyDown));
            blockTout = Convert.ToInt32(W.core.getConfig("BLOCK_TOUT"));     
        }

        static int currentImportance = 0;
        static bool keyPressed = false;

        static void onUserKeyDown(Object sender, WGameEvent ev)
        {
            WKeyDownEvent keyev = ((WKeyDownEvent)ev);

            if ((keyev.key >= Keys.D0) && (keyev.key <= Keys.D9))
            {
                int imp = keyev.key - Keys.D0;
                currentImportance = imp;
                keyPressed = true;
            }
        }



        static Stopwatch timer;
        public static void block(String message, int importance = 0, bool tout = true)
        {
            if (currentImportance > importance) return;
            W.core.debugWidget.setValue("blocking debug", message + " (importance level = " + importance + ", press keys 0-9)");

            keyPressed = false;
            if (blockTout != 0 && tout) timer = Stopwatch.StartNew();
            while (!keyPressed)
            {
                Application.DoEvents();
                W.core.mainWidget.render();

                if (blockTout != 0 && tout)
                    if (timer.ElapsedMilliseconds >= blockTout)
                    {
                        timer.Stop();
                        break;
                    }
            }
            W.core.debugWidget.setValue("blocking debug", "false");
        }
    }
}
