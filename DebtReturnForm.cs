﻿using System;
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

        public DateTime When { get { return FormUtil.GetDate(monthCalendar1); } }

        public DebtReturnForm()
        {
            InitializeComponent();
        }
    }
}
