using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using Petzold.Media2D;

namespace penToText
{
    public class convertToText
    {
        //general data storage
        private dynamicDisplay thisDynamicDisplay;
        private List<Point> originalData;
        public Size inputSize;

        //dominique's data
        private Point lastPoint;
        String lastDirection = "";
        String[] directionArray;
        int i = 0;        


        //canvas stuff
        private lineDrawCanvas inputCopy;
        private List<lineDrawCanvas> cleanTesters;
        public Size canvasSizes;
       
        public convertToText(dynamicDisplay display, Size inputSize)
        {
            this.inputSize = inputSize;
            
            lastPoint = new Point(-5, -5);
            originalData = new List<Point>();

            thisDynamicDisplay = display;

            //copy of input
            inputCopy = new lineDrawCanvas(0, 0, display, "Copy Input");
            inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            thisDynamicDisplay.addCanvas(inputCopy);

            //initial clean
            cleanTesters = new List<lineDrawCanvas>();
            
            lineDrawCanvas lineClean = new lineDrawCanvas(1, 0, display, "remove lines r=10");
            lineClean.outOfX = inputSize.Width;
            lineClean.outOfy = inputSize.Height;
            lineClean.toAddCircles = true;
            thisDynamicDisplay.addCanvas(lineClean);
            cleanTesters.Add(lineClean);

            lineClean = new lineDrawCanvas(0, 1, display, "remove distance d=8");
            lineClean.outOfX = inputSize.Width;
            lineClean.outOfy = inputSize.Height;
            lineClean.toAddCircles = true;
            thisDynamicDisplay.addCanvas(lineClean);
            cleanTesters.Add(lineClean);

            lineClean = new lineDrawCanvas(1, 1, display, "remove lines r=10 d=8");
            lineClean.outOfX = inputSize.Width;
            lineClean.outOfy = inputSize.Height;
            lineClean.toAddCircles = true;
            thisDynamicDisplay.addCanvas(lineClean);
            cleanTesters.Add(lineClean);

            

        }

        public void createDisplays()
        {
            //might be needed
        }

        public void endDraw()
        {
            /*int equalCount = 0;
            int aNill = 0;
            int bNill = 0;
            int bothNill = 0;
            int total = 0;
            for (int i = 2; i < originalData.Count; i++)
            {
                Point a, b, c;
                a = originalData[i-2];
                b = originalData[i - 1];
                c = originalData[i];
                double angleWith1 = angle(a, b, c);
                double angleWith2 = angle2(a, b, c);

                total++;
                if (Double.IsNaN(angleWith1) && Double.IsNaN(angleWith2))
                {
                    bothNill++;
                }
                else if (Double.IsNaN(angleWith1))
                {
                    aNill++;
                }
                else if (Double.IsNaN(angleWith2))
                {
                    bNill++;
                }
                else if (Math.Abs(angleWith1 - angleWith2) <.01)
                {
                    equalCount++;
                }
            }
            Console.WriteLine("total: " + total);
            Console.WriteLine("how many time both get the same result: " + equalCount);
            Console.WriteLine("how many times angle1 is NaN: " + aNill);
            Console.WriteLine("how many times angle2 is NaN: " + bNill);
            Console.WriteLine("both angles return NaN: " + bothNill); */

            // to test clean methods, will clean 100 times
            //start timer
            Stopwatch stopWatch = new Stopwatch();
            int testIterations = 100000;
            stopWatch.Start();
            for (int i = 0; i < testIterations; i++)
            {
                cleanTesters[0].newData(clearLines(originalData, 10));
            }
            cleanTesters[0].updateDraw();
            stopWatch.Stop();

            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
               ts.Hours, ts.Minutes, ts.Seconds,
               ts.Milliseconds / 10);

            cleanTesters[0].title.Text = cleanTesters[0].title.Text + "\n" + elapsedTime;


            //next test
            stopWatch.Reset();
            stopWatch.Start();
            for (int i = 0; i < testIterations; i++)
            {
                cleanTesters[1].newData(cleanDistance(originalData, 8));
            }
            cleanTesters[1].updateDraw();

            stopWatch.Stop();

            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
               ts.Hours, ts.Minutes, ts.Seconds,
               ts.Milliseconds / 10);

