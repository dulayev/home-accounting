using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Home_Accounting
{
    internal class Statement
    {
        public struct Transaction
        {
            public DateTime date;
            public string description;
            public Decimal amount;
            public string sourceText;
        };

        static private string packDescription(string description)
        {
            description = description.Trim();
            Regex regex = new Regex(" +");
            description = regex.Replace(description, " ");
            return description;
        }

        static internal void importStatement(List<Statement.Transaction> transactions, int accountID)
        {
            string unresolvedOperations = "";

            foreach (Statement.Transaction transaction in transactions)
            {
                bool found = false;

                OleDbCommand cmd = DataUtil.Connection.CreateCommand();
                cmd.CommandText = string.Format("select ID from BankAccountDebit where AccountID = {0} and Amount = {1} and Checked = false order by [When] asc", accountID, -transaction.amount);
                object transactionID = cmd.ExecuteScalar();
                if (transactionID != null)
                {
                    found = true;
                    OleDbCommand cmdUpdate = DataUtil.Connection.CreateCommand();
                    cmdUpdate.CommandText = "update BankAccountDebit set Checked = True where ID = " + transactionID.ToString();
                    cmdUpdate.ExecuteNonQuery();
                }

                if (!found)
                {
                    bool recognized = false;
                    /*
                    string[] operationSigns = { "ПОКУПКА ПО КАРТЕ", "ЗАРАБОТНАЯ ПЛАТА", "ALERTING КОМИССИЯ", "ПРОЦЕНТ ПО ДЕПОЗИТУ", "ЭЛЕКТРОННЫЙ ПЛАТЕЖ", "ОПЛАТА УСЛУГ" };
                    foreach(string operationSign in operationSigns)
                    {
                        if(transaction.description.StartsWith(operationSign))
                        {
                            recognized = true;
                            break;
                        }
                    }*/
                    recognized = true;

                    if (recognized)
                    {
                        AccountForm accountForm = MainForm.GetInstance().GetAccountForm(accountID);
                        PurchaseForm purchaseForm = new PurchaseForm(accountForm);

                        purchaseForm.Amount = (-transaction.amount).ToString();
                        purchaseForm.Description = packDescription(transaction.description);
                        purchaseForm.When = transaction.date;
                        purchaseForm.DebitChecked = true;

                        if (purchaseForm.ShowDialog(MainForm.GetInstance()) != System.Windows.Forms.DialogResult.OK)
                        {
                            recognized = false;
                        }
                    }

                    if (!recognized)
                    {
                        unresolvedOperations += transaction.sourceText;
                        unresolvedOperations += "\r\n";
                    }
                }
            }
            if (unresolvedOperations.Length != 0)
            {
                MessageBox.Show(unresolvedOperations, "Unresolved Operations");
            }
        }
    }
}
