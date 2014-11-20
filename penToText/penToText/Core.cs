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
    public class Core
    {

        public BlockingCollection<mPoint> blockingData;
        private Task addingData;
        public List<mPoint> originalData; //this is stored just in case we want it later?

        private List<textConverter> textConverters;
        private List<dataTree> dataTrees;
        public BlockingCollection<mPoint>[] collections;
        public Task[] addingThreads;


        /*store dynamic windows:
         * input
         * display outputs
         *      steps of breakdown
         *      letter being guessed
         *      etc.
         * display data representation
         * dynamic display to hold these
         */
        private PenToText mainWindow;
        private dynamicDisplay2 display;
        private inputView input;
        public List<multiLineDrawView> TextBreakDown;
        private bool windowStarted;
       // private convertToText2 myTextConverter;
       // private KaseyConvertToText kaseyTextConvert;
        //private DominiqueConvertToText dominiqueTextConvert;
        private static long pause = (long)(.25 * 1000);
        public Core()
        {
            windowStarted = false;
            mainWindow = new PenToText();
            display = new dynamicDisplay2(this);
            mainWindow.Window_Container.Children.Add(display.getContent());            

            mainWindow.Clear.Click += new RoutedEventHandler(Clear_Click);
            mainWindow.Submit.Click += new RoutedEventHandler(Submit_Click);
            mainWindow.TextBreakDown.Click += new RoutedEventHandler(Display_Click);
            mainWindow.OverallData.Click += new RoutedEventHandler(Data_Click);
            mainWindow.SetLetter.Click += new RoutedEventHandler(Submit_Option_Click);

            mainWindow.InitializeComponent();
            //mainWindow.SizeToContent = SizeToContent.WidthAndHeight;
            mainWindow.Top = System.Windows.SystemParameters.WorkArea.Top;
            mainWindow.Left = System.Windows.SystemParameters.WorkArea.Left;
            mainWindow.Width = System.Windows.SystemParameters.WorkArea.Width;
            mainWindow.Height = System.Windows.SystemParameters.WorkArea.Height;
            mainWindow.ShowActivated = true;
            mainWindow.Show();
            windowStarted = true;

            input = new inputView(0, 0, 2, 2, display, pause, true);
            display.addCanvas(input);
            TextBreakDown = new List<multiLineDrawView>();



            multiLineDrawView nextCanvas;

            int thisY = 0;
            int thisX = 2;
            //Current Displays
            nextCanvas = new multiLineDrawView(thisX, thisY, 1, 1, display, "Resample", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(thisX, thisY+1, 1, 1, display, "Sections", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            thisX++;

            //Dominique Displays
            nextCanvas = new multiLineDrawView(thisX, thisY, 1, 1, display, "Resample", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(thisX, thisY + 1, 1, 1, display, "Sections", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            thisX++;

            //Kasey Windows
            nextCanvas = new multiLineDrawView(thisX, thisY, 1, 1, display, "Resample", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(thisX, thisY + 1, 1, 1, display, "Sections", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            thisX++;      

            textConverters = new List<textConverter>();
            textConverters.Add(new currentTextConverter(this, 0, 1));
            textConverters.Add(new dominiqueTextConverter(this, 2, 3));
            textConverters.Add(new kaseyTextConverter(this, 4, 5));

            collections = new BlockingCollection<mPoint>[textConverters.Count];
            addingThreads = new Task[textConverters.Count];

            dataTrees = new List<dataTree>();

            blockingData = new BlockingCollection<mPoint>();
            addingData = Task.Factory.StartNew(() => sendData(blockingData));
            /*for (int i = 0; i < textConverters.Count; i++ )
            {
                int workingVal = i;
                collections[workingVal] = new BlockingCollection<mPoint>();
                addingThreads[workingVal] = Task.Factory.StartNew(() => textConverters[workingVal].getData(collections[workingVal]));
            }*/

        }

        public bool getWindowStarted()
        {
            return windowStarted;
        }

        public PenToText getWindow()
        {
            return mainWindow;
        }

        public void addData(mPoint newPoint)
        {
            /*for (int i = 0; i < collections.Length; i++)
            {
                if (!collections[i].IsAddingCompleted)
                {
                    collections[i].Add(newPoint);
                }
            }*/

            if (!blockingData.IsAddingCompleted)
            {
                blockingData.Add(newPoint);
            }
        }

        public void sendData(BlockingCollection<mPoint> data)
        {
            mPoint last = null;
            if (originalData == null)
            {
                originalData = new List<mPoint>();
            }
            foreach (var item in data.GetConsumingEnumerable())
            {
                mPoint current = item;

                if (originalData.Count > 0)
                {
                    last = originalData[originalData.Count - 1];
                }

                if (originalData.Count == 0 || !(current.Equals(last)))
                {
                    originalData.Add(current);
                    //updateData();
                    for (int i = 0; i < textConverters.Count; i++)
                    {
                        textConverters[i].updateData(current);
                    }
                }
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            display.clear();
            /*
            for (int i = 0; i < textConverters.Count; i++)
            {
                int workingVal = i;
                collections[workingVal].CompleteAdding();
                addingThreads[workingVal].Wait();
                textConverters[workingVal].clear();
                collections[workingVal] = new BlockingCollection<mPoint>();
                addingThreads[workingVal] = Task.Factory.StartNew(() => textConverters[workingVal].getData(collections[workingVal]));
            }*/

            blockingData.CompleteAdding();
            addingData.Wait();
            //myTextConverter.clear();

            for (int i = 0; i < textConverters.Count; i++)
            {
                textConverters[i].clear();
            }
            originalData.Clear();
            blockingData = new BlockingCollection<mPoint>();
            addingData = Task.Factory.StartNew(() => sendData(blockingData)); 
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Submit_Option_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Display_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Data_Click(object sender, RoutedEventArgs e)
        {

        }

        public void draw()
        {
            display.draw();
        }
    }
}
