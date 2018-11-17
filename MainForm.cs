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
using System.Globalization;

namespace Home_Accounting
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            instance = this;
        }

        OleDbConnection connection;
        private static MainForm instance;

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

            string selectCmdText = "SELECT Account.* FROM         Account WHERE Active <> 0";
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

        internal static MainForm GetInstance()
        {
            return instance;
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

        public AccountForm GetAccountForm(int accountID)
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

        private static string trimSymmetric(string text, char symbol)
        {
            while (text.Length >= 2 && text[0] == symbol && text[text.Length - 1] == symbol)
            {
                text = text.Substring(1, text.Length - 2);
            }
            return text;
        }

        private static string[] splitToFields(string text, char delimiter)
        {
            List<String> list = new List<String>();
            int startField = 0;
            int delimitersMet = 0;
            for(int i = 0; i < text.Length; i++)
            {
                if(text[i] == '"')
                    delimitersMet++;
                else if(text[i] == delimiter && (delimitersMet % 2 == 0))
                {
                    list.Add(text.Substring(startField, i - startField));
                    startField = i + 1;
                }
            }
            list.Add(text.Substring(startField));
            return list.ToArray();
        }

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
                else if(dialog.FileName.EndsWith(".csv")) 
                {
                    if (Path.GetFileName(dialog.FileName).StartsWith("report")) // processing russian standard
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

                        for (int i = 1; i < lines.Length; i++) // ignore first line - header
                        {
                            string line = lines[i];
                            string[] fields = line.Split(',');

                            Transaction transaction = new Transaction();
                            int type = int.Parse(fields[typeIndex]);
                            transaction.date = DateTime.Parse(fields[timeIndex]);
                            transaction.description = fields[descriptionIndex];
                            Decimal amount = Decimal.Parse(fields[amountIndex]);
                            if (type == 0)
                            {
                                amount = -amount;
                            }
                            transaction.amount = amount;
                            transaction.sourceText = line;

                            transactions.Add(transaction);
                        }
                    }
                    else if (Path.GetFileName(dialog.FileName).StartsWith("avangard-")) // processing avangard
                    {
                        // read csv into list
                        string[] lines = System.IO.File.ReadAllLines(dialog.FileName, Encoding.Default);

                        for (int i = 0; i < lines.Length; i++)
                        {
                            string line = lines[i];
                            string[] fields = line.Split(';');
                            for(int j = 0; j < fields.Length; j++)
                            {
                                fields[j] = fields[j].Trim('\"');
                            }

                            Transaction transaction = new Transaction();
                            transaction.sourceText = line;
                            try
                            {
                                // account will be determined by last line
                                accountID = (fields[5] == "*3690") ? 26 : 17;

                                transaction.date = DateTime.Parse(fields[0]);
                                String credit = fields[1];
                                String debit = fields[2];
                                if ((debit == String.Empty) == (credit == String.Empty))
                                    throw new Exception("Only debit or credit should be not empty");

                                if (debit != String.Empty)
                                {
                                    transaction.amount = -Decimal.Parse(debit);
                                }
                                else
                                {
                                    transaction.amount = Decimal.Parse(credit);
                                }
                                transaction.description = String.Format("{0} {1} {2}{3} {4}",
                                    fields[3], fields[4], fields[6], fields[7], fields[9]);
                            }
                            catch(Exception)
                            {
                                transaction.amount = 0;
                            }
                            transactions.Add(transaction);
                        }
                    }
                    else if (Path.GetFileName(dialog.FileName).StartsWith("statement_")) // processing Sankt-Peterburg
                    {
                        accountID = 19;

                        // read csv into list
                        string contents = System.IO.File.ReadAllText(dialog.FileName, Encoding.Default);
                        string[] lines = contents.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = lines.Length - 1; i >= 0; --i)
                        {
                            // bank inserts \n to description, have to split lines on '\r\n', then delete '\n'
                            string line = lines[i].Replace("\n", "");
                            line = line.Replace("Покупка товара НДС не облагается.", ""); // remove useless phrase

                            if (char.IsDigit(line[0]))
                            {
                                string[] fields = splitToFields(line, ';');

                                Transaction transaction = new Transaction();
                                transaction.date = DateTime.Parse(fields[0]);
                                transaction.description = trimSymmetric(fields[2], '\"') + " " + trimSymmetric(fields[3], '\"');
                                transaction.amount = Decimal.Parse(fields[4]);
                                transaction.sourceText = line;

                                transactions.Add(transaction);
                            }
                        }
                    }
                    else if (Path.GetFileName(dialog.FileName).StartsWith("alfa-")) // processing Alfa
                    {
                        // read csv into list
                        string[] lines = System.IO.File.ReadAllLines(dialog.FileName, Encoding.Default);
                        bool alex_account = (null != Array.Find<string>(lines, delegate(string s) { return s.Contains("40817810704370064412"); }));
                        accountID = alex_account ? 27 : 23;
                        for (int i = lines.Length - 1; i >= 0; --i)
                        {
                            string line = lines[i];
                            string[] fields = splitToFields(line, ';');

                            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                            nfi.NumberDecimalSeparator = ",";

                            if (char.IsDigit(fields[1][0])) // account number is digit
                            {

                                Transaction transaction = new Transaction();
                                transaction.date = DateTime.Parse(fields[3]);
                                transaction.description = fields[5];
                                transaction.amount = Decimal.Parse(fields[6], nfi) - Decimal.Parse(fields[7], nfi);
                                transaction.sourceText = line;

                                transactions.Add(transaction);
                            }
                        }
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

        private void clipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PurchaseSpreadSheet form = new PurchaseSpreadSheet();
            form.ShowDialog(this);
        }
    }
}