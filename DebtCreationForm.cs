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
    public partial class DebtCreationForm : Form
    {
        AccountForm accountForm;
        DebtForm debtForm;
        
        public DebtCreationForm(AccountForm accountForm, DebtForm debtForm)
        {
            this.accountForm = accountForm;
            this.debtForm = debtForm;

            InitializeComponent();
        }

        private void DebtCreationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                try
                {
                    decimal amount = decimal.Parse(textBox2.Text);
                    DateTime when = DataUtil.Now;

                    string sql = "INSERT INTO Debt (Name, Lend, Plan, Amount, [Currency]) " +
                        "VALUES (:name, :when, :plan, :amount, :currency)";
                    OleDbCommand cmd = new OleDbCommand(sql, DataUtil.Connection);
                    cmd.Parameters.AddWithValue("name", textBox1.Text);
                    cmd.Parameters.AddWithValue("when", when);
                    cmd.Parameters.AddWithValue("plan", monthCalendar1.SelectionStart);
                    cmd.Parameters.AddWithValue("amount", amount);
                    cmd.Parameters.AddWithValue("currency", accountForm.Currency);
                    cmd.ExecuteNonQuery();

                    accountForm.IncreaseBalance(-amount, when);
                    debtForm.ReloadDebts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    e.Cancel = true;
                }
            }
        }
    }
}