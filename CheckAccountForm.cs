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
    public partial class CheckAccountForm : Form
    {
        AccountForm accountForm = null;

        public CheckAccountForm(AccountForm accountForm)
        {
            InitializeComponent();

            this.accountForm = accountForm;
        }

        private void CheckAccountForm_Load(object sender, EventArgs e)
        {
            textBoxExpected.Text = accountForm.Balance.ToString();
            textBoxActual.Text = accountForm.Balance.ToString();
        }

        private void CheckAccountForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(DialogResult != DialogResult.OK)
                return;

            try
            {
                decimal actual = decimal.Parse(textBoxActual.Text);
                string sql = 
                    "INSERT INTO [AccountCheck] (AccountID, Expected, Actual, [When]) "+
                    "VALUES (:accountID, :expected, :actual, Now())";
                OleDbCommand cmd = new OleDbCommand(sql, DataUtil.Connection);
                cmd.Parameters.AddWithValue("accountID", accountForm.ID);
                cmd.Parameters.AddWithValue("expected", accountForm.Balance);
                cmd.Parameters.AddWithValue("actual", actual);
                cmd.ExecuteNonQuery();

                accountForm.SetBalance(actual);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                e.Cancel = true;
            }
        }
    }
}