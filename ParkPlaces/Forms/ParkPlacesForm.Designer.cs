using ParkPlaces.Controls;
namespace ParkPlaces
{
    partial class ParkPlacesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParkPlacesForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.drawPolygonButton = new System.Windows.Forms.ToolStripButton();
            this.RemovePolygonButton = new System.Windows.Forms.ToolStripButton();
            this.SelectMapProviderButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.googleMapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openStreetMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblZoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblMouse = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.Map = new ParkPlaces.Controls.Map();
            this.statisticsButton = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawPolygonButton,
            this.RemovePolygonButton,
            this.statisticsButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1029, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // drawPolygonButton
            // 
            this.drawPolygonButton.Image = ((System.Drawing.Image)(resources.GetObject("drawPolygonButton.Image")));
            this.drawPolygonButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawPolygonButton.Name = "drawPolygonButton";
            this.drawPolygonButton.Size = new System.Drawing.Size(96, 22);
            this.drawPolygonButton.Text = "Add polygon";
            this.drawPolygonButton.Click += new System.EventHandler(this.drawPolygonButton_Click);
            // 
            // RemovePolygonButton
            // 
            this.RemovePolygonButton.Image = global::ParkPlaces.Properties.Resources.if_f_cross_256_282471;
            this.RemovePolygonButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.RemovePolygonButton.Name = "RemovePolygonButton";
            this.RemovePolygonButton.Size = new System.Drawing.Size(117, 22);
            this.RemovePolygonButton.Text = "Remove polygon";
            this.RemovePolygonButton.Click += new System.EventHandler(this.RemovePolygonButton_Click);
            // 
            // SelectMapProviderButton
            // 
            this.SelectMapProviderButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.SelectMapProviderButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.googleMapsToolStripMenuItem,
            this.openStreetMapToolStripMenuItem});
            this.SelectMapProviderButton.Image = ((System.Drawing.Image)(resources.GetObject("SelectMapProviderButton.Image")));
            this.SelectMapProviderButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.SelectMapProviderButton.Name = "SelectMapProviderButton";
            this.SelectMapProviderButton.Size = new System.Drawing.Size(29, 20);
            this.SelectMapProviderButton.Text = "Select map provider";
            // 
            // googleMapsToolStripMenuItem
            // 
            this.googleMapsToolStripMenuItem.Name = "googleMapsToolStripMenuItem";
            this.googleMapsToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.googleMapsToolStripMenuItem.Text = "Google Maps";
            this.googleMapsToolStripMenuItem.Click += new System.EventHandler(this.googleMapsToolStripMenuItem_Click);
            // 
            // openStreetMapToolStripMenuItem
            // 
            this.openStreetMapToolStripMenuItem.Name = "openStreetMapToolStripMenuItem";
            this.openStreetMapToolStripMenuItem.Size = new System.Drawing.Size(157, 22);
            this.openStreetMapToolStripMenuItem.Text = "OpenStreetMap";
            this.openStreetMapToolStripMenuItem.Click += new System.EventHandler(this.openStreetMapToolStripMenuItem_Click);
            // 
            // lblZoom
            // 
            this.lblZoom.Name = "lblZoom";
            this.lblZoom.Size = new System.Drawing.Size(52, 17);
            this.lblZoom.Text = "               ";
            // 
            // lblMouse
            // 
            this.lblMouse.Name = "lblMouse";
            this.lblMouse.Size = new System.Drawing.Size(52, 17);
            this.lblMouse.Text = "               ";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SelectMapProviderButton,
            this.lblZoom,
            this.lblMouse});
            this.statusStrip1.Location = new System.Drawing.Point(0, 604);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(1029, 22);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Map
            // 
            this.Map.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Map.BackColor = System.Drawing.Color.Transparent;
            this.Map.Bearing = 0F;
            this.Map.CanDragMap = true;
            this.Map.DisplayCopyright = true;
            this.Map.DisplayVersionInfo = true;
            this.Map.EmptyTileColor = System.Drawing.Color.Navy;
            this.Map.GradientWidth = 125;
            this.Map.GrayScaleMode = false;
            this.Map.HasGradientSide = false;
            this.Map.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.Map.LevelsKeepInMemmory = 5;
            this.Map.Location = new System.Drawing.Point(-2, 22);
            this.Map.MarkersEnabled = true;
            this.Map.MaxZoom = 20;
            this.Map.MinZoom = 8;
            this.Map.MouseWheelZoomEnabled = true;
            this.Map.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            this.Map.Name = "Map";
            this.Map.NegativeMode = false;
            this.Map.PolygonsEnabled = true;
            this.Map.RetryLoadTile = 0;
            this.Map.RoutesEnabled = false;
            this.Map.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
            this.Map.SelectedAreaFillColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(65)))), ((int)(((byte)(105)))), ((int)(((byte)(225)))));
            this.Map.ShowTileGridLines = false;
            this.Map.Size = new System.Drawing.Size(1031, 580);
            this.Map.TabIndex = 0;
            this.Map.Zoom = 15D;
            this.Map.OnDrawPolygonEnd += new ParkPlaces.Controls.Map.DrawPolygonEnd(this.Map_DrawPolygonEnd);
            this.Map.OnMapZoomChanged += new GMap.NET.MapZoomChanged(this.Map_OnMapZoomChanged);
            this.Map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Map_MouseMove);
            // 
            // statisticsButton
            // 
            this.statisticsButton.Image = ((System.Drawing.Image)(resources.GetObject("statisticsButton.Image")));
            this.statisticsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.statisticsButton.Name = "statisticsButton";
            this.statisticsButton.Size = new System.Drawing.Size(73, 22);
            this.statisticsButton.Text = "Statistics";
            // 
            // ParkPlacesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1029, 626);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.Map);
            this.Name = "ParkPlacesForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ParkPlaces Editor";
            this.Load += new System.EventHandler(this.ParkPlacesForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Map Map;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton drawPolygonButton;
        private System.Windows.Forms.ToolStripDropDownButton SelectMapProviderButton;
        private System.Windows.Forms.ToolStripMenuItem googleMapsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openStreetMapToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel lblZoom;
        private System.Windows.Forms.ToolStripStatusLabel lblMouse;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripButton RemovePolygonButton;
        private System.Windows.Forms.ToolStripButton statisticsButton;
    }
}

