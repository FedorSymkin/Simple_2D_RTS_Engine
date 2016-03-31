using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WOOP;

namespace WOOP
{
    public delegate bool UnitPredicate(WUnit unit);

    public interface IWUnits
    {
        WUnit CreateUnit(Type unitType, Point position, WPlayer player);
        void RemoveUnit(WUnit unit);

        void tick(uint dt);
        void render(IWXNAControl g);

        void moveUnits(List<WUnit> units, Point position, WUnit commander, bool isOrder = false, bool attackingMove = false);

        MultiKeyDictionary<Type, WPlayer, TexturesSquare> textures { get; }
        TexturesSquare getTexturesForUnit(Type unitType, WPlayer player);

        List<WUnit> getUnitsByPredicate(UnitPredicate predicate);

		//Automatically generated:
		List<Point> getPointsHeap(Point centralPoint, List<WUnit> units);
		List<Point> getPointsHeap(Point centralPoint, WUnit unit, Int32 count);
	}

    public class WUnits : List<WUnit>, IWUnits
	{
        public MultiKeyDictionary<Type, WPlayer, TexturesSquare> textures { get; private set; }
        public PathCachingSingleton pathCaching = new PathCachingSingleton();

        public WUnits()
        {
            textures = new MultiKeyDictionary<Type, WPlayer, TexturesSquare>();

            W.core.registerEventHandlerItem(typeof(WRubberBandEvent), new WGameEventHandler(onRubberBandEvent));
            W.core.registerEventHandlerItem(typeof(WRightClickEvent), new WGameEventHandler(onRightClick));
        }

        void onRightClick(Object sender, WGameEvent e)
        {
            WRightClickEvent ev = (WRightClickEvent)e;
            loge("Right clicked on " + ev.GamePos);
            List<WUnit> unitsOnCell = W.core.world.UnitsInWorld(ev.GamePos);



            foreach (var unit in W.core.players.humanPlayer.selectedUnits)
            {
                if (unitsOnCell.Count == 0)
                    unit.onRightClickCommand(ev.GamePos);
                else
                    unit.onRightClickCommand(unitsOnCell[0]);
            }
          
        }

        void onRubberBandEvent(Object sender, WGameEvent e)
        {
            WRubberBandEvent ev = (WRubberBandEvent)e;
            logm("rubberBand event rect = " + ev.rect.ToString());

            W.core.players.humanPlayer.unselectAllUnits();
            Rectangle corrRect = ev.rect;
            corrRect.Width += 1;
            corrRect.Height += 1;

            Point rubberCenter = WUtilites.rectCenter(corrRect);
            foreach (WUnit unit in this)
            {
                if (corrRect.Contains(unit.getPosition(rubberCenter)))
                {
                    if (unit.inScreen()) unit.select();
                }
            }

            W.core.panel.setupForUnits(W.core.players.humanPlayer.selectedUnits);
        }

        public TexturesSquare getTexturesForUnit(Type unitType, WPlayer player)
        {
            TexturesSquare sq = null;
            if (!textures.TryGetValue(unitType, player, out sq))
            {
                sq = new TexturesSquare();
                bool isBuilding = (unitType.IsSubclassOf(typeof(WBuilding))) || (unitType == typeof(WBuilding));
                sq.loadFromDir(String.Format("{0}/textures/units/{1}/self", W.core.path, unitType.Name), player.color, !isBuilding);
                textures.add(unitType, player, sq);

                logm(String.Format("textures for unit {0} loaded", unitType.Name));
            }
            else logm(String.Format("textures for unit {0} already exists", unitType.Name));

            return sq;
        }

        public List<WUnit> getUnitsByPredicate(UnitPredicate predicate)
        {
            List<WUnit> res = new List<WUnit>();
            foreach (var unit in this)
            {
                if (predicate(unit)) res.Add(unit);
            }
            return res;
        }

        public List<Point> getPointsHeap(Point centralPoint, List<WUnit> units)
        {
            List<Point> res = new List<Point>();
            if (units.Count == 0) return res;
            int pos = 0;

            Func<Point, bool> setPointFunc =
            (p) =>
            {
                if (units[pos].CanPlacedTo(p))
                {
                    res.Add( p );
                    pos++;    
                }
                return (pos >= units.Count);
            };

            

            for (int r = 0; ((r < W.core.world.Width) || (r < W.core.world.Height)); r++)
            {
                if (r == 0) res.Add(centralPoint);
                else
                {
                    
                    int x;
                    int y;

                    //Top
                    y = centralPoint.Y - r;
                    for (x = centralPoint.X - r; x <= centralPoint.X + r; x++)
                    {
                        if (setPointFunc(new Point (x,y))) return res;
                    }

                    //Bottom
                    y = centralPoint.Y + r;
                    for (x = centralPoint.X - r; x <= centralPoint.X + r; x++)
                    {
                        if (setPointFunc(new Point(x, y))) return res;
                    }

                    //Left
                    x = centralPoint.X - r;
                    for (y = centralPoint.Y - r + 1; y < centralPoint.Y + r; y++)
                    {
                        if (setPointFunc(new Point(x, y))) return res;
                    }

                    //Right
                    x = centralPoint.X + r;
                    for (y = centralPoint.Y - r + 1; y < centralPoint.Y + r; y++)
                    {
                        if (setPointFunc(new Point(x, y))) return res;
                    }
                }
            }

            return res;
        }

        public List<Point> getPointsHeap(Point centralPoint, WUnit unit, int count)
        {
            List<Point> res = new List<Point>();
            int pos = 0;

            Func<Point, bool> setPointFunc =
            (p) =>
            {
                if (unit.CanPlacedTo(p))
                {
                    res.Add(p);
                    pos++;
                }
                return (pos >= count);
            };



            for (int r = 0; ((r < W.core.world.Width) || (r < W.core.world.Height)); r++)
            {
                if (r == 0) res.Add(centralPoint);
                else
                {

                    int x;
                    int y;

                    //Top
                    y = centralPoint.Y - r;
                    for (x = centralPoint.X - r; x <= centralPoint.X + r; x++)
                    {
                        if (setPointFunc(new Point(x, y))) return res;
                    }

                    //Bottom
                    y = centralPoint.Y + r;
                    for (x = centralPoint.X - r; x <= centralPoint.X + r; x++)
                    {
                        if (setPointFunc(new Point(x, y))) return res;
                    }

                    //Left
                    x = centralPoint.X - r;
                    for (y = centralPoint.Y - r + 1; y < centralPoint.Y + r; y++)
                    {
                        if (setPointFunc(new Point(x, y))) return res;
                    }

                    //Right
                    x = centralPoint.X + r;
                    for (y = centralPoint.Y - r + 1; y < centralPoint.Y + r; y++)
                    {
                        if (setPointFunc(new Point(x, y))) return res;
                    }
                }
            }

            return res;
        }

        public void moveUnits(List<WUnit> units, Point position, WUnit commander = null, bool isOrder = false, bool attacking = false)
		{
            //It is deprecated algorythm - moving units to multiple points. Can be uncommented.
                    /*List<WUnit> MovingUnits = new List<WUnit>();
                    foreach (var unit in units)
                    {
                        if (unit is WMovingUnit) MovingUnits.Add(unit);
                    }

                    List<Point> heap = getPointsHeap(position, MovingUnits);

                    for (int i = 0; i < MovingUnits.Count; ++i)
                    {
                        ((WMovingUnit)MovingUnits[i]).Move(heap[i]);
                    }*/

          /*  foreach (var unit in units)
            {
                if (unit is WSimpleAttackingUnit) ((WSimpleAttackingUnit)unit).attackingMove(position);
                else if (unit is WMovingUnit) ((WMovingUnit)unit).Move(position);           
            }*/

            foreach (var unit in units)
            {
                if (!unit.inGame) continue;

                if (attacking)
                {
                    if (unit is WSimpleAttackingUnit) ((WSimpleAttackingUnit)unit).attackingMove(position);
                }
                else
                {
                    if (unit is WMovingUnit) ((WMovingUnit)unit).Move(position);
                }
            }
            
                
                     
		}

		public void tick(uint dt)
		{
            logt("Units TICK");

            for (int i = 0; i < this.Count;)
            {
                if (!this[i].willRemoved)
                {
                    this[i].tick(dt);
                    i++;
                }
                else
                {
                    RemoveUnitNow(this[i]);
                }
            }

            pathCaching.tick((int)dt);
		}

        void RemoveUnitNow(WUnit unit)
        {
            logm("Unit " + unit.GetType().ToString() + " now removed");
            unit.unselectAll();
            unit.ClearWorldFromMe();
            this.Remove(unit);
        }

        public WUnit CreateUnit(Type unitType, Point position, WPlayer player)
        {
            logm(String.Format("Creating unit: {0} for player {1} on {2}...", unitType, player.name, position));

            WUnit unit = (WUnit)Activator.CreateInstance(unitType);
            unit.OwnerPlayer = player;
            unit.init();

            bool ok;
            if (unit is WBuilding) ok = ((WBuilding)unit).CanBuildingPlacedTo(position);
            else ok = unit.CanPlacedTo(position);

            if (ok)
            {
                this.Add(unit);
                unit.setPosition(position);

                logm("Unit created");
                return unit;
            } 
            else
            {
                logm("Error: cannot create unit in this position");

                return null;
            }
        }

		public void RemoveUnit(WUnit unit)
		{
            logm("Unit " + unit.GetType().ToString() + " will removed");
            unit.RemoveLater();
		}

        public void render(IWXNAControl g)
        {
            Point scrPoint = W.core.players.humanPlayer.screenPoint;
            foreach (WUnit unit in this)
            {
                if (unit.inScreen()) unit.render(g, scrPoint);
            }
        }

        String LogTag { get { return "Units: "; } }
        void logm(String text) { W.core.textLogs.MainUnitsLog.log(LogTag + text); }
        void logt(String text) { W.core.textLogs.TickLog.log(LogTag + text); }
        void loge(String text) { W.core.textLogs.UserEventLog.log(LogTag + text); }
        
    }
}