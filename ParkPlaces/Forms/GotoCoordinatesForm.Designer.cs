namespace ParkPlaces.Forms
{
    partial class GotoCoordinatesForm
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
            this.Okbutton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.longitudeLabel = new System.Windows.Forms.Label();
            this.longitudeTextBox = new System.Windows.Forms.TextBox();
            this.latitudeTextBox = new System.Windows.Forms.TextBox();
            this.latitudeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Okbutton
            // 
            this.Okbutton.Location = new System.Drawing.Point(214, 74);
            this.Okbutton.Name = "Okbutton";
            this.Okbutton.Size = new System.Drawing.Size(75, 23);
            this.Okbutton.TabIndex = 0;
            this.Okbutton.Text = "OK";
            this.Okbutton.UseVisualStyleBackColor = true;
            this.Okbutton.Click += new System.EventHandler(this.Okbutton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(133, 74);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // longitudeLabel
            // 
            this.longitudeLabel.AutoSize = true;
            this.longitudeLabel.Location = new System.Drawing.Point(13, 13);
            this.longitudeLabel.Name = "longitudeLabel";
            this.longitudeLabel.Size = new System.Drawing.Size(57, 13);
            this.longitudeLabel.TabIndex = 2;
            this.longitudeLabel.Text = "Longitude:";
            // 
            // longitudeTextBox
            // 
            this.longitudeTextBox.Location = new System.Drawing.Point(76, 10);
            this.longitudeTextBox.Name = "longitudeTextBox";
            this.longitudeTextBox.Size = new System.Drawing.Size(213, 20);
            this.longitudeTextBox.TabIndex = 3;
            // 
            // latitudeTextBox
            // 
            this.latitudeTextBox.Location = new System.Drawing.Point(77, 36);
            this.latitudeTextBox.Name = "latitudeTextBox";
            this.latitudeTextBox.Size = new System.Drawing.Size(213, 20);
            this.latitudeTextBox.TabIndex = 5;
            // 
            // latitudeLabel
            // 
            this.latitudeLabel.AutoSize = true;
            this.latitudeLabel.Location = new System.Drawing.Point(14, 39);
            this.latitudeLabel.Name = "latitudeLabel";
            this.latitudeLabel.Size = new System.Drawing.Size(45, 13);
            this.latitudeLabel.TabIndex = 4;
            this.latitudeLabel.Text = "Latitude";
            // 
            // GotoCoordinatesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 109);
            this.Controls.Add(this.latitudeTextBox);
            this.Controls.Add(this.latitudeLabel);
            this.Controls.Add(this.longitudeTextBox);
            this.Controls.Add(this.longitudeLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.Okbutton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GotoCoordinatesForm";
            this.Text = "Enter coordinates";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Okbutton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label longitudeLabel;
        private System.Windows.Forms.TextBox longitudeTextBox;
        private System.Windows.Forms.TextBox latitudeTextBox;
        private System.Windows.Forms.Label latitudeLabel;
    }
}