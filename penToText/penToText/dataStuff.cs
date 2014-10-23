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
    public class dataStuff
    {
        private ScrollViewer content;
        private StackPanel container;
        private List<StackPanel> dataView;
        private char[] alphabet = "ABCDEFGHIJKLMNOP0123456789".ToCharArray();
        public dataStuff()
        {
            dataView = new List<StackPanel>();
            content = new ScrollViewer();
            content.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            container = new StackPanel();
            container.Orientation = Orientation.Vertical;
            content.Content = container;
            for (int i = 0; i < alphabet.Length; i++)
            {
                Border currentBorder = new Border();
                currentBorder.BorderBrush = Brushes.Black;
                currentBorder.BorderThickness = new Thickness(1);

                StackPanel current = new StackPanel();
                current.Orientation = Orientation.Horizontal;
                TextBlock part1 = new TextBlock();
                part1.Margin = new Thickness(10, 10, 10, 10);
                part1.MinWidth = 30;
                part1.FontSize = 12;
                part1.Text = alphabet[i] + " : 0";
                TextBlock part2 = new TextBlock();
                part2.Margin = new Thickness(10, 10, 10, 10);
                part2.FontSize = 12;
                part2.Text = "Something for the breakdown";

                Rectangle toSeperate = new Rectangle();
                toSeperate.VerticalAlignment = VerticalAlignment.Stretch;
                toSeperate.Width = 2;
                toSeperate.Stroke = Brushes.Black;

                current.Children.Add(part1);
                current.Children.Add(toSeperate);
                current.Children.Add(part2);

                dataView.Add(current);

                currentBorder.Child = current;
                container.Children.Add(currentBorder);
            }
        }
        public ScrollViewer getContent()
        {
            return content;
        }
        public void Submit(List<Point> cleanedData, char associatedLetter)
        {

        }

        public void getData()
        {

        }
    }
}
