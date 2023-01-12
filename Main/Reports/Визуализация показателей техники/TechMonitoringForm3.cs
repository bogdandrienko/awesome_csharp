using System;
using System.Data;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

using XrtlExplorer;
using OracleClient = Oracle.ManagedDataAccess.Client;
using Excel = Microsoft.Office.Interop.Excel;
using WpfToolkit = Xceed.Wpf.Toolkit;
using _WinForms = System.Windows.Forms;
using _WinControls = System.Windows.Controls;
using _Forms = Main.Forms;
using _Reports = Main.Reports;
using _Utils = Main.Utils;
using _Queries = Main.Queries;

namespace Main.Forms
{
    public partial class TechMonitoringForm3 : Form
    {
        public TechMonitoringForm3()
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
FROM   SHOVEVENTSTATEARCHIVE t1
       --inner join SHOVELS t2 ON t1.SHOVID = t2.SHOVID
       --inner join SHOVELMODELS t4 ON MODEL = t4.NAME
       join (SELECT SHOVID,
                          MAX(TIME) AS MaxTime
                   FROM   (SELECT *
                           FROM   (SELECT *
                                   FROM   SHOVEVENTSTATEARCHIVE
                                   WHERE  SHOVEVENTSTATEARCHIVE.SHOVID = :SHOVID
                                   ORDER  BY EVENTCOUNTER DESC)
                           WHERE  ROWNUM < 1000)
                   GROUP  BY SHOVID) t2
               ON t1.SHOVID = t2.SHOVID
                  AND t1.TIME = t2.MAXTIME
WHERE  ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', 1, SYSDATE) AND GETPREDEFINEDTIMETO('за указанную смену', 2, SYSDATE) )
ORDER  BY t1.TIME DESC 


select * from (select * from SHOVELS t1
join SHOVEVENTSTATEARCHIVE t2 ON t1.SHOVID = t2.SHOVID
join SHOVELMODELS t3 ON t1.MODEL = t3.NAME
where t1.SHOVID = '207'
order by TIME desc) where ROWNUM = 1;


";
                #endregion sqlExpression

                // Sql query instanse
                //DataTable dataTable = await new Utils.Sql(
                //        sqlExpression: sqlExpressionShov,
                //        queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                //            new Tuple<string, OracleClient.OracleDbType, object>("SHOVID", OracleClient.OracleDbType.Int32, 207),
                //        }
                //    ).ExecuteSelectAsync();
                //dataGridViewShov.DataSource = dataTable;

                //radioButtonMototHours.Text = $"моточасы: {dataTable.Rows[0]["MOTOHOURS"]} (с начала смены 12)";
                //radioButtonSpeed.Text = $"скорость: {dataTable.Rows[0]["SPEED"]} км/ч";
                ////radioButtonOdometer.Text = $"одометер: {dataTable.Rows[0]["ODOMETER"]} км/ч";
                //radioButtonFuel.Text = $"объём бака: {dataTable.Rows[0]["FUEL"]} л (из ):  в %"; //{dataTable.Rows[0]["FUEL_TANK_VOLUME"]}
                //radioButtonCoordinats.Text = $"x: {dataTable.Rows[0]["X"]} y: {dataTable.Rows[0]["Y"]} z: {dataTable.Rows[0]["Z"]}";
                //radioButtonSignalTech.Text = $"время устройства: {dataTable.Rows[0]["TIME"]}";
                //radioButtonSignalServer.Text = $"время сервера: {dataTable.Rows[0]["SYSTEMTIME"]}";

                //GodClass.UtilsClass.PrintConsoleMessageMethod(message: $" {dataTable.Rows[0]["MOTOHOURS"]} ", isNewLine: true);

                //foreach (DataRow row in dataTable.Rows.)
                //{
                //    //

                //}


                #endregion SHOV
                /// <summary>
                /// SHOV
                /// </summary>

                await Task.Delay(3000);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string paramSelectTechId = "205";

