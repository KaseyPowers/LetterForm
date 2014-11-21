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

        private char[] alphabet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        dataManager mDataManager;

        public BlockingCollection<mPoint> blockingData;
        private Task addingData;
        public List<mPoint> originalData; //this is stored just in case we want it later?

        private List<textConverter> textConverters;
        private List<dataTree> dataTrees;
        public BlockingCollection<mPoint>[] collections;
        public Task[] addingThreads;

        private PenToText mainWindow;
        private dynamicDisplay2 display;
        private inputView input;
        public List<multiLineDrawView> TextBreakDown;
        public charGuessView guessOutput;
        public charGuessView savedCharCounts;
        private charInputView charSubmit;
        private treeView myTreeView;
        private bool windowStarted;
        private static long pause = (long)(.25 * 1000);
        public Core()
        {
            windowStarted = false;
            mainWindow = new PenToText(this);
            display = new dynamicDisplay2(this);
            mainWindow.Window_Container.Children.Add(display.getContent());            

            mainWindow.Clear.Click += new RoutedEventHandler(Clear_Click);
            //mainWindow.
            /*mainWindow.Submit.Click += new RoutedEventHandler(Submit_Click);
            mainWindow.TextBreakDown.Click += new RoutedEventHandler(Display_Click);
            mainWindow.OverallData.Click += new RoutedEventHandler(Data_Click);
            mainWindow.SetLetter.Click += new RoutedEventHandler(Submit_Option_Click);*/

            mainWindow.InitializeComponent();
            //mainWindow.SizeToContent = SizeToContent.WidthAndHeight;
            mainWindow.Top = System.Windows.SystemParameters.WorkArea.Top;
            mainWindow.Left = System.Windows.SystemParameters.WorkArea.Left;
            mainWindow.Width = System.Windows.SystemParameters.WorkArea.Width;
            mainWindow.Height = System.Windows.SystemParameters.WorkArea.Height;
            mainWindow.ShowActivated = true;
            mainWindow.Show();
            windowStarted = true;           

            textConverters = new List<textConverter>();
            textConverters.Add(new currentTextConverter(this, 0, 1, 0));
            textConverters.Add(new dominiqueTextConverter(this, 2, 3, 1));
            textConverters.Add(new kaseyTextConverter(this, 4, 5, 2));

            collections = new BlockingCollection<mPoint>[textConverters.Count];
            addingThreads = new Task[textConverters.Count];

            

            dataTrees = new List<dataTree>();
            dataTrees.Add(new dataTree(textConverters[0]));

            mDataManager = new dataManager(this);

            setupWindow();
            setupTrees();

            blockingData = new BlockingCollection<mPoint>();
            addingData = Task.Factory.StartNew(() => sendData(blockingData));            
        }

        public void closing()
        {
            mDataManager.writeData();
        }

        private void setupWindow()
        {
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

            nextCanvas = new multiLineDrawView(thisX, thisY + 1, 1, 1, display, "Sections", true);
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


            guessOutput = new charGuessView(thisX, thisY, 1, 1, display, true, textConverters.Count);
            charSubmit = new charInputView(thisX, thisY + 1, 1, 1, display, true);
            savedCharCounts = new charGuessView(0, 2, 1, 1, display, true, alphabet.Length);
            display.addCanvas(guessOutput);
            display.addCanvas(charSubmit);
            display.addCanvas(savedCharCounts);

            myTreeView = new treeView(1,2,4,1,display, true, new dataNode("", 0));
            display.addCanvas(myTreeView);

        }

        //getting rid of this soon
        public void draw()
        {
            display.draw();
        }
        public void setupTrees()
        {
            List<Tuple<List<mPoint>, char>> elements = mDataManager.getElements();
            //for now we just are gonna use pos 0
            int[] counts = new int[alphabet.Length];
            for(int i=0; i<elements.Count; i++){
                counts[ Array.IndexOf(alphabet,  elements[i].Item2)]++;
            }
            for(int i=0; i<alphabet.Length; i++){
                savedCharCounts.updateGuess(i, alphabet[i] + " : " + counts[i]);
            }
            dataTrees[0].smartStart(elements, alphabet);
            /*
            for (int i = 0; i < dataTrees.Count; i++)
            {
                dataTrees[i].smartStart(elements, alphabet);
            }*/
            myTreeView.newTree(dataTrees[0].getRoot());
            display.draw();
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
                    display.draw();
                }
            }
        }

        public void submitData(char submitChar)
        {
            if (alphabet.Contains(submitChar))
            {
                display.clear();

                blockingData.CompleteAdding();
                addingData.Wait();

                //use current textConverter to get the cleaned data
                mDataManager.addData(new Tuple<List<mPoint>, char>(textConverters[0].getCleanedData(), submitChar));
                setupTrees();


                for (int i = 0; i < textConverters.Count; i++)
                {
                    textConverters[i].clear();
                }

                originalData.Clear();
                blockingData = new BlockingCollection<mPoint>();
                addingData = Task.Factory.StartNew(() => sendData(blockingData));

                display.draw();             
                
                
            }
        }

        private void clear()
        {
            display.clear();

            blockingData.CompleteAdding();
            addingData.Wait();

            for (int i = 0; i < textConverters.Count; i++)
            {
                textConverters[i].clear();
            }
            originalData.Clear();
            blockingData = new BlockingCollection<mPoint>();
            addingData = Task.Factory.StartNew(() => sendData(blockingData));

            display.draw();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            clear();
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
    }
}
