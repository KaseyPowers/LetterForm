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

            double columnWidth = core.getWindow().ActualWidth / x_max;
            double rowHeight = core.getWindow().ActualHeight / y_max;

            double size = columnWidth;
            if (size > rowHeight)
            {
                size = rowHeight;
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
            core.getWindow().Dispatcher.BeginInvoke( new drawingDelegate(() =>
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
            drawCanvas.AddHandler(Canvas.StylusDownEvent , new StylusDownEventHandler(startStylusDraw));

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
                drawCanvas.Children.Add(myLine);
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
            double radius = 4;            
            List<Polyline> lines = new List<Polyline>();
            List<Ellipse> circles = new List<Ellipse>();

            //title = new TextBox();
            title.Text = titleText;
            drawCanvas.Children.Clear();
            //DockPanel.SetDock(title, Dock.Top);
            //dock.Children.Add(title);
            //dock.Children.Add(drawCanvas);

            for (int i = 0; i < data.Count; i++)
            {
                while (lines.Count <= data[i].line)
                {
                        Polyline myLine = new Polyline();
                        myLine.Stroke = Brushes.Black;
                        myLine.StrokeThickness = 2;
                        drawCanvas.Children.Add(myLine);
                        lines.Add(myLine);
                        
                }
                if (toAddCircles)
                {
                    drawCircle(data[i].X, data[i].Y, radius);
                }
                lines[data[i].line].Points.Add(data[i].getPoint());
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

    public class charInputView : dynamicView2
    {
        TextBlock prompt;
        TextBox input;
        public charInputView()
        {
            
        }

        public override void clear()
        {
        }

        public override void draw()
        {

        }
    }
}
