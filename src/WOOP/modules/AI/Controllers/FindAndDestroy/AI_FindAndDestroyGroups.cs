using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows;
using System.IO;
using WOOP;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Diagnostics;
using IMaker;
using System.Threading;

namespace WOOP
{
    class FindAndDestroyGroupsController : WAIController
    {
        int groupsCount = 3;
        int state = 0;
        bool waitLates = false;

        public FindAndDestroyGroupsController(WPlayer plr, bool topLevel, int groupsCount, bool waitLates)
            : base(plr, topLevel) 
        {
            this.groupsCount = groupsCount;
            this.waitLates = waitLates;
        }

        
        protected override void onStart()
        {
            if (topLevel)
            {
                units = W.core.units.getUnitsByPredicate(
                    u => ((u.OwnerPlayer == player) && (u is WMovingUnit))
                );
            } //if no topLevel - units list is defined by parent controller


        }

        DefineGroupsAlg defineGroups;
        protected override void onTick(uint dt)
        {
            switch (state)
            {
                case 0: // init
                    state = 0x10;
                    break;

                case 0x10: //define groups
                    defineGroups = new DefineGroupsAlg(units, groupsCount);
                    //defineGroups.applyDebug(1);
                    defineGroups.startExecution();
                    state = 0x11;
                    break;

                case 0x11: //wait for define groups execution
                    if (!defineGroups.finished) defineGroups.continueExecution();
                    if (defineGroups.finished) state = 0x12;
                    break;

                case 0x12: //create children algs  
                    foreach (var group in defineGroups.getResult())
                    {
                        FindAndDestroyAllController ctrl = new FindAndDestroyAllController(player, false, waitLates);
                        ctrl.units = group;
                        //ctrl.applyDebug();
                        subControllers.Add(ctrl);
                    }
                    //BlockingDebug.block("groups defined", 9, false);
                    state = 0x13;
                    break;

                case 0x13:
                    break;
            }    
        }
    }
}
