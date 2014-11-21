using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace penToText
{
    public abstract class textConverter
    {
        protected Core core;
        protected dataTree myTree;
        public int resampleID = -1;
        public int sectionsID = -1;
        public int guessOutputID = -1;
        protected TimeSpan[] elapsedTime;
        protected static TimeSpan Time(Action action)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }
        /*
        public void getData(BlockingCollection<mPoint> data)
        {
            mPoint last = null;
            foreach (var item in data.GetConsumingEnumerable())
            {
                mPoint current = item;

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
        }*/

        public void setTree(dataTree thisTree)
        {
            myTree = thisTree;
        }
        abstract public void updateData(mPoint newPoint);

        abstract public void clear();

        abstract public List<mPoint> getCleanedData();

        abstract public List<Tuple<String, int>> getSectionBreakDown(List<mPoint> input);

        protected double distance(mPoint a, mPoint b)
        {
            double output = 0;
            if (a.line == b.line)
            {
                output = Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
            }
            return output;
        }

        protected static Double RoundToNearest(Double passednumber, Double roundto)
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

    public class kaseyTextConverter : textConverter
    {
        private String TAG = "Kasey";
        private List<mPoint> scaling;
        private List<mPoint> resampling;

        int timeCounts = 2;
        /*
         * 0 is rescaling
         * 1 is the other thing
         */

        public kaseyTextConverter(Core core)
        {
            this.core = core;
            resetTimes();
        }
        public kaseyTextConverter(Core core, int resampleID, int sectionsID, int guessOutputID)
        {
            this.core = core;
            this.resampleID = resampleID;
            this.sectionsID = sectionsID;
            this.guessOutputID = guessOutputID;
            resetTimes();
        }

        public override void clear()
        {
            //originalData.Clear();
            scaling.Clear();
            resampling.Clear();
            resetTimes();
        }

        public override void updateData(mPoint newPoint)
        {
            double minDistance = .1;
            int timeId = 0;
            /*
             * 0 is rescaling
             * 1 is the other thing
             */

            //add new point
            elapsedTime[timeId] += Time(() =>
            {
                addingToSampled(newPoint, minDistance);
            });
            //display the resampled data
            if (resampleID >= 0)
            {
                core.TextBreakDown[resampleID].newData(new List<mPoint>(resampling));
                core.TextBreakDown[resampleID].titleText = TAG + " Resample: " + core.originalData.Count + "\nTo: " + resampling.Count + "\nTime: " + (elapsedTime[timeId].TotalMilliseconds);
            }

            //Kasey Unique Stuff
            //sections, this has long math for drawing
            timeId = 1;
            List<mPoint> cleaned2 = new List<mPoint>();
            elapsedTime[timeId] += Time(() =>
            {
                cleaned2 = getSections(new List<mPoint>(resampling));
            });
            if (sectionsID >= 0)
            {
                core.TextBreakDown[sectionsID].newData(new List<mPoint>(cleaned2));
                core.TextBreakDown[sectionsID].titleText = "Kasey Sections: " + core.originalData.Count + "\nTo: " + cleaned2.Count + "\nTime: " + (elapsedTime[timeId]).TotalMilliseconds;
            }
        }

        public override List<Tuple<String, int>> getSectionBreakDown(List<mPoint> input)
        {
            return null;
        }

        public override List<mPoint> getCleanedData()
        {
            return new List<mPoint>(resampling);
        }

        private void resetTimes()
        {
            if (elapsedTime == null || elapsedTime.Length != timeCounts)
            {
                elapsedTime = new TimeSpan[timeCounts];
            }
            for (int i = 0; i < timeCounts; i++)
            {
                elapsedTime[i] = TimeSpan.Zero;
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

            mPoint newPoint = new mPoint((adding.X - minX) / scale, (adding.Y - minY) / scale, adding.line);

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

        private void addingToSampled(mPoint adding, double minDistance)
        {

            if (scaling == null)
            {
                scaling = new List<mPoint>();
            }
            if (resampling == null)
            {
                resampling = new List<mPoint>();
            }

            bool changed = addToScale(adding, scaling);

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
                    if (b.line != c.line || a.line != b.line)
                    {
                        output.Add(b);
                    }
                    else if (bigD > minDistance)
                    {
                        output.Add(b);
                    }
                    else if (bigD + lilD > minDistance)
                    {
                        mPoint temp = new mPoint((b.X + ((minDistance - bigD) / lilD) * (c.X - b.X)),
                            (b.Y + ((minDistance - bigD) / lilD) * (c.Y - b.Y)),
                            c.line);
                        output.Add(temp);
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

        protected double distanceIgnoreLine(mPoint a, mPoint b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        public List<mPoint> getSections(List<mPoint> input)
        {
            //create list of points and their directions
            List<mPoint> points = new List<mPoint>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                double initialRound = .05;
                double unitScale = .01;
                List<double> lengths = new List<double>();
                List<int> directions = new List<int>();
                List<bool> sameLine = new List<bool>();
                double unitLength = 0;

                int sDir = getDirectionCircular(input[0], input[1]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = input[sLoc].line == input[i + 1].line && sDir == getDirectionIgnoreLine(input[i], input[i + 1]);
                    if (!sameSlope)
                    {
                        double length = distanceIgnoreLine(input[sLoc], input[i]);
                        if (unitLength == 0)
                        {
                            unitLength = RoundToNearest(length, initialRound);
                        }
                        else
                        {
                            length = RoundToNearest(length, unitScale * unitLength);
                        }
                        lengths.Add(length);
                        directions.Add(sDir);
                        sameLine.Add(input[sLoc].line == input[i].line);

                        sLoc = i;
                        sDir = getDirectionIgnoreLine(input[sLoc], input[sLoc + 1]);
                    }
                }


                double length2 = distanceIgnoreLine(input[sLoc], input[input.Count - 1]);
                if (unitLength == 0)
                {
                    unitLength = RoundToNearest(length2, initialRound);
                }
                else
                {
                    length2 = RoundToNearest(length2, unitScale * unitLength);
                }
                lengths.Add(length2);
                directions.Add(sDir);
                sameLine.Add(input[sLoc].line == input[input.Count - 1].line);

                int lineAt = 0;
                mPoint lastPoint = input[0];
                points.Add(lastPoint);

                for (int i = 0; i < lengths.Count; i++)
                {

                    double angle = degreesToRadians((directions[i] - 1) * 45.0);
                    double newX = (Math.Cos(angle) * lengths[i]) + lastPoint.X;
                    double newY = (Math.Sin(angle) * lengths[i]) + lastPoint.Y;

                    if (!sameLine[i]) { lineAt++; }

                    mPoint thisPoint = new mPoint(newX, newY, lineAt);

                    points.Add(thisPoint);
                    lastPoint = thisPoint;
                }
            }
            else
            {
                points = input;
            }
            return points;
        }

        public List<mPoint> getSections2(List<mPoint> input)
        {
            //create list of points and their directions
            List<mPoint> points = new List<mPoint>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                double initialRound = .05;
                double unitScale = .01;
                List<double> lengths = new List<double>();
                List<int> directions = new List<int>();
                List<mPoint> newLinePoints = new List<mPoint>();
                newLinePoints.Add(input[0]);
                double unitLength = 0;

                int sDir = getDirectionCircular(input[0], input[1]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = input[sLoc].line == input[i + 1].line && sDir == getDirectionCircular(input[i], input[i + 1]);
                    if (!sameSlope)
                    {
                        if (input[sLoc].line == input[i].line)
                        {
                            double length = distance(input[sLoc], input[i]);
                            if (unitLength == 0)
                            {
                                unitLength = RoundToNearest(length, initialRound);
                            }
                            else
                            {
                                length = RoundToNearest(length, unitScale * unitLength);
                            }
                            lengths.Add(length);
                            directions.Add(sDir);
                        }
                        else
                        {
                            //assume lenght of line would never be negative;
                            lengths.Add(-1);
                            directions.Add(-1);
                            newLinePoints.Add(input[i]);
                        }

                        sLoc = i;
                        sDir = getDirectionCircular(input[sLoc], input[sLoc + 1]);
                    }
                }


                double length2 = distance(input[sLoc], input[input.Count - 1]);
                if (unitLength == 0)
                {
                    unitLength = RoundToNearest(length2, initialRound);
                }
                else
                {
                    length2 = RoundToNearest(length2, unitScale * unitLength);
                }
                lengths.Add(length2);
                directions.Add(sDir);



                int lineAt = 0;
                mPoint lastPoint = newLinePoints[lineAt];
                points.Add(lastPoint);

                for (int i = 0; i < lengths.Count; i++)
                {
                    if (lengths[i] >= 0)
                    {
                        double angle = degreesToRadians((directions[i] - 1) * 45.0);
                        double newX = (Math.Cos(angle) * lengths[i]) + lastPoint.X;
                        double newY = (Math.Sin(angle) * lengths[i]) + lastPoint.Y;

                        mPoint thisPoint = new mPoint(newX, newY, lineAt);
                        points.Add(thisPoint);
                        lastPoint = thisPoint;
                    }
                    else
                    {
                        lineAt++;
                        lastPoint = newLinePoints[lineAt];
                        lastPoint.line = lineAt;
                        points.Add(lastPoint);
                    }
                }
            }
            else
            {
                points = input;
            }
            return points;
        }

        public int getDirectionCircular(mPoint a, mPoint b)
        {
            int output = 0;
            /*
             * 0: invalid
             * 1: right
             * 2: up-right
             * 3: up
             * 4: up-left
             * 5: left
             * 6: down-left
             * 7: down
             * 8: down-right
             */
            if (a.line == b.line)
            {
                //the vector values

                double X = b.X - a.X;
                double Y = b.Y - a.Y;
                output = smartFitTo45(X, Y);
            }
            return output;
        }

        public int getDirectionIgnoreLine(mPoint a, mPoint b)
        {
            /*
             * 0: invalid
             * 1: right
             * 2: up-right
             * 3: up
             * 4: up-left
             * 5: left
             * 6: down-left
             * 7: down
             * 8: down-right
             */
            //the vector values

            double X = b.X - a.X;
            double Y = b.Y - a.Y;
            return smartFitTo45(X, Y);
        }

        private double angleDifferent(double a, double b)
        {
            double difference = Math.Abs(a - b) % 360;

            if (difference > 180)
            {
                difference = 360 - difference;
            }

            return difference;
        }

        private int smartFitTo45(double X, double Y)
        {
            //get the angle of theline, accounting for quadrant, in degrees
            double degrees = ((Math.Atan2(Y, X) * (180.0 / Math.PI)) + 360) % 360;
            //round angle to a 45 degree angle
            int flatAngle = (int)RoundToNearest(degrees, 90.0) / 45;

            int slopeAngle = flatAngle;
            if (angleDifferent((flatAngle + 1) * 45.0, degrees) < angleDifferent((flatAngle - 1) * 45.0, degrees))
            {
                slopeAngle++;
            }
            else
            {
                slopeAngle--;
            }

            int newAngle = flatAngle;

            if (3 * angleDifferent(flatAngle * 45.0, degrees) > 2 * angleDifferent(slopeAngle * 45.0, degrees))
            {
                newAngle = slopeAngle;
            }

            return (newAngle % 8) + 1;
        }

        private double degreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

    }

    public class currentTextConverter : textConverter
    {
        private String TAG = "Current";
        private List<mPoint> scaling;
        private List<mPoint> resampling;
        private bool withLines = false;

        int timeCounts = 3;
        /*
         * 0 is rescaling
         * 1 is the other thing
         * 2 time to search the tree
         */

        public currentTextConverter(Core core)
        {
            this.core = core;
            resetTimes();
        }
        public currentTextConverter(Core core, int resampleID, int sectionsID, int guessOutputID)
        {
            this.core = core;
            this.resampleID = resampleID;
            this.sectionsID = sectionsID;
            this.guessOutputID = guessOutputID;
            resetTimes();
        }

        public override void updateData(mPoint newPoint)
        {
            double minDistance = .1;
            /*
             * 0 is rescaling
             * 1 is the other thing
             */

            //add new point
            elapsedTime[0] += Time(() =>
            {
                addingToSampled(newPoint, minDistance);
            });
            //display the resampled data
            if (resampleID >= 0)
            {
                core.TextBreakDown[resampleID].newData(new List<mPoint>(resampling));
                core.TextBreakDown[resampleID].titleText = TAG + " Resample: " + core.originalData.Count + "\nTo: " + resampling.Count + "\nTime: " + (elapsedTime[0].TotalMilliseconds);
            }

            //Current Unique Stuff

            //get sections
            List<mPoint> cleaned = new List<mPoint>();
            elapsedTime[1] += Time(() =>
            {
                cleaned = cleanSections(Sections(new List<mPoint>(resampling)));
            });
            if (sectionsID >= 0)
            {
                core.TextBreakDown[sectionsID].newData(new List<mPoint>(cleaned));
                core.TextBreakDown[sectionsID].titleText = TAG + " Sections: " + core.originalData.Count + "\nTo: " + cleaned.Count + "\nTotal Time: " + ((elapsedTime[0]).TotalMilliseconds + (elapsedTime[1]).TotalMilliseconds);
            }


            if (myTree != null && guessOutputID >= 0)
            {
                Tuple<String, String> results = new Tuple<string, string>("", "");
                elapsedTime[2] += Time(() =>
                {
                    results = myTree.searchTree(getBreakDown(cleaned));
                });
                core.guessOutput.updateGuess(guessOutputID,
                    TAG + " guess:" +
                    "\nPossible letters: " + results.Item1 +
                    "\nCurrent Guess: " + results.Item2 +
                    "\nTotal time: " + (elapsedTime[0].TotalMilliseconds + elapsedTime[1].TotalMilliseconds + elapsedTime[2].TotalMilliseconds));
            }
        }

        public override List<Tuple<String, int>> getSectionBreakDown(List<mPoint> input)
        {
            //This method is used to build the tree, assume input list is scaled and resampled, break it into sections and return those sections
            return getBreakDown(cleanSections(Sections(new List<mPoint>(input))));
        }

        public List<Tuple<String, int>> getBreakDown(List<mPoint> cleaned)
        {
            List<Tuple<string, int>> output = new List<Tuple<string, int>>();
            //This method is used with tree data, assume input list is broken down sections
            //No specific definition is required as long as it is concsistent within the textConverter
            //Slopes are 1 = 'A' and 2 = 'B' ... if its a line break use 'L'
            //Knowing this, The slope is always represented by 1 char
            //Line lenghts will be represented by 0-1000, with 100 being the unit length 
            //(was using 0-10 with 2 decimal places but this is same accuracy and would be easier to switch to a non-string memory-saving system for a small integer or whatnot)
            double unitLength = 0;
            for (int i = 1; i < cleaned.Count; i++)
            {
                double thisLength = distance(cleaned[i - 1], cleaned[i]);
                if (unitLength == 0) { unitLength = thisLength; }

                thisLength /= unitLength;
                //moves to in scale so 0-1 is 0-100, so stores in that is equally accurate to two decimal points
                int scaleLength = (int)(Math.Ceiling(thisLength * 100));

                //keeps the length in 4 didgets
                //scaleLength = Math.Min(scaleLength, 9999);

                String slopeString = "L";

                if (cleaned[i - 1].line == cleaned[i].line)
                {
                    //slopeString = "" + (char)('A' + getDirection(cleaned[i - 1], cleaned[i]));

                    //readable string system
                    slopeString = "";
                    int thisDirection = getDirection(cleaned[i - 1], cleaned[i]);
                    if (thisDirection == 1 || thisDirection == 5 || thisDirection == 6)
                    {
                        slopeString += "U";
                    }
                    else if (thisDirection == 2 || thisDirection == 7 || thisDirection == 8)
                    {
                        slopeString += "D";
                    }
                    else
                    {
                        slopeString += "N";
                    }

                    if (thisDirection > 2)
                    {
                        if (thisDirection % 2 == 1)
                        {
                            slopeString += "L";
                        }
                        else
                        {
                            slopeString += "R";
                        }
                    }
                    else
                    {
                        slopeString += "N";
                    }
                }

                Tuple<string, int> nextTuple = new Tuple<string, int>(slopeString, scaleLength);
                output.Add(nextTuple);
            }

            return output;
        }

        public override List<mPoint> getCleanedData()
        {
            return new List<mPoint>(resampling);
        }

        public override void clear()
        {
            //originalData.Clear();
            scaling.Clear();
            resampling.Clear();
            resetTimes();
        }

        private void resetTimes()
        {
            if (elapsedTime == null || elapsedTime.Length != timeCounts)
            {
                elapsedTime = new TimeSpan[timeCounts];
            }
            for (int i = 0; i < timeCounts; i++)
            {
                elapsedTime[i] = TimeSpan.Zero;
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

            mPoint newPoint = new mPoint((adding.X - minX) / scale, (adding.Y - minY) / scale, adding.line);

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

        private void addingToSampled(mPoint adding, double minDistance)
        {

            if (scaling == null)
            {
                scaling = new List<mPoint>();
            }
            if (resampling == null)
            {
                resampling = new List<mPoint>();
            }

            bool changed = addToScale(adding, scaling);

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
                    if (b.line != c.line || a.line != b.line)
                    {
                        output.Add(b);
                    }
                    else if (bigD > minDistance)
                    {
                        output.Add(b);
                    }
                    else if (bigD + lilD > minDistance)
                    {
                        mPoint temp = new mPoint((b.X + ((minDistance - bigD) / lilD) * (c.X - b.X)),
                            (b.Y + ((minDistance - bigD) / lilD) * (c.Y - b.Y)),
                            c.line);
                        output.Add(temp);
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

        public List<mPoint> Sections(List<mPoint> input)
        {
            for (int i = 0; i < input.Count; i++)
            {

                mPoint temp = roundedPoint(input[i], .05);
                if (i == 0 || !temp.Equals(input[i - 1]))
                {
                    input[i] = temp;
                }
            }
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

        private mPoint roundedPoint(mPoint input, double toRound)
        {
            return new mPoint(RoundToNearest(input.X, toRound), RoundToNearest(input.Y, toRound), input.line);
        }

        public int getDirection(mPoint a, mPoint b)
        {
            int output = 0;
            /*
             * 0: invalid
             * 1: up
             * 2: down
             * 3: left
             * 4: right
             * 5: up-left
             * 6: up-right
             * 7: down-left
             * 8: down-right
             */
            if (a.X == b.X && withLines)
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
                if (a.Y == b.Y && withLines)
                {
                    //no Y change
                    output = 4;
                }
                else if (a.Y >= b.Y)
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
            else if (a.X >= b.X)
            {
                //going left
                if (a.Y == b.Y && withLines)
                {
                    //no Y change
                    output = 3;
                }
                else if (a.Y >= b.Y)
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

        private double lineDistance(mPoint a, mPoint b, mPoint c)
        {
            //a and b make line, return distance c from line
            double uX = b.X - a.X;
            double uY = b.Y - a.Y;
            double vX = c.X - a.X;
            double vY = c.Y - a.Y;
            double val = ((uX * vX) + (uY * vY)) / ((uX * uX) + (uY * uY));
            mPoint other = new mPoint(uX * val + a.X, uY * val + a.Y, c.line);
            double output = double.PositiveInfinity;
            if (val >= 0.0 && val <= 1.0)
            {
                output = distance(c, other);
            }
            return output;
        }
    }

    public class dominiqueTextConverter : textConverter
    {
        private String TAG = "Dominique";
        private List<mPoint> scaling;
        private List<mPoint> resampling;

        int timeCounts = 2;
        /*
         * 0 is rescaling
         * 1 is the other thing
         */

        public dominiqueTextConverter(Core core)
        {
            this.core = core;
            resetTimes();
        }
        public dominiqueTextConverter(Core core, int resampleID, int sectionsID, int guessOutputID)
        {
            this.core = core;
            this.resampleID = resampleID;
            this.sectionsID = sectionsID;
            this.guessOutputID = guessOutputID;
            resetTimes();
        }

        public override void updateData(mPoint nextPoint)
        {
            double minDistance = .1;
            int timeId = 0;
            /*
             * 0 is rescaling
             * 1 is the other thing
             */

            //add new point
            elapsedTime[timeId] += Time(() =>
            {
                addingToSampled(nextPoint, minDistance);
            });
            //display the resampled data
            if (resampleID >= 0)
            {
                core.TextBreakDown[resampleID].newData(new List<mPoint>(resampling));
                core.TextBreakDown[resampleID].titleText = TAG + " Resample: " + core.originalData.Count + "\nTo: " + resampling.Count + "\nTime: " + (elapsedTime[timeId].TotalMilliseconds);
            }

            //Current Unique Stuff

            //get sections
            timeId = 1;
            List<mPoint> cleaned = new List<mPoint>();
            elapsedTime[timeId] += Time(() =>
            {
                cleaned = Sections(new List<mPoint>(resampling));
            });
            if (sectionsID >= 0)
            {
                core.TextBreakDown[sectionsID].newData(new List<mPoint>(cleaned));
                core.TextBreakDown[sectionsID].titleText = TAG + " Sections: " + core.originalData.Count + "\nTo: " + cleaned.Count + "\nTime: " + (elapsedTime[timeId]).TotalMilliseconds;
            }
        }

        public override List<mPoint> getCleanedData()
        {
            return new List<mPoint>(resampling);
        }

        public override List<Tuple<String, int>> getSectionBreakDown(List<mPoint> input)
        {
            return null;
        }

        public override void clear()
        {
            //originalData.Clear();
            scaling.Clear();
            resampling.Clear();
            resetTimes();
        }

        private void resetTimes()
        {
            if (elapsedTime == null || elapsedTime.Length != timeCounts)
            {
                elapsedTime = new TimeSpan[timeCounts];
            }
            for (int i = 0; i < timeCounts; i++)
            {
                elapsedTime[i] = TimeSpan.Zero;
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

            mPoint newPoint = new mPoint((adding.X - minX) / scale, (adding.Y - minY) / scale, adding.line);

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

        private void addingToSampled(mPoint adding, double minDistance)
        {

            if (scaling == null)
            {
                scaling = new List<mPoint>();
            }
            if (resampling == null)
            {
                resampling = new List<mPoint>();
            }

            bool changed = addToScale(adding, scaling);

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
                    if (b.line != c.line || a.line != b.line)
                    {
                        output.Add(b);
                    }
                    else if (bigD > minDistance)
                    {
                        output.Add(b);
                    }
                    else if (bigD + lilD > minDistance)
                    {
                        mPoint temp = new mPoint((b.X + ((minDistance - bigD) / lilD) * (c.X - b.X)),
                            (b.Y + ((minDistance - bigD) / lilD) * (c.Y - b.Y)),
                            c.line);
                        output.Add(temp);
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

        public List<mPoint> Sections(List<mPoint> input)
        {
            for (int i = 0; i < input.Count; i++)
            {

                mPoint temp = roundedPoint(input[i], .01);
                if (i == 0 || !temp.Equals(input[i - 1]))
                {
                    input[i] = temp;
                }
            }
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

        private mPoint roundedPoint(mPoint input, double toRound)
        {
            return new mPoint(RoundToNearest(input.X, toRound), RoundToNearest(input.Y, toRound), input.line);
        }

        public int getDirection(mPoint a, mPoint b)
        {
            int output = 0;
            /*
             * 0: invalid
             * 1: right
             * 2: up-right
             * 3: up
             * 4: up-left
             * 5: left
             * 6: down-left
             * 7: down
             * 8: down-right
             */
            if (a.line == b.line)
            {
                //the vector values

                double X = b.X - a.X;
                double Y = b.Y - a.Y;
                output = smartFitTo45(X, Y);
            }
            return output;
        }

        private double angleDifferent(double a, double b)
        {
            double difference = Math.Abs(a - b) % 360;

            if (difference > 180)
            {
                difference = 360 - difference;
            }

            return difference;
        }

        private int smartFitTo45(double X, double Y)
        {
            //get the angle of theline, accounting for quadrant, in degrees
            double degrees = ((Math.Atan2(Y, X) * (180.0 / Math.PI)) + 360) % 360;
            //round angle to a 45 degree angle
            int flatAngle = (int)RoundToNearest(degrees, 90.0) / 45;

            int slopeAngle = flatAngle;
            if (angleDifferent((flatAngle + 1) * 45.0, degrees) < angleDifferent((flatAngle - 1) * 45.0, degrees))
            {
                slopeAngle++;
            }
            else
            {
                slopeAngle--;
            }

            int newAngle = flatAngle;

            if (3 * angleDifferent(flatAngle * 45.0, degrees) > 2 * angleDifferent(slopeAngle * 45.0, degrees))
            {
                newAngle = slopeAngle;
            }

            return (newAngle % 8) + 1;
        }

        private double degreesToRadians(double degrees)
        {
            return Math.PI * degrees / 180.0;
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

        private double lineDistance(mPoint a, mPoint b, mPoint c)
        {
            //a and b make line, return distance c from line
            double uX = b.X - a.X;
            double uY = b.Y - a.Y;
            double vX = c.X - a.X;
            double vY = c.Y - a.Y;
            double val = ((uX * vX) + (uY * vY)) / ((uX * uX) + (uY * uY));
            mPoint other = new mPoint(uX * val + a.X, uY * val + a.Y, c.line);
            double output = double.PositiveInfinity;
            if (val >= 0.0 && val <= 1.0)
            {
                output = distance(c, other);
            }
            return output;
        }
    }
}
