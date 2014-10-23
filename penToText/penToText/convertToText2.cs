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
    public class convertToText2
    {

        private delegate void drawingDelegate();

        //general data storage
        private dynamicDisplay thisDynamicDisplay;
        private List<mPoint> originalData;
        private List<mPoint> cleanedData;

        //canvas stuff
        private multiLineDrawCanvas clean;
        public Size canvasSizes;

        //testing lists and whatnot
        private long c1;
        private Stopwatch timer;

        private bool activeDisplay;

        public convertToText2(dynamicDisplay display)
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
        }

        public void setDisplayActive(bool isActive)
        {
            activeDisplay = isActive;
        }

        public bool getDisplayActive()
        {
            return activeDisplay;
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
                clean.newData(cleanedData);
                clean.titleText = "Scale original From: " + originalData.Count + "\nTicks: " + c1;
                clean.myPanel.Dispatcher.BeginInvoke(new drawingDelegate(clean.updateDraw));
            }
        }

        public List<mPoint> getCleanedData()
        {
            return cleanedData;
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
        }


        private double lineDistance(mPoint a, mPoint lineA, mPoint lineB)
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
                    if(data[i].line == data[i-1].line){
                    lilD = distance(data[i], data[i - 1]);
                        if (bigD + lilD > spaceBetweenPoints )
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

        /*private List<Point> Dominique2(List<Point> input)
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
        }*/
    }
}