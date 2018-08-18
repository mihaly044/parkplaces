using System.ComponentModel;
using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    partial class EditZoneForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.textBoxId = new System.Windows.Forms.TextBox();
            this.textBoxColor = new System.Windows.Forms.TextBox();
            this.buttonSelectColor = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxFee = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxServiceNa = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxCities = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxTimetable = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxCommonName = new System.Windows.Forms.TextBox();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(19, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Id:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Szín:";
            // 
            // textBoxId
            // 
            this.textBoxId.Location = new System.Drawing.Point(99, 6);
            this.textBoxId.Name = "textBoxId";
            this.textBoxId.ReadOnly = true;
            this.textBoxId.Size = new System.Drawing.Size(227, 20);
            this.textBoxId.TabIndex = 3;
            // 
            // textBoxColor
            // 
            this.textBoxColor.Location = new System.Drawing.Point(99, 36);
            this.textBoxColor.Name = "textBoxColor";
            this.textBoxColor.ReadOnly = true;
            this.textBoxColor.Size = new System.Drawing.Size(145, 20);
            this.textBoxColor.TabIndex = 4;
            // 
            // buttonSelectColor
            // 
            this.buttonSelectColor.Location = new System.Drawing.Point(250, 34);
            this.buttonSelectColor.Name = "buttonSelectColor";
            this.buttonSelectColor.Size = new System.Drawing.Size(75, 23);
            this.buttonSelectColor.TabIndex = 5;
            this.buttonSelectColor.Text = "Kiválaszt";
            this.buttonSelectColor.UseVisualStyleBackColor = true;
            this.buttonSelectColor.Click += new System.EventHandler(this.buttonSelectColor_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Díj:";
            // 
            // textBoxFee
            // 
            this.textBoxFee.Location = new System.Drawing.Point(99, 62);
            this.textBoxFee.Name = "textBoxFee";
            this.textBoxFee.Size = new System.Drawing.Size(227, 20);
            this.textBoxFee.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 91);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Szolgáltató:";
            // 
            // textBoxServiceNa
            // 
            this.textBoxServiceNa.Location = new System.Drawing.Point(99, 88);
            this.textBoxServiceNa.Name = "textBoxServiceNa";
            this.textBoxServiceNa.Size = new System.Drawing.Size(227, 20);
            this.textBoxServiceNa.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 117);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Település:";
            // 
            // comboBoxCities
            // 
            this.comboBoxCities.Enabled = false;
            this.comboBoxCities.FormattingEnabled = true;
            this.comboBoxCities.Location = new System.Drawing.Point(99, 114);
            this.comboBoxCities.Name = "comboBoxCities";
            this.comboBoxCities.Size = new System.Drawing.Size(226, 21);
            this.comboBoxCities.TabIndex = 11;
            this.comboBoxCities.Text = "Betöltés...";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 144);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(69, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Nyitva tartás:";
            // 
            // textBoxTimetable
            // 
            this.textBoxTimetable.Location = new System.Drawing.Point(99, 141);
            this.textBoxTimetable.Name = "textBoxTimetable";
            this.textBoxTimetable.Size = new System.Drawing.Size(227, 20);
            this.textBoxTimetable.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(74, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Általános név:";
            // 
            // textBoxCommonName
            // 
            this.textBoxCommonName.Location = new System.Drawing.Point(99, 168);
            this.textBoxCommonName.Name = "textBoxCommonName";
            this.textBoxCommonName.Size = new System.Drawing.Size(227, 20);
            this.textBoxCommonName.TabIndex = 15;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(251, 194);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 16;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(170, 194);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 17;
            this.buttonCancel.Text = "Mégsem";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // EditZoneForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(337, 223);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.textBoxCommonName);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxTimetable);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.comboBoxCities);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxServiceNa);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxFee);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.buttonSelectColor);
            this.Controls.Add(this.textBoxColor);
            this.Controls.Add(this.textBoxId);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "EditZoneForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Zóna szerkesztése";
            this.Load += new System.EventHandler(this.EditZoneForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Label label1;
        private Label label2;
        private ColorDialog colorDialog;
        private TextBox textBoxId;
        private TextBox textBoxColor;
        private Button buttonSelectColor;
        private Label label3;
        private TextBox textBoxFee;
        private Label label4;
        private TextBox textBoxServiceNa;
        private Label label5;
        private ComboBox comboBoxCities;
        private Label label6;
        private TextBox textBoxTimetable;
        private Label label7;
        private TextBox textBoxCommonName;
        private Button buttonCancel;
        private Button buttonOk;
    }
}