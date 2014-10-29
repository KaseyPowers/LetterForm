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

    public class mLetterSections
    {
        public List<mPoint> points;

        public mLetterSections(List<mPoint> points)
        {
            this.points = points.Distinct().ToList();
            /*int min = points[0].line;
            for (int i = 0; i < points.Count; i++)
            {
                points[i].line -= min;
            }*/
        }

        public String getString( bool length)
        {
            String output = "";
            double firstLength = Math.Round(distance(points[0], points[ 1]), 1);
            if (firstLength != 0)
            {

                for (int i = 0; i < points.Count - 1; i++)
                {
                    if (points[i].line == points[i + 1].line)
                    {
                        double thisLength = Math.Round(distance(points[i], points[i + 1]), 1);
                        if (i == 0)
                        {
                            firstLength = thisLength;
                        }

                        int direction = getDirection(points[i], points[i + 1]);
                        switch (direction)
                        {
                            case 4:
                                output += "A";
                                break;
                            case 5:
                                output += "B";
                                break;
                            case 6:
                                output += "C";
                                break;
                            case 7:
                                output += "D";
                                break;

                        }
                        if (length)
                        {
                            double value = (thisLength / firstLength);
                            value = RoundToNearest(value, .2);
                            /*if (value <= 2.5)
                            {
                                value = RoundToNearest(value, .5);
                            }
                            else
                            {
                                value = RoundToNearest(value, 1);
                            }*/
                            String temp = value.ToString("F2").PadLeft(5, '0');
                            output += temp;
                        }
                    }
                    else
                    {
                        if (length)
                        {
                            output += "Line00";
                        }
                        else
                        {
                            output += "L";
                        }
                    }
                }
            }
            return output;
        }
        public static Double RoundToNearest(Double passednumber, Double roundto)
        {
            // 105.5 up to nearest 1 = 106
            // 105.5 up to nearest 10 = 110
            // 105.5 up to nearest 7 = 112
            // 105.5 up to nearest 100 = 200
            // 105.5 up to nearest 0.2 = 105.6
            // 105.5 up to nearest 0.3 = 105.6

            //if no rounto then just pass original number back
            if (roundto == 0)
            {
                return passednumber;
            }
            else
            {
                double up = Math.Ceiling(passednumber / roundto) * roundto;
                double down = Math.Floor(passednumber / roundto) * roundto;
                if (Math.Abs(up - passednumber) >= Math.Abs(down - passednumber))
                {
                    return down;
                }
                else
                {
                    return up;
                }
            }
        }
        private int getDirection(mPoint startPoint, mPoint endPoint)
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
            int direction = -1;
            double deltaX = xChange(startPoint, endPoint);
            double deltaY = yChange(startPoint, endPoint);

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

            return direction;
        }

        private double xChange(mPoint a, mPoint b)
        {

            return (a.X - b.X);
        }

        private double yChange(mPoint a, mPoint b)
        {
            return (a.Y - b.Y);
        }
        private double distance(mPoint a, mPoint b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }
    }

    public class mSectionNode
    {
        public mSectionNode parent;
        public List<mSectionNode> children;
        public String SectionLetter;
        public double SectoinValue;
        public String chars;
        public String ifStopHere;

        public mSectionNode(String letter, double value, String chars)
        {
            parent = null;
            children = new List<mSectionNode>();
            SectionLetter = letter;
            SectoinValue = value;
            this.chars = chars;
        }

        public void addChild(mSectionNode child)
        {
            child.parent = this;
            children.Add(child);
        }

        public void addChar(char newChar)
        {            
            chars += newChar;
            chars = new string(chars.ToList().Distinct().ToArray());
        }

        public void addFinalChar(char newChar)
        {
            ifStopHere += newChar;
            ifStopHere = new string(ifStopHere.ToList().Distinct().ToArray());
        }
    }

    /*public class mSectionNode2
    {
        public mSectionNode parent;
        public List<mSectionNode2> children;
        public List<double> rangeValues;
        public String SectionLetter;
        public double SectoinValue;
        public String chars;
        public String ifStopHere;

        public mSectionNode2(String letter, double value, String chars)
        {
            parent = null;
            children = new List<mSectionNode>();
            rangeValues = new List<double>();
            SectionLetter = letter;
            SectoinValue = value;
            this.chars = chars;
        }

        public void addChild(mSectionNode2 child)
        {
            child.parent = this;
            children.Add(child);
        }

        public void addChar(char newChar)
        {
            chars += newChar;
            chars = new string(chars.ToList().Distinct().ToArray());
        }

        public void addFinalChar(char newChar)
        {
            ifStopHere += newChar;
            ifStopHere = new string(ifStopHere.ToList().Distinct().ToArray());
        }
    }
    */
}
