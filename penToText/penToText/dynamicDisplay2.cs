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

namespace penToText
{
    public class dynamicDisplay2
    {

        private delegate void drawingDelegate();

        private Grid flexibleGrid;
        public Core core;
        private int x_max;
        private int y_max;
        private List<dynamicView2> children;

        public dynamicDisplay2(Core parent)
        {
            core = parent;
            flexibleGrid = new Grid();
            flexibleGrid.Height = System.Windows.SystemParameters.WorkArea.Height;
            flexibleGrid.Width = System.Windows.SystemParameters.WorkArea.Width;
            children = new List<dynamicView2>();
        }

        public Panel getContent()
        {
            return flexibleGrid;
        }

        public void updateDisplay()
        {
            if (core.getWindowStarted())
            {
                //start from scratch
                flexibleGrid.Children.Clear();
                flexibleGrid.ColumnDefinitions.Clear();
                flexibleGrid.RowDefinitions.Clear();
                // double columnWidth = core.getWindow().ActualWidth;
                //double rowHeight = core.getWindow().ActualHeight;

                double columnWidth = flexibleGrid.ActualWidth - 100;
                double rowHeight = flexibleGrid.ActualHeight - 100;

                columnWidth /= x_max;
                rowHeight /= y_max;

                double size = rowHeight;
                if (columnWidth < size)
                {
                    size = columnWidth;
                }

                for (int x = 0; x <= x_max; x++)
                {
                    ColumnDefinition newColumn = new ColumnDefinition();
                    newColumn.Width = new GridLength(size);
                    flexibleGrid.ColumnDefinitions.Add(newColumn);
                }

                for (int y = 0; y <= y_max; y++)
                {
                    RowDefinition newRow = new RowDefinition();
                    newRow.Height = new GridLength(size);
                    flexibleGrid.RowDefinitions.Add(newRow);
                }

                if (children.Count != 0)
                {
                    foreach (dynamicView2 current in children)
                    {
                        if (current.active)
                        {
                            Grid.SetColumn(current.myFrame, current.xPos);
                            Grid.SetColumnSpan(current.myFrame, current.xSize);
                            Grid.SetRow(current.myFrame, current.yPos);
                            Grid.SetRowSpan(current.myFrame, current.ySize);

                            flexibleGrid.Children.Add(current.myFrame);
                        }
                    }
                }
            }
        }

        public void canvasLocChanged()
        {
            x_max = 0;
            y_max = 0;
            foreach (dynamicView2 canvas in children)
            {
                if (canvas.xPos + canvas.xSize > x_max)
                {
                    x_max = canvas.xPos + canvas.xSize;
                }
                if (canvas.yPos + canvas.ySize > y_max)
                {
                    y_max = canvas.yPos + canvas.ySize;
                }
            }
            updateDisplay();
        }

        public void addCanvas(dynamicView2 view)
        {
            children.Add(view);
            canvasLocChanged();
        }

        public void clear()
        {
            for (int i = 0; i < children.Count; i++)
            {
                children[i].clear();
            }
        }

        public void draw()
        {
            core.getWindow().Dispatcher.BeginInvoke(new drawingDelegate(() =>
            {
                for (int i = 0; i < children.Count; i++)
                {
                    children[i].draw();
                }
            }));
        }
    }

    public abstract class dynamicView2
    {
        public Frame myFrame;
        public int xPos;
        public int yPos;
        public int xSize;
        public int ySize;
        protected dynamicDisplay2 parent;
        public bool active;
        public bool reserveSpace;

        public void changeLoc(int xPos, int yPos)
        {
            this.xPos = xPos;
            this.yPos = yPos;
            parent.canvasLocChanged();
        }

        public void changeWeight(int xSize, int ySize)
        {
            this.xSize = xSize;
            this.ySize = ySize;
            parent.canvasLocChanged();
        }

        abstract public void clear();

        abstract public void draw();

    }

