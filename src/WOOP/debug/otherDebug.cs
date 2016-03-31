using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;


namespace WOOP
{
    public partial class WCore
    {
        public TextDebugWindow textDebugWindow = new TextDebugWindow();

     
        
        void nowDebug()
        {

        }

        void DebugMouseMove(Object sedner, WGameEvent e)
        {
            WMouseMoveEvent ev = (WMouseMoveEvent)e;

            if (sedner == W.core.gameField)
            {
                bool ok = false;
                Point gamepos = W.core.gameField.ScreenToWorld(ev.pos, ref ok);

                W.core.debugWidget.setValue("pos", gamepos.ToString());
            }
            else if (sedner == W.core.miniMap)
            {
                Point gamepos = new Point();
                gamepos.X = ev.pos.X / (int)W.core.miniMap.pixelPerPoint;
                gamepos.Y = ev.pos.Y / (int)W.core.miniMap.pixelPerPoint;

                W.core.debugWidget.setValue("pos", gamepos.ToString());
            }
        }
    }

    public class TextDebugWindow : Form
    {
        public RichTextBox textBox;
        public WUnit unit;

        public TextDebugWindow()
        {
            textBox = new RichTextBox();
            textBox.Parent = this;
            this.Width = 400;
            this.Height = 700;
            textBox.Show();
            textBox.Width = this.Width;
            textBox.Height = this.Height;

            textBox.Text = "Text1\nText2";

        }

        
    }
}
