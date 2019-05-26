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

            bool sameCurrency = srcAccount.Currency.Equals(dstAccount.Currency);
            textBoxDstAmount.ReadOnly = sameCurrency;
        }

        private void TransactionForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
            {
                try
                {
                    decimal srcAmount = decimal.Parse(textBoxSrcAmount.Text);
                    decimal dstAmount = decimal.Parse(textBoxDstAmount.Text);

                    DateTime when = DataUtil.Now;
                    srcAccount.IncreaseBalance(-srcAmount, when);
                    dstAccount.IncreaseBalance(dstAmount, when);

                    OleDbCommand cmd = new OleDbCommand("INSERT INTO [Transaction] ( SourceAmount, DestinationAmount, Source, Destination, [Date] ) " +
                        "VALUES (:srcAmount, :dstAmount, :srcID, :dstID, :when)", DataUtil.Connection);
                    cmd.Parameters.AddWithValue("srcAmount", srcAmount);
                    cmd.Parameters.AddWithValue("dstAmount", dstAmount);
                    cmd.Parameters.AddWithValue("srcID", srcAccount.ID);
                    cmd.Parameters.AddWithValue("dstID", dstAccount.ID);
                    cmd.Parameters.AddWithValue("when", FormUtil.GetDate(monthCalendar1));
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    e.Cancel = true;
                }
            }
        }

        private void TextBoxSrcAmount_TextChanged(object sender, EventArgs e)
        {
            if (textBoxDstAmount.ReadOnly)
            {
                textBoxDstAmount.Text = textBoxSrcAmount.Text;
            }
            UpdateRate();
        }

        private void TextBoxDstAmount_TextChanged(object sender, EventArgs e)
        {
            UpdateRate();
        }

        private void UpdateRate()
        {
            try
            {
                decimal srcAmount = decimal.Parse(textBoxSrcAmount.Text);
                decimal dstAmount = decimal.Parse(textBoxDstAmount.Text);
                decimal min = Math.Min(srcAmount, dstAmount);
                decimal max = Math.Max(srcAmount, dstAmount);
                labelRate.Text = (max / min).ToString();
            }
            catch
            {
                labelRate.Text = "?";
            }
        }
    }
}