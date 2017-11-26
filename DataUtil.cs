using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace Home_Accounting
{
    class DataUtil
    {
        private static OleDbConnection connection = null;

        //private static OleDbCommand commandStart;
        //private static OleDbCommand commandCommit;
        //private static OleDbCommand commandRollback;

        public static OleDbConnection Connection
        {
            get { return DataUtil.connection; }
            set { 
                DataUtil.connection = value;
                /* Access 2000 doesn't support transactions
                commandStart = new OleDbCommand("START TRANSACTION", connection);
                commandStart.Prepare();
                commandCommit = new OleDbCommand("COMMIT TRANSACTION", connection);
                commandCommit.Prepare();
                commandRollback = new OleDbCommand("ROLLBACK TRANSACTION", connection);
                commandRollback.Prepare();
                 * */
            }
        }

        //public class Transaction : IDisposable
        //{
        //    bool commited = false;
        //    #region IDisposable Members

        //    public void Dispose()
        //    {
        //        if (!commited)
        //            commandRollback.ExecuteNonQuery();
        //    }

        //    #endregion
        //    public Transaction()
        //    {
        //        commandStart.ExecuteNonQuery();
        //    }

        //    public void Commit()
        //    {
        //        commandCommit.ExecuteNonQuery();
        //    }
        //}
    }
}
