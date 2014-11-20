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
            currentConverter.setTree(this);
            root = new dataNode("", 0, ' ', false);
        }

        public Tuple<string, string> searchTree(List<Tuple<String, int>> thisBreakdown)
        {
            //the int is the position of the breakdown this datanode will compare its children with
            List<Tuple<dataNode, int>> possibleOptions = new List<Tuple<dataNode, int>>();
            Queue<Tuple<dataNode, int>> frontier = new Queue<Tuple<dataNode, int>>();
            frontier.Enqueue(new Tuple<dataNode, int>(root, 0));

            while (frontier.Count > 0)
            {
                Tuple<dataNode, int> current= frontier.Dequeue();

                if (current.Item2 < thisBreakdown.Count && current.Item1.children.Count > 0)
                {
                    bool fitFound = false;
                    for (int i = 0; i < current.Item1.children.Count; i++)
                    {
                        dataNode child = current.Item1.children[i];
                        if (child.SectionLetter.Equals(thisBreakdown[current.Item2].Item1) && child.minValue <= thisBreakdown[current.Item2].Item2 && child.maxValue >= thisBreakdown[current.Item2].Item2)
                        {
                            frontier.Enqueue(new Tuple<dataNode, int>(child, current.Item2 + 1));
                            fitFound = true;
                        }      
                    }

                    if (!fitFound)
                    {
                        possibleOptions.Add(current);
                    }
                }
                else
                {
                    possibleOptions.Add(current);
                }

            }


            //if multiple equally likely option exist, will combine them, could say no match as well, can discuss later
            string possibleLetters = "";
            string ifStopHere = "";

            if (possibleOptions.Count > 1)
            {
                //find best option
                bool anyPerfectMatches = false; //exact matches, same length of input (only likely towards the end of input
                bool anygreaterThan = false; //if there are any of the options that are equal to input but with children (the goal for most inputs)
                int closestSmaller = thisBreakdown.Count + 1;
                for (int i = 0; i < possibleOptions.Count && !anyPerfectMatches; i++)
                {
                    int difference = thisBreakdown.Count - possibleOptions[i].Item2;
                    bool lessThan = difference > 0;
                    if (lessThan && closestSmaller > difference)
                    {
                        closestSmaller = difference;
                    }
                    bool greaterThan = possibleOptions[i].Item1.children.Count > 0  && !lessThan;
                    anyPerfectMatches =!greaterThan && !lessThan;
                    anygreaterThan = anygreaterThan || greaterThan;
                }


                for (int i = 0; i < possibleOptions.Count && !anyPerfectMatches; i++)
                {
                    int difference = thisBreakdown.Count - possibleOptions[i].Item2;
                    bool lessThan = thisBreakdown.Count - possibleOptions[i].Item2 > 0;
                    bool greaterThan = possibleOptions[i].Item1.children.Count > 0 && !lessThan;
                    if ((anyPerfectMatches && (greaterThan || lessThan)) || (anygreaterThan && !anyPerfectMatches && lessThan) || (!anyPerfectMatches && !anygreaterThan && difference > closestSmaller))
                    {
                        possibleOptions.RemoveAt(i);
                        i--;
                    }
                }

            }

            for (int i = 0; i < possibleOptions.Count; i++)
            {
                possibleLetters = smartAlphaBetCombine(possibleLetters, possibleOptions[i].Item1.chars);
                ifStopHere = smartAlphaBetCombine(ifStopHere, possibleOptions[i].Item1.ifStopHere + "");
            }

            return new Tuple<string, string>(possibleLetters, ifStopHere);
        }

        private String smartAlphaBetCombine(string a, string b)
        {
            //lets assume start at 'a'
            List<bool> values = new List<bool>();

            for (int i = 0; i < a.Length; i++)
            {
                int loc = a[i] - 'a';
                while (values.Count <= loc)
                {
                    values.Add(false);
                }
                values[loc] = true;
            }

            for (int i = 0; i < b.Length; i++)
            {
                int loc = b[i] - 'a';
                while (values.Count <= loc)
                {
                    values.Add(false);
                }
                values[loc] = true;
            }

            string output = "";
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i]) { output += "" + ('a' + i); }
            }
            return output;
        }

        public void smartStart(List<Tuple<List<mPoint>, char>> data, char[] alphabet)
        {
            dataNode[] roots = new dataNode[alphabet.Length];
            
            for (int i = 0; i < alphabet.Length; i++)
            {
                roots[i] = new dataNode("", 0, ' ', false);
                char thisLetter = alphabet[i];
                for (int j = 0; j < data.Count; j++)
                {
                    if (data[j].Item2 == thisLetter)
                    {
                        addToTree(roots[i], data[j].Item1, data[j].Item2);
                        combineTree(roots[i]);
                    }
                }
            }
            for (int i = 0; i < roots.Length; i++)
            {
                root = new dataNode(root, roots[i]);
                combineTree(root);
            }
        }
        public void addToTree(dataNode start, List<mPoint> input, char associatedLetter)
        {
            List<Tuple<string, int>> newBreakdown = currentConverter.getSectionBreakDown(input);

            dataNode current = start;
            for (int i = 0; i < newBreakdown.Count; i++)
            {
                dataNode next = new dataNode(newBreakdown[i].Item1, newBreakdown[i].Item2, associatedLetter, (i==(newBreakdown.Count-1)));
                current.addChild(next);
                current = next;
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
        public int minValue;
        public int maxValue;
        public String chars;
        public char ifStopHere;

        public dataNode(String letter, int value, char newChar , bool final)
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
