using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace penToText
{
    class dataManager
    {
        private List<Tuple<List<mPoint>, char>> elements;
        Core core;
        bool dataPulled = false;


        private String fileLoc = "textConverter.xml";

        public dataManager(Core core)
        {
            this.core = core;
            if (!File.Exists(fileLoc))
            {
                File.Create(fileLoc).Dispose();
            }
            elements = new List<Tuple<List<mPoint>, char>>();
            dataPulled = false;
        }

        public List<Tuple<List<mPoint>, char>> getElements()
        {
            if (!dataPulled)
            {
                pullData();
            }

            return elements;
        }

        public void addData(Tuple<List<mPoint>, char> newElement){
            elements.Add(newElement);
            writeData();
            pullData();
        }
        private Tuple<List<mPoint>, char> getDataElement(XmlElement xmlement){
            char associatedChar = xmlement.Attributes[0].InnerText.ToCharArray()[0];
            List<mPoint> data = new List<mPoint>();
            if (xmlement.HasChildNodes)
            {
                XmlNodeList points = xmlement.ChildNodes;

                foreach (XmlNode point in points)
                {
                    mPoint temp = new mPoint(Double.Parse(point.Attributes[0].Value), Double.Parse(point.Attributes[1].Value), int.Parse(point.Attributes[2].Value));
                    data.Add(temp);
                }
            }
            return new Tuple<List<mPoint>,char>(data, associatedChar);
        }

        private XmlElement getXmlElement(Tuple<List<mPoint>, char> dataElement, XmlDocument thisDoc){
            XmlElement output = thisDoc.CreateElement("DataElement");

            output.SetAttribute("Char", "" + dataElement.Item2);

            foreach (mPoint point in dataElement.Item1)
            {
                XmlElement pointNode = thisDoc.CreateElement("Point");
                pointNode.SetAttribute("X", point.X + "");
                pointNode.SetAttribute("Y", "" + point.Y);
                pointNode.SetAttribute("Line", "" + point.line);
                output.AppendChild(pointNode);
            }
            return output;
        }

        public void pullData(){
            dataPulled = true;
            if (elements.Count != 0)
            {
                writeData();
            }
            XmlDocument myXmlDocument = new XmlDocument();
            try
            {
                myXmlDocument.Load(fileLoc);
            }
            catch (XmlException exception)
            {
                //file not setup
                Console.WriteLine("Exception Reading XML: " + exception.ToString());
                XmlDeclaration dec = myXmlDocument.CreateXmlDeclaration("1.0", null, null);
                myXmlDocument.AppendChild(dec);
                myXmlDocument.AppendChild(myXmlDocument.CreateElement("myData"));
            }

            XmlNodeList currentLetters = myXmlDocument.DocumentElement.SelectNodes("DataElement");
            foreach (XmlElement element in currentLetters)
            {
                elements.Add( getDataElement(element));
                element.ParentNode.RemoveChild(element);
            }
            myXmlDocument.Save(fileLoc);
        }

        public void writeData()
        {
            XmlDocument myXmlDocument = new XmlDocument();
            try
            {
                myXmlDocument.Load(fileLoc);
            }
            catch (XmlException exception)
            {
                //file not setup
                Console.WriteLine("Exception Reading XML: " + exception.ToString());
                XmlDeclaration dec = myXmlDocument.CreateXmlDeclaration("1.0", null, null);
                myXmlDocument.AppendChild(dec);
                myXmlDocument.AppendChild(myXmlDocument.CreateElement("myData"));
            }
            foreach (Tuple<List<mPoint>,char> current in elements)
            {
                myXmlDocument.DocumentElement.AppendChild(getXmlElement(current, myXmlDocument));
            }
            elements.Clear();
            myXmlDocument.Save(fileLoc);
            dataPulled = false;
        }

    }
}
