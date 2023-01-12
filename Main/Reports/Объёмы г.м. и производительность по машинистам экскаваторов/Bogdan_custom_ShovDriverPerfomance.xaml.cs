using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using OracleClient = Oracle.ManagedDataAccess.Client;
using Excel = Microsoft.Office.Interop.Excel;

namespace Main.Reports
{
    #region wrapper

    public partial class Bogdan_custom_Container_ShovDriverPerfomance : Utils.WinForms.WinForms_Container
    {
        public Bogdan_custom_Container_ShovDriverPerfomance()
        {
            this.Report = new Bogdan_custom_ShovDriverPerfomance(Xrtl_Container: this);
        }
    }

    #endregion wrapper



    public partial class Bogdan_custom_ShovDriverPerfomance : System.Windows.Controls.UserControl
    {
        #region variables

        public Utils.WinForms.WinForms_Container Xrtl_Container;

        #endregion variables



        #region constructor

        public Bogdan_custom_ShovDriverPerfomance(Utils.WinForms.WinForms_Container Xrtl_Container)
        {
            InitializeComponent();



            #region Xrtl_Container Initialization

            this.Xrtl_Container = Xrtl_Container;

            #endregion Xrtl_Container Initialization



            #region Initialize User Interface settings

            DateTime previousMonth = Utils.DateTime_.Get_PlusMonthCount_DateTime(monthCount: -1, dateTime: Utils.DateTime_.Get_Now_DateTime());

            // shov
            Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: DatePicker_DateFrom_Shov,
                    dateTime: new DateTime(previousMonth.Year, previousMonth.Month, 1)
                );
            Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: DatePicker_DateTo_Shov,
                    dateTime: new DateTime(
                        previousMonth.Year,
                        previousMonth.Month,
                        Utils.DateTime_.Get_LastDayInSelectMonth_Int(dateTime: previousMonth)
                    )
                );
            Utils.Wpf.Set_Combobox_List(comboBox: ComboBox_SelectTechId_Shov, list: Utils.Report.Get_Shovel_List());

