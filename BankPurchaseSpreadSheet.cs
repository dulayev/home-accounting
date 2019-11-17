using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Home_Accounting
{
    class BankPurchaseSpreadSheet : PurchaseSpreadSheet
    {
        private readonly int accountID;
        private Button pasteAuthorizations = new Button() { Text = "Paste Auth" };

        private static string FormatFilter(int accountID)
        {
            return string.Format("ID = {0}", accountID);
        }

        public BankPurchaseSpreadSheet(List<Statement.Transaction> transactions, int accountID) :
            base(FormatFilter(accountID), "Exists = False")
        {
            GridForm.GridView.CellFormatting += GridView_CellFormatting;
            GridForm.GridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
            GridForm.GridView.AllowUserToResizeColumns = true;

            GridForm.Load += GridForm_Load;

            this.accountID = accountID;

            base.DataTable = MakeTable(transactions);

            base.OnSave += Save;

            pasteAuthorizations.Click += PasteAuthorizations_Click;
            GridForm.AddControl(pasteAuthorizations);
        }

        const string DATE = "Date";
        const string AMOUNT = "Amount";
        const string CATEGORY = "Category";
        const string DESCRIPTION = "Description";

        private void GridForm_Load(object sender, EventArgs e)
        {
            DataGridViewColumn column = GridForm.GridView.Columns[CATEGORY];
            if (column != null)
            {
                int index = GridForm.GridView.Columns.IndexOf(column);
                GridForm.GridView.Columns.Remove(column);
                DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn
                {
                    Name = CATEGORY,
                    UseColumnTextForButtonValue = true
                };
                GridForm.GridView.Columns.Insert(index, buttonColumn);

                GridForm.GridView.CellClick += GridView_CellClick;
                GridForm.GridView.CellContextMenuStripNeeded += GridView_CellContextMenuStripNeeded;

                string commandText = "SELECT Name, Category FROM Purchase " +
                    "where Account = :AccountID and Date >= :When order by Date desc";
                OleDbCommand command = DataUtil.CreateCommand(commandText);
                command.Parameters.AddWithValue("AccountID", accountID);
                command.Parameters.AddWithValue("When", DataUtil.Now.AddYears(-1));

                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                DataTable srcTable = new DataTable();
                adapter.Fill(srcTable);

                DataTable dstTable = (DataTable)GridForm.GridView.DataSource;

                DataColumn columnSrcDescripion = srcTable.Columns["Name"];
                DataColumn columnDstDescripion = dstTable.Columns[DESCRIPTION];
                DataColumn columnSrcCategory = srcTable.Columns[CATEGORY];
                //DataColumn columnDstCategory = dstTable.Columns[CATEGORY];

                foreach (DataGridViewRow viewRow in GridForm.GridView.Rows) // loop on view rows
                {
                    DataRow dstRow = ((DataRowView)viewRow.DataBoundItem).Row;
                    string dstDescription = (string)dstRow[columnDstDescripion];
                    Dictionary<int, int> categorySimilaruties = new Dictionary<int, int>();
                    Dictionary<int, string> maxDescriptions = new Dictionary<int, string>();//TODO:REMOVE
                    foreach (DataRow srcRow in srcTable.Rows)
                    {
                        string srcDescription = srcRow[columnSrcDescripion] as string;
                        if (!string.IsNullOrWhiteSpace(srcDescription))
                        {
                            int category = (int)srcRow[columnSrcCategory];
                            int common = DataUtil.CommonPart(dstDescription, srcDescription);
                            if (!categorySimilaruties.ContainsKey(category) || common > categorySimilaruties[category])
                            {   // choose max
                                categorySimilaruties[category] = common;
                                maxDescriptions[category] = srcDescription;
                            }
                        }
                    }
                    if (categorySimilaruties.Count > 0)
                    {
                        int max = categorySimilaruties.Values.Max();
                        int chosenCategory = categorySimilaruties.First(pair => pair.Value == max).Key;
                        if (max > 500)
                        {
                            dstRow[CATEGORY] = chosenCategory;
                        }
                        dstRow["Debug"] = max.ToString() + " --- " + maxDescriptions[chosenCategory];
                    }
                    GridForm.GridView.UpdateCellValue(buttonColumn.Index, viewRow.Index);
                }
            }
        }

        private void GridView_CellContextMenuStripNeeded(object sender, DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                e.ContextMenuStrip = new ContextMenuStrip();

                ToolStripItem item = e.ContextMenuStrip.Items.Add("Manually Save Selection");
                item.Click += new EventHandler((object o, EventArgs dummy) => {

                    DataGridViewRow[] selectedRows = new DataGridViewRow[GridForm.GridView.SelectedRows.Count];
                    GridForm.GridView.SelectedRows.CopyTo(selectedRows, 0);

                    foreach (DataGridViewRow viewRow in selectedRows.OrderBy(row => row.Index))
                    {
                        DataRow row = GetGridDataRow(viewRow);
                        Statement.Transaction transaction = Convert(row);
                        if (Statement.ImportTransaction(transaction, accountID, true))
                        {
                            GridForm.GridView.Rows.Remove(viewRow);
                        }
                    }
                });

                if (GridForm.GridView.Columns[e.ColumnIndex].Name == CATEGORY)
                {
                    item = e.ContextMenuStrip.Items.Add("Clear");
                    item.Click += new EventHandler((object o, EventArgs dummy) => {
                        DataRow row = GetGridDataRow(e.RowIndex);
                        row[CATEGORY] = -1;
                        GridForm.GridView.UpdateCellValue(e.ColumnIndex, e.RowIndex);
                    });
                }
            }
        }

        private DataRow GetGridDataRow(int rowIndex)
        {
            return GetGridDataRow(GridForm.GridView.Rows[rowIndex]);
        }

        private DataRow GetGridDataRow(DataGridViewRow viewRow)
        {
            return ((DataRowView)viewRow.DataBoundItem).Row;
        }

        private void GridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && GridForm.GridView.Columns[e.ColumnIndex].Name == CATEGORY)
            {
                CategoryPicker picker = new CategoryPicker();
                DataRow row = GetGridDataRow(e.RowIndex);
                int? category = row[CATEGORY] as int?;
                if (category.HasValue) picker.CategoryID = category.Value;

                if (picker.ShowDialog() != DialogResult.Cancel)
                {
                    row[CATEGORY] = picker.CategoryID;
                }
            }
        }

        struct Authorization
        {
            public string merchant;
            public decimal amount;
            public string currency;
            public DateTime moment;
            public string accountId;

            static bool DateTimeEquals(DateTime a, DateTime b, TimeSpan tolerance) {
                return (a <= b + tolerance) && (a >= b - tolerance);
            }

            public void Test() {
                DateTime a = DateTime.FromFileTime(0);
                Trace.Assert(DateTimeEquals(a, a, TimeSpan.FromMinutes(0)));
                Trace.Assert(!DateTimeEquals(a, a.AddMinutes(1), TimeSpan.FromMinutes(0)));
                Trace.Assert(DateTimeEquals(a, a.AddMinutes(1), TimeSpan.FromMinutes(1)));
                Trace.Assert(DateTimeEquals(a.AddMinutes(-1), a, TimeSpan.FromMinutes(1)));
                Trace.Assert(!DateTimeEquals(a, a.AddMinutes(2), TimeSpan.FromMinutes(1)));
                Trace.Assert(!DateTimeEquals(a.AddMinutes(-2), a, TimeSpan.FromMinutes(1)));
            }

            public bool AuthEquals(Authorization other, TimeSpan tolerance) {
                bool CommonEquals(string a, string b) {
                    int len = Math.Min(a.Length, b.Length);
                    return string.Compare(a, 0, b, 0, len) == 0;
                }
                return CommonEquals(merchant, other.merchant) &&
                    amount == other.amount &&
                    currency == other.currency &&
                    DateTimeEquals(moment, other.moment, tolerance) &&
                    accountId == other.accountId;
            }
            public override string ToString() {
                return string.Join(";", merchant, amount, currency, moment, accountId);
            }
        }

        private void PasteAuthorizations_Click(object sender, EventArgs e)
        {
            const string regexText = "^\\*(?<account>\\d{4}) Оплата (?<merchant>.+?): " +
              "(?<amount>\\d+(\\.\\d{1,2})?)RUB " +
              "(?<moment>\\d{2}\\.\\d{2} \\d{2}:\\d{2}) Доступно (\\d+(\\.\\d{1,2})?)RUB$";
            Regex regex = new Regex(regexText, RegexOptions.Compiled);

            string text = Clipboard.GetText();
            if (text != null)
            {
                List<string> authLines = new List<string>(text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries));
                List<Tuple<Authorization, string>> authorizations = new List<Tuple<Authorization, string>>();
                List<string> noMatch = new List<string>();

                foreach(string line in authLines) {
                    Match match = regex.Match(line);
                    if (match.Success) {
                        Authorization auth = new Authorization
                        {
                            accountId = match.Groups["account"].Value,
                            amount = decimal.Parse(match.Groups["amount"].Value),
                            currency = "RUB",
                            moment = DateTime.ParseExact(match.Groups["moment"].Value, "dd.MM HH:mm", CultureInfo.InvariantCulture),
                            merchant = match.Groups["merchant"].Value
                        };
                        authorizations.Add(new Tuple<Authorization, string>(auth, line));
                    } else {
                        noMatch.Add(line);
                    }
                }

                DataTable gridTable = (DataTable)GridForm.GridView.DataSource;

                List<DataRow> transactions = new List<DataRow>();
                foreach (DataGridViewRow viewRow in GridForm.GridView.Rows) // loop on view rows
                {
                    transactions.Add(((DataRowView)viewRow.DataBoundItem).Row);
                }

                SearchBestMatching(transactions, authorizations);

                if (authorizations.Count > 0)
                {
                    var lines = authorizations.Select(item => item.Item2);
                    Clipboard.SetText(string.Join(Environment.NewLine, lines));
                } else
                {
                    Clipboard.Clear();
                }
            }
        }

        private void SearchBestMatching(List<DataRow> transactions, List<Tuple<Authorization, string>> authorizations) {
            if (transactions.Count == 0) return;

            DataColumn columnAmount = transactions[0].Table.Columns["Amount"];
            DataColumn columnDescription = transactions[0].Table.Columns["Description"];
            DataColumn columnAuth = transactions[0].Table.Columns["Authorization"];

            const string regexText =
                @"^(?<merchant>.+?)\,.*(?<moment>\d{2}\.\d{2}\.\d{4} \d{2}:\d{2}) карта \*(?<account>\d{4})$";
            Regex regex = new Regex(regexText, RegexOptions.Compiled);

            var transactionAuths = new List<Tuple<Authorization, DataRow>>();

            foreach (DataRow transaction in transactions) {
                decimal amount = (decimal)transaction[columnAmount];
                string description = (string)transaction[columnDescription];

                Match match = regex.Match(description);
                if (match.Success) {
                    Authorization auth = new Authorization
                    {
                        accountId = match.Groups["account"].Value,
                        amount = amount,
                        currency = "RUB",
                        moment = DateTime.ParseExact(match.Groups["moment"].Value, "dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
                        merchant = match.Groups["merchant"].Value
                    };

                    transactionAuths.Add(new Tuple<Authorization, DataRow>(auth, transaction));
                }
            }
            foreach (var tolerance in new TimeSpan[]{ new TimeSpan(), TimeSpan.FromMinutes(1) }) {
                for (int i = 0; i < transactionAuths.Count; ) {
                    var transactionAuth = transactionAuths[i];
                    var found = authorizations.Find(item => item.Item1.AuthEquals(transactionAuth.Item1, tolerance));
                    if (found != null) {
                        transactionAuth.Item2[columnAuth] = found.Item2;
                        authorizations.Remove(found);
                        transactionAuths.Remove(transactionAuth);
                    } else {
                        ++i;
                    }
                }
            }
        }

        private void SearchBestMatching(List<DataRow> transactions, List<string> authLines)
        {
            if (transactions.Count == 0) return;

            DataColumn columnAmount = transactions[0].Table.Columns["Amount"];
            DataColumn columnDescription = transactions[0].Table.Columns["Description"];
            DataColumn columnAuth = transactions[0].Table.Columns["Authorization"];

            List<DataRow> leftTransactions = new List<DataRow>();

            int counter = transactions.Count;
            while (transactions.Count > 0)
            {
                var transaction = transactions[0];
                Decimal amount = (Decimal)transaction[columnAmount];
                List<DataRow> sameAmountTransactions = transactions.FindAll(t => amount.Equals(t[columnAmount]));
                if (amount < 0)
                {
                    amount = -amount;
                }
                List<string> sameAmountAuths = authLines.FindAll(s => FindAmount(s, amount));

                if (sameAmountAuths.Count == 0)
                {
                    foreach (DataRow tran in sameAmountTransactions)
                    {
                        transactions.Remove(tran);
                        leftTransactions.Add(tran);
                    }
                }
                else if (sameAmountTransactions.Count == 1 && sameAmountAuths.Count == 1)
                {
                    string description = (string)sameAmountTransactions[0][columnDescription];
                    int similarity = DataUtil.CommonPart(description, sameAmountAuths[0]);
                    if (similarity >= 0) // always true, TODO: remove code around later
                    {
                        CompleteMatch(transactions, transaction, columnAuth, authLines, sameAmountAuths[0], "1-amount," + similarity.ToString());
                    }
                    else
                    {
                        transactions.Remove(transaction);
                        leftTransactions.Add(transaction);
                    }
                }
                else
                {
                    BestMatchAmong(transactions, authLines, columnDescription, columnAuth, sameAmountTransactions, sameAmountAuths, "amount,");
                    // delete all from transactions which are left in sameAmountTransactions (didn't pass similarity check)
                    transactions.RemoveAll(t =>
                    {
                        bool left = sameAmountTransactions.Contains(t);
                        if (left)
                        {
                            leftTransactions.Add(t);
                        }
                        return left;
                    } );
                }
            }
            List<string> leftAuthLines = new List<string>(authLines); // make a copy to not break fix array inside BestMatchAmong method
            BestMatchAmong(transactions, authLines, columnDescription, columnAuth, leftTransactions, leftAuthLines, "");
        }

        private void BestMatchAmong(List<DataRow> transactions, List<string> authLines, 
            DataColumn columnDescription, DataColumn columnAuth, 
            List<DataRow> sameAmountTransactions, List<string> sameAmountAuths, string debug/*REMOVE*/)
        {
            int[,] similarity = new int[sameAmountTransactions.Count, sameAmountAuths.Count];

            //Dictionary<Tuple<int, int>, int> matching = new Dictionary<Tuple<int, int>, int>();
            for (int i = 0; i < similarity.GetLength(0); i++)
            {
                for (int j = 0; j < similarity.GetLength(1); j++)
                {
                    string description = (string)sameAmountTransactions[i][columnDescription];
                    similarity[i, j] = DataUtil.CommonPart(description, sameAmountAuths[j]);
                }
            }

            for (int ti = 0; ti < Math.Min(sameAmountTransactions.Count, sameAmountAuths.Count); ti++)
            {
                int maxSimilarity = 0;
                int iMax = 0;
                int jMax = 0;
                for (int i = 0; i < similarity.GetLength(0); i++)
                {
                    for (int j = 0; j < similarity.GetLength(1); j++)
                    {
                        if (similarity[i, j] > maxSimilarity)
                        {
                            maxSimilarity = similarity[i, j];
                            iMax = i;
                            jMax = j;
                        }
                    }
                }

                if (maxSimilarity < 1) break;

                CompleteMatch(transactions, sameAmountTransactions[iMax], columnAuth, authLines, sameAmountAuths[jMax], debug + maxSimilarity.ToString());

                for (int i = 0; i < similarity.GetLength(0); i++) similarity[i, jMax] = -1;
                for (int j = 0; j < similarity.GetLength(1); j++) similarity[iMax, j] = -1;
            }
        }

        static BankPurchaseSpreadSheet()
        {
            Debug.Assert(!FindAmount("400", 400.00m));
            Debug.Assert(!FindAmount("400.00", 400m));
            Debug.Assert(FindAmount("400RUB", 400.00m));
            Debug.Assert(FindAmount("400.00RUB", 400m));
            Debug.Assert(FindAmount("4.5RUB", 4.5m));
            Debug.Assert(FindAmount("4.50RUB", 4.5m));
            Debug.Assert(FindAmount("[4.5RUB]", 4.5m));
            Debug.Assert(FindAmount("[4.50RUB]", 4.5m));
        }

        private static bool FindWholeNumber(string text, string number)
        {
            bool edgeChar(char ch) => !char.IsLetterOrDigit(ch) && (ch != '.');
            bool leftEdge(int i) => (i < 0) || edgeChar(text[i]);
            bool rightEdge(int i) => (i >= text.Length) || edgeChar(text[i]);

            foreach (string currency in new string[]{ "RUB", " EUR" })
            {
                string lookFor = number + currency;
                int startIndex = 0;
                while (startIndex < text.Length)
                {
                    int pos = text.IndexOf(lookFor, startIndex);
                    if (pos >= 0)
                    {
                        if (leftEdge(pos - 1) && rightEdge(pos + lookFor.Length))
                        {
                            return true;
                        }
                        else
                        {
                            startIndex = pos + 1;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return false;
        }

        private static bool FindAmount(string text, decimal amount)
        {
            string s = amount.ToString("0.##"); 

            bool res = FindWholeNumber(text, s);
            if (!res)
            {
                string s2 = amount.ToString("0.00");
                if (s2 != s)
                {
                    res = FindWholeNumber(text, s2);
                }
            }

            return res;
        }

        private void CompleteMatch(List<DataRow> transactions, DataRow transaction, DataColumn column, List<string> authLines, string authLine, 
            string debug/*REMOVE*/)
        {
            transaction["Debug"] = debug; //REMOVE
            transaction[column] = authLine;
            transactions.Remove(transaction);
            authLines.Remove(authLine);
        }

        private void GridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (GridForm.GridView.Columns[e.ColumnIndex].Name.Equals(CATEGORY) && e.RowIndex >= 0)
            {
                DataRow row = GetGridDataRow(e.RowIndex);
                int category = (int)row[CATEGORY];
                if (category >= 0)
                {
                    Category service = new Category();
                    e.Value = service.GetFullName(category);
                } else
                {
                    e.Value = "";
                }
                e.FormattingApplied = true;
            }

            bool skip = (Boolean)((DataRowView)((DataGridView)sender).Rows[e.RowIndex].DataBoundItem)["Exists"];
            if (e.CellStyle.Font.Strikeout != skip)
            {
                Font font = new Font(e.CellStyle.Font, FontStyle.Strikeout);
                e.CellStyle.Font = font;
            }
        }

        private Statement.Transaction Convert(DataRow row)
        {
            Statement.Transaction res = new Statement.Transaction
            {
                date = (DateTime)row[DATE],
                category = (int)row[CATEGORY],
                amount = -(decimal)row[AMOUNT],
                description = (string)row[DESCRIPTION]
            };
            return res;
        }

        private DataTable MakeTable(List<Statement.Transaction> transactions)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Exists", typeof(bool));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("Category", typeof(int));
            table.Columns.Add("Amount", typeof(Decimal));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Authorization", typeof(string));

            table.Columns.Add("Debug", typeof(string)); //REMOVE for debug purpose

            DataTable uncheckedTransactions = Statement.GetUncheckedTransactions(accountID);

            foreach (var transaction in transactions)
            {
                object[] row = new object[table.Columns.Count];
                int i = 0;

                DataRow[] uncheckedRows = uncheckedTransactions.Select(string.Format("Amount = {0}", -transaction.amount), "When");
                // TODO: match descriptions
                row[i++] = uncheckedRows.Length > 0;
                if (uncheckedRows.Length > 0)
                {
                    uncheckedTransactions.Rows.Remove(uncheckedRows.First());
                }
                row[i++] = transaction.date;
                row[i++] = -1;
                row[i++] = -transaction.amount;
                row[i++] = transaction.description;

                table.Rows.Add(row);
            }

            if (uncheckedTransactions.Rows.Count > 0)
            {
                Debug.Write(string.Format("Left uncheckedTransactions Count: {0}", uncheckedTransactions.Rows.Count));
            }

            return table;
        }

        private void Save(object sender, EventArgs e)
        {
            //Statement.importStatement(transactions, accountID);
        }
    }
}
