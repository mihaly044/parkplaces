using System;
using System.Windows.Forms;
using ParkPlaces.Forms;
using ParkPlaces.IO.Database;

namespace ParkPlaces
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new ParkPlacesForm());
        }
    }
}