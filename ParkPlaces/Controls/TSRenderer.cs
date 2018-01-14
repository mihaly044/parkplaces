using System.Windows.Forms;

namespace ParkPlaces.Controls
{
    /// <summary>
    /// Reference: https://stackoverflow.com/questions/1918247/how-to-disable-the-line-under-tool-strip-in-winform-c
    /// </summary>
    public class TsRenderer : ToolStripSystemRenderer
    {
        public TsRenderer()
        {
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            //base.OnRenderToolStripBorder(e);
        }
    }
}