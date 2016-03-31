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
    public abstract class WAIController
    {
        //public
        public WPlayer player { get; private set; }
        public List<WUnit> units = new List<WUnit>();
        public List<WAIController> subControllers = new List<WAIController>();
        public bool topLevel { get; private set; } //sub Controller is not top level
        public bool started { get; private set; }  //starts automatically in AI module
        public bool finished { get; private set; } //if finished -> it will be deleted in AI module

        public void start() { started = true; logai("started"); onStart(); }
        public void finish() { finished = true; logai("finished"); onFinish(); }

        public WAIController(WPlayer player, bool topLevel) 
        {
            this.player = player;
            this.topLevel = topLevel;
            started = false;
            finished = false;
        }
        
        public void tick(uint dt)
        {
            onTick(dt);
            foreach (var c in subControllers) c.tick(dt);
        }


        //to inherit
        protected virtual void onStart() { }
        protected virtual void onFinish() { }
        protected abstract void onTick(uint dt);


        //private
        protected void logai(String str) 
        { 
            W.core.textLogs.AILog.log(String.Format("{0} for player {1}: {2}", this.GetType().Name, player.name, str)); 
        }
    }
}
