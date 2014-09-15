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
        public dynamicView dynamicInput;
        public dynamicView arrows;
        public dynamicDisplay display;

        public DisplayWindow(dynamicDisplay display)
        {
            InitializeComponent();
            this.display=display;
            canvasSize = new Size();
            canvasSize.Height = 300;
            canvasSize.Width = 300;
            this.Content = display.getGrid();

            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Show();
        }

        internal void resize()
        {
           //todo: read this
        }
    }   
}
