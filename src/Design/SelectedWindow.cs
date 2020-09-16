using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using System.Windows;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace Spectrogram_Plus.Design
{
    class SelectedSpecWindow
    {
        /**
         * When the user windows out a selection of the spectrogram, this is the class that handles that.
         * To draw, assign windowToDraw to a parent element. Eg PaintGrid.Children.Add(this.windowToDraw)
         * To update the position, use UpdateShape(new point) with the location of the new point
         * When the window is finally selected after the user lets go of the mouse, use FinishDrawing()
         */
        public Point startPoint { get; private set; }
        public Point endPoint { get; private set; }
        public bool shouldDraw { get; private set; } //Determines if the selectedWindowToDraw should continue updating its position 
        public Rectangle windowToDraw { get; private set; } //This is the rectangle that shows the area of the spectrogram selected


        public SelectedSpecWindow()
        {
            shouldDraw = false;

            windowToDraw = new Rectangle
            {
                Stroke = Brushes.White,
                StrokeThickness = 1.0
            };
        }

        public bool WindowExists()
        {
            return startPoint != endPoint;
        }

        public SelectedSpecWindow(Point startPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = startPoint;
            this.shouldDraw = true;

            windowToDraw = new Rectangle
            {
                Stroke = Brushes.White,
                StrokeThickness = 1.0
            };
        }

        public void FinishDrawing() => shouldDraw = false;
        public Rectangle GetSpecWindow() => windowToDraw;
        public void SetEndPoint(Point p) => this.endPoint = p;
        public void UpdateShape(Point endPoint)
        {
            if (shouldDraw)
            {
                this.endPoint = endPoint;
                Rect rectWindow = new Rect(startPoint, endPoint);
                windowToDraw.Arrange(rectWindow);
            }
        }
    }
}
