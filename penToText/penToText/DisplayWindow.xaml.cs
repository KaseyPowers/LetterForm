using System;
using System.Collections.Generic;
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
    /// Interaction logic for DisplayWindow.xaml
    /// </summary>
    public partial class DisplayWindow : Window
    {
        public Size canvasSize;
        public mainWindows manager;
        public dynamicCanvas dynamicInput;
        public dynamicCanvas arrows;
        public dynamicDisplay display;

        public DisplayWindow()
        {
            InitializeComponent();
            canvasSize = new Size();
            canvasSize.Height = 300;
            canvasSize.Width = 300;

            display = new dynamicDisplay();

            Canvas InputCanvas = new Canvas();
            InputCanvas.Name = "InputCopy";
            InputCanvas.Width = canvasSize.Width;
            InputCanvas.Height = canvasSize.Height;
            InputCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            InputCanvas.VerticalAlignment = VerticalAlignment.Top;

            dynamicInput = new TempCanvas(InputCanvas, 0, 0, display);
            display.addCanvas(dynamicInput);


            Canvas ArrowCanvas = new Canvas();
            ArrowCanvas.Name = "ArrowCanvas";
            ArrowCanvas.Width = canvasSize.Width;
            ArrowCanvas.Height = canvasSize.Height;
            ArrowCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            ArrowCanvas.VerticalAlignment = VerticalAlignment.Top;

            arrows = new TempCanvas(ArrowCanvas, 1, 0, display);
            display.addCanvas(arrows);


            this.Content = display.getGrid();


            this.SizeToContent = SizeToContent.WidthAndHeight;

            /*Grid DynamicGrid = new Grid();
            DynamicGrid.HorizontalAlignment = HorizontalAlignment.Left;
            DynamicGrid.VerticalAlignment = VerticalAlignment.Top;
            DynamicGrid.ShowGridLines = true;

            // Create Columns
            ColumnDefinition column0 = new ColumnDefinition();
            ColumnDefinition column1 = new ColumnDefinition();
            DynamicGrid.ColumnDefinitions.Add(column0);
            DynamicGrid.ColumnDefinitions.Add(column1);

            // Create Rows
            RowDefinition row0 = new RowDefinition();
            DynamicGrid.RowDefinitions.Add(row0);

           

            Grid.SetRow(ArrowCanvas, 0);
            Grid.SetColumn(ArrowCanvas, 1);

            DynamicGrid.Children.Add(InputCanvas);
            DynamicGrid.Children.Add(ArrowCanvas);
            this.Content = DynamicGrid;


            this.SizeToContent = SizeToContent.WidthAndHeight;*/
        }

        internal void resize()
        {
            dynamicInput.myCanvas.Width = canvasSize.Width;
            dynamicInput.myCanvas.Height = canvasSize.Height;

            arrows.myCanvas.Width = canvasSize.Width;
            arrows.myCanvas.Height = canvasSize.Height;
        }
    }

    public class dynamicDisplay

    {

        private Grid flexibleGrid;
        int x_grid;
        int y_grid;
        List<dynamicCanvas> canvases;


        public dynamicDisplay()
        {
            canvases = new List<dynamicCanvas>();

            x_grid = 1;
            y_grid = 1;

            flexibleGrid = new Grid();
            flexibleGrid.ShowGridLines = true;
            flexibleGrid.HorizontalAlignment = HorizontalAlignment.Left;
            flexibleGrid.VerticalAlignment = VerticalAlignment.Top;
            updateGrid();
        }

        public Grid getGrid()
        {
            return flexibleGrid;
        }

        private void updateGrid()
        {
            //start from scratch
            flexibleGrid.Children.Clear();
            flexibleGrid.ColumnDefinitions.Clear();
            flexibleGrid.RowDefinitions.Clear();

            //create enough columns and rows
            for (int x = 0; x <= x_grid; x++)
            {
                ColumnDefinition newColumn = new ColumnDefinition();
                flexibleGrid.ColumnDefinitions.Add(newColumn);
            }

            for (int y = 0; y <= y_grid; y++)
            {
                RowDefinition newRow = new RowDefinition();
                flexibleGrid.RowDefinitions.Add(newRow);
            }

            //puts the canvases in location
            foreach(dynamicCanvas current in canvases)
            {
                Grid.SetColumn(current.myCanvas, current.xPos);
                Grid.SetRow(current.myCanvas, current.yPos);

                flexibleGrid.Children.Add(current.myCanvas);
            }
        }

        public void canvasLocChanged()
        {
            x_grid = 0;
            y_grid = 0;
            foreach (dynamicCanvas canvas in canvases)
            {
                if (canvas.xPos > x_grid)
                {
                    x_grid = canvas.xPos;
                }
                if (canvas.yPos > y_grid)
                {
                    y_grid = canvas.yPos;
                }
            }
            updateGrid();
        }

        public void addCanvas(dynamicCanvas canvas)
        {
            canvases.Add(canvas);
            if (canvas.xPos > x_grid) 
            { 
                x_grid = canvas.xPos;
            }
            if (canvas.yPos > y_grid)
            {
                y_grid = canvas.yPos;
            }
            updateGrid();
        }

        
    }

    public abstract class dynamicCanvas
    {
        //thats it for now
        public Canvas myCanvas;
        public int xPos;
        public int yPos;
        protected dynamicDisplay parent;

        public void changeLoc(int xPos, int yPos)
        {
            this.xPos = xPos;
            this.yPos = yPos;
            parent.canvasLocChanged();
        }

    }

    public class TempCanvas : dynamicCanvas
    {
        public TempCanvas(Canvas canvas, int xPos, int yPos, dynamicDisplay parent)
        {
            myCanvas = canvas;
            this.xPos = xPos;
            this.yPos = yPos;
            this.parent = parent;
        }

    }
}
