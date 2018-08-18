using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace ParkPlaces.Controls
{
    /// <summary>
    /// Reference: https://stackoverflow.com/questions/778678/how-to-change-the-color-of-progressbar-in-c-sharp-net-3-5
    /// </summary>
    public class ProgressBarEx : ProgressBar
    {
        /// <summary>
        /// Inner margin of the progressbar from the left, right, top and bottom sides
        /// </summary>
        [Category("Design")]
        [DefaultValue(0)]
        // ReSharper disable once MemberCanBePrivate.Global
        public int InnerMargin { get; set; }

        public ProgressBarEx()
        {
            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var rec = new Rectangle(0, 0, Width, Height);
            var scaleFactor = (Value - (double)Minimum) / (Maximum - (double)Minimum);

            if (ProgressBarRenderer.IsSupported)
                ProgressBarRenderer.DrawHorizontalBar(e.Graphics, rec);

            rec.Width = (int)((rec.Width * scaleFactor) - 4);
            rec.Height -= InnerMargin * InnerMargin;
            if (rec.Width == 0) rec.Width = 1;
            var brush = new LinearGradientBrush(rec, ForeColor, BackColor, LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(brush, InnerMargin, InnerMargin, rec.Width, rec.Height);
        }
    }
}
