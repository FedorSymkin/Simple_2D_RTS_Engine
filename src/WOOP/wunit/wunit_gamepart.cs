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
        int hitPoints { set; get; }
        int maxHitPoints { set; get; }
        uint visibleRange { set; get; }
        void Damage(WUnit damager, int dmg);
        void Die(bool force = true);
        bool isDead { get; }
        void Stop(bool force = true);
	
		//Automatically generated:
		Boolean isEnemy(WUnit otherUnit);
	}

    public partial class WUnit : IWUnit
    {
        //May be overriden================================
        protected virtual void userInit() {}
        protected virtual void AddDefaultCommand() {Stop();}
        public virtual void Damage(WUnit damager, int dmg)
        {
            hitPoints -= dmg;
        }
 
        //Implementation==================================
        uint _visibleRange;
        public uint visibleRange 
        { 
            get
            {
                return _visibleRange;
            }

            set
            {
                _visibleRange = value;
                //See();
            }
        }

        public int maxHitPoints { set; get; }
        public bool isDead { private set; get; }

        int _hitPoints;
        public int hitPoints 
        {
            get { return _hitPoints; }
            set
            {
                if (value <= maxHitPoints)
                {
                    _hitPoints = value;
                    if ((int)value <= 0) Die();
                }
                else
                {
                    logm("Warning: trying to set hit points more than maximum");
                    _hitPoints = maxHitPoints;
                }
            }  
        }
 

        public void Die(bool force = true)
        {
            logg("Die");
            isDead = true;
            AddCommand(typeof(WDeadCommand), null, force);
        }

        public void Stop(bool force = true)
        {
            logg("Stop");
            AddCommand(typeof(WStopCommand), 0, force);
        }

        public bool isEnemy(WUnit otherUnit)
        {
            if (otherUnit == null) return false;
            if (otherUnit == this) return false;
            if (!otherUnit.inGame) return false;

            return otherUnit.OwnerPlayer != this.OwnerPlayer;   
        }

        public void See()
        {
            for (int dx = -(int)visibleRange; dx <= visibleRange; dx++)
            {
                for (int dy = -(int)visibleRange; dy <= visibleRange; dy++)
                {
                    int x = getPosition().X + dx;
                    int y = getPosition().Y + dy;
                    if (W.core.world.pointInWorld(x, y))
                    {
                        if (this.OwnerPlayer.mapIsOpen[x, y] == false)
                        {
                            this.OwnerPlayer.mapIsOpen[x, y] = true;
                            W.core.miniMap.changedPoints.Add(new Point(x, y));
                        }
                    }
                }
            }
        }
    }
}
