using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace penToText
{
    public class convertToText2
    {


        //general data storage
        private List<mPoint> originalData;
        private List<mPoint> cleanedData;

        //canvas stuff
        private Core core;
        private mSectionNode2 root;

        //testing lists and whatnot
        private long[] tickCounts;
        private Stopwatch timer;

        public convertToText2(Core core)
        {
            timer = new Stopwatch();
            this.core = core;
            originalData = new List<mPoint>();
            cleanedData = new List<mPoint>();

            tickCounts = new long[5];
            for (int i = 0; i < tickCounts.Length; i++)
            {
                tickCounts[i] = 0;
            }
            /*
             * id 0: Rescaled Original
             * id 1: Original Sections
             * id 2: Current Section Clean
             * id 3: Kasey Section Clean
             * if 4: Dominique Section Clean
             */           

        }

        public void setTree(mSectionNode2 treeRoot)
        {
            root = treeRoot;
        }

        public void getData(BlockingCollection<mPoint> data)
        {
            foreach (var item in data.GetConsumingEnumerable())
            {
                mPoint current = item;
                
                mPoint last = null;
                if (originalData.Count > 0)
                {
                    last = originalData[originalData.Count - 1];
                }

                if (originalData.Count == 0 || !(current.X == last.X && current.Y == last.Y && current.line == last.line))
                {
                    
                    originalData.Add(current);
                    updateData();
                }
            }
        }

        public List<mPoint> scaleList(List<mPoint> data)
        {
            double newScale;
            bool worked = true;
            double xMin = double.PositiveInfinity, yMin = double.PositiveInfinity, xMax = double.NegativeInfinity, yMax = double.NegativeInfinity;
            List<mPoint> output = new List<mPoint>();
            if (data.Count > 1)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    if (xMin > data[i].X) { xMin = data[i].X; }
                    if (yMin > data[i].Y) { yMin = data[i].Y; }

                    if (xMax < data[i].X) { xMax = data[i].X; }
                    if (yMax < data[i].Y) { yMax = data[i].Y; }
                }
                newScale = (xMax - xMin);
                if (yMax - yMin > newScale) { newScale = yMax - yMin; }
                worked = !double.IsNaN(newScale) && !double.IsInfinity(newScale) && newScale != 0.0;
                for (int i = 0; i < data.Count && worked; i++)
                {
                    mPoint temp = new mPoint(((data[i].X - xMin) / newScale), ((data[i].Y - yMin) / newScale), data[i].line);
                    output.Add(temp);
                }
            }
            else { worked = false; }

            if (!worked)
            {
                output = data;
            }

            return output;
        }

        public void updateData()
        {
            /*
             * id 0: Rescaled Original
             * id 1: Original Sections
             * id 2: Current Section Clean
             * id 3: Kasey Section Clean
             * if 4: Dominique Section Clean
             */   

            timer.Start();
            cleanedData = resample(scaleList(new List<mPoint>(originalData)), .1);
            timer.Stop();
            tickCounts[0] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[0].newData(new List<mPoint>(cleanedData));
            core.TextBreakDown[0].titleText = "Original Resample From: " + originalData.Count + "\nTo: " + cleanedData.Count + "\nTicks: " + tickCounts[0];
            //core.TextBreakDown[0].myPanel.Dispatcher.BeginInvoke(new drawingDelegate(core.TextBreakDown[0].draw));
  

            //Make Initial Segments "clean2" id=1
            timer.Start();
            List<mPoint> segments = Dominique(new List<mPoint>(cleanedData));
            timer.Stop();
            tickCounts[1] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[1].newData(new List<mPoint>(segments));
            core.TextBreakDown[1].titleText = "Initial Segments\nTicks: " + tickCounts[1];
            //core.TextBreakDown[1].myPanel.Dispatcher.BeginInvoke(new drawingDelegate(core.TextBreakDown[1].draw));
            

            List<mPoint> cleanedSections;
            //Current Section Cleaning "SectionClean1" id =2
            timer.Start();
            cleanedSections = minimumLines(cleanSections(new List<mPoint>(segments)));
            timer.Stop();
            tickCounts[2] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[2].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[2].titleText = "Current section Cleaning\nTicks: " + tickCounts[2];
            //sectionClean1.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(sectionClean1.updateDraw));

            //Kasey Section Cleaning "SectionClean2" id=3
            timer.Start();
            cleanedSections = minimumLines(kaseySectionClean(new List<mPoint>(segments)));
            timer.Stop();
            tickCounts[3] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[3].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[3].titleText = "Kasey section Cleaning\nTicks: " + tickCounts[3];
            //sectionClean2.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(sectionClean2.updateDraw));

            //Dominique Section Cleaning "SectionClean3" id=4
            timer.Start();
            cleanedSections = minimumLines(dominiqueSectionClean(new List<mPoint>(segments)));
            timer.Stop();
            tickCounts[4] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[4].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[4].titleText = "Dominique section Cleaning\nTicks: " + tickCounts[4];
            //core.TextBreakDown[4].myPanel.Dispatcher.BeginInvoke(new drawingDelegate(sectionClean3.updateDraw));

            core.draw();
            
        }
        public string getSearchString(string searchFor)
        {
            Queue<mSectionNode2> frontier = new Queue<mSectionNode2>();
            Queue<int> chunkLocs = new Queue<int>();
            List<mSectionNode2> fits = new List<mSectionNode2>();
            int chunkLength = 6;
            frontier.Enqueue(root);
            chunkLocs.Enqueue(0);
            while (frontier.Count > 0)
            {
                mSectionNode2 current = frontier.Dequeue();
                int chunkAt = chunkLocs.Dequeue();
                if (searchFor.Length >= (chunkAt + 1) * chunkLength)
                {
                    String thisChunk = searchFor.Substring(chunkAt * chunkLength, chunkLength);
                    String searchChunk = "";
                    double value = 0.0;
                    if (thisChunk.Equals("Line00"))
                    {
                        searchChunk = thisChunk;
                    }
                    else
                    {
                        searchChunk = thisChunk.Substring(0, 1);
                        value = Double.Parse(thisChunk.Substring(1));
                    }
                    bool found = false;
                    for (int i = 0; i < current.children.Count && !found; i++)
                    {
                        mSectionNode2 child = current.children[i];
                        if (child.SectionLetter.Equals(searchChunk) && value >= child.minValue && value <= child.maxValue)
                        {
                            frontier.Enqueue(child);
                            chunkLocs.Enqueue(chunkAt + 1);
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        fits.Add(current);
                    }
                }
                else
                {
                    fits.Add(current);
                }
            }
            string output = "";
            string terminalString = "";
            string possibileString = "";
            for (int i = 0; i < fits.Count; i++)
            {
                //found an endcase
                if (!terminalString.Contains(fits[i].ifStopHere))
                {
                    terminalString += "" + fits[i].ifStopHere;
                }
                possibileString += fits[i].chars;
            }
            if (terminalString.Length != 0)
            {
                output = "If stopped here: " + terminalString + "\n";
            }
            possibileString = new string(possibileString.ToList().Distinct().ToArray());
            output += "Possible Letters: " + possibileString;

            return output;
        }



        /* This might be useful later
         public mSectionNode bestPosibility(String inputString, List<mSectionNode> possibilities)
         {
             String inputLetters = "";
             List<double> inputValues = new List<double>();

             int chunkAt = 0;
             int chunkLength = 6;
             while (inputString.Length >= (chunkAt + 1) * chunkLength)
             {
                 String chunk = inputString.Substring(chunkAt * chunkLength, chunkLength);
                 if(chunk.Equals("Line00")){
                     inputLetters+= chunk;
                     inputValues.Add(0.0);
                 }else{
                     inputLetters+= chunk.Substring(0,1);
                     inputValues.Add(Double.Parse(chunk.Substring(1)));
                 }
                 chunkAt++;
             }

             int bestLoc = 0;
             for (int i = 1; i < possibilities.Count; i++)
             {
                 String a = getTreeString(possibilities[bestLoc]);
                 String b = getTreeString(possibilities[i]);
                 bool sameA = inputLetters.Equals(a);
                 bool sameB = inputLetters.Equals(b);
                 if (sameB && !sameA)
                 {
                     //b is by default closer
                     bestLoc = i;
                 }
                 else if((sameA == sameB) || a.Equals(b))
                 {
                     //which is closer based on values
                     double[] aValues = getValues(possibilities[bestLoc]);
                     double[] bValues = getValues(possibilities[i]);
                     double aClose = 0;
                     double bClose = 0;
                     for (int j = 0; j < inputValues.Count && j < aValues.Length; j++) 
                     {
                         aClose += Math.Abs(inputValues[j] - aValues[j]);
                         bClose += Math.Abs(inputValues[j] - bValues[j]);
                     }

                     if (bClose < aClose)
                     {
                         bestLoc = i;
                     }

                 }
                 else 
                 {
                     //a equal and not b, neither equal to input, and not equal to each other
                     //keep bestLoc as it
                     if (!sameA)
                     {
                         if (Math.Abs(inputLetters.Length - a.Length) > Math.Abs(inputLetters.Length - b.Length))
                         {
                             bestLoc = i;
                         }
                     }
                 }
             }

             return possibilities[bestLoc];
         }
         */
        public List<mPoint> getCleanedData()
        {
            updateData();
            return new List<mPoint>(cleanedData);
        }

        public void clear()
        {
            //reset data
            originalData.Clear();
            cleanedData.Clear();

            tickCounts = new long[5];
            for (int i = 0; i < tickCounts.Length; i++)
            {
                tickCounts[i] = 0;
            }
        }

        private double length(List<mPoint> input)
        {
            double length = 0;
            if (input.Count > 1)
            {
                for (int i = 1; i < input.Count; i++)
                {
                    length += distance(input[i - 1], input[i]);
                }
            }
            return length;
        }

        private double distance(mPoint a, mPoint b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        private List<mPoint> resample(List<mPoint> data, double spaceBetweenPoints)
        {
            if (length(data) > 3 * spaceBetweenPoints)
            {
                double bigD = 0;
                double lilD = 0;
                List<mPoint> output = new List<mPoint>();
                output.Add(data[0]);
                for (int i = 1; i < data.Count; i++)
                {
                    if (data[i].line == data[i - 1].line)
                    {
                        lilD = distance(data[i], data[i - 1]);
                        if (bigD + lilD > spaceBetweenPoints)
                        {
                            mPoint temp = new mPoint((data[i - 1].X + ((spaceBetweenPoints - bigD) / lilD) * (data[i].X - data[i - 1].X)),
                                (data[i - 1].Y + ((spaceBetweenPoints - bigD) / lilD) * (data[i].Y - data[i - 1].Y)),
                                data[i].line);
                            output.Add(temp);
                            data.Insert(i, temp);
                            bigD = 0;
                        }
                        else
                        {
                            if (i == data.Count - 1) { output.Add(data[i]); }
                            bigD += lilD;
                        }
                    }
                }

                return output;
            }
            else { return data; }
        }

        public List<mPoint> Dominique(List<mPoint> input)
        {
            List<mPoint> output = new List<mPoint>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                output.Add(input[0]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = input[sLoc].line == input[i + 1].line && getDirection(input[sLoc], input[sLoc + 1]) == getDirection(input[i], input[i + 1]);
                    if (!sameSlope)
                    {
                        output.Add(input[i]);
                        sLoc = i;
                    }
                }
                output.Add(input[input.Count - 1]);
            }
            else
            {
                output = input;
            }

            return output;
        }

        public List<mPoint> minimumLines(List<mPoint> input)
        {
            int linePoints = 0;
            for (int i = 1; i < input.Count; i++)
            {
                if (input[i - 1].line == input[i].line)
                {
                    linePoints++;
                }
                else
                {
                    if (linePoints == 0)
                    {
                        //do something
                        input.Insert(i, new mPoint(input[i].X, input[i].Y + .001, input[i].line));
                    }
                    else
                    {
                        linePoints = 0;
                        if (i == (input.Count - 1))
                        {
                            input.Add(new mPoint(input[i].X, input[i].Y + .001, input[i].line));
                        }
                    }
                }
            }
            return input;
        }

        public List<mPoint> cleanSections(List<mPoint> input)
        {
            double e = .15; // height over length
            for (int i = 2; i < input.Count; i++)
            {
                if (input[i - 2].line == input[i - 1].line && input[i - 1].line == input[i].line)
                {
                    if (getDirection(input[i - 2], input[i - 1]) == getDirection(input[i - 1], input[i]))
                    {
                        input.RemoveAt(i - 1);
                        i = 2;
                    }
                    else
                    {
                        double lineDistanceA = lineDistance(input[i - 2], input[i], input[i - 1]);
                        double lineDistanceB = lineDistance(input[i - 1], input[i], input[i - 2]);
                        double lineLength;

                        if (lineDistanceA < lineDistanceB)
                        {
                            lineLength = distance(input[i - 2], input[i]);
                            if (lineDistanceA / lineLength <= e)
                            {
                                input.RemoveAt(i - 1);
                                i = 2;
                            }
                        }
                        else
                        {
                            lineLength = distance(input[i - 1], input[i]);
                            if (lineDistanceB / lineLength <= e)
                            {
                                input.RemoveAt(i - 2);
                                i = 2;
                            }
                        }
                    }
                }
            }
            return input;
        }

        public List<mPoint> kaseySectionClean(List<mPoint> input)
        {
            return cleanSections(input);
        }

        public List<mPoint> dominiqueSectionClean(List<mPoint> input)
        {
            return cleanSections(input);
        }

        private double dotProduct(Point a, Point b)
        {
            return ((a.X * b.X) + (a.Y * b.Y));
        }


        private double lineDistance(mPoint a, mPoint b, mPoint c)
        {
            //a and b make line, return distance c from line
            Point u = new Point(b.X - a.X, b.Y - a.Y);
            Point v = new Point(c.X - a.X, c.Y - a.Y);
            double value = dotProduct(u, v) / dotProduct(u, u);
            Point p = new Point(u.X * value + a.X, u.Y * value + a.Y);
            double output = double.PositiveInfinity;
            if (value >= 0.0 && value <= 1.0)
            {
                output = distance(c, new mPoint(p, c.line));
            }
            return output;

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
    }
}