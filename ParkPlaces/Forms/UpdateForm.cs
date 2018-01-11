using ParkPlaces.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    public partial class UpdateForm : Form
    {
        public UpdateForm()
        {
            InitializeComponent();

            IoHandler.Instance.OnUpdateChangedEventHandler += (sender, e) =>
            {
                var param = (UpdateProcessChangedArgs)e;

                progressBar.Maximum = param.TotalChunks;
                progressBar.Value = param.CurrentChunks;

                label1.Text = $"{param.CurrentChunks} / {param.TotalChunks}";

                if (param.TotalChunks == param.CurrentChunks)
                    Close();
            };
        }
    }
}
