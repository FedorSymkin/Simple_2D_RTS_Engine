using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WOOP;
using System.Drawing;

namespace WOOP
{
    public partial interface IWUnit
    {
        void init();
        void tick(uint dt);
        void RemoveLater();
        bool willRemoved { get; }

        //int X { set; get; }
        //int Y { set; get; }
        bool CanPlacedTo(Point pos);
        bool CanPlacedTo(int X, int Y);
        bool CanPlacedTogetherWith(WUnit unit);
        bool CanPlacedToTerrain(uint terrainCode);

        WPlayer OwnerPlayer { set; get; }
    
		//Automatically generated:
		void SetMeToWorldCell(Point pos);
		void ClearWorldCellFromMe(Point pos);
		void SetMeToWorldCell(Int32 x, Int32 y);
		void ClearWorldCellFromMe(Int32 x, Int32 y);
		String ToString();
		Boolean inGame {get;}
	}

    public partial class WUnit
	{
		public WPlayer OwnerPlayer { set; get; }

        public virtual void init()
        {
            textureSquare = W.core.units.getTexturesForUnit(this.GetType(), this.OwnerPlayer);

            MsPerMADict.Add(typeof(WDecayMicroAction), 5000);
            userInit();

            visibleRange = 5;

            maxHitPoints = 100;
            hitPoints = maxHitPoints;
            AddDefaultCommand();
            initCurrentCmd();

            logm("unit inited");
        }

        public void tick(uint dt)
        {
            logt(String.Format("Unit {0} TICK", this.GetType().ToString()));
            tickActions(dt);
        }


        public bool willRemoved { get; private set; }
        public bool inGame { get { return ((!willRemoved) && (!isDead)); } }
        public void RemoveLater() {willRemoved = true;}

        bool assertPointBounds(int X, int Y) {return assertPointBounds(new Point(X, Y));}
        bool assertPointBounds(Point p)
        {
            if (W.core.world.pointInWorld(p)) return true;
            else
            {
                logm("Error: trying to access out-of-bounds point");
                return false;
            }
        }

        
        //public int X { set { setPosition(new Point(value, _position.Y); } get { return _position.X; } }
        //public int Y { set { position = new Point(_position.X, value); } get { return _position.Y; } }
        Point _position = new Point(-1, -1);
        
        public virtual void setPosition(Point value)
        {
            if (this.CanPlacedTo(value))
            {
                ClearWorldCellFromMe(_position);
                _position = value;
                SetMeToWorldCell(value);
                See();
                loga("Position changed to " + value);
            }
            else
            {
                logm("Error: can't placed to " + value);
            }
        }

        public virtual Point getPosition(Point? neareastTo = null)
        {
            return _position;
        }

        public Point pos
        {
            get { return getPosition(); }
            set { setPosition(value); }
        }


        public bool CanPlacedTogetherWith(WUnit unit)
        {
            return false;
        }


        public override String ToString()
        {
            return this.GetType().Name + " at " + this.getPosition().ToString();
        }


        public bool CanPlacedTo(Point pos) { return CanPlacedTo(pos.X, pos.Y); }
        public bool CanPlacedTo(int X, int Y) 
        {
            if (!assertPointBounds(X, Y)) return false;
            if (!CanPlacedToTerrain( W.core.world.getTerrain(X,Y) )) return false;

            List<WUnit> unitsOnCell = W.core.world.UnitsInWorld(X, Y);
            foreach (WUnit unit in unitsOnCell)
            {
                if (this != unit)
                if (!this.CanPlacedTogetherWith(unit)) return false;
            }
 
            return true;
        }



        public void SetMeToWorldCell(Point pos) { SetMeToWorldCell(pos.X, pos.Y); }
        public void SetMeToWorldCell(int x, int y)
        {
            List<WUnit> NewCell = W.core.world.UnitsInWorld(x,y);
            if (!NewCell.Contains(this)) NewCell.Add(this);
            W.core.miniMap.changedPoints.Add(new Point(x,y));
        }

        public virtual void ClearWorldFromMe()
        {
            ClearWorldCellFromMe(pos);
        }

        public void ClearWorldCellFromMe(Point pos) { ClearWorldCellFromMe(pos.X, pos.Y); }
        public void ClearWorldCellFromMe(int x, int y)
        {
            if ((x >= 0) && (y >= 0))
            {
                List<WUnit> OldCell = W.core.world.UnitsInWorld(x, y);
                if (OldCell.Contains(this))
                {
                    OldCell.Remove(this);
                    W.core.miniMap.changedPoints.Add(new Point(x, y));
                }
                else logm("Warning: while change position old cell not contains this unit");
            }
        }

      

        public bool CanPlacedToTerrain(uint terrainCode)
        {
            return (W.core.world.terrainType(terrainCode) == "allowed");
        }
    }
}
