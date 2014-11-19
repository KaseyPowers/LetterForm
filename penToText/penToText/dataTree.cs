using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace penToText
{
    class dataTree
    {
        dataNode root;
        textConverter currentConverter;
        public dataTree(textConverter converter){
            currentConverter = converter;
            root = new dataNode("", 0, ' ', false);
        }
        public void addToTree(dataNode start, List<mPoint> input, char associatedLetter)
        {
            String newInfo = currentConverter.getSectionString(input);
            int chunkAt = 0;
            int chunkLength = 6;
            dataNode current = start;
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
                dataNode next = new dataNode(sectionString, value, associatedLetter, final);

                current.addChild(next);
                current = next;
                chunkAt++;
            }
        }
        public void addToThisTree(dataNode start, string newInfo, char associatedLetter)
        {
            int chunkAt = 0;
            int chunkLength = 6;
            dataNode current = start;
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
                dataNode next = new dataNode(sectionString, value, associatedLetter, final);

                current.addChild(next);
                current = next;
                chunkAt++;
            }
        }

        public void combineTree(dataNode start)
        {
            Queue<dataNode> frontier = new Queue<dataNode>();
            frontier.Enqueue(start);

            while (frontier.Count > 0)
            {
                dataNode current = frontier.Dequeue();
                List<dataNode> currentChildren = current.children;
                List<dataNode> nextCildren = new List<dataNode>();
                bool hasChanged = true;
                while (hasChanged)
                {
                    hasChanged = false;
                    int foundLoc = -1;
                    for (int i = 0; i < currentChildren.Count; i++)
                    {
                        bool foundMatch = false;
                        List<int> possibleLocs = new List<int>();
                        for (int j = 0; j < currentChildren.Count && !hasChanged; j++)
                        {
                            if (i != j && currentChildren[i].canComabine(currentChildren[j]))
                            {
                                //hasChanged = true;
                                foundMatch = true;
                                possibleLocs.Add(j);
                                //nextCildren.Add(new mSectionNode2(currentChildren[i], currentChildren[j]));
                            }
                        }
                        if (!foundMatch && i != foundLoc)
                        {
                            nextCildren.Add(currentChildren[i]);
                        }
                        else if (possibleLocs.Count > 0)
                        {
                            hasChanged = true;
                            int bestLoc = 0;
                            for (int j = 1; j < possibleLocs.Count; j++)
                            {
                                //compare values
                                if (currentChildren[i].bestCombine(currentChildren[possibleLocs[bestLoc]], currentChildren[possibleLocs[j]]) < 0)
                                {
                                    bestLoc = j;
                                }
                            }
                            foundLoc = possibleLocs[bestLoc];
                            nextCildren.Add(new dataNode(currentChildren[i], currentChildren[possibleLocs[bestLoc]]));
                        }
                    }
                    currentChildren = new List<dataNode>(nextCildren);
                    nextCildren = new List<dataNode>();
                }

                current.children = currentChildren;

                for (int i = 0; i < current.children.Count; i++)
                {
                    frontier.Enqueue(current.children[i]);
                }
            }
        }
    }

    public class dataNode
    {
        public dataNode parent;
        public List<dataNode> children;
        public String SectionLetter;
        public double minValue;
        public double maxValue;
        public String chars;
        public char ifStopHere;

        public dataNode(String letter, double value, char newChar , bool final)
        {
            parent = null;
            children = new List<dataNode>();
            SectionLetter = letter;
            minValue = value;
            maxValue = value;
            chars = "";
            ifStopHere = ' ';
            if (final)
            {
                addFinalChar(newChar);               
            }
            else
            {
                //addChar(newChar);  
            }
        }

        public bool canComabine(dataNode other)
        {
            bool main = (this.parent == other.parent) && (this.SectionLetter.Equals(other.SectionLetter));
            bool rangesMatch = ((this.minValue >= other.minValue && this.maxValue <= other.maxValue) 
                ||  (this.minValue <= other.minValue && this.maxValue >= other.maxValue));
            //bool matchingEndPoint = ifStopHere.Equals(other.ifStopHere) && ifStopHere.Length>0;
            bool aCharContainB = true;
            bool bCharContainA = true;
            for (int i = 0; i < other.chars.Length && aCharContainB; i++)
            {
                aCharContainB = chars.Contains(other.chars[i]);
            }
            for (int i = 0; i < chars.Length && bCharContainA; i++)
            {
                bCharContainA = other.chars.Contains(chars[i]);
            }
            bool validChars = aCharContainB || bCharContainA;
            bool validRoots = true;
            if (ifStopHere != ' ' && other.ifStopHere != ' ')
            {
                validRoots = ifStopHere == other.ifStopHere;
            }
            return main && (rangesMatch ||validChars) && validRoots;
        }

        public int bestCombine(dataNode a, dataNode b)
        {
            int output = 0;

            for (int i = 0; i < chars.Length; i++){
                if (a.chars.Contains(chars[i]))
                {
                    output++;
                }
                if (b.chars.Contains(chars[i]))
                {
                    output--;
                }
            }

            if (a.ifStopHere == ifStopHere)
            {
                output++;
            }
            else if(b.ifStopHere == ifStopHere)
            {

            }

            return output;
        }

        public dataNode(dataNode a, dataNode b)
        {
            this.parent = a.parent;
            this.SectionLetter = a.SectionLetter;
            if (a.minValue < b.minValue)
            {
                this.minValue = a.minValue;
            }
            else
            {
                this.minValue = b.minValue;
            }

            if (a.maxValue > b.maxValue)
            {
                this.maxValue = a.maxValue;
            }
            else
            {
                this.maxValue = b.maxValue;
            }

            chars = "";
            for (int i = 0; i < a.chars.Length; i++)
            {
                addChar(a.chars[i]);
            }
            for (int i = 0; i < b.chars.Length; i++)
            {
                addChar(b.chars[i]);
            }

            ifStopHere = ' ';
            addFinalChar(a.ifStopHere);
            addFinalChar(b.ifStopHere);

            children = new List<dataNode>();
            for (int i = 0; i < a.children.Count; i++)
            {
                children.Add(a.children[i]);
            }
            for (int i = 0; i < b.children.Count; i++)
            {
                children.Add(b.children[i]);
            }
            for (int i = 0; i < children.Count; i++)
            {
                children[i].parent = this;
            }
        }

        public void addChild(dataNode child)
        {
            child.parent = this;
            children.Add(child);
        }

        public void addChar(char newChar)
        {
            if (!chars.Contains(newChar))
            {
                chars += newChar;
                if(parent != null){
                    parent.addChar(newChar);
                }
            }
        }


        public void addFinalChar(char newChar)
        {
            if (ifStopHere==' ')
            {
                ifStopHere = newChar;
                addChar(newChar);
            }
        }        
    }
}
