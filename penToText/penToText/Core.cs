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

       // public BlockingCollection<mPoint> blockingData;
       // private Task addingData;

        private List<textConverter> textConverters;
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
            //Current Displays
            nextCanvas = new multiLineDrawView(2, thisY, 1, 1, display, "Resample", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(3, thisY, 1, 1, display, "Sections", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            thisY++;

            //Dominique Displays
            nextCanvas = new multiLineDrawView(2, thisY, 1, 1, display, "Resample", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(3, thisY, 1, 1, display, "Sections", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            thisY++;

            //Kasey Windows
            nextCanvas = new multiLineDrawView(2, thisY, 1, 1, display, "Resample", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(3, thisY, 1, 1, display, "Sections", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            thisY++;           

            //myTextConverter = new convertToText2(this);
           // kaseyTextConvert = new KaseyConvertToText(this, new int[] { 0, 1 });
            //dominiqueTextConvert = new DominiqueConvertToText(this, new int[] { 2, 3 });

            textConverters = new List<textConverter>();
            textConverters.Add(new currentTextConverter(this, 0, 1));
            textConverters.Add(new dominiqueTextConverter(this, 2, 3));
            textConverters.Add(new kaseyTextConverter(this, 4, 5));

            collections = new BlockingCollection<mPoint>[textConverters.Count];
            addingThreads = new Task[textConverters.Count];
           
            for (int i = 0; i < textConverters.Count; i++ )
            {
                int workingVal = i;
                collections[workingVal] = new BlockingCollection<mPoint>();
                addingThreads[workingVal] = Task.Factory.StartNew(() => textConverters[workingVal].getData(collections[workingVal]));
            }

            /*
            collections[0] = new BlockingCollection<mPoint>();
            addingThreads[0] = Task.Factory.StartNew(() => kaseyTextConvert.getData(collections[0]));

            collections[1] = new BlockingCollection<mPoint>();
            addingThreads[1] = Task.Factory.StartNew(() => dominiqueTextConvert.getData(collections[1]));  */

            //blockingData = new BlockingCollection<mPoint>();
            //addingData = Task.Factory.StartNew(() => myTextConverter.getData(blockingData));  
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
            for (int i = 0; i < collections.Length; i++)
            {
                if (!collections[i].IsAddingCompleted)
                {
                    collections[i].Add(newPoint);
                }
            }
           /* if (!blockingData.IsAddingCompleted)
            {
                blockingData.Add(newPoint);
            }*/
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            display.clear();

            for (int i = 0; i < textConverters.Count; i++)
            {
                int workingVal = i;
                collections[workingVal].CompleteAdding();
                addingThreads[workingVal].Wait();
                textConverters[workingVal].clear();
                collections[workingVal] = new BlockingCollection<mPoint>();
                addingThreads[workingVal] = Task.Factory.StartNew(() => textConverters[workingVal].getData(collections[workingVal]));

               /* switch (i)
                {
                    case 0:
                        kaseyTextConvert.clear();
                        collections[i] = new BlockingCollection<mPoint>();
                        addingThreads[i] = Task.Factory.StartNew(() => kaseyTextConvert.getData(collections[i]));
                        break;
                    case 1:
                        dominiqueTextConvert.clear();
                        collections[i] = new BlockingCollection<mPoint>();
                        addingThreads[i] = Task.Factory.StartNew(() => dominiqueTextConvert.getData(collections[i]));
                        break;
                }*/
            }
/*            blockingData.CompleteAdding();
            addingData.Wait();
            myTextConverter.clear();
            blockingData = new BlockingCollection<mPoint>();

            addingData = Task.Factory.StartNew(() => myTextConverter.getData(blockingData)); */
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