            DateTime currentDateTime = _Utils.DateTime_.Get_Now_DateTime();
            DateTime beginDateTime = currentDateTime;
            DateTime endDateTime = currentDateTime;
            int currentShift = _Utils.DateTime_.Get_NowShift_Int(dateTime: currentDateTime);
            if (currentShift == 1)
            {
                if (currentDateTime.Hour < 19)
                {
                    DateTime previousDayDateTime = _Utils.DateTime_.Get_PlusDayCount_DateTime(dayCount: -1, dateTime: currentDateTime);
                    beginDateTime = new DateTime(year: previousDayDateTime.Year, month: previousDayDateTime.Month, day: previousDayDateTime.Day, hour: 20, minute: 0, second: 0);
                    endDateTime = new DateTime(year: currentDateTime.Year, month: currentDateTime.Month, day: currentDateTime.Day, hour: 8, minute: 0, second: 0);
                } else
                {
                    beginDateTime = new DateTime(year: currentDateTime.Year, month: currentDateTime.Month, day: currentDateTime.Day, hour: 20, minute: 0, second: 0);
                    endDateTime = new DateTime(year: currentDateTime.Year, month: currentDateTime.Month, day: currentDateTime.Day, hour: 8, minute: 0, second: 0);
                }
            } else
            {
                beginDateTime = new DateTime(year: currentDateTime.Year, month: currentDateTime.Month, day: currentDateTime.Day, hour: 8, minute: 0, second: 0);
                endDateTime = new DateTime(year: currentDateTime.Year, month: currentDateTime.Month, day: currentDateTime.Day, hour: 20, minute: 0, second: 0);
            }
            double passed = (double)Math.Round((double)(endDateTime - currentDateTime).TotalSeconds / 60 / 60 * 100 / 12, 1, MidpointRounding.ToEven);
            double still = 1.0D;
            label5.Text = $"смена: {currentShift}, текущее время: {currentDateTime}, \nвремя начала: {beginDateTime}, время окончания: {endDateTime}\nосталось: {passed} %";
            progressBar1.Value = (int)(100 - passed);
            // arhive
            DataTable dataTableArhive = await Task.Run(() => _Utils.Report.TechVisualization.LastShovelArhiveRecord(
                paramSelectTechId: paramSelectTechId
            ));
            dataGridView_Arhive.DataSource = dataTableArhive;
            // arhive

            // state
            DataTable dataTableState = await Task.Run(() => _Utils.Report.TechVisualization.LastShovelState(
                paramSelectTechId: paramSelectTechId
            ));
            dataGridView_State.DataSource = dataTableState;
            Dictionary<string, object> state = new Dictionary<string, object>() { };
            state.Add(key: "FUEL", value: dataTableState.Rows[0]["FUEL"]);
            radioButtonFuel.Text = $"Топливо: {state["FUEL"]}";
            // state

            // sumtrips
            DataTable dataTableSumTrips = await Task.Run(() => _Utils.Report.TechVisualization.Get_Bogdan_custom_Vehtrips_Analyze_By_Shovel(
                paramSelectTechId: paramSelectTechId
            ));
            dataGridView_SumTrips.DataSource = dataTableSumTrips;
            // sumtrips


            // trips
            DataTable dataTableTrips = await Task.Run(() => _Utils.Report.TechVisualization.AnalyseTruck(
                paramSelectTechId: paramSelectTechId
            ));
            dataGridView_Trips.DataSource = dataTableTrips;
            // trips

            //DataTable result1 = await Task.Run(() => _Utils.Report.TechVisualization.AnalyseTruck(
            //    paramSelectTechId: paramSelectTechId
            //));
            //dataGridView_Trips.DataSource = result1;
            //List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>() { };
            //foreach (DataRow dataRow in result1.Rows)
            //{
            //    Dictionary<string, object> row = new Dictionary<string, object>() { };
            //    foreach (DataColumn dataColumn in result1.Columns)
            //    {
            //        row[$"{dataColumn}"] = dataRow[dataColumn];
            //    }
            //    rows.Add(row);
            //}
            //float Weight = 0.0F;
            //Console.WriteLine(rows.Count);
            //foreach (Dictionary<string, object> row in rows)
            //{
            //    foreach (KeyValuePair<string, object> item in row)
            //    {
            //        if (item.Key == "WEIGHT")
            //        {
            //            Console.WriteLine($"WEIGHT: {item.Value}");
            //            Weight += (float)item.Value;
            //        }
            //        Console.WriteLine($"key: {item.Key}, value: {item.Value}");
            //    }
            //    Console.WriteLine($"");
            //}
            //Console.WriteLine($"Weight: {Weight}");

        }
    }
}
