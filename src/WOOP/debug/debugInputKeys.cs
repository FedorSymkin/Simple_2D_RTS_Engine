using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Windows.Forms;
using System.Drawing;

namespace WOOP
{
    public class DebugInputKeys
    {
        bool keysEnable = false;

        public void init()
        {
            keysEnable = (W.core.getConfig("KEYS_DEBUG") == "true");

            W.core.logm("init debug input keys");
            W.core.registerEventHandlerItem(typeof(WKeyDownEvent), new WGameEventHandler(onUserKeyDown));
         }

       

        void changeTerr(int x, int y)
        {
            if (W.core.world.UnitsInWorld(x, y).Count == 0)
            {
                uint t = W.core.world.getTerrain(x,y);
                if (t == 0) W.core.world.setTerrain(x, y, 1);
                if (t == 1) W.core.world.setTerrain(x, y, 0);
            }
        }

        public void onUserKeyDown(Object sender, WGameEvent ev)
        {
            WKeyDownEvent keyev = ((WKeyDownEvent)ev);

            if (keysEnable)
            {
                W.core.logm("key Debug");

                if (keyev.key == Keys.Space)
                {
                   /* changeTerr(0, 4);
                    changeTerr(1, 4);
                    changeTerr(2, 4);
                    changeTerr(3, 4);
                    changeTerr(4, 4);
                    changeTerr(5, 4);
                    changeTerr(6, 4);

                    changeTerr(4, 0);
                    changeTerr(4, 1);
                    changeTerr(4, 2);
                    changeTerr(4, 3);*/

                    if (!W.core.paused) W.core.pause(); else W.core.unpause();
                    
                }

                if (keyev.key == Keys.Escape)
                {
                    W.core.clearMaxTickTime();
                    W.core.debugWidget.setValue("modules perfomance:", "OK");
                }

                if (keyev.key == Keys.Q)
                {
                    //changeTerr(0, 1);
                    //changeTerr(1, 0);
                    //changeTerr(1, 1);
                    //Console.WriteLine(W.core.players[1].mapIsOpen[0,0]);

                    //W.core.players[0].openAllMap();


                    Console.WriteLine("units of player red = " + W.core.units.getUnitsByPredicate(unit => unit.OwnerPlayer == W.core.players[0]).Count);
                    Console.WriteLine("units of player blue = " + W.core.units.getUnitsByPredicate(unit => unit.OwnerPlayer == W.core.players[1]).Count);
           
                  //  W.core.players[0].openAllMap();
                   //AStarTest test = new AStarTest();
                    //test.run();
                }
            }
        }
    }
}
