using ParkPlaces.IO;
using System;
using System.Windows.Forms;
using ParkPlaces.IO.Database;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
// ReSharper disable RedundantCast

// ReSharper disable ArrangeRedundantParentheses

namespace ParkPlaces.Forms
{
    public partial class LoadingForm : Form
    {
        private Dto2Object _dto;
        public EventHandler<Dto2Object> OnReadyEventHandler;

        public LoadingForm()
        {
            InitializeComponent();


        }

        public async void LoadDataAsync()
        {
            Progress<int> progress = new Progress<int>();

            progress.ProgressChanged += (sender, progressPercentage) =>
            {
                progressBar.Value = progressPercentage;
            };

            await Task.Run(() => { _dto = Sql.Instance.LoadZones(progress); });

            OnReadyEventHandler?.Invoke(this, _dto);
            Close();
        }
    }
}
