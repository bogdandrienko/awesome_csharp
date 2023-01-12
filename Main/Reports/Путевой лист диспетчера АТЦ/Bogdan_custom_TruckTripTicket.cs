using OracleClient = Oracle.ManagedDataAccess.Client;
using Excel = Microsoft.Office.Interop.Excel;
using XrtlExplorer;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Threading.Tasks;
using Utils = Main.Utils;


namespace Main.Reports
{
    public partial class Bogdan_custom_TruckTripTicket : UserControl, IXrtlControl
    {
        /// <summary>
        /// IXrtlControl Realization
        /// </summary>
        #region IXrtlControl Realization
        IXrtlExplorer m_xrtl;
        string m_templateFile;
        string m_templateFile2;
        public string BasePath
        {
            private set; get;
        }
        public object InternalControl => this;
        public string GetFieldFormat(string sFieldName) => string.Empty;
        public string[] GetFieldNames() => null;
        public string GetName() => "Main.Reports";
        public string GetQueryName() => string.Empty;
        public string GetValue(string sFieldName) => string.Empty;
        public void SendMessage(string sSender, string sReceiver, string sMessage)
        {
        }
        public void SetValue(string sFieldName, string sFieldValue)
        {
        }
        public void SetXmlDescription(string sXml)
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(sXml);
            m_templateFile = xml.SelectSingleNode("/Object").Attributes["template"].Value;
            m_templateFile2 = xml.SelectSingleNode("/Object").Attributes["template2"].Value;
        }
        public void SetXrtlInterface(IXrtlExplorer xrtlExplorer)
        {
            m_xrtl = xrtlExplorer;
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(m_xrtl.GetSystemInfo());
            BasePath = Path.Combine(new string[]
            {
                Directory.GetCurrentDirectory(),
                xml.SelectSingleNode("/SystemInfo/AppLocalDir").Attributes["value"].Value,
                xml.SelectSingleNode("/SystemInfo/AppSystem").Attributes["name"].Value,
            });
        }
        #endregion IXrtlControl Realization
        /// <summary>
        /// IXrtlControl Realization
        /// </summary>

