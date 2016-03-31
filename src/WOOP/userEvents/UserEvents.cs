using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace WOOP
{
    public class WGameEvent { }
    public delegate void WGameEventHandler(Object sedner, WGameEvent e);

    public partial interface IWCore
    {
        void emitGameEvent(Object sender, WGameEvent e);
        WGameEventHandler gameEventHandler(Type eventType);
        void registerEventHandlerItem(Type eventType, WGameEventHandler method);
    }

    
    public partial class WCore
    {
        Dictionary<Type, WGameEventHandler> GameEventHandlers = new Dictionary<Type, WGameEventHandler>();
        
        public WGameEventHandler gameEventHandler(Type eventType)
        {
            WGameEventHandler res;
            if (GameEventHandlers.TryGetValue(eventType, out res)) return res;
            else
            {
                GameEventHandlers.Add(eventType, null);
                return GameEventHandlers[eventType];
            }
        }

        public void emitGameEvent(Object sender, WGameEvent e)
        {
            WGameEventHandler hdl = gameEventHandler(e.GetType());
            if (hdl != null) hdl.Invoke(sender, e);
        }

        public void registerEventHandlerItem(Type eventType, WGameEventHandler method)
        {
            WGameEventHandler h = W.core.gameEventHandler(eventType);
            h += method;
            GameEventHandlers[eventType] = h;
        }
    }

    //Game events==================================================================================
    public class WMouseDownEvent : WGameEvent
    {
        public Point pos { get { return new Point(x, y); } }
        public int x;
        public int y;
        public MouseButtons button;
    }

    public class WKeyDownEvent : WGameEvent
    {
        public Keys key;
    }


    public class WMouseUpEvent : WGameEvent
    {
        public Point pos { get { return new Point(x, y); } }
        public int x;
        public int y;
    }

    public class WMouseMoveEvent : WGameEvent
    {
        public Point pos { get { return new Point(x, y); } }
        public int x;
        public int y;
    }

    public class WRubberBandEvent : WGameEvent
    {
        public Rectangle rect;
    }

    public class WRightClickEvent : WGameEvent
    {
        public Point GamePos { get { return new Point(GameX, GameY); } set { GameX = value.X; GameY = value.Y; } }
        public int GameX;
        public int GameY;
    }

    public class WTickEvent : WGameEvent
    {
        public uint dt;
    }
}
