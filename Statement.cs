using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Home_Accounting
{
    internal class Statement
    {
        public class Transaction
        {
            public DateTime date;
            public string description;
            public Decimal amount;
            public int category;
            public string sourceText;

            public Transaction()
            {
                category = -1; // uninit value
            }
        };

        static private string PackDescription(string description)
        {
            description = description.Trim();
            Regex regex = new Regex(" +");
            description = regex.Replace(description, " ");
            return description;
        }

        static internal void ImportStatement(List<Statement.Transaction> transactions, int accountID)
        {
            string unresolvedOperations = "";

            foreach (Statement.Transaction transaction in transactions)
            {
                bool res = ImportTransaction(transaction, accountID, true);
                if (!res)
                {
                    unresolvedOperations += transaction.sourceText;
                    unresolvedOperations += "\r\n";
                }
            }
            if (unresolvedOperations.Length != 0)
            {
                MessageBox.Show(unresolvedOperations, "Unresolved Operations");
            }
        }

        internal static bool ImportTransaction(Transaction transaction, int accountID, bool interactive)
        {
            bool res = false;

            OleDbCommand cmd = DataUtil.Connection.CreateCommand();
            cmd.CommandText = string.Format("select ID from BankAccountDebit where AccountID = {0} and Amount = {1} and Checked = false order by [When] asc", accountID, -transaction.amount);
            object transactionID = cmd.ExecuteScalar();
            if (transactionID != null)
            {
                OleDbCommand cmdUpdate = DataUtil.Connection.CreateCommand();
                cmdUpdate.CommandText = "update BankAccountDebit set Checked = True where ID = " + transactionID.ToString();
                cmdUpdate.ExecuteNonQuery();
                res = true;
            }
            else
            {
                AccountForm accountForm = MainForm.GetInstance().GetAccountForm(accountID);
                PurchaseForm purchaseForm = new PurchaseForm(accountForm)
                {
                    Amount = (-transaction.amount).ToString(),
                    Description = PackDescription(transaction.description),
                    When = transaction.date,
                    CategoryID = transaction.category,
                    DebitChecked = true
                };
                //purchaseForm.AutoClose = !interactive; //TODO:uncomment

                res = (purchaseForm.ShowDialog(MainForm.GetInstance()) == System.Windows.Forms.DialogResult.OK);
            }
            return res;
        }

        internal static DataTable GetUncheckedTransactions(int accountID)
        {
            OleDbCommand cmd = DataUtil.Connection.CreateCommand();
            cmd.CommandText = string.Format("select ID, Amount, [When] from BankAccountDebit where AccountID = {0} and " +
                "Checked = false order by [When] asc", accountID);

            OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }
    }
}
