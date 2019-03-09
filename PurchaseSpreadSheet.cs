using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace Home_Accounting
{
    public class PurchaseSpreadSheet
    {
        private readonly string accountFilter;
        private GridForm gridForm;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.ComboBox comboAccount;
        private System.Windows.Forms.Label labelTotalExpense;

        internal DataTable DataTable {
            get => (DataTable)gridForm.GridView.DataSource;
            set => gridForm.GridView.DataSource = value;
        }

        internal GridForm GridForm { get => gridForm; }
        internal int AccountId { get => (int)comboAccount.SelectedValue; }

        public event System.EventHandler OnSave {
            add => this.buttonSave.Click += value;
            remove => this.buttonSave.Click -= value;
        }

        public DialogResult ShowDialog(IWin32Window owner)
        {
            return gridForm.ShowDialog(owner);
        }

        protected PurchaseSpreadSheet(string accountFilter)
        {
            this.accountFilter = accountFilter;

            gridForm = new GridForm(InitControls());

            gridForm.GridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            gridForm.GridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            gridForm.AcceptButton = this.buttonSave;
            gridForm.Load += new EventHandler(this.PurchaseSpreadSheet_Load);
        }

        private Control[] InitControls()
        {
            Label label = new Label
            {
                AutoSize = true,
                Text = "Account:"
            };

            buttonSave = new Button();
            this.buttonSave.Text = "Save";
            this.buttonSave.DialogResult = System.Windows.Forms.DialogResult.OK;

            comboAccount = new ComboBox();
            this.comboAccount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            labelTotalExpense = new Label
            {
                AutoSize = true,
                Text = "RUB: 999999.99 -> 999999.99" // placeholder
            };

            return new Control[] { label, comboAccount, labelTotalExpense, buttonSave };
        }

        private void PurchaseSpreadSheet_Load(object sender, EventArgs e)
        {
            string query = "Select ID, Name, Balance, Currency from Account where " + accountFilter;
            OleDbDataAdapter adapter = new OleDbDataAdapter(DataUtil.CreateCommand(query));
            DataTable table = new DataTable();
            adapter.Fill(table);
            comboAccount.DataSource = table;

            comboAccount.DisplayMember = "Name";
            comboAccount.ValueMember = "ID";

            UpdateTotalExpense();

            comboAccount.SelectedIndexChanged += new System.EventHandler(ComboAccount_SelectedIndexChanged);
        }

        private void ComboAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTotalExpense();
        }

        private void UpdateTotalExpense()
        {
            DataRowView selectedAccount = (DataRowView)comboAccount.SelectedItem;
            DataTable tableExpenses = (DataTable)gridForm.GridView.DataSource;

            Decimal total = (Decimal)tableExpenses.Compute("SUM(Amount)", null);
            Decimal balance = (Decimal)selectedAccount["Balance"];
            labelTotalExpense.Text = string.Format("{0}: {1} -> {2}",
                selectedAccount["Currency"], balance, balance - total);
        }
    }
}
