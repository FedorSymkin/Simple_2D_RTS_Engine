using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WOOP;
using System.Diagnostics;
using IMaker;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public void makeunit(Type unitType, int x, int y, int player)
        {
            W.core.units.CreateUnit(unitType, new Point(x, y), W.core.players[player]);
        }

        void WInit()
        {
            /*
             * Test scenario. 
             * It created simple near-attak and far-attack units (like footmans and archers)
             * for player 0(red) and player 1(blue) using red and blue points from map-file "/maps/testMap_units.bmp".
             * After that red player units will automatically investigate the map, attacking all met blue enemies
             * You can also control red units by mouse (using standard warcraft style: left button for select, right button for command)
             */


            W.core.players.createPlayer(Color.Blue);

            bool tmp1 = false;
            bool tmp2 = false;

            Bitmap pict = new Bitmap(W.core.path + "/maps/testMap_units.bmp");
            for (int x = 0; x < pict.Width; x++)
                for (int y = 0; y < pict.Height; y++)
                {
                    if ((pict.GetPixel(x, y).R > 127) && (pict.GetPixel(x, y).B < 127))
                    {
                        Type unitType = tmp1 ? typeof(WSimpleAttackingUnit) : typeof(WRangeAttackingUnit);
                        tmp1 = !tmp1;

                        makeunit(unitType, x, y, 0);
                        W.core.units.Last().select();  
                    }

                    if ((pict.GetPixel(x, y).B > 127) && (pict.GetPixel(x, y).R < 127))
                    {
                        Type unitType = tmp2 ? typeof(WSimpleAttackingUnit) : typeof(WRangeAttackingUnit);
                        tmp2 = !tmp2;

                        makeunit(unitType, x, y, 1);
                    }
                }
            Console.WriteLine("units of player red = " + W.core.units.getUnitsByPredicate(unit => unit.OwnerPlayer == W.core.players[0]).Count);
            Console.WriteLine("units of player blue = " + W.core.units.getUnitsByPredicate(unit => unit.OwnerPlayer == W.core.players[1]).Count);
          
            W.core.addAI(new FindAndDestroyGroupsController(W.core.players[0], true, 3, false));
        }

        void WTick(uint dt)
        {
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.panel1.Hide();

            Left = 0;
            Top = 0;
            Width = 1100;
            Height = 1000;

            W.core = new WCore(this,
                            this.res_input.Text,
                            this.src_input.Text,
                            25,
                            25);

            W.core.OnInit += new OnInitHandler(WInit);
            W.core.OnTick += new OnTickHandler(WTick);
        }
    }
}
