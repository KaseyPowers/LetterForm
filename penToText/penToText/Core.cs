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
        private convertToText2 myTextConverter;
        private static long pause = (long)(.25 * 1000);
        public Core()
        {            
            mainWindow = new PenToText();
            mainWindow.WindowState = WindowState.Maximized;
            display = new dynamicDisplay2(this);
            mainWindow.Window_Container.Children.Add(display.getContent());            

            mainWindow.Clear.Click += new RoutedEventHandler(Clear_Click);
            mainWindow.Submit.Click += new RoutedEventHandler(Submit_Click);
            mainWindow.TextBreakDown.Click += new RoutedEventHandler(Display_Click);
            mainWindow.OverallData.Click += new RoutedEventHandler(Data_Click);
            mainWindow.SetLetter.Click += new RoutedEventHandler(Submit_Option_Click);

            mainWindow.InitializeComponent();
            mainWindow.SizeToContent = SizeToContent.WidthAndHeight;
            mainWindow.Top = System.Windows.SystemParameters.WorkArea.Top;
            mainWindow.Left = System.Windows.SystemParameters.WorkArea.Left;
            mainWindow.ShowActivated = true;
            mainWindow.Show();

            input = new inputView(0, 0, 2, 2, display, pause, true);
            display.addCanvas(input);

            TextBreakDown = new List<multiLineDrawView>();



            multiLineDrawView nextCanvas;

            nextCanvas = new multiLineDrawView(2, 0, 1, 1, display, "Rescale", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(2, 1, 1, 1, display, "Resample then Rescale", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(3, 0, 1, 1, display, "Resample and Rescale", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);


            /*nextCanvas = new multiLineDrawView(3, 1, 1, 1, display, "Resample As New", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            /*nextCanvas = new multiLineDrawView(3, 0, 1, 1, display, "SectionTest", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(2, 1, 1, 1, display, "Current Section Clean", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(3, 1, 1, 1, display, "Kasey Section Clean", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);

            nextCanvas = new multiLineDrawView(4, 1, 1, 1, display, "Dominique Section Clean", true);
            nextCanvas.outOf = 1.2;
            nextCanvas.padding = .1;
            nextCanvas.toAddCircles = true;
            display.addCanvas(nextCanvas);
            TextBreakDown.Add(nextCanvas);*/

            myTextConverter = new convertToText2(this);
            blockingData = new BlockingCollection<mPoint>();
            addingData = Task.Factory.StartNew(() => myTextConverter.getData(blockingData));  
        }

        public PenToText getWindow()
        {
            return mainWindow;
        }

        public void addData(mPoint newPoint)
        {
            if (!blockingData.IsAddingCompleted)
            {
                blockingData.Add(newPoint);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            display.clear();

            blockingData.CompleteAdding();
            addingData.Wait();
            myTextConverter.clear();
            blockingData = new BlockingCollection<mPoint>();

            addingData = Task.Factory.StartNew(() => myTextConverter.getData(blockingData)); 
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
