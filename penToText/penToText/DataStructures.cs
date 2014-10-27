using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace penToText
{    
    public class mPoint
    {
        public double X;
        public double Y;
        public int line;

        public mPoint(double x, double y, int line)
        {
            X = x;
            Y = y;
            this.line = line;
        }

        public mPoint(Point input, int line){
            X = input.X;
            Y = input.Y;
            this.line = line;
        }

        public Point getPoint()
        {
            return new Point(X, Y);
        }

    }

    public class mLetterPortion
    {
        public mPoint startPoint;
        public mPoint endPoint;
        public double length;
        public int direction;

        public mLetterPortion(mPoint start){
            startPoint = start;
            endPoint = null;
            length = 0;
        }

        public mLetterPortion(mPoint start, mPoint end)
        {
            startPoint = start;
            endPoint = end;
            length = distance(startPoint, endPoint);
            setDirection();
        }

        public void setEnd(mPoint end){
            endPoint = end;
            length = distance(startPoint, endPoint);
            setDirection();
        }

        private double distance(mPoint a, mPoint b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        private void setDirection()
        {
            /*
             * 0: up
             * 1: down
             * 2: left
             * 3: right
             * 4: up-left
             * 5: up-right
             * 6: down-left
             * 7: down-right
             */
            direction = -1;
            double threshold = .01;
            double deltaX = xChange(startPoint, endPoint);
            double deltaY = yChange(startPoint, endPoint);
            bool vertical = (Math.Abs(deltaX) <= threshold);
            bool horizontal = (Math.Abs(deltaY) <= threshold);
            if (vertical != horizontal)
            {
                if (vertical)
                {
                    if (deltaY < 0)
                    {
                        //up
                        direction = 0;
                    }
                    else
                    {
                        //down
                        direction = 1;
                    }
                }
                else
                {
                    if (deltaX > 0)
                    {
                        //right
                        direction = 3;
                    }
                    else
                    {
                        //left
                        direction = 2;
                    }
                }
            }
            else
            {
                //has a slope
                if (deltaY < 0)
                {
                    //up
                    if (deltaX > 0)
                    {
                        //right
                        direction = 5;
                    }
                    else
                    {
                        //left
                        direction = 4;
                    }
                }
                else
                {
                    //down
                    if (deltaX > 0)
                    {
                        //right
                        direction = 7;
                    }
                    else
                    {
                        //left
                        direction = 6;
                    }
                }
            }
        }

        private double xChange(mPoint a, mPoint b)
        {

            return (a.X - b.X);
        }

        private double yChange(mPoint a, mPoint b)
        {
            return (a.Y - b.Y);
        }
    }
}
