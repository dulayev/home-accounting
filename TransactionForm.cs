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
    public partial class TransactionForm : Form
    {
        AccountForm srcAccount;
        AccountForm dstAccount;

        public TransactionForm(AccountForm srcAccount, AccountForm dstAccount)
        {
            InitializeComponent();

            this.srcAccount = srcAccount;
            this.dstAccount = dstAccount;

            labelSrc.Text = srcAccount.Text;
            labelDst.Text = dstAccount.Text;
        }

        private void TransactionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                try
                {
                    decimal amount = decimal.Parse(textBox1.Text);

                    srcAccount.IncreaseBalance(-amount);
                    dstAccount.IncreaseBalance(amount);

                    OleDbCommand cmd = new OleDbCommand("INSERT INTO [Transaction] ( Amount, Source, Destination, [Date] ) " +
                        "VALUES (:amount, :srcID, :dstID, Now())", DataUtil.Connection);
                    cmd.Parameters.AddWithValue("amount", amount);
                    cmd.Parameters.AddWithValue("srcID", srcAccount.ID);
                    cmd.Parameters.AddWithValue("dstID", dstAccount.ID);
                    cmd.ExecuteNonQuery();
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