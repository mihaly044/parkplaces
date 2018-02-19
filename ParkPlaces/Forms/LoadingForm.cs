using ParkPlaces.IO;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ParkPlaces.Forms
{
    public partial class LoadingForm : Form
    {
        private Sql _sql;
        private Dto2Object _dto;
        public EventHandler<Dto2Object> OnReadyEventHandler;

        public LoadingForm()
        {
            InitializeComponent();
            _sql = new Sql();

            _sql.OnUpdateChangedEventHandler += (sender, updateProcessChangedArgs) => {
                var currentProgress = ((double)(updateProcessChangedArgs.TotalChunks - updateProcessChangedArgs.CurrentChunks) / (double)updateProcessChangedArgs.TotalChunks) * 100;
                if(currentProgress - progressBar.Value > 1)
                {
                    progressBar.Value = (int)currentProgress;
                   
                }
            };

            IoHandler.Instance.OnUpdateChangedEventHandler = _sql.OnUpdateChangedEventHandler;
        }

        public async void LoadDataAsync()
        {
            Load:
            try
            {
                _dto = await _sql.LoadFromDb();
                OnReadyEventHandler?.Invoke(this, _dto);
                Close();

            } catch (Exception)
            {
                var dlgResult = MessageBox.Show("Unable to communicate with the database server.\n" +
                    "Press retry to try again or cancel to close the application.", "Error", MessageBoxButtons.RetryCancel, MessageBoxIcon.Error);

                if (dlgResult == DialogResult.Cancel)
                {
                    Environment.Exit(0);
                }
                else if (dlgResult == DialogResult.Retry)
                {
                    goto Load;
                }
            }

            
        }
    }
}
