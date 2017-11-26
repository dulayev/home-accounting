using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Home_Accounting
{
    public partial class PurchaseForm : Form
    {
        AccountForm accountForm;
        int categoryID;
        DataTable otherPurchaces = null;
        DataTable computingTable = new DataTable();
        CategoryPicker categoryPicker = new CategoryPicker();
        bool debitChecked = false;

        public PurchaseForm(AccountForm accountForm)
        {
            InitializeComponent();

            this.accountForm = accountForm;

            label1.Text = string.Format(label1.Text, accountForm.Text);
            labelCurrency.Text = accountForm.Currency;

            dateTimePicker1.Value = DataUtil.Now;
        }

        public bool DebitChecked
        {
            set
            {
                debitChecked = value;
            }
        }

        public string Amount
        {
            set
            {
                textBoxAmount.Text = value;
            }
        }

        public string Description
        {
            set
            {
                textBoxDescription.Text = value;
            }
        }

        public DateTime When
        {
            set
            {
                this.dateTimePicker1.Value = value;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            CategoryPicker picker = new CategoryPicker();
            picker.CategoryID = categoryID;
            if (picker.ShowDialog() != DialogResult.Cancel)
            {
                categoryID = picker.CategoryID;
                button3.Text = picker.CategoryName;
            }
        }

        private void PurchaseForm_Load(object sender, EventArgs e)
        {
            otherPurchaces = new DataTable();
            otherPurchaces.Columns.Add("Amount", typeof(decimal));
            otherPurchaces.Columns.Add("Category", typeof(int));
            otherPurchaces.Columns.Add("Name", typeof(string));

            dataGridView1.AutoGenerateColumns = true;
            dataGridView1.DataSource = otherPurchaces;

            //this.ColumnCategory.UseColumnTextForButtonValue = true;

            dataGridView1.CellContentClick += new DataGridViewCellEventHandler(dataGridView1_CellContentClick);
            dataGridView1.CellFormatting += new DataGridViewCellFormattingEventHandler(dataGridView1_CellFormatting);
            dataGridView1.CellParsing += new DataGridViewCellParsingEventHandler(dataGridView1_CellParsing);
        }

        void dataGridView1_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                if (e.Value != null)
                {
                    try
                    {
                        object ooo = computingTable.Compute((string)e.Value, null);
                        e.Value = Convert.ToDecimal(ooo);
                        e.ParsingApplied = true;
                    }
                    catch
                    {
                        e.ParsingApplied = false;
                    }
                }
            }
        }

        void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            switch(e.ColumnIndex)
            {
                case 1:
                    if (e.Value is int)
                    {
                        e.Value = categoryPicker.IDToName((int)e.Value);
                        e.FormattingApplied = true;
                    }
                    break;
            }
        }

        void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                CategoryPicker picker = new CategoryPicker();
                DataGridViewCell cell = dataGridView1.Rows[e.RowIndex].Cells[1];
                if(!(cell.Value is DBNull))
                    picker.CategoryID = (int)cell.Value;
                if (picker.ShowDialog() != DialogResult.Cancel)
                {
                    cell.Value = picker.CategoryID;
                }
            }
        }

        private void PurchaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                try
                {
                    DateTime when = dateTimePicker1.Value;

                    object ooo = computingTable.Compute(this.textBoxAmount.Text, null);
                    decimal amount = Convert.ToDecimal(ooo);
                    accountForm.IncreaseBalance(-amount, when, debitChecked);

                    OleDbCommand cmd = new OleDbCommand("INSERT INTO [Purchase] ( Account, Amount, Category, Name, [Date] ) " +
                        "VALUES (:account, :amount, :category, :name, :when)", DataUtil.Connection);
                    cmd.Parameters.AddWithValue("account", accountForm.ID);
                    cmd.Parameters.Add("amount", OleDbType.Currency);
                    cmd.Parameters.Add("category", OleDbType.Integer);
                    cmd.Parameters.Add("name", OleDbType.VarWChar);
                    cmd.Parameters.AddWithValue("when", when);

                    foreach (DataRow dr in otherPurchaces.Rows)
                    {
                        foreach (DataColumn dc in otherPurchaces.Columns)
                            cmd.Parameters[dc.ColumnName].Value = dr[dc];
                        amount -= (decimal)dr["amount"];
                        cmd.ExecuteNonQuery();
                    }

                    cmd.Parameters["amount"].Value = amount;
                    cmd.Parameters["name"].Value = textBoxDescription.Text;
                    cmd.Parameters["category"].Value = categoryID;
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    e.Cancel = true;
                }
            }
        }

        private void textBoxDescription_TextChanged(object sender, EventArgs e)
        {
            errorProvider1.SetError(textBoxDescription,
                textBoxDescription.TextLength > 50 ?
                string.Format("{0} extra symbols", textBoxDescription.TextLength - 50) : string.Empty);
        }
    }
}