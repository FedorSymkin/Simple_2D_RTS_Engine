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

namespace WOOP
{
    public partial class WCore : IWCore
    {
        void main(object o, EventArgs a)
        {
            init();

            while (!this.exitFlag)
            {
                tickProc();
                redrawProc();
                Application.DoEvents();
            }
        }

        Stopwatch tmrTick = Stopwatch.StartNew();
        bool wasTick = false;
        void tickProc()
        {
            if (tmrTick.ElapsedMilliseconds >= tickTime)
            {
                if (tmrTick.ElapsedMilliseconds > tickTime)
                {
                    //Console.WriteLine(String.Format("Warning! Too low tick time! (ideal tickTime = {0} ms, real tick time = {1} ms)", tickTime, tmrTick.ElapsedMilliseconds));
                    logm(String.Format("Warning! Too low tick time! (ideal tickTime = {0} ms, real tick time = {1} ms)", tickTime, tmrTick.ElapsedMilliseconds));
                }
                tmrTick.Restart();

                Stopwatch t = Stopwatch.StartNew();
                W.core.tick(tickTime);
                wasTick = true;
                W.core.emitGameEvent(this, new WTickEvent() { dt = tickTime });
                t.Stop();
                updateAvgTickTime(t.ElapsedMilliseconds);

                if (maxTickTime < t.ElapsedMilliseconds)
                {
                    maxTickTime = (int)t.ElapsedMilliseconds;
                    W.core.debugWidget.setValue("max tick time", maxTickTime.ToString());
                }
            }
        }

        Stopwatch tmrRedraw = Stopwatch.StartNew();
        void redrawProc()
        {
            if (tmrRedraw.ElapsedMilliseconds >= redrawTime)
            {
                if (tmrRedraw.ElapsedMilliseconds > redrawTime)
                {
                    //Console.WriteLine(String.Format("Warning! Too low REDRAW time! (ideal  = {0} ms, real = {1} ms)", redrawTime, tmrRedraw.ElapsedMilliseconds));
                    logm(String.Format("Warning! Too low REDRAW time! (ideal  = {0} ms, real = {1} ms)", redrawTime, tmrRedraw.ElapsedMilliseconds));
                }
                tmrRedraw.Restart();

                W.core.redraw(redrawTime);
            }
        }

        void tick(uint dt)
        {
            if (dt < 500)
            {
                textLogs.TickLog.enabledToConsole = false;
                textLogs.TickLog.enabledToFile = false;
            }
            logt("TOTAL TICK");

            if (!paused)
            {
                units.tick(dt);
                players.tick(dt);
                world.tick(dt);
                timersTick(dt);
                miniMap.tick(dt);
                shells.tick(dt);
                OnTick(dt);

                modulesTick(dt); //must be last!
            }
        }

        void modulesTick(uint dt)
        {
            foreach (var module in modules)
            {
                if (W.core.getCurrentElapsedTick() > tickTimeUseful) 
                {
                    logt("not enough time for mudules!");
                    W.core.debugWidget.setValue("modules perfomance:", "not enough time!");
                    break;
                }

                if (module.started) module.tick(dt);

                if (W.core.getCurrentElapsedTick() > tickTimeUseful)
                {
                    logt("not enough time for mudules!");
                    W.core.debugWidget.setValue("modules perfomance:", "not enough time!");
                    break;
                }

                W.core.debugWidget.setValue("modules perfomance:", "OK");
            }
        }

        void timersTick(uint dt)
        {
            foreach (WTimer t in timers) t.tick(dt);
        }

        void redraw(uint dt)
        {
            gameField.Invalidate();
            miniMap.Invalidate();
        }
    }
}