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
    }
}