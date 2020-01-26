using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Diagnostics;

namespace Home_Accounting
{
    class DataUtil
    {
        static readonly List<string> NO_MATCH_WORDS = new List<string>() { " ŒœÀ¿“¿ " }; // meaningless matches
        public static int CommonPart(string s0, string s1)
        {
            s0 = s0.ToUpper();
            s1 = s1.ToUpper();
            const int MIN_MATCH = 5; // min length of match to be taken into account
            // to suppress accumulation matches of single symbols
            
            //s0: 0123456
            //s1: +j  012
            // j: [-s1.len-1, s0.len-1]
            // compare s0[j+i]==s1[i], 
            int total = 0;
            for (int j = -(s1.Length - 1); j < s0.Length; ++j)
            {
                //Prints regions being compared
                //Debug.WriteLine(new string(' ', 10) + s0);
                //Debug.WriteLine(new string(' ', 10 + j) + s1);
                int match = 0;
                //Debug.Write(new string(' ', 10 + j + (j < 0 ? -j : 0)));
                int i;
                void useMatch(int endPos)
                {
                    if (match >= MIN_MATCH && !NO_MATCH_WORDS.Contains(s1.Substring(endPos - match, match))) // 
                    {
                        total += match * match;
                    }
                    match = 0;
                }
                for (i = (j < 0 ? -j : 0); i < s1.Length && j + i < s0.Length; ++i)
                {
                    //Debug.Write("*");
                    if (s0[j + i] == s1[i]) match++;
                    else
                    {
                        useMatch(i);
                    }
                }
                //Debug.WriteLine("");
                useMatch(i);
            }
            return total;
        }

        public static void Test_CommonPart()
        {
            Debug.Assert(CommonPart("AAAAA1111111111", "22222AAAAA") == 25);
            Debug.Assert(CommonPart("1111111111AAAAA", "AAAAA22222") == 25);
            Debug.Assert(CommonPart("1111ABCDE111", "222ABCDE2222") == 25);
            Debug.Assert(CommonPart("1D11ABCDE111", "222ABCDE22D2") == 25); // ABCDE gives 5*5, D ignored
        }

        public static event Action<int> OnAccountUpdate; // Action is generic delegate without return

        public static void FireAccountUpdate(int accountID)
        {
            OnAccountUpdate?.Invoke(accountID);
        }

        private static OleDbConnection connection = null;

        public static void UpgradeDatabase()
        {
            // types used til now: TEXT, BIT, INTEGER, COUNTER, CURRENCY, DATETIME
            foreach (string tableName in new string[] { "Account", "Debt" })
            {
                CreateColumn(tableName, "Currency", "TEXT(3)");
            }
            RenameColumn("Transaction", "Amount", "SourceAmount;DestinationAmount", "CURRENCY");
            AssureColumnSize("Purchase", "Name", "TEXT", 100);
            CreateColumnWithDefault("Account", "Active", "BIT", 1);
            CreateColumn("Debt", "AmountBack", "CURRENCY");
            CreateColumn("Account", "StatementName", "TEXT(50)"); // regex to match statement file name
        }
        private static void CreateColumnWithDefault(string tableName, string columnName, string type, object defaultValue)
        {
            if (!ColumnExists(tableName, columnName))
            {
                OleDbCommand cmd = Connection.CreateCommand();
                cmd.CommandText = string.Format("ALTER TABLE [{0}] ADD COLUMN [{1}] {2} DEFAULT {3}", tableName, columnName, type, defaultValue);
                cmd.ExecuteNonQuery();
                cmd.CommandText = string.Format("UPDATE [{0}] SET [{1}] = {2}", tableName, columnName, defaultValue);
                cmd.ExecuteNonQuery();
            }
        }
        private static void CreateColumn(string tableName, string columnName, string type)
        {
            if(!ColumnExists(tableName, columnName))
            {
                string sql = string.Format("ALTER TABLE [{0}] ADD COLUMN [{1}] {2}", tableName, columnName, type);
                new OleDbCommand(sql, Connection).ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="fromColumnName"></param>
        /// <param name="toColumnsName">here could be list of columns delemitied with ;</param>
        /// <param name="type"></param>
        private static void RenameColumn(string tableName, string fromColumnName, string toColumnsName, string type)
        {
            OleDbCommand command = Connection.CreateCommand();
            if (ColumnExists(tableName, fromColumnName))
            {
                foreach (string toColumnName in toColumnsName.Split(';'))
                {
                    if (!ColumnExists(tableName, toColumnName))
                    {
                        command.CommandText = string.Format("ALTER TABLE [{0}] ADD COLUMN [{1}] {2}", tableName, toColumnName, type);
                        command.ExecuteNonQuery();
                        command.CommandText = string.Format("UPDATE [{0}] SET [{1}] = [{2}]", tableName, toColumnName, fromColumnName);
                        command.ExecuteNonQuery();
                    }
                }
                command.CommandText = string.Format("ALTER TABLE [{0}] DROP COLUMN [{1}]", tableName, fromColumnName);
                command.ExecuteNonQuery();
            }
        }
        private static bool ColumnExists(string tableName, string columnName)
        {
            string filter = string.Format("TABLE_NAME='{0}' AND COLUMN_NAME='{1}'", tableName, columnName);
            DataRow[] result = Connection.GetSchema("COLUMNS").Select(filter);
            return result.Length > 0;
        }
        public static long GetColumnSize(string tableName, string columnName)
        {
            return GetColumnInfo(tableName, columnName).Field<long>("CHARACTER_MAXIMUM_LENGTH");
        }
        private static DataRow GetColumnInfo(string tableName, string columnName)
        {
            string filter = string.Format("TABLE_NAME='{0}' AND COLUMN_NAME='{1}'", tableName, columnName);
            DataRow[] result = Connection.GetSchema("COLUMNS").Select(filter);
            if (result.Length == 1)
            {
                return result[0];
            }
            else
                throw new Exception(string.Format("Cannot find only {0}.{1} database column", tableName, columnName));
        }
        private static void AssureColumnSize(string tableName, string columnName, string dataType, int newSize)
        {
            long columnSize = GetColumnSize(tableName, columnName);

            if (columnSize < newSize)
            {
                OleDbCommand command = Connection.CreateCommand();
                command.CommandText = string.Format("ALTER TABLE [{0}] ALTER COLUMN [{1}] {2}({3})",
                    tableName, columnName, dataType, newSize);
                command.ExecuteNonQuery();
            }
        }

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

        private static OleDbTransaction transaction;
        public static OleDbTransaction Transaction
        {
            get { return transaction; }
        }

        public static OleDbTransaction Begin()
        {
            transaction = connection.BeginTransaction();
            return transaction;
        }
        public static void Commit()
        {
            transaction.Commit();
            transaction = null;
        }
        public static void Rollback()
        {
            transaction.Rollback();
            transaction = null;
        }

        public static OleDbCommand CreateCommand(string commandText)
        {
            return new OleDbCommand(commandText, connection, transaction);
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

        /// <summary>
        /// Generates current date/time with zeroed milliseconds (required by Jet Database to be saved into DBTimeStamp data type) 
        /// </summary>
        public static DateTime Now
        {
            get
            {
                DateTime now = DateTime.Now;
                return new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            }
        }
    }
}
