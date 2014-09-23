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
    /// <summary>
    /// Interaction logic for InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        //Point currentPoint;
        public mainWindows manager;
        private Polyline myLine;
        public bool loaded;
        public double aspectRatio = 0.0;

        public InputWindow()
        {
            myLine = new Polyline();
            InitializeComponent();
            this.Show();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            aspectRatio = this.ActualWidth / this.ActualHeight;
            loaded = true;
        }

        private void update_size(object sender, SizeChangedEventArgs sizeInfo)
        {
            if (loaded)
            {
                double xChange = Math.Abs(sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width);
                double yChange = Math.Abs(sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height);

                if (xChange > yChange)
                {
                    this.Height= this.Width;
                }
                else
                {
                    this.Width = this.Height;
                }         
                manager.resized();
            }
        }

         private void Clear_Click(object sender, RoutedEventArgs e)
        {
            InputCanvas.Children.Clear();
            manager.clear();
            //manager.myDisplayWindow.arrows.changeLoc(0, 1);
        }

        private void startDraw(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                Point position = e.GetPosition(this);
                manager.newData(position);
                //currentPoint = position;
                myLine = new Polyline();
                myLine.Stroke = System.Windows.Media.Brushes.Black;
                myLine.StrokeThickness = 2;
                myLine.Points.Add(position);
                InputCanvas.Children.Add(myLine);
            }
        }

        private void startDraw(object sender, StylusEventArgs e)
        {
            Point position = e.GetPosition(this);
            manager.newData(position);
            //currentPoint = position;
            myLine = new Polyline();
            myLine.Stroke = System.Windows.Media.Brushes.Black;
            myLine.StrokeThickness = 2;
            myLine.Points.Add(position);
            InputCanvas.Children.Add(myLine);            
            e.Handled = true;
        }        

        private void moveDraw(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.StylusDevice==null)
            {

                Point position = e.GetPosition(this);
                manager.newData(position);
                myLine.Points.Add(position);
                /*Line myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.Black;
                myLine.X1 = currentPoint.X;
                myLine.X2 = position.X;
                myLine.Y1 = currentPoint.Y;
                myLine.Y2 = position.Y;
                myLine.StrokeThickness = 2;

                currentPoint = position;
                InputCanvas.Children.Add(myLine);*/

            }
        }

        private void moveDraw(object sender, StylusEventArgs e)
        {

            Point position = e.GetPosition(this);
            manager.newData(position);
            myLine.Points.Add(position);
            /*Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Black;
            myLine.X1 = currentPoint.X;
            myLine.X2 = position.X;
            myLine.Y1 = currentPoint.Y;
            myLine.Y2 = position.Y;
            myLine.StrokeThickness = 2;

            currentPoint = position;

            InputCanvas.Children.Add(myLine);*/

            e.Handled = true;   
        }

        private void endDraw(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                manager.endDraw();
            }
        }
        private void endDraw(object sender, StylusEventArgs e)
        {
            manager.endDraw();
            e.Handled = true;
        }
    }
}
