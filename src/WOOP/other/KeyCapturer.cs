using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Windows.Forms;

namespace WOOP
{
    public class YourFilter : IMessageFilter
    {
        public event EventHandler KeyPressed = delegate { };
        private const Int32 WM_KEYDOWN = 0x0100;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_KEYDOWN) return false;
            KeyEventArgs args = new KeyEventArgs((Keys)m.WParam);
            KeyPressed(this, args);

            return false;
        }
    }

    public class KeyCapturer
    {
        public KeyCapturer()
        {
            W.core.logm("Init key capturer");

            var filter = new YourFilter();
            Application.AddMessageFilter(filter);
            filter.KeyPressed += onKey;
        }

        void onKey(object sender, EventArgs e)
        {
            KeyEventArgs ev = ((KeyEventArgs)e);
            W.core.logm("Pressed " + ev.KeyCode.ToString());

            W.core.emitGameEvent(this, new WKeyDownEvent { key = ev.KeyCode });

        }
    }
}