        /// <summary>
        /// Class Constructor
        /// </summary>
        #region Class Constructor
        public Bogdan_custom_TruckTripTicket()
        {
            try
            {
                InitializeComponent();

                /// <summary>
                /// Initialize User Interface settings
                /// </summary>
                #region Initialize User Interface settings
                DateTime now = DateTime.Now.AddMonths(-1);

                dateTimePickerDateFromShov.Value = new DateTime(now.Year, now.Month, 1);
                dateTimePickerDateToShov.Value = new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month));
                #endregion Initialize User Interface settings
                /// <summary>
                /// Initialize User Interface settings
                /// </summary>
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);
            }
        }
        #endregion Class Constructor
        /// <summary>
        /// Class Constructor
        /// </summary>

        /// <summary>
        /// Click Button Analyse Shov
        /// </summary>
        #region Click Button Analyse Shov
        private void ButtonAnalyseShov_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime start = DateTime.Now;
                ButtonAnalyseTruck.Text = $"ОБНОВИТЬ \n-загрузка-";



                /// <summary>
                /// FILL EXCEL
                /// </summary>
                #region FILL EXCEL
                Excel.Application excelApplication = new Excel.Application();
                Excel.Workbook workbook;
                Excel.Worksheet worksheet;

                excelApplication.DisplayAlerts = false;
                excelApplication.Visible = false;

                try
                {
                    workbook = excelApplication.Workbooks.Add($"{BasePath}/{m_templateFile}");
                } catch
                {
                    workbook = excelApplication.Workbooks.Add($"{Directory.GetCurrentDirectory()}/templates/Путевой лист автосамосвалы.xlsx");
                }

                Excel.Worksheet templateWorksheet = workbook.Worksheets[1];
                templateWorksheet.Copy(templateWorksheet);
                worksheet = workbook.Worksheets[2];
                workbook.Worksheets[1].Delete();
                worksheet.Name = "Лист 1";

                //read
                List<Tuple<string, object>> tuples = new List<Tuple<string, object>>() {
                        new Tuple<string, object>("$KALENTIME", 27.0),
                        new Tuple<string, object>("$CHVHOZ", 16.0),
                        new Tuple<string, object>("$WT_ALL", 200.2),
                    };
                List<Tuple<int, int, object>> matrix = new List<Tuple<int, int, object>>() { };

                //bool FindEqual(string arg1, string arg2)
                //{
                //    return arg1 == arg2;
                //}
                //Predicate<string> predicate = FindEqual;

                for (int row = 1; row <= 50; row += 1)
                {
                    for (int col = 1; col <= 50; col += 1)
                    {
                        try
                        {
                            object obj = worksheet.Cells[row, col].Value;
                            foreach (Tuple<string, object> tuple in tuples)
                            {
                                if (tuple.Item1 == $"{obj}")
                                {
                                    matrix.Add(new Tuple<int, int, object>(row, col, tuple.Item2));
                                }
                            }
                        } catch (Exception exception)
                        {
                            Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);
                        }
                    }
                }

                foreach (Tuple<int, int, object> val in matrix)
                {
                    worksheet.Cells[val.Item1, val.Item2].Value = val.Item3;
                }
                worksheet.Cells[3, 1].Value = $"сформировано: {Utils.DateTime_.Get_FormatOnlyDateTime_String(Utils.DateTime_.Get_Now_DateTime())}";

                excelApplication.DisplayAlerts = true;
                excelApplication.Visible = true;
                #endregion FILL EXCEL
                /// <summary>
                /// FILL EXCEL
                /// </summary>





                ButtonAnalyseTruck.Text = $"ОБНОВИТЬ \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} ({Math.Truncate((DateTime.Now - start).TotalSeconds)} сек)";
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                ButtonAnalyseTruck.Text = $"ОБНОВИТЬ \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} (ошибка)";
            }
        }
        #endregion Click Button Analyse Shov
        /// <summary>
        /// Click Button Analyse Shov
        /// </summary>

        /// <summary>
        /// Async Analyse Shov
        /// </summary>
        #region Async Analyse Shov
        private async Task<DataTable> AnalyseShov(DateTime paramDateFrom, int paramShiftFrom, DateTime paramDateTo, int paramShiftTo, string paramSelectTechId, int roundPoint)
        {
            try
            {
                /// <summary>
                /// ASYNC SELECT DATA FROM DATABASE
                /// </summary>
                #region ASYNC SELECT DATA FROM DATABASE

                //Sql expression
                #region sqlExpression
                string sqlExpression = @"




SELECT t.*,
       ROUND(( ( t.PERFORMANCE_F_VAL - t.PERFORMANCE_P_VAL ) / t.PERFORMANCE_P_VAL ) * 100, :paramRoundPoint) AS difference_val
FROM   (SELECT 1                                                                                                                                                                  AS count_val,
               SHOV_ID                                                                                                                                                            AS shovid_val,
               TASK_DATE                                                                                                                                                          AS date_val,
               SHIFT                                                                                                                                                              AS shift_val,
               TRIM(FAMNAME)
               || ' '
               || TRIM(FIRSTNAME)
               || ' '
               || TRIM(SECNAME)                                                                                                                                                   AS fio_val,
               NVL(VOLUMEITOG, 0)                                                                                                                                                 AS volume_val,
               ROUND(MOTOHOURS, :paramRoundPoint)                                                                                                                                 AS motohours_val,
               TO_CHAR(MOVE_TIME_AUTO, 'HH;MI;SS')                                                                                                                                AS work_time_val,
               ROUND(( TO_DATE('01/12/1999 '
                               || TO_CHAR(MOVE_TIME_AUTO, 'HH;MI;SS'), 'DD-MM-YYYY HH24;MI;SS') - DATE '1999-12-01' ) * 24, :paramRoundPoint)                                     AS work_time_h_val,
               ROUND(NVL(VOLUMEITOG, 0) / NULLIF(( ( TO_DATE('01/12/1999 '
                                                             || TO_CHAR(MOVE_TIME_AUTO, 'HH;MI;SS'), 'DD-MM-YYYY HH24;MI;SS') - DATE '1999-12-01' ) * 24 ), 0), :paramRoundPoint) AS performance_f_val,
               NVL(VOLUME, 0.001)                                                                                                                                                 AS performance_p_val
        FROM   (SELECT tasks_t.*,
                       drivers_t.*,
                       details_t.*,
                       reports_t.*,
                       plan_t.*,
                       (SELECT SUM(SHOV_SHIFT_REPORTS_ADV.VOLUME_ITOG)
                        FROM   SHOV_SHIFT_REPORTS_ADV
                        WHERE  SHOV_SHIFT_REPORTS_ADV.REPORT_ID = reports_t.ID) AS VOLUMEITOG
                FROM   SHOV_SHIFT_TASKS tasks_t
                       left join SHOVDRIVERS drivers_t
                              ON tasks_t.TABEL_NUM = drivers_t.TABELNUM
                       left join SHOV_DRIVER_PLAN plan_t
                              ON tasks_t.SHOV_ID = plan_t.SHOVID
                       left join SHOV_SHIFT_TASK_DETAILS details_t
                              ON tasks_t.ID = details_t.TASK_ID
                       left join SHOV_SHIFT_REPORTS reports_t
                              ON tasks_t.ID = reports_t.TASK_ID
                WHERE  ( :paramSelectTechId = 'Все'
                          OR tasks_t.SHOV_ID = :paramSelectTechId )
                       AND ( TASK_DATE BETWEEN :ParamDateFrom AND :ParamDateTo )
                       AND ( ( TASK_DATE > :ParamDateFrom
                               AND :ParamDateTo > TASK_DATE )
                              OR ( :ParamDateFrom = TASK_DATE
                                   AND SHIFT >= :ParamShiftFrom
                                   AND :ParamDateTo > :ParamDateFrom )
                              OR ( :ParamDateTo = TASK_DATE
                                   AND :ParamShiftTo >= SHIFT
                                   AND :ParamDateTo > :ParamDateFrom )
                              OR ( :ParamDateTo = TASK_DATE
                                   AND SHIFT BETWEEN :ParamShiftFrom AND :ParamShiftTo
                                   AND :ParamDateFrom = :ParamDateTo ) )
                ORDER  BY TASK_DATE ASC,
                          SHIFT ASC,
                          SHOV_ID ASC)) t 





";
                #endregion sqlExpression

                // Sql query instanse
                DataTable dataTable = await new Utils.Sql(
                        sqlExpression: sqlExpression,
                        queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateFrom", OracleClient.OracleDbType.Date, paramDateFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftFrom", OracleClient.OracleDbType.Int32, paramShiftFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateTo", OracleClient.OracleDbType.Date, paramDateTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftTo", OracleClient.OracleDbType.Int32, paramShiftTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramRoundPoint", OracleClient.OracleDbType.Int32, roundPoint),
                        }
                    ).ExecuteSelectAsync();
                #endregion ASYNC SELECT DATA FROM DATABASE
                /// <summary>
                /// ASYNC SELECT DATA FROM DATABASE
                /// </summary>

                /// ANALYZE DATA
                /// </summary>
                #region ANALYZE DATA
                List<List<object>> matrix = new List<List<object>>() {};
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    List<object> row = new List<object>();
                    foreach (string column in new List<string> {
                            "SHOVID_VAL", "DATE_VAL", "SHIFT_VAL", "FIO_VAL",
                            "VOLUME_VAL", "MOTOHOURS_VAL", "WORK_TIME_VAL", "WORK_TIME_H_VAL",
                            "PERFORMANCE_F_VAL", "PERFORMANCE_P_VAL", "DIFFERENCE_VAL"
                        })
                    {
                        try
                        {
                            row.Add(dataRow[column]);
                        } catch
                        {
                            row.Add("ошибка");
                        }
                    }
                    matrix.Add(row);
                }
                #endregion ANALYZE DATA
                /// <summary>
                /// ANALYZE DATA
                /// </summary>

                /// <summary>
                /// FILL EXCEL
                /// </summary>
                #region FILL EXCEL
                Excel.Application excelApplication = new Excel.Application();
                Excel.Workbook workbook;
                Excel.Worksheet worksheet;

                excelApplication.DisplayAlerts = false;
                excelApplication.Visible = false;

                try
                {
                    workbook = excelApplication.Workbooks.Add($"{BasePath}/{m_templateFile}");
                } catch
                {
                    workbook = excelApplication.Workbooks.Add($"{Directory.GetCurrentDirectory()}/templates/Отчёт производительности водителей экскаваторов.xlsx");
                }

                Excel.Worksheet templateWorksheet = workbook.Worksheets[1];
                templateWorksheet.Copy(templateWorksheet);
                worksheet = workbook.Worksheets[2];
                workbook.Worksheets[1].Delete();
                worksheet.Name = "Лист 1";

                int startRowValue = 6;
                foreach (List<object> col in matrix)
                {
                    int startColumnValue = 1;
                    foreach (object cell in col)
                    {
                        worksheet.Cells[startRowValue, startColumnValue].Value = cell;
                        startColumnValue += 1;
                    }
                    startRowValue += 1;
                }
                worksheet.Cells[3, 1].Value = $"сформировано: {Utils.DateTime_.Get_FormatOnlyDateTime_String(Utils.DateTime_.Get_Now_DateTime())}";

                excelApplication.DisplayAlerts = true;
                excelApplication.Visible = true;
                #endregion FILL EXCEL
                /// <summary>
                /// FILL EXCEL
                /// </summary>
                
                return dataTable;
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                return new DataTable();
            }
        }
        #endregion Async Analyse Shov
        /// <summary>
        /// Async Analyse Shov
        /// </summary>

        /// <summary>
        /// Click Button Download Reference
        /// </summary>
        #region Click Button Download Reference
        private async void ButtonDownloadReference_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime start = DateTime.Now;
                ButtonDownloadReference.Text = $"ВЫГРУЗИТЬ справочник \n-загрузка-";

                /// <summary>
                /// ASYNC SELECT DATA
                /// </summary>
                #region ASYNC SELECT DATA
                DataTable dataTable = await Task.Run(() => DownloadReference());
                #endregion ASYNC SELECT DATA
                /// <summary>
                /// ASYNC SELECT DATA
                /// </summary>

                /// <summary>
                /// UPDATE DATASOURCE
                /// </summary>
                #region UPDATE DATASOURCE
                dataGridView2.DataSource = dataTable;
                #endregion UPDATE DATASOURCE
                /// <summary>
                /// UPDATE DATASOURCE
                /// </summary>

                ButtonDownloadReference.Text = $"ВЫГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} ({Math.Truncate((DateTime.Now - start).TotalSeconds)} сек)";
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                ButtonDownloadReference.Text = $"ВЫГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} (ошибка)";
            }
        }
        #endregion Click Button Download Reference
        /// <summary>
        /// Click Button Download Reference
        /// </summary>

        /// <summary>
        /// Async Download Reference
        /// </summary>
        #region Async Download Reference
        private async Task<DataTable> DownloadReference()
        {
            try
            {
                /// <summary>
                /// ASYNC SELECT DATA FROM DATABASE
                /// </summary>
                #region ASYNC SELECT DATA FROM DATABASE

                //Sql expression
                #region sqlExpression
                string sqlExpression = @"



SELECT *
FROM   SHOV_DRIVER_PLAN
ORDER  BY SHOV_DRIVER_PLAN.SHOVID 




";
                #endregion sqlExpression

                // Sql query instanse
                DataTable dataTable = await new Utils.Sql(
                        sqlExpression: sqlExpression
                    ).ExecuteSelectAsync();
                #endregion ASYNC SELECT DATA FROM DATABASE
                /// <summary>
                /// ASYNC SELECT DATA FROM DATABASE
                /// </summary>

                /// <summary>
                /// ANALYZE DATA
                /// </summary>
                #region ANALYZE DATA
                List<List<object>> matrix = new List<List<object>>() { };
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    List<object> row = new List<object>();
                    foreach (string column in new List<string> { "SHOVID", "VOLUME" })
                    {
                        try
                        {
                            row.Add(dataRow[column]);
                        } catch
                        {
                            row.Add("ошибка");
                        }
                    }
                    matrix.Add(row);
                }
                #endregion ANALYZE DATA
                /// <summary>
                /// ANALYZE DATA
                /// </summary>

                /// <summary>
                /// FILL EXCEL
                /// </summary>
                #region FILL EXCEL
                Excel.Application excelApplication = new Excel.Application();
                Excel.Workbook workbook;
                Excel.Worksheet worksheet;

                excelApplication.DisplayAlerts = false;
                excelApplication.Visible = false;

                try
                {
                    workbook = excelApplication.Workbooks.Add($"{BasePath}/{m_templateFile2}");
                } catch
                {
                    workbook = excelApplication.Workbooks.Add($"{Directory.GetCurrentDirectory()}/templates/Справочник производительности водителей экскаваторов.xlsx");
                }

                Excel.Worksheet templateWorksheet = workbook.Worksheets[1];
                templateWorksheet.Copy(templateWorksheet);
                worksheet = workbook.Worksheets[2];
                workbook.Worksheets[1].Delete();
                worksheet.Name = "Лист 1";

                int startRowValue = 6;
                foreach (List<object> col in matrix)
                {
                    int startColumnValue = 1;
                    foreach (object cell in col)
                    {
                        worksheet.Cells[startRowValue, startColumnValue].Value = cell;
                        startColumnValue += 1;
                    }
                    startRowValue += 1;
                }
                worksheet.Cells[3, 1].Value = $"сформировано: {Utils.DateTime_.Get_FormatOnlyDateTime_String(Utils.DateTime_.Get_Now_DateTime())}";

                excelApplication.DisplayAlerts = true;
                excelApplication.Visible = true;
                #endregion FILL EXCEL
                /// <summary>
                /// FILL EXCEL
                /// </summary>

                return dataTable;
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                return new DataTable();
            }
        }
        #endregion Async Download Reference
        /// <summary>
        /// Async Download Reference
        /// </summary>

        /// <summary>
        /// Click Button Download Reference
        /// </summary>
        #region Click Button Upload Reference
        private async void ButtonUploadReference_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime start = DateTime.Now;
                ButtonUploadReference.Text = $"ЗАГРУЗИТЬ справочник \n-загрузка-";

                /// <summary>
                /// GET INPUT FROM USER INTERFACE
                /// </summary>
                #region GET INPUT FROM USER INTERFACE
                string fileExcelPath = Utils.WinForms.OpenFileDialog_SelectedFilePath(
                    titleDialog: "Укажите Excel файл для загрузки",
                    filterFiles: @"Excel файл|*.xlsx|Excel файл(устаревший)|*.xls|Excel файл(с макросами)|*.xlsm"
                );
                #endregion GET INPUT FROM USER INTERFACE
                /// <summary>
                /// GET INPUT FROM USER INTERFACE
                /// </summary>

                /// <summary>
                /// ASYNC SELECT DATA
                /// </summary>
                #region ASYNC SELECT DATA
                DataTable dataTable = new DataTable();
                if (fileExcelPath.Length < 3)
                {
                    MessageBox.Show("Вы не выбрали файл для загрузки!", "Отмена операции", MessageBoxButtons.OK);
                } else
                {
                    dataTable = await Task.Run(() => UploadReference(fileExcelPath));
                }
                #endregion ASYNC SELECT DATA
                /// <summary>
                /// ASYNC SELECT DATA
                /// </summary>

                /// <summary>
                /// UPDATE DATASOURCE
                /// </summary>
                #region UPDATE DATASOURCE
                dataGridView2.DataSource = dataTable;
                #endregion UPDATE DATASOURCE
                /// <summary>
                /// UPDATE DATASOURCE
                /// </summary>

                ButtonUploadReference.Text = $"ЗАГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} ({Math.Truncate((Utils.DateTime_.Get_Now_DateTime() - start).TotalSeconds)} сек)";
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                ButtonUploadReference.Text = $"ЗАГРУЗИТЬ справочник \n{Utils.DateTime_.Get_FormatOnlyTime_String(Utils.DateTime_.Get_Now_DateTime())} (ошибка)";
            }
        }
        #endregion Click Button Upload Reference
        /// <summary>
        /// Click Button Upload Reference
        /// </summary>

        /// <summary>
        /// Async Upload Reference
        /// </summary>
        #region Async Upload Reference
        private DataTable UploadReference(string fileExcelPath)
        {
            try
            {
                /// <summary>
                /// READ EXCEL DATA
                /// </summary>
                #region READ EXCEL DATA
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
                /// <summary>
                /// READ EXCEL DATA
                /// </summary>

                /// <summary>
                /// ASYNC UPDATE DATA IN DATABASE
                /// </summary>
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
                /// <summary>
                /// ASYNC UPDATE DATA IN DATABASE
                /// </summary>

                /// <summary>
                /// ASYNC SELECT DATA FROM DATABASE
                /// </summary>
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
                /// <summary>
                /// ASYNC SELECT DATA FROM DATABASE
                /// </summary>

                return dataTableSelect;
            } catch (Exception exception)
            {
                Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                return new DataTable();
            }
        }
        #endregion Async Upload Reference
        /// <summary>
        /// Async Upload Reference
        /// </summary>
    }
}
