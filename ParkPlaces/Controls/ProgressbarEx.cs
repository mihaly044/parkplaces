﻿using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace ParkPlaces.Controls
{
    /// <summary>
    /// Reference: https://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
    /// </summary>
    public class ProgressBarEx : ProgressBar
    {
        public ProgressBarEx()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            LinearGradientBrush brush = null;
            Rectangle rec = new Rectangle(0, 0, this.Width, this.Height);
            double scaleFactor = (((double)Value - (double)Minimum) / ((double)Maximum - (double)Minimum));

            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);

            rec.Width = (int)((rec.Width * scaleFactor) - 4);
            rec.Height -= 4;
            brush = new LinearGradientBrush(rec, this.ForeColor, this.BackColor, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, 2, 2, rec.Width, rec.Height);
        }
    }
}
