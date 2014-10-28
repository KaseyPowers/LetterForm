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
    public class convertToText
    {

        private delegate void drawingDelegate();

        //general data storage
        private dynamicDisplay thisDynamicDisplay;
        private List<mPoint> originalData;
        private List<mPoint> cleanedData;

        //canvas stuff
        private multiLineDrawCanvas clean;
        private multiLineDrawCanvas clean2;
        private multiLineDrawCanvas clean3;
        private multiLineDrawCanvas characters;
        private mSectionNode root;
        public Size canvasSizes;

        //testing lists and whatnot
        private long c1;
        private Stopwatch timer;

        private bool activeDisplay;

        public convertToText(dynamicDisplay display)
        {
            timer = new Stopwatch();

            originalData = new List<mPoint>();
            cleanedData = new List<mPoint>();

            c1 = 0;

            thisDynamicDisplay = display;

            double side = 350;

            clean = new multiLineDrawCanvas(0, 0, display, "Resample Original");
            clean.outOfX = 1.2;
            clean.outOfy = 1.2;
            clean.padding = .1;
            clean.myPanel.Width = side;
            clean.myPanel.Height = side;
            clean.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean);

            clean2 = new multiLineDrawCanvas(0, 1, display, "SectionTest");
            clean2.outOfX = 1.2;
            clean2.outOfy = 1.2;
            clean2.padding = .1;
            clean2.myPanel.Width = side;
            clean2.myPanel.Height = side;
            clean2.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean2);

            clean3 = new multiLineDrawCanvas(1, 1, display, "CleanedSectionTest");
            clean3.outOfX = 1.2;
            clean3.outOfy = 1.2;
            clean3.padding = .1;
            clean3.myPanel.Width = side;
            clean3.myPanel.Height = side;
            clean3.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean3);

            characters = new multiLineDrawCanvas(1, 0, display, "Character Options");
            characters.outOfX = 1.2;
            characters.outOfy = 1.2;
            characters.padding = .1;
            characters.myPanel.Width = side;
            characters.myPanel.Height = side;
            thisDynamicDisplay.addCanvas(characters);
        }

        public void setDisplayActive(bool isActive)
        {
            activeDisplay = isActive;
        }

        public bool getDisplayActive()
        {
            return activeDisplay;
        }

        public void setTree(mSectionNode treeRoot){
            root = treeRoot;
        }

        public void getData(BlockingCollection<mPoint> data)
        {
            foreach (var item in data.GetConsumingEnumerable())
            {
                mPoint current = item;

                if (originalData.Count == 0 || !current.Equals(originalData[originalData.Count - 1]))
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
            double xMin = double.PositiveInfinity, yMin = double.PositiveInfinity, xMax = 0, yMax = 0;
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
            timer.Start();

            cleanedData = resample(scaleList(new List<mPoint>(originalData)), .1);

            timer.Stop();

            c1 += timer.ElapsedTicks;
            timer.Reset();

            if (activeDisplay)
            {
                clean.newData(new List<mPoint>(cleanedData));
                clean.titleText = "Scale original From: " + originalData.Count + "\nTicks: " + c1;
                clean.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean.updateDraw));
            }
            if (cleanedData.Count > 1)
            {
                List<mPoint> sectionData = Dominique(resample(scaleList(new List<mPoint>(originalData)), .1));

                if (activeDisplay)
                {
                    clean2.newData(new List<mPoint>(sectionData));
                    clean2.titleText = "Section Test";
                    clean2.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean2.updateDraw));
                }

                //List<mPoint> sectionData2 = minimumLines(cleanSections(Dominique(resample(scaleList(new List<mPoint>(originalData)), .1))));

                if (activeDisplay)
                {
                    String searchFor = (new mLetterSections(minimumLines(cleanSections(Dominique(resample(scaleList(new List<mPoint>(originalData)), .1)))))).getString(true);
                    mSectionNode found = searchTree(searchFor);
                    String options = found.chars;
                    String ifStoppedHere = found.ifStopHere;

                    clean3.newData(minimumLines(cleanSections(Dominique(resample(scaleList(new List<mPoint>(originalData)), .1)))));
                    clean3.titleText = "Current Letter" + searchFor + "\nCurrent Options: " + options + "\nOption if you stop: " + ifStoppedHere;
                    clean3.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean3.updateDraw));                    
                }

            }
        }

        public mSectionNode searchTree(string searchFor)
        {
            int chunkAt = 0;
            int chunkLegth = 6;
            mSectionNode output = root;
            bool toContinue = true;
            
            Queue<mSectionNode> frontier = new Queue<mSectionNode>();
            Queue<int> chunks = new Queue<int>();
            frontier.Enqueue(root);
            chunks.Enqueue(0);

            List<mSectionNode> possibilites = new List<mSectionNode>();

            while (frontier.Count != 0)
            {
                mSectionNode current = frontier.Dequeue();
                chunkAt = chunks.Dequeue();

                if (searchFor.Length >= ((chunkAt + 1) * chunkLegth))
                {
                    String thisChunk = searchFor.Substring(chunkAt * chunkLegth, chunkLegth);
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
                    for (int i = 0; i < current.children.Count; i++)
                    {
                        mSectionNode child = current.children[i];
                        if (child.SectionLetter.Equals(searchChunk))
                        {
                            frontier.Enqueue(child);
                            chunks.Enqueue(chunkAt + 1);
                            found = true;
                        }
                    }

                    if (!found)
                    {
                        possibilites.Add(current);
                    }
                }
                else
                {
                    possibilites.Add(current);
                }
            }

            return bestPosibility(searchFor, possibilites);

           /* while (toContinue)
            {
                if (searchFor.Length >= ((chunkAt + 1) * chunkLegth))
                {
                    String thisChunk = searchFor.Substring(chunkAt * chunkLegth, chunkLegth);
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
                    bool[] validLocs = new bool[current.children.Count];
                    for (int i = 0; i < current.children.Count && !found; i++)
                    {
                        mSectionNode child = current.children[i];
                        if (child.SectionLetter.Equals(searchChunk))
                        {
                            validLocs[i] = true;
                            chunkAt++;
                            found = true;
                        }
                        else
                        {
                            validLocs[i] = false;
                        }
                    }

                    if (!found)
                    {
                        toContinue = false;
                    }
                    else
                    {
                        int closestLoc = -1;
                        double bestValue = double.PositiveInfinity;
                        for (int i = 0; i < current.children.Count; i++)
                        {
                            if (validLocs[i])
                            {
                                double thisValue = Math.Abs(value - current.children[i].SectoinValue);
                                if (thisValue < bestValue)
                                {
                                    bestValue = thisValue;
                                    closestLoc = i;
                                }
                            }
                        }

                        if (closestLoc != -1)
                        {
                            current = current.children[closestLoc];
                        }
                    }
                }
                else
                {
                    toContinue = false;
                }
            }

            return current;*/
        }

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

        public double[] getValues(mSectionNode start)
        {
            List<double> output = new List<double>();
            mSectionNode current = start;
            while (current.parent != null)
            {
                output.Insert(0, current.SectoinValue);
                current = current.parent;
            }
            return output.ToArray();
        }

        public List<mPoint> getCleanedData()
        {
            updateData();
            return new List<mPoint>(cleanedData);
        }

        public String getTreeString(mSectionNode input){
            String output = "";
            mSectionNode current = input;
            while (current.parent != null)
            {
                output = current.SectionLetter + output;
                current = current.parent;
            }
            return output;
        }


        public void resize()
        {

        }

        public void clear()
        {
            //reset data
            originalData.Clear();
            cleanedData.Clear();

            c1 = 0;

            clean.newData(new List<mPoint>());
            clean.updateDraw();

            clean2.newData(new List<mPoint>());
            clean2.updateDraw();
        }


       /* private double lineDistance(mPoint a, mPoint lineA, mPoint lineB)
        {
            double X0, X1, X2, Y0, Y1, Y2;
            X0 = a.X;
            Y0 = a.Y;
            X1 = lineA.X;
            Y1 = lineA.Y;
            X2 = lineB.X;
            Y2 = lineB.Y;

            double numerator = Math.Abs((X2 - X1) * (Y1 - Y0) - (X1 - X0) * (Y2 - Y1));
            double denominator = Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));

            return (numerator / denominator);
        }

        private double cleanliness(List<mPoint> input)
        {
            return (input.Count / length(input));
        }*/

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


        /*private List<mLetterPortion> Dominique(List<mPoint> input)
        {
            List<mLetterPortion> output = new List<mLetterPortion>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = getDirection(input[sLoc], input[sLoc + 1]) == getDirection(input[i], input[i + 1]) && input[sLoc].line == input[i].line;
                    if (!sameSlope)
                    {
                        output.Add(new mLetterPortion(input[sLoc], input[i]));
                        sLoc = i;
                    }
                }
                output.Add(new mLetterPortion(input[sLoc], input[input.Count -1]));
            }
            return output;
        }*/
        public List<mPoint> Dominique(List<mPoint> input)
        {
            List<mPoint> output = new List<mPoint>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                output.Add(input[0]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = input[sLoc].line == input[i + 1].line && getDirection(input[sLoc], input[sLoc + 1]) == getDirection(input[i], input[i + 1]) && input[sLoc].line == input[i].line;
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
            double e = .2; // height over length
            for (int i = 2; i < input.Count; i++)
            {
                if (input[i - 2].line == input[i - 1].line && input[i - 1].line == input[i].line) 
                {
                    /*double thisLineDistance = lineDistance(input[i - 2], input[i], input[i - 1]);
                    double lineLength = distance(input[i-2], input[i]);
                    lineLength = lineLength * lineLength;
                    if (thisLineDistance / lineLength <= e)
                    {
                        input.RemoveAt(i - 1);
                        i--;
                    }*/
                    double lineDistanceA = lineDistance(input[i - 2], input[i], input[i - 1]);
                    double lineDistanceB = lineDistance(input[i - 1], input[i], input[i - 2]);
                    double lineLength;

                    if (lineDistanceA < lineDistanceB)
                    {
                        lineLength = distance(input[i - 2], input[i]);
                        if (lineDistanceA / lineLength <= e)
                        {
                            input.RemoveAt(i - 1);
                            i--;
                        }
                    }
                    else
                    {
                        lineLength = distance(input[i - 1], input[i]);
                        if (lineDistanceB / lineLength <= e)
                        {
                            input.RemoveAt(i - 2);
                            i--;
                        }

                    }

                    
                }
            }
            return input;
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
            if (value >= 0.0 && value <= 1.0) {
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