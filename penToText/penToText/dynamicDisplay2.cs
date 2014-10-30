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

        public Panel getContent(){
            return flexibleGrid;
        }

        public void updateDisplay()
        {
            //start from scratch
            flexibleGrid.Children.Clear();
            flexibleGrid.ColumnDefinitions.Clear();
            flexibleGrid.RowDefinitions.Clear();

            double xTotal = 0.0;
            double yTotal = 0.0;
            for (int x = 0; x <= x_max; x++)
            {
                double bestX = 0.0;
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].xPos == x && children[i].xSize > bestX && (children[i].active || children[i].reserveSpace))
                    {
                        bestX = children[i].xSize;
                    }
                }
                xTotal += bestX;
            }

            for (int y = 0; y <= y_max; y++)
            {
                double bestY = 0.0;
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].yPos == y && children[i].ySize > bestY && (children[i].active || children[i].reserveSpace))
                    {
                        bestY = children[i].ySize;
                    }
                }
                yTotal += bestY;
            }

            double columnWidth = System.Windows.SystemParameters.WorkArea.Width / xTotal;
            double rowHeight = System.Windows.SystemParameters.WorkArea.Height / yTotal;

            double size = columnWidth;
            if (size > rowHeight)
            {
                size = rowHeight;
            }

            for (int x = 0; x <= xTotal; x++)
            {
                ColumnDefinition newColumn = new ColumnDefinition();
                newColumn.Width = new GridLength(size);
                flexibleGrid.ColumnDefinitions.Add(newColumn);
            }

            for (int y = 0; y <= yTotal; y++)
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
                        //current.myPanel.Width = size - .5;
                        //current.myPanel.Height = size - .5;
                        Grid.SetColumn(current.thisView, current.xPos);
                        Grid.SetColumnSpan(current.thisView, current.xSize);
                        Grid.SetRow(current.thisView, current.yPos);
                        Grid.SetRowSpan(current.thisView, current.ySize);

                        flexibleGrid.Children.Add(current.thisView);
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
                if (canvas.xPos > x_max)
                {
                    x_max = canvas.xPos;
                }
                if (canvas.yPos > y_max)
                {
                    y_max = canvas.yPos;
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
            for (int i = 0; i < children.Count; i++)
            {
                children[i].myPanel.Dispatcher.BeginInvoke(new drawingDelegate(children[i].draw));
            }
        }
    }

    public abstract class dynamicView2
    {
        public UIElement thisView;
        public Panel myPanel;
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

            Canvas thisCanvas = new Canvas();

            thisCanvas.AddHandler(Canvas.MouseDownEvent, new MouseButtonEventHandler(startMouseDraw));
            thisCanvas.AddHandler(Canvas.StylusDownEvent , new StylusDownEventHandler(startStylusDraw));

            thisCanvas.AddHandler(Canvas.MouseMoveEvent, new MouseEventHandler(mouseMoveDraw));
            thisCanvas.AddHandler(Canvas.StylusMoveEvent, new StylusEventHandler(stylusMoveDraw));

            thisCanvas.AddHandler(Canvas.MouseUpEvent, new MouseButtonEventHandler(endMouseDraw));
            thisCanvas.AddHandler(Canvas.StylusUpEvent, new StylusEventHandler(endStylusDraw));
            thisCanvas.Background = Brushes.White;

            currentBorder.Child = thisCanvas;

            myPanel = thisCanvas;
            thisView = currentBorder;

        }    
       
        public override void clear()
        {
            currentLine = 0;
            first = true;
            myPanel.Children.Clear();
        }

        public override void draw()
        {
           
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
                    myPanel.Children.Add(myLine);
                }
                timer.Reset();
                Point position = e.GetPosition(parent.core.getWindow());
                parent.core.addData(new mPoint(position, currentLine));
                myLine.Points.Add(position);
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
                myPanel.Children.Add(myLine);
            }
            timer.Reset();
            Point position = e.GetPosition(parent.core.getWindow());
            parent.core.addData(new mPoint(position, currentLine));
            myLine.Points.Add(position);
        }        

        private void mouseMoveDraw(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.StylusDevice==null)
            {

                Point position = e.GetPosition(parent.core.getWindow());
                parent.core.addData(new mPoint(position, currentLine));
                myLine.Points.Add(position);

            }
        }

        private void stylusMoveDraw(object sender, StylusEventArgs e)
        {

            Point position = e.GetPosition(parent.core.getWindow());
            parent.core.addData(new mPoint(position, currentLine));
            myLine.Points.Add(position);

            e.Handled = true;   
        }

        private void endMouseDraw(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                Point position = e.GetPosition(parent.core.getWindow());
                parent.core.addData(new mPoint(position, currentLine));
                timer.Start();
            }
        }
        private void endStylusDraw(object sender, StylusEventArgs e)
        {
            Point position = e.GetPosition(parent.core.getWindow());
            parent.core.addData(new mPoint(position, currentLine));
            timer.Start();
            e.Handled = true;
        }
    }

    public class multiLineDrawCanvas2 : dynamicView2
    {
        private List<mPoint> data;
        public TextBox title;
        public Canvas drawCanvas;
        public string titleText;
        public bool toAddCircles;
        public double padding = .1;
        public double outOf = 1.2;
        //public double outOfy = 1.2;
        public multiLineDrawCanvas2(int xPos, int yPos, int xSize, int ySize, dynamicDisplay2 parent, string titleText, bool reserved)
        {
            toAddCircles = false;
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

            myPanel = new StackPanel();
            drawCanvas = new Canvas();

            currentBorder.Child = myPanel;

            thisView = currentBorder;

            title = new TextBox();
            title.Text = titleText;
            myPanel.Children.Add(title);
            myPanel.Children.Add(drawCanvas);
        }

        public void newData(List<mPoint> data)
        {
            this.data = new List<mPoint>(data);
        }
        public List<mPoint> getData() { return data; }

        public override void clear()
        {
            drawCanvas.Children.Clear();
            myPanel.Children.Clear();
            myPanel.Children.Add(title);
            myPanel.Children.Add(drawCanvas);
        }

        public override void draw()
        {
            myPanel.Children.Clear();
            
            myPanel.Children.Add(drawCanvas);
            drawCanvas.Children.Clear();
            drawCanvas.Children.Add(title);
            title.Text = titleText;

            double radius = 4;

            if ((data != null) && data.Count > 1)
            {
                List<Polyline> lines = new List<Polyline>();               

                double Scale = drawCanvas.ActualHeight / outOf;
                double leftPad = (Math.Abs(drawCanvas.RenderSize.Width - drawCanvas.RenderSize.Height)) / 2.0;
                
                for (int i = 0; i < data.Count; i++)
                {
                    double newX = data[i].X;
                    double newY = data[i].Y;

                    while (lines.Count <= data[i].line)
                    {
                        Polyline myLine = new Polyline();
                        myLine.Stroke = Brushes.Black;
                        myLine.StrokeThickness = 2;
                        lines.Add(myLine);
                        drawCanvas.Children.Add(myLine);
                    }

                    newX += padding+leftPad;
                    newY += padding;

                    newX *= Scale;
                    newY *= Scale;
                    if (toAddCircles) { drawCircle(newX, newY, radius); }
                    lines[data[i].line].Points.Add(new Point(newX, newY));
                }
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
}
