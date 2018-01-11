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

                label1.Text = $"Downloaded {param.CurrentChunks} items of {param.TotalChunks}";

                if (param.TotalChunks == param.CurrentChunks)
                    Close();
            };
        }

        /// <summary>
        /// Reference: https://www.codeproject.com/Articles/20379/Disabling-Close-Button-on-Forms
        /// </summary>
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
    }
}