            cleanTesters[1].title.Text = cleanTesters[1].title.Text + "\n" + elapsedTime;

            stopWatch.Reset();
            stopWatch.Start();
            for (int i = 0; i < testIterations; i++)
            {
                cleanTesters[2].newData(clearLines(cleanDistance(originalData, 8), 10));
            }
            cleanTesters[2].updateDraw();

            stopWatch.Stop();

            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
               ts.Hours, ts.Minutes, ts.Seconds,
               ts.Milliseconds / 10);

            cleanTesters[2].title.Text = cleanTesters[2].title.Text + "\n" + elapsedTime;
           
        }

        public void clear()
        {
            //reset data
            originalData.Clear();
            inputCopy.newData(originalData);
            inputCopy.updateDraw();
            for (int i = 0; i < cleanTesters.Count; i++)
            {
                cleanTesters[i].newData(originalData);
            }
            //lineClean.updateDraw();
        }

        public void resize()
        {
            inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            inputCopy.updateDraw();

            for (int i = 0; i < cleanTesters.Count; i++)
            {
                lineDrawCanvas lineClean= cleanTesters[i];
                lineClean.outOfX = inputSize.Width;
                lineClean.outOfy = inputSize.Height;
            }
            /*lineClean.outOfX = inputSize.Width;
            lineClean.outOfy = inputSize.Height;
            lineClean.updateDraw();*/
        }
        
        public bool newData(Point newPoint)
        {
            //show copy of raw data
            originalData.Add(newPoint);
            inputCopy.newData(originalData);
            inputCopy.updateDraw();

           


            if (lastPoint != new Point(-5, -5))
            {
                Dominique1(newPoint);
            }      
           
            lastPoint = newPoint;

            
            
            return true;
        }

        private void Kasey1()
        {

        }

        private void Kasey2()
        {

        }

        private void Dominique1(Point newPoint)
        {
            /*String direction ="";
            if (newPoint.X > lastPoint.X)
            {
                if (newPoint.Y > lastPoint.Y)
                {
                    direction = "up positive";
                }
                else {
                    direction = "down negative";
                }
            }

            if (newPoint.X < lastPoint.X)
            {
                if (newPoint.Y > lastPoint.Y)
                {
                    direction = "up negative";
                }
                else {
                    direction = "down positive";
                }
            }

            // change in direction
            if (direction != lastDirection)
            {
                directionArray[i]=direction;
                i++;
            }

            lastDirection = direction;*/
        }

        private void Dominique2()
        {

        }

        private List<Point> clearLines(List<Point> input, double r)
        {
            double lineAngleMax = degreesToRadians(r);
            List<Point> output = new List<Point>();
            if (input.Count > 2)
            {
                output.Add(input[0]);
                output.Add(input[1]);


                for (int i = 2; i < input.Count; i++)
                {
                    Point a, b, c;
                    a = output[output.Count - 2];
                    b = output[output.Count - 1];
                    c = input[i];

                    if (Math.Abs(angle(b, a, c)-Math.PI) < lineAngleMax)
                    {
                        output.RemoveAt(output.Count - 1);
                    }

                    output.Add(c);

                }

            }
            return output;
        }

        private double angle(Point A, Point B, Point C)
        {
            double output = 0.0;

            Point a = new Point();
            a.X = A.X - B.X;
            a.Y = A.Y - B.Y;
            double aMag= Math.Sqrt((a.X*a.X)+(a.Y*a.Y));

            Point b = new Point();
            b.X = A.X - C.X;
            b.Y = A.Y - C.Y;
            double bMag= Math.Sqrt((b.X*b.X) + (b.Y*b.Y));

            output = Math.Acos((a.X * b.X + a.Y * b.Y) / (aMag * bMag));


            return Math.Abs(output);
        }

        private List<Point> cleanDistance(List<Point> input, double inDistance)
        {
            int pos = 1;
            while (pos < input.Count)
            {
                if (distance(input[pos - 1], input[pos]) < inDistance)
                {
                    input.RemoveAt(pos);
                }
                else { pos++; }
            }
            return input;
        }

        private double distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        private double degreesToRadians(double input)
        {
            return ((Math.PI) / 180.0) * input;
        }
    }    
}
