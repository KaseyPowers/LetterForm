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
    /// Interaction logic for DataDisplay.xaml
    /// </summary>
    public partial class DataDisplay : Window
    {
        private dataStuff myDataStuff;
        public DataDisplay(dataStuff myDataStuff)
        {
            InitializeComponent();

            this.myDataStuff = myDataStuff;
            this.Content = myDataStuff.getContent();
            this.ShowActivated = false;
            this.Show();
        }
    }
}
