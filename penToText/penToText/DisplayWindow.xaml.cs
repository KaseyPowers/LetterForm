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
            Closing += mainWindows.OnDisplayWindowClose;
            InitializeComponent();
            this.display=display;
            this.Content = display.getScrollView();

            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Show();
        }

        internal void resize()
        {
           //todo: read this
        }
    }   
}
