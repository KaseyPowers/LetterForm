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
        public dynamicDisplay myDynamicDisplay;
        public static convertToText myPenToText;
        public dataStuff myDataStuff;

        //threading attempt 2
        public BlockingCollection<Point> blockingData;
        private Task addingData;

        public mainWindows()
        {         
            myDynamicDisplay = new dynamicDisplay();
        }

        public void createWindows()
        {
            myInputWindow = new InputWindow();
            myInputWindow.manager = this;

            myPenToText = new convertToText(myDynamicDisplay);
            blockingData = new BlockingCollection<Point>();
            addingData = Task.Factory.StartNew(() => myPenToText.getData(blockingData));            

            myDisplayWindow = new DisplayWindow(myDynamicDisplay);

            myPenToText.setDisplayActive(true);

            myDisplayWindow.manager = this;
            myDisplayWindow.Visibility = Visibility.Visible;

            myDisplayWindow.Top = myInputWindow.Top;
            myDisplayWindow.Left = myInputWindow.Left + myInputWindow.ActualWidth;

            myDisplayWindow.Owner = myInputWindow;

            resized();

        }

        internal static void OnDisplayWindowClose(object sender, CancelEventArgs e)
        {
            myPenToText.setDisplayActive(false);
        }

        public void toggleDisplayWindow()
        {
            if (myPenToText.getDisplayActive())
            {
                myDisplayWindow.Close();
            }
            else
            {
                myDisplayWindow = new DisplayWindow(myDynamicDisplay);

                myPenToText.setDisplayActive(true);

                myDisplayWindow.manager = this;
                myDisplayWindow.Visibility = Visibility.Visible;

                myDisplayWindow.Top = myInputWindow.Top;
                myDisplayWindow.Left = myInputWindow.Left + myInputWindow.ActualWidth;

                myDisplayWindow.Owner = myInputWindow;
            }
        }

        public void toggleDataDisplayWindow()
        {

        }

        public void resized()
        {

        }

        public void Submit(char associatedCharacter)
        {
            blockingData.CompleteAdding();
            addingData.Wait();
            myDataStuff.Submit(myPenToText.getCleanedData(), associatedCharacter);
            myPenToText.clear();
            blockingData = new BlockingCollection<Point>();

            addingData = Task.Factory.StartNew(() => myPenToText.getData(blockingData)); 
        }
        
        public void newData(Point newPoint)
        {
            if (!blockingData.IsAddingCompleted) { blockingData.Add(newPoint); }            
        }       

        public void clear()
        {
            blockingData.CompleteAdding();
            addingData.Wait();
            myPenToText.clear();
            blockingData = new BlockingCollection<Point>();

            addingData = Task.Factory.StartNew(() => myPenToText.getData(blockingData)); 
        }

        public void endDraw()
        {

        }

       
    }
}
