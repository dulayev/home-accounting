using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Home_Accounting
{
    public class CashPurchaseSpreadSheet : PurchaseSpreadSheet
    {
        public CashPurchaseSpreadSheet() : base("Cash and Active")
        {
            GridForm.GridView.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.Grid_CellPainting);
            GridForm.Text = "Cash Purchases";

            base.DataTable = PasteClipboard();
            base.OnSave += Save;
            GridForm.GridView.CellPainting += Grid_CellPainting;
        }

        private DataTable PasteClipboard()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Amount", typeof(Decimal));
            table.Columns.Add("Description", typeof(string));

            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo
            {
                ShortDatePattern = "MM/dd/yyyy"
            };

            string text = Clipboard.GetText();
            if (text != null)
            {
                string[] lines = text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string line in lines)
                {
                    string[] fields = line.Split('\t');
                    object[] row = new object[fields.Length];
                    fields.CopyTo(row, 0);
                    row[0] = Convert.ToDateTime(fields[0], dtFormat);
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        private void Grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && 
                GridForm.GridView.Columns[e.ColumnIndex].Name == "Category")
            {
                if (!new Category().Exists(e.Value as string))
                {
                    e.CellStyle.BackColor = Color.LightPink;
                    e.CellStyle.SelectionBackColor = Color.LightPink;
                }
            }
            e.Handled = false;
        }

        private void Save(object sender, EventArgs e)
        {
            AccountForm accountForm = MainForm.GetInstance().GetAccountForm(AccountId);
            Category categoryService = new Category();

            DataTable table = base.DataTable;
            for (int i = 0; i < table.Rows.Count; )
            {
                DataRow row = table.Rows[i];
                int categoryId = categoryService.CreateMissing((string)row["Category"]);
                // always re-create form to it catches new categories
                PurchaseForm purchaseForm = new PurchaseForm(accountForm)
                {
                    Amount = ((Decimal)row["amount"]).ToString(),
                    Description = (string)row["Description"],
                    When = (DateTime)row["Date"],
                    CategoryID = categoryId,
                    DebitChecked = false
                };

                if (purchaseForm.ShowDialog(GridForm) == System.Windows.Forms.DialogResult.OK)
                {
                    row.Delete(); // causes exception about loop failure
                }
                else
                {
                    i++;
                }
            }
        }
    }
}
