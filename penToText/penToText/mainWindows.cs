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
        public DataDisplay myDataDisplay;
        public dynamicDisplay myDynamicDisplay;
        public static convertToText myPenToText;
        public dataStuff myDataStuff;

        //threading attempt 2
        public BlockingCollection<mPoint> blockingData;
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
            myDataStuff = new dataStuff(this);
            blockingData = new BlockingCollection<mPoint>();
            addingData = Task.Factory.StartNew(() => myPenToText.getData(blockingData));            

            myDisplayWindow = new DisplayWindow(myDynamicDisplay);

            myPenToText.setDisplayActive(true);

            myDisplayWindow.manager = this;
            myDisplayWindow.Visibility = Visibility.Visible;

            

            myDisplayWindow.Owner = myInputWindow;

            myDataDisplay = new DataDisplay(myDataStuff);
            myDataDisplay.Visibility = Visibility.Visible;

            myDataDisplay.Owner = myInputWindow;

            myInputWindow.Top = System.Windows.SystemParameters.WorkArea.Top;
            myInputWindow.Left = System.Windows.SystemParameters.WorkArea.Left;

            myDataDisplay.Top = myInputWindow.Top+ myInputWindow.Height;
            myDataDisplay.Left = myInputWindow.Left;
            myDataDisplay.Width = System.Windows.SystemParameters.WorkArea.Width - myDisplayWindow.Width;
            myDataDisplay.Height = System.Windows.SystemParameters.WorkArea.Height - myDataDisplay.Top;

            myDisplayWindow.Top = System.Windows.SystemParameters.WorkArea.Top;
            myDisplayWindow.Left = System.Windows.SystemParameters.WorkArea.Width - myDisplayWindow.Width;

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
            if (myDataDisplay.IsVisible)
            {
                myDataDisplay.Visibility = Visibility.Hidden;
            }
            else if(myDataDisplay !=null )
            {
                myDataDisplay.Visibility = Visibility.Visible;
                myDataDisplay.Top = System.Windows.SystemParameters.WorkArea.Top;
                myDataDisplay.Left = System.Windows.SystemParameters.WorkArea.Left;
                myDataDisplay.Height = System.Windows.SystemParameters.WorkArea.Height;
                myDataDisplay.Owner = myInputWindow;
            }
            else
            {
                myDataDisplay = new DataDisplay(myDataStuff);
                myDataDisplay.Visibility = Visibility.Visible;
                myDataDisplay.Top = System.Windows.SystemParameters.WorkArea.Top;
                myDataDisplay.Left = System.Windows.SystemParameters.WorkArea.Left;
                myDataDisplay.Height = System.Windows.SystemParameters.WorkArea.Height;
                myDataDisplay.Owner = myInputWindow;
            }
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
            blockingData = new BlockingCollection<mPoint>();
            currentLine = 0;
            addingData = Task.Factory.StartNew(() => myPenToText.getData(blockingData)); 
        }

        private int currentLine;
        public void newData(Point newPoint)
        {
            if (!blockingData.IsAddingCompleted) { blockingData.Add(new mPoint(newPoint, currentLine)); }            
        }

        public void closing()
        {
            myDataStuff.writeData();
        }

        public void clear()
        {
            currentLine = 0;
            blockingData.CompleteAdding();
            addingData.Wait();
            myPenToText.clear();
            blockingData = new BlockingCollection<mPoint>();

            addingData = Task.Factory.StartNew(() => myPenToText.getData(blockingData)); 
        }

        public void endDraw()
        {
            currentLine++;
        }

        public void updateTree(mSectionNode2 treeRoot){
            Console.WriteLine(treeRoot.getString());
            myPenToText.setTree(treeRoot);
        }
       
    }
}
