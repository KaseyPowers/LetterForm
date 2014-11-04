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
        private List<mPoint> cleanedData2;
        private List<mPoint> cleanedData3;
        public List<mPoint> scaledList;

        //canvas stuff
        private Core core;
        private mSectionNode2 root;

        //testing lists and whatnot
        private TimeSpan[] elapsedTime;

        public convertToText2(Core core)
        {
            this.core = core;
            originalData = new List<mPoint>();
            cleanedData = new List<mPoint>();
            cleanedData2 = new List<mPoint>();
            cleanedData3 = new List<mPoint>();
            scaledList = new List<mPoint>();

            elapsedTime = new TimeSpan[core.TextBreakDown.Count];
            for (int i = 0; i < elapsedTime.Length; i++)
            {
                elapsedTime[i] = TimeSpan.Zero;
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

                    elapsedTime[0] += Time(() =>
                    {
                        addToScale(current, scaledList);
                    });

                    elapsedTime[2] += Time(() =>
                    {
                        addingToSampled(current, .1);
                    });

                    updateData();
                }
            }
        }


        private double minX, minY, scale;

        private bool addToScale(mPoint adding, List<mPoint> addTo)
        {
            if (addTo.Count == 0)
            {
                minX = adding.X;
                minY = adding.Y;
                scale = 1.0;
            }

            bool changed = false;

            double yToAdd = 0.0, xToAdd = 0.0;

            if (minY > adding.Y)
            {
                yToAdd = (minY - adding.Y) / scale;
                minY = adding.Y;
                changed = true;
            }

            if (minX > adding.X)
            {
                xToAdd = (minX - adding.X) / scale;
                minX = adding.X;
                changed = true;
            }

            mPoint newPoint = new mPoint((adding.X - minX) / scale, (adding.Y - minY) / scale, 0);

            bool changeFromMax = !changed;
            bool newScaleX = true;
            if (!changed)
            {
                if (newPoint.X > newPoint.Y && newPoint.X > 1)
                {
                    changed = true;
                    newScaleX = true;
                }
                else if (newPoint.Y > newPoint.X && newPoint.Y > 1)
                {
                    changed = true;
                    newScaleX = false;
                }
            }



            addTo.Add(newPoint);




            if (changed)
            {
                double max = double.NegativeInfinity;

                for (int i = 0; i < addTo.Count; i++)
                {
                    if (i != addTo.Count - 1)
                    {
                        addTo[i].X += xToAdd;
                    }
                    if (i != addTo.Count - 1)
                    {
                        addTo[i].Y += yToAdd;
                    }
                    if (!changeFromMax)
                    {
                        if (max < addTo[i].X)
                        {
                            max = addTo[i].X;
                        }
                        if (max < addTo[i].Y)
                        {
                            max = addTo[i].Y;
                        }
                    }
                    else if (newScaleX)
                    {
                        if (max < addTo[i].X)
                        {
                            max = addTo[i].X;
                        }
                    }
                    else
                    {
                        if (max < addTo[i].Y)
                        {
                            max = addTo[i].Y;
                        }
                    }
                }

                for (int i = 0; i < addTo.Count; i++)
                {
                    addTo[i].X /= max;
                    addTo[i].Y /= max;
                }
                scale *= max;
            }

            return changed;
        }

        private double minX2, minY2, scale2;

        private bool addToScale2(mPoint adding, List<mPoint> addTo)
        {
            if (addTo.Count == 0)
            {
                minX2 = adding.X;
                minY2 = adding.Y;
                scale2 = 1.0;
            }

            bool changed = false;

            double yToAdd = 0.0, xToAdd = 0.0;

            if (minY2 > adding.Y)
            {
                yToAdd = (minY2 - adding.Y) / scale2;
                minY2 = adding.Y;
                changed = true;
            }

            if (minX2 > adding.X)
            {
                xToAdd = (minX2 - adding.X) / scale2;
                minX2 = adding.X;
                changed = true;
            }

            mPoint newPoint = new mPoint((adding.X - minX2) / scale2, (adding.Y - minY2) / scale2, 0);

            bool changeFromMax = !changed;
            bool newScaleX = true;
            if (!changed)
            {
                if (newPoint.X > newPoint.Y && newPoint.X > 1)
                {
                    changed = true;
                    newScaleX = true;
                }
                else if (newPoint.Y > newPoint.X && newPoint.Y > 1)
                {
                    changed = true;
                    newScaleX = false;
                }
            }



            addTo.Add(newPoint);




            if (changed)
            {
                double max = double.NegativeInfinity;

                for (int i = 0; i < addTo.Count; i++)
                {
                    if (i != addTo.Count - 1)
                    {
                        addTo[i].X += xToAdd;
                    }
                    if (i != addTo.Count - 1)
                    {
                        addTo[i].Y += yToAdd;
                    }
                    if (!changeFromMax)
                    {
                        if (max < addTo[i].X)
                        {
                            max = addTo[i].X;
                        }
                        if (max < addTo[i].Y)
                        {
                            max = addTo[i].Y;
                        }
                    }
                    else if (newScaleX)
                    {
                        if (max < addTo[i].X)
                        {
                            max = addTo[i].X;
                        }
                    }
                    else
                    {
                        if (max < addTo[i].Y)
                        {
                            max = addTo[i].Y;
                        }
                    }
                }

                for (int i = 0; i < addTo.Count; i++)
                {
                    addTo[i].X /= max;
                    addTo[i].Y /= max;
                }
                scale2 *= max;
            }

            return changed;
        }


        private List<mPoint> scaling;
        private List<mPoint> resampling;
        private void addingToSampled(mPoint adding, double minDistance){

            if (scaling == null)
            {
                scaling = new List<mPoint>();
            }
            if (resampling == null)
            {
                resampling = new List<mPoint>();
            }

            bool changed = addToScale2(adding, scaling);            
            
            if (scaling.Count > 1 && resampling.Count > 1)
            {
                int startPos = 1;
                List<mPoint> data = new List<mPoint>(scaling);
                double bigD = 0;
                double lilD = 0;
                List<mPoint> output = new List<mPoint>();
                
                if (!changed)
                {
                    startPos = data.Count - 1;
                    output = new List<mPoint>(resampling);
                    mPoint a = output[output.Count - 2], b = output[output.Count - 1], c = data[startPos];
                    bigD = distance(a, b);
                    lilD = distance(b, c);
                    output.RemoveAt(output.Count - 1);
                    if (bigD > minDistance)
                    {
                        output.Add(b);
                    }
                    else if (bigD + lilD > minDistance)
                    {
                        mPoint temp = new mPoint((b.X + ((minDistance - bigD) / lilD) * (c.X - b.X)),
                            (b.Y + ((minDistance - bigD) / lilD) * (c.Y - b.Y)),
                            c.line);
                        output.Add(temp);
                    } if (b.line != c.line)
                    {
                        output.Add(b);
                    }
                    output.Add(c);
                }
                else
                {
                    output.Add(data[0]);
                    for (int i = startPos; i < data.Count; i++)
                    {
                        lilD = distance(data[i], data[i - 1]);
                        if (bigD + lilD > minDistance)
                        {
                            mPoint temp = new mPoint((data[i - 1].X + ((minDistance - bigD) / lilD) * (data[i].X - data[i - 1].X)),
                                (data[i - 1].Y + ((minDistance - bigD) / lilD) * (data[i].Y - data[i - 1].Y)),
                                data[i].line);
                            output.Add(temp);
                            data.Insert(i, temp);
                            bigD = 0;
                        }
                        else
                        {
                            if (lilD == 0 || i == data.Count - 1)
                            {
                                bigD = 0;
                                output.Add(data[i]);
                            }
                            bigD += lilD;
                        }
                    }
                }

                resampling = new List<mPoint>(output);
            }
            else
            {
                resampling = new List<mPoint>(scaling);
            }

        }


        /*
        private double Xmin, Ymin, scale2, D;

        private List<mPoint> addToScaleAndSlope(mPoint addPoint, List<mPoint> addTo, double minDistance, double roundValue)
        {

            mPoint adding = new mPoint(RoundToNearest(addPoint.X, .01), RoundToNearest(addPoint.Y, .01), addPoint.line);

            if (addTo.Count == 0)
            {
                Xmin = adding.X;
                Ymin = adding.Y;
                scale2 = 1.0;
                D = 0;
            }

            bool changed = false;

            double yToAdd = 0.0, xToAdd = 0.0;

            if (Ymin > adding.Y)
            {
                yToAdd = (Ymin - adding.Y) / scale2;
                Ymin = adding.Y;
                changed = true;
            }

            if (Xmin > adding.X)
            {
                xToAdd = (Xmin - adding.X) / scale2;
                Xmin = adding.X;
                changed = true;
            }

            mPoint newPoint = new mPoint((adding.X - Xmin) / scale2, (adding.Y - Ymin) / scale2, 0);

            bool changeFromMax = !changed;
            bool newScaleX = true;
            if (!changed)
            {
                if (newPoint.X > newPoint.Y && newPoint.X > 1)
                {
                    changed = true;
                    newScaleX = true;
                }
                else if (newPoint.Y > newPoint.X && newPoint.Y > 1)
                {
                    changed = true;
                    newScaleX = false;
                }
            }



            addTo.Add(newPoint);




            if (changed)
            {
                double max = double.NegativeInfinity;

                for (int i = 0; i < addTo.Count; i++)
                {
                    if (i != addTo.Count - 1)
                    {
                        addTo[i].X += xToAdd;
                    }
                    if (i != addTo.Count - 1)
                    {
                        addTo[i].Y += yToAdd;
                    }
                    if (!changeFromMax)
                    {
                        if (max < addTo[i].X)
                        {
                            max = addTo[i].X;
                        }
                        if (max < addTo[i].Y)
                        {
                            max = addTo[i].Y;
                        }
                    }
                    else if (newScaleX)
                    {
                        if (max < addTo[i].X)
                        {
                            max = addTo[i].X;
                        }
                    }
                    else
                    {
                        if (max < addTo[i].Y)
                        {
                            max = addTo[i].Y;
                        }
                    }
                }

                for (int i = 0; i < addTo.Count; i++)
                {
                    addTo[i].X /= max;
                    addTo[i].Y /= max;
                }
                scale2 *= max;
            }



            if (addTo.Count >= 1)
            {
                double bigD = 0;
                double lilD = 0;
                int sloc = 1;
                List<mPoint>  output = new List<mPoint>();
                output.Add(addTo[0]);

                for (int i = 1; i < addTo.Count; i++)
                {
                    lilD = distance(addTo[i], addTo[i - 1]);
                    if (bigD + lilD > minDistance)
                    {
                        mPoint temp = new mPoint(
                            (addTo[i - 1].X + ((minDistance - bigD) / lilD) * (addTo[i].X - addTo[i - 1].X)),
                            (addTo[i - 1].Y + ((minDistance - bigD) / lilD) * (addTo[i].Y - addTo[i - 1].Y)),
                            addTo[i].line);
                        output.Add(temp);
                        addTo.Insert(i, temp);
                        bigD = 0;
                    }
                    else
                    {
                        if (lilD == 0 || i == addTo.Count - 1)
                        {
                            bigD = 0;
                            output.Add(addTo[i]);
                        }
                        bigD += lilD;
                    }
                }

                return output;
            }
            else
            {
                return new List<mPoint>(addTo);
            }
        }

        */
        private mPoint roundedPoint(mPoint input, double toRound)
        {
            return new mPoint(RoundToNearest(input.X, toRound), RoundToNearest(input.Y, toRound), input.line);
        }

        public int direction(mPoint a, mPoint b)
        {
            int output = 0;
            /*
             * 1: up
             * 2: down
             * 3: left
             * 4: right
             * 5: up-left
             * 6: up-right
             * 7: down-left
             * 8: down-right
             */
            if (a.X == b.X)
            {
                //up or down
                if (a.Y == b.Y)
                {
                    //no Y change
                    output = 0;
                }
                else if (a.Y > b.Y)
                {
                    //down
                    output = 2;
                }
                else if (a.Y < b.Y)
                {
                    //up
                    output = 1;
                }
            }
            else if (a.X < b.X)
            {
                //going right
                if (a.Y == b.Y)
                {
                    //no Y change
                    output = 4;
                }
                else if (a.Y > b.Y)
                {
                    //down
                    output = 8;
                }
                else if (a.Y < b.Y)
                {
                    //up
                    output = 6;
                }
            }
            else if (a.X > b.X)
            {
                //going left
                if (a.Y == b.Y)
                {
                    //no Y change
                    output = 3;
                }
                else if (a.Y > b.Y)
                {
                    //down
                    output = 5;
                }
                else if (a.Y < b.Y)
                {
                    //up
                    output = 7;
                }
            }
            return output;
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
             
            int id = 0;

            
            core.TextBreakDown[id].newData(new List<mPoint>(scaledList));
            core.TextBreakDown[id].titleText = "ReScale: From: " + originalData.Count + "\nTo: " + scaledList.Count + "\nTime: " + (elapsedTime[id].TotalMilliseconds);
            id++;


            elapsedTime[id] += Time(() =>
            {
                cleanedData = resample(new List<mPoint>(scaledList), .1);
            });            
            core.TextBreakDown[id].newData(new List<mPoint>(cleanedData));
            core.TextBreakDown[id].titleText = "Rescale From then Resample: " + originalData.Count + "\nTo: " + cleanedData.Count + "\nTime: " + (elapsedTime[id]+elapsedTime[id-1]).TotalMilliseconds;
            id++;
              

          
            core.TextBreakDown[id].newData(new List<mPoint>(resampling));
            core.TextBreakDown[id].titleText = "Original Rescale: " + originalData.Count + "\nTo: " + resampling.Count + "\nTime: " + (elapsedTime[id].TotalMilliseconds);
            id++;

            /*
            //Make Initial Segments "clean2" id=1
            timer.Start();
            List<mPoint> segments = Dominique(new List<mPoint>(cleanedData));
            timer.Stop();
            miliSecondCounts[id] += timer.ElapsedMilliseconds;
            timer.Reset();
            core.TextBreakDown[id].newData(new List<mPoint>(segments));
            core.TextBreakDown[id].titleText = "Initial Segments\nTicks: " + miliSecondCounts[id];
            

           List<mPoint> cleanedSections;
            //Current Section Cleaning "SectionClean1" id =2
            timer.Start();
            cleanedSections = minimumLines(cleanSections(new List<mPoint>(segments)));
            timer.Stop();
            miliSecondCounts[2] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[2].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[2].titleText = "Current section Cleaning\nTicks: " + miliSecondCounts[2];

            //Kasey Section Cleaning "SectionClean2" id=3
            timer.Start();
            cleanedSections = minimumLines(kaseySectionClean(new List<mPoint>(segments)));
            timer.Stop();
            miliSecondCounts[3] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[3].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[3].titleText = "Kasey section Cleaning\nTicks: " + miliSecondCounts[3];

            //Dominique Section Cleaning "SectionClean3" id=4
            timer.Start();
            cleanedSections = minimumLines(dominiqueSectionClean(new List<mPoint>(segments)));
            timer.Stop();
            miliSecondCounts[4] += timer.ElapsedTicks;
            timer.Reset();
            core.TextBreakDown[4].newData(new List<mPoint>(cleanedSections));
            core.TextBreakDown[4].titleText = "Dominique section Cleaning\nTicks: " + miliSecondCounts[4];*/

            core.draw();
            
        }
             
        public static TimeSpan Time(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
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
            //updateData();
            return new List<mPoint>(cleanedData);
        }

        /*
        public void updateDataBenchmark()
        {
            double iterations = 1000;
            //int id = 0;

            double[] milliseconds = new double[4];
            for (int i = 0; i < 4; i++)
            {
                milliseconds[i] = 0;
            }

            for (int i = 0; i < iterations; i++)
            {
                List<mPoint> scale1 = new List<mPoint>();
                List<mPoint> scale2 = new List<mPoint>();
                List<mPoint> resample1 = new List<mPoint>();
                List<mPoint> resample2 = new List<mPoint>();
                List<mPoint> original2 = new List<mPoint>();

                for (int n = 0; n < originalData.Count; n++)
                {
                    original2.Add(originalData[n]);

                    milliseconds[0] += Time(() =>
                    {
                        scale1 = scaleList(original2);
                    }).TotalMilliseconds;

                    milliseconds[1] += Time(() =>
                    {
                        addToScaleList(originalData[n],scale2);
                    }).TotalMilliseconds;

                    milliseconds[2] += Time(() =>
                    {
                        resample1 = resample(new List<mPoint>(scale1), .1);
                    }).TotalMilliseconds;

                    milliseconds[3] += Time(() =>
                    {
                        resample2 = resample(new List<mPoint>(scale2), .1);
                    }).TotalMilliseconds;                    
                }
                if (i == 0)
                {
                    core.TextBreakDown[0].titleText = "Scale all";
                    core.TextBreakDown[1].titleText = "Scale as new";
                    core.TextBreakDown[2].titleText = "Resample all";
                    core.TextBreakDown[3].titleText = "Resamel as new";
                    core.TextBreakDown[0].newData(new List<mPoint>(scale1));
                    core.TextBreakDown[1].newData(new List<mPoint>(scale2));
                    core.TextBreakDown[2].newData(new List<mPoint>(resample1));
                    core.TextBreakDown[3].newData(new List<mPoint>(resample2));
                    core.draw();
                }
            }

            core.TextBreakDown[0].titleText = "Scale all\nTime: " + milliseconds[0] + "ms";
            core.TextBreakDown[1].titleText = "Scale as new\nTime: " + milliseconds[1] + "ms";
            core.TextBreakDown[2].titleText = "Resample all\nTime: " + milliseconds[2] + "ms";
            core.TextBreakDown[3].titleText = "Resamel as new\nTime: " + milliseconds[3] + "ms";
            core.draw();
            

        }
        */
        public void clear()
        {

            //updateDataBenchmark();

            //reset data
            originalData.Clear();
            cleanedData.Clear();
            cleanedData2.Clear();
            scaledList.Clear();
            scaling.Clear();
            resampling.Clear();

            for (int i = 0; i < elapsedTime.Length; i++)
            {
                elapsedTime[i] = TimeSpan.Zero;
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
            double output = 0;
            if (a.line == b.line)
            {
                output = Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
            }
            return output;
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
                        if (lilD == 0 || i == data.Count - 1) {
                            bigD = 0;
                            output.Add(data[i]);
                        }
                        bigD += lilD;
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
    }
}