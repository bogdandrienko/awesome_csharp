using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using OracleClient = Oracle.ManagedDataAccess.Client;
using Excel = Microsoft.Office.Interop.Excel;
using Utils = Main.Utils;


namespace Main.Reports
{
    #region wrapper

    public partial class Bogdan_custom_Container_FindTech : Utils.WinForms.WinForms_Container
    {
        #region constructor

        public Bogdan_custom_Container_FindTech()
        {
            // set report to wrap
            this.Report = new Bogdan_custom_FindTech(Xrtl_Container: this);
        }

        #endregion constructor
    }

    #endregion wrapper

    public partial class Bogdan_custom_FindTech : System.Windows.Controls.UserControl
    {
        #region variables

        public Utils.WinForms.WinForms_Container Xrtl_Container;

        #endregion variables



        #region constructor

        public Bogdan_custom_FindTech(Utils.WinForms.WinForms_Container Xrtl_Container)
        {
            InitializeComponent();



            #region IXrtlControl Interface Realization

            this.Xrtl_Container = Xrtl_Container;

            #endregion IXrtlControl Interface Realization



            #region Initialize User Interface settings

            DateTime today = DateTime.Now;
            DateTime yesterday = today.AddDays(-1);

            // find
            this.DatePicker_DateFrom_Find.SelectedDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day);
            this.DatePicker_DateTo_Find.SelectedDate = new DateTime(today.Year, today.Month, today.Day);

            #endregion Initialize User Interface settings
        }

        #endregion constructor



        #region Analyse Truck

        private async void Button_StartAnalyze_Truck_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DateTime start = DateTime.Now;
                Button_StartAnalyze_Truck.Content = $"ОБНОВИТЬ \n-загрузка-";



                #region GET INPUT FROM USER INTERFACE

                DateTime paramDateFrom = this.DatePicker_DateFrom_Find.SelectedDate.Value;
                int paramShiftFrom = int.Parse(this.ComboBox_ShiftTo_Truck.Text);
                DateTime paramDateTo = this.DatePicker_DateTo_Find.SelectedDate.Value;
                int paramShiftTo = int.Parse(this.ComboBox_ShiftTo_Truck.Text);

                int paramMinX = (int)this.IntegerUpDown_MinX.Value;
                int paramMaxX = (int)this.IntegerUpDown_MaxX.Value;
                int paramMinY = (int)this.IntegerUpDown_MinY.Value;
                int paramMaxY = (int)this.IntegerUpDown_MaxY.Value;

                #endregion GET INPUT FROM USER INTERFACE



                #region ASYNC SELECT DATA

                DataTable dataTable = await Task.Run(() => AnalyseTruck(
                        paramDateFrom: paramDateFrom,
                        paramShiftFrom: paramShiftFrom,
                        paramDateTo: paramDateTo,
                        paramShiftTo: paramShiftTo,
                        paramMinX: paramMinX,
                        paramMaxX: paramMaxX,
                        paramMinY: paramMinY,
                        paramMaxY: paramMaxY
                    ));

                #endregion ASYNC SELECT DATA



                #region UPDATE DATASOURCE

                DataGrid_Truck.BeginInit();
                DataGrid_Truck.SetBinding(System.Windows.Controls.ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding {
                    Source = dataTable
                });
                DataGrid_Truck.Items.Refresh();
                DataGrid_Truck.EndInit();

                #endregion UPDATE DATASOURCE



