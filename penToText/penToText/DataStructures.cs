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
        mPoint startPoint;
        mPoint endPoint;
        double length;
    }
}
