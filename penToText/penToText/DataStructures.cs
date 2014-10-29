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

        public String getString( bool length, double roundTo)
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
                            case 7:
                                output += "A";
                                break;
                            case 5:
                                output += "B";
                                break;
                            case 4:
                                output += "C";
                                break;
                            case 6:
                                output += "D";
                                break;

                        }
                        if (length)
                        {

                            double value = (thisLength / firstLength);
                            value = RoundToNearest(value, roundTo);
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

    public class mSectionNode2
    {
        public mSectionNode2 parent;
        public List<mSectionNode2> children;
        public String SectionLetter;
        public double minValue;
        public double maxValue;
        public String chars;
        public char ifStopHere;

        public mSectionNode2(String letter, double value, char newChar , bool final)
        {
            parent = null;
            children = new List<mSectionNode2>();
            SectionLetter = letter;
            minValue = value;
            maxValue = value;
            chars = "";
            ifStopHere = ' ';
            if (final)
            {
                addFinalChar(newChar);               
            }
            else
            {
                //addChar(newChar);  
            }
        }

        public bool canComabine(mSectionNode2 other)
        {
            bool main = (this.parent == other.parent) && (this.SectionLetter.Equals(other.SectionLetter));
            bool rangesMatch = ((this.minValue >= other.minValue && this.maxValue <= other.maxValue) 
                ||  (this.minValue <= other.minValue && this.maxValue >= other.maxValue));
            //bool matchingEndPoint = ifStopHere.Equals(other.ifStopHere) && ifStopHere.Length>0;
            bool aCharContainB = true;
            bool bCharContainA = true;
            for (int i = 0; i < other.chars.Length && aCharContainB; i++)
            {
                aCharContainB = chars.Contains(other.chars[i]);
            }
            for (int i = 0; i < chars.Length && bCharContainA; i++)
            {
                bCharContainA = other.chars.Contains(chars[i]);
            }
            bool validChars = aCharContainB || bCharContainA;
            bool validRoots = true;
            if (ifStopHere != ' ' && other.ifStopHere != ' ')
            {
                validRoots = ifStopHere == other.ifStopHere;
            }
            return main && (rangesMatch ||validChars) && validRoots;
        }


        public int bestCombine(mSectionNode2 a, mSectionNode2 b)
        {
            int output = 0;

            for (int i = 0; i < chars.Length; i++){
                if (a.chars.Contains(chars[i]))
                {
                    output++;
                }
                if (b.chars.Contains(chars[i]))
                {
                    output--;
                }
            }

            if (a.ifStopHere == ifStopHere)
            {
                output++;
            }
            else if(b.ifStopHere == ifStopHere)
            {

            }

            return output;
        }

        public mSectionNode2(mSectionNode2 a, mSectionNode2 b)
        {
            this.parent = a.parent;
            this.SectionLetter = a.SectionLetter;
            if (a.minValue < b.minValue)
            {
                this.minValue = a.minValue;
            }
            else
            {
                this.minValue = b.minValue;
            }

            if (a.maxValue > b.maxValue)
            {
                this.maxValue = a.maxValue;
            }
            else
            {
                this.maxValue = b.maxValue;
            }

            chars = "";
            for (int i = 0; i < a.chars.Length; i++)
            {
                addChar(a.chars[i]);
            }
            for (int i = 0; i < b.chars.Length; i++)
            {
                addChar(b.chars[i]);
            }

            ifStopHere = ' ';
            addFinalChar(a.ifStopHere);
            addFinalChar(b.ifStopHere);

            children = new List<mSectionNode2>();
            for (int i = 0; i < a.children.Count; i++)
            {
                children.Add(a.children[i]);
            }
            for (int i = 0; i < b.children.Count; i++)
            {
                children.Add(b.children[i]);
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].parent = this;
            }
        }

        public void addChild(mSectionNode2 child)
        {
            child.parent = this;
            children.Add(child);
        }

        public void addChar(char newChar)
        {
            if (!chars.Contains(newChar))
            {
                chars += newChar;
                if(parent != null){
                    parent.addChar(newChar);
                }
            }
        }


        public void addFinalChar(char newChar)
        {
            if (ifStopHere==' ')
            {
                ifStopHere = newChar;
                addChar(newChar);
            }
        }

        public string getString()
        {
            List<string> strings = this.getStrings();

            string output = "";
            for (int i = 0; i < strings.Count; i++)
            {
                output += strings[i];
                if (i != strings.Count - 1)
                {
                    output += "\n";
                }
            }
            return output;
        }

        private List<string> getStrings()
        {
            List<string> strings = new List<string>();
            string letter = SectionLetter;
            if (SectionLetter.Equals("Line00"))
            {
                letter = "L";
            }
            String thisLine = letter + "[" + minValue.ToString("F2").PadLeft(5, '0') + "," + maxValue.ToString("F2").PadLeft(5, '0') + "]";
            int toAdd = thisLine.Length;
            
            if (children.Count > 0)
            {
                if (ifStopHere != ' ')
                {
                    strings.Add(ifStopHere+"");
                }
                for (int i = 0; i < children.Count; i++)
                {
                    List<string> newLines = children[i].getStrings();

                    for (int j = 0; j < newLines.Count; j++)
                    {
                        strings.Add(newLines[j]);
                    }
                }

                for (int i = 0; i < strings.Count; i++)
                {
                    String actualLine = "";
                    if (i == 0)
                    {
                        actualLine = thisLine;
                    }
                    else
                    {
                        for (int j = 0; j < toAdd; j++)
                        {
                            actualLine += " ";
                        }
                    }

                    actualLine += "-";
                    strings[i] = actualLine + strings[i];
                }
            }
            else
            {
                strings.Add(thisLine + " -" + ifStopHere);
            }
            return strings;
        }
    }
}
