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
    /// Interaction logic for submitPopup.xaml
    /// </summary>
    public partial class submitPopup : Window
    {
        private InputWindow myInput;
        public submitPopup(InputWindow inputWindow)
        {
            myInput = inputWindow;
            InitializeComponent();
        }

        private void Set_Letter_Click(object sender, RoutedEventArgs e)
        {
            string start = LetterSet.Text;
            if (start.Length == 1)
            {
                char output = start[0];

                myInput.Title = "Input Window-" + output;
                myInput.setSubmitChar(output);
                this.Close();
            }
        }

        private void Cancel_Letter_Change(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
