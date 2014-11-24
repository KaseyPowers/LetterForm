using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace penToText
{
    public class dataTree
    {
        dataNode root;
        textConverter currentConverter;
        Core core;
        public dataTree(textConverter converter, Core core)
        {
            this.core = core;
            currentConverter = converter;
            currentConverter.setTree(this);
            root = new dataNode("", 0);
        }

        public dataNode getRoot()
        {
            //do a deep copy just in case
            return new dataNode(root);
        }

        public Tuple<string, string> getTotalString(List<dataNode> input)
        {
            string possibleLetters = "";
            string ifStopHere = "";
            for (int i = 0; i < input.Count; i++)
            {
                possibleLetters = smartAlphabetCombine(possibleLetters, input[i].chars);
                ifStopHere = smartAlphabetCombine(ifStopHere, input[i].ifStopHere + "");
            }
            return new Tuple<string, string>(possibleLetters, ifStopHere);
        }

        public List<dataNode> searchTree(List<Tuple<String, int>> thisBreakdown)
        {
            List<Tuple<dataNode, int>> start = new List<Tuple<dataNode,int>>();
            start.Add(new Tuple<dataNode, int>(root, 0));
            return searchTree(start, thisBreakdown, true);
        }

        public List<dataNode> searchTree(List<Tuple<dataNode, int>> input, List<Tuple<String, int>> thisBreakdown, bool withScale)
        {
            //the int is the position of the breakdown this datanode will compare its children with
            List<Tuple<dataNode, int>> possibleOptions = new List<Tuple<dataNode, int>>();
            List<dataNode> output = new List<dataNode>();
            Queue<Tuple<dataNode, int>> frontier = new Queue<Tuple<dataNode, int>>();
            for (int i = 0; i < input.Count; i++)
            {
                frontier.Enqueue(input[i]);
            }

            while (frontier.Count > 0)
            {
                Tuple<dataNode, int> current = frontier.Dequeue();

                if (current.Item2 < thisBreakdown.Count && current.Item1.children.Count > 0)
                {
                    bool fitFound = false;
                    for (int i = 0; i < current.Item1.children.Count; i++)
                    {
                        dataNode child = current.Item1.children[i];
                        if (child.SectionLetter.Equals(thisBreakdown[current.Item2].Item1) && (withScale &&  child.minValue <= thisBreakdown[current.Item2].Item2 && child.maxValue >= thisBreakdown[current.Item2].Item2))
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

            if (possibleOptions.Count > 1)
            {
                if (withScale)
                {
                    output = searchTree(possibleOptions, thisBreakdown, false);
                }
                else
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
                        bool greaterThan = possibleOptions[i].Item1.children.Count > 0 && !lessThan;
                        anyPerfectMatches = !greaterThan && !lessThan;
                        anygreaterThan = anygreaterThan || greaterThan;
                    }


                    for (int i = 0; i < possibleOptions.Count; i++)
                    {
                        int difference = thisBreakdown.Count - possibleOptions[i].Item2;
                        bool lessThan = difference > 0;
                        bool greaterThan = possibleOptions[i].Item1.children.Count > 0 && !lessThan;
                        if ((anyPerfectMatches && (greaterThan || lessThan)) || (!anyPerfectMatches && anygreaterThan && lessThan) || (!anyPerfectMatches && !anygreaterThan && difference > closestSmaller))
                        {
                            possibleOptions.RemoveAt(i);
                            i--;
                        }
                    }
                }               
            }

            if(output.Count == 0)
            {
                for (int i = 0; i < possibleOptions.Count; i++)
                {
                    output.Add(possibleOptions[i].Item1);
                }
            };
            

            return output;
        }

        private String smartAlphabetCombine(string a, string b)
        {
            //lets assume start at 'a'
            bool[] values = new bool[core.alphabet.Length];
            for(int i=0; i< values.Length; i++){
                values[i]=false;
            }
            a = a.Replace(" ", "");
            b = b.Replace(" ", "");

            if (a.Length > 0)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    values[Array.IndexOf(core.alphabet, a[i])] = true;
                }
            }

            if (b.Length > 0)
            {
                for (int i = 0; i < b.Length; i++)
                {
                    values[Array.IndexOf(core.alphabet, b[i])] = true;
                }
            }

            string output = "";
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i]) { output += ""+core.alphabet[i]; }
            }
            return output.Replace(" ", "");
        }

        public void smartStart(List<Tuple<List<mPoint>, char>> data, char[] alphabet)
        {
            root = new dataNode("", 0);
            dataNode[] roots = new dataNode[alphabet.Length];

            for (int i = 0; i < alphabet.Length; i++)
            {
                roots[i] = new dataNode("", 0);
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
                dataNode next = new dataNode(newBreakdown[i].Item1, newBreakdown[i].Item2);


                current.addChild(next);
                if (i == (newBreakdown.Count - 1))
                {
                    next.addFinalChar(associatedLetter);
                }
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
                    List<int> usedLocs = new List<int>();
                    for (int i = 0; i < currentChildren.Count; i++)
                    {

                        List<int> matches = new List<int>();
                        for (int j = 0; j < currentChildren.Count; j++)
                        {
                            if (i != j && !usedLocs.Contains(j) && currentChildren[i].canComabine(currentChildren[j]))
                            {
                                matches.Add(j);
                            }
                        }
                        if (matches.Count == 0 && !usedLocs.Contains(i))
                        {
                            nextCildren.Add(currentChildren[i]);
                            usedLocs.Add(i);
                        }
                        else if (matches.Count > 0)
                        {
                            hasChanged = true;
                            int bestLoc = matches[0];
                            for (int j = 1; j < matches.Count; j++)
                            {
                                //compare values
                                if (currentChildren[i].bestCombine(currentChildren[matches[j]], currentChildren[bestLoc]) >= 0)
                                {
                                    bestLoc = matches[j];
                                }
                            }
                            usedLocs.Add(i);
                            usedLocs.Add(bestLoc);
                            nextCildren.Add(new dataNode(currentChildren[i], currentChildren[bestLoc]));
                        }
                    }
                    currentChildren = new List<dataNode>(nextCildren);
                    nextCildren = new List<dataNode>();
                }

                current.children = new List<dataNode>(currentChildren);

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

        public dataNode(dataNode copy)
        {
            this.parent = null;
            this.SectionLetter = copy.SectionLetter;
            this.minValue = copy.minValue;
            this.maxValue = copy.maxValue;
            this.chars = copy.chars;
            this.ifStopHere = copy.ifStopHere;
            this.children = new List<dataNode>();

            for (int i = 0; i < copy.children.Count; i++)
            {
                this.addChild(new dataNode(copy.children[i]));
            }
        }

        public dataNode(String letter, int value)
        {
            parent = null;
            children = new List<dataNode>();
            SectionLetter = letter;
            minValue = value;
            maxValue = value;
            chars = "";
            ifStopHere = ' ';
        }

        public bool canComabine(dataNode other)
        {
            bool output = false;

            bool main = (this.parent == other.parent) && (this.SectionLetter.Equals(other.SectionLetter));
            bool validRoots = !(ifStopHere != ' ' && other.ifStopHere != ' ' && ifStopHere == other.ifStopHere);
            if (main && validRoots)
            {
                bool rangesMatch = ((this.minValue >= other.minValue && this.maxValue <= other.maxValue)
                    || (this.minValue <= other.minValue && this.maxValue >= other.maxValue));
                //bool matchingEndPoint = ifStopHere.Equals(other.ifStopHere) && ifStopHere.Length>0;
                output = rangesMatch;

                if (!rangesMatch)
                {
                    bool aCharContainB = this.chars.Contains(other.chars);
                    bool bCharContainA = other.chars.Contains(this.chars);

                    output = aCharContainB || bCharContainA;
                }


            }
            return output;
        }

        public int bestCombine(dataNode a, dataNode b)
        {
            int output = 0;

            for (int i = 0; i < chars.Length; i++)
            {
                if (a.chars.Contains(chars[i]))
                {
                    output++;
                }
                if (b.chars.Contains(chars[i]))
                {
                    output--;
                }
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
                if (parent != null)
                {
                    parent.addChar(newChar);
                }
            }
        }


        public void addFinalChar(char newChar)
        {
            if (ifStopHere == ' ')
            {
                ifStopHere = newChar;
                addChar(newChar);
            }
        }
    }
}
