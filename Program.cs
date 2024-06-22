using System;
using System.Linq;
using System.Windows.Forms;

namespace GsCapture
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) //TODO: Pass as par.. (string[] args)
        {
            string imagePath = args.FirstOrDefault();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new MainWindow());

            if (!string.IsNullOrEmpty(imagePath))
            {
                Application.Run(new MainWindow(imagePath));
            }
            else
            {
                Application.Run(new MainWindow(null));
                //MessageBox.Show("No image file specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
