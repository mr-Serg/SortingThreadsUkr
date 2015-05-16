using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SortingThreads
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SortController sc = new SortController();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new VisualForm(sc, new SortModel(sc)));
        }
    }
}
