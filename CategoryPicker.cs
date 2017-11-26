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
    public partial class CategoryPicker : Form
    {
        public int CategoryID;
        public string CategoryName;
        static private DataTable tableCategories = null;
        //static private Dictionary<int, string> cacheIDToName = new Dictionary<int, string>();

        public string IDToName(int id)
        {
            TreeNode node = SelectWithTag(id, treeView1.Nodes);
            return node == null ? "Unknown" : node.FullPath;

            //LoadTableCategories();

            //if (cacheIDToName.ContainsKey(id))
            //    return cacheIDToName[id];
            //else
            //{
            //    int requiredID = id;
            //    string name = "";
            //    while(id != 0)
            //    {
            //        DataRow[] rows = tableCategories.Select(string.Format("ID = {0}", id));
            //        if (rows.Length != 1)
            //            break;
            //        if(name != "")
            //            name = "/" + name;
            //        name = (string)rows[0]["Name"] + name;
            //        id = (int)rows[0]["ParentID"];
            //    }
            //    cacheIDToName.Add(requiredID, name);
            //    return name;
            //}
        }

        public CategoryPicker()
        {
            InitializeComponent();
            LoadTableCategories();
            LoadCategories(0, treeView1.Nodes);
        }

        private void CategoryPicker_Load(object sender, EventArgs e)
        {
            //LoadTableCategories();

            //LoadCategories(0, treeView1.Nodes);
            treeView1.SelectedNode = SelectWithTag(CategoryID, treeView1.Nodes);

            treeView1.NodeMouseClick += new TreeNodeMouseClickEventHandler(treeView1_NodeMouseClick);
        }

        private TreeNode SelectWithTag(int tag, TreeNodeCollection treeNodeCollection)
        {
            foreach (TreeNode node in treeNodeCollection)
            {
                if ((int)node.Tag == tag)
                    return node;
                TreeNode childFound = SelectWithTag(tag, node.Nodes);
                if (childFound != null)
                    return childFound;
            }
            return null;
        }

        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null)
            {
                if (e.Node.Bounds.Contains(e.Location))
                {
                    CategoryID = (int)e.Node.Tag;
                    CategoryName = e.Node.FullPath;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private static void LoadTableCategories()
        {
            if (tableCategories == null)
            {
                OleDbCommand cmd = new OleDbCommand("Select ID, ParentID, Name From PurchaseCategory", DataUtil.Connection);
                OleDbDataAdapter adapter = new OleDbDataAdapter(cmd);
                tableCategories = new DataTable();
                adapter.Fill(tableCategories);
            }
        }

        private void LoadCategories(int parentID, TreeNodeCollection nodesToInsert)
        {
            string filter = string.Format("ParentID = {0}", parentID);
            foreach (DataRow dr in tableCategories.Select(filter))
            {
                TreeNode node = nodesToInsert.Add((string)dr["Name"]);
                node.Tag = dr["ID"];
                LoadCategories((int)dr["ID"], node.Nodes);
            }
        }

        //private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        //{
        //    if(e.Node != null && e.Action != TreeViewAction.Unknown)
        //    {
        //        CategoryID = (int)e.Node.Tag;
        //        CategoryName = e.Node.Text;
        //        TreeNode node = e.Node;
        //        while (node.Parent != null)
        //        {
        //            node = node.Parent;
        //            CategoryName = node.Text + "/" + CategoryName;
        //        }
        //        DialogResult = DialogResult.OK;
        //        Close();
        //    }
        //}
    }
}