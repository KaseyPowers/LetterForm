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

namespace penToText
{
    public class dynamicDisplay
    {
        private ScrollViewer holder;
        private Grid flexibleGrid;
        int x_grid;
        int y_grid;
        List<dynamicView> views;


        public dynamicDisplay()
        {
            holder = new ScrollViewer();
            holder.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            holder.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            views = new List<dynamicView>();

            x_grid = 1;
            y_grid = 1;

            flexibleGrid = new Grid();
            flexibleGrid.ShowGridLines = true;
            flexibleGrid.HorizontalAlignment = HorizontalAlignment.Left;
            flexibleGrid.VerticalAlignment = VerticalAlignment.Top;
            holder.Content= flexibleGrid;
            updateGrid();
        }

        public Grid getGrid()
        {            
            return flexibleGrid;
        }

        public ScrollViewer getScrollView()
        {
            return holder;
        }


        private void updateGrid()
        {
            //start from scratch
            flexibleGrid.Children.Clear();
            flexibleGrid.ColumnDefinitions.Clear();
            flexibleGrid.RowDefinitions.Clear();

            //create enough columns and rows
            for (int x = 0; x <= x_grid; x++)
            {
                ColumnDefinition newColumn = new ColumnDefinition();
                flexibleGrid.ColumnDefinitions.Add(newColumn);
            }

            for (int y = 0; y <= y_grid; y++)
            {
                RowDefinition newRow = new RowDefinition();
                flexibleGrid.RowDefinitions.Add(newRow);
            }

            //puts the canvases in location
            if (views.Count!=0)
            {
                foreach (dynamicView current in views)
                {
                    Grid.SetColumn(current.myPanel, current.xPos);
                    Grid.SetRow(current.myPanel, current.yPos);

                    flexibleGrid.Children.Add(current.myPanel);
                }
            }
        }

        public void canvasLocChanged()
        {
            x_grid = 0;
            y_grid = 0;
            foreach (dynamicView canvas in views)
            {
                if (canvas.xPos > x_grid)
                {
                    x_grid = canvas.xPos;
                }
                if (canvas.yPos > y_grid)
                {
                    y_grid = canvas.yPos;
                }
            }
            updateGrid();
        }

        public void addCanvas(dynamicView view)
        {
            views.Add(view);
            canvasLocChanged();
        }


    }

    public abstract class dynamicView
    {
        //thats it for now
        public Panel myPanel;
        public int xPos;
        public int yPos;
        protected dynamicDisplay parent;


        protected int default_sizeX = 300;
        protected int default_sizeY = 300;

         public void changeLoc(int xPos, int yPos)
        {
            this.xPos = xPos;
            this.yPos = yPos;
            parent.canvasLocChanged();
        }

        abstract public void updateDraw();

    }

    /*public class TempCanvas : dynamicView
    {
        public TempCanvas(Canvas canvas, int xPos, int yPos, dynamicDisplay parent)
        {
            myPanel = canvas;
            this.xPos = xPos;
            this.yPos = yPos;
            this.parent = parent;
        }

    }*/

    public class lineDrawCanvas : dynamicView
    {
        private List<Point> data;
        public TextBox title;
        public string titleText;
        public double outOfX;
        public double outOfy;
        public bool toAddCircles;
        private Polyline myLine;
        public lineDrawCanvas( int xPos, int yPos, dynamicDisplay parent, string titleText)
        {
            toAddCircles = false;
            this.data = new List<Point>();
            this.xPos = xPos;
            this.yPos = yPos;
            this.parent = parent;
            this.titleText = titleText;
            outOfy = 0;
            outOfX = 0;            
            //Grid holder = new Grid(); 

            myPanel = new Canvas();
            myPanel.Name = "InputCopyCanvas";
            myPanel.HorizontalAlignment = HorizontalAlignment.Left;
            myPanel.VerticalAlignment = VerticalAlignment.Top;
            myPanel.Width = default_sizeX;
            myPanel.Height = default_sizeY;     

            title = new TextBox();
            title.Text = titleText;
            title.HorizontalAlignment = HorizontalAlignment.Left;
            title.VerticalAlignment = VerticalAlignment.Top;
            title.Width = Double.NaN;
            title.Height = Double.NaN;  
            myPanel.Children.Add(title);

            myLine = new Polyline();
            myLine.Stroke = Brushes.Black;
            myLine.StrokeThickness = 2;
            myPanel.Children.Add(myLine);

            outOfy = myPanel.Height;
            outOfX = myPanel.Width;
        }

        public void newData(List<Point> data) { this.data = data; }
        public List<Point> getData() { return data; }

        public override void updateDraw()
        { 
            myPanel.Children.Clear();
            myPanel.Children.Add(title);
            myLine.Points.Clear();
            myPanel.Children.Add(myLine);
            title.Text = titleText + "\n " + data.Count;

            double radius = 4;
            if (outOfX != 0 && outOfy != 0 && !Double.IsNaN(outOfX) && ! Double.IsNaN(outOfy))
            {
                double xScale = myPanel.Width / outOfX;
                double yScale = myPanel.Height / outOfy;
                if ((data != null) && data.Count > 1)
                {
                    //Point lastPoint = data[0];
                    //if (toAddCircles) { drawCircle(lastPoint.X * xScale, lastPoint.Y * yScale, radius); }
                    for (int i = 0; i < data.Count; i++)
                    {
                        Point currentPoint = data[i];
                        currentPoint.X *= xScale;
                        currentPoint.Y *= yScale;
                        if (toAddCircles) { drawCircle(currentPoint.X, currentPoint.Y, radius); }
                        //myPoints.Add(currentPoint);
                        myLine.Points.Add(currentPoint);
                        /*Line temp = new Line();
                        temp.Stroke = Brushes.Black;
                        temp.StrokeThickness = 2;
                        temp.X1 = currentPoint.X * xScale;
                        temp.Y1 = currentPoint.Y * yScale;
                        temp.X2 = lastPoint.X * xScale;
                        temp.Y2 = lastPoint.Y * yScale;
                        temp.HorizontalAlignment = HorizontalAlignment.Left;
                        temp.VerticalAlignment = VerticalAlignment.Top;
                        childs.Add(temp);
                        lastPoint = currentPoint;*/
                    }
                }
                /*Polyline myLine = new Polyline();
                myLine.Stroke = Brushes.Black;
                myLine.StrokeThickness = 2;
                myLine.Points = myPoints;
                myPanel.Children.Add(myLine);*/
               
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
            myPanel.Children.Add(circle);
        }
    }
}
