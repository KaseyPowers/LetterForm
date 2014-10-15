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
        public Size inputSize;

        //canvas stuff
        private lineDrawCanvas clean1;
        private lineDrawCanvas clean2;
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
            e = 0;

            c1 = 0;
            c2 = 0;
            c3 = 0;

            minX = 0;
            minY = 0;
            scale = 1;
            scaleChanged = false;

            thisDynamicDisplay = display;

            double side = 350;

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
        }

        public void getData(BlockingCollection<Point> data)
        {
            foreach (var item in data.GetConsumingEnumerable())
            {
                Point current = item;

                if (originalData.Count == 0 || !current.Equals(originalData[originalData.Count - 1]))
                {
                    originalData.Add(current);
                    if (originalData.Count < 2)
                    {
                        cleanedData.Add(current);
                    }
                    else
                    {
                        timer.Start();
                        cleanedData.Add(testScale(current));
                        timer.Stop();
                        c1 += timer.ElapsedTicks;
                        timer.Reset();
                    }
                    if (scaleChanged)
                    {
                        timer.Start();
                        cleanedData = scaleList(new List<Point>(cleanedData));
                        timer.Stop();
                        c1 += timer.ElapsedTicks;
                        timer.Reset();
                    }
                    cleanedData = cleanedData.Distinct().ToList();
                    cleanedData2.Add(current);
                    updateData();
                }
            }
        }

        public List<Point> scaleList(List<Point> data)
        {
            double newScale;
            bool worked = true;
            double xMin = double.PositiveInfinity, yMin = double.PositiveInfinity, xMax = 0, yMax = 0;
            List<Point> output = new List<Point>();
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
                    Point temp = new Point();
                    temp.X = ((data[i].X - xMin) / newScale);
                    temp.Y = ((data[i].Y - yMin) / newScale);
                    output.Add(temp);
                }

                minX += (xMin * scale);
                minY += (yMin * scale);
                scale *= newScale;
                scaleChanged = false;
            }
            else { worked = false; }

            if (!worked)
            {
                output = data;
                scaleChanged = false;
            }

            return output;
        }

        public Point testScale(Point input)
        {
            Point output = new Point();
            output.X = ((input.X - minX) / scale);
            output.Y = ((input.Y - minY) / scale);
            if (output.X > 1.0 || output.Y > 1.0 || output.X < 0 || output.Y < 0) { scaleChanged = true; }
            return output;
        }

        public void updateData()
        {            
            //Console.WriteLine("Copmleted 1: " + originalData.Count);*/
            timer.Start();
            clean1.newData(resample(new List<Point>(cleanedData), .1));
            timer.Stop();
            c1 += timer.ElapsedTicks;
            timer.Reset();
            //clean1.titleText = "Resample original, From: "+originalData.Count+ "\nTicks: " + c1;
            clean1.titleText = "Scaled to 1.0, From: " + originalData.Count + "\nTicks: " + c1;

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

            
        }



        public void resize()
        {

            clean2.outOfX = inputSize.Width;
            clean2.outOfy = inputSize.Height;
            clean2.updateDraw();

        }

        public void clear()
        {
            //reset data
            originalData.Clear();
            cleanedData.Clear();
            cleanedData2.Clear();           

            c1 = 0;
            c2 = 0;
            c3 = 0;
            e = 0;

            minX = 0;
            minY = 0;
            scale = 1;
            scaleChanged = false;


            clean1.newData(new List<Point>());
            clean1.updateDraw();

            clean2.newData(new List<Point>());
            clean2.updateDraw();

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
            if (data.Count > n - 1)
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
            if (length(data) > 3 * spaceBetweenPoints)
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