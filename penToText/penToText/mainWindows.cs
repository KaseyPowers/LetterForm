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
        public convertToText myPenToText;
        public dynamicDisplay myDynamicDisplay;

        private double inputHeight = 300;
        private double inputWidth = 300;

        Point currentPoint;
        List<Point> data;

        public mainWindows()
        {
            data = new List<Point>();
            currentPoint = new Point(-5, -5);            
            myDynamicDisplay = new dynamicDisplay();
        }

        public void createWindows()
        {
            myInputWindow = new InputWindow();
            myInputWindow.manager = this;

            Size inputSize = new Size();
            inputSize.Width = myInputWindow.InputCanvas.Width;
            inputSize.Height = myInputWindow.InputCanvas.Height;
            myPenToText = new convertToText(myDynamicDisplay, inputSize);

            myDisplayWindow = new DisplayWindow(myDynamicDisplay);

            myDisplayWindow.manager = this;
            myDisplayWindow.Visibility = Visibility.Visible;

            myDisplayWindow.Top = myInputWindow.Top;
            myDisplayWindow.Left = myInputWindow.Left + inputWidth;

            myDisplayWindow.Owner = myInputWindow;

            resized();

        }

        public void resized()
        {
            myPenToText.inputSize = myInputWindow.InputCanvas.RenderSize;
            myPenToText.resize();
            /*myDisplayWindow.canvasSize.Height = inputHeight;
            myDisplayWindow.canvasSize.Width = inputWidth;
            myDisplayWindow.resize();*/
        }

        
        public void newData(Point newPoint)
        {
            myPenToText.newData(newPoint);          

        }       

        public void clear()
        {
            data.Clear();
            currentPoint = new Point(-5, -5);
            myPenToText.clear();
        }

        public void endDraw()
        {
            myPenToText.endDraw();
        }
    }
}
