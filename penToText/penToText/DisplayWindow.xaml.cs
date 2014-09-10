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
        public Canvas InputCanvas;
        public Canvas ArrowCanvas;

        public DisplayWindow()
        {
            InitializeComponent();
            canvasSize = new Size();
            canvasSize.Height = 300;
            canvasSize.Width = 300;

            Grid DynamicGrid = new Grid();
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

            InputCanvas = new Canvas();
            InputCanvas.Name = "InputCopy";
            InputCanvas.Width = canvasSize.Width;
            InputCanvas.Height = canvasSize.Height;
            InputCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            InputCanvas.VerticalAlignment = VerticalAlignment.Top;

            Grid.SetRow(InputCanvas, 0);
            Grid.SetColumn(InputCanvas, 0);

            ArrowCanvas = new Canvas();
            ArrowCanvas.Name = "ArrowCanvas";
            ArrowCanvas.Width = canvasSize.Width;
            ArrowCanvas.Height = canvasSize.Height;
            ArrowCanvas.HorizontalAlignment = HorizontalAlignment.Left;
            ArrowCanvas.VerticalAlignment = VerticalAlignment.Top;

            Grid.SetRow(ArrowCanvas, 0);
            Grid.SetColumn(ArrowCanvas, 1);

            DynamicGrid.Children.Add(InputCanvas);
            DynamicGrid.Children.Add(ArrowCanvas);
            this.Content = DynamicGrid;

            this.SizeToContent = SizeToContent.WidthAndHeight;
        }

        internal void resize()
        {
            InputCanvas.Width = canvasSize.Width;
            InputCanvas.Height = canvasSize.Height;

            ArrowCanvas.Width = canvasSize.Width;
            ArrowCanvas.Height = canvasSize.Height;
        }
    }
}
