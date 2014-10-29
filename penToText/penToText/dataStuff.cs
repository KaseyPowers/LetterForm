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
        private mainWindows manager;

        private mSectionNode root;
        private mSectionNode2 root2;

        private String file = "PenToTextData.xml";
        
        public dataStuff(mainWindows myManager)
        {
            if (!File.Exists(file))
            {
                File.Create(file).Dispose();
            }
            manager = myManager;
            root = new mSectionNode("", 0.0, "");
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
                part2.FontFamily = new FontFamily("Courier New");
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
            if (cleanedData.Count > 1) { 
                if (currentChar != associatedLetter)
                {
                    pullData(associatedLetter);
                }
                currentChar = associatedLetter;
                elements.Add(new dataElement(associatedLetter, cleanedData));
                dataUpdated();
            }
        }

        public void dataUpdated()
        {
            root = new mSectionNode("", 0, "");
            root2 = new mSectionNode2("", 0, "", false);

            List<String> total = new List<String>();
            List<String> temp1 = new List<String>();
            List<String> temp2 = new List<String>();
            for (int i = 0; i < alphabet.Length; i++)
            {
                pullData(alphabet[i]);
                ((TextBlock)dataView[i].Children[0]).Text = alphabet[i] + ": " + elements.Count;
                temp1 = new List<String>();
                temp2 = new List<string>();
                for (int j = 0; j < elements.Count; j++)
                {
                    temp1.Add(new mLetterSections(minimumLines( cleanSections(Dominique(elements[j].cleanedData)))).getString(true));
                    temp2.Add(new mLetterSections(minimumLines(cleanSections(Dominique(elements[j].cleanedData)))).getString(false));
                }
                temp1 = temp1.Distinct().ToList();
                temp2 = temp2.Distinct().ToList();
                bool unique = true;
                // String uniqueText = "";
                ///tring repeatedText = "";
                String text = "";
                String text2 = "";
                int uniqueCount = 0;
                int nonUniqueCount = 0;
                int chunkSize = 6;

                for (int j = 0; j < temp1.Count; j++)
                {
                    addToTree2(temp1[j], alphabet[i]);
                    text2 += temp1[j] + "\n";
                    text = prettyOutput(temp1[j], text, chunkSize);
                    
                }
                chunkSize = 1;
                for (int j = 0; j < temp2.Count; j++)
                {
                   // text2 += temp2[j] + "\n";
                    //text = prettyOutput(temp2[j], text, chunkSize);
                }

                if (temp1.Count == 0)
                {
                    ((TextBlock)dataView[i].Children[2]).Text = "No Data";
                }
                else
                {
                    String thisText = text + "\n2:\n" + text2;
                    ((TextBlock)dataView[i].Children[2]).Text = thisText;
                }                
            }
            combineTree();
            manager.updateTree(root2);
        }

        public string prettyOutput(String input, String addingTo, int chunkLength)
        {
            List<String> asList= new List<String>();

            if(addingTo.Length !=0){
                asList = addingTo.Split(new string[] { "\n"}, StringSplitOptions.None).ToList();
            }
            /*for (int j = 0; j < asList.Count; j++)
            {
                    asList[j] = asList[j].Replace('-', ' ');
            }*/

            Boolean found = false;
            int chunkAt =0;
            int i = 0;
            String emptyChunk = "";
            String leftChunk = "";
            for (int j = 0; j < chunkLength; j++)
            {
                emptyChunk += " ";
            }
            while (i < asList.Count && !found)
            {
                Boolean stringsLongEnough = asList[i].Length >= (chunkAt+1) * chunkLength && input.Length >= (chunkAt+1) * chunkLength;
                if (stringsLongEnough)
                {
                    String currentChunk = asList[i].Substring(chunkAt * chunkLength, chunkLength).Replace('-', ' ');
                    String searchString = input.Substring(chunkAt * chunkLength, chunkLength);
                    Boolean sameChunk = currentChunk.Equals(searchString);

                    if (currentChunk.Equals(emptyChunk))
                    {
                        i++;
                    }
                    else
                    {
                        if (chunkAt != 0)
                        {
                            String previousChunk = asList[i].Substring((chunkAt - 1) * chunkLength, chunkLength).Replace('-', ' ');
                            //String leftChunk = asList[i].Substring(chunkAt-1)
                            if (!(previousChunk.Equals(leftChunk) || previousChunk.Equals(emptyChunk)))
                            {
                                found = true;
                            }

                        }

                        if (!found)
                        {
                            if (sameChunk)
                            {
                                chunkAt++;
                                leftChunk = currentChunk;
                            }
                            else
                            {
                                i++;
                            }
                        }
                    }
                }
                else
                {
                    if (chunkAt == 0)
                    {
                        i++;
                    }
                    else
                    {
                        if (input.Length >= asList[i].Length)
                        {
                            i++;
                        }
                    }
                    found = true;
                }
            }
            if (!found)
            {
                asList.Add(makeLine(input, chunkLength, chunkAt));
            }
            else
            {
                asList.Insert(i, makeLine(input, chunkLength, chunkAt));
            }
            String output="";
            for (int j = 0; j < asList.Count; j++){
                output += asList[j];
                if (j != asList.Count-1)
                {
                    output += "\n";
                }                
            }
            return output;
        }

        public void addToTree2(String newInfo, char associatedLetter)
        {
            int chunkAt = 0;
            int chunkLength = 6;
            mSectionNode2 current = root2;
            while (newInfo.Length >= (chunkAt + 1) * chunkLength)
            {
                string chunk = newInfo.Substring(chunkAt * chunkLength, chunkLength);
                string sectionString;
                double value = 0.0;
                if (chunk.Equals("Line00"))
                {
                    sectionString = chunk;
                }
                else
                {
                    sectionString = chunk.Substring(0, 1);
                    value = Double.Parse(chunk.Substring(1));
                }

                bool final = (newInfo.Length < (chunkAt + 2) * chunkLength);
                mSectionNode2 next = new mSectionNode2(sectionString, value, associatedLetter+"", final);

                current.addChild(next);
                current = next;
                chunkAt++;
            }

        }

        public void combineTree()
        {
            Queue<mSectionNode2> frontier = new Queue<mSectionNode2>();
            frontier.Enqueue(root2);

            while (frontier.Count > 0)
            {
                mSectionNode2 current = frontier.Dequeue();
                List<mSectionNode2> currentChildren = current.children;
                List<mSectionNode2> nextCildren = new List<mSectionNode2>();
                bool hasChanged = true;
                while (hasChanged)
                {
                    hasChanged = false;
                    int foundLoc = -1;
                    for (int i = 0; i < currentChildren.Count; i++)
                    {
                        bool foundMatch = false;
                        for (int j = 0; j < currentChildren.Count && !foundMatch && !hasChanged; j++)
                        {
                            if (i != j && currentChildren[i].canComabine(currentChildren[j]))
                            {
                                hasChanged = true;
                                foundMatch = true;
                                foundLoc = j;
                                nextCildren.Add(new mSectionNode2(currentChildren[i], currentChildren[j]));
                            }
                        }
                        if (!foundMatch && foundLoc != i)
                        {
                            nextCildren.Add(currentChildren[i]);
                        }
                    }
                    /*for (int i = 1; i < currentChildren.Count; i++)
                    {
                        if (currentChildren[i - 1].canComabine(currentChildren[i]))
                        {
                            hasChanged = true;
                            nextCildren.Add(new mSectionNode2(currentChildren[i - 1], currentChildren[i]));
                            i++;
                        }
                    }*/
                    currentChildren = new List<mSectionNode2>(nextCildren);
                    nextCildren = new List<mSectionNode2>();
                }

                current.children = currentChildren;

                for (int i = 0; i < current.children.Count; i++)
                {
                    frontier.Enqueue(current.children[i]);
                }
            }
        }

        public void addToTree(String newInfo, char associatedLetter)
        {
            int chunkAt = 0;
            int chunkLength = 6;
            mSectionNode current = root;
            bool toContinue = true;
            while(toContinue){
                if (newInfo.Length >= (chunkAt + 1) * chunkLength)
                {
                    string chunk = newInfo.Substring(chunkAt * chunkLength, chunkLength);
                    string sectionString;
                    double value = 0.0;
                    if (chunk.Equals("Line00"))
                    {
                        sectionString = chunk;
                    }
                    else
                    {
                        sectionString = chunk.Substring(0, 1);
                        value = Double.Parse(chunk.Substring(1));
                    }

                    current.addChar(associatedLetter);
                    bool found = false;
                    for (int i = 0; i < current.children.Count && !found; i++)
                    {
                        mSectionNode child = current.children[i];
                        if (child.SectionLetter.Equals(sectionString) && child.SectoinValue == value)
                        {
                            child.addChar(associatedLetter);
                            current = child;
                            chunkAt++;
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        mSectionNode child = new mSectionNode(sectionString, value, ""+associatedLetter);
                        current.addChild(child);
                        current = child;
                        chunkAt++;
                    }
                }
                else
                {
                    current.addFinalChar(associatedLetter);
                    toContinue = false;
                }
            }
        }

        public String makeLine(String input, int chunkSize, int chunkAt)
        {
            String output = "";
            for (int i = 0; i < chunkAt * chunkSize; i++)
            {
                if (i != chunkAt * chunkSize - 1)
                {
                    output += " ";
                }
                else
                {
                    output += "-";
                }
            }
            if (input.Length >= (chunkAt + 1) * chunkSize)
            {
                output += input.Substring(chunkAt*chunkSize);
            }
            else
            {
                output += input;
            }
            return output;
        }

        public List<mPoint> Dominique(List<mPoint> input)
        {
            List<mPoint> output = new List<mPoint>();
            if (input.Count > 2)
            {
                int sLoc = 0;
                output.Add(input[0]);
                for (int i = 0; i < (input.Count - 1); i++)
                {
                    bool sameSlope = input[sLoc].line == input[i + 1].line && getDirection(input[sLoc], input[sLoc + 1]) == getDirection(input[i], input[i + 1]) && input[sLoc].line == input[i].line;
                    if (!sameSlope)
                    {
                        output.Add(input[i]);
                        sLoc = i;
                    }
                }
                output.Add(input[input.Count - 1]);
            }
            else
            {
                output = input;
            }

            return output;
        }

        public List<mPoint> minimumLines(List<mPoint> input)
        {
            int linePoints = 0;
            for (int i = 1; i < input.Count; i++)
            {
                if (input[i - 1].line == input[i].line)
                {
                    linePoints++;
                }
                else
                {
                    if (linePoints == 0)
                    {
                        //do something
                        input.Insert(i, new mPoint(input[i].X, input[i].Y + .001, input[i].line));
                    }
                    else
                    {
                        linePoints = 0;
                        if (i == (input.Count - 1))
                        {
                            input.Add(new mPoint(input[i].X, input[i].Y + .001, input[i].line));
                        }
                    }
                }
            }
            return input;
        }

        public List<mPoint> cleanSections(List<mPoint> input)
        {
            double e = .2; // height over length
            for (int i = 2; i < input.Count; i++)
            {
                if (input[i - 2].line == input[i - 1].line && input[i - 1].line == input[i].line)
                {
                    double thisLineDistance = lineDistance(input[i - 2], input[i], input[i - 1]);
                    double lineLength = distance(input[i - 2], input[i]);
                    //lineLength = lineLength * lineLength;
                    if (thisLineDistance / lineLength <= e)
                    {
                        input.RemoveAt(i - 1);
                        i--;
                    }
                }
            }
            return input;
        }

        private double dotProduct(Point a, Point b)
        {
            return ((a.X * b.X) + (a.Y * b.Y));
        }


        private double lineDistance(mPoint a, mPoint b, mPoint c)
        {
            //a and b make line, return distance c from line
            Point u = new Point(b.X - a.X, b.Y - a.Y);
            Point v = new Point(c.X - a.X, c.Y - a.Y);
            double value = dotProduct(u, v) / dotProduct(u, u);
            Point p = new Point(u.X * value + a.X, u.Y * value + a.Y);
            double output = double.PositiveInfinity;
            if (value >= 0.0 && value <= 1.0)
            {
                output = distance(c, new mPoint(p, 0));
            }
            return output;

        }

        private double distance(mPoint a, mPoint b)
        {
            double deltaX = b.X - a.X;
            double deltaY = b.Y - a.Y;
            return Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        private int getDirection(mPoint startPoint, mPoint endPoint)
        {
            /*
             * 0: up
             * 1: down
             * 2: left
             * 3: right
             * 4: up-left
             * 5: up-right
             * 6: down-left
             * 7: down-right
             */
            int direction = -1;
            double deltaX = xChange(startPoint, endPoint);
            double deltaY = yChange(startPoint, endPoint);

            if (deltaY < 0)
            {
                //up
                if (deltaX > 0)
                {
                    //right
                    direction = 5;
                }
                else
                {
                    //left
                    direction = 4;
                }
            }
            else
            {
                //down
                if (deltaX > 0)
                {
                    //right
                    direction = 7;
                }
                else
                {
                    //left
                    direction = 6;
                }
            }

            return direction;
        }

        private double xChange(mPoint a, mPoint b)
        {

            return (a.X - b.X);
        }

        private double yChange(mPoint a, mPoint b)
        {
            return (a.Y - b.Y);
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
            foreach (dataElement current in elements)
            {
                myXmlDocument.DocumentElement.AppendChild(current.addNodeForElement(myXmlDocument));
            }
            elements.Clear();
            /*while (elements.Count != 0)
            {
                dataElement current = elements[0];
                elements.RemoveAt(0);

                myXmlDocument.DocumentElement.AppendChild(current.addNodeForElement(myXmlDocument));
            }*/
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
                    element.ParentNode.RemoveChild(element);
                }
            }
            myXmlDocument.Save(file);
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
