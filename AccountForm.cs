using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Reflection;
using System.IO;

namespace Home_Accounting
{
    public partial class AccountForm : Form
    {
        private bool triedToLoadImage = false;

        public int ID;
        public bool Cash;
        private string currency;
        public string Currency
        {
            get { return currency; }
            set
            {
                currency = value;
                labelCurrency.Text = currency;
            }
        }

        private decimal balance;
        public decimal Balance
        {
            get { return balance; }
            set
            { 
                balance = value;
                labelBalance.Text = balance.ToString("0.00");
            }
        }
        public AccountForm(int id)
        {
            InitializeComponent();

            labelBalance.BackColor = //Color.Transparent;
                Color.FromArgb(64, Color.White);

            labelCheck.BackColor = Color.FromArgb(64, Color.White);

            this.ID = id;

            ReloadData();
        }

        private void ReloadData()
        {
            OleDbCommand cmd = new OleDbCommand("select Name, Balance, Cash, [Currency] from [Account] where ID = :id", DataUtil.Connection);
            cmd.Parameters.AddWithValue("id", ID);

            OleDbDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Text = (string)reader["Name"];
                Balance = (decimal)reader["Balance"];
                Cash = (bool)reader["Cash"];
                Currency = reader["Currency"].ToString();
            }
            reader.Close();

            cmd = new OleDbCommand("select [When] from [AccountCheck] where AccountID = :iddd order by id desc", DataUtil.Connection);
            cmd.Parameters.AddWithValue("iddd", ID);
            string lastCheck = "Никогда";
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                lastCheck = ((DateTime)reader["When"]).ToLongDateString();
            }
            reader.Close();
            labelCheck.Text = "Проверка: " + lastCheck;

            if (!triedToLoadImage)
            {
                string file = Assembly.GetExecutingAssembly().Location;
                string path = Path.GetDirectoryName(file);
                for (int i = 0; i < 3; i++)
                {
                    string fileName = path + "\\" + Text + ".jpg";
                    if (File.Exists(fileName))
                    {
                        pictureBox1.Image = Image.FromFile(fileName);
                        break;
                    }
                    path = Path.GetDirectoryName(path);
                }
                triedToLoadImage = true;
            }
        }

        private void AccountForm_MouseDown(object sender, MouseEventArgs e)
        {
            DragDropEffects effects = DoDragDrop(this, DragDropEffects.All);
            if (effects != DragDropEffects.None)
            {
            }
        }

        private void AccountForm_DragOver(object sender, DragEventArgs e)
        {
            AccountForm sourceForm = 
                (AccountForm)e.Data.GetData(typeof(AccountForm));
            if (sourceForm != null)
            {
                if (sourceForm != this)
                {
                    e.Effect = DragDropEffects.Move;
                }
            }
            else if(e.Data.GetData(typeof(DebtForm)) != null)
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void AccountForm_DragDrop(object sender, DragEventArgs e)
        {
            AccountForm sourceForm =
                (AccountForm)e.Data.GetData(typeof(AccountForm));
            if(sourceForm != null)
            {
                new TransactionForm(sourceForm, this).ShowDialog();
            }
            else
            {
                DebtForm debtForm =
                    (DebtForm)e.Data.GetData(typeof(DebtForm));
                if (debtForm != null)
                {
                    OleDbCommand cmd = new OleDbCommand("UPDATE Debt SET Actual = Now() WHERE ID = :id", DataUtil.Connection);
                    cmd.Parameters.Add("id", OleDbType.Integer);

                    foreach (DataRow dataRow in debtForm.SelectedRows)
                    {
                        string text = string.Format("Погашение {0} от {1} в {2}?",
                            dataRow["Amount"], dataRow["Name"], this.Text);
                        DialogResult dialogResult = MessageBox.Show(text, "Долг", MessageBoxButtons.YesNoCancel);
                        if (dialogResult == DialogResult.Cancel)
                            break;
                        else if (dialogResult == DialogResult.Yes)
                        {
                            cmd.Parameters["id"].Value = dataRow["id"];
                            cmd.ExecuteNonQuery();
                        }
                        IncreaseBalance((decimal)dataRow["Amount"]);
                    }
                    debtForm.ReloadDebts();
                }
            }
        }

        public void IncreaseBalance(decimal amount)
        {
            OleDbCommand cmd = new OleDbCommand("update Account Set Balance = Balance + :amount where ID = :ID", DataUtil.Connection);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();

            if (!Cash)
            {
                cmd.Parameters.Clear();
                cmd.CommandText = "INSERT INTO [BankAccountDebit] (AccountID, Amount, [When]) " +
                        "VALUES (:accountID, :amount, Now())";
                cmd.Parameters.AddWithValue("accountID", ID);
                cmd.Parameters.AddWithValue("amount", -amount);
                cmd.ExecuteNonQuery();
            }

            ReloadData();
        }

        private void buttonCheck_Click(object sender, EventArgs e)
        {
            new CheckAccountForm(this).ShowDialog();            
        }

        internal void SetBalance(decimal amount)
        {
            OleDbCommand cmd = new OleDbCommand("update Account Set Balance = :amount where ID = :ID", DataUtil.Connection);
            cmd.Parameters.AddWithValue("amount", amount);
            cmd.Parameters.AddWithValue("ID", ID);
            cmd.ExecuteNonQuery();

            ReloadData();
        }
    }
}