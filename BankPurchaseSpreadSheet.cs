using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
            base(FormatFilter(accountID), "Exists = False")
        {
            GridForm.GridView.CellFormatting += GridView_CellFormatting;

            this.transactions = transactions;
            this.accountID = accountID;

            base.DataTable = MakeTable();

            base.OnSave += Save;
        }

        private void GridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            bool skip = (Boolean)((DataRowView)((DataGridView)sender).Rows[e.RowIndex].DataBoundItem)["Exists"];

            if (e.CellStyle.Font.Strikeout != skip)
            {
                Font font = new Font(e.CellStyle.Font, FontStyle.Strikeout);
                e.CellStyle.Font = font;
            }
        }

        private DataTable MakeTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Exists", typeof(bool));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Amount", typeof(Decimal));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Authorization", typeof(string));

            DataTable uncheckedTransactions = Statement.GetUncheckedTransactions(accountID);

            foreach (var transaction in transactions)
            {
                object[] row = new object[table.Columns.Count];
                int i = 0;

                DataRow[] uncheckedRows = uncheckedTransactions.Select(string.Format("Amount = {0}", -transaction.amount), "When");
                // TODO: match descriptions
                row[i++] = uncheckedRows.Length > 0;
                if (uncheckedRows.Length > 0)
                {
                    uncheckedTransactions.Rows.Remove(uncheckedRows.First());
                }
                row[i++] = transaction.date;
                row[i++] = "";
                row[i++] = -transaction.amount;
                row[i++] = transaction.description;

                table.Rows.Add(row);
            }

            if (uncheckedTransactions.Rows.Count > 0)
            {
                Debug.Write(string.Format("Left uncheckedTransactions Count: {0}", uncheckedTransactions.Rows.Count));
            }

            return table;
        }

        private void Save(object sender, EventArgs e)
        {
            Statement.importStatement(transactions, accountID);
        }
    }
}
