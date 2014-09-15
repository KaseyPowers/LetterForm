using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Petzold.Media2D;

namespace penToText
{
    public class convertToText
    {
        //general data storage
        private dynamicDisplay thisDynamicDisplay;
        private List<Point> originalData;
        public Size inputSize;

        //dominique's data
        private Point lastPoint;
        String lastDirection = "";
        String[] directionArray;
        int i = 0;        


        //canvas stuff
        private lineDrawCanvas inputCopy;
        public Size canvasSizes;
       
        public convertToText(dynamicDisplay display, Size inputSize)
        {
            this.inputSize = inputSize;
            
            lastPoint = new Point(-5, -5);
            originalData = new List<Point>();

            thisDynamicDisplay = display;

            inputCopy = new lineDrawCanvas(0, 0, display);
            inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            thisDynamicDisplay.addCanvas(inputCopy);
        }

        public void createDisplays()
        {
            //might be needed
        }

        public void clear()
        {
            //reset data
        }

        public void resize()
        {
            inputCopy.outOfX = inputSize.Width;
            inputCopy.outOfy = inputSize.Height;
            inputCopy.updateDraw();
        }
        
        public bool newData(Point newPoint)
        {
            //show copy of raw data
            originalData.Add(newPoint);
            inputCopy.newData(originalData);
            inputCopy.updateDraw();


            if (lastPoint != new Point(-5, -5))
            {
                Dominique1(newPoint);
            }      
           
            lastPoint = newPoint;

            
            
            return true;
        }

        private void Kasey1()
        {

        }

        private void Kasey2()
        {

        }

        private void Dominique1(Point newPoint)
        {
            /*String direction ="";
            if (newPoint.X > lastPoint.X)
            {
                if (newPoint.Y > lastPoint.Y)
                {
                    direction = "up positive";
                }
                else {
                    direction = "down negative";
                }
            }

            if (newPoint.X < lastPoint.X)
            {
                if (newPoint.Y > lastPoint.Y)
                {
                    direction = "up negative";
                }
                else {
                    direction = "down positive";
                }
            }

            // change in direction
            if (direction != lastDirection)
            {
                directionArray[i]=direction;
                i++;
            }

            lastDirection = direction;*/
        }

        private void Dominique2()
        {

        }

    }    
}
