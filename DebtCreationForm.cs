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
                    decimal amount = decimal.Parse(textBoxLendAmount.Text);
                    decimal amountBack = decimal.Parse(textBoxPayBackAmount.Text);
                    DateTime when = FormUtil.GetDate(monthCalendarLend);
                    DateTime whenBack = FormUtil.GetDate(monthCalendarPayBack);

                    if (whenBack <= when)
                    {
                        throw new Exception("Payback date should be later than lending date");
                    }

                    string sql = "INSERT INTO Debt (Name, Lend, Plan, Amount, [Currency], AmountBack) " +
                        "VALUES (:name, :when, :plan, :amount, :currency, :amountBack)";
                    OleDbCommand cmd = new OleDbCommand(sql, DataUtil.Connection);
                    cmd.Parameters.AddWithValue("name", textBoxName.Text);
                    cmd.Parameters.AddWithValue("when", when);
                    cmd.Parameters.AddWithValue("plan", whenBack);
                    cmd.Parameters.AddWithValue("amount", amount);
                    cmd.Parameters.AddWithValue("currency", accountForm.Currency);
                    cmd.Parameters.AddWithValue("amountBack", amountBack);
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

        private void TextBoxLendAmount_TextChanged(object sender, EventArgs e)
        {
            textBoxPayBackAmount.Text = textBoxLendAmount.Text;
            UpdateAnnualRate();
        }

        private void TextBoxPayBackAmount_TextChanged(object sender, EventArgs e)
        {
            UpdateAnnualRate();
        }

        private void UpdateAnnualRate()
        {
            if (Decimal.TryParse(textBoxLendAmount.Text, out decimal lend) &&
                Decimal.TryParse(textBoxPayBackAmount.Text, out decimal payBack) &&
                lend != payBack &&
                monthCalendarLend.SelectionStart != monthCalendarPayBack.SelectionStart)
            {
                int daysCount = (monthCalendarPayBack.SelectionStart - monthCalendarLend.SelectionStart).Days;
                Decimal profit = payBack - lend;
                Decimal annualRate = profit / lend * 365 / daysCount;
                labelAnnualRate.Text = annualRate.ToString("P");
            }
            else
            {
                labelAnnualRate.Text = "";
            }
        }

        private void MonthCalendarLend_DateChanged(object sender, DateRangeEventArgs e)
        {
            UpdateAnnualRate();
        }

        private void MonthCalendarPayBack_DateChanged(object sender, DateRangeEventArgs e)
        {
            UpdateAnnualRate();
        }
    }
}