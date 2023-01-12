using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Main.Forms
{
    public partial class TechMonitoringForm1 : Form
    {
        public TechMonitoringForm1()
        {
            InitializeComponent();
        }

        private void Button7_Click_1(object sender, EventArgs e)
        {
            if (button7.Text == "X")
            {
                panel2.Hide();
                button7.Text = "O";
            } else
            {
                panel2.Show();
                button7.Text = "X";
            }
        }

        private void Button9_Click_1(object sender, EventArgs e)
        {
            if (button9.Text == "X")
            {
                panel4.Hide();
                button9.Text = "O";
            } else
            {
                panel4.Show();
                button9.Text = "X";
            }
        }

        private void Button11_Click_1(object sender, EventArgs e)
        {
            if (button11.Text == "X")
            {
                panel3.Hide();
                button11.Text = "O";
            } else
            {
                panel3.Show();
                button11.Text = "X";
            }
        }

        private async void TechMonitoringForm1_Load(object sender, EventArgs e)
        {
            while (true)
            {
                /// <summary>
                /// SHOV
                /// </summary>
                #region SHOV

                //Sql expression
                #region sqlExpression
                string sqlExpressionShov = @"



SELECT *
FROM   dispatcher.SHOVEVENTSTATEARCHIVE t1
       join (SELECT SHOVID,
                    MAX(TIME) AS MaxTime
             FROM   (SELECT *
                     FROM   (SELECT *
                             FROM   dispatcher.SHOVEVENTSTATEARCHIVE
                             ORDER  BY dispatcher.SHOVEVENTSTATEARCHIVE.EVENTCOUNTER DESC)
                     WHERE  ROWNUM < 1000)
             GROUP  BY SHOVID) t3
         ON t1.SHOVID = t3.SHOVID
            AND t1.TIME = t3.MAXTIME
WHERE  ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', 1, SYSDATE) AND GETPREDEFINEDTIMETO('за указанную смену', 2, SYSDATE) )
ORDER  BY t1.TIME DESC 




";
                #endregion sqlExpression

                // Sql query instanse
                dataGridViewShov.DataSource = await new Utils.Sql(sqlExpression: sqlExpressionShov).ExecuteSelectAsync();
                #endregion SHOV
                /// <summary>
                /// SHOV
                /// </summary>

                /// <summary>
                /// TRUCK
                /// </summary>
                #region TRUCK

                //Sql expression
                #region sqlExpression
                string sqlExpressionTruck = @"



SELECT *
FROM   DISPATCHER.EVENTSTATEARCHIVE t1
       join (SELECT VEHID,
                    MAX(TIME) AS MaxTime
             FROM   (SELECT *
                     FROM   (SELECT *
                             FROM   dispatcher.EVENTSTATEARCHIVE
                             ORDER  BY dispatcher.EVENTSTATEARCHIVE.MESCOUNTER DESC)
                     WHERE  ROWNUM < 100)
             GROUP  BY VEHID) t3
         ON t1.VEHID = t3.VEHID
            AND t1.TIME = t3.MAXTIME
WHERE  ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', 1, SYSDATE) AND GETPREDEFINEDTIMETO('за указанную смену', 2, SYSDATE) )
ORDER  BY t1.TIME DESC 



";
                #endregion sqlExpression

                // Sql query instanse
                dataGridViewTruck.DataSource = await new Utils.Sql(sqlExpression: sqlExpressionTruck).ExecuteSelectAsync();
                #endregion TRUCK
                /// <summary>
                /// TRUCK
                /// </summary>

                /// <summary>
                /// AUX
                /// </summary>
                #region AUX

                //Sql expression
                #region sqlExpression
                string sqlExpressionAux = @"



SELECT *
FROM   dispatcher.AUXEVENTARCHIVE t1
       left join dispatcher.AUXTECHNICS t2
              ON t1.AUXID = t2.AUXID
       join (SELECT AUXID,
                    MAX(TIME) AS MaxTime
             FROM   (SELECT *
                     FROM   (SELECT *
                             FROM   dispatcher.AUXEVENTARCHIVE
                             ORDER  BY dispatcher.AUXEVENTARCHIVE.EVENTCOUNTER DESC)
                     WHERE  ROWNUM < 1000)
             GROUP  BY AUXID) t3
         ON t1.AUXID = t3.AUXID
            AND t1.TIME = t3.MAXTIME
WHERE  ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', 1, SYSDATE) AND GETPREDEFINEDTIMETO('за указанную смену', 2, SYSDATE) )
ORDER  BY t1.TIME DESC 




";
                #endregion sqlExpression

                // Sql query instanse
                dataGridViewAux.DataSource = await new Utils.Sql(sqlExpression: sqlExpressionAux).ExecuteSelectAsync();
                #endregion AUX
                /// <summary>
                /// AUX
                /// </summary>

                await Task.Delay(100);
            }
        }
    }
}
