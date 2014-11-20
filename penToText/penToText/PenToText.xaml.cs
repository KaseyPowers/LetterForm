using System;
using System.ComponentModel;
using System.Windows;

namespace penToText
{
    /// <summary>
    /// Interaction logic for PenToText.xaml
    /// </summary>
    public partial class PenToText : Window
    {
        Core myCore;
        public PenToText(Core core)
        {
            myCore = core;
            InitializeComponent();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            myCore.closing();
        }

    }
}