    public class inputView : dynamicView2
    {
        Polyline myLine;
        long pauseLength;
        Stopwatch timer;
        int currentLine;
        private bool first;
        Canvas drawCanvas;
        private Point last;
        public inputView(int xPos, int yPos, int xSize, int ySize, dynamicDisplay2 parent, long pause, bool reserved)
        {
            currentLine = 0;
            first = true;
            pauseLength = pause;
            timer = new Stopwatch();
            this.reserveSpace = reserved;
            this.active = true;
            this.xPos = xPos;
            this.yPos = yPos;
            this.xSize = xSize;
            this.ySize = ySize;
            this.parent = parent;

            Border currentBorder = new Border();
            currentBorder.BorderBrush = Brushes.Black;
            currentBorder.BorderThickness = new Thickness(1);

            drawCanvas = new Canvas();

            drawCanvas.AddHandler(Canvas.MouseDownEvent, new MouseButtonEventHandler(startMouseDraw));
            drawCanvas.AddHandler(Canvas.StylusDownEvent, new StylusDownEventHandler(startStylusDraw));

            drawCanvas.AddHandler(Canvas.MouseMoveEvent, new MouseEventHandler(mouseMoveDraw));
            drawCanvas.AddHandler(Canvas.StylusMoveEvent, new StylusEventHandler(stylusMoveDraw));

            drawCanvas.AddHandler(Canvas.MouseUpEvent, new MouseButtonEventHandler(endMouseDraw));
            drawCanvas.AddHandler(Canvas.StylusUpEvent, new StylusEventHandler(endStylusDraw));
            drawCanvas.Background = Brushes.White;

            currentBorder.Child = drawCanvas;
            myFrame = new Frame();
            myFrame.Content = currentBorder;
        }

        public override void clear()
        {
            currentLine = 0;
            first = true;
            drawCanvas.Children.Clear();
        }

        public override void draw()
        {

        }

        private void sendData(Point newPoint)
        {
            if (last != null)
            {
                if (!withinTolerance(newPoint, last, .005))
                {
                    myLine.Points.Add(newPoint);
                    parent.core.addData(new mPoint(newPoint, currentLine));
                    last = newPoint;
                }
            }
            else
            {
                last = newPoint;
            }
        }

        private bool withinTolerance(Point a, Point b, double percentTolerance)
        {
            double xTolerance = xSize * percentTolerance;
            double yTolerance = ySize * percentTolerance;

            return (Math.Abs(a.X - b.X) <= xTolerance && Math.Abs(a.Y - b.Y) <= yTolerance);
        }

        private void startMouseDraw(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                timer.Stop();
                if (timer.ElapsedMilliseconds > pauseLength || first)
                {

                    if (!first)
                    {
                        currentLine++;
                    }
                    first = false;
                    myLine = new Polyline();
                    myLine.Stroke = System.Windows.Media.Brushes.Black;
                    myLine.StrokeThickness = 2;
                    drawCanvas.Children.Add(myLine);
                }
                timer.Reset();
                Point position = e.GetPosition(parent.core.getWindow());
                sendData(position);
                /*
                parent.core.addData(new mPoint(position, currentLine));
                myLine.Points.Add(position);*/
            }
        }

        private void startStylusDraw(object sender, StylusEventArgs e)
        {
            timer.Stop();
            if (timer.ElapsedMilliseconds > pauseLength || first)
            {

                if (!first)
                {
                    currentLine++;
                }
                first = false;
                myLine = new Polyline();
                myLine.Stroke = System.Windows.Media.Brushes.Black;
                myLine.StrokeThickness = 2;
                drawCanvas.Children.Add(myLine);
            }
            timer.Reset();
            Point position = e.GetPosition(parent.core.getWindow());
            sendData(position);
            /*
            parent.core.addData(new mPoint(position, currentLine));
            myLine.Points.Add(position);*/

            e.Handled = true;
        }

