using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Diagnostics;

namespace penToText
{
    public class convertToText
    {

        private delegate void drawingDelegate();

        //general data storage
        private dynamicDisplay thisDynamicDisplay;
        private List<Point> originalData;
        private List<Point> cleanedData;
        private List<Point> cleanedData2;
        //private List<Point> cleanedData3;
        private List<Point> slopeData1;
        private List<Point> slopeData2;
        public Size inputSize;

        //canvas stuff
        //private lineDrawCanvas inputCopy;
        private lineDrawCanvas clean1;
        private lineDrawCanvas clean2;
        //private lineDrawCanvas clean3;
        //private lineDrawCanvas slopes1;
        //private lineDrawCanvas slopes2;
        public Size canvasSizes;

        //testing lists and whatnot
        private double goalClean;
        private double e;
        private long c1, c2, c3;
        private Stopwatch timer;

        double minX = 0;
        double minY = 0;
        double scale = 1;
        bool scaleChanged = false;

        public convertToText(dynamicDisplay display, Size inputSize)
        {
            this.inputSize = inputSize;
            timer = new Stopwatch();

            goalClean = (1 / 15.0);

            originalData = new List<Point>();            
            cleanedData = new List<Point>();
            cleanedData2 = new List<Point>();
            //cleanedData3 = new List<Point>();
            slopeData1 = new List<Point>();
            slopeData2 = new List<Point>();
            e = 0;

            c1 = 0;
            c2 = 0;
            c3 = 0;

            minX = 0;
            minY = 0;
            scale = 1;
            scaleChanged = false;

            //thisDynamicDisplay = new dynamicDisplay();
            thisDynamicDisplay = display;

            double side = 250;

            //copy of input
            /*inputCopy = new lineDrawCanvas(0, 0, display, "Threaded Copy Input");
            inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            inputCopy.myPanel.Width = side;
            inputCopy.myPanel.Height = side;
            inputCopy.toAddCircles = false;
            thisDynamicDisplay.addCanvas(inputCopy);*/

            clean1 = new lineDrawCanvas(0, 0, display, "Resample Original");
            clean1.outOfX = 1.2;
            clean1.outOfy = 1.2;
            clean1.padding = .1;
            clean1.myPanel.Width = side;
            clean1.myPanel.Height = side;
            clean1.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean1);

            clean2 = new lineDrawCanvas(1, 0, display, "Resample  as add");
            clean2.outOfX = inputSize.Width;
            clean2.outOfy = inputSize.Height;
            clean2.myPanel.Width = side;
            clean2.myPanel.Height = side;
            clean2.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean2);

            /*clean3 = new lineDrawCanvas(2, 0, display, "Clean All ");
            clean3.outOfX = inputSize.Width;
            clean3.outOfy = inputSize.Height;
            clean3.myPanel.Width = side;
            clean3.myPanel.Height = side;
            clean3.toAddCircles = true;
            thisDynamicDisplay.addCanvas(clean3);*/

