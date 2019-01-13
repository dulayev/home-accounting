using System;
using System.Windows.Forms;

namespace Home_Accounting
{
    internal class FormUtil
    {
        internal static DateTime GetDate(MonthCalendar monthCalendar1)
        {
            return monthCalendar1.SelectionStart != monthCalendar1.TodayDate ?
                        monthCalendar1.SelectionStart : DataUtil.Now;
        }
    }
}