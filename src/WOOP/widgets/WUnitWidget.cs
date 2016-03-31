using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using WOOP;

namespace WOOP
{
    public interface IWUnitWidget
    {
    }

    public class WUnitWidget : Panel, IWUnitWidget
    {
        public void addWidget(Control w, int pos = -1)
        {
            w.Parent = content;
            content.Controls.Add(w, 0, (pos == -1) ? content.Controls.Count : pos);
            Height += w.Height + this.Margin.All*2;
        }

        TableLayoutPanel content = new TableLayoutPanel();
        public WUnitWidget()
        {
            content.Parent = this;
            content.Padding = new Padding(5);
            this.Dock = DockStyle.Fill;
            content.Dock = DockStyle.Fill;

            this.Margin = new Padding(5);
            Height = 0;

            this.BackgroundImage = W.core.panel.panelBackgruond;
            this.BackgroundImageLayout = ImageLayout.Stretch;
            content.BackgroundImage = W.core.panel.panelBackgruond;
            content.BackgroundImageLayout = ImageLayout.Stretch;
        }
    }

    public class WUnitPhoto : Panel
    {
        static Dictionary<Type, Bitmap> cache = new Dictionary<Type,Bitmap>();
        
        public WUnitPhoto()
        {
            this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.Width = 75;
            this.Height = 75;
            this.Margin = new Padding(5);
        }

        public void init(Type unitType)
        {
            Bitmap b;
            if (!cache.TryGetValue(unitType, out b))
            {
                b = new Bitmap(String.Format("{0}/textures/units/{1}/photo/photo.bmp", W.core.path, unitType.Name));
            }
            this.BackgroundImage = b;
            this.BackgroundImageLayout = ImageLayout.Stretch;
        }
    }

    public class WUnitLabel : Label
    {
        public WUnitLabel()
        {
            this.BackColor = System.Drawing.Color.Transparent;
            this.Margin = new Padding(5);
            this.Font = new Font("Arial", 10);
            this.AutoSize = true;
        }
    }

    public class WUnitName : WUnitLabel
    {
        public WUnitName()
        {
            this.Font = new Font("Arial", 16);
        }
    }
}