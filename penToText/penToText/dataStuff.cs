using System;
using System.IO;
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
        private char[] alphabet = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
        private List<dataElement> elements;
        private char currentChar;

        private String file = "PenToTextData.xml";
        
        public dataStuff()
        {
            if (!File.Exists(file))
            {
                File.Create(file);
            }
            currentChar = ' ';
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
            if (currentChar != associatedLetter)
            {
                pullData(associatedLetter);
            }
            currentChar = associatedLetter;
            elements.Add(new dataElement(associatedLetter, cleanedData));
            dataUpdated();
        }

        public void dataUpdated()
        {
            for (int i = 0; i < alphabet.Length; i++)
            {
                pullData(alphabet[i]);
                ((TextBlock)dataView[i].Children[0]).Text = alphabet[i] + ": " + elements.Count;
            }
        }

        public void getData()
        {
            elements = new List<dataElement>();
            //pupulate from xml
            dataUpdated();
        }

        public bool writeData()
        {
            bool output = true;
            /*XmlWriterSettings settings = new XmlWriterSettings();
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
            Writer.Close();*/
            writeNewData();

            return output;
        }


        public void writeNewData()
        {
            XmlDocument myXmlDocument = new XmlDocument();
            try
            {
                myXmlDocument.Load(file);
            }
            catch (XmlException exception)
            {
                //file not setup
                XmlDeclaration dec = myXmlDocument.CreateXmlDeclaration("1.0", null, null);
                myXmlDocument.AppendChild(dec);
                myXmlDocument.AppendChild(myXmlDocument.CreateElement("myData"));
            }

            while (elements.Count != 0)
            {
                dataElement current = elements[0];
                elements.RemoveAt(0);

                myXmlDocument.DocumentElement.AppendChild(current.addNodeForElement(myXmlDocument));
            }
            myXmlDocument.Save(file);
        }

        public void pullData(char toPull)
        {
            if (elements.Count != 0)
            {
                writeNewData();
            }
            XmlDocument myXmlDocument = new XmlDocument();
            try
            {
                myXmlDocument.Load(file);
            }
            catch (XmlException exception)
            {
                //file not setup
                XmlDeclaration dec = myXmlDocument.CreateXmlDeclaration("1.0", null, null);
                myXmlDocument.AppendChild(dec);
                myXmlDocument.AppendChild(myXmlDocument.CreateElement("myData"));
            }
            
            XmlNodeList currentLetters = myXmlDocument.DocumentElement.SelectNodes("DataElement");
            foreach(XmlElement element in currentLetters)
            {
                //XmlNode node = currentLetters[i];
                //char letter = node.Attributes[0].Value.ToCharArray()[0];

                /*string innerText = currentLetters[i].InnerText;
                XmlElement element = myXmlDocument.CreateElement("DataElement");
                element.SetAttribute("Char", currentLetters[i].Attributes[0].Value);
                element.InnerText = innerText;
                //make data element from node
                */
                if (element.GetAttribute("Char").ToCharArray()[0] == toPull)
                {
                    elements.Add(new dataElement(element));
                    myXmlDocument.DocumentElement.RemoveChild(element);
                }
            }
            
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

        public dataElement(XmlElement buildFrom)
        {
            associatedCharacter = buildFrom.Attributes[0].InnerText.ToCharArray()[0];
            cleanedData = new List<mPoint>();
            if (buildFrom.HasChildNodes)
            {
                XmlNodeList points = buildFrom.ChildNodes;

                foreach (XmlNode point in points)
                {
                    mPoint temp = new mPoint(Double.Parse(point.Attributes[0].Value), Double.Parse(point.Attributes[1].Value), int.Parse(point.Attributes[2].Value));
                    cleanedData.Add(temp);
                }
            }
        }

        public XmlElement addNodeForElement(XmlDocument myXmlDoc)
        {
            XmlElement output = myXmlDoc.CreateElement("DataElement");

            output.SetAttribute("Char", ""+associatedCharacter);

            foreach(mPoint point in cleanedData){
                XmlElement pointNode = myXmlDoc.CreateElement("Point");
                pointNode.SetAttribute("X", point.X+"");
                pointNode.SetAttribute("Y", "" + point.Y);
                pointNode.SetAttribute("Line", "" + point.line);
                output.AppendChild(pointNode);
            }

            return output;
        }
    }
}
