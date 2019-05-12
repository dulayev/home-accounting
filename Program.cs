using System;
using System.Collections.Generic;
using System.Windows.Forms;

//TODO: implement computing allowed sum for month for category - limit
//TODO: think about account operation verification
//TODO: think about rollback operations

namespace Home_Accounting
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Call unit tests
            DataUtil.Test_CommonPart();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}