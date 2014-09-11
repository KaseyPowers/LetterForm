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
        Point currentPoint;
        public mainWindows manager;
        public bool loaded;


        public InputWindow()
        {
            InitializeComponent();
            this.Show();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loaded = true;
        }

        private void update_size(object sender, SizeChangedEventArgs e)
        {
            if (loaded)
            {
                manager.resized();
            }
        }

         private void Clear_Click(object sender, RoutedEventArgs e)
        {
            InputCanvas.Children.Clear();
            manager.clear();
            manager.myDisplayWindow.arrows.changeLoc(0, 1);
        }

        private void startDraw(object sender, MouseEventArgs e)
        {
            if (e.StylusDevice == null)
            {
                Point position = e.GetPosition(this);
                currentPoint = position;
                manager.newData(position);
            }
        }

        private void startDraw(object sender, StylusEventArgs e)
        {
            Point position = e.GetPosition(this);
            currentPoint = position;
            manager.newData(position);
            e.Handled = true;
        }
        
        private void moveDraw(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && e.StylusDevice==null)
            {

                Point position = e.GetPosition(this);
                manager.newData(position);
                Line myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.Black;
                myLine.X1 = currentPoint.X;
                myLine.X2 = position.X;
                myLine.Y1 = currentPoint.Y;
                myLine.Y2 = position.Y;
                myLine.StrokeThickness = 2;

                currentPoint = position;
                InputCanvas.Children.Add(myLine);

            }
        }

        private void moveDraw(object sender, StylusEventArgs e)
        {

            Point position = e.GetPosition(this);
            manager.newData(position);
            Line myLine = new Line();
            myLine.Stroke = System.Windows.Media.Brushes.Black;
            myLine.X1 = currentPoint.X;
            myLine.X2 = position.X;
            myLine.Y1 = currentPoint.Y;
            myLine.Y2 = position.Y;
            myLine.StrokeThickness = 2;

            currentPoint = position;

            InputCanvas.Children.Add(myLine);

            e.Handled = true;   
        }
    }
}
