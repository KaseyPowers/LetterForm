using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
        public delegate void addDataDelegate(List<Point> arg);

        public InputWindow myInputWindow;
        public DisplayWindow myDisplayWindow;
        //public DisplayWindow myDisplayWindow2;
        //public convertToText myPenToText;
        public dynamicDisplay myDynamicDisplay;
        //public dynamicDisplay myDynamicDisplay2;
        public convertToText3 myPenToText3;

        private double inputHeight = 300;
        private double inputWidth = 300;

        Point currentPoint;
        List<Point> data;
        List<Point> toAdd;


        //threading attempt 2
        public BlockingCollection<Point> blockingData;
        private Task addingData;

        public mainWindows()
        {
            data = new List<Point>();
            toAdd = new List<Point>();
            currentPoint = new Point(-5, -5);            
            myDynamicDisplay = new dynamicDisplay();
            //myDynamicDisplay2 = new dynamicDisplay();
        }

        public void createWindows()
        {
            myInputWindow = new InputWindow();
            myInputWindow.manager = this;

            Size inputSize = new Size();
            inputSize.Width = myInputWindow.InputCanvas.Width;
            inputSize.Height = myInputWindow.InputCanvas.Height;

            //myPenToText = new convertToText(myDynamicDisplay, inputSize);
            myPenToText3 = new convertToText3(myDynamicDisplay, inputSize);
            blockingData = new BlockingCollection<Point>();
            addingData = Task.Factory.StartNew(() => myPenToText3.getData(blockingData));            

            myDisplayWindow = new DisplayWindow(myDynamicDisplay);

            myDisplayWindow.manager = this;
            myDisplayWindow.Visibility = Visibility.Visible;

            myDisplayWindow.Top = myInputWindow.Top;
            myDisplayWindow.Left = myInputWindow.Left + myInputWindow.ActualWidth;

            myDisplayWindow.Owner = myInputWindow;

            /*Thread thread = new Thread(() =>
            {
                this.myDisplayWindow = new DisplayWindow(this.myDynamicDisplay);
                this.myDisplayWindow.manager = this;

                myDisplayWindow.Visibility = Visibility.Visible;

                this.myDisplayWindow.Closed += (sender2, e2) =>
                this.myDisplayWindow.Dispatcher.InvokeShutdown();
                this.myDisplayWindow.Owner = this.myInputWindow;
                this.myDisplayWindow.Top = this.myInputWindow.Top;
                this.myDisplayWindow.Left = this.myInputWindow.Left + this.myInputWindow.ActualWidth;
                System.Windows.Threading.Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();*/

            /*myDisplayWindow2 = new DisplayWindow(myDynamicDisplay);

            myDisplayWindow2.manager = this;
            myDisplayWindow2.Visibility = Visibility.Visible;

            myDisplayWindow2.Top = myInputWindow.Top + myInputWindow.ActualHeight;
            myDisplayWindow2.Left = myInputWindow.Left;

            myDisplayWindow2.Owner = myInputWindow;*/

            resized();

        }

        public void resized()
        {
            /*myPenToText.inputSize = myInputWindow.InputCanvas.RenderSize;
            myPenToText.resize();*/

            myPenToText3.inputSize = myInputWindow.InputCanvas.RenderSize;
            myPenToText3.resize();
            /*myDisplayWindow.canvasSize.Height = inputHeight;
            myDisplayWindow.canvasSize.Width = inputWidth;
            myDisplayWindow.resize();*/
        }

        
        public void newData(Point newPoint)
        {
            //myPenToText.newData(newPoint);
            if (!blockingData.IsAddingCompleted) { blockingData.Add(newPoint); }
            //blockingData.Add(newPoint);
            
        }       

        public void clear()
        {
            blockingData.CompleteAdding();
            addingData.Wait();            
            blockingData = new BlockingCollection<Point>();

            data.Clear();
            currentPoint = new Point(-5, -5);
            //myPenToText.clear();
            myPenToText3.clear();

            addingData = Task.Factory.StartNew(() => myPenToText3.getData(blockingData)); 
        }

        public void endDraw()
        {
            //myPenToText.endDraw();
        }
    }
}
