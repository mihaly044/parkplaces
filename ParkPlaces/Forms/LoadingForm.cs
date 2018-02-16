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
            _sql = new Sql("localhost", "parkplaces", "root", "");

            _sql.OnUpdateChangedEventHandler += (sender, updateProcessChangedArgs) => {
                var currentProgress = ((double)(updateProcessChangedArgs.TotalChunks - updateProcessChangedArgs.CurrentChunks) / (double)updateProcessChangedArgs.TotalChunks) * 100;
                if(currentProgress - progressBar.Value > 1)
                    progressBar.Value = (int)currentProgress;
            };
        }

        public async void LoadDataAsync()
        {
            _dto = await _sql.LoadFromDb();
            OnReadyEventHandler?.Invoke(this, _dto);
            Close();
        }
    }
}
