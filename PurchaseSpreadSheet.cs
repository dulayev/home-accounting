using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Home_Accounting
{
    public partial class PurchaseSpreadSheet : Form
    {
        public PurchaseSpreadSheet()
        {
            InitializeComponent();
        }

        private void PurchaseSpreadSheet_Load(object sender, EventArgs e)
        {
            string query = "Select ID, Name, Balance, Currency from Account where Cash and Active";
            OleDbDataAdapter adapter = new OleDbDataAdapter(DataUtil.CreateCommand(query));
            DataTable table = new DataTable();
            adapter.Fill(table);
            comboAccount.DataSource = table;

            comboAccount.DisplayMember = "Name";
            comboAccount.ValueMember = "ID";

            grid.DataSource = PasteClipboard();

            UpdateTotalExpense();

            this.comboAccount.SelectedIndexChanged += new System.EventHandler(this.comboAccount_SelectedIndexChanged);
        }

        private void UpdateTotalExpense()
        {
            DataRowView selectedAccount = (DataRowView)comboAccount.SelectedItem;
            DataTable tableExpenses = (DataTable)grid.DataSource;

            Decimal total = (Decimal)tableExpenses.Compute("SUM(Amount)", null);
            Decimal balance = (Decimal)selectedAccount["Balance"];
            labelTotalExpense.Text = string.Format("{0}: {1} -> {2}", 
                selectedAccount["Currency"], balance, balance - total);
        }

        private DataTable PasteClipboard()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Amount", typeof(Decimal));
            table.Columns.Add("Description", typeof(string));

            DateTimeFormatInfo dtFormat = new DateTimeFormatInfo();
            dtFormat.ShortDatePattern = "MM/dd/yyyy";

            string text = Clipboard.GetText();
            if (text != null)
            {
                string[] lines = text.Split(new string[]{Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
                foreach(string line in lines) {
                    string[] fields = line.Split('\t');
                    object[] row = new object[fields.Length];
                    fields.CopyTo(row, 0);
                    row[0] = Convert.ToDateTime(fields[0], dtFormat);
                    table.Rows.Add(row);
                }
            }
            return table;
        }

        private void grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "Category")
            {
                if (!new Category().Exists(e.Value as string)) {
                    e.CellStyle.BackColor = Color.LightPink;
                }
            }
            e.Handled = false;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            AccountForm accountForm = MainForm.GetInstance().GetAccountForm((int)comboAccount.SelectedValue);
            Category categoryService = new Category();

            DataTable table = (DataTable)grid.DataSource;
            for (int i = 0; i < table.Rows.Count; )
            {
                DataRow row = table.Rows[i];
                int categoryId = categoryService.CreateMissing((string)row["Category"]);
                // always re-create form to it catches new categories
                PurchaseForm purchaseForm = new PurchaseForm(accountForm);
                purchaseForm.Amount = ((Decimal)row["amount"]).ToString();
                purchaseForm.Description = (string)row["Description"];
                purchaseForm.When = (DateTime)row["Date"];
                purchaseForm.CategoryID = categoryId;
                purchaseForm.DebitChecked = false;

                if (purchaseForm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    row.Delete(); // causes exception about loop failure
                } else
                {
                    i++;
                }
            }
        }

        private void comboAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTotalExpense();
        }
    }
}
