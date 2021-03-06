﻿using System.ComponentModel;
using System.Windows.Forms;

namespace ParkPlaces.Controls
{
    partial class Map
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.drawPolygonCtxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.finalizeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelEscToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.polygonPointCtxMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addPointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deletePointToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.drawPolygonCtxMenu.SuspendLayout();
            this.polygonPointCtxMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // drawPolygonCtxMenu
            // 
            this.drawPolygonCtxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.finalizeToolStripMenuItem,
            this.cancelEscToolStripMenuItem});
            this.drawPolygonCtxMenu.Name = "drawPolygonCtxMenu";
            this.drawPolygonCtxMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.drawPolygonCtxMenu.Size = new System.Drawing.Size(213, 48);
            // 
            // finalizeToolStripMenuItem
            // 
            this.finalizeToolStripMenuItem.Name = "finalizeToolStripMenuItem";
            this.finalizeToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.finalizeToolStripMenuItem.Text = "Zóna véglegesítése (Enter)";
            this.finalizeToolStripMenuItem.Click += new System.EventHandler(this.finalizeToolStripMenuItem_Click);
            // 
            // cancelEscToolStripMenuItem
            // 
            this.cancelEscToolStripMenuItem.Name = "cancelEscToolStripMenuItem";
            this.cancelEscToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.cancelEscToolStripMenuItem.Text = "Zóna elvetése (Esc)";
            this.cancelEscToolStripMenuItem.Click += new System.EventHandler(this.cancelToolStripMenuItem_Click);
            // 
            // polygonPointCtxMenu
            // 
            this.polygonPointCtxMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addPointToolStripMenuItem,
            this.deletePointToolStripMenuItem});
            this.polygonPointCtxMenu.Name = "polygonPointCtxMenu";
            this.polygonPointCtxMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.polygonPointCtxMenu.Size = new System.Drawing.Size(207, 70);
            // 
            // addPointToolStripMenuItem
            // 
            this.addPointToolStripMenuItem.Name = "addPointToolStripMenuItem";
            this.addPointToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.addPointToolStripMenuItem.Text = "Csomópont hozzáaadása";
            this.addPointToolStripMenuItem.Click += new System.EventHandler(this.addPointToolStripMenuItem_Click);
            // 
            // deletePointToolStripMenuItem
            // 
            this.deletePointToolStripMenuItem.Name = "deletePointToolStripMenuItem";
            this.deletePointToolStripMenuItem.Size = new System.Drawing.Size(206, 22);
            this.deletePointToolStripMenuItem.Text = "Csomópont törlése";
            this.deletePointToolStripMenuItem.Click += new System.EventHandler(this.deletePointToolStripMenuItem_Click);
            // 
            // Map
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "Map";
            this.OnPolygonClick += new GMap.NET.WindowsForms.PolygonClick(this.Map_OnPolygonClick);
            this.OnMarkerEnter += new GMap.NET.WindowsForms.MarkerEnter(this.Map_OnMarkerEnter);
            this.OnMarkerLeave += new GMap.NET.WindowsForms.MarkerLeave(this.Map_OnMarkerLeave);
            this.drawPolygonCtxMenu.ResumeLayout(false);
            this.polygonPointCtxMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private ContextMenuStrip drawPolygonCtxMenu;
        private ToolStripMenuItem finalizeToolStripMenuItem;
        private ToolStripMenuItem cancelEscToolStripMenuItem;
        private ContextMenuStrip polygonPointCtxMenu;
        private ToolStripMenuItem deletePointToolStripMenuItem;
        private ToolStripMenuItem addPointToolStripMenuItem;
    }
}
