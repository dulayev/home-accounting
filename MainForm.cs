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

        private void importToolStripMenuItem_Click(bool check)
        {
            int accountID = 12;

            OpenFileDialog dialog = new OpenFileDialog();
            //dialog.Filter = "*.xml";
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                // processing citibank
                
                //Stream stream = dialog.OpenFile();
                string contents = System.IO.File.ReadAllText(dialog.FileName, Encoding.Default);
                //Xml

                string unresolvedOperations = "";
                //XmlTextReader textReader = new XmlTextReader(stream);
                //textReader

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(contents);
                //XmlReaderSettings settings = new XmlReaderSettings();
                //XmlReader reader = XmlReader.Create(stream, settings);
                XmlNodeList elements = doc.SelectNodes("Transactions/Transaction");
                for(int i = elements.Count - 1; i >= 0; i--)
                {
                    XmlElement element = elements[i] as XmlElement;
                    DateTime date = DateTime.Parse(element["date"].InnerText);
                    string description = element["description"].InnerText;
                    Decimal amount = Decimal.Parse(element["amount"].InnerText);
                    //"account_number"
                    //"40817810430017806653"

                    bool found = false;

                    if (check)
                    {
                        OleDbCommand cmd = DataUtil.Connection.CreateCommand();
                        cmd.CommandText = string.Format("select ID from BankAccountDebit where AccountID = {0} and Amount = {1} and Checked = false order by [When] asc", accountID, -amount);
                        object transactionID = cmd.ExecuteScalar();
                        if (transactionID != null)
                        {
                            found = true;
                            OleDbCommand cmdUpdate = DataUtil.Connection.CreateCommand();
                            cmdUpdate.CommandText = "update BankAccountDebit set Checked = True where ID = " + transactionID.ToString();
                            cmdUpdate.ExecuteNonQuery();
                        }
                    }

                    if (!found)
                    {
                        string[] operationSigns = { "ПОКУПКА ПО КАРТЕ", "ЗАРАБОТНАЯ ПЛАТА", "ALERTING КОМИССИЯ", "ПРОЦЕНТ ПО ДЕПОЗИТУ", "ЭЛЕКТРОННЫЙ ПЛАТЕЖ", "ОПЛАТА УСЛУГ" };
                        bool recognized = false;
                        int retainFirstCharsLen = 0;
                        foreach(string operationSign in operationSigns)
                        {
                            if(description.StartsWith(operationSign))
                            {
                                recognized = true;
                                retainFirstCharsLen = operationSign.Length;
                                break;
                            }
                        }

                        if (recognized)
                        {
                            AccountForm accountForm = GetAccountForm(accountID);
                            PurchaseForm purchaseForm = new PurchaseForm(accountForm);

                            purchaseForm.Amount = (-amount).ToString();
                            purchaseForm.Description = packDescription(description);
                            purchaseForm.When = date;
                            purchaseForm.DebitChecked = true;

                            if (purchaseForm.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                            {
                                recognized = false;
                            }
                        }
                        
                        if(!recognized)
                        {
                            unresolvedOperations += element.OuterXml;
                            unresolvedOperations += "\r\n";
                        }
                    }
                }

                if (unresolvedOperations.Length != 0)
                {
                    MessageBox.Show(unresolvedOperations, "Unresolved Operations");
                }
            }
        }

        private void checkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(true);
        }

        private void fillToolStripMenuItem_Click(object sender, EventArgs e)
        {
            importToolStripMenuItem_Click(false);
        }
    }
}