            #endregion Initialize User Interface settings
        }

        #endregion constructor



        #region Analyse Shov

        private async void Button_StartAnalyze_Shov_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DateTime start = Utils.DateTime_.Get_Now_DateTime();
                Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Shov,
                        text: $"ОБНОВИТЬ \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} " +
                        $"({Math.Truncate((Utils.DateTime_.Get_Now_DateTime() - start).TotalSeconds)} сек)"
                    );



                #region GET INPUT FROM USER INTERFACE

                DateTime paramDateFrom = Utils.Wpf.Get_DataPicker_DateTime(datePicker: DatePicker_DateFrom_Shov);
                int paramShiftFrom = Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_ShiftFrom_Shov);
                DateTime paramDateTo = Utils.Wpf.Get_DataPicker_DateTime(datePicker: DatePicker_DateTo_Shov);
                int paramShiftTo = Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_ShiftTo_Shov);
                string paramSelectTechId = Utils.Wpf.Get_Combobox_String(comboBox: ComboBox_SelectTechId_Shov);
                int roundPoint = Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_RoundedPoint_Shov);

                #endregion GET INPUT FROM USER INTERFACE



                #region ASYNC SELECT DATA

                DataTable dataTable = await Task.Run(() => AnalyseShov(
                        paramDateFrom: paramDateFrom,
                        paramShiftFrom: paramShiftFrom,
                        paramDateTo: paramDateTo,
                        paramShiftTo: paramShiftTo,
                        paramSelectTechId: paramSelectTechId,
                        roundPoint: roundPoint
                    ));

                #endregion ASYNC SELECT DATA



                #region UPDATE DATASOURCE

                Utils.Wpf.Set_DataGridUpdate_DataTable(dataGrid: DataGrid_Shov, dataTable: dataTable);

                #endregion UPDATE DATASOURCE



                Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Shov,
                        text: $"ОБНОВИТЬ \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} " +
                        $"({Math.Truncate((Utils.DateTime_.Get_Now_DateTime() - start).TotalSeconds)} сек)"
                    );
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Shov,
                        text: $"ОБНОВИТЬ \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} (ошибка)"
                    );
            }
        }

        private DataTable AnalyseShov(DateTime paramDateFrom, int paramShiftFrom, DateTime paramDateTo, int paramShiftTo, string paramSelectTechId, int roundPoint)
        {

            #region ASYNC SELECT DATA FROM DATABASE

            DataTable dataTable = new Utils.Sql(
                    sqlExpression: Queries.Get_Bogdan_custom_ShovDriverPerfomance_Shov(),
                    queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateFrom", OracleClient.OracleDbType.Date, paramDateFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftFrom", OracleClient.OracleDbType.Int32, paramShiftFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateTo", OracleClient.OracleDbType.Date, paramDateTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftTo", OracleClient.OracleDbType.Int32, paramShiftTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramRoundPoint", OracleClient.OracleDbType.Int32, roundPoint),
                    }
                ).ExecuteSelect();

            #endregion ASYNC SELECT DATA FROM DATABASE



            #region ANALYZE DATA

            List<List<object>> matrix = Utils.DataTable_.Get_ConvertDataTable_List(
                    columns: new List<string>() {
                            "SHOVID_VAL", "DATE_VAL", "SHIFT_VAL", "FIO_VAL",
                            "VOLUME_VAL", "MOTOHOURS_VAL", "WORK_TIME_VAL", "WORK_TIME_H_VAL",
                            "PERFORMANCE_F_VAL", "PERFORMANCE_P_VAL", "DIFFERENCE_VAL"
                        },
                    dataTable: dataTable
                );

            #endregion ANALYZE DATA



            #region FILL EXCEL

            Utils.Excel_ excelClass = new Utils.Excel_();
            excelClass.Method_Hide_Application(isVisible: false);
            excelClass.Method_Load_Template(
                    templateName: "template1",
                    templateNameDebug: "Отчёт производительности водителей экскаваторов.xlsx",
                    Xrtl_Container: Xrtl_Container
                );

            int rowIndex = 6;
            foreach (List<object> col in matrix)
            {
                int colIndex = 1;
                foreach (object cell in col)
                {
                    switch (Study.ExcelClass.UtilsClass.GetExcelColumnNameMethod(colIndex))
                    {
                        case "I":
                            {
                                excelClass.worksheet.Cells[rowIndex, colIndex].FormulaLocal = $"=ОКРУГЛВВЕРХ(E{rowIndex}/H{rowIndex}; {roundPoint})";
                                break;
                            }
                        case "K":
                            {
                                excelClass.worksheet.Cells[rowIndex, colIndex].FormulaLocal = $"=ОКРУГЛВВЕРХ(((I{rowIndex}-J{rowIndex})/J{rowIndex})*100; {roundPoint})";
                                break;
                            }
                        default:
                            {
                                excelClass.worksheet.Cells[rowIndex, colIndex].Value = cell;
                                break;
                            }
                    }
                    colIndex += 1;
                }
                rowIndex += 1;
            }

            excelClass.Method_Set_Cell_Value(
                    rowIndex: 3,
                    colIndex: 1,
                    value: $"сформировано: {Utils.DateTime_.Get_FormatOnlyDateTime_String(Utils.DateTime_.Get_Now_DateTime())}"
                );
            excelClass.Method_Hide_Application(isVisible: true);

            #endregion FILL EXCEL



            return dataTable;
        }

        #endregion Analyse Shov

        #region Download Reference

        private async void Button_Reference_Download_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime start = Utils.DateTime_.Get_Now_DateTime();
                Utils.Wpf.Set_ButtonText_String(
                        button: Button_Reference_Download,
                        text: $"ВЫГРУЗИТЬ справочник \n-загрузка-"
                    );



                #region ASYNC SELECT DATA

                DataTable dataTable = await Task.Run(() => DownloadReference());

                #endregion ASYNC SELECT DATA



                #region UPDATE DATASOURCE

                Utils.Wpf.Set_DataGridUpdate_DataTable(dataGrid: DataGrid_Reference, dataTable: dataTable);

                #endregion UPDATE DATASOURCE



                Utils.Wpf.Set_ButtonText_String(
                        button: Button_Reference_Download,
                        text: $"ВЫГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} " +
                        $"({Math.Truncate((Utils.DateTime_.Get_Now_DateTime() - start).TotalSeconds)} сек)"
                    );
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                Utils.Wpf.Set_ButtonText_String(
                        button: Button_Reference_Download,
                        text: $"ВЫГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} (ошибка)"
                    );
            }
        }

        private DataTable DownloadReference()
        {
            #region ASYNC SELECT DATA FROM DATABASE

            DataTable dataTable = new Utils.Sql(
                    sqlExpression: Queries.Get_Bogdan_custom_ShovDriverPerfomance_DownloadReference()
                ).ExecuteSelect();

            #endregion ASYNC SELECT DATA FROM DATABASE



            #region ANALYZE DATA

            List<List<object>> matrix = Utils.DataTable_.Get_ConvertDataTable_List(
                    columns: new List<string>() {
                            "SHOVID", "VOLUME"
                        },
                    dataTable: dataTable
                );

            #endregion ANALYZE DATA



            #region FILL EXCEL

            Utils.Excel_ excelClass = new Utils.Excel_();
            excelClass.Method_Hide_Application(isVisible: false);
            excelClass.Method_Load_Template(
                    templateName: "template2",
                    templateNameDebug: "Справочник производительности водителей экскаваторов.xlsx",
                    Xrtl_Container: Xrtl_Container
                );

            excelClass.Method_Fill_Sheet_From_List(
                    matrix: matrix,
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



            return dataTable;
        }

        #endregion Download Reference



        #region Upload Reference

        private async void Button_Reference_Upload_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime start = Utils.DateTime_.Get_Now_DateTime();
                Utils.Wpf.Set_ButtonText_String(
                        button: Button_Reference_Upload,
                        text: $"ЗАГРУЗИТЬ справочник \n-загрузка-"
                    );



                #region GET INPUT FROM USER INTERFACE

                string fileExcelPath = Utils.WinForms.OpenFileDialog_SelectedFilePath(
                        titleDialog: "Укажите Excel файл для загрузки",
                        filterFiles: @"Excel файл|*.xlsx|Excel файл(устаревший)|*.xls|Excel файл(с макросами)|*.xlsm"
                    );

                #endregion GET INPUT FROM USER INTERFACE



                #region ASYNC SELECT DATA

                DataTable dataTable = new DataTable();
                if (fileExcelPath.Length < 3)
                {
                    System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
                            "Вы не выбрали файл для загрузки!", "Отмена операции",
                            System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.Yes
                        );
                } else
                {
                    dataTable = await Task.Run(() => UploadReference(fileExcelPath));
                }

                #endregion ASYNC SELECT DATA



                #region UPDATE DATASOURCE

                Utils.Wpf.Set_DataGridUpdate_DataTable(dataGrid: DataGrid_Reference, dataTable: dataTable);

                #endregion UPDATE DATASOURCE



                Utils.Wpf.Set_ButtonText_String(
                        button: Button_Reference_Upload,
                        text: $"ЗАГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} " +
                        $"({Math.Truncate((Utils.DateTime_.Get_Now_DateTime() - start).TotalSeconds)} сек)"
                    );
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                Utils.Wpf.Set_ButtonText_String(
                        button: Button_Reference_Upload,
                        text: $"ЗАГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} (ошибка)"
                    );
            }
        }

        private DataTable UploadReference(string fileExcelPath)
        {

            #region READ EXCEL DATA

            //Utils.Excel_ excelClass = new Utils.Excel_();
            //excelClass.Method_Hide_Application(isVisible: false);
            //excelClass.Method_Load_Template(
            //        templateName: "template2",
            //        templateNameDebug: "Справочник производительности водителей экскаваторов.xlsx",
            //        Xrtl_Container: Xrtl_Container
            //    );

            Excel.Application excelApplication = new Excel.Application();
            Excel.Workbook workbook;
            Excel.Worksheet worksheet;

            excelApplication.DisplayAlerts = false;
            excelApplication.Visible = false;

            workbook = excelApplication.Workbooks.Add(fileExcelPath);
            Excel.Worksheet templateWorksheet = workbook.Worksheets[1];
            templateWorksheet.Copy(templateWorksheet);
            worksheet = workbook.Worksheets[2];
            workbook.Worksheets[1].Delete();
            worksheet.Name = "Лист 1";

            List<List<object>> rows = new List<List<object>>() { };
            for (int row = 6; row <= 50; row += 1)
            {
                try
                {
                    List<object> newRow = new List<object>() { };
                    for (int col = 1; col <= 2; col += 1)
                    {
                        object value = worksheet.Cells[row, col].Value;
                        newRow.Add(value);
                    }
                    if (!(newRow[0] is null) && !(newRow[1] is null))
                    {
                        rows.Add(newRow);
                    }
                } catch
                {

                }
            }
            workbook.Close();
            excelApplication.Quit();

            #endregion READ EXCEL DATA

            #region ASYNC UPDATE DATA IN DATABASE

            //Sql expression

            // Create specific query
            string cases = "";
            foreach (List<object> col in rows)
            {
                cases += $" SELECT TO_CHAR('{col[0]}') SHOVID, TO_NUMBER('{col[1]}') VOLUME FROM dual UNION ALL ";
            }

            #region sqlExpression
            string sqlExpression = @"

BEGIN
    FOR rows_to_upsert IN ( " + $"{cases.Substring(0, cases.Length - 10)}" +
            @" ) LOOP
        BEGIN
            INSERT INTO SHOV_DRIVER_PLAN t
                        (t.SHOVID,
                            t.VOLUME)
            SELECT ROWS_TO_UPSERT.shovid,
                    ROWS_TO_UPSERT.volume
            FROM   DUAL;
        EXCEPTION
            WHEN DUP_VAL_ON_INDEX THEN
                UPDATE SHOV_DRIVER_PLAN t
                SET    t.VOLUME = ROWS_TO_UPSERT.volume
                WHERE  t.SHOVID = ROWS_TO_UPSERT.shovid;
        END;
    END LOOP;
END; 

";
            #endregion sqlExpression

            // Sql query instanse
            int rowsAffected = new Utils.Sql(
                    sqlExpression: sqlExpression
                ).ExecuteUpdateOrInsert();

            #endregion ASYNC UPDATE DATA IN DATABASE

            #region ASYNC SELECT DATA FROM DATABASE

            //Sql expression
            #region sqlExpression

            string sqlExpressionSelect = @"



SELECT *
FROM   SHOV_DRIVER_PLAN
ORDER  BY SHOV_DRIVER_PLAN.SHOVID 




";
            #endregion sqlExpression

            // Sql query instanse
            DataTable dataTableSelect = new Utils.Sql(
                    sqlExpression: sqlExpressionSelect
                ).ExecuteSelect();

            #endregion ASYNC SELECT DATA FROM DATABASE

            return dataTableSelect;

        }

        #endregion Upload Reference
    }
}