        private void mouseMoveDraw(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.StylusDevice == null)
            {

                Point position = e.GetPosition(parent.core.getWindow());
                sendData(position);
                /*
                parent.core.addData(new mPoint(position, currentLine));
                myLine.Points.Add(position);*/

            }
        }

        private void stylusMoveDraw(object sender, StylusEventArgs e)
        {

            Point position = e.GetPosition(parent.core.getWindow());
            sendData(position);
            /*
            parent.core.addData(new mPoint(position, currentLine));
            myLine.Points.Add(position);*/

            e.Handled = true;
        }

        private void endMouseDraw(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                Point position = e.GetPosition(parent.core.getWindow());
                sendData(position);
                /*
                parent.core.addData(new mPoint(position, currentLine));*/
                timer.Start();
            }
        }
        private void endStylusDraw(object sender, StylusEventArgs e)
        {
            Point position = e.GetPosition(parent.core.getWindow());
            sendData(position);
            /*
            parent.core.addData(new mPoint(position, currentLine));*/
            timer.Start();
            e.Handled = true;
        }
    }

    public class multiLineDrawView : dynamicView2
    {
        private List<mPoint> data;
        public TextBox title;
        public Canvas drawCanvas;
        public string titleText;
        public bool toAddCircles;
        public double padding = .1;
        public double outOf = 1.2;
        //public double outOfy = 1.2;
        public multiLineDrawView(int xPos, int yPos, int xSize, int ySize, dynamicDisplay2 parent, string titleText, bool reserved)
        {
            toAddCircles = false;
            this.reserveSpace = reserved;
            this.active = true;
            this.xPos = xPos;
            this.yPos = yPos;
            this.xSize = xSize;
            this.ySize = ySize;
            this.parent = parent;

            data = new List<mPoint>();

            myFrame = new Frame();
            Border currentBorder = new Border();
            DockPanel dock = new DockPanel();
            currentBorder.BorderBrush = Brushes.Black;
            currentBorder.BorderThickness = new Thickness(1);
            drawCanvas = new Canvas();

            myFrame.Content = currentBorder;
            currentBorder.Child = dock;


            title = new TextBox();
            title.Text = titleText;
            DockPanel.SetDock(title, Dock.Top);
            dock.Children.Add(title);
            dock.Children.Add(drawCanvas);

        }

        public void newData(List<mPoint> newData)
        {
            data = new List<mPoint>();

            if ((newData != null) && newData.Count > 1)
            {
                double Scale = drawCanvas.ActualHeight / outOf;
                double leftPad = (Math.Abs(drawCanvas.ActualWidth - drawCanvas.ActualHeight)) / 2.0;

                for (int i = 0; i < newData.Count; i++)
                {
                    double newX = newData[i].X;
                    double newY = newData[i].Y;

                    newX += padding;
                    newY += padding;

                    newX *= Scale;
                    newY *= Scale;
                    data.Add(new mPoint(newX, newY, newData[i].line));
                }
            }

        }
        public List<mPoint> getData() { return data; }

        public override void clear()
        {
            drawCanvas.Children.Clear();
        }

        public override void draw()
        {
            List<mPoint> toDraw = new List<mPoint>(data);
            double radius = 4;
            List<Polyline> lines = new List<Polyline>();
            List<Ellipse> circles = new List<Ellipse>();

            title.Text = titleText;
            drawCanvas.Children.Clear();

            for (int i = 0; i < toDraw.Count; i++)
            {
                while (lines.Count <= toDraw[i].line)
                {
                    Polyline myLine = new Polyline();
                    myLine.Stroke = Brushes.Black;
                    myLine.StrokeThickness = 2;
                    drawCanvas.Children.Add(myLine);
                    lines.Add(myLine);

                }
                if (toAddCircles)
                {
                    drawCircle(toDraw[i].X, toDraw[i].Y, radius);
                }
                lines[toDraw[i].line].Points.Add(toDraw[i].getPoint());
            }
        }



        public void drawCircle(double centerX, double centerY, double r)
        {
            Ellipse circle = new Ellipse();
            circle.Width = r;
            circle.Height = r;
            circle.Stroke = Brushes.Black;
            circle.StrokeThickness = 2;
            double left = centerX - (r / 2.0);
            double top = centerY - (r / 2.0);
            Canvas.SetLeft(circle, left);
            Canvas.SetTop(circle, top);
            drawCanvas.Children.Add(circle);
        }
    }

    public class charGuessView : dynamicView2
    {
        TextBlock[] charGuesses;
        StackPanel container;
        String[] guessStrings;
        int num_guessers;

        public charGuessView(int xPos, int yPos, int xSize, int ySize, dynamicDisplay2 parent, bool reserved, int guessers)
        {
            this.reserveSpace = reserved;
            this.active = true;
            this.xPos = xPos;
            this.yPos = yPos;
            this.xSize = xSize;
            this.ySize = ySize;
            this.parent = parent;

            num_guessers = guessers;
            charGuesses = new TextBlock[num_guessers];
            guessStrings = new String[num_guessers];

            myFrame = new Frame();
            Border currentBorder = new Border();
            currentBorder.BorderBrush = Brushes.Black;
            currentBorder.BorderThickness = new Thickness(1);

            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            container = new StackPanel();

            myFrame.Content = currentBorder;
            currentBorder.Child = scroll;
            scroll.Content = container;

            for (int i = 0; i < num_guessers; i++)
            {
                Border childBorder = new Border();
                childBorder.BorderBrush = Brushes.Black;
                childBorder.BorderThickness = new Thickness(1);
                Thickness margin = childBorder.Margin;
                margin.Top = 10;
                childBorder.Margin = margin;
                charGuesses[i] = new TextBlock();
                childBorder.Child = charGuesses[i];
                container.Children.Add(childBorder);
            }
        }

        public void updateGuess(int ID, String newText)
        {
            if (ID >= 0 && ID < num_guessers)
            {
                guessStrings[ID] = newText;
            }
        }

        public override void clear()
        {

        }

        public override void draw()
        {
            for (int i = 0; i < num_guessers; i++)
            {
                charGuesses[i].Text = guessStrings[i];
            }
        }
    }

    public class charInputView : dynamicView2
    {
        TextBox input;
        public charInputView(int xPos, int yPos, int xSize, int ySize, dynamicDisplay2 parent, bool reserved)
        {
            this.reserveSpace = reserved;
            this.active = true;
            this.xPos = xPos;
            this.yPos = yPos;
            this.xSize = xSize;
            this.ySize = ySize;
            this.parent = parent;

            myFrame = new Frame();
            Border currentBorder = new Border();
            currentBorder.BorderBrush = Brushes.Black;
            currentBorder.BorderThickness = new Thickness(1);

            int numViews = 3;

            Grid gridContainer = new Grid();
            ColumnDefinition singleColumn = new ColumnDefinition();
            singleColumn.Width = new GridLength(1, GridUnitType.Star);
            gridContainer.ColumnDefinitions.Add(singleColumn);
            for (int i = 0; i < numViews; i++)
            {
                RowDefinition thisRow = new RowDefinition();
                thisRow.Height = new GridLength(1, GridUnitType.Star);
                gridContainer.RowDefinitions.Add(thisRow);
            }

            myFrame.Content = currentBorder;
            currentBorder.Child = gridContainer;

            TextBlock prompt = new TextBlock();
            prompt.HorizontalAlignment = HorizontalAlignment.Stretch;
            prompt.VerticalAlignment = VerticalAlignment.Stretch;
            prompt.Text = "Submit letter";
            Grid.SetColumn(prompt, 0);
            Grid.SetRow(prompt, 0);
            gridContainer.Children.Add(prompt);


            Grid middleView = new Grid();
            RowDefinition singleRow = new RowDefinition();
            singleRow.Height = new GridLength(1, GridUnitType.Star);
            middleView.RowDefinitions.Add(singleRow);
            for (int i = 0; i < 3; i++)
            {
                ColumnDefinition thisColumn = new ColumnDefinition();
                thisColumn.Width = new GridLength(1, GridUnitType.Star);
                middleView.ColumnDefinitions.Add(thisColumn);
            }

            Button cycleLeft = new Button();
            cycleLeft.Content = "<";
            cycleLeft.Click += new RoutedEventHandler(cycle_Left);
            Grid.SetColumn(cycleLeft, 0);
            Grid.SetRow(cycleLeft, 0);
            middleView.Children.Add(cycleLeft);

            Button cycleRight = new Button();
            cycleRight.Content = "<";
            cycleRight.Click += new RoutedEventHandler(cycle_Right);
            Grid.SetColumn(cycleRight, 2);
            Grid.SetRow(cycleRight, 0);
            middleView.Children.Add(cycleRight);

            input = new TextBox();
            input.MaxLength = 1;
            input.KeyDown += new KeyEventHandler(OnKeyDownHandler);
            Grid.SetColumn(input, 1);
            Grid.SetRow(input, 0);
            middleView.Children.Add(input);


            Grid.SetColumn(middleView, 0);
            Grid.SetRow(middleView, 1);
            gridContainer.Children.Add(middleView);

            Button submit = new Button();
            submit.Content = "Submit";
            submit.Click += new RoutedEventHandler(Submit_Click);
            Grid.SetColumn(submit, 0);
            Grid.SetRow(submit, 2);
            gridContainer.Children.Add(submit);
        }

        public void Submit_Click(object sender, RoutedEventArgs e)
        {
            submit();
        }

        public void cycle_Left(object sender, RoutedEventArgs e)
        {
            cycleChar(true);
        }

        public void cycle_Right(object sender, RoutedEventArgs e)
        {
            cycleChar(false);
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                submit();
            }
            else if (e.Key == Key.Left)
            {
                cycleChar(true);
            }
            else if (e.Key == Key.Right)
            {
                cycleChar(false);
            }
        }

        public void submit()
        {
            if (input.Text.Length > 0)
            {
                char toSend = input.Text.ToCharArray()[0];
                parent.core.submitData(toSend);
            }
        }

        public void cycleChar(bool isLeft)
        {
            int currentLoc = 0;
            if (input.Text.Length > 0)
            {
                currentLoc = Array.IndexOf(parent.core.alphabet, input.Text.ToArray()[0]);
                if (isLeft)
                {
                    currentLoc--;
                }
                else
                {
                    currentLoc++;
                }
            }
            currentLoc = currentLoc % parent.core.alphabet.Length;
            if (currentLoc < 0) { currentLoc += parent.core.alphabet.Length; }
            input.Text = "" + (parent.core.alphabet[currentLoc]);
        }

        public override void clear()
        {

        }

        public override void draw()
        {

        }

    }

    public class treeView : dynamicView2
    {
        dataNode root;
        bool updated;
        TreeView mainView;

        public treeView(int xPos, int yPos, int xSize, int ySize, dynamicDisplay2 parent, bool reserved, dataNode root)
        {
            this.reserveSpace = reserved;
            this.active = true;
            this.xPos = xPos;
            this.yPos = yPos;
            this.xSize = xSize;
            this.ySize = ySize;
            this.parent = parent;
            this.root = root;
            updated = true;

            myFrame = new Frame();
            Border currentBorder = new Border();
            currentBorder.BorderBrush = Brushes.Black;
            currentBorder.BorderThickness = new Thickness(1);

            ScrollViewer scroll = new ScrollViewer();
            scroll.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            mainView = new TreeView();

            myFrame.Content = currentBorder;
            currentBorder.Child = scroll;
            scroll.Content = mainView;
        }

        public void newTree(dataNode root)
        {
            this.root = root;
            updated = true;
        }

        public override void clear()
        {
        }

        public override void draw()
        {
            //build the tree here
            if (updated)
            {
                mainView.Items.Clear();
                //build the tree if there is a new one, it is stored at mainView
                for (int i = 0; i < root.children.Count; i++)
                {
                    mainView.Items.Add(getView(root.children[i]));
                }
            }
            updated = false;
        }

        private TreeViewItem getView(dataNode node)
        {
            TreeViewItem output = new TreeViewItem();
            String header = node.SectionLetter + " [ " + node.minValue + " , " + node.maxValue + " ] " +
                "\nPossible Letters: " + node.chars.Replace(" ", "");
            if (node.ifStopHere != ' ') { header += "\nIf stop here: " + node.ifStopHere; }
            output.Header = header;

            for (int i = 0; i < node.children.Count; i++)
            {
                output.Items.Add(getView(node.children[i]));
            }

            return output;
        }


    }
}
