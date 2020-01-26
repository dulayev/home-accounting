using System;
using System.Data.OleDb;
using System.Text.RegularExpressions;

namespace Home_Accounting
{
    internal class Account
    {
        internal static int? StatementNameToID(string fileName) {
            int? result = null;
            var query = DataUtil.CreateCommand("select ID, StatementName from Account where " +
                "StatementName is not null and Active = True and Cash = False");
            using (OleDbDataReader reader = query.ExecuteReader()) {
                while (reader.Read()) {
                    if (reader[0] is int accountID) {
                        if (reader[1] is string statementPattern) {
                            if (Regex.IsMatch(fileName, statementPattern)) {
                                if (result != null) {
                                    throw new Exception(string.Format("Both {0} and {1} account satisfy file:{2}",
                                        result.Value, accountID, fileName));
                                }
                                result = accountID;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}