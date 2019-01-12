using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Home_Accounting
{
    public partial class DebtReturnForm : Form
    {
        public string Message { set { label1.Text = value; } }

        public DateTime When {
            get {
                return monthCalendar1.SelectionStart != monthCalendar1.TodayDate ?
                    monthCalendar1.SelectionStart : DataUtil.Now;
            }
        }

        public DebtReturnForm()
        {
            InitializeComponent();
        }
    }
}
