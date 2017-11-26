using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Home_Accounting
{
    public partial class ShopForm : Form
    {
        public ShopForm()
        {
            InitializeComponent();
        }

        private void ShopForm_DragDrop(object sender, DragEventArgs e)
        {
            AccountForm accountForm = (AccountForm)e.Data.GetData(typeof(AccountForm));
            if (accountForm != null)
            {
                PurchaseForm purchaseForm = new PurchaseForm(accountForm);
                purchaseForm.ShowDialog(this);
            }
        }

        private void ShopForm_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
    }
}