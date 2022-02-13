using System;

namespace MonthReadingRecalculation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            FrmRecalculate frm = new FrmRecalculate();
            frm.ShowDialog();
        }
    }
}
