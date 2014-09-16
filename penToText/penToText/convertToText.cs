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
            inputCopy.myPanel.Width = 400;
            inputCopy.myPanel.Height = 400;
            inputCopy.toAddCircles = true;
            thisDynamicDisplay.addCanvas(inputCopy);

            //initial clean
            cleanTesters = new List<lineDrawCanvas>();            
            
            /*for (int y = 0; y <= 10; y++)
            {
                for (int x = 0; x <= 10; x++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        double r = x * 2;
                        double d = y * 2;
                        lineDrawCanvas lineClean = new lineDrawCanvas(x, y, display, "remove lines");
                        lineClean.outOfX = inputSize.Width;
                        lineClean.outOfy = inputSize.Height;
                        lineClean.myPanel.Width = 200;
                        lineClean.myPanel.Height = 200;
                        lineClean.toAddCircles = true;
                        thisDynamicDisplay.addCanvas(lineClean);
                        cleanTesters.Add(lineClean);
                    }
                }
            }*/

            lineDrawCanvas lineClean = new lineDrawCanvas(1, 0, display, "remove lines");
            lineClean.outOfX = inputSize.Width;
            lineClean.outOfy = inputSize.Height;
            lineClean.myPanel.Width = 400;
            lineClean.myPanel.Height = 400;
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
            /*// to test clean methods, will clean N times
            //start timer
           
            int testIterations = 10000;

            for (int y = 0; y <= 10; y++)
            {
                for (int x = 0; x <= 10; x++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        double r = x * 2;
                        double d = y * 2;
                        int pos=((x - 1) + (11 * y));
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        for (int i = 0; i < testIterations; i++)
                        {
                            cleanTesters[pos].newData(clearLines (clearDistance(originalData, d), r));
                        }
                        cleanTesters[pos].titleText = "Line Clean d= " + d + " r= " + r;
                        cleanTesters[pos].updateDraw();
                        stopWatch.Stop();
                        TimeSpan ts = stopWatch.Elapsed;
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                           ts.Hours, ts.Minutes, ts.Seconds,
                           ts.Milliseconds / 10);
                        cleanTesters[pos].title.Text = cleanTesters[pos].title.Text + "\n" + elapsedTime;

                    }
                }
            }
            
           

            //next test
            stopWatch.Reset();
            stopWatch.Start();
            for (int i = 0; i < testIterations; i++)
            {
                cleanTesters[1].newData(clearDistance(originalData, 8));
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
                cleanTesters[2].newData(clearLines(clearDistance(originalData, 8), 10));
            }
            cleanTesters[2].updateDraw();

            stopWatch.Stop();

            ts = stopWatch.Elapsed;
            elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
               ts.Hours, ts.Minutes, ts.Seconds,
               ts.Milliseconds / 10);

            cleanTesters[2].title.Text = cleanTesters[2].title.Text + "\n" + elapsedTime;*/

            
            double goalClean = 2 / degreesToRadians(15);
            double testcleanliness = goalClean + 1;
            List<Point> cleaned = new List<Point>(); ;
            double d=0;
            double r=0;
            int iterations=0;
            double step = .1;
            while (testcleanliness > goalClean && iterations< 1000000)
            {
                
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                List<Point> clean1 = clearLines(clearDistance(originalData, d+step), r);
                stopWatch.Stop();
                long time1 = stopWatch.ElapsedTicks;
                stopWatch.Restart();
                List<Point> clean2 = clearLines(clearDistance(originalData, d), r + step);
                stopWatch.Stop();
                long time2 = stopWatch.ElapsedTicks;
                double cleanValue1, cleanValue2;
                cleanValue1= cleanliness(clean1);
                cleanValue2 = cleanliness(clean2);
                if ( (long)cleanValue1*time1 < (long)cleanValue2*time2)
                {
                    cleaned = clean1;
                    testcleanliness = cleanValue1;
                    d += step;
                }
                else
                {
                    cleaned = clean2;
                    testcleanliness = cleanValue2;
                    r += step;
                } 
                iterations++;
            }

            cleanTesters[0].newData(cleaned);
            cleanTesters[0].titleText = "Line Clean clean#: " + testcleanliness.ToString("N3") + " out of: " + goalClean.ToString("N3") + " after :" + iterations + "\n r: " + r + " d: " + d;
            cleanTesters[0].updateDraw();

            Point average = new Point();
            average.X = 0;
            average.Y = 0;
            for (int i = 0; i < originalData.Count; i++)
            {
                average.X += originalData[i].X;
                average.Y += originalData[i].Y;
            }
            average.X /= originalData.Count;
            average.Y /= originalData.Count;

            double xScale = cleanTesters[0].myPanel.Width / cleanTesters[0].outOfX;
            double yScale = cleanTesters[0].myPanel.Height / cleanTesters[0].outOfy;

            cleanTesters[0].drawCircle(average.X * xScale, average.Y * yScale, 6);


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

        private List<Point> clearDistance(List<Point> input, double inDistance)
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


        private double cleanliness(List<Point> input)
        {
            Point average = new Point();
            average.X = 0;
            average.Y = 0;
            for (int i = 0; i < input.Count; i++)
            {
                average.X += input[i].X;
                average.Y += input[i].Y;
            }
            average.X /= input.Count;
            average.Y /= input.Count;

            double theta = 0.0;
            for (int i = 1; i < input.Count; i++)
            {
                theta += angle(average, input[i - 1], input[i]);
            }

            return (input.Count / theta);
        }

        private double degreesToRadians(double input)
        {
            return ((Math.PI) / 180.0) * input;
        }
    }    
}
