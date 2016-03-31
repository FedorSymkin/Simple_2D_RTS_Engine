using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;
using System.Windows.Forms;

namespace WOOP
{
    public partial interface IWUnit
    {
        void onRightClickCommand(Point pos);
        void onRightClickCommand(WUnit OtherUnit);

        void select(WPlayer selecter);
        void unselect(WPlayer selecter);
        void unselectAll();
        void select();
        void unselect();

        List<WPlayer> PlayersWhoSelectedMe {get;}
        bool isSelectedBy(WPlayer player);
	
		//Automatically generated:
		Boolean TTTT444(Int32 g56);
		Boolean isSelected();
	}

    public partial class WUnit : IWUnit
    {
        public bool TTTT444(int g56)
        {
            return true;
        }

        public virtual void onRightClickCommand(Point pos)
        {
            //empty
        }

        public virtual void onRightClickCommand(WUnit OtherUnit)
        {
            //empty
        }

        bool? Actions_debug = null;
        WUnitActionDbg actionsGraphicDebugger = null;
        public void select(WPlayer selecter)
        {
            if (!PlayersWhoSelectedMe.Contains(selecter) && (inGame))
            {
                PlayersWhoSelectedMe.Add(selecter);
                selecter.selectedUnits.Add(this);

                if (Actions_debug == null) Actions_debug = (W.core.getConfig("ACTIONS_DEBUG") == "true");
                if (Actions_debug == true)
                {
                    if (selecter == W.core.players.humanPlayer)
                    {
                        if (selecter.selectedUnits.Count == 1)
                        {
                            W.core.textDebugWindow.Show();
                            W.core.textDebugWindow.Left = 1100;
                            W.core.textDebugWindow.unit = this;

                            actionsGraphicDebugger = new WUnitActionDbg();
                            actionsGraphicDebugger.unit = this;
                            W.core.gameField.graphicDebuggers.Add(actionsGraphicDebugger);
                        }
                        else
                        {
                            W.core.textDebugWindow.Hide();
                        }
                    }
                }

                logm("selected");
            }
        }

        public void unselect(WPlayer selecter)
        {
            PlayersWhoSelectedMe.Remove(selecter);
            selecter.selectedUnits.Remove(this);

            if (Actions_debug == null) Actions_debug = (W.core.getConfig("ACTIONS_DEBUG") == "true");
            if (Actions_debug == true)
            {
                if (selecter == W.core.players.humanPlayer)
                {
                    W.core.textDebugWindow.Hide();
                }

                if (actionsGraphicDebugger != null)
                {
                    W.core.gameField.graphicDebuggers.Remove(actionsGraphicDebugger);
                    actionsGraphicDebugger = null;
                }
            }

            logm("unselected");
        }

        public void unselectAll()
        {
            while (PlayersWhoSelectedMe.Count > 0) unselect(PlayersWhoSelectedMe[0]);
        }

        public void select()   { select(W.core.players.humanPlayer); }
        public void unselect() { unselect(W.core.players.humanPlayer); }

        List<WPlayer> _PlayersWhoSelectedMe = new List<WPlayer>();
        public List<WPlayer> PlayersWhoSelectedMe { get { return _PlayersWhoSelectedMe; } }

        public bool isSelectedBy(WPlayer player)
        {
            return _PlayersWhoSelectedMe.Contains(player);
        }

        public bool isSelected()
        {
            return _PlayersWhoSelectedMe.Contains(W.core.players.humanPlayer);
        }

        public virtual WUnitWidget createPanelGui(List<WUnit> units)
        {
            WUnitWidget res = new WUnitWidget();

            WUnitName n = new WUnitName();
            n.Text = unitName();
            res.addWidget(n);

            WUnitPhoto p = new WUnitPhoto();
            p.init(this.GetType());
            res.addWidget(p);


            WUnitLabel prm = new WUnitLabel();
            prm.Text = getParamStr();
            res.addWidget(prm);
            
            return res;
        }

        public virtual String getParamStr()
        {
            String res = "";
            res += String.Format("max HP: {0}\n",this.maxHitPoints);
            res += String.Format("Видит: {0} клеток\n", this.visibleRange);
            return res;
        }


        public virtual String unitName()
        {
            return "Юнит";
        }
    }
}
