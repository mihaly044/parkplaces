using System.ComponentModel;
using System.Windows.Forms;
using ParkPlaces.Controls;

namespace ParkPlaces.Forms
{
    partial class ParkPlacesForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            this.toolStripContainer = new System.Windows.Forms.ToolStripContainer();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SelectMapProviderButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.googleMapsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openStreetMapToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblZoom = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblMouse = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblShapesCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.Map = new ParkPlaces.Controls.Map();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.forceUpdateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.closeCurrentSessionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.adminToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageUsersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.drawPolygonButton = new System.Windows.Forms.ToolStripButton();
            this.removePolygonButton = new System.Windows.Forms.ToolStripButton();
            this.gotoButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.coordinateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addressToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripContainer.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer.ContentPanel.SuspendLayout();
            this.toolStripContainer.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStripContainer
            // 
            // 
            // toolStripContainer.BottomToolStripPanel
            // 
            this.toolStripContainer.BottomToolStripPanel.Controls.Add(this.statusStrip);
            // 
            // toolStripContainer.ContentPanel
            // 
            this.toolStripContainer.ContentPanel.Controls.Add(this.Map);
            this.toolStripContainer.ContentPanel.Size = new System.Drawing.Size(1029, 555);
            this.toolStripContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer.Name = "toolStripContainer";
            this.toolStripContainer.Size = new System.Drawing.Size(1029, 626);
            this.toolStripContainer.TabIndex = 7;
            this.toolStripContainer.Text = "toolStripContainer";
            // 
            // toolStripContainer.TopToolStripPanel
            // 
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.menuStrip);
            this.toolStripContainer.TopToolStripPanel.Controls.Add(this.toolStrip);
            // 
            // statusStrip
            // 
            this.statusStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar,
            this.toolStripStatusLabel,
            this.SelectMapProviderButton,
            this.lblZoom,
            this.lblMouse,
            this.lblShapesCount});
            this.statusStrip.Location = new System.Drawing.Point(0, 0);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1029, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip";
            // 
            // toolStripProgressBar
            // 
            this.toolStripProgressBar.Name = "toolStripProgressBar";
            this.toolStripProgressBar.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar.Visible = false;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(30, 17);
            this.toolStripStatusLabel.Text = "Kész";
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
            this.lblMouse.Visible = false;
            // 
            // lblShapesCount
            // 
            this.lblShapesCount.Name = "lblShapesCount";
            this.lblShapesCount.Size = new System.Drawing.Size(52, 17);
            this.lblShapesCount.Text = "               ";
            // 
            // Map
            // 
            this.Map.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Map.BackColor = System.Drawing.Color.Transparent;
            this.Map.Bearing = 0F;
            this.Map.CanDragMap = true;
            this.Map.EmptyTileColor = System.Drawing.Color.Navy;
            this.Map.GrayScaleMode = false;
            this.Map.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
            this.Map.IsDrawingPolygon = false;
            this.Map.LevelsKeepInMemmory = 5;
            this.Map.Location = new System.Drawing.Point(0, 0);
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
            this.Map.Size = new System.Drawing.Size(1029, 555);
            this.Map.TabIndex = 0;
            this.Map.ZoneOpacity = 80;
            this.Map.Zoom = 15D;
            this.Map.OnDrawPolygonEnd += new ParkPlaces.Controls.Map.DrawPolygonEnd(this.Map_DrawPolygonEnd);
            this.Map.OnVerticlesChanged += new ParkPlaces.Controls.Map.VerticlesChanged(this.Map_VerticlesChanged);
            this.Map.OnMapZoomChanged += new GMap.NET.MapZoomChanged(this.Map_OnMapZoomChanged);
            this.Map.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Map_MouseMove);
            // 
            // menuStrip
            // 
            this.menuStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.adminToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1029, 24);
            this.menuStrip.TabIndex = 5;
            this.menuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.forceUpdateToolStripMenuItem,
            this.toolStripSeparator1,
            this.closeCurrentSessionToolStripMenuItem,
            this.logoutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_53_62710;
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openToolStripMenuItem.Text = "Import from JSON";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_Export_File_40053;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.saveToolStripMenuItem.Text = "Export to JSON";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // forceUpdateToolStripMenuItem
            // 
            this.forceUpdateToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_update_64935;
            this.forceUpdateToolStripMenuItem.Name = "forceUpdateToolStripMenuItem";
            this.forceUpdateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.forceUpdateToolStripMenuItem.Text = "Force update";
            this.forceUpdateToolStripMenuItem.Click += new System.EventHandler(this.forceUpdateToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(190, 6);
            // 
            // closeCurrentSessionToolStripMenuItem
            // 
            this.closeCurrentSessionToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_cancel_48_10277;
            this.closeCurrentSessionToolStripMenuItem.Name = "closeCurrentSessionToolStripMenuItem";
            this.closeCurrentSessionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.closeCurrentSessionToolStripMenuItem.Text = "Close session";
            this.closeCurrentSessionToolStripMenuItem.Click += new System.EventHandler(this.closeCurrentSessionToolStripMenuItem_Click);
            // 
            // logoutToolStripMenuItem
            // 
            this.logoutToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_logout_63128;
            this.logoutToolStripMenuItem.Name = "logoutToolStripMenuItem";
            this.logoutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.logoutToolStripMenuItem.Text = "Logout";
            this.logoutToolStripMenuItem.Click += new System.EventHandler(this.logoutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_exit_6035;
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // adminToolStripMenuItem
            // 
            this.adminToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageUsersToolStripMenuItem});
            this.adminToolStripMenuItem.Name = "adminToolStripMenuItem";
            this.adminToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.adminToolStripMenuItem.Text = "Admin";
            // 
            // manageUsersToolStripMenuItem
            // 
            this.manageUsersToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_Users_85409;
            this.manageUsersToolStripMenuItem.Name = "manageUsersToolStripMenuItem";
            this.manageUsersToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.manageUsersToolStripMenuItem.Text = "Manage users";
            this.manageUsersToolStripMenuItem.Click += new System.EventHandler(this.manageUsersToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Image = global::ParkPlaces.Properties.Resources.if_Information_27854;
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip
            // 
            this.toolStrip.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.drawPolygonButton,
            this.removePolygonButton,
            this.gotoButton,
            this.saveButton});
            this.toolStrip.Location = new System.Drawing.Point(3, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(412, 25);
            this.toolStrip.TabIndex = 4;
            this.toolStrip.Text = "toolStrip";
            // 
            // drawPolygonButton
            // 
            this.drawPolygonButton.Image = global::ParkPlaces.Properties.Resources.if_draw_polygon_64148;
            this.drawPolygonButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.drawPolygonButton.Name = "drawPolygonButton";
            this.drawPolygonButton.Size = new System.Drawing.Size(101, 22);
            this.drawPolygonButton.Text = "Draw polygon";
            this.drawPolygonButton.Click += new System.EventHandler(this.drawPolygonButton_Click);
            // 
            // removePolygonButton
            // 
            this.removePolygonButton.Image = global::ParkPlaces.Properties.Resources.if_f_cross_256_282471;
            this.removePolygonButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.removePolygonButton.Name = "removePolygonButton";
            this.removePolygonButton.Size = new System.Drawing.Size(117, 22);
            this.removePolygonButton.Text = "Remove polygon";
            this.removePolygonButton.Click += new System.EventHandler(this.RemovePolygonButton_Click);
            // 
            // gotoButton
            // 
            this.gotoButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.coordinateToolStripMenuItem,
            this.addressToolStripMenuItem});
            this.gotoButton.Image = global::ParkPlaces.Properties.Resources.if_Rounded___High_Ultra_Colour02___Maps_2250024;
            this.gotoButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.gotoButton.Name = "gotoButton";
            this.gotoButton.Size = new System.Drawing.Size(83, 22);
            this.gotoButton.Text = "Navigate";
            // 
            // coordinateToolStripMenuItem
            // 
            this.coordinateToolStripMenuItem.Name = "coordinateToolStripMenuItem";
            this.coordinateToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.coordinateToolStripMenuItem.Text = "Coordinates";
            this.coordinateToolStripMenuItem.Click += new System.EventHandler(this.coordinateToolStripMenuItem_Click);
            // 
            // addressToolStripMenuItem
            // 
            this.addressToolStripMenuItem.Name = "addressToolStripMenuItem";
            this.addressToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addressToolStripMenuItem.Text = "Address";
            this.addressToolStripMenuItem.Click += new System.EventHandler(this.addressToolStripMenuItem_Click);
            // 
            // saveButton
            // 
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.saveButton.Image = ((System.Drawing.Image)(resources.GetObject("saveButton.Image")));
            this.saveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(99, 22);
            this.saveButton.Text = "Save to database";
            this.saveButton.Click += new System.EventHandler(this.SaveButton_ClickAsync);
            // 
            // ParkPlacesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1029, 626);
            this.Controls.Add(this.toolStripContainer);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "ParkPlacesForm";
            this.ShowIcon = false;
            this.Text = "ParkPlaces Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ParkPlacesForm_FormClosing);
            this.Load += new System.EventHandler(this.ParkPlacesForm_Load);
            this.Shown += new System.EventHandler(this.ParkPlacesForm_Shown);
            this.toolStripContainer.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer.ContentPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer.TopToolStripPanel.PerformLayout();
            this.toolStripContainer.ResumeLayout(false);
            this.toolStripContainer.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private Map Map;
        private ToolStrip toolStrip;
        private ToolStripButton drawPolygonButton;
        private ToolStripDropDownButton SelectMapProviderButton;
        private ToolStripMenuItem googleMapsToolStripMenuItem;
        private ToolStripMenuItem openStreetMapToolStripMenuItem;
        private ToolStripStatusLabel lblZoom;
        private ToolStripStatusLabel lblMouse;
        private StatusStrip statusStrip;
        private ToolStripButton removePolygonButton;
        private ToolStripProgressBar toolStripProgressBar;
        private ToolStripStatusLabel toolStripStatusLabel;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem saveToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStripMenuItem closeCurrentSessionToolStripMenuItem;
        private ToolStripContainer toolStripContainer;
        private ToolStripMenuItem forceUpdateToolStripMenuItem;
        private ToolStripDropDownButton gotoButton;
        private ToolStripMenuItem coordinateToolStripMenuItem;
        private ToolStripMenuItem addressToolStripMenuItem;
        private ToolStripStatusLabel lblShapesCount;
        private ToolStripMenuItem logoutToolStripMenuItem;
        private ToolStripButton saveButton;
        private ToolStripMenuItem adminToolStripMenuItem;
        private ToolStripMenuItem manageUsersToolStripMenuItem;
    }
}