            /*slopes1 = new lineDrawCanvas(0, 1, display, "Slopes1");
            slopes1.outOfX = inputSize.Width;
            slopes1.outOfy = inputSize.Height;
            slopes1.myPanel.Width = side;
            slopes1.myPanel.Height = side;
            slopes1.toAddCircles = true;
            thisDynamicDisplay.addCanvas(slopes1);

            slopes2 = new lineDrawCanvas(1, 1, display, "Slopes2");
            slopes2.outOfX = inputSize.Width;
            slopes2.outOfy = inputSize.Height;
            slopes2.myPanel.Width = side;
            slopes2.myPanel.Height = side;
            slopes2.toAddCircles = true;
            thisDynamicDisplay.addCanvas(slopes2);*/
        }

        public void getData(BlockingCollection<Point> data)
        {
            foreach (var item in data.GetConsumingEnumerable())
            {
                Point current = item;
                originalData.Add(current);
                if (originalData.Count <2)
                {
                    cleanedData.Add(current);
                }
                else
                {
                    cleanedData.Add(testScale(current));
                }
                if (scaleChanged)
                {
                    cleanedData = scaleList(new List<Point>(cleanedData));
                }
                cleanedData2.Add(current);
                //cleanedData3.Add(current);
                updateData();
            }
        }

        
        //double goalSize = 100;
        public List<Point> scaleList(List<Point> data)
        {
            double newScale;
            double xMin = data[0].X + 1, yMin = data[0].Y + 1, xMax = data[0].X - 1, yMax = data[0].Y - 1;
            List<Point> output = new List<Point>();
            if (data.Count > 1)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    xMin = Math.Min(xMin, data[i].X);
                    yMin = Math.Min(yMin, data[i].Y);

                    xMax = Math.Max(xMax, data[i].X);
                    yMax = Math.Max(yMin, data[i].Y);
                }
                newScale = Math.Max((xMax - xMin), (yMax - yMin));
                for (int i = 0; i < data.Count; i++)
                {
                    Point temp = new Point();
                    temp.X =  ((data[i].X - xMin) / newScale);
                    temp.Y =  ((data[i].Y - yMin) / newScale);
                    output.Add(temp);
                }

                minX += (xMin * scale);
                minY += (yMin * scale);
                scale *= newScale;
                scaleChanged = false;
            }
            else
            {
                output = data;
            }
            return output;
        }

        public Point testScale(Point input)
        {
            Point output = new Point();
            output.X =  ((input.X - minX) / scale);
            output.Y =  ((input.Y - minY) / scale);
            //if (output.X > goalSize || output.Y > goalSize || output.X < 0 || output.Y < 0) { scaleChanged = true; }
            if (output.X > 1.0 || output.Y > 1.0 || output.X < 0 || output.Y < 0) { scaleChanged = true; }
            return output;
        }

        public void updateData()
        {
            //Thread.Sleep(1000);
            /*inputCopy.newData(originalData);
            inputCopy.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(inputCopy.updateDraw));

            double testClean = goalClean + 1;
            double step = .1;
            //if (e != 0) { e -= step; }

            //double originalClean = cleanliness(originalData);

            int i = 0;
            int iterations = 100;
            /*if (e != 0) {
                int steps = (int)(e / step);
                steps = (int)((double)steps * .75);
                e = (double)(steps * step);

            }

            timer.Start(); 
            while (testClean > goalClean && i < iterations)
            {
                e += step;
                i++;

                cleanedData = RDPclean(originalData, e);
                //cleanedData = cleanedData.Distinct().ToList();
                testClean = cleanliness(cleanedData);
                
                //Console.WriteLine("e: " + e + " cleanliness: " + testClean +" out ouf: " + goalClean);
            }

            if (i == iterations)
            {
                e = 0;
                cleanedData = originalData;
            }
            timer.Stop();
            c1 += timer.ElapsedTicks;
            timer.Reset();
            clean1.newData(cleanedData);
            clean1.titleText = "Clean from: " + originalData.Count + " e: " + e + "\nClean: " + testClean +" out of: " +goalClean + "\nTicks: " + c1;
            clean1.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean1.updateDraw));*/

            /*int goalPointCount = 20;

            timer.Start();
            cleanedData = resample(new List<Point>(originalData), goalPointCount);
            timer.Stop();

            c1 += timer.ElapsedTicks;
            timer.Reset();

            //Console.WriteLine("Copmleted 1: " + originalData.Count);*/
            
            clean1.newData(resample(new List<Point>(cleanedData), .1));
            //clean1.titleText = "Resample original, From: "+originalData.Count+ "\nTicks: " + c1;
            clean1.titleText = "Scaled to 1.0, From: " + originalData.Count;
            clean1.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean1.updateDraw));

            //Console.WriteLine("Completed drawing 1");



            //List<Point> testing = cleanedData2;
            //int lines = goalPointCount-1;
            double pointsPerLength = cleanedData2.Count / length(cleanedData2);


            timer.Start();
            
            //lines++;
            cleanedData2 = resample(new List<Point>(cleanedData2), 15.0);
            

            //cleanedData2 = testing;
            timer.Stop();

            c2 += timer.ElapsedTicks;
            timer.Reset();

            //Console.WriteLine("Copmleted 2: " + originalData.Count);

            clean2.newData(cleanedData2);
            clean2.titleText = "Resample as new, From: " + originalData.Count + "\nTicks: " + c2;
            clean2.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean2.updateDraw));

            //Console.WriteLine("Completed drawing 2");
           
            //if (e2 != 0) { e2 -= step; }
            //if (e2 != 0) { e2 /= 2; }
            //e2 = 0;
            double step = .1;

            /*if (e != 0)
            {
                int steps = (int)(e / step);
                steps = (int)((double)steps * .5);
                e = (double)(steps * step);

            }
            e = 0;
            double testClean = goalClean + 1;
            int i = 0;
            int iterations = 100;
           
            List<Point> temp = cleanedData3;
            //goalClean *= 1.25;
            String newTitle = "Clean e: ";
            timer.Start();
            while (testClean > goalClean && i < iterations)
            {
                e += step;
                i++;

                temp = RDPclean(cleanedData3, e);
                //temp = temp.Distinct().ToList();
                testClean = cleanliness(temp);

                //Console.WriteLine("e: " + e + " cleanliness: " + testClean +" out ouf: " + goalClean);
            }

            if (i == iterations)
            {
                e = 0;
                newTitle += "F";
                //cleanedData = originalData;
            }
            else
            {
                cleanedData3 = temp;
                newTitle += e + "\nClean: " + testClean.ToString("F3") + " out of: " + goalClean.ToString("F3");
            }

            timer.Stop();
            c3 += timer.ElapsedTicks;
            timer.Reset();
            newTitle += "\nTicks: " + c3;
            clean3.newData(cleanedData3);
            clean3.titleText = newTitle;
            clean3.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean3.updateDraw));

            /*slopeData1 = Dominique2(cleanedData);
            slopes1.newData(slopeData1);
            slopes1.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(slopes1.updateDraw));

            slopeData2 = Dominique2(cleanedData2);
            slopes2.newData(slopeData2);
            slopes2.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(slopes2.updateDraw));*/
        }

        

        public void resize()
        {
            /*inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            inputCopy.updateDraw();*/

            //clean1.outOfX = inputSize.Width;
            //clean1.outOfy = inputSize.Height;
            /*clean1.outOfX = goalSize;
            clean1.outOfy = goalSize;
            clean1.updateDraw();*/

            clean2.outOfX = inputSize.Width;
            clean2.outOfy = inputSize.Height;
            clean2.updateDraw();

            /*clean3.outOfX = inputSize.Width;
            clean3.outOfy = inputSize.Height;
            clean3.updateDraw();*/

            /*slopes1.outOfX = inputSize.Width;
            slopes1.outOfy = inputSize.Height;
            slopes1.updateDraw();

            slopes2.outOfX = inputSize.Width;
            slopes2.outOfy = inputSize.Height;
            slopes2.updateDraw();*/
        }

        public void clear()
        {
            //reset data
            originalData.Clear();
            cleanedData.Clear();
            cleanedData2.Clear();
            //cleanedData3.Clear();
            slopeData1.Clear();
            slopeData2.Clear();

            c1 = 0;
            c2 = 0;
            c3 = 0;
            e = 0;

            minX = 0;
            minY = 0;
            scale = 1;
            scaleChanged = false;


            /*inputCopy.newData(originalData);
            inputCopy.updateDraw();*/

            clean1.newData(originalData);
            clean1.updateDraw();

            clean2.newData(originalData);
            clean2.updateDraw();

            /*clean3.newData(originalData);
            clean3.updateDraw();*/


            /*slopes1.newData(originalData);
            slopes1.updateDraw();

            slopes2.newData(originalData);
            slopes2.updateDraw();*/
        }

        private List<Point> RDPclean(List<Point> input, double epsilon)
        {
            List<Point> output = new List<Point>();
            if (input.Count > 2)
            {
                double maxDistance = 0.0;
                int pos = 0;
                //find the greatest distance
                for (int i = 1; i < input.Count - 1; i++)
                {
                    double d = lineDistance(input[i], input[0], input[input.Count - 1]);
                    if (d > maxDistance)
                    {
                        maxDistance = d;
                        pos = i;
                    }
                }

                if (maxDistance > epsilon)
                {


                    output = RDPclean(input.GetRange(0, pos + 1), epsilon);
                    output.RemoveAt(output.Count - 1);
                    output.AddRange(RDPclean(input.GetRange(pos, (input.Count - pos)), epsilon));

                    //output = output.Distinct().ToList();

                }
                else
                {
                    //no points in list are long enough, so just return first and last point
                    output.Add(input[0]);
                    output.Add(input[input.Count - 1]);
                }
            }
            else
            {
                output = input;
            }
            return output;
        }

        private double lineDistance(Point a, Point lineA, Point lineB)
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

        private double cleanliness(List<Point> input)
        {
            return (input.Count / length(input));
        }

        private double length(List<Point> input)
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

        private double distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        private List<Point> resample(List<Point> data, int n)
        {
            if (data.Count > n-1)
            {
                double spaceBetweenPoints = length(data) / (n - 1);
                double bigD = 0;
                double lilD = 0;
                List<Point> output = new List<Point>();
                output.Add(data[0]);
                for (int i = 1; i < data.Count; i++)
                {
                    lilD = distance(data[i], data[i - 1]);
                    if (bigD + lilD > spaceBetweenPoints)
                    {
                        Point temp = new Point();
                        temp.X = data[i - 1].X + ((spaceBetweenPoints - bigD) / lilD) * (data[i].X - data[i - 1].X);
                        temp.Y = data[i - 1].Y + ((spaceBetweenPoints - bigD) / lilD) * (data[i].Y - data[i - 1].Y);
                        output.Add(temp);
                        data.Insert(i, temp);
                        //i--;
                        bigD = 0;
                    }
                    else
                    {
                        if (i == data.Count - 1) { output.Add(data[i]); }
                        bigD += lilD;
                    }
                }
                
                return output;
            }
            else { return data; }
        }

        private List<Point> resample(List<Point> data, double spaceBetweenPoints)
        {
            if (length(data) > 3*spaceBetweenPoints)
            {
                double bigD = 0;
                double lilD = 0;
                List<Point> output = new List<Point>();
                output.Add(data[0]);
                for (int i = 1; i < data.Count; i++)
                {
                    lilD = distance(data[i], data[i - 1]);
                    if (bigD + lilD > spaceBetweenPoints)
                    {
                        Point temp = new Point();
                        temp.X = data[i - 1].X + ((spaceBetweenPoints - bigD) / lilD) * (data[i].X - data[i - 1].X);
                        temp.Y = data[i - 1].Y + ((spaceBetweenPoints - bigD) / lilD) * (data[i].Y - data[i - 1].Y);
                        output.Add(temp);
                        data.Insert(i, temp);
                        //i--;
                        bigD = 0;
                    }
                    else
                    {
                        if (i == data.Count - 1) { output.Add(data[i]); }
                        bigD += lilD;
                    }
                }

                return output;
            }
            else { return data; }
        }

        private List<Point> Dominique2(List<Point> input)
        {
            List<Point> output = new List<Point>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                output.Add(input[0]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = ((xChange(input[sLoc], input[sLoc + 1]) == xChange(input[i], input[i + 1])) && (yChange(input[sLoc], input[sLoc + 1]) == yChange(input[i], input[i + 1])));
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

        private int xChange(Point a, Point b)
        {
            //0 is same, 1 a is greater, 2 b is greater;
            int output = 0;
            if (a.X > b.X)
            {
                output = 1;
            }
            else if (a.X < b.X)
            {
                output = 2;
            }
            return output;
        }

        private int yChange(Point a, Point b)
        {
            int output = 0;
            if (a.Y > b.Y)
            {
                output = 1;
            }
            else if (a.Y < b.Y)
            {
                output = 2;
            }
            return output;
        }
    }
}
