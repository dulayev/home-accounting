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
            if (Properties.Settings.Default["MainWindowState"] != null)
                WindowState = Properties.Settings.Default.MainWindowState;

            foreach (Control control in Controls)
            {
                if (control is MdiClient mdiClient)
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
            catch (Exception ex)
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
                AccountForm accountForm = new AccountForm((int)dr["ID"])
                {
                    MdiParent = this
                };

                accountForm.Show();
            }

            ShopForm shopForm = new ShopForm
            {
                MdiParent = this
            };
            shopForm.Show();

            DebtForm debtForm = new DebtForm
            {
                MdiParent = this
            };
            debtForm.Show();

            CreateOperationsForm();

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

        private void CreateOperationsForm()
        {
            ComboBox comboAccount = new ComboBox();
            string query = "Select ID, Name from Account where Active";
            OleDbDataAdapter adapter = new OleDbDataAdapter(DataUtil.CreateCommand(query));
            DataTable table = new DataTable();
            adapter.Fill(table);
            comboAccount.DataSource = table;
            comboAccount.DisplayMember = "Name";
            comboAccount.ValueMember = "ID";
            comboAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            Control[] controls = new Control[] { comboAccount };

            OleDbCommand cmd = new OleDbCommand(null, DataUtil.Connection);
            QueryForm queryForm = new QueryForm(controls, cmd);

            void updateQuery(object sender, EventArgs e)
            {
                queryForm.DbCommand.CommandText = string.Format(
                    "SELECT * FROM Purchase where Account = {0} order by Date desc",
                    comboAccount.SelectedValue);
                queryForm.Reload();
            }

            comboAccount.SelectedValueChanged += updateQuery;
            // when become visible, comboAccount.SelectedValue gets value and reasonable sql could be made
            comboAccount.VisibleChanged += updateQuery;

            DataUtil.OnAccountUpdate += (int accountId) =>
            {
                if (comboAccount.SelectedValue.Equals(accountId))
                {
                    queryForm.Reload();
                }
            };

            queryForm.Text = "Operations";
            queryForm.MdiParent = this;
            queryForm.Show();
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
                if (form is AccountForm accountForm && accountForm.ID == accountID)
                {
                    return accountForm;
                }
            }

            return null;
        }

        private static string TrimSymmetric(string text, char symbol)
        {
            while (text.Length >= 2 && text[0] == symbol && text[text.Length - 1] == symbol)
            {
                text = text.Substring(1, text.Length - 2);
            }
            return text;
        }

        private static string[] SplitToFields(string text, char delimiter)
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

        private void LoadStatement()
        {
            int accountID = 0;

            List<Statement.Transaction> transactions = new List<Statement.Transaction>();

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
                        Statement.Transaction transaction = new Statement.Transaction();

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

                            Statement.Transaction transaction = new Statement.Transaction();
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

                            Statement.Transaction transaction = new Statement.Transaction
                            {
                                sourceText = line
                            };
                            try
                            {
                                // account will be determined by last line
                                accountID = (fields[5] == "*0559") ? 26 : 17; // TODO: Match by account name

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
                        int? validLineIndex = null;
                        for (int i = lines.Length - 1; i >= 0; --i)
                        {
                            // bank inserts \n to description, have to split lines on '\r\n', then delete '\n'
                            string line = lines[i].Replace("\n", "");
                            line = line.Replace("Покупка товара НДС не облагается.", ""); // remove useless phrase

                            string[] fields = SplitToFields(line, ';');
                            if (fields.Length - 1 < 4) continue;

                            if (!DateTime.TryParse(fields[0], out DateTime inDate)) continue;
                            if (!Decimal.TryParse(fields[4], out Decimal inAmount)) continue;

                            Statement.Transaction transaction = new Statement.Transaction
                            {
                                date = inDate,
                                description = TrimSymmetric(fields[2], '\"') + " " + TrimSymmetric(fields[3], '\"'),
                                amount = inAmount,
                                sourceText = line
                            };

                            transactions.Add(transaction);

                            if (validLineIndex.HasValue && Math.Abs(validLineIndex.Value - i) != 1)
                            {
                                throw new Exception(string.Format("Valid %d and %d lines aren't adjacent", validLineIndex.Value, i));
                            }
                            validLineIndex = i;
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
                            string[] fields = SplitToFields(line, ';');

                            NumberFormatInfo nfi = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                            nfi.NumberDecimalSeparator = ",";

                            if (char.IsDigit(fields[1][0])) // account number is digit
                            {

                                Statement.Transaction transaction = new Statement.Transaction
                                {
                                    date = DateTime.Parse(fields[3]),
                                    description = fields[5],
                                    amount = Decimal.Parse(fields[6], nfi) - Decimal.Parse(fields[7], nfi),
                                    sourceText = line
                                };

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
                    var form = new BankPurchaseSpreadSheet(transactions, accountID);
                    form.Show(this);
                }
            }
        }

        private void CheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadStatement();
        }

        private void ClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new CashPurchaseSpreadSheet();
            form.ShowDialog(this);
        }
    }
}