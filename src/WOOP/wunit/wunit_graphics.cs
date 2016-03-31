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
        void render(IWXNAControl g, Point screenPoint);
        bool inScreen();
	
		//Automatically generated:
		Point shiftDirection {get;set;}
	}

    public partial class WUnit : IWUnit
    {
        public virtual Rectangle getDrawRect(Point screenPoint)
        {
            Point shift = getRenderShift();
            Size cellSize = W.core.gameField.cellSize;

            return new Rectangle(
                (getPosition().X - screenPoint.X) * cellSize.Width + shift.X,
                (getPosition().Y - screenPoint.Y) * cellSize.Height + shift.Y,
                cellSize.Width,
                cellSize.Height
                );
        }

        public void render(IWXNAControl g, Point screenPoint)
        {
            if (willRemoved) return;

            Rectangle drawRect = getDrawRect(screenPoint);

            if (currentMA != null) renderSelf(g, drawRect);
            if (!isDead) renderHP(g, drawRect);
            if (isSelectedBy(W.core.players.humanPlayer)) renderSelectionRect(g, drawRect);
        }

        void renderHP(IWXNAControl g, Rectangle drawRect)
        {
            int x = drawRect.X + 2;
            int y = drawRect.Y + 2;
            int w = drawRect.Width - 5;
            int h = 3;
            g.DrawRectangle(new Pen(Color.Black), x, y, w, h);

            Int64 percent = hitPoints;
            percent *= 100;
            percent /= maxHitPoints;

            Color c;
            if (percent >= 70) c = Color.Green;
            else if (percent >= 30) c = Color.Yellow;
            else c = Color.Red;

            Int64 pw = w - 1;
            pw *= hitPoints;
            pw /= maxHitPoints;

            g.FillRectangle(new SolidBrush(c), x + 1, y + 1, (int)pw, h - 1);
            g.FillRectangle(new SolidBrush(Color.Gray), x + 1 + (int)pw, y + 1, w - 1 - (int)pw, h - 1);
        }

        public Point rotateDirection { set; get; }
        public Point _shiftDirection = new Point(0,0);
        public Point shiftDirection
        {
            get
            {
                return _shiftDirection;
            }

            set
            {
                if ((Math.Abs(value.X) > 1) || (Math.Abs(value.Y) > 1))
                    logm("Error: trying to set incorect move direction");
                else 
                    _shiftDirection = value;
            }
        }

        Point getRenderShift()
        {
            Point res = new Point(0,0);

            if ((shiftDirection.X != 0) || (shiftDirection.Y != 0))
            {
                uint maxt = MsPerMA(currentMA.GetType());
                if (shiftDirection.X != 0) res.X = (int)(shiftDirection.X * (W.core.gameField.cellSize.Width  * MATmr) / maxt);
                if (shiftDirection.Y != 0) res.Y = (int)(shiftDirection.Y * (W.core.gameField.cellSize.Height * MATmr) / maxt);   
            }

            return res;
        }

        void renderSelf(IWXNAControl g, Rectangle drawRect)
        {
            g.DrawImage(this.getCurrentPicture(), drawRect);      
        }

        void renderSelectionRect(IWXNAControl g, Rectangle drawRect)
        {
            g.DrawRectangle(new Pen(Color.Red, 2), drawRect);
        }


        int getRotateCode()
        {
            switch (rotateDirection.X)
            {
                case -1:
                    switch (rotateDirection.Y)
                    {
                        case -1: return 7;
                        case 0: return 6;
                        case 1: return 5;
                    }
                    break;

                case 0:
                    switch (rotateDirection.Y)
                    {
                        case -1: return 0;
                        case 1: return 4;
                    }
                    break;

                case 1:
                    switch (rotateDirection.Y)
                    {
                        case -1: return 1;
                        case 0: return 2;
                        case 1: return 3;
                    }
                    break;
            }

            return 0;
        }

        TexturesSquare textureSquare;
        uint oldfr = 99999;
        IWTexture getCurrentPicture()
        {
            TexturesLine currentTexturesLine;

            if (textureSquare.TryGetValue(currentMA.GetType().Name, out currentTexturesLine))
            {
                int r = getRotateCode();
                List<IWTexture> simpleLine = currentTexturesLine[r];

                uint maxt = MsPerMA(currentMA.GetType());
                uint fr = (MATmr * (uint)simpleLine.Count) / maxt;
                if (fr != oldfr)
                {
                    //loga("New frame");
                    oldfr = fr;
                }

                
                int fint = (int)fr;
                if (fint >= simpleLine.Count) fint = simpleLine.Count - 1;


                return simpleLine[fint];
            }
            else
            {
                logm("Error: cannot load texture for microAction " + currentMA.GetType().ToString());
                return WUtilites.NoTextureStub();
            }
        }
     
        public virtual bool inScreen()
        {
            return W.core.viewRect.Contains(pos) && (W.core.players.watchPlayer.mapIsOpen[pos.X, pos.Y]);
        }
    }
}
