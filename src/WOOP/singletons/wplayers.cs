using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using WOOP;

namespace WOOP
{
    public interface IWPlayers
    {
        WPlayer humanPlayer { set; get; }
        WPlayer watchPlayer { set; get; }
        WPlayer createPlayer(Color color);
        void tick(uint dt);
	}

    public class WPlayers : List<WPlayer>, IWPlayers
	{
        public WPlayers()
        {
            createPlayer(Color.Maroon);
        }

		public WPlayer createPlayer(Color color)
		{
            WPlayer player = new WPlayer();
            this.Add(player);
            player.color = color;
            player.name = "Player" + Convert.ToString(this.Count);

            if (humanPlayer == null)
            {
                humanPlayer = player;
                watchPlayer = player;
            }

            logm("Player created: " + player.name);
            return player;
		}



		public WPlayer humanPlayer { set; get; }
        public WPlayer watchPlayer { set; get; }
        public void tick(uint dt)
        {
            logt("Players TICK");
        }


        String LogTag { get { return "Players: "; } }
        void logm(String text) { W.core.textLogs.MainPlayersLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
    }

    //=============================================
    //==============================================================================================
    //=============================================
    public interface IWPlayer
    {
        List<WUnit> selectedUnits { get;  }
        WUnit selectedCommander { get; set; }
        void unselectAllUnits();
        void openAllMap();

        Color color { set; get; }
        Point screenPoint { set; get; }
        String name { set; get; }
	}

    public class WPlayer : IWPlayer
	{
        public Color color { set; get; }
        public String name { set; get; }
        public WUnit selectedCommander { get; set; }
        public List<WUnit> selectedUnits { get; private set; }

        public void unselectAllUnits()
        {
            while (selectedUnits.Count > 0) selectedUnits[0].unselect(this);
        }

        public bool openedPoint(int x, int y)
        {
            if (W.core.world.pointInWorld(x, y))
            {
                return mapIsOpen[x, y];
            }
            else return true;
        }

        public void openAllMap()
        {
            for (int x = 0; x < W.core.world.Width; x++)
            for (int y = 0; y < W.core.world.Height; y++)
                mapIsOpen[x,y] = true;
        }

        public WPlayer()
        {
            selectedUnits = new List<WUnit>();
            color = Color.Red;

            updateMapIsOpenArray();
        }

        public void updateMapIsOpenArray()
        {
            if (W.core.world != null) 
            mapIsOpen = new bool[W.core.world.Width, W.core.world.Height];
        }

        public Point centerOFScreen
        {
            get
            {
                Size scrSize = W.core.gameField.CellsInField;

                return new Point(
                        screenPoint.X + scrSize.Width/2,
                        screenPoint.Y + scrSize.Height/2
                    );
            }
        }



        Point _screenPoint;
        public Point screenPoint
        {
            get { return _screenPoint; }
            set
            {
                bool warn = false;
                Size scrSize = W.core.gameField.CellsInField;
                //define points
                Point point1 = value;
                Point point2 = new Point(value.X + scrSize.Width, value.Y + scrSize.Height);

                //check right border
                if (point2.X > W.core.world.Width) { point2.X = W.core.world.Width; warn = true; }
                if (point2.Y > W.core.world.Height) { point2.Y = W.core.world.Height; warn = true; }

                //define new left point
                point1.X = point2.X - scrSize.Width;
                point1.Y = point2.Y - scrSize.Height;

                //check left border
                if (point1.X < 0) { point1.X = 0; warn = true; }
                if (point1.Y < 0) {point1.Y = 0; warn = true;}

                //check for errors
                if ((point2.X <= point1.X) || (point2.Y <= point1.Y))
                {
                    logm("Error setting screen point: invalid coordinates");
                    return;
                }

                //setting
                _screenPoint = point1;
                if (warn) logm("Warning: trying to set screen to out-of-border points. Corrected.");
            }
        }

        public bool[,] mapIsOpen { get; set; }

        String LogTag { get { return "Player " + name + ": "; } }
        void logm(String text) { W.core.textLogs.MainPlayersLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }


    }
}



