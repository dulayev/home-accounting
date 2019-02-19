using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Home_Accounting
{
    class BankPurchaseSpreadSheet : PurchaseSpreadSheet
    {
        private readonly List<Statement.Transaction> transactions;
        private readonly int accountID;

        private static string FormatFilter(int accountID)
        {
            return string.Format("ID = {0}", accountID);
        }

        public BankPurchaseSpreadSheet(List<Statement.Transaction> transactions, int accountID) :
            base(FormatFilter(accountID))
        {
            this.transactions = transactions;
            this.accountID = accountID;

            base.DataTable = MakeTable();

            base.OnSave += Save;
        }

        private DataTable MakeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Amount", typeof(Decimal));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Authorization", typeof(string));

            foreach (var transaction in transactions)
            {
                object[] row = new object[table.Columns.Count];
                int i = 0;
                row[i++] = transaction.date;
                row[i++] = "";
                row[i++] = transaction.amount;
                row[i++] = transaction.description;

                table.Rows.Add(row);
            }

            return table;
        }

        private void Save(object sender, EventArgs e)
        {
            Statement.importStatement(transactions, accountID);
        }
    }
}