                Button_StartAnalyze_Truck.Content = $"ОБНОВИТЬ \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} ({Math.Truncate((DateTime.Now - start).TotalSeconds)} сек)";
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                Button_StartAnalyze_Truck.Content = $"ОБНОВИТЬ \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} (ошибка)";
            }
        }

        private DataTable AnalyseTruck(
                DateTime paramDateFrom, int paramShiftFrom, DateTime paramDateTo, int paramShiftTo,
                int paramMinX, int paramMaxX, int paramMinY, int paramMaxY
            )
        {
            #region ASYNC SELECT DATA FROM DATABASE

            //Sql expression
            #region sqlExpression

            string sqlExpression = @"



SELECT TYPE,
       TEHID,
       TIME,
       X,
       Y,
       DESCRIPTION
FROM   (SELECT VEHID                      AS TEHID,
               'Автосамосвал' AS TYPE,
               TIME,
               X,
               Y,
               EVENTDESCR                 AS DESCRIPTION
        FROM   EVENTSTATEARCHIVE t1
        WHERE  ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', :paramShiftFrom, :paramDateFrom) AND GETPREDEFINEDTIMETO('за указанную смену', :paramShiftTo, :paramDateTo) )
               AND ( t1.X > :paramMinX
                     AND t1.X < :paramMaxX
                     AND t1.Y > :paramMinY
                     AND t1.Y < :paramMaxY )
        UNION ALL
        SELECT AUXID                AS TEHID,
               'Бульдозер' AS TYPE,
               TIME,
               X,
               Y,
               '-'                  AS EVENTDESCR
        FROM   AUXEVENTARCHIVE t2
        WHERE  ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', :paramShiftFrom, :paramDateFrom) AND GETPREDEFINEDTIMETO('за указанную смену', :paramShiftTo, :paramDateTo) )
               AND ( t2.X > :paramMinX
                     AND t2.X < :paramMaxX
                     AND t2.Y > :paramMinY
                     AND t2.Y < :paramMaxY )
        UNION ALL
        SELECT SHOVID                 AS TEHID,
               'Экскаватор' AS TYPE,
               TIME,
               X,
               Y,
               '-'                    AS EVENTDESCR
        FROM   SHOVEVENTSTATEARCHIVE t3
        WHERE  ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', :paramShiftFrom, :paramDateFrom) AND GETPREDEFINEDTIMETO('за указанную смену', :paramShiftTo, :paramDateTo) )
               AND ( t3.X > :paramMinX
                     AND t3.X < :paramMaxX
                     AND t3.Y > :paramMinY
                     AND t3.Y < :paramMaxY ))
ORDER  BY TIME,
          TEHID 



";

            #endregion sqlExpression

            // Sql query instanse
            DataTable dataTableFromDatabase = new Utils.Sql(
                    sqlExpression: sqlExpression,
                    queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateFrom", OracleClient.OracleDbType.Date, paramDateFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftFrom", OracleClient.OracleDbType.Int32, paramShiftFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateTo", OracleClient.OracleDbType.Date, paramDateTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftTo", OracleClient.OracleDbType.Int32, paramShiftTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramMinX", OracleClient.OracleDbType.Int32, paramMinX),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramMaxX", OracleClient.OracleDbType.Int32, paramMaxX),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramMinY", OracleClient.OracleDbType.Int32, paramMinY),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramMaxY", OracleClient.OracleDbType.Int32, paramMaxY)
                    }
                ).ExecuteSelect();

            #endregion ASYNC SELECT DATA FROM DATABASE



            #region ANALYSE AND CONVERT DATATABLE

            // Convert DataTable To List
            List<List<object>> matrix = Utils.DataTable_.Get_ConvertDataTable_List(
                    dataTable: dataTableFromDatabase,
                    columns: new List<string>() {
                                "TYPE", "TEHID", "TIME", "X", "Y", "DESCRIPTION"
                        },
                    matrix: new List<List<object>>() { }
                );

            // Convert List to DataTable
            DataTable dataTableNew = Utils.DataTable_.Get_ConvertList_DataTable(
                    columns: new List<string>() {
                                    "Тип", "Хоз. номер",  "Дата и время",
                                    "Координата X", "Координата Y", "Описание"
                            },
                    matrix: matrix
                );

            #endregion ANALYSE AND CONVERT DATATABLE



            #region FILL EXCEL

            Utils.Excel_ excelClass = new Utils.Excel_();
            excelClass.Method_Hide_Application(isVisible: false);
            excelClass.Method_Load_Template(
                    templateName: "template1",
                    templateNameDebug: "Отчёт поиск техники в координатах.xlsx",
                    Xrtl_Container: Xrtl_Container
                );
            excelClass.Method_Fill_Sheet_From_List(
                    matrix: Utils.Extra.Get_Sliced_List(matrix: matrix, startIndex: 1, stopIndex: -1),
                    startRow: 6,
                    startCol: 1
                );
            excelClass.Method_Set_Cell_Value(
                    rowIndex: 3,
                    colIndex: 1,
                    value: $"сформировано: {Utils.DateTime_.Get_FormatOnlyDateTime_String(Utils.DateTime_.Get_Now_DateTime())}"
                );
            excelClass.Method_Hide_Application(isVisible: true);

            #endregion FILL EXCEL



            return dataTableNew;
        }

        #endregion Analyse Truck
    }
}
