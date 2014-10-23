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
using System.Xml;

namespace penToText
{
    public class dataStuff
    {
        private ScrollViewer content;
        private StackPanel container;
        private List<StackPanel> dataView;
        private char[] alphabet = "ABCDEFGHIJKLMNOP0123456789".ToCharArray();
        private List<dataElement> elements;

        private String file = "PenToTextData.xml";
        
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

            getData();
        }
        public ScrollViewer getContent()
        {
            return content;
        }
        public void Submit(List<mPoint> cleanedData, char associatedLetter)
        {
            elements.Add(new dataElement(associatedLetter, cleanedData));
        }

        public void dataUpdated()
        {

        }

        public void getData()
        {
            elements = new List<dataElement>();
            //pupulate from xml
        }

        public bool writeData()
        {
            bool output = true;
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter Writer = XmlWriter.Create(file, settings);
            Writer.WriteStartDocument();
            Writer.WriteComment("This is written by the program");
            for (int i = 0; i < elements.Count; i++)
            {
                Writer.WriteStartElement("DataElement");
                dataElement current = elements[i];
                Writer.WriteAttributeString("Char" , current.associatedCharacter+"");
                for(int j=0; j<current.cleanedData.Count; j++){
                    Writer.WriteStartElement("Point");
                    Writer.WriteAttributeString("X", current.cleanedData[j].X + "");
                    Writer.WriteAttributeString("Y", current.cleanedData[j].Y + "");                    
                    Writer.WriteAttributeString("L", current.cleanedData[j].line + "");
                    Writer.WriteEndElement();
                }
                Writer.WriteEndElement();
            }
            if (elements.Count == 0)
            {
                Writer.WriteStartElement("NoDataYet");
                Writer.WriteEndElement();
            }
            Writer.WriteEndDocument();

            Writer.Flush();
            Writer.Close();

            return output;
        }
    }

    public class dataElement
    {
        public char associatedCharacter;
        public List<mPoint> cleanedData;

        public dataElement(char thisChar, List<mPoint> data)
        {
            cleanedData = data;
            associatedCharacter = thisChar;
        }
    }
}
