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
        private lineDrawCanvas dominique1;
        private lineDrawCanvas dominique2;
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

            dominique1 = new lineDrawCanvas(0, 1, display, "Dominique 1");
            dominique1.outOfX = inputSize.Width;
            dominique1.outOfy = inputSize.Height;
            dominique1.myPanel.Width = 400;
            dominique1.myPanel.Height = 400;
            dominique1.toAddCircles = true;
            thisDynamicDisplay.addCanvas(dominique1);

            dominique2 = new lineDrawCanvas(1, 1, display, "Dominique 2");
            dominique2.outOfX = inputSize.Width;
            dominique2.outOfy = inputSize.Height;
            dominique2.myPanel.Width = 400;
            dominique2.myPanel.Height = 400;
            dominique2.toAddCircles = true;
            thisDynamicDisplay.addCanvas(dominique2);


            /*Point a = new Point();
            Point b = new Point();
            Point c = new Point();
            a.X = 1;
            b.X = 2;
            c.X = 3;
            a.Y = 4;
            b.Y = 2;
            c.Y = 1;

            Console.WriteLine(KsectionToString(getSection(a, b, c)));
            Console.WriteLine(KsectionToString(getSection(c, b, a)));
            double[] xValues = { .1, .2, .3, .4, .5, .6, .7, .8, .9 };
            double[] yValues = { .75, .85, .9, .875, .825, .75, .63, .525, .375 };
            for (int i = xValues.Length-1; i >=0; i--)
            {
                Point temp = new Point();
                temp.X = (int)((xValues[i]) * 300.0);
                temp.Y = (int)((1.0- yValues[i]) * 300.0);
                newData(temp);
            }
           kasey1.newData(Kasey1(originalData));
           kasey1.updateDraw();*/
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

            
            double goalClean = 2 / degreesToRadians(13);
            double testcleanliness = goalClean + 1;
            List<Point> cleaned = new List<Point>(); ;
            double d=2;
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
                if ( cleanValue1*time1 < cleanValue2*time2)
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


            dominique2.newData(Dominique2(cleaned));
            dominique2.updateDraw();


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

            dominique1.outOfX = inputSize.Width;
            dominique1.outOfy = inputSize.Height;
            dominique1.updateDraw();

            dominique2.outOfX = inputSize.Width;
            dominique2.outOfy = inputSize.Height;
            dominique2.updateDraw();

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

            dominique1.newData(Dominique2(originalData));
            dominique1.updateDraw();


            if (lastPoint != new Point(-5, -5))
            {
                Dominique1(newPoint);
            }      
           
            lastPoint = newPoint;

            
            
            return true;
        }

        private List<Point> Kasey1(List<Point> input)
        {
            List<Point> sectionDevision = new List<Point>();
            Stack<Ksection> sections = new Stack<Ksection>();
            if (input.Count > 3)
            {
                
                //sectionDevision.Add(originalData[0]);
                //sectionDevision.Add(originalData[1]);
                for (int i = 2; i < input.Count; i++)
                {
                    Point a, b, c;
                    //a = sectionDevision[sectionDevision.Count - 2];
                    //b = sectionDevision[sectionDevision.Count - 1];
                    a = input[i - 2];
                    b = input[i - 1];
                    c = input[i];
                    Ksection sectionOne = getSection(a, b, c);
                    sectionOne.startLoc = i - 2;
                    sectionOne.endLoc = i;
                    if (sections.Count == 0)
                    {
                        //sectionDevision.Add(c);
                        sections.Push(sectionOne);
                    }
                    else
                    {

                        Ksection sectionTwo = sections.Pop();
                        bool combined = false;
                        if (sameQuadrant(sectionOne, sectionTwo) || !sectionTwo.validSection)
                        {
                            combined = true;
                            //try combining
                            if (!sectionTwo.validSection)
                            {
                                Point average = new Point();
                                average.X = 0;
                                average.Y = 0;
                                for (int j = sectionTwo.startLoc + 1; j < sectionOne.endLoc; j++)
                                {
                                    average.X += input[j].X;
                                    average.Y += input[j].Y;
                                }
                                double avgX = average.X / ((double)(sectionOne.endLoc - sectionTwo.startLoc - 1));
                                double avgY = average.Y / ((double)(sectionOne.endLoc - sectionTwo.startLoc - 1));
                                average.X = (int)avgX;
                                average.Y = (int)avgY;
                                Ksection test = getSection(input[sectionTwo.startLoc], average, input[sectionOne.endLoc]);
                                if (test.validSection) {
                                    int startPos = sectionTwo.startLoc;
                                    sectionTwo = test;
                                    sectionTwo.startLoc = startPos;
                                }
                            }
                            if (combined)
                            {
                                sectionTwo.endLoc = sectionOne.endLoc;
                                sections.Push(sectionTwo);
                            }
                        }

                        if(!combined)
                        {
                            /*test sections
                             * if sections go ABA test section for first point, 
                             * the average of middle points, and the last point
                             * if sections have to invalid ones in a row do same the check*/
                            
                            
                            if (sections.Count > 0)
                            {
                                Ksection sectionThree = sections.Pop();
                                combined = (sameQuadrant(sectionOne, sectionThree) || ((!sectionThree.validSection || !sectionOne.validSection) && !sectionTwo.validSection));
                                if (combined)
                                {
                                    Point average = new Point();
                                    average.X = 0;
                                    average.Y = 0;
                                    for (int j = sectionThree.startLoc + 1; j < sectionOne.endLoc; j++)
                                    {
                                        average.X += input[j].X;
                                        average.Y += input[j].Y;
                                    }
                                    double avgX = average.X / ((double)(sectionOne.endLoc - sectionThree.startLoc - 1));
                                    double avgY = average.Y / ((double)(sectionOne.endLoc - sectionThree.startLoc - 1));
                                     average.X = (int)avgX;
                                     average.Y = (int)avgY;
                                    Ksection test= getSection(input[sectionThree.startLoc], average, input[sectionOne.endLoc]);
                                    combined = (sectionThree.validSection && sameQuadrant(test, sectionThree));
                                    combined = combined || (!sectionThree.validSection && sectionOne.validSection && sameQuadrant(test, sectionOne));
                                    combined = combined || (!test.validSection && !sectionTwo.validSection);
                                    if (combined)
                                    {
                                        sectionThree.endLoc = sectionOne.endLoc;
                                    }
                                    
                                }
                                sections.Push(sectionThree);
                            }
                            if(!combined)
                            {
                                //sectionOne.startLoc = sectionDevision.Count - 2;
                                //sectionOne.startLoc = sectionDevision.Count;
                                
                                //sectionDevision.Add(c);
                                sections.Push(sectionTwo);
                                sections.Push(sectionOne);
                            }

                            
                        }
                    }

                }
            }
            /*for (int i = 0; i < sections.Count; i++)
            {
                int thisStart = sections[i].startLoc;
                int thisEnd = sections[i].endLoc;
                if (i > 0)
                {
                    int lastEnd = sections[i - 1].endLoc;
                    if (thisStart > lastEnd) { sectionDevision.Add(originalData[thisStart]); }
                }
                else { sectionDevision.Add(originalData[thisStart]); }

                if (i < sections.Count - 1)
                {
                    int nextStart = sections[i + 1].startLoc;
                    if (nextStart < thisEnd) { sectionDevision.Add(originalData[nextStart]); }
                }
                sectionDevision.Add(originalData[thisEnd]);
            }*/
            List<int> locs = new List<int>();
            while(sections.Count >0)
            {
                Ksection temp = sections.Pop();
                Console.WriteLine(KsectionToString(temp));
                locs.Add(temp.startLoc);
                //locs.Add((sections[i].startLoc + sections[i].endLoc) / 2);
                locs.Add(temp.endLoc);
            }
            locs.Sort();
            locs = locs.Distinct().ToList();
            for (int i = 0; i < locs.Count; i++)
            {
                sectionDevision.Add(input[locs[i]]);
            }
                return sectionDevision;
        }

        private bool sameQuadrant(Ksection a, Ksection b)
        {
            return (a.validSection && b.validSection && a.goesRight == b.goesRight && a.onTop == b.onTop && a.rightSide && b.rightSide);
        }

        /*private List<Point> adjustSection(Ksection section, List<Point> adjusted)
        {
            int range = section.endLoc - section.startLoc;
            if (range > 5)
            {
                Point[] newPoints= new Point[3];
                range--;
                if (range % 3 == 0)
                {
                    
                    int subset = range / 3;
                    for (int i = 0; i < 3; i++)
                    {
                        newPoints[i] = new Point();
                        newPoints[i].X = 0;
                        newPoints[i].Y = 0;
                        for (int j = 0; j < subset; j++)
                        {
                            newPoints[i].X += adjusted[section.startLoc + 1 + j + (i * subset)].X;
                            newPoints[i].Y += adjusted[section.startLoc + 1 + j + (i * subset)].Y;
                        }
                        newPoints[i].X = newPoints[i].X / subset;
                        newPoints[i].Y = newPoints[i].Y / subset;
                    }
                }
                else if (range % 2 == 0)
                {
                    //finish later
                }
            }
            return adjusted;
        }*/
        private String KsectionToString(Ksection temp)
        {
            String output = "";           
            if (temp.validSection)
            {
                output += "Valid ";
                if (temp.goesRight) { output += "R"; } else { output += "L"; }
                if (temp.onTop) { output += "T"; } else { output += "B"; }
                if (temp.rightSide) { output += "R"; } else { output += "L"; }

               

            }
            else { output = "invalid"; }
            output += " Start: " + temp.startLoc + " End: " + temp.endLoc;
            return output;
        }

        private Ksection getSection( Point a, Point b, Point c){                     
            Ksection output= new Ksection();
            output.validSection = true;
            output.goesRight = true;
            output.onTop = true;
            output.rightSide = true;
            if(a.X> b.X && b.X>c.X){
                output.goesRight = false;
            }
            else if (a.X < b.X && b.X < c.X)
            {
                output.goesRight = true;
            }
            else
            {
                output.validSection = false;
            }
            double m1 = (b.Y - a.Y) / ((double)(b.X - a.X));
            double m2 = (c.Y - b.Y) / ((double)(c.X - b.X));

           // Console.WriteLine("m1: " + m1 + " m2: " + m2);
            output.validSection=(output.validSection && (m1*m2 > 0));
            if (output.validSection)
            {
                bool mPositive = (m1 > m2);
                bool deltaMPositive = (Math.Abs(m1) > Math.Abs(m2));

                //output.onTop= ((mPositive && output.goesRight)||(!mPositive && !output.goesRight));
                output.onTop= !(mPositive == output.goesRight);
                //output.rightSide= ((deltaMPositive && !output.goesRight)||( !deltaMPositive && output.goesRight));
                output.rightSide = deltaMPositive ^ output.goesRight;
                //Console.WriteLine("m+: " + mPositive + " deltaM+: " + deltaMPositive + " top: " + output.onTop + " right: " + output.rightSide);
            }


            return output;
        }
        struct Ksection
        {
            public bool validSection;
            public bool goesRight;
            public bool rightSide;
            public Boolean onTop;
            public int startLoc;
            public int endLoc;
        }

        /*private List<Point> Kasey2(List<Point> input)
        {
            List<Point> output=new List<Point>();
            Stack<Ksection> Ksections = new Stack<Ksection>();
            for (int i = 2; i < input.Count; i++)
            {
                Ksection one, two;
                Point a, b, c;
                a = input[i - 2];
                b = input[i - 1];
                c = input[i];

                one = getSection(a, b, c);
                one.startLoc = i - 2;
                one.endLoc = i;
                if (Ksections.Count > 0)
                { 
                    two = Ksections.Pop();

                    bool bothNegative = (!one.validSection && !two.validSection);

                    Ksection three = combine(one, two, input);

                    if(bothNegative || three.validSection){
                        Ksections.Push(three);
                    }
                    else
                    {
                        Ksections.Push(one);
                        Ksections.Push(two);
                    }


                }
                else
                {
                    Ksections.Push(one);
                }
            }

            //go through stack, test for ABA patterns
            return output;
        }

        private Ksection combine(Ksection a, Ksection b, List<Point> theList)
        {
            Ksection output = new Ksection();
            Point middlePoint;
            if (a.startLoc < b.startLoc)
            {
                output.startLoc = a.startLoc;
                middlePoint= avgPoint(a.endLoc, b.startLoc, theList);
                output.endLoc= b.endLoc;
            }
            else
            {
                output.startLoc = b.startLoc;
                middlePoint= avgPoint(b.endLoc, a.startLoc, theList);
                output.endLoc= a.endLoc;
            }

            output.validSection= getSection(theList[output.startLoc], middlePoint, theList[output.endLoc]).validSection;

            if(output.validSection){
                middlePoint= avgPoint(output.startLoc+1, output.endLoc -1, theList);
                Ksection temp= getSection(theList[output.startLoc], middlePoint, theList[output.endLoc]);

            }
            return new Ksection();
        }

        private Point avgPoint(int startLoc, int endLoc, List<Point>){
            return new Point();
        }*/

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

        private List<Point> Dominique2(List<Point> input)
        {
            List<Point> output= new List<Point>();
            int sLoc = 0;
            output.Add(input[0]);            
            for (int i = 0; i < (input.Count-1); i++)
            {
                bool sameSlope = ((xChange(input[sLoc], input[sLoc + 1]) == xChange(input[i], input[i+1])) && (yChange(input[sLoc], input[sLoc + 1]) == yChange(input[i], input[i+1])));
                if (!sameSlope)
                {
                    output.Add(input[i]);
                    sLoc = i;
                }
            }
            output.Add(input[input.Count - 1]);
            return output;
        }

        private bool xChange(Point a, Point b)
        {
            return (a.X >= b.X);
        }

        private bool yChange(Point a, Point b)
        {
            return (a.Y >= b.Y);
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
