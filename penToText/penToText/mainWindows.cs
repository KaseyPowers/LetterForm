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
using Petzold.Media2D;

namespace penToText
{
    class program
    {
        private static mainWindows manager;
        [STAThread]
        static void Main(string[] args)
        {
            manager = new mainWindows();
            manager.createWindows();
           
            App app = new App();
            app.Run(manager.myInputWindow);

        }
    }
    public class mainWindows
    {
        public InputWindow myInputWindow;
        public DisplayWindow myDisplayWindow;

        private double inputHeight = 300;
        private double inputWidth = 300;

        Point currentPoint;
        List<Point> data;

        public mainWindows()
        {
            data = new List<Point>();
            currentPoint = new Point(-5, -5);
        }

        public void createWindows()
        {
            myInputWindow = new InputWindow();
            myInputWindow.manager = this;

            myDisplayWindow = new DisplayWindow();

            myDisplayWindow.manager = this;
            myDisplayWindow.Visibility = Visibility.Visible;

            myDisplayWindow.Top = myInputWindow.Top;
            myDisplayWindow.Left = myInputWindow.Left + inputWidth;

            myDisplayWindow.Owner = myInputWindow;

            resized();

        }

        public void resized()
        {
            inputWidth = ((Grid)myInputWindow.Content).ActualWidth;
            inputHeight = ((Grid)myInputWindow.Content).ActualHeight;

            myDisplayWindow.canvasSize.Height = inputHeight;
            myDisplayWindow.canvasSize.Width = inputWidth;
            myDisplayWindow.resize();
        }

        
        public void newData(Point newPoint)
        {
            //input copy
            if(currentPoint==new Point(-5,-5)){
                currentPoint=newPoint;
            }else{
                Line temp= new Line();
                temp.Stroke= Brushes.Black;
                temp.StrokeThickness=2;
                temp.X1= currentPoint.X;
                temp.Y1= currentPoint.Y;
                temp.X2= newPoint.X;
                temp.Y2= newPoint.Y;
                myDisplayWindow.InputCanvas.Children.Add(temp);
                currentPoint = newPoint;
            }

            //arrow copy
            double recordDistance = 15.0f;

            if (data.Count == 0) { data.Add(newPoint); }

            if (distance(newPoint, data[data.Count - 1]) >= recordDistance)
            {
                data.Add(newPoint);
                /*ArrowLine test = new ArrowLine();
                test.Stroke = Brushes.Black;
                test.StrokeThickness = 3;
                test.X1 = 15;
                test.Y1 = 15;
                test.X2 = 200;
                test.Y2 = 200;
                DisplayCanvas.Children.Add(test);*/
                //drawArrow(new Point(5, 5), new Point(100, 100));

                myDisplayWindow.ArrowCanvas.Children.Clear();
                for (int i = 1; i < data.Count; i++)
                {
                    Point a = data[i - 1];
                    Point b = data[i];

                    ArrowLine test = new ArrowLine();
                    test.Stroke = Brushes.Black;
                    test.StrokeThickness = 3;
                    test.X1 = a.X;
                    test.Y1 = a.Y;
                    test.X2 = b.X;
                    test.Y2 = b.Y;
                    myDisplayWindow.ArrowCanvas.Children.Add(test);

                }
            }

        }

        private double distance(Point a, Point b)
        {
            return Math.Sqrt(Math.Pow((a.X - b.X), 2) + Math.Pow((a.Y - b.Y), 2));
        }

        internal void clear()
        {
            data.Clear();
            currentPoint = new Point(-5, -5);
            myDisplayWindow.InputCanvas.Children.Clear();
            myDisplayWindow.ArrowCanvas.Children.Clear();
        }
    }
}
