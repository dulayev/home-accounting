using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.IO;
using System.Collections;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using System.Data.Common;
using System.Xml;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Home_Accounting
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        OleDbConnection connection; 
        private void MainForm_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
            if (Properties.Settings.Default["MainWindowRestoreBounds"] != null)
                Bounds = Properties.Settings.Default.MainWindowRestoreBounds;
            if(Properties.Settings.Default["MainWindowState"] != null)
                WindowState = Properties.Settings.Default.MainWindowState;

            foreach (Control control in Controls)
            {
                MdiClient mdiClient = control as MdiClient;
                if (mdiClient != null)
                    mdiClient.BackColor = BackColor;
            }

            ConnectionStringSettings css = ConfigurationManager.ConnectionStrings["Home_Accounting.Properties.Settings.Home_AccountingConnectionString"];

            connection = (OleDbConnection)DbProviderFactories.GetFactory(css.ProviderName).CreateConnection();
            connection.ConnectionString = css.ConnectionString;

            try
            {
                connection.Open();
                DataUtil.Connection = connection;
                DataUtil.UpgradeDatabase();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Application.Exit();
            }

            string selectCmdText = "SELECT Account.* FROM         Account";
            OleDbDataAdapter adapter = new OleDbDataAdapter(selectCmdText, connection);
            DataTable tableAccounts = new DataTable();
            adapter.Fill(tableAccounts);

            foreach (DataRow dr in tableAccounts.Rows)
            {
                AccountForm accountForm = new AccountForm((int)dr["ID"]);
                accountForm.MdiParent = this;

                accountForm.Show();
            }

            ShopForm shopForm = new ShopForm();
            shopForm.MdiParent = this;
            shopForm.Show();

            DebtForm debtForm = new DebtForm();
            debtForm.MdiParent = this;
            debtForm.Show();

            //ReportForm reportForm = new ReportForm();
            //reportForm.MdiParent = this;
            //reportForm.Show();

            if (Properties.Settings.Default.WindowLocations != null)
            {
                try
                {
                    byte[] b = Properties.Settings.Default.WindowLocations;
                    Dictionary<string, Point> locations;
                    BinaryFormatter formatter = new BinaryFormatter();
                    //XmlSerializer xmlSerializer = new XmlSerializer(typeof(Hashtable));
                    locations = (Dictionary<string, Point>)formatter.Deserialize(new MemoryStream(b));

                    foreach (Form form in MdiChildren)
                    {
                        if (locations.ContainsKey(form.Text))
                            form.Location = locations[form.Text];
                    }
                }
                catch
                {
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Properties.Settings.Default["MainWindowRestoreBounds"] = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
            Properties.Settings.Default["MainWindowState"] = WindowState;

            Dictionary<string, Point> locations = new Dictionary<string,Point>();
            foreach (Form form in MdiChildren)
                locations[form.Text] = form.Location;

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream sw = new MemoryStream();
            formatter.Serialize(sw, locations);
            Properties.Settings.Default.WindowLocations = sw.ToArray();

            Properties.Settings.Default.Save();
        }

        private AccountForm GetAccountForm(int accountID)
        {
            foreach(Form form in MdiChildren)
            {
                AccountForm accountForm = form as AccountForm;
                if (accountForm != null && accountForm.ID == accountID)
                {
                    return accountForm;
                }
            }

            return null;
        }

        private string packDescription(string description)
        {
            description = description.Trim();
            Regex regex = new Regex(" +");
            description = regex.Replace(description, " ");
            return description;
        }

        struct Transaction
        {
            public DateTime date;
            public string description;
            public Decimal amount;
            public string sourceText;
        };

        private void loadStatement()
        {
            int accountID = 0;

            List<Transaction> transactions = new List<Transaction>();

            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                if (dialog.FileName.EndsWith(".xml")) // processing citibank
                {
                    accountID = 12;
                    string contents = System.IO.File.ReadAllText(dialog.FileName, Encoding.Default);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(contents);
                    XmlNodeList elements = doc.SelectNodes("Transactions/Transaction");
                    for(int i = elements.Count - 1; i >= 0; i--)
                    {
                        Transaction transaction = new Transaction();

                        XmlElement element = elements[i] as XmlElement;
                        transaction.date = DateTime.Parse(element["date"].InnerText);
                        transaction.description = element["description"].InnerText;
                        transaction.amount = Decimal.Parse(element["amount"].InnerText);
                        transaction.sourceText = element.OuterXml;

                        transactions.Add(transaction);
                    }
                }
                else if(dialog.FileName.EndsWith(".csv")) // processing citibank
                {
                    accountID = 13;

                    // read csv into list
                    string[] lines = System.IO.File.ReadAllLines(dialog.FileName, Encoding.Default);
                    string[] headerFields = lines[0].Split(',');
                    int typeIndex = Array.IndexOf(headerFields, "type");
                    Trace.Assert(typeIndex >= 0);
                    int amountIndex = Array.IndexOf(headerFields, "amount");
                    Trace.Assert(amountIndex >= 0);
                    int descriptionIndex = Array.IndexOf(headerFields, "description");
                    Trace.Assert(descriptionIndex >= 0);
                    int timeIndex = Array.IndexOf(headerFields, "timestamp");
                    Trace.Assert(timeIndex >= 0);

                    for(int i = 1; i < lines.Length; i++) // ignore first line - header
                    {
                        string line = lines[i];
                        string[] fields = line.Split(',');

                        Transaction transaction = new Transaction();
                        int type = int.Parse(fields[typeIndex]);
                        transaction.date = DateTime.Parse(fields[timeIndex]);
                        transaction.description = fields[descriptionIndex];
                        Decimal amount = Decimal.Parse(fields[amountIndex]);
                        if(type == 0)
                        {
                            amount = -amount;
                        }
                        transaction.amount = amount;
                        transaction.sourceText = line;

                        transactions.Add(transaction);
                    }
                }

                if(accountID == 0)
                {
                    MessageBox.Show(dialog.FileName + " is unknown");
                }
                else
                {
                    importStatement(transactions, accountID);
                }
            }
        }

        private void importStatement(List<Transaction> transactions, int accountID)
        {
            string unresolvedOperations = "";

            foreach(Transaction transaction in transactions)
            {
                bool found = false;

                OleDbCommand cmd = DataUtil.Connection.CreateCommand();
                cmd.CommandText = string.Format("select ID from BankAccountDebit where AccountID = {0} and Amount = {1} and Checked = false order by [When] asc", accountID, -transaction.amount);
                object transactionID = cmd.ExecuteScalar();
                if (transactionID != null)
                {
                    found = true;
                    OleDbCommand cmdUpdate = DataUtil.Connection.CreateCommand();
                    cmdUpdate.CommandText = "update BankAccountDebit set Checked = True where ID = " + transactionID.ToString();
                    cmdUpdate.ExecuteNonQuery();
                }

                if (!found)
                {
                    bool recognized = false;
                    /*
                    string[] operationSigns = { "ПОКУПКА ПО КАРТЕ", "ЗАРАБОТНАЯ ПЛАТА", "ALERTING КОМИССИЯ", "ПРОЦЕНТ ПО ДЕПОЗИТУ", "ЭЛЕКТРОННЫЙ ПЛАТЕЖ", "ОПЛАТА УСЛУГ" };
                    foreach(string operationSign in operationSigns)
                    {
                        if(transaction.description.StartsWith(operationSign))
                        {
                            recognized = true;
                            break;
                        }
                    }*/
                    recognized = true;

                    if (recognized)
                    {
                        AccountForm accountForm = GetAccountForm(accountID);
                        PurchaseForm purchaseForm = new PurchaseForm(accountForm);

                        purchaseForm.Amount = (-transaction.amount).ToString();
                        purchaseForm.Description = packDescription(transaction.description);
                        purchaseForm.When = transaction.date;
                        purchaseForm.DebitChecked = true;

                        if (purchaseForm.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                        {
                            recognized = false;
                        }
                    }
                        
                    if(!recognized)
                    {
                        unresolvedOperations += transaction.sourceText;
                        unresolvedOperations += "\r\n";
                    }
                }
            }
            if (unresolvedOperations.Length != 0)
            {
                MessageBox.Show(unresolvedOperations, "Unresolved Operations");
            }
        }

        private void checkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadStatement();
        }
    }
}