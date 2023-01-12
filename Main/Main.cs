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


namespace Main
{
    public class Utils
    {
        public class Testing
        {
            public static string RunMethod()
            {
                try
                {
                    Console.WriteLine("start");
                    DateTime start = DateTime.Now;

                    return $"stop";
                } catch (Exception error)
                {
                    Utils.Debug.Set_ShowToWindowScreen(text: $"text: {error}", isXrtl: false);
                    Console.WriteLine($"error: {error}");

                    return $"error: {error}";
                }
            }
        }

        public class Debug
        {
            public static void Set_PrintToConsole(object text, bool isNewLine)
            {
                if (isNewLine)
                {
                    Console.WriteLine(text);
                } else
                {
                    Console.Write(text);
                }
            }

            public static void Set_ShowToWindowScreen(string text, bool isXrtl)
            {
                if (isXrtl)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show(text);
                } else
                {
                    MessageBox.Show(text);
                }
            }

            public static void Set_ExceptionPrintAndShow(Exception exception, bool isShowScreenWindow)
            {
                string exceptionText = $"{exception}";
                Utils.Debug.Set_PrintToConsole(text: exceptionText, isNewLine: true);
                if (isShowScreenWindow)
                {
                    Utils.Debug.Set_ShowToWindowScreen(text: exceptionText, isXrtl: false);
                }
            }

            public static void DelaySync(int milliseconds)
            {
                System.Threading.Thread.Sleep(millisecondsTimeout: milliseconds);
            }

            public static async Task<int> DelayAsync(int milliseconds)
            {
                await System.Threading.Tasks.Task.Delay(millisecondsDelay: milliseconds);
                return 1;
            }
        }

        public class Extra
        {
            public static List<List<object>> Get_Sliced_List(List<List<object>> matrix, int startIndex, int stopIndex)
            {
                if (stopIndex < 0)
                {
                    stopIndex = matrix.Count;
                }
                return matrix.Skip(startIndex).Take(stopIndex).ToList();
            }
        }

        public class Report
        {
            public static List<string> Get_Dumtruck_List()
            {
                // all dumtrucks
                return new List<string>() {
                            "101", "103", "105", "106", "107", "108", "109", "110", "114", "115", "116",
                            "117", "119", "120", "121", "122", "124", "125", "126", "127", "128", "129",
                            "130", "131", "132", "133", "134", "135", "136", "137", "138", "139", "140"
                        };
            }

            public static List<string> Get_Shovel_List()
            {
                // all shovels
                return new List<string>() {
                            "001", "003", "201", "202", "203", "205", "206", "207", "401"
                        };
            }

            public static List<string> Get_Aux_List()
            {
                // all auxes "41", "601", "603", "607", "608", "609", "702", "2222", "3333", "8733"
                return new List<string>() {
                            "601", "603", "607", "608", "609", "702"
                        };
            }

            public class AuxStoppages
            {
                public static async Task<DataTable> GetDataFirst(DateTime paramDateFrom, int paramShiftFrom, DateTime paramDateTo, int paramShiftTo, int paramSelectTechId)
                {
                    return await new _Utils.Sql(
                        sqlExpression: _Queries.Get_Bogdan_custom_AuxStoppages(),
                        queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                    new Tuple<string, OracleClient.OracleDbType, object>("paramDateFrom", OracleClient.OracleDbType.Date, paramDateFrom),
                    new Tuple<string, OracleClient.OracleDbType, object>("paramShiftFrom", OracleClient.OracleDbType.Int32, paramShiftFrom),
                    new Tuple<string, OracleClient.OracleDbType, object>("paramDateTo", OracleClient.OracleDbType.Date, paramDateTo),
                    new Tuple<string, OracleClient.OracleDbType, object>("paramShiftTo", OracleClient.OracleDbType.Int32, paramShiftTo),
                    new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Int32, paramSelectTechId),
                        }
                        ).ExecuteSelectAsync();
                }

                public static DataTable GetDataSecond(DataTable dataTable, int timeDiff)
                {
                    List<DataRow> allValues = dataTable.AsEnumerable().ToList();
                    List<List<DataRow>> intervals = new List<List<DataRow>>();
                    DateTime dateTimeFirst = DateTime.Now;
                    while (true)
                    {
                        try
                        {
                            Tuple<List<DataRow>, int> tuple = ExtractFirstInterval(rows: allValues, dateTimeFirst);
                            dateTimeFirst = (DateTime)tuple.Item1[tuple.Item1.Count - 1]["TIME"];
                            DateTime dateTimeLast = (DateTime)tuple.Item1[0]["TIME"];
                            if ((dateTimeLast - dateTimeFirst).TotalSeconds > timeDiff * 60)
                            {
                                intervals.Add(tuple.Item1);
                            }
                            allValues = allValues.Where((value, index) => index >= tuple.Item2 - 1 && index <= allValues.Count - 1).ToList();
                        } catch
                        {
                            break;
                        }
                    }

                    List<List<string>> stoppages = new List<List<string>>();
                    foreach (List<DataRow> row in intervals)
                    {
                        string tech = $"{row[0]["TECH"]}";
                        DateTime dateTimeFirst1 = (DateTime)row[0]["TIME"];
                        DateTime dateTimeLast1 = (DateTime)row[row.Count - 1]["TIME"];
                        TimeSpan difference = dateTimeFirst1 - dateTimeLast1;

                        if (row.Count > 0)
                        {
                            stoppages.Add(
                                    new List<string>() {
                                tech,
                                dateTimeLast1.ToString("dd.MM.yyyy HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                                dateTimeFirst1.ToString("dd.MM.yyyy HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo),
                                $"{Math.Round((float)difference.TotalMinutes, 1, MidpointRounding.ToEven)}"
                                });
                        }
                    }

                    DataTable dataTableResult = new DataTable();
                    if (stoppages.Count() > 0)
                    {
                        for (int column = 1; column <= stoppages[0].Count; column += 1)
                        {
                            dataTableResult.Columns.Add($"{column}", typeof(object));
                        }
                        for (int row = 1; row <= stoppages.Count; row++)
                        {
                            DataRow dataRow = dataTableResult.NewRow();
                            for (int column = 1; column <= stoppages[0].Count; column += 1)
                            {
                                dataRow[$"{column}"] = stoppages[row - 1][column - 1];
                            }
                            dataTableResult.Rows.Add(dataRow);
                        }

                        return dataTableResult;
                    } else
                    {
                        return new DataTable();
                    }
                }

                public static Tuple<List<DataRow>, int> ExtractFirstInterval(List<DataRow> rows, DateTime dateTimeStart)
                {
                    List<DataRow> local_interval = new List<DataRow>();
                    DateTime target_DateTime = DateTime.Now;
                    int target_fuel = 0;
                    int last_index = 0;
                    foreach (DataRow row in rows)
                    {
                        DateTime local_DateTime = (DateTime)row["TIME"];
                        int target_DateTimetSec = (int)((target_DateTime - target_DateTime.Date).TotalSeconds);
                        int local_DateTimeSec = (int)((local_DateTime - local_DateTime.Date).TotalSeconds);

                        target_DateTime = local_DateTime;
                        int local_speed = int.Parse(row["SPEED"].ToString());
                        int local_fuel = int.Parse(row["FUEL"].ToString());
                        if (dateTimeStart > local_DateTime)
                        {
                            if (local_interval.Count() == 0)
                            {
                                if (local_speed == 0)
                                {
                                    target_fuel = local_fuel;
                                    local_interval.Add(row);
                                }
                            } else
                            {
                                if (local_speed == 0 && local_fuel == target_fuel && (target_DateTimetSec - local_DateTimeSec) < 10 * 60)
                                {
                                    target_fuel = local_fuel;
                                    local_interval.Add(row);
                                } else
                                {
                                    break;
                                }
                            }
                        }
                        last_index += 1;
                    }

                    return new Tuple<List<DataRow>, int>(local_interval, last_index);
                }

                public static async Task<List<List<object>>> GetData(DateTime paramDateFrom, int paramShiftFrom, DateTime paramDateTo, int paramShiftTo, string paramSelectTechId, int timeDiff, List<Tuple<DateTime, DateTime>> excludes, Utils.WinForms.WinForms_Container Xrtl_Container)
                {
                    List<string> TechIds;
                    if (paramSelectTechId == "Все")
                    {
                        TechIds = Utils.Report.Get_Aux_List();
                    } else
                    {
                        TechIds = new List<string>() { paramSelectTechId };
                    }

                    List<List<object>> matrix = new List<List<object>>() { new List<object>() { "№", "Начало", "Конец", "Длительность(мин)" } };

                    foreach (string tech in TechIds)
                    {
                        DataTable result1 = await Task.Run(() => GetDataFirst(
                                paramDateFrom: paramDateFrom,
                                paramShiftFrom: paramShiftFrom,
                                paramDateTo: paramDateTo,
                                paramShiftTo: paramShiftTo,
                                paramSelectTechId: int.Parse(tech)
                            ));

                        if (result1.Rows.Count < 1)
                        {
                            continue;
                        }

                        DataTable result2 = await Task.Run(() => ExcludeFromSourceListDataTimeRanges(dataTable: result1, excludes: excludes));

                        DataTable result3 = await Task.Run(() => GetDataSecond(dataTable: result2, timeDiff: timeDiff));

                        matrix = Utils.DataTable_.Get_ConvertDataTable_List(
                                dataTable: result3,
                                columns: new List<string>() { "1", "2", "3", "4" },
                                matrix: matrix
                            );
                    }
                    if (matrix.Count < 2)
                    {
                        return new List<List<object>>() { };
                    }
                    return matrix;
                }

                public static DataTable ExcludeFromSourceListDataTimeRanges(DataTable dataTable, List<Tuple<DateTime, DateTime>> excludes)
                {
                    DataTable dataTableNew = dataTable.Clone();
                    foreach (DataRow row in dataTable.Rows)
                    {
                        bool isNeedToExclude = false;
                        foreach (Tuple<DateTime, DateTime> exclude in excludes)
                        {
                            if (CheckDateTimeInRange(rangeStart: exclude.Item1, rangeStop: exclude.Item2, dateTimeToCheck: (DateTime)row["TIME"]))
                            {
                                isNeedToExclude = true;
                                break;
                            }
                        }
                        if (isNeedToExclude)
                        {
                            continue;
                        }
                        dataTableNew.ImportRow(row);
                    }
                    return dataTableNew;
                }

                public static DataTable GetDataThird(DataTable dataTable, Tuple<DateTime, DateTime> tupleDateTimeExclude)
                {
                    DataTable newDataTable = dataTable.Clone();
                    List<DataRow> allValues = dataTable.AsEnumerable().ToList();
                    foreach (DataRow i in allValues)
                    {
                        DateTime from = DateTime.ParseExact((string)i["1"], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime to = DateTime.ParseExact((string)i["2"], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        int fromSeconds = (int)((from - from.Date).TotalSeconds);
                        int fromSecondsDiff = (int)((tupleDateTimeExclude.Item1 - tupleDateTimeExclude.Item1.Date).TotalSeconds);

                        int toSeconds = (int)((to - to.Date).TotalSeconds);
                        int toSecondsDiff = (int)((tupleDateTimeExclude.Item2 - tupleDateTimeExclude.Item2.Date).TotalSeconds);

                        if (CheckDateTimeInRange(rangeStart: from, rangeStop: to, dateTimeToCheck: tupleDateTimeExclude.Item1) ||
                            CheckDateTimeInRange(rangeStart: from, rangeStop: to, dateTimeToCheck: tupleDateTimeExclude.Item2) ||
                            (fromSeconds > fromSecondsDiff && toSeconds < toSecondsDiff))
                        {
                        } else
                        {
                            newDataTable.Rows.Add(i.ItemArray);
                        }
                    }

                    return newDataTable;
                }

                public static bool CheckDateTimeInRange(DateTime rangeStart, DateTime rangeStop, DateTime dateTimeToCheck)
                {
                    int rangeStartSec = (int)((rangeStart - rangeStart.Date).TotalSeconds);
                    int rangeStopSec = (int)((rangeStop - rangeStop.Date).TotalSeconds);
                    int rangeCurrentSec = (int)((dateTimeToCheck - dateTimeToCheck.Date).TotalSeconds);

                    if (rangeCurrentSec > rangeStartSec && rangeCurrentSec < rangeStopSec)
                    {
                        return true;
                    } else
                    {
                        return false;
                    }
                }
            }

            public class OperPoroda
            {
                public static DataTable AnalyseTruck(DateTime paramDateFrom, int paramShiftFrom, DateTime paramDateTo, int paramShiftTo, string paramSelectTechId, int roundPoint)
                {

                    #region ASYNC SELECT DATA FROM DATABASE

                    DataTable dataTableSource = new Utils.Sql(
                            sqlExpression: Queries.Get_Bogdan_custom_OperPoroda_Truck(),
                            queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateFrom", OracleClient.OracleDbType.Date, paramDateFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftFrom", OracleClient.OracleDbType.Int32, paramShiftFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateTo", OracleClient.OracleDbType.Date, paramDateTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftTo", OracleClient.OracleDbType.Int32, paramShiftTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            }
                        ).ExecuteSelect();

                    #endregion ASYNC SELECT DATA FROM DATABASE



                    #region ANALYSE AND CONVERT DATATABLE

                    // Create headers
                    List<List<object>> matrix = new List<List<object>>() {
                            new List<object>() {"Хоз. номер", "Тип", "Тип", "Объём", "Расстояние"}
                        };

                    // Create body
                    foreach (DataRow dataRow in dataTableSource.Rows)
                    {
                        // Convert DataRow to List objects
                        List<List<object>> matrixTemp = new List<List<object>> {
                            new List<object> { dataRow["TECH_ID"], "тр,км", "тр", dataRow["val_tr"], dataRow["len_tr"] },
                            new List<object> { "", "ск,км", "ск", dataRow["val_sk"], dataRow["len_sk"] },
                            new List<object> { "", "рых,км", "рых", dataRow["val_rih"], dataRow["len_rih"] },
                            new List<object> { "", "ПРС,км", "прс", dataRow["val_prs"], dataRow["len_prs"] },
                            new List<object> { "", "руда,км", "руд", dataRow["val_rud"], dataRow["len_rud"] },
                        };

                        // Fill matrix from List
                        foreach (List<object> i in matrixTemp)
                        {
                            // Round decimal values
                            List<object> newRow = new List<object>();
                            foreach (object j in i)
                            {
                                try
                                {
                                    newRow.Add(Math.Round(d: Convert.ToDecimal(j), decimals: roundPoint, mode: MidpointRounding.ToEven));
                                } catch
                                {
                                    newRow.Add(j);
                                }
                            }
                            matrix.Add(newRow);
                        }
                    }

                    DataTable dataTable = Utils.DataTable_.Get_ConvertList_DataTable(
                            columns: new List<string>() { "Хоз номер", "Тип", "Тип ", "Объём", "Расстояние" },
                            matrix: Utils.Extra.Get_Sliced_List(matrix: matrix, startIndex: 1, stopIndex: -1)
                        );

                    #endregion ANALYSE AND CONVERT DATATABLE



                    return dataTable;
                }

                public static DataTable AnalyseShov(DateTime paramDateFrom, int paramShiftFrom, DateTime paramDateTo, int paramShiftTo, string paramSelectTechId, int roundPoint)
                {
                    #region ASYNC SELECT DATA FROM DATABASE

                    DataTable dataTableSource = new Utils.Sql(
                            sqlExpression: Queries.Get_Bogdan_custom_OperPoroda_Shov2(),
                            queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateFrom", OracleClient.OracleDbType.Date, paramDateFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftFrom", OracleClient.OracleDbType.Int32, paramShiftFrom),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramDateTo", OracleClient.OracleDbType.Date, paramDateTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramShiftTo", OracleClient.OracleDbType.Int32, paramShiftTo),
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            }
                        ).ExecuteSelect();

                    #endregion ASYNC SELECT DATA FROM DATABASE



                    #region ANALYSE AND CONVERT DATATABLE

                    // Create headers
                    List<List<object>> matrix = new List<List<object>>() {
                            new List<object>() {
                                "Погрузочная техника", "ед. изм.", "Руда", "Руда",
                                "Скала", "Скала", "Транзитная", "Транзитная",
                                "Рыхлая", "Рыхлая", "ПРС" , "ПРС"
                            },
                            new List<object>() {
                                "Погрузочная техника", "ед. изм.", "Оперучет АСД", "Замер",
                                "Оперучет АСД", "Замер", "Оперучет АСД", "Замер",
                                "Оперучет АСД", "Замер", "Оперучет АСД", "Замер"
                            }
                        };

                    // Create body
                    foreach (DataRow dataRow in dataTableSource.Rows)
                    {
                        // Convert DataRow to List objects
                        List<List<object>> matrixTemp = new List<List<object>> {
                            new List<object> {
                                $"{dataRow["category"]} {dataRow["MODEL"]} - №{dataRow["TECH_ID"]}", "м3",
                                dataRow["val_rud"], "", dataRow["val_sk"], "", dataRow["val_tr"], "", dataRow["val_rih"], "", dataRow["val_prs"], ""
                            },
                        };

                        // Fill matrix from List
                        foreach (List<object> matrixTempObjList in matrixTemp)
                        {
                            // Round double values
                            List<object> newRow = new List<object>();
                            foreach (object matrixTempObj in matrixTempObjList)
                            {
                                try
                                {
                                    newRow.Add(Math.Round(d: Convert.ToDecimal(matrixTempObj), decimals: roundPoint, mode: MidpointRounding.ToEven));
                                } catch
                                {
                                    newRow.Add(matrixTempObj);
                                }
                            }
                            matrix.Add(newRow);
                        }
                    }

                    DataTable dataTable = Utils.DataTable_.Get_ConvertList_DataTable(
                        columns: new List<string>() {
                        "Погрузочная техника", "Ед изм", "Руда АСД", "Руда Замер",
                        "Скала АСД", "Скала Замер", "Транзитная АСД", "Транзитная Замер",
                        "Рыхлая АСД", "Рыхлая Замер", "ПРС АСД" , "ПРС Замер"
                            },
                        matrix: Utils.Extra.Get_Sliced_List(matrix: matrix, startIndex: 2, stopIndex: -1)
                    );

                    #endregion ANALYSE AND CONVERT DATATABLE



                    return dataTable;
                }
            }

            public class TechVisualization
            {
                public static DataTable AnalyseTruck(string paramSelectTechId)
                {
                    #region ASYNC SELECT DATA FROM DATABASE

                    DataTable dataTableSource = new Utils.Sql(
                            sqlExpression: Queries.Get_Bogdan_custom_Vehtrips_By_Shovel(),
                            queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            }
                        ).ExecuteSelect();

                    #endregion ASYNC SELECT DATA FROM DATABASE


                    return dataTableSource;
                }

                public static DataTable LastShovelState(string paramSelectTechId)
                {
                    #region ASYNC SELECT DATA FROM DATABASE

                    DataTable dataTableSource = new Utils.Sql(
                            sqlExpression: Queries.Get_Bogdan_custom_Last_Shovel_State(),
                            queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            }
                        ).ExecuteSelect();

                    #endregion ASYNC SELECT DATA FROM DATABASE


                    return dataTableSource;
                }

                public static DataTable LastShovelArhiveRecord(string paramSelectTechId)
                {
                    #region ASYNC SELECT DATA FROM DATABASE

                    DataTable dataTableSource = new Utils.Sql(
                            sqlExpression: Queries.Get_Bogdan_custom_Last_Shovel_Arhive_Record(),
                            queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            }
                        ).ExecuteSelect();

                    #endregion ASYNC SELECT DATA FROM DATABASE


                    return dataTableSource;
                }

                public static DataTable Get_Bogdan_custom_Vehtrips_Analyze_By_Shovel(string paramSelectTechId)
                {
                    #region ASYNC SELECT DATA FROM DATABASE

                    DataTable dataTableSource = new Utils.Sql(
                            sqlExpression: Queries.Get_Bogdan_custom_Vehtrips_Analyze_By_Shovel(),
                            queryCommandParameters: new List<Tuple<string, OracleClient.OracleDbType, object>>() {
                            new Tuple<string, OracleClient.OracleDbType, object>("paramSelectTechId", OracleClient.OracleDbType.Varchar2, paramSelectTechId),
                            }
                        ).ExecuteSelect();

                    #endregion ASYNC SELECT DATA FROM DATABASE


                    return dataTableSource;
                }
            }
        }

        bool checkAdult(int age)
        {
            return age >= 18;
        }

        public class DateTime_
        {
            public static string Get_NumberWithZero_String(int value)
            {
                string result = value.ToString();
                if (result.Length < 2)
                {
                    return $"0{result}";
                } else
                {
                    return result;
                }
            }

            public static DateTime Get_Now_DateTime()
            {
                return DateTime.Now;
            }

            public static DateTime Get_PlusDayCount_DateTime(int dayCount, DateTime dateTime)
            {
                return dateTime.AddDays(dayCount);
            }

            public static DateTime Get_PlusMonthCount_DateTime(int monthCount, DateTime dateTime)
            {
                return dateTime.AddMonths(monthCount);
            }

            public static int Get_LastDayInSelectMonth_Int(DateTime dateTime)
            {
                return DateTime.DaysInMonth(dateTime.Year, dateTime.Month);
            }

            public static string Get_FormatOnlyTime_String(DateTime dateTime)
            {
                return $"{Utils.DateTime_.Get_NumberWithZero_String(value: dateTime.Hour)}:" +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Minute)}:" +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Second)}";
            }

            public static string Get_FormatOnlyDate_String(DateTime dateTime)
            {
                return $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Day)}." +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Month)}." +
                    $"{dateTime.Year}";
            }

            public static string Get_FormatOnlyDateTime_String(DateTime dateTime)
            {
                return $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Day)}." +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Month)}." +
                    $"{dateTime.Year} " +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(value: dateTime.Hour)}:" +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Minute)}:" +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Second)}";
            }

            public static int Get_NowShift_Int(DateTime? dateTime = null)
            {
                if (!dateTime.HasValue)
                {
                    dateTime = DateTime.Now;
                }
                if (dateTime.Value.Hour >= 20 || dateTime.Value.Hour < 8)
                {
                    return 1;
                } else
                {
                    return 2;
                }
            }

            public static string Get_FormatDateTime_String(DateTime dateTime, string format = "01.08.2022 23:59:59")
            {
                //DateTime myDate = DateTime.ParseExact("2022-08-09 11:37:14", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                //DateTime local_DateTime = DateTime.FromOADate(Convert.ToDouble(row["TIME"]));

                string result = "error";
                switch (format)
                {
                    case "01.08.2022 23:59:59":
                        {
                            result = $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Day)}.{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Month)}.{dateTime.Year} " +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Hour)}:{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Minute)}:{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Second)}";
                            break;
                        }
                    case "01.08.2022":
                        {
                            result = $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Day)}.{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Month)}.{dateTime.Year}";
                            break;
                        }
                    case "23:59:59":
                        {
                            result = $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Hour)}:{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Minute)}:{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Second)}";
                            break;
                        }
                    case "23:59":
                        {
                            result = $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Hour)}:{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Minute)}";
                            break;
                        }
                    default:
                        {
                            result = $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Day)}.{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Month)}.{dateTime.Year} " +
                    $"{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Hour)}:{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Minute)}:{Utils.DateTime_.Get_NumberWithZero_String(dateTime.Second)}";
                            break;
                        }
                }

                return result;
            }

            public static int Get_Difference_Milliseconds(DateTime dateTime1, DateTime dateTime2)
            {
                return (int)(dateTime1 - dateTime2).TotalMilliseconds;
            }
        }

        public class DataTable_
        {
            public static List<List<object>> Get_ConvertDataTable_List(DataTable dataTable, List<string> columns, List<List<object>> matrix = null)
            {
                List<List<object>> result;
                if (matrix is null)
                {
                    result = new List<List<object>>() { };
                } else
                {
                    result = matrix;
                }
                foreach (DataRow dataRow in dataTable.Rows)
                {
                    List<object> row = new List<object>();
                    foreach (var column in columns)
                    {
                        try
                        {
                            row.Add(dataRow[column]);
                        } catch
                        {
                            row.Add("$ошибка");
                        }
                    }
                    result.Add(row);
                }
                return result;
            }

            public static DataTable Get_ConvertList_DataTable(List<string> columns, List<List<object>> matrix)
            {
                // find rows count and cols count
                int height = matrix.Count;
                int widht = columns.Count;

                // create new table
                DataTable dataTableNew = new DataTable();

                // create headers
                for (int column = 0; column < widht; column += 1)
                {
                    dataTableNew.Columns.Add($"{columns[column]}", typeof(object));
                }

                // fill body
                for (int rowIndex = 0; rowIndex < height; rowIndex += 1)
                {
                    DataRow dataRowTemp = dataTableNew.NewRow();
                    for (int columnIndex = 0; columnIndex < widht; columnIndex += 1)
                    {
                        dataRowTemp[$"{dataTableNew.Columns[columnIndex].ColumnName}"] = matrix[rowIndex][columnIndex];
                    }
                    dataTableNew.Rows.Add(dataRowTemp);
                }

                return dataTableNew;
            }
        }

        public class Xrtl
        {
            public static DataTable Get_XrtlFill_DataTable(XrtlExplorer.IXrtlExplorer xrtl, DataTable table, string queryName)
            {
                DataTable dataTable = table.Clone();

                xrtl.OpenQuery(queryName);
                while (!xrtl.IsEof(queryName))
                {
                    DataRow row = dataTable.NewRow();

                    foreach (DataColumn col in dataTable.Columns)
                    {
                        try
                        {
                            object value = xrtl.GetFieldValue(queryName, col.ColumnName);
                            row[col.ColumnName] = value ?? DBNull.Value;
                        } catch (Exception e)
                        {
                            xrtl?.WriteLogMessage(e.Message, 0);
                        }
                    }
                    dataTable.Rows.Add(row);
                    xrtl.MoveNext(queryName);
                }
                xrtl.CloseQuery(queryName);

                return dataTable;
            }

            public static void Set_XrtlFill_DataTable(XrtlExplorer.IXrtlExplorer xrtl, DataTable table, string queryName)
            {
                xrtl.OpenQuery(queryName);
                while (!xrtl.IsEof(queryName))
                {
                    DataRow row = table.NewRow();

                    foreach (DataColumn col in table.Columns)
                    {
                        try
                        {
                            object value = xrtl.GetFieldValue(queryName, col.ColumnName);
                            row[col.ColumnName] = value ?? DBNull.Value;
                        } catch (Exception e)
                        {
                            xrtl?.WriteLogMessage(e.Message, 0);
                        }
                    }
                    table.Rows.Add(row);
                    xrtl.MoveNext(queryName);
                }
                xrtl.CloseQuery(queryName);
            }
        }

        public class WinForms
        {
            public partial class WinForms_Container : System.Windows.Forms.UserControl, XrtlExplorer.IXrtlControl
            {
                #region Код, автоматически созданный конструктором компонентов

                /// <summary> 
                /// Обязательная переменная конструктора.
                /// </summary>
                private System.ComponentModel.IContainer components = null;

                /// <summary> 
                /// Освободить все используемые ресурсы.
                /// </summary>
                /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
                protected override void Dispose(bool disposing)
                {
                    if (disposing && (components != null))
                    {
                        components.Dispose();
                    }
                    base.Dispose(disposing);
                }

                /// <summary> 
                /// Требуемый метод для поддержки конструктора — не изменяйте 
                /// содержимое этого метода с помощью редактора кода.
                /// </summary>
                private void InitializeComponent()
                {
                    this.SuspendLayout();
                    // 
                    // Bogdan_custom_WpfContainer
                    // 
                    this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                    this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                    this.BackColor = System.Drawing.Color.White;
                    this.Margin = new System.Windows.Forms.Padding(0);
                    this.MaximumSize = new System.Drawing.Size(3840, 2160);
                    this.MinimumSize = new System.Drawing.Size(640, 480);
                    this.Name = "Bogdan_custom_WpfContainer";
                    this.Size = new System.Drawing.Size(1280, 720);
                    this.Load += new System.EventHandler(this.Bogdan_custom_WpfContainer_Load);
                    this.ResumeLayout(false);

                }

                #endregion Код, автоматически созданный конструктором компонентов

                #region IXrtlControl Interface Realization

                public System.Xml.XmlDocument XrtlXmlFile
                {
                    get; set;
                } = new System.Xml.XmlDocument();
                public string XrtlAppBasePath
                {
                    get; set;
                } = string.Empty;
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
                    XrtlXmlFile.LoadXml(sXml);
                }
                public void SetXrtlInterface(IXrtlExplorer xrtlExplorer)
                {
                    System.Xml.XmlDocument XrtlXmlTempFile = new System.Xml.XmlDocument();
                    XrtlXmlTempFile.LoadXml(xrtlExplorer.GetSystemInfo());
                    string basePath = System.IO.Path.Combine(new string[]
                            {
                        System.IO.Directory.GetCurrentDirectory(),
                        XrtlXmlTempFile.SelectSingleNode("/SystemInfo/AppLocalDir").Attributes["value"].Value,
                        XrtlXmlTempFile.SelectSingleNode("/SystemInfo/AppSystem").Attributes["name"].Value,
                            }
                        );
                    this.XrtlAppBasePath = basePath;
                }

                #endregion IXrtlControl Interface Realization

                #region constructor

                public WinForms_Container()
                {
                    InitializeComponent();
                }

                #endregion constructor

                #region wrapper initialization

                public System.Windows.UIElement Report { get; set; } = new System.Windows.UIElement();

                private void Bogdan_custom_WpfContainer_Load(object sender, EventArgs e)
                {
                    #region wrapper

                    System.Windows.Forms.Integration.ElementHost host = new System.Windows.Forms.Integration.ElementHost() {
                        Dock = System.Windows.Forms.DockStyle.Fill,
                        TabIndex = 0,
                        Child = Report
                    };
                    this.Controls.Add(host);
                    this.Dock = System.Windows.Forms.DockStyle.Fill;
                    host.Dock = System.Windows.Forms.DockStyle.Fill;

                    #endregion wrapper
                }

                #endregion wrapper initialization

                #region utils methods

                public string GetTemplate(string templateName = "template1")
                {
                    return $"{XrtlAppBasePath}\\{XrtlXmlFile.SelectSingleNode("/Object").Attributes[templateName].Value}";
                }

                public string GetDebugTemplate(string templateName = "Отчёт производительности водителей экскаваторов.xlsx")
                {
                    return $"{System.IO.Directory.GetCurrentDirectory()}\\templates\\{templateName}";
                }

                #endregion utils methods

            }

            public static void SetBorder()
            {
                            _WinForms.Form form = new _WinForms.Form();  // form.MinimumSize;  // this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            int Height = 390;
            int Width = 270;
            form.Height = Height;
            form.Width = Width;

            form.Controls.Add(new _Reports.Bogdan_custom_Container_AuxMonitoringStoppages());
            form.Show();
            form.Activate();
            }

            public static string OpenFileDialog_SelectedFilePath(string titleDialog = "Укажите Excel файл для загрузки", string filterFiles = @"Excel файл|*.xlsx|Excel файл(устаревший)|*.xls|Excel файл(с макросами)|*.xlsm")
            {
                string fileExcelPath = "";
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "C:\\";
                    openFileDialog.Filter = filterFiles;
                    openFileDialog.FilterIndex = 1;
                    openFileDialog.RestoreDirectory = true;
                    openFileDialog.Title = titleDialog;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        fileExcelPath = openFileDialog.FileName;
                    }
                }
                return fileExcelPath;
            }
        }

        public class Wpf
        {
            public static void Set_DataGridUpdate_DataTable(System.Windows.Controls.DataGrid dataGrid, DataTable dataTable)
            {
                dataGrid.BeginInit();
                dataGrid.SetBinding(System.Windows.Controls.ItemsControl.ItemsSourceProperty, new System.Windows.Data.Binding {
                    Source = dataTable
                });
                dataGrid.Items.Refresh();
                dataGrid.EndInit();
            }

            public static void Set_ButtonText_String(System.Windows.Controls.Button button, string text)
            {
                button.Content = text;
            }

            public static DateTime Get_DataPicker_DateTime(System.Windows.Controls.DatePicker datePicker)
            {
                return datePicker.SelectedDate.Value;
            }

            public static void Set_DataPicker_DateTime(System.Windows.Controls.DatePicker datePicker, DateTime dateTime)
            {
                datePicker.SelectedDate = dateTime;
            }

            public static void Set_DataTimePicker_DateTime(WpfToolkit.DateTimePicker dateTimePicker, DateTime dateTime)
            {
                dateTimePicker.Value = dateTime;
            }

            public static string Get_Combobox_String(System.Windows.Controls.ComboBox comboBox)
            {
                return comboBox.Text;
            }

            public static int Get_Combobox_Int(System.Windows.Controls.ComboBox comboBox)
            {
                return int.Parse(comboBox.Text);
            }

            public static decimal Get_Combobox_Decimal(System.Windows.Controls.ComboBox comboBox)
            {
                return decimal.Parse(comboBox.Text);
            }

            public static void Set_Combobox_Text(System.Windows.Controls.ComboBox comboBox, string text)
            {
                comboBox.Items.Add(text);
            }

            public static void Set_Combobox_List(System.Windows.Controls.ComboBox comboBox, List<string> list)
            {
                foreach (string text in list)
                {
                    Utils.Wpf.Set_Combobox_Text(comboBox: comboBox, text: text);
                }
            }

            public static void Set_Visibility(System.Windows.UIElement element, bool visibility)
            {
                if (visibility)
                {
                    element.Visibility = System.Windows.Visibility.Visible;
                } else
                {
                    element.Visibility = System.Windows.Visibility.Hidden;
                }
            }

            public static void Set_BackgroundControl(_WinControls.Control element, byte alfa, byte red, byte green, byte blue)
            {
                element.Background = new SolidColorBrush(Color.FromArgb(a:alfa, r:red, g:green, b:blue));
            }

            public static void Set_ForegroundControl(_WinControls.Control element, byte alfa, byte red, byte green, byte blue)
            {
                element.Foreground = new SolidColorBrush(Color.FromArgb(a: alfa, r: red, g: green, b: blue));
            }

            public static void Set_BackgroundTextBlock(_WinControls.TextBlock element, byte alfa, byte red, byte green, byte blue)
            {
                element.Background = new SolidColorBrush(Color.FromArgb(a: alfa, r: red, g: green, b: blue));
            }
        }

        public class Excel_
        {
            #region variables

            public Excel.Application excelApplication;
            public Excel.Workbook workbook;
            public Excel.Worksheet worksheet;

            #endregion variables



            #region constructor

            public Excel_()
            {
                this.excelApplication = new Excel.Application();
                this.Method_Hide_Application(isVisible: false);
            }

            #endregion constructor



            public void Method_Hide_Application(bool isVisible)
            {
                excelApplication.DisplayAlerts = isVisible;
                excelApplication.Visible = isVisible;
            }

            public void Method_Load_Template(string templateName, string templateNameDebug, Utils.WinForms.WinForms_Container Xrtl_Container)
            {
                try
                {
                    this.workbook = excelApplication.Workbooks.Add(Xrtl_Container.GetTemplate(templateName: templateName));
                } catch
                {
                    this.workbook = excelApplication.Workbooks.Add(Xrtl_Container.GetDebugTemplate(templateName: templateNameDebug));
                }
                Excel.Worksheet templateWorksheet = workbook.Worksheets[1];
                templateWorksheet.Copy(templateWorksheet);
                this.worksheet = this.workbook.Worksheets[2];
                this.workbook.Worksheets[1].Delete();
                this.worksheet.Name = "Лист 1";
            }

            public void Method_Set_Cell_Value(int rowIndex, int colIndex, object value)
            {
                this.worksheet.Cells[rowIndex, colIndex].Value = value;
            }

            public void Method_Fill_Sheet_From_List(List<List<object>> matrix, int startRow = 1, int startCol = 1)
            {
                // find rows count and cols count
                int height = matrix.Count;
                int widht = matrix[0].Count;
                if (height > 0 && widht > 0)
                {
                    // convert List<List<object>> to object[,]
                    object[,] arr = new object[height, widht];
                    for (int rowIndex = 0; rowIndex < height; rowIndex += 1)
                    {
                        for (int columnIndex = 0; columnIndex < widht; columnIndex += 1)
                        {
                            arr[rowIndex, columnIndex] = matrix[rowIndex][columnIndex];
                        }
                    }

                    // set object[,] values to Excel.Range
                    this.worksheet.get_Range(
                            (Excel.Range)this.worksheet.Cells[startRow, startCol],
                            (Excel.Range)this.worksheet.Cells[startRow + height - 1, startCol + widht - 1]
                        ).Value = arr;
                } else
                {
                    Utils.Debug.Set_PrintToConsole(text: $"height or widht lower than 1 (FillExcelSheetFromList)", isNewLine: true);
                }
            }

            public static void FillExcelSheetFromDataTable(Excel.Worksheet worksheet, DataTable dataTable)
            {
                object[,] arr = new object[dataTable.Rows.Count, dataTable.Columns.Count];
                for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
                    {
                        arr[rowIndex, columnIndex] = dataTable.Rows[rowIndex][columnIndex];
                    }
                }
                worksheet.get_Range(
                        (Excel.Range)worksheet.Cells[1, 1],
                        (Excel.Range)worksheet.Cells[1 + dataTable.Rows.Count - 1, dataTable.Columns.Count]
                    ).Value = arr;
            }

            public static void FillExcelSheetFromList(Excel.Worksheet worksheet, List<List<object>> matrix, int startRow, int startCol)
            {
                try
                {
                    // find rows count and cols count
                    int height = matrix.Count;
                    int widht = matrix[0].Count;
                    if (height > 0 && widht > 0)
                    {
                        // convert List<List<object>> to object[,]
                        object[,] arr = new object[height, widht];
                        for (int rowIndex = 0; rowIndex < height; rowIndex += 1)
                        {
                            for (int columnIndex = 0; columnIndex < widht; columnIndex += 1)
                            {
                                arr[rowIndex, columnIndex] = matrix[rowIndex][columnIndex];
                            }
                        }

                        // set object[,] values to Excel.Range
                        worksheet.get_Range(
                                (Excel.Range)worksheet.Cells[startRow, startCol],
                                (Excel.Range)worksheet.Cells[startRow + height - 1, startCol + widht - 1]
                            ).Value = arr;
                    } else
                    {
                        Utils.Debug.Set_PrintToConsole(text: $"height or widht lower than 1 (FillExcelSheetFromList)", isNewLine: true);
                    }
                } catch (Exception error)
                {
                    Utils.Debug.Set_ExceptionPrintAndShow(exception: error, isShowScreenWindow: true);
                }
            }

            public static Excel.Worksheet FillWorksheetFromList(Excel.Worksheet worksheet, List<List<object>> matrix, int startRow = 1, int startColumn = 1)
            {
                int startRowValue = startRow;
                foreach (List<object> col in matrix)
                {
                    int startColumnValue = startColumn;
                    foreach (object cell in col)
                    {
                        worksheet.Cells[startRowValue, startColumnValue].Value = cell;
                        startColumnValue += 1;
                    }
                    startRowValue += 1;
                }
                return worksheet;
            }
        }

        public class Sql
        {
            #region variables

            readonly string connectionString;
            readonly string sqlExpression;
            readonly List<Tuple<string, OracleClient.OracleDbType, object>> queryParameters = new List<Tuple<string, OracleClient.OracleDbType, object>>() { };

            #endregion variables



            #region constructor

            public Sql(
                string sqlExpression,
                string connectionString = @"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=172.30.23.16)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=PITENEW))); User id=DISPATCHER; Password=disp",
                List<Tuple<string, OracleClient.OracleDbType, object>> queryCommandParameters = null)
            {
                this.sqlExpression = sqlExpression;

                this.connectionString = connectionString;

                if (!(queryCommandParameters is null))
                {
                    AddSqlCommandParameters(newQueryParameters: queryCommandParameters);
                }
            }

            #endregion constructor



            public void AddSqlCommandParameters(List<Tuple<string, OracleClient.OracleDbType, object>> newQueryParameters)
            {
                foreach (Tuple<string, OracleClient.OracleDbType, object> newQueryParameter in newQueryParameters)
                {
                    this.queryParameters.Add(newQueryParameter);
                }
            }

            public void AddSqlCommandParameter(Tuple<string, OracleClient.OracleDbType, object> newQueryParameter)
            {
                this.queryParameters.Add(newQueryParameter);
            }

            public async Task<DataTable> ExecuteSelectAsync()
            {
                OracleClient.OracleConnection oracleConnection = new OracleClient.OracleConnection();
                OracleClient.OracleCommand oracleCommand = new OracleClient.OracleCommand();
                OracleClient.OracleDataAdapter oracleDataAdapter = new OracleClient.OracleDataAdapter();
                try
                {
                    oracleConnection.ConnectionString = this.connectionString;
                    if (oracleConnection.State == ConnectionState.Closed)
                    {
                        oracleConnection.Open();
                    }

                    oracleCommand.Connection = oracleConnection;
                    oracleCommand.CommandType = CommandType.Text;
                    oracleCommand.CommandText = this.sqlExpression;

                    oracleCommand.BindByName = true;
                    foreach (Tuple<string, OracleClient.OracleDbType, object> parameter in this.queryParameters)
                    {
                        oracleCommand.Parameters.Add(parameter.Item1, parameter.Item2, parameter.Item3, ParameterDirection.Input);
                    }

                    oracleDataAdapter.SelectCommand = oracleCommand;

                    DataTable dataTable = new DataTable();
                    await Task.Run(() => oracleDataAdapter.Fill(dataTable));

                    return dataTable;
                } catch (Exception error)
                {
                    Utils.Debug.Set_ExceptionPrintAndShow(exception: error, isShowScreenWindow: true);

                    return new DataTable();
                } finally
                {
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                    oracleCommand.Dispose();
                    oracleDataAdapter.Dispose();
                }
            }

            public DataTable ExecuteSelect()
            {
                OracleClient.OracleConnection oracleConnection = new OracleClient.OracleConnection();
                OracleClient.OracleCommand oracleCommand = new OracleClient.OracleCommand();
                OracleClient.OracleDataAdapter oracleDataAdapter = new OracleClient.OracleDataAdapter();
                try
                {
                    oracleConnection.ConnectionString = this.connectionString;
                    if (oracleConnection.State == ConnectionState.Closed)
                    {
                        oracleConnection.Open();
                    }

                    oracleCommand.Connection = oracleConnection;
                    oracleCommand.CommandType = CommandType.Text;
                    oracleCommand.CommandText = this.sqlExpression;

                    oracleCommand.BindByName = true;
                    foreach (Tuple<string, OracleClient.OracleDbType, object> parameter in this.queryParameters)
                    {
                        oracleCommand.Parameters.Add(parameter.Item1, parameter.Item2, parameter.Item3, ParameterDirection.Input);
                    }

                    oracleDataAdapter.SelectCommand = oracleCommand;

                    DataTable dataTable = new DataTable();
                    oracleDataAdapter.Fill(dataTable);

                    return dataTable;
                } catch (Exception error)
                {
                    Utils.Debug.Set_ExceptionPrintAndShow(exception: error, isShowScreenWindow: true);

                    return new DataTable();
                } finally
                {
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                    oracleCommand.Dispose();
                    oracleDataAdapter.Dispose();
                }
            }

            public int ExecuteUpdateOrInsert()
            {
                OracleClient.OracleConnection oracleConnection = new OracleClient.OracleConnection();
                OracleClient.OracleCommand oracleCommand = new OracleClient.OracleCommand();
                try
                {
                    oracleConnection.ConnectionString = this.connectionString;
                    if (oracleConnection.State == ConnectionState.Closed)
                    {
                        oracleConnection.Open();
                    }

                    OracleClient.OracleTransaction transaction = oracleConnection.BeginTransaction();
                    try
                    {
                        oracleCommand.Connection = oracleConnection;
                        oracleCommand.Transaction = transaction;
                        oracleCommand.CommandType = CommandType.Text;
                        oracleCommand.CommandText = this.sqlExpression;

                        oracleCommand.BindByName = true;
                        foreach (Tuple<string, OracleClient.OracleDbType, object> parameter in this.queryParameters)
                        {
                            oracleCommand.Parameters.Add(parameter.Item1, parameter.Item2, parameter.Item3, ParameterDirection.Input);
                        }

                        int rowsAffected = oracleCommand.ExecuteNonQuery();

                        transaction.Commit();

                        return rowsAffected;
                    } catch (Exception error)
                    {
                        transaction.Rollback();

                        Utils.Debug.Set_ExceptionPrintAndShow(exception: error, isShowScreenWindow: true);

                        return -1;
                    } finally
                    {
                        transaction.Dispose();
                    }
                } catch (Exception error)
                {
                    Utils.Debug.Set_ExceptionPrintAndShow(exception: error, isShowScreenWindow: true);

                    return -1;
                } finally
                {
                    oracleCommand.Dispose();
                    oracleConnection.Close();
                    oracleConnection.Dispose();
                }
            }

            public static DataTable OracleSqlQueryExecute(string queryString, List<Tuple<string, OracleClient.OracleDbType, object>> queryParameters)
            {


                return new DataTable();
            }

            public static DataTable SqlOracleQuery(string queryString, List<Tuple<string, OracleClient.OracleDbType, object>> queryParameters)
            {
                OracleClient.OracleConnection connection = new OracleClient.OracleConnection("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=172.30.23.16)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=PITENEW))); User id=DISPATCHER; Password=disp");

                try
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    OracleClient.OracleCommand command = new OracleClient.OracleCommand(queryString, connection) {
                        BindByName = true
                    };
                    foreach (Tuple<string, OracleClient.OracleDbType, object> parameter in queryParameters)
                    {
                        command.Parameters.Add(parameter.Item1, parameter.Item2, parameter.Item3, ParameterDirection.Input);
                    }

                    OracleClient.OracleDataAdapter adapter = new OracleClient.OracleDataAdapter(command);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet);

                    return dataSet.Tables[0];
                } catch (Exception error)
                {
                    Utils.Debug.Set_ExceptionPrintAndShow(exception: error, isShowScreenWindow: true);

                    return new DataTable();
                } finally
                {
                    connection.Close();
                    connection.Dispose();
                }
            }

            public static int SqlOracleQueryExecute(string queryString, List<Tuple<string, OracleClient.OracleDbType, object>> queryParameters, string connectionString)
            {
                using (OracleClient.OracleConnection oracleConnection = new OracleClient.OracleConnection(connectionString: connectionString))
                {
                    OracleClient.OracleTransaction transaction = oracleConnection.BeginTransaction();
                    int rowsAffected = -1;
                    try
                    {
                        if (oracleConnection.State == ConnectionState.Closed)
                        {
                            oracleConnection.Open();
                        }
                        using (OracleClient.OracleCommand oracleCommand = new OracleClient.OracleCommand(cmdText: queryString, conn: oracleConnection) { Transaction = transaction })
                        {
                            //cmd.Parameters.AddWithValue("@Codigo", comboBox1.Text);
                            rowsAffected = oracleCommand.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    } catch (Exception error)
                    {
                        Utils.Debug.Set_ExceptionPrintAndShow(exception: error, isShowScreenWindow: true);
                    } finally
                    {
                        transaction.Rollback();
                    }
                    return rowsAffected;
                }
            }

            public static List<string> GetColNameListFromTableMethod(DataTable dataTableArg)
            {
                List<string> result = new List<string>();
                foreach (DataColumn col_i in dataTableArg.Columns)
                {
                    result.Add(col_i.ColumnName);
                }

                return result;
            }
        }
    }

    public class Queries
    {
        public static string Get_Bogdan_custom_OperPoroda_Truck()
        {
            return @"



WITH st AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      st.taskdate shiftdate,
                      st.shift    shiftnum
           FROM       shifttasks st
           inner join dispatcher.dumptrucks d
           ON         d.vehid = st.vehid
           AND        d.columnnum=1
           WHERE      ((
                                            st.taskdate = :paramDateFrom
                                 AND        st.shift >= :paramShiftFrom)
                      OR         (
                                            st.taskdate > :paramDateFrom))
           AND        ((
                                            st.taskdate = :paramDateTo
                                 AND        :paramShiftTo >= st.shift)
                      OR         (
                                            :paramDateTo > st.taskdate)) ), stpgs AS
(
       SELECT sel1.tech_key,
              sel1.vehid,
              sel1.shiftdate,
              sel1.shiftnum,
              sel1.timestop,
              sel1.timego,
              sel1.poly_stop_cat_name category
       FROM   (
                         SELECT     VEHIDTOCONTROLID(s.vehid) tech_key,
                                    s.vehid,
                                    s.shiftdate,
                                    s.shiftnum,
                                    GREATEST(s.timestop, GETPREDEFINEDTIMEFROM('за указанную смену', s.shiftnum,s.shiftdate)) timestop,
                                    LEAST(s.timego,GETPREDEFINEDTIMETO('за указанную смену', s.shiftnum,s.shiftdate))         timego,
                                    s.idlestoptype,
                                    psc.poly_stop_cat_name
                         FROM       dispatcher.shiftstoppages s
                         inner join dispatcher.poly_user_stoppages_dump ps
                         ON         ps.poly_stop_bindings_id = 23
                         AND        (
                                               ps.code = s.idlestoptype
                                    AND        ps.poly_stop_cat_id IS NOT NULL)
                         inner join dispatcher.poly_stop_categories psc
                         ON         psc.poly_stop_cat_id = ps.poly_stop_cat_id
                         WHERE      NVL(s.idlestoptype,0) NOT IN(0,1,67)
                         AND        (
                                               s.timego - s.timestop) * 24 * 60 >=4
                         AND        s.timestop IS NOT NULL
                         AND        s.timego IS NOT NULL
                         AND        psc.poly_stop_cat_name IS NOT NULL
                         AND        ((
                                                          shiftdate = :paramDateFrom
                                               AND        shiftnum >= :paramShiftFrom)
                                    OR         (
                                                          shiftdate > :paramDateFrom))
                         AND        ((
                                                          shiftdate = :paramDateTo
                                               AND        :paramShiftTo >= shiftnum)
                                    OR         (
                                                          :paramDateTo > shiftdate )) )sel1 ), stpgsemerg AS
(
         SELECT   sel1.tech_key,
                  sel1.vehid,
                  sel1.shiftdate,
                  sel1.shiftnum,
                  SUM((sel1.timego-sel1.timestop)*24) emergidle
         FROM     (
                             SELECT     VEHIDTOCONTROLID(s.vehid) tech_key,
                                        s.vehid,
                                        s.shiftdate,
                                        s.shiftnum,
                                        GREATEST(s.timestop, GETPREDEFINEDTIMEFROM('за указанную смену', s.shiftnum,s.shiftdate)) timestop,
                                        LEAST(s.timego,GETPREDEFINEDTIMETO('за указанную смену', s.shiftnum,s.shiftdate))         timego,
                                        s.idlestoptype,
                                        psc.poly_stop_cat_name
                             FROM       dispatcher.shiftstoppages s
                             inner join dispatcher.poly_user_stoppages_dump ps
                             ON         ps.poly_stop_bindings_id = 23
                             AND        (
                                                   ps.code = s.idlestoptype
                                        AND        ps.poly_stop_cat_id IS NOT NULL)
                             inner join dispatcher.poly_stop_categories psc
                             ON         psc.poly_stop_cat_id = ps.poly_stop_cat_id
                             inner join userstoppagetypes ust
                             ON         ust.code=ps.code
                             AND        ust.isrepair=1
                             WHERE      NVL(s.idlestoptype,0) NOT IN(0,1,67)
                             AND        (
                                                   s.timego - s.timestop) * 24 * 60 >=4
                             AND        s.timestop IS NOT NULL
                             AND        s.timego IS NOT NULL
                             AND        psc.poly_stop_cat_name IS NOT NULL
                             AND        ((
                                                              shiftdate = :paramDateFrom
                                                   AND        shiftnum >= :paramShiftFrom)
                                        OR         (
                                                              shiftdate > :paramDateFrom))
                             AND        ((
                                                              shiftdate = :paramDateTo
                                                   AND        :paramShiftTo >= shiftnum)
                                        OR         (
                                                              :paramDateTo > shiftdate )) )sel1
         GROUP BY sel1.tech_key,
                  sel1.vehid,
                  sel1.shiftdate,
                  sel1.shiftnum ), prgq AS
(
       SELECT VEHIDTOCONTROLID(t.vehid) tech_key,
              t.vehid,
              t.shiftdate,
              t.shiftnum,
              GREATEST(t.timebegin, GETPREDEFINEDTIMEFROM('за указанную смену',t.shiftnum,t.shiftdate)) timebegin,
              LEAST(t.timeend, GETPREDEFINEDTIMETO('за указанную смену', t.shiftnum,t.shiftdate))       timeend,
              NVL(t.duration,0)*24                                                                                      transdur
       FROM   transitions t
       WHERE  t.transtype IN (6,7)
       AND    t.length>0
       AND    t.duration>0
       AND    15>=t.duration*24*60
       AND    t.timebegin IS NOT NULL
       AND    t.timeend IS NOT NULL
       AND    ((
                            t.shiftdate = :paramDateFrom
                     AND    t.shiftnum >= :paramShiftFrom)
              OR     (
                            t.shiftdate > :paramDateFrom))
       AND    ((
                            t.shiftdate = :paramDateTo
                     AND    :paramShiftTo >= t.shiftnum)
              OR     (
                            :paramDateTo > t.shiftdate )) ), peregemerg AS
(
          SELECT    sel1.tech_key,
                    sel1.vehid,
                    sel1.shiftdate,
                    sel1.shiftnum,
                    'перегон на ремонт' category,
                    CASE
                              WHEN NVL(stpgsemerg.emergidle,0)=0 THEN 0
                              ELSE sel1.duration
                    END duration
          FROM      (
                             SELECT   tech_key,
                                      vehid,
                                      shiftdate,
                                      shiftnum,
                                      SUM(transdur) duration
                             FROM     prgq
                             GROUP BY tech_key,
                                      vehid,
                                      shiftdate,
                                      shiftnum)sel1
          left join stpgsemerg
          ON        stpgsemerg.tech_key=sel1.tech_key
          AND       stpgsemerg.shiftdate=sel1.shiftdate
          AND       stpgsemerg.shiftnum=sel1.shiftnum ), s AS
(
         SELECT   tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category,
                  SUM((timego-timestop)*24) TIME
         FROM     stpgs
         GROUP BY tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category
         UNION ALL
         SELECT   tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category,
                  SUM(duration) TIME
         FROM     peregemerg
         GROUP BY tech_key,
                  vehid,
                  shiftdate,
                  shiftnum,
                  category ), totalstop AS
(
         SELECT   tech_key,
                  shiftdate,
                  shiftnum,
                  SUM(TIME) total_idle
         FROM     s
         GROUP BY tech_key,
                  shiftdate,
                  shiftnum ), psub AS
(
       SELECT *
       FROM   s pivot (SUM(TIME) FOR category IN ( 'ТР' tr,
                                                  'Т1,Т2,Т3,Т4,Т5' service,
                                                  'КР' kr,
                                                  'Обед' dinner,
                                                  'Прием/передача смены' breaks,
                                                  'ЕТО' eto,
                                                  'заправка(ДТ,вода)' refuel,
                                                  'перегоны' relocation,
                                                  'личные нужды' pers_need,
                                                  'перемещение по блоку' move_block,
                                                  'ожидание погрузки' wait_load,
                                                  'ожидание разгрузки' wait_unload,
                                                  'планировка подъездов/разбивка блока' porch_plan,
                                                  'работа бульдозера' aux_work,
                                                  'чистка ковшей/кузовов' body_clean,
                                                  'ВЗРЫВНЫЕ РАБОТЫ' vr,
                                                  'ТЕХНИЧЕСКИЙ ПЕРЕРЫВ' techper,
                                                  'Климатические условия' weather,
                                                  'Рем.элек.оборуд.' electrical,
                                                  'ДВС' dvs,
                                                  'Трансмиссия' transmission,
                                                  'Ходовая часть' chassis,
                                                  'Навесное оборудование' hinge,
                                                  'ремонт а/ш' tires,
                                                  'Гидравлическая часть' hydraulic,
                                                  'перегон на ремонт' reloc_repair,
                                                  'Наладочные работы' adjustment,
                                                  'Аварийные прочие' emerg_others,
                                                  'отсутствие вспомогательной техники' aux_lack,
                                                  'Отсут.зап.частей' parts_lack,
                                                  'Прочие' others_reason,
                                                  'доливка масла/антифриза' topp_oil,
                                                  'Остановка контралирующими органами' reg_auth,
                                                  'Отсутствие диз.топлива' fuel_lack,
                                                  'Работа с маркшейдерами, отстой а/с.' surv_work,
                                                  'Работа с геологами, отстой а/с.' geo_work,
                                                  'Очистка ходовой базы' go_base,
                                                  'переэкскавация без отгрузки' excav_nounload,
                                                  'отсутствие оператора' staff_lack,
                                                  'разборка отказов' breakdown,
                                                  'Дренажные работы без отгрузки' drainage,
                                                  'заправка экскаватора' shov_refuel,
                                                  'Резерв (ремонт экскаватора)' reserve_shov,
                                                  'Организац.прочие' org_others,
                                                  'Отсутствие экипажа (без бригады)' crew_lack,
                                                  'резерв (без учета резерва ремонта экскаватора)' reserve_noshov,
                                                  'Отсут.фронта работ' work_lack )) ), wtcq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.trips, 0)      trips
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual) trips
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 201
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), wtc AS
(
       SELECT *
       FROM   wtcq pivot (SUM(trips) FOR category IN ( 'ПРС в контуре карьера' wtc_prs,
                                                      'вскрыша скальная' wtc_rockstrip,
                                                      'вскрыша рыхлая' wtc_loosestrip,
                                                      'вскрыша транзитная' wtc_transstrip,
                                                      'руда скальная' wtc_rockore,
                                                      'руда рыхлая' wtc_looseore,
                                                      'руда транзитная' wtc_transore,
                                                      'щебень' wtc_macadam,
                                                      'ВСП' wtc_iwt,
                                                      'ВКП' wtc_ipt,
                                                      'снег' wtc_snow,
                                                      'ПРС вне контура карьера' wtc_prsoutcont )) ), wrasq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length, 0)     length
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 202
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), wras AS
(
       SELECT *
       FROM   wrasq pivot (SUM(length) FOR category IN ( 'ПРС в контуре карьера' wras_prs,
                                                        'вскрыша скальная' wras_rockstrip,
                                                        'вскрыша рыхлая' wras_loosestrip,
                                                        'вскрыша транзитная' wras_transstrip,
                                                        'руда скальная' wras_rockore,
                                                        'руда рыхлая' wras_looseore,
                                                        'руда транзитная' wras_transore,
                                                        'щебень' wras_macadam,
                                                        'ВСП' wras_iwt,
                                                        'ВКП' wras_ipt,
                                                        'снег' wras_snow,
                                                        'ПРС вне контура карьера' wras_prsoutcont )) ), twt AS
(
         SELECT   tech_key,
                  shiftdate,
                  shiftnum,
                  12-SUM(total_idle) totalworktime
         FROM     totalstop
         GROUP BY tech_key,
                  shiftdate,
                  shiftnum ), wtbt AS
(
       SELECT sel.tech_key,
              sel.vehid,
              sel.shiftdate,
              sel.shiftnum, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_prs,0)/nvl(sel.wtsum,0)
              END                          * nvl(sel.totalworktime,0) ) wt_prs, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_rockstrip,0)/nvl(sel.wtsum,0)
              END                                * nvl(sel.totalworktime,0) ) wt_rockstrip, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_loosestrip,0)/nvl(sel.wtsum,0)
              END                                 * nvl(sel.totalworktime,0) ) wt_loosestrip, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_transstrip,0)/nvl(sel.wtsum,0)
              END                                 * nvl(sel.totalworktime,0) ) wt_transstrip, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_rockore,0)/nvl(sel.wtsum,0)
              END                              * nvl(sel.totalworktime,0) ) wt_rockore, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_looseore,0)/nvl(sel.wtsum,0)
              END                               * nvl(sel.totalworktime,0) ) wt_looseore, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_transore,0)/nvl(sel.wtsum,0)
              END                               * nvl(sel.totalworktime,0) ) wt_transore, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_macadam,0)/nvl(sel.wtsum,0)
              END                              * nvl(sel.totalworktime,0) ) wt_macadam, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_iwt,0)/nvl(sel.wtsum,0)
              END                          * nvl(sel.totalworktime,0) ) wt_iwt, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_ipt,0)/nvl(sel.wtsum,0)
              END                          * nvl(sel.totalworktime,0) ) wt_ipt, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_snow,0)/nvl(sel.wtsum,0)
              END                           * nvl(sel.totalworktime,0) ) wt_snow, (
              CASE
                     WHEN nvl(sel.wtsum,0)=0 THEN 0
                     ELSE nvl(sel.wt_prsoutcont,0)/nvl(sel.wtsum,0)
              END                                 * nvl(sel.totalworktime,0) ) wt_prsoutcont,
              sel.totalworktime
       FROM   (
                        SELECT    wtc.tech_key,
                                  wtc.vehid,
                                  wtc.shiftdate,
                                  wtc.shiftnum,
                                  wtc.wtc_prs                                                                                                                                                                                                        wtc_prs,
                                  wtc.wtc_rockstrip                                                                                                                                                                                                        wtc_rockstrip,
                                  wtc.wtc_loosestrip                                                                                                                                                                                                        wtc_loosestrip,
                                  wtc.wtc_transstrip                                                                                                                                                                                                        wtc_transstrip,
                                  wtc.wtc_rockore                                                                                                                                                                                                        wtc_rockore,
                                  wtc.wtc_looseore                                                                                                                                                                                                        wtc_looseore,
                                  wtc.wtc_transore                                                                                                                                                                                                        wtc_transore,
                                  wtc.wtc_macadam                                                                                                                                                                                                        wtc_macadam,
                                  wtc.wtc_iwt                                                                                                                                                                                                        wtc_iwt,
                                  wtc.wtc_ipt                                                                                                                                                                                                        wtc_ipt,
                                  wtc.wtc_snow                                                                                                                                                                                                        wtc_snow,
                                  wtc.wtc_prsoutcont                                                                                                                                                                                                        wtc_prsoutcont,
                                  wras.wras_prs                                                                                                                                                                                                        wras_prs,
                                  wras.wras_rockstrip                                                                                                                                                                                                        wras_rockstrip,
                                  wras.wras_loosestrip                                                                                                                                                                                                        wras_loosestrip,
                                  wras.wras_transstrip                                                                                                                                                                                                        wras_transstrip,
                                  wras.wras_rockore                                                                                                                                                                                                        wras_rockore,
                                  wras.wras_looseore                                                                                                                                                                                                        wras_looseore,
                                  wras.wras_transore                                                                                                                                                                                                        wras_transore,
                                  wras.wras_macadam                                                                                                                                                                                                        wras_macadam,
                                  wras.wras_iwt                                                                                                                                                                                                        wras_iwt,
                                  wras.wras_ipt                                                                                                                                                                                                        wras_ipt,
                                  wras.wras_snow                                                                                                                                                                                                        wras_snow,
                                  wras.wras_prsoutcont                                                                                                                                                                                                        wras_prsoutcont,
                                  twt.totalworktime                                                                                                                                                                                                        totalworktime,
                                  nvl(wtc.wtc_prs, 0)        * nvl(wras.wras_prs, 0)                                                                                                                                                                                                        wt_prs,
                                  nvl(wtc.wtc_rockstrip, 0)  * nvl(wras.wras_rockstrip, 0)                                                                                                                                                                                                        wt_rockstrip,
                                  nvl(wtc.wtc_loosestrip, 0) * nvl(wras.wras_loosestrip, 0)                                                                                                                                                                                                        wt_loosestrip,
                                  nvl(wtc.wtc_transstrip, 0) * nvl(wras.wras_transstrip, 0)                                                                                                                                                                                                        wt_transstrip,
                                  nvl(wtc.wtc_rockore, 0)    * nvl(wras.wras_rockore, 0)                                                                                                                                                                                                        wt_rockore,
                                  nvl(wtc.wtc_looseore, 0)   * nvl(wras.wras_looseore, 0)                                                                                                                                                                                                        wt_looseore,
                                  nvl(wtc.wtc_transore, 0)   * nvl(wras.wras_transore, 0)                                                                                                                                                                                                        wt_transore,
                                  nvl(wtc.wtc_macadam, 0)    * nvl(wras.wras_macadam, 0)                                                                                                                                                                                                        wt_macadam,
                                  nvl(wtc.wtc_iwt, 0)        * nvl(wras.wras_iwt, 0)                                                                                                                                                                                                        wt_iwt,
                                  nvl(wtc.wtc_ipt, 0)        * nvl(wras.wras_ipt, 0)                                                                                                                                                                                                        wt_ipt,
                                  nvl(wtc.wtc_snow, 0)       * nvl(wras.wras_snow, 0)                                                                                                                                                                                                        wt_snow,
                                  nvl(wtc.wtc_prsoutcont, 0) * nvl(wras.wras_prsoutcont, 0)                                                                                                                                                                                                        wt_prsoutcont,
                                  nvl(wtc.wtc_prs, 0)        * nvl(wras.wras_prs, 0)+ nvl(wtc.wtc_rockstrip, 0) * nvl(wras.wras_rockstrip, 0)+ nvl(wtc.wtc_loosestrip, 0) * nvl(wras.wras_loosestrip, 0)+ nvl(wtc.wtc_transstrip, 0) * nvl(wras.wras_transstrip, 0)+ nvl(wtc.wtc_rockore, 0) * nvl(wras.wras_rockore, 0)+ nvl(wtc.wtc_looseore, 0) * nvl(wras.wras_looseore, 0)+ nvl(wtc.wtc_transore, 0) * nvl(wras.wras_transore, 0)+ nvl(wtc.wtc_macadam, 0) * nvl(wras.wras_macadam, 0)+ nvl(wtc.wtc_iwt, 0) * nvl(wras.wras_iwt, 0)+ nvl(wtc.wtc_ipt, 0) * nvl(wras.wras_ipt, 0)+ nvl(wtc.wtc_snow, 0) * nvl(wras.wras_snow, 0)+ nvl(wtc.wtc_prsoutcont, 0) * nvl(wras.wras_prsoutcont, 0) wtsum
                        FROM      wtc
                        left join wras
                        ON        wtc.tech_key=wras.tech_key
                        AND       wtc.shiftdate=wras.shiftdate
                        AND       wtc.shiftnum=wras.shiftnum
                        left join twt
                        ON        wtc.tech_key=twt.tech_key
                        AND       wtc.shiftdate=twt.shiftdate
                        AND       wtc.shiftnum=twt.shiftnum )sel ), wt AS
(
       SELECT tech_key,
              vehid,
              shiftdate,
              shiftnum,
              nvl(wt_rockstrip, 0) + nvl(wt_rockore, 0)                         wt_rockgm,
              nvl(wt_prs, 0)       + nvl(wt_loosestrip, 0)+ nvl(wt_looseore, 0) wt_loosegm,
              nvl(wt_transstrip, 0)+ nvl(wt_transore, 0)                        wt_transgm,
              0                                                                 wt_equiptrans,
              nvl(wt_macadam, 0)                                                wt_macadam,
              nvl(wt_iwt, 0)                                                    wt_iwt,
              nvl(wt_ipt, 0)                                                    wt_ipt,
              nvl(wt_snow, 0)                                                   wt_snow,
              nvl(wt_prsoutcont, 0)                                             wt_prsoutcont
       FROM   wtbt ), p AS
(
       SELECT psub.tech_key,
              psub.vehid,
              psub.shiftdate,
              psub.shiftnum,
              psub.tr,
              psub.service,
              psub.kr,
              psub.dinner,
              psub.breaks,
              psub.eto,
              psub.refuel,
              0 relocation,
              psub.pers_need,
              psub.move_block,
              psub.wait_load,
              psub.wait_unload,
              psub.porch_plan,
              psub.aux_work,
              psub.body_clean,
              psub.vr,
              psub.techper,
              psub.weather,
              psub.electrical,
              psub.dvs,
              psub.transmission,
              psub.chassis,
              psub.hinge,
              psub.tires,
              psub.hydraulic,
              psub.reloc_repair,
              psub.adjustment,
              psub.emerg_others,
              psub.aux_lack,
              psub.parts_lack,
              psub.others_reason,
              psub.topp_oil,
              psub.reg_auth,
              psub.fuel_lack,
              psub.surv_work,
              psub.geo_work,
              psub.go_base,
              psub.excav_nounload,
              psub.staff_lack,
              psub.breakdown,
              psub.drainage,
              psub.shov_refuel,
              psub.reserve_shov,
              psub.org_others,
              psub.crew_lack,
              psub.reserve_noshov,
              psub.work_lack,
              nvl(psub.tr, 0)        + nvl(psub.service, 0)+ nvl(psub.kr, 0)+ nvl(psub.dinner, 0)+ nvl(psub.breaks, 0)+ nvl(psub.eto, 0)+ nvl(psub.refuel, 0)+ nvl(psub.pers_need, 0)+ nvl(psub.move_block, 0)+ nvl(psub.wait_load, 0)+ nvl(psub.wait_unload, 0)+ nvl(psub.porch_plan, 0)+ nvl(psub.aux_work, 0)+ nvl(psub.body_clean, 0)+ nvl(psub.vr, 0)+ nvl(psub.techper, 0)                                                                                                                                                                                                        regnorm,
              nvl(psub.tr, 0)        + nvl(psub.service, 0)+ nvl(psub.kr, 0)                                                                                                                                                                                                        itogplanrem,
              nvl(psub.dinner, 0)    + nvl(psub.breaks, 0)+ nvl(psub.eto, 0)+ nvl(psub.refuel, 0)+ nvl(psub.pers_need, 0)+ nvl(psub.move_block, 0)+ nvl(psub.wait_load, 0)+ nvl(psub.wait_unload, 0)+ nvl(psub.porch_plan, 0)+ nvl(psub.aux_work, 0)+ nvl(psub.body_clean, 0)+ nvl(psub.vr, 0)+ nvl(psub.techper, 0)                                                                                                                                                                                                        itogtechnol,
              nvl(psub.electrical, 0)+ nvl(psub.dvs, 0)+ nvl(psub.transmission, 0)+ nvl(psub.chassis, 0)+ nvl(psub.hinge, 0)+ nvl(psub.tires, 0)+ nvl(psub.hydraulic, 0)+ nvl(psub.reloc_repair, 0)+ nvl(psub.adjustment, 0)+ nvl(psub.emerg_others, 0)+ nvl(psub.parts_lack, 0)+ nvl(psub.others_reason, 0)+ nvl(psub.topp_oil, 0)                                                                                                                                                                                                        itogemerg,
              nvl(psub.reg_auth, 0)  + nvl(psub.fuel_lack, 0)+ nvl(psub.surv_work, 0)+ nvl(psub.geo_work, 0)+ nvl(psub.go_base, 0)+ nvl(psub.excav_nounload, 0)+ nvl(psub.staff_lack, 0)+ nvl(psub.breakdown, 0)+ nvl(psub.drainage, 0)+ nvl(psub.shov_refuel, 0)+ nvl(psub.reserve_shov, 0)+ nvl(psub.org_others, 0)+ nvl(psub.crew_lack, 0)+ nvl(psub.reserve_noshov, 0)+ nvl(psub.work_lack, 0)                                                                                                                                                                                                        itogorg,
              nvl(psub.dinner, 0)    + nvl(psub.breaks, 0)+ nvl(psub.eto, 0)+ nvl(psub.refuel, 0)+ nvl(psub.pers_need, 0)+ nvl(psub.move_block, 0)+ nvl(psub.wait_load, 0)+ nvl(psub.wait_unload, 0)+ nvl(psub.porch_plan, 0)+ nvl(psub.aux_work, 0)+ nvl(psub.body_clean, 0)+ nvl(psub.vr, 0)+ nvl(psub.techper, 0)+ nvl(psub.weather, 0)+ nvl(psub.reg_auth, 0)+ nvl(psub.fuel_lack, 0)+ nvl(psub.surv_work, 0)+ nvl(psub.geo_work, 0)+ nvl(psub.go_base, 0)+ nvl(psub.excav_nounload, 0)+ nvl(psub.staff_lack, 0)+ nvl(psub.breakdown, 0)+ nvl(psub.drainage, 0)+ nvl(psub.shov_refuel, 0)+ nvl(psub.reserve_shov, 0)+ nvl(psub.org_others, 0)+ nvl(psub.crew_lack, 0)+ nvl(psub.reserve_noshov, 0)+ nvl(psub.work_lack, 0) s_kio
       FROM   psub ), gmwq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.weight, 0)     weight
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual*selres.avweight)/1000 weight
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 3
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), gmw AS
(
       SELECT *
       FROM   gmwq pivot (SUM(weight) FOR category IN ( 'ПРС в контуре карьера' gmw_prs,
                                                       'вскрыша скальная' gmw_rockstrip,
                                                       'вскрыша рыхлая' gmw_loosestrip,
                                                       'вскрыша транзитная' gmw_transstrip,
                                                       'руда скальная' gmw_rockore,
                                                       'руда рыхлая' gmw_looseore,
                                                       'руда транзитная' gmw_transore )) ), gmvq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.vol, 0)        vol
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(decode(nvl(selres.avweight, 0),
                                                       0, 0,
                                                       selres.avweight*selres.tripnumbermanual/nvl(decode(selres.weightrate,
                                                                                                          0, selres.avweight,
                                                                                                          selres.weightrate), selres.avweight)*selres.volumerate))/1000 vol
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight,
                                                                  sra.weightrate,
                                                                  sra.volumerate
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 3
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), gmv AS
(
       SELECT *
       FROM   gmvq pivot (SUM(vol) FOR category IN ( 'ПРС в контуре карьера' gmv_prs,
                                                    'вскрыша скальная' gmv_rockstrip,
                                                    'вскрыша рыхлая' gmv_loosestrip,
                                                    'вскрыша транзитная' gmv_transstrip,
                                                    'руда скальная' gmv_rockore,
                                                    'руда рыхлая' gmv_looseore,
                                                    'руда транзитная' gmv_transore )) ), trq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.trips, 0)      trips
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual) trips
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 4
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), tr AS
(
       SELECT *
       FROM   trq pivot (SUM(trips) FOR category IN ( 'ПРС в контуре карьера' tr_prs,
                                                     'вскрыша скальная' tr_rockstrip,
                                                     'вскрыша рыхлая' tr_loosestrip,
                                                     'вскрыша транзитная' tr_transstrip,
                                                     'по руде' tr_ore )) ), rasgmq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length, 0)     length
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 61
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), rasgm AS
(
       SELECT *
       FROM   rasgmq pivot (SUM(length) FOR category IN ( 'ср.взв.расст.г.м.' ras_gm )) ), rasq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length, 0)     length
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 5
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), ras AS
(
       SELECT *
       FROM   rasq pivot (SUM(length) FOR category IN ( 'ПРС в контуре карьера' ras_prs,
                                                       'вскрыша скальная' ras_rockstrip,
                                                       'вскрыша рыхлая' ras_loosestrip,
                                                       'вскрыша транзитная' ras_transstrip,
                                                       'руда скальная' ras_rockore,
                                                       'руда рыхлая' ras_looseore,
                                                       'руда транзитная' ras_transore )) ), avwq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.avweight, 0)   avweight
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avweight)/SUM(selres.tripnumbermanual)) avweight
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 6
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), avw AS
(
       SELECT *
       FROM   avwq pivot (SUM(avweight) FOR category IN ( 'ПРС в контуре карьера' avw_prs,
                                                         'по скале' avw_rock,
                                                         'по рыхлой' avw_loose,
                                                         'по транзитной' avw_trans )) ), avwgmq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.avweight, 0)   avweight
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0, 0,
                                                   SUM(selres.tripnumbermanual*selres.avweight)/SUM(selres.tripnumbermanual)) avweight
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 41
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), avwgm AS
(
       SELECT *
       FROM   avwgmq pivot (SUM(avweight) FOR category IN ( 'ср.взв.загр.г.м.' avw_gm )) ), ftq AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.gruzob, 0)     gruzob
           FROM       (
                                 SELECT     selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            SUM(selres.tripnumbermanual*selres.avweight)/1000 * decode(SUM(selres.tripnumbermanual),
                                                                                                       0, 0,
                                                                                                       SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) gruzob
                                 FROM       (
                                                       SELECT     sra.vehid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 21
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.vehid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), ft AS
(
       SELECT *
       FROM   ftq pivot (SUM(gruzob) FOR category IN ( 'ПРС в контуре карьера' ft_prs,
                                                      'вскрыша скальная' ft_rockstrip,
                                                      'вскрыша рыхлая' ft_loosestrip,
                                                      'вскрыша транзитная' ft_transstrip,
                                                      'руда скальная' ft_rockore,
                                                      'руда рыхлая' ft_looseore,
                                                      'руда транзитная' ft_transore )) ), dtmh AS
(
           SELECT     d.controlid tech_key,
                      d.vehid,
                      nvl(sel.mh, 0) mh
           FROM       (
                               SELECT   selres.vehid,
                                        SUM(selres.motohoursend-selres.motohoursbegin) mh
                               FROM     (
                                               SELECT vehid,
                                                      taskdate,
                                                      shift,
                                                      motohoursbegin,
                                                      motohoursend
                                               FROM   shiftlensandtimes
                                               WHERE  ((
                                                                    taskdate = :paramDateFrom
                                                             AND    shift >= :paramShiftFrom)
                                                      OR     (
                                                                    taskdate > :paramDateFrom))
                                               AND    ((
                                                                    taskdate = :paramDateTo
                                                             AND    :paramShiftTo >= shift)
                                                      OR     (
                                                                    :paramDateTo > taskdate )) )selres
                               GROUP BY selres.vehid)sel
           inner join dumptrucks d
           ON         d.vehid = sel.vehid
           AND        d.columnnum=1 ), norep AS
(
       SELECT vehidtocontrolid(vehid) tech_key,
              vehid,
              taskdate shiftdate,
              shift    shiftnum
       FROM   kgp_noreptech
       WHERE  ((
                            taskdate = :paramDateFrom
                     AND    shift >= :paramShiftFrom)
              OR     (
                            taskdate > :paramDateFrom))
       AND    ((
                            taskdate = :paramDateTo
                     AND    :paramShiftTo >= shift)
              OR     (
                            :paramDateTo > taskdate )) )
SELECT    'Автосамосвалы' category,
          smsv.model                   model,
          position,
          tech_id,
          /* вскрыша транзитная м3 */
          round(SUM((gmv_transstrip)) * 1000, 3)                                                  AS val_tr,
          round(nvl(SUM(ras_transstrip * gmv_transstrip) / nullif(SUM(gmv_transstrip), 0), 0), 3) AS len_tr,
          /* вскрыша транзитная км */
          /* вскрыша скальная м3 */
          round(SUM((gmv_rockstrip)) * 1000, 3)                                                AS val_sk,
          round(nvl(SUM(ras_rockstrip * gmv_rockstrip) / nullif(SUM(gmv_rockstrip), 0), 0), 3) AS len_sk,
          /* вскрыша скальная км */
          /* вскрыша рыхлая м3 */
          round(SUM((gmv_loosestrip)) * 1000, 3)                                                  AS val_rih,
          round(nvl(SUM(ras_loosestrip * gmv_loosestrip) / nullif(SUM(gmv_loosestrip), 0), 0), 3) AS len_rih,
          /* вскрыша рыхлая км */
          /* ПРС м3 */
          round(SUM((gmv_prs)) * 1000, 3)                                    AS val_prs,
          round(nvl(SUM(ras_prs * gmv_prs) / nullif(SUM(gmv_prs), 0), 0), 3) AS len_prs,
          /* ПРС км */
          /* руда м3 */
          round(SUM(gmv_rockore + gmv_looseore + gmv_transore) * 1000, 3)                                                                                                                                                                                                        AS val_rud,
          round( nvl((SUM(gmv_rockore) * nvl(SUM(ras_rockore * gmv_rockore) / nullif(SUM(gmv_rockore), 0), 0) + SUM(gmv_looseore) * nvl(SUM(ras_looseore * gmv_looseore) / nullif(SUM(gmv_looseore), 0), 0) + SUM(gmv_transore) * nvl(SUM(ras_transore * gmv_transore) / nullif(SUM(gmv_transore), 0), 0)) / nullif(SUM(gmv_rockore + gmv_looseore + gmv_transore), 0), 0), 3) AS len_rud,
          /* руда км */
          /* transore м3 */
          round(SUM((gmv_transore)) * 1000, 3)                                              AS val_transore,
          round(nvl(SUM(ras_transore * gmv_transore) / nullif(SUM(gmv_transore), 0), 0), 3) AS len_transore,
          /* transore км */
          /* rockore м3 */
          round(SUM((gmv_rockore)) * 1000, 3)                                            AS val_rockore,
          round(nvl(SUM(ras_rockore * gmv_rockore) / nullif(SUM(gmv_rockore), 0), 0), 3) AS len_rockore,
          /* rockore км */
          /* looseore м3 */
          round(SUM((gmv_looseore)) * 1000, 3)                                              AS val_looseore,
          round(nvl(SUM(ras_looseore * gmv_looseore) / nullif(SUM(gmv_looseore), 0), 0), 3) AS len_looseore
          /* looseore км */
FROM      (
                     SELECT     pt.position,
                                t.vehid tech_id,
                                st.shiftdate,
                                st.shiftnum,
                                nvl(dtmh.mh, 0) mh,
                                CASE
                                           WHEN nvl(norep.tech_key, 0) = 0 THEN 12
                                           ELSE NULL
                                END                                                                                                                                                                                                        kalentime,
                                nvl(wt.wt_rockgm, 0) + nvl(wt.wt_loosegm, 0) + nvl(wt.wt_transgm, 0) + nvl(wt.wt_equiptrans, 0) + nvl(wt.wt_macadam, 0) + nvl(wt.wt_iwt, 0) + nvl(wt.wt_ipt, 0) + nvl(wt.wt_snow, 0) + nvl(wt.wt_prsoutcont, 0) + nvl(p.dinner, 0) + nvl(p.breaks, 0) + nvl(p.eto, 0) + nvl(p.refuel, 0) + nvl(p.relocation, 0) + nvl(p.pers_need, 0) + nvl(p.move_block, 0) + nvl(p.wait_load, 0) + nvl(p.wait_unload, 0) + nvl(p.porch_plan, 0) + nvl(p.aux_work, 0) + nvl(p.body_clean, 0) + nvl(p.vr, 0) + nvl(p.techper, 0) chvhoz,
                                nvl(wt.wt_rockgm, 0) + nvl(wt.wt_loosegm, 0) + nvl(wt.wt_transgm, 0) + nvl(wt.wt_equiptrans, 0) + nvl(wt.wt_macadam, 0) + nvl(wt.wt_iwt, 0) + nvl(wt.wt_ipt, 0) + nvl(wt.wt_snow, 0) + nvl(wt.wt_prsoutcont, 0)                                                                                                                                                                                                        wt_all,
                                nvl(wt.wt_rockgm, 0) + nvl(wt.wt_loosegm, 0) + nvl(wt.wt_transgm, 0)                                                                                                                                                                                                        wt_gm,
                                nvl(wt.wt_rockgm, 0)                                                                                                                                                                                                        wt_rockgm,
                                nvl(wt.wt_loosegm, 0)                                                                                                                                                                                                        wt_loosegm,
                                nvl(wt.wt_transgm, 0)                                                                                                                                                                                                        wt_transgm,
                                nvl(wt.wt_equiptrans, 0) + nvl(wt.wt_macadam, 0) + nvl(wt.wt_iwt, 0) + nvl(wt.wt_ipt, 0) + nvl(wt.wt_snow, 0) + nvl(wt.wt_prsoutcont, 0)                                                                                                                                                                                                        wt_ew,
                                nvl(wt.wt_equiptrans, 0)                                                                                                                                                                                                        wt_equiptrans,
                                nvl(wt.wt_macadam, 0)                                                                                                                                                                                                        wt_macadam,
                                nvl(wt.wt_iwt, 0)                                                                                                                                                                                                        wt_iwt,
                                nvl(wt.wt_ipt, 0)                                                                                                                                                                                                        wt_ipt,
                                nvl(wt.wt_snow, 0)                                                                                                                                                                                                        wt_snow,
                                nvl(wt.wt_prsoutcont, 0)                                                                                                                                                                                                        wt_prsoutcont,
                                nvl(p.regnorm, 0)                                                                                                                                                                                                        regnorm,
                                nvl(p.itogplanrem, 0)                                                                                                                                                                                                        itogplanrem,
                                nvl(p.tr, 0)                                                                                                                                                                                                        tr,
                                nvl(p.service, 0)                                                                                                                                                                                                        service,
                                nvl(p.kr, 0)                                                                                                                                                                                                        kr,
                                nvl(p.itogtechnol, 0)                                                                                                                                                                                                        itogtechnol,
                                nvl(p.dinner, 0)                                                                                                                                                                                                        dinner,
                                nvl(p.breaks, 0)                                                                                                                                                                                                        breaks,
                                nvl(p.eto, 0)                                                                                                                                                                                                        eto,
                                nvl(p.refuel, 0)                                                                                                                                                                                                        refuel,
                                nvl(p.relocation, 0)                                                                                                                                                                                                        relocation,
                                nvl(p.pers_need, 0)                                                                                                                                                                                                        pers_need,
                                nvl(p.move_block, 0)                                                                                                                                                                                                        move_block,
                                nvl(p.wait_load, 0)                                                                                                                                                                                                        wait_load,
                                nvl(p.wait_unload, 0)                                                                                                                                                                                                        wait_unload,
                                nvl(p.porch_plan, 0)                                                                                                                                                                                                        porch_plan,
                                nvl(p.aux_work, 0)                                                                                                                                                                                                        aux_work,
                                nvl(p.body_clean, 0)                                                                                                                                                                                                        body_clean,
                                nvl(p.vr, 0)                                                                                                                                                                                                        vr,
                                nvl(p.techper, 0)                                                                                                                                                                                                        techper,
                                nvl(p.weather, 0)                                                                                                                                                                                                        weather,
                                nvl(p.itogemerg, 0)                                                                                                                                                                                                        itogemerg,
                                nvl(p.electrical, 0)                                                                                                                                                                                                        electrical,
                                nvl(p.dvs, 0)                                                                                                                                                                                                        dvs,
                                nvl(p.transmission, 0)                                                                                                                                                                                                        transmission,
                                nvl(p.chassis, 0)                                                                                                                                                                                                        chassis,
                                nvl(p.hinge, 0)                                                                                                                                                                                                        hinge,
                                nvl(p.tires, 0)                                                                                                                                                                                                        tires,
                                nvl(p.hydraulic, 0)                                                                                                                                                                                                        hydraulic,
                                nvl(p.reloc_repair, 0)                                                                                                                                                                                                        reloc_repair,
                                nvl(p.adjustment, 0)                                                                                                                                                                                                        adjustment,
                                nvl(p.emerg_others, 0)                                                                                                                                                                                                        emerg_others,
                                nvl(p.aux_lack, 0)                                                                                                                                                                                                        aux_lack,
                                nvl(p.parts_lack, 0)                                                                                                                                                                                                        parts_lack,
                                nvl(p.others_reason, 0)                                                                                                                                                                                                        others_reason,
                                nvl(p.topp_oil, 0)                                                                                                                                                                                                        topp_oil,
                                nvl(p.itogorg, 0)                                                                                                                                                                                                        itogorg,
                                nvl(p.reg_auth, 0)                                                                                                                                                                                                        reg_auth,
                                nvl(p.fuel_lack, 0)                                                                                                                                                                                                        fuel_lack,
                                nvl(p.surv_work, 0)                                                                                                                                                                                                        surv_work,
                                nvl(p.geo_work, 0)                                                                                                                                                                                                        geo_work,
                                nvl(p.go_base, 0)                                                                                                                                                                                                        go_base,
                                nvl(p.excav_nounload, 0)                                                                                                                                                                                                        excav_nounload,
                                nvl(p.staff_lack, 0)                                                                                                                                                                                                        staff_lack,
                                nvl(p.breakdown, 0)                                                                                                                                                                                                        breakdown,
                                nvl(p.drainage, 0)                                                                                                                                                                                                        drainage,
                                nvl(p.shov_refuel, 0)                                                                                                                                                                                                        shov_refuel,
                                nvl(p.reserve_shov, 0)                                                                                                                                                                                                        reserve_shov,
                                nvl(p.org_others, 0)                                                                                                                                                                                                        org_others,
                                nvl(p.crew_lack, 0)                                                                                                                                                                                                        crew_lack,
                                nvl(p.reserve_noshov, 0)                                                                                                                                                                                                        reserve_noshov,
                                nvl(p.work_lack, 0)                                                                                                                                                                                                        work_lack,
                                nvl(p.s_kio, 0)                                                                                                                                                                                                        s_kio,
                                nvl(gmv.gmv_prs, 0) + nvl(gmv.gmv_rockstrip, 0) + nvl(gmv.gmv_loosestrip, 0) + nvl(gmv.gmv_transstrip, 0) + nvl(gmv.gmv_rockore, 0) + nvl(gmv.gmv_looseore, 0) + nvl(gmv.gmv_transore, 0)                                                                                                                                                                                                        gmv_gm,
                                nvl(gmw.gmw_prs, 0) + nvl(gmw.gmw_rockstrip, 0) + nvl(gmw.gmw_loosestrip, 0) + nvl(gmw.gmw_transstrip, 0) + nvl(gmw.gmw_rockore, 0) + nvl(gmw.gmw_looseore, 0) + nvl(gmw.gmw_transore, 0)                                                                                                                                                                                                        gmw_gm,
                                nvl(gmv.gmv_prs, 0)                                                                                                                                                                                                        gmv_prs,
                                nvl(gmw.gmw_prs, 0)                                                                                                                                                                                                        gmw_prs,
                                nvl(gmv.gmv_rockstrip, 0)                                                                                                                                                                                                        gmv_rockstrip,
                                nvl(gmw.gmw_rockstrip, 0)                                                                                                                                                                                                        gmw_rockstrip,
                                nvl(gmv.gmv_loosestrip, 0)                                                                                                                                                                                                        gmv_loosestrip,
                                nvl(gmw.gmw_loosestrip, 0)                                                                                                                                                                                                        gmw_loosestrip,
                                nvl(gmv.gmv_transstrip, 0)                                                                                                                                                                                                        gmv_transstrip,
                                nvl(gmw.gmw_transstrip, 0)                                                                                                                                                                                                        gmw_transstrip,
                                nvl(gmv.gmv_rockore, 0)                                                                                                                                                                                                        gmv_rockore,
                                nvl(gmw.gmw_rockore, 0)                                                                                                                                                                                                        gmw_rockore,
                                nvl(gmv.gmv_looseore, 0)                                                                                                                                                                                                        gmv_looseore,
                                nvl(gmw.gmw_looseore, 0)                                                                                                                                                                                                        gmw_looseore,
                                nvl(gmv.gmv_transore, 0)                                                                                                                                                                                                        gmv_transore,
                                nvl(gmw.gmw_transore, 0)                                                                                                                                                                                                        gmw_transore,
                                CASE
                                           WHEN nvl(wt.wt_rockgm, 0) = 0 THEN 0
                                           ELSE(nvl(gmv.gmv_rockstrip, 0) + nvl(gmv.gmv_rockore, 0)) / nvl(wt.wt_rockgm, 0)
                                END opv_rockgm,
                                CASE
                                           WHEN nvl(wt.wt_rockgm, 0) = 0 THEN 0
                                           ELSE(nvl(gmw.gmw_rockstrip, 0) + nvl(gmw.gmw_rockore, 0)) / nvl(wt.wt_rockgm, 0)
                                END opw_rockgm,
                                CASE
                                           WHEN nvl(wt.wt_loosegm, 0) = 0 THEN 0
                                           ELSE(nvl(gmv.gmv_loosestrip, 0) + nvl(gmv.gmv_looseore, 0)) / nvl(wt.wt_loosegm, 0)
                                END opv_loosegm,
                                CASE
                                           WHEN nvl(wt.wt_loosegm, 0) = 0 THEN 0
                                           ELSE(nvl(gmw.gmw_loosestrip, 0) + nvl(gmw.gmw_looseore, 0)) / nvl(wt.wt_loosegm, 0)
                                END opw_loosegm,
                                CASE
                                           WHEN nvl(wt.wt_transgm, 0) = 0 THEN 0
                                           ELSE(nvl(gmv.gmv_transstrip, 0) + nvl(gmv.gmv_transore, 0)) / nvl(wt.wt_transgm, 0)
                                END opv_transgm,
                                CASE
                                           WHEN nvl(wt.wt_transgm, 0) = 0 THEN 0
                                           ELSE(nvl(gmw.gmw_transstrip, 0) + nvl(gmw.gmw_transore, 0)) / nvl(wt.wt_transgm, 0)
                                END opw_transgm,
                                CASE
                                           WHEN(
                                                                 nvl(gmv.gmv_prs, 0) + nvl(gmv.gmv_rockstrip, 0) + nvl(gmv.gmv_loosestrip, 0) + nvl(gmv.gmv_transstrip, 0) + nvl(gmv.gmv_rockore, 0) + nvl(gmv.gmv_looseore, 0) + nvl(gmv.gmv_transore, 0)) = 0 THEN 0
                                           ELSE((nvl(gmv.gmv_rockstrip, 0) + nvl(gmv.gmv_rockore, 0)) *
                                                      CASE
                                                                 WHEN nvl(wt.wt_rockgm, 0) = 0 THEN 0
                                                                 ELSE(nvl(gmv.gmv_rockstrip, 0) + nvl(gmv.gmv_rockore, 0)) / nvl(wt.wt_rockgm, 0)
                                                      END + (nvl(gmv.gmv_loosestrip, 0) + nvl(gmv.gmv_looseore, 0)) *
                                                      CASE
                                                                 WHEN nvl(wt.wt_loosegm, 0) = 0 THEN 0
                                                                 ELSE(nvl(gmv.gmv_loosestrip, 0) + nvl(gmv.gmv_looseore, 0)) / nvl(wt.wt_loosegm, 0)
                                                      END + (nvl(gmv.gmv_transstrip, 0) + nvl(gmv.gmv_transore, 0)) *
                                                      CASE
                                                                 WHEN nvl(wt.wt_transgm, 0) = 0 THEN 0
                                                                 ELSE(nvl(gmv.gmv_transstrip, 0) + nvl(gmv.gmv_transore, 0)) / nvl(wt.wt_transgm, 0)
                                                      END) / (nvl(gmv.gmv_prs, 0) + nvl(gmv.gmv_rockstrip, 0) + nvl(gmv.gmv_loosestrip, 0) + nvl(gmv.gmv_transstrip, 0) + nvl(gmv.gmv_rockore, 0) + nvl(gmv.gmv_looseore, 0) + nvl(gmv.gmv_transore, 0))
                                END avv_prod,
                                CASE
                                           WHEN(
                                                                 nvl(gmw.gmw_prs, 0) + nvl(gmw.gmw_rockstrip, 0) + nvl(gmw.gmw_loosestrip, 0) + nvl(gmw.gmw_transstrip, 0) + nvl(gmw.gmw_rockore, 0) + nvl(gmw.gmw_looseore, 0) + nvl(gmw.gmw_transore, 0)) = 0 THEN 0
                                           ELSE((nvl(gmw.gmw_rockstrip, 0) + nvl(gmw.gmw_rockore, 0)) *
                                                      CASE
                                                                 WHEN nvl(wt.wt_rockgm, 0) = 0 THEN 0
                                                                 ELSE(nvl(gmw.gmw_rockstrip, 0) + nvl(gmw.gmw_rockore, 0)) / nvl(wt.wt_rockgm, 0)
                                                      END + (nvl(gmw.gmw_loosestrip, 0) + nvl(gmw.gmw_looseore, 0)) *
                                                      CASE
                                                                 WHEN nvl(wt.wt_loosegm, 0) = 0 THEN 0
                                                                 ELSE(nvl(gmw.gmw_loosestrip, 0) + nvl(gmw.gmw_looseore, 0)) / nvl(wt.wt_loosegm, 0)
                                                      END + (nvl(gmw.gmw_transstrip, 0) + nvl(gmw.gmw_transore, 0)) *
                                                      CASE
                                                                 WHEN nvl(wt.wt_transgm, 0) = 0 THEN 0
                                                                 ELSE(nvl(gmw.gmw_transstrip, 0) + nvl(gmw.gmw_transore, 0)) / nvl(wt.wt_transgm, 0)
                                                      END) / (nvl(gmw.gmw_prs, 0) + nvl(gmw.gmw_rockstrip, 0) + nvl(gmw.gmw_loosestrip, 0) + nvl(gmw.gmw_transstrip, 0) + nvl(gmw.gmw_rockore, 0) + nvl(gmw.gmw_looseore, 0) + nvl(gmw.gmw_transore, 0))
                                END                                                                                                                   avw_prod,
                                nvl(tr.tr_prs, 0) + nvl(tr.tr_rockstrip, 0) + nvl(tr.tr_loosestrip, 0) + nvl(tr.tr_transstrip, 0) + nvl(tr.tr_ore, 0) tr_gm,
                                nvl(tr.tr_prs, 0)                                                                                                     tr_prs,
                                nvl(tr.tr_rockstrip, 0)                                                                                               tr_rockstrip,
                                nvl(tr.tr_loosestrip, 0)                                                                                              tr_loosestrip,
                                nvl(tr.tr_transstrip, 0)                                                                                              tr_transstrip,
                                nvl(tr.tr_ore, 0)                                                                                                     tr_ore,
                                nvl(rasgm.ras_gm, 0)                                                                                                  ras_gm,
                                nvl(ras.ras_prs, 0)                                                                                                   ras_prs,
                                nvl(ras.ras_rockstrip, 0)                                                                                             ras_rockstrip,
                                nvl(ras.ras_loosestrip, 0)                                                                                            ras_loosestrip,
                                nvl(ras.ras_transstrip, 0)                                                                                            ras_transstrip,
                                nvl(ras.ras_rockore, 0)                                                                                               ras_rockore,
                                nvl(ras.ras_looseore, 0)                                                                                              ras_looseore,
                                nvl(ras.ras_transore, 0)                                                                                              ras_transore,
                                CASE
                                           WHEN nvl(norep.tech_key, 0) = 0 THEN
                                                      CASE
                                                                 WHEN(
                                                                                       12 - nvl(p.breaks, 0) - nvl(p.eto, 0) - nvl(p.dinner, 0)) = 0 THEN NULL
                                                                 ELSE((nvl(wt.wt_rockgm, 0) + nvl(wt.wt_loosegm, 0) + nvl(wt.wt_transgm, 0) + nvl(wt.wt_equiptrans, 0) + nvl(wt.wt_macadam, 0) + nvl(wt.wt_iwt, 0) + nvl(wt.wt_ipt, 0) + nvl(wt.wt_snow, 0) + nvl(wt.wt_prsoutcont, 0)) + nvl(p.work_lack, 0) + nvl(p.itogorg, 0) + nvl(p.weather, 0)) / (12 - nvl(p.breaks, 0) - nvl(p.eto, 0) - nvl(p.dinner, 0))
                                                      END
                                           ELSE NULL
                                END koldumpline,
                                CASE
                                           WHEN nvl(norep.tech_key, 0) = 0 THEN
                                                      CASE
                                                                 WHEN(
                                                                                       12  - nvl(p.breaks, 0) - nvl(p.eto, 0) - nvl(p.dinner, 0)) = 0 THEN NULL
                                                                 ELSE(nvl(wt.wt_rockgm, 0) + nvl(wt.wt_loosegm, 0) + nvl(wt.wt_transgm, 0) + nvl(wt.wt_equiptrans, 0) + nvl(wt.wt_macadam, 0) + nvl(wt.wt_iwt, 0) + nvl(wt.wt_ipt, 0) + nvl(wt.wt_snow, 0) + nvl(wt.wt_prsoutcont, 0)) / (12 - nvl(p.breaks, 0) - nvl(p.eto, 0) - nvl(p.dinner, 0))
                                                      END
                                           ELSE NULL
                                END                          koldumpwork,
                                nvl(avwgm.avw_gm, 0)         avw_gm,
                                nvl(avw.avw_prs, 0)          avw_prs,
                                nvl(avw.avw_rock, 0)         avw_rock,
                                nvl(avw.avw_loose, 0)        avw_loose,
                                nvl(avw.avw_trans, 0)        avw_trans,
                                nvl(totalstop.total_idle, 0) total_idle,
                                CASE
                                           WHEN nvl(norep.tech_key, 0) = 0 THEN 12
                                           ELSE NULL
                                END - (nvl(wt.wt_rockgm, 0) + nvl(wt.wt_loosegm, 0) + nvl(wt.wt_transgm, 0) + nvl(wt.wt_equiptrans, 0) + nvl(wt.wt_macadam, 0) + nvl(wt.wt_iwt, 0) + nvl(wt.wt_ipt, 0) + nvl(wt.wt_snow, 0) + nvl(wt.wt_prsoutcont, 0) + nvl(totalstop.total_idle, 0)) balance_time
                     FROM       dispatcher.allvehs t
                     inner join kgp.pto_tech pt
                     ON         pt.controlid = t.controlid
                     AND        pt.category = 'Автосамосвалы'
                     left join  st
                     ON         st.tech_key = t.controlid
                     left join  wt
                     ON         wt.tech_key = t.controlid
                     AND        wt.shiftdate = st.shiftdate
                     AND        wt.shiftnum = st.shiftnum
                     left join  p
                     ON         p.tech_key = t.controlid
                     AND        p.shiftdate = st.shiftdate
                     AND        p.shiftnum = st.shiftnum
                     left join  gmw
                     ON         gmw.tech_key = t.controlid
                     AND        gmw.shiftdate = st.shiftdate
                     AND        gmw.shiftnum = st.shiftnum
                     left join  gmv
                     ON         gmv.tech_key = t.controlid
                     AND        gmv.shiftdate = st.shiftdate
                     AND        gmv.shiftnum = st.shiftnum
                     left join  tr
                     ON         tr.tech_key = t.controlid
                     AND        tr.shiftdate = st.shiftdate
                     AND        tr.shiftnum = st.shiftnum
                     left join  rasgm
                     ON         rasgm.tech_key = t.controlid
                     AND        rasgm.shiftdate = st.shiftdate
                     AND        rasgm.shiftnum = st.shiftnum
                     left join  ras
                     ON         ras.tech_key = t.controlid
                     AND        ras.shiftdate = st.shiftdate
                     AND        ras.shiftnum = st.shiftnum
                     left join  ft
                     ON         ft.tech_key = t.controlid
                     AND        ft.shiftdate = st.shiftdate
                     AND        ft.shiftnum = st.shiftnum
                     left join  avw
                     ON         avw.tech_key = t.controlid
                     AND        avw.shiftdate = st.shiftdate
                     AND        avw.shiftnum = st.shiftnum
                     left join  avwgm
                     ON         avwgm.tech_key = t.controlid
                     AND        avwgm.shiftdate = st.shiftdate
                     AND        avwgm.shiftnum = st.shiftnum
                     left join  dtmh
                     ON         dtmh.tech_key = t.controlid
                     left join  totalstop
                     ON         totalstop.tech_key = t.controlid
                     AND        totalstop.shiftdate = st.shiftdate
                     AND        totalstop.shiftnum = st.shiftnum
                     left join  norep
                     ON         norep.tech_key = t.controlid
                     AND        norep.shiftdate = st.shiftdate
                     AND        norep.shiftnum = st.shiftnum) src
left join
          (
                 SELECT *
                 FROM   dumptrucks
                 WHERE  columnnum = 1)smsv
ON        src.tech_id = smsv.vehid
WHERE     (
                    tech_id = :paramSelectTechId
          OR        : paramSelectTechId = 'Все')
GROUP BY  model,
          position,
          tech_id
ORDER BY  model,
          position,
          tech_id DESC





";
        }

        public static string Get_Bogdan_custom_OperPoroda_Shov()
        {
            return @"




WITH sst AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      sst.task_date shiftdate,
                      sst.shift     shiftnum
           FROM       shov_shift_tasks sst
           inner join dispatcher.shovels d
           ON         d.shovid = sst.shov_id
           AND        d.site='ТОО Комаровское'
           WHERE      ((
                                            sst.task_date = :paramDateFrom
                                 AND        sst.shift >= :paramShiftFrom)
                      OR         (
                                            sst.task_date > :paramDateFrom))
           AND        ((
                                            sst.task_date = :paramDateTo
                                 AND        :paramShiftTo >= sst.shift)
                      OR         (
                                            :paramDateTo > sst.task_date )) ), stpgs AS
( -- Простои берем из отчетов по завершению смены
           SELECT     d.controlid tech_key,
                      d.shovid,
                      s.shiftdate,
                      s.shiftnum,
                      psc.poly_stop_cat_name category,
                      s.timego,
                      s.timestop
           FROM       dispatcher.shiftstoppages_shov s
           inner join dispatcher.shovels d
           ON         d.shovid = s.vehid
           AND        d.site='ТОО Комаровское'
           inner join dispatcher.poly_user_stoppages_shov ps
           ON         ps.poly_stop_bindings_id = 104
           AND        (
                                 ps.code = s.idlestoptype
                      AND        ps.poly_stop_cat_id IS NOT NULL)
           inner join dispatcher.poly_stop_categories psc
           ON         psc.poly_stop_cat_id = ps.poly_stop_cat_id
           WHERE      ((
                                            shiftdate = :paramDateFrom
                                 AND        shiftnum >= :paramShiftFrom)
                      OR         (
                                            shiftdate > :paramDateFrom))
           AND        ((
                                            shiftdate = :paramDateTo
                                 AND        :paramShiftTo >= shiftnum)
                      OR         (
                                            :paramDateTo > shiftdate ))
           AND        (
                                 s.timego - s.timestop) * 24 * 60 >=5
           AND        s.timestop IS NOT NULL
           AND        s.timego IS NOT NULL
           AND        psc.poly_stop_cat_name IS NOT NULL ), detalq AS
(
           SELECT     selres.tech_key,
                      selres.shovid,
                      selres.shiftdate,
                      selres.shiftnum,
                      psc.poly_work_cat_name                category,
                      SUM(selres.detaltime)                 detaltime,
                      SUM(idletime)                         idletime,
                      SUM(selres.detaltime-selres.idletime) detaldur
           FROM       (
                               SELECT   sel.tech_key,
                                        sel.shovid,
                                        sel.shiftdate,
                                        sel.shiftnum,
                                        sel.worktype,
                                        SUM((sel.finish_time_manual-sel.start_time_manual)*24) detaltime,
                                        SUM( NVL(
                                                   (
                                                   SELECT SUM( NVL( (LEAST(stpgs.timego,sel.finish_time_manual) - GREATEST(sel.start_time_manual,stpgs.timestop) ),0) * 24) idle_time
                                                   FROM   stpgs
                                                   WHERE  (
                                                                 stpgs.timestop BETWEEN sel.start_time_manual AND    sel.finish_time_manual
                                                          OR     stpgs.timego BETWEEN sel.start_time_manual AND    sel.finish_time_manual)
                                                   AND    stpgs.shovid=sel.shovid
                                                   AND    sel.shiftdate=stpgs.shiftdate
                                                   AND    sel.shiftnum=stpgs.shiftnum ) ,0) ) idletime
                               FROM     (
                                                   SELECT     d.controlid tech_key,
                                                              d.shovid,
                                                              st.task_date shiftdate,
                                                              st.shift     shiftnum,
                                                              ssra.worktype,
                                                              ssra.start_time_manual,
                                                              ssra.finish_time_manual
                                                   FROM       shov_shift_tasks st
                                                   inner join shov_shift_reports ssr
                                                   ON         ssr.task_id=st.id
                                                   inner join shov_shift_reports_adv ssra
                                                   ON         ssra.report_id=ssr.id
                                                   inner join dispatcher.shovels d
                                                   ON         d.shovid = st.shov_id
                                                   AND        d.site='ТОО Комаровское'
                                                   WHERE      ((
                                                                                    st.task_date = :paramDateFrom
                                                                         AND        st.shift >= :paramShiftFrom)
                                                              OR         (
                                                                                    st.task_date > :paramDateFrom))
                                                   AND        ((
                                                                                    st.task_date = :paramDateTo
                                                                         AND        :paramShiftTo >= st.shift)
                                                              OR         (
                                                                                    :paramDateTo > st.task_date ))
                                                   AND        ssra.finish_time_manual IS NOT NULL
                                                   AND        ssra.start_time_manual IS NOT NULL ) sel
                               GROUP BY sel.tech_key,
                                        sel.shovid,
                                        sel.worktype,
                                        sel.shiftdate,
                                        sel.shiftnum )selres
           inner join dispatcher.poly_user_works_shov ps
           ON         ps.poly_work_bindings_id = 162
           AND        (
                                 ps.id = KGP_SHOVWTTOWTID(selres.worktype)
                      AND        ps.poly_work_cat_id IS NOT NULL)
           inner join dispatcher.poly_work_categories psc
           ON         psc.poly_work_cat_id = ps.poly_work_cat_id
           GROUP BY   selres.tech_key,
                      selres.shovid,
                      selres.shiftdate,
                      selres.shiftnum,
                      psc.poly_work_cat_name ),
---------------простои экскаваторов------------------------
s AS
( -- Простои берем из отчетов по завершению смены
         SELECT   s.tech_key,
                  s.shovid,
                  s.shiftdate,
                  s.shiftnum,
                  s.category,
                  SUM((s.timego - s.timestop) * 24) TIME
         FROM     stpgs s
         GROUP BY s.tech_key,
                  s.shovid,
                  s.shiftdate,
                  s.shiftnum,
                  s.category
         UNION ALL
         SELECT   d.tech_key,
                  d.shovid,
                  d.shiftdate,
                  d.shiftnum,
                  d.category,
                  SUM(d.detaldur) TIME
         FROM     detalq d
         WHERE    d.category IN( 'перегон',
                                'подготовка подъезда',
                                'зачистка забоя',
                                'чистка кузова',
                                'чистка ковша',
                                'перегон на ВР',
                                'перегон на ремонт',
                                'перецепка кабеля на ВР',
                                'перемещение по забою' )
         GROUP BY d.tech_key,
                  d.shovid,
                  d.shiftdate,
                  d.shiftnum,
                  d.category ), totalstop AS
(
         SELECT   tech_key,
                  shiftdate,
                  shiftnum,
                  SUM(TIME) totalidle
         FROM     s
         WHERE    category IS NOT NULL
         GROUP BY tech_key,
                  shiftdate,
                  shiftnum ), psub AS
( -- Транспонируем таблицу с простоями
       SELECT *
       FROM   s pivot (SUM(TIME) FOR category IN ( 'ТР' tr,
                                                  'Т1,Т2,Т3,Т4,Т5' service,
                                                  'КР' kr,
                                                  'Обед' dinner,
                                                  'Прием/передача смены' breaks,
                                                  'ЕТО' eto,
                                                  'заправка(ДТ,вода)' refuel,
                                                  'перегон' relocation,
                                                  'личные нужды' persneed,
                                                  'перемещение по забою' moveblock,
                                                  'ожидание а/с' waittruck,
                                                  'подготовка подъезда' prepentrance,
                                                  'зачистка забоя' zaboiclean,
                                                  'чистка кузова' bodyclean,
                                                  'чистка ковша' bucketclean,
                                                  'ВЗРЫВНЫЕ РАБОТЫ' vr,
                                                  'перегон на ВР' vrreloc,
                                                  'ТЕХНИЧЕСКИЙ ПЕРЕРЫВ' techper,
                                                  'Прием/передача смены ХотСит' breakshotseat,
                                                  'Климатические условия' weather,
                                                  'Рем.элек.оборуд.' electrical,
                                                  'ДВС' dvs,
                                                  'Трансмиссия' transmission,
                                                  'Ходовая часть' chassis,
                                                  'Навесное оборудование' hinge,
                                                  'ремонт а/ш' tires,
                                                  'Гидравлическая часть' hydraulic,
                                                  'перегон на ремонт' repairreloc,
                                                  'Наладочные работы' adjustment,
                                                  'Механизм поворота' turnmech,
                                                  'Перепасовка каната' ropeshuffle,
                                                  'Замена кабеля и перемычек' cablereplace,
                                                  'Замена каната' ropereplace,
                                                  'Аварийные прочие' emergothers,
                                                  'отсутствие вспомогательной техники' auxlack,
                                                  'Отсут.зап.частей' partslack,
                                                  'Прочие' othersreason,
                                                  'доливка масла/антифриза' toppoil,
                                                  'Остановка контролирующими органами' regauth,
                                                  'Отсутствие диз.топлива' fuellack,
                                                  'Работа с маркшейдерами' survwork,
                                                  'Работа с геологами' geowork,
                                                  'Очистка ходовой базы' gobase,
                                                  'отсутствие оператора' stafflack,
                                                  'Организац.прочие' orgothers,
                                                  'Отсутствие экипажа (без бригады)' crewlack,
                                                  'Отсут.фронта работ' worklack,
                                                  'перецепка кабеля на ВР' reconcable )) ), p AS
(
       SELECT psub.tech_key,
              psub.shovid,
              psub.shiftdate,
              psub.shiftnum,
              psub.tr,
              psub.service,
              psub.kr,
              psub.dinner,
              psub.breaks,
              psub.eto,
              psub.refuel,
              psub.relocation,
              psub.persneed,
              psub.moveblock,
              psub.waittruck,
              psub.prepentrance,
              psub.zaboiclean,
              psub.bodyclean,
              psub.bucketclean,
              psub.vr,
              psub.vrreloc,
              psub.techper,
              psub.breakshotseat,
              psub.weather,
              psub.electrical,
              psub.dvs,
              psub.transmission,
              psub.chassis,
              psub.hinge,
              psub.tires,
              psub.hydraulic,
              psub.repairreloc,
              psub.adjustment,
              psub.turnmech,
              psub.ropeshuffle,
              psub.cablereplace,
              psub.ropereplace,
              psub.emergothers,
              psub.auxlack,
              psub.partslack,
              psub.othersreason,
              psub.toppoil,
              psub.regauth,
              psub.fuellack,
              psub.survwork,
              psub.geowork,
              psub.gobase,
              psub.stafflack,
              psub.orgothers,
              psub.crewlack,
              psub.worklack,
              psub.reconcable,
              --ITOGPLANREM
              nvl(psub.tr,0)+ nvl(psub.service,0)+ nvl(psub.kr,0) itogplanrem,
              --ITOGTECHNOL
              nvl(psub.dinner,0)+ nvl(psub.breaks,0)+ nvl(psub.eto,0)+ nvl(psub.refuel,0)+ nvl(psub.relocation,0)+ nvl(psub.persneed,0)+ nvl(psub.moveblock,0)+ nvl(psub.waittruck,0)+ nvl(psub.prepentrance,0)+ nvl(psub.zaboiclean,0)+ nvl(psub.bodyclean,0)+ nvl(psub.bucketclean,0)+ nvl(psub.vr,0)+ nvl(psub.vrreloc,0)+ nvl(psub.techper,0)+ nvl(psub.breakshotseat,0)+ nvl(psub.reconcable,0) itogtechnol,
              --ITOGEMERG
              nvl(psub.electrical,0)+ nvl(psub.dvs,0)+ nvl(psub.transmission,0)+ nvl(psub.chassis,0)+ nvl(psub.hinge,0)+ nvl(psub.tires,0)+ nvl(psub.hydraulic,0)+ nvl(psub.repairreloc,0)+ nvl(psub.adjustment,0)+ nvl(psub.turnmech,0)+ nvl(psub.ropeshuffle,0)+ nvl(psub.cablereplace,0)+ nvl(psub.ropereplace,0)+ nvl(psub.emergothers,0)+ nvl(psub.auxlack,0)+ nvl(psub.partslack,0)+ nvl(psub.othersreason,0)+ nvl(psub.toppoil,0) itogemerg,
              --ITOGORG
              nvl(psub.regauth,0)+ nvl(psub.fuellack,0)+ nvl(psub.survwork,0)+ nvl(psub.geowork,0)+ nvl(psub.gobase,0)+ nvl(psub.stafflack,0)+ nvl(psub.orgothers,0)+ nvl(psub.crewlack,0)+ nvl(psub.worklack,0)                                                                                                                                                                                                        itogorg,
              nvl(psub.dinner,0) + nvl(psub.breaks,0)+ nvl(psub.eto,0)+ nvl(psub.refuel,0)+ nvl(psub.relocation,0)+ nvl(psub.persneed,0)+ nvl(psub.moveblock,0)+ nvl(psub.waittruck,0)+ nvl(psub.prepentrance,0)+ nvl(psub.zaboiclean,0)+ nvl(psub.bodyclean,0)+ nvl(psub.bucketclean,0)+ nvl(psub.vr,0)+ nvl(psub.vrreloc,0)+ nvl(psub.techper,0)+ nvl(psub.breakshotseat,0)+ nvl(psub.weather,0)+ nvl(psub.regauth,0)+ nvl(psub.fuellack,0)+ nvl(psub.survwork,0)+ nvl(psub.geowork,0)+ nvl(psub.gobase,0)+ nvl(psub.stafflack,0)+ nvl(psub.orgothers,0)+ nvl(psub.crewlack,0)+ nvl(psub.worklack,0)+ nvl(psub.reconcable,0) s_kio,
              nvl(psub.tr,0)     + nvl(psub.service,0)+ nvl(psub.kr,0)+ nvl(psub.electrical,0)+ nvl(psub.dvs,0)+ nvl(psub.transmission,0)+ nvl(psub.chassis,0)+ nvl(psub.hinge,0)+ nvl(psub.tires,0)+ nvl(psub.hydraulic,0)+ nvl(psub.repairreloc,0)+ nvl(psub.adjustment,0)+ nvl(psub.turnmech,0)+ nvl(psub.ropeshuffle,0)+ nvl(psub.cablereplace,0)+ nvl(psub.ropereplace,0)+ nvl(psub.emergothers,0)+ nvl(psub.auxlack,0)+ nvl(psub.partslack,0)+ nvl(psub.othersreason,0)+ nvl(psub.toppoil,0)                                                                                                                             s_ktg
       FROM   psub ), detal AS
(
         SELECT   tech_key,
                  shovid,
                  shiftdate,
                  shiftnum,
                  category,
                  SUM(detaldur) TIME
         FROM     detalq
         WHERE    category IN ( 'дренажные работы',
                               'доборы',
                               'постановка борта в конеч.положение',
                               'переэкскавация',
                               'с разборкой забоя',
                               'хоз.работы прочие' )
         GROUP BY tech_key,
                  shovid,
                  shiftdate,
                  shiftnum,
                  category ), totaldetal AS
(
         SELECT   tech_key,
                  shiftdate,
                  shiftnum,
                  SUM(TIME) totaldetal
         FROM     detal
         WHERE    category IS NOT NULL
         GROUP BY tech_key,
                  shiftdate,
                  shiftnum ), wtdetal AS
(
       SELECT *
       FROM   detal pivot (SUM(TIME) FOR category IN ( 'дренажные работы' wtdrghr,
                                                      'доборы' wtdoborhr,
                                                      'постановка борта в конеч.положение' wtpostborthr,
                                                      'переэкскавация' wtreshovhr,
                                                      'с разборкой забоя' wtrazbzabhr,
                                                      'хоз.работы прочие' wtothershr )) ), ttlstpdetal AS
(
         SELECT   tech_key,
                  shiftdate,
                  shiftnum,
                  SUM(TIME) allstpdetaltime
         FROM     (
                         SELECT tech_key,
                                shiftdate,
                                shiftnum,
                                totaldetal TIME
                         FROM   totaldetal
                         UNION ALL
                         SELECT tech_key,
                                shiftdate,
                                shiftnum,
                                totalidle TIME
                         FROM   totalstop )
         GROUP BY tech_key,
                  shiftdate,
                  shiftnum ),
--------кол-во рейсов, вес, объем--------
tripssra AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      sra.taskdate              shiftdate,
                      sra.shift                 shiftnum,
                      psc.poly_work_cat_name    category,
                      SUM(sra.tripnumbermanual) tripskol
           FROM       shiftreportsadv sra
           inner join dispatcher.shovels d
           ON         d.shovid = sra.shovid
           AND        d.site='ТОО Комаровское'
           inner join dispatcher.poly_user_works_dump ps
           ON         ps.poly_work_bindings_id = 161
           AND        (
                                 ps.id = kgp_dumpwttowtid(sra.worktype)
                      AND        ps.poly_work_cat_id IS NOT NULL)
           inner join dispatcher.poly_work_categories psc
           ON         psc.poly_work_cat_id = ps.poly_work_cat_id
           WHERE      ((
                                            sra.taskdate = :paramDateFrom
                                 AND        sra.shift >= :paramShiftFrom)
                      OR         (
                                            sra.taskdate > :paramDateFrom))
           AND        ((
                                            sra.taskdate = :paramDateTo
                                 AND        :paramShiftTo >= sra.shift)
                      OR         (
                                            :paramDateTo > sra.taskdate ))
           AND        NOT ( (
                                            trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                 OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                 OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                      AND        (
                                            trim(upper(worktype)) LIKE '%ПРС%'
                                 OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                 OR         trim(upper(worktype)) LIKE '%РУДА%' ) )
           GROUP BY   d.controlid,
                      d.shovid,
                      sra.taskdate,
                      sra.shift,
                      psc.poly_work_cat_name ), weightsra AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      selres.shiftdate,
                      selres.shiftnum,
                      psc.poly_work_cat_name                            category,
                      SUM(selres.tripnumbermanual*selres.avweight)/1000 weight
           FROM       (
                                 SELECT     sra.shovid,
                                            sra.taskdate shiftdate,
                                            sra.shift    shiftnum,
                                            wt.id        worktype_id,
                                            sra.tripnumbermanual,
                                            CASE
                                                       WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                       ELSE sra.avweight
                                            END avweight
                                 FROM       shiftreportsadv sra
                                 inner join worktypes wt
                                 ON         sra.worktype=wt.name
                                 WHERE      ((
                                                                  sra.taskdate = :paramDateFrom
                                                       AND        sra.shift >= :paramShiftFrom)
                                            OR         (
                                                                  sra.taskdate > :paramDateFrom))
                                 AND        ((
                                                                  sra.taskdate = :paramDateTo
                                                       AND        :paramShiftTo >= sra.shift)
                                            OR         (
                                                                  :paramDateTo > sra.taskdate ))
                                 AND        NOT ( (
                                                                  trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                       OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                       OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                            AND        (
                                                                  trim(upper(worktype)) LIKE '%ПРС%'
                                                       OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                       OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
           inner join dispatcher.shovels d
           ON         d.shovid = selres.shovid
           AND        d.site='ТОО Комаровское'
           inner join dispatcher.poly_user_works_dump ps
           ON         ps.poly_work_bindings_id = 161
           AND        (
                                 ps.id = selres.worktype_id
                      AND        ps.poly_work_cat_id IS NOT NULL)
           inner join dispatcher.poly_work_categories psc
           ON         psc.poly_work_cat_id = ps.poly_work_cat_id
           GROUP BY   d.controlid,
                      d.shovid,
                      selres.shiftdate,
                      selres.shiftnum,
                      psc.poly_work_cat_name ), volsra AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      selres.shiftdate,
                      selres.shiftnum,
                      psc.poly_work_cat_name category,
                      SUM(decode(nvl(selres.avweight,0),
                                 0,0,
                                 selres.avweight*selres.tripnumbermanual/nvl(decode(selres.weightrate,
                                                                                    0,selres.avweight,
                                                                                    selres.weightrate),selres.avweight)*selres.volumerate))/1000 vol
           FROM       (
                                 SELECT     sra.shovid,
                                            sra.taskdate shiftdate,
                                            sra.shift    shiftnum,
                                            wt.id        worktype_id,
                                            sra.tripnumbermanual,
                                            CASE
                                                       WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                       ELSE sra.avweight
                                            END avweight,
                                            sra.weightrate,
                                            sra.volumerate
                                 FROM       shiftreportsadv sra
                                 inner join worktypes wt
                                 ON         sra.worktype=wt.name
                                 WHERE      ((
                                                                  sra.taskdate = :paramDateFrom
                                                       AND        sra.shift >= :paramShiftFrom)
                                            OR         (
                                                                  sra.taskdate > :paramDateFrom))
                                 AND        ((
                                                                  sra.taskdate = :paramDateTo
                                                       AND        :paramShiftTo >= sra.shift)
                                            OR         (
                                                                  :paramDateTo > sra.taskdate ))
                                 AND        NOT ( (
                                                                  trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                       OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                       OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                            AND        (
                                                                  trim(upper(worktype)) LIKE '%ПРС%'
                                                       OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                       OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
           inner join dispatcher.shovels d
           ON         d.shovid = selres.shovid
           AND        d.site='ТОО Комаровское'
           inner join dispatcher.poly_user_works_dump ps
           ON         ps.poly_work_bindings_id = 161
           AND        (
                                 ps.id = selres.worktype_id
                      AND        ps.poly_work_cat_id IS NOT NULL)
           inner join dispatcher.poly_work_categories psc
           ON         psc.poly_work_cat_id = ps.poly_work_cat_id
           GROUP BY   d.controlid,
                      d.shovid,
                      selres.shiftdate,
                      selres.shiftnum,
                      psc.poly_work_cat_name ), wtsra AS
(
       -------транспонируем рейсы-------
       SELECT *
       FROM   tripssra pivot (SUM(tripskol) FOR category IN ( 'вскрыша скальная' wtrockstrip,
                                                             'вскрыша рыхлая' wtloosestrip,
                                                             'ПРС в контуре карьера' wtprs,
                                                             'вскрыша транзитная' wttransstrip,
                                                             'руда скальная' wtrockore,
                                                             'руда рыхлая' wtlooseore,
                                                             'руда транзитная' wttransore,
                                                             'ВКП' wtipthr,
                                                             'ВСП' wtiwthr,
                                                             'ПРС вне контура карьера' wtprsouthr,
                                                             'хоз.работы прочие' wtsneg )) ), wtsub AS
(
       SELECT sel.tech_key,
              sel.shovid,
              sel.shiftdate,
              sel.shiftnum, (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtrockstrip,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                               * sel.totalworktime ) wtrockstrip, ----
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtloosestrip,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                                * sel.totalworktime ) wtloosestrip,----
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtprs,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                         * sel.totalworktime ) wtprs,---
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wttransstrip,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                                * sel.totalworktime ) wttransstrip, --
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtrockore,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                             * sel.totalworktime ) wtrockore, ---
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtlooseore,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                              * sel.totalworktime ) wtlooseore,--
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wttransore,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                              * sel.totalworktime ) wttransore, --
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtipthr,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                           * sel.totalworktime ) wtipthr,---
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtiwthr,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                           * sel.totalworktime ) wtiwthr,---
              (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtprsouthr,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                              * sel.totalworktime ) wtprsouthr, (
              CASE
                     WHEN (
                                   nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )=0 THEN 0
                     ELSE nvl(sel.wtsneg,0)/ (nvl(sel.wtrockstrip,0)+ nvl(sel.wtloosestrip,0)+ nvl(sel.wtprs,0)+ nvl(sel.wttransstrip,0)+ nvl(sel.wtrockore,0)+ nvl(sel.wtlooseore,0)+ nvl(sel.wttransore,0)+ nvl(sel.wtipthr,0)+ nvl(sel.wtiwthr,0)+ nvl(sel.wtprsouthr,0)+ nvl(sel.wtsneg,0) )
              END                          * sel.totalworktime ) wtothershr,
              nvl(sel.allstpdetaltime,0)                         allstpdetaltime,
              sel.totalworktime
       FROM   (
                        SELECT    wtsra.tech_key,
                                  wtsra.shovid,
                                  wtsra.shiftdate,
                                  wtsra.shiftnum,
                                  wtsra.wtrockstrip,
                                  wtsra.wtloosestrip,
                                  wtsra.wtprs,
                                  wtsra.wttransstrip,
                                  wtsra.wtrockore,
                                  wtsra.wtlooseore,
                                  wtsra.wttransore,
                                  wtsra.wtipthr,
                                  wtsra.wtiwthr,
                                  wtsra.wtprsouthr,
                                  wtsra.wtsneg,
                                  ttlstpdetal.allstpdetaltime,
                                  12-nvl(ttlstpdetal.allstpdetaltime,0) totalworktime
                        FROM      wtsra
                        left join ttlstpdetal
                        ON        wtsra.tech_key=ttlstpdetal.tech_key
                        AND       wtsra.shiftdate=ttlstpdetal.shiftdate
                        AND       wtsra.shiftnum=ttlstpdetal.shiftnum )sel ), wt AS
(
          SELECT    wtsub.tech_key,
                    wtsub.shovid,
                    wtsub.shiftdate,
                    wtsub.shiftnum,
                    --WTGM
                    nvl(wtsub.wtrockstrip,0)+ nvl(wtsub.wtloosestrip,0)+ nvl(wtsub.wtprs,0)+ nvl(wtsub.wttransstrip,0)+ nvl(wtsub.wtrockore,0)+ nvl(wtsub.wtlooseore,0)+ nvl(wtsub.wttransore,0) wtgm,
                    wtsub.wtrockstrip,
                    wtsub.wtloosestrip,
                    wtsub.wtprs,
                    wtsub.wttransstrip,
                    wtsub.wtrockore,
                    wtsub.wtlooseore,
                    wtsub.wttransore,
                    --WTHR
                    nvl(wtdetal.wtdrghr,0)+ nvl(wtdetal.wtdoborhr,0)+ nvl(wtsub.wtipthr,0)+ nvl(wtsub.wtiwthr,0)+ nvl(wtdetal.wtpostborthr,0)+ nvl(wtdetal.wtreshovhr,0)+ nvl(wtdetal.wtrazbzabhr,0)+ nvl(wtsub.wtprsouthr,0)+ nvl(wtsub.wtothershr,0)+ nvl(wtdetal.wtothershr,0) wthr,
                    wtdetal.wtdrghr,
                    wtdetal.wtdoborhr,
                    wtsub.wtipthr,
                    wtsub.wtiwthr,
                    wtdetal.wtpostborthr,
                    wtdetal.wtreshovhr,
                    wtdetal.wtrazbzabhr,
                    wtsub.wtprsouthr,
                    nvl(wtsub.wtothershr,0)+ nvl(wtdetal.wtothershr,0) wtothershr
          FROM      wtsub
          left join wtdetal
          ON        wtdetal.tech_key=wtsub.tech_key
          AND       wtdetal.shiftdate=wtsub.shiftdate
          AND       wtdetal.shiftnum=wtsub.shiftnum ),
-----------моточасы экск--
shmh AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      shiftdate,
                      shiftnum,
                      nvl(sel.mh,0) mh
           FROM       (
                               SELECT   selres.shovid,
                                        selres.shiftdate,
                                        selres.shiftnum,
                                        SUM(selres.motohoursend-selres.motohoursbegin) mh
                               FROM     (
                                                   SELECT     sst.shov_id   shovid,
                                                              sst.task_date shiftdate,
                                                              sst.shift     shiftnum,
                                                              ssr.motohoursbegin,
                                                              ssr.motohoursend
                                                   FROM       shov_shift_reports ssr
                                                   inner join shov_shift_tasks sst
                                                   ON         sst.id=ssr.task_id
                                                   WHERE      ((
                                                                                    task_date = :paramDateFrom
                                                                         AND        shift >= :paramShiftFrom)
                                                              OR         (
                                                                                    task_date > :paramDateFrom))
                                                   AND        ((
                                                                                    task_date = :paramDateTo
                                                                         AND        :paramShiftTo >= shift)
                                                              OR         (
                                                                                    :paramDateTo > task_date )) )selres
                               GROUP BY selres.shovid,
                                        selres.shiftdate,
                                        selres.shiftnum )sel
           inner join shovels d
           ON         d.shovid = sel.shovid
           AND        d.site='ТОО Комаровское' ),
-----горизонты г.м.------
horq AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      shiftdate,
                      shiftnum,
                      sel.hors horizont
           FROM       (
                               SELECT   selres.shovid,
                                        selres.shiftdate,
                                        selres.shiftnum,
                                        selres.hors
                               FROM     (
                                                 SELECT   sra.shovid,
                                                          sra.taskdate                                           shiftdate,
                                                          sra.shift                                              shiftnum,
                                                          listagg(sra.area,', ') within GROUP(ORDER BY sra.area) hors
                                                 FROM     (
                                                                   SELECT   shovid,
                                                                            taskdate,
                                                                            shift,
                                                                            area
                                                                   FROM     shiftreportsadv
                                                                   WHERE    NOT ( (
                                                                                              trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                                     OR       trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                                     OR       trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                            AND      (
                                                                                              trim(upper(worktype)) LIKE '%ПРС%'
                                                                                     OR       trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                                     OR       trim(upper(worktype)) LIKE '%РУДА%' ) )
                                                                   GROUP BY shovid,
                                                                            taskdate,
                                                                            shift,
                                                                            area ) sra
                                                 WHERE    ((
                                                                            sra.taskdate = :paramDateFrom
                                                                   AND      sra.shift >= :paramShiftFrom)
                                                          OR       (
                                                                            sra.taskdate > :paramDateFrom))
                                                 AND      ((
                                                                            sra.taskdate = :paramDateTo
                                                                   AND      :paramShiftTo >= sra.shift)
                                                          OR       (
                                                                            :paramDateTo > sra.taskdate ))
                                                 GROUP BY sra.shovid,
                                                          sra.taskdate,
                                                          sra.shift )selres
                               GROUP BY selres.shovid,
                                        selres.shiftdate,
                                        selres.shiftnum,
                                        selres.hors )sel
           inner join shovels d
           ON         d.shovid = sel.shovid
           AND        d.site='ТОО Комаровское' ), gmwsub AS
(
       -------транспонируем рейсы-------
       SELECT *
       FROM   weightsra pivot (SUM(weight) FOR category IN ( 'вскрыша скальная' gmwrockstrip,
                                                            'вскрыша рыхлая' gmwloosestrip,
                                                            'ПРС в контуре карьера' gmwprs,
                                                            'вскрыша транзитная' gmwtransstrip,
                                                            'руда скальная' gmwrockore,
                                                            'руда рыхлая' gmwlooseore,
                                                            'руда транзитная' gmwtransore )) ), gmw AS
(
       SELECT gmwsub.tech_key,
              gmwsub.shovid,
              gmwsub.shiftdate,
              gmwsub.shiftnum,
              --GMWROCK
              nvl(gmwsub.gmwrockstrip,0)+ nvl(gmwsub.gmwrockore,0) gmwrock,
              --GMWLOOSE
              nvl(gmwsub.gmwloosestrip,0)+ nvl(gmwsub.gmwprs,0)+ nvl(gmwsub.gmwlooseore,0) gmwloose,
              --GMWTRANS
              nvl(gmwsub.gmwtransstrip,0)+ nvl(gmwsub.gmwtransore,0) gmwtrans,
              gmwrockstrip,
              gmwloosestrip,
              gmwprs,
              gmwtransstrip,
              gmwrockore,
              gmwlooseore,
              gmwtransore
       FROM   gmwsub ), gmvsub AS
(
       -------транспонируем рейсы-------
       SELECT *
       FROM   volsra pivot (SUM(vol) FOR category IN ( 'вскрыша скальная' gmvrockstrip,
                                                      'вскрыша рыхлая' gmvloosestrip,
                                                      'ПРС в контуре карьера' gmvprs,
                                                      'вскрыша транзитная' gmvtransstrip,
                                                      'руда скальная' gmvrockore,
                                                      'руда рыхлая' gmvlooseore,
                                                      'руда транзитная' gmvtransore )) ), gmv AS
(
       SELECT gmvsub.tech_key,
              gmvsub.shovid,
              gmvsub.shiftdate,
              gmvsub.shiftnum,
              --GMVROCK
              nvl(gmvsub.gmvrockstrip,0)+ nvl(gmvsub.gmvrockore,0) gmvrock,
              --GMVLOOSE
              nvl(gmvsub.gmvloosestrip,0)+ nvl(gmvsub.gmvprs,0)+ nvl(gmvsub.gmvlooseore,0) gmvloose,
              --GMVTRANS
              nvl(gmvsub.gmvtransstrip,0)+ nvl(gmvsub.gmvtransore,0) gmvtrans,
              gmvrockstrip,
              gmvloosestrip,
              gmvprs,
              gmvtransstrip,
              gmvrockore,
              gmvlooseore,
              gmvtransore
       FROM   gmvsub ), trsub AS
(
       -------транспонируем рейсы-------
       SELECT *
       FROM   tripssra pivot (SUM(tripskol) FOR category IN ( 'вскрыша скальная' trrockstrip,
                                                             'вскрыша рыхлая' trloosestrip,
                                                             'ПРС в контуре карьера' trprs,
                                                             'вскрыша транзитная' trtransstrip,
                                                             'руда скальная' trrockore,
                                                             'руда рыхлая' trlooseore,
                                                             'руда транзитная' trtransore )) ), tr AS
(
       SELECT trsub.tech_key,
              trsub.shovid,
              trsub.shiftdate,
              trsub.shiftnum,
              --TRGM
              nvl(trsub.trrockstrip,0)+ nvl(trsub.trloosestrip,0)+ nvl(trsub.trprs,0)+ nvl(trsub.trtransstrip,0)+ nvl(trsub.trrockore,0)+ nvl(trsub.trlooseore,0)+ nvl(trsub.trtransore,0) trgm,
              trsub.trrockstrip,
              trsub.trloosestrip,
              trsub.trprs,
              trsub.trtransstrip,
              trsub.trrockore,
              trsub.trlooseore,
              trsub.trtransore
       FROM   trsub ),
-------------------------------ср.взв.расстояние г.м.------
rasgmq AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length,0)      length
           FROM       (
                                 SELECT     selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0,0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.shovid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 61 --категория ср.взв.расстояние г.м.
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join shovels d
           ON         d.shovid = sel.shovid
           AND        d.site='ТОО Комаровское' ),
----------------------
rasgm AS
(
       -- Транспонируем таблицу с ср.взв.расстояниями
       SELECT *
       FROM   rasgmq pivot (SUM(length) FOR category IN ( 'ср.взв.расст.г.м.' rasgm )) ),
-------Расстояние--------
rasq AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.length,0)      length
           FROM       (
                                 SELECT     selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0,0,
                                                   SUM(selres.tripnumbermanual*selres.avlength)/SUM(selres.tripnumbermanual)) length
                                 FROM       (
                                                       SELECT     sra.shovid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avlength,0) = 0 THEN sra.lengthmanual
                                                                             ELSE sra.avlength
                                                                  END avlength
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 5 -- категория расстрояние
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join shovels d
           ON         d.shovid = sel.shovid
           AND        d.site='ТОО Комаровское' ),
----------------------
ras AS
(
       -- Транспонируем таблицу с расстояниями
       SELECT *
       FROM   rasq pivot (SUM(length) FOR category IN ( 'ПРС в контуре карьера' rasprs,
                                                       'вскрыша скальная' rasrockstrip,
                                                       'вскрыша рыхлая' rasloosestrip,
                                                       'вскрыша транзитная' rastransstrip,
                                                       'руда скальная' rasrockore,
                                                       'руда рыхлая' raslooseore,
                                                       'руда транзитная' rastransore )) ),
---------------загрузка г.м. средневзв----
avwgmq AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.avweight,0)    avweight
           FROM       (
                                 SELECT     selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0,0,
                                                   SUM(selres.tripnumbermanual*selres.avweight)/SUM(selres.tripnumbermanual)) avweight
                                 FROM       (
                                                       SELECT     sra.shovid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 41 --категория загрузка а/с г.м.
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join shovels d
           ON         d.shovid = sel.shovid
           AND        d.site='ТОО Комаровское' ),
----------------------
avwgm AS
(
       -- Транспонируем таблицу с загрузками а/с г.м.
       SELECT *
       FROM   avwgmq pivot (SUM(avweight) FOR category IN ( 'ср.взв.загр.г.м.' avwgm )) ),
-------Загрузка а/с г.м.--------
avwq AS
(
           SELECT     d.controlid tech_key,
                      d.shovid,
                      sel.shiftdate,
                      sel.shiftnum,
                      sel.poly_work_cat_name category,
                      nvl(sel.avweight,0)    avweight
           FROM       (
                                 SELECT     selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name,
                                            decode(SUM(selres.tripnumbermanual),
                                                   0,0,
                                                   SUM(selres.tripnumbermanual*selres.avweight)/SUM(selres.tripnumbermanual)) avweight
                                 FROM       (
                                                       SELECT     sra.shovid,
                                                                  sra.taskdate shiftdate,
                                                                  sra.shift    shiftnum,
                                                                  wt.id        worktype_id,
                                                                  sra.tripnumbermanual,
                                                                  CASE
                                                                             WHEN nvl(sra.avweight,0) = 0 THEN sra.weightrate
                                                                             ELSE sra.avweight
                                                                  END avweight
                                                       FROM       shiftreportsadv sra
                                                       inner join worktypes wt
                                                       ON         sra.worktype=wt.name
                                                       WHERE      ((
                                                                                        sra.taskdate = :paramDateFrom
                                                                             AND        sra.shift >= :paramShiftFrom)
                                                                  OR         (
                                                                                        sra.taskdate > :paramDateFrom))
                                                       AND        ((
                                                                                        sra.taskdate = :paramDateTo
                                                                             AND        :paramShiftTo >= sra.shift)
                                                                  OR         (
                                                                                        :paramDateTo > sra.taskdate ))
                                                       AND        NOT ( (
                                                                                        trim(upper(unloadid)) LIKE ('%АВТОДОРОГА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
                                                                             OR         trim(upper(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%') )
                                                                  AND        (
                                                                                        trim(upper(worktype)) LIKE '%ПРС%'
                                                                             OR         trim(upper(worktype)) LIKE '%ВСКРЫША%'
                                                                             OR         trim(upper(worktype)) LIKE '%РУДА%' ) ) )selres
                                 inner join dispatcher.poly_user_works_dump ps
                                 ON         ps.poly_work_bindings_id = 181 --категория загрузка экскаваторов
                                 AND        (
                                                       ps.id = selres.worktype_id
                                            AND        ps.poly_work_cat_id IS NOT NULL)
                                 inner join dispatcher.poly_work_categories psc
                                 ON         psc.poly_work_cat_id = ps.poly_work_cat_id
                                 GROUP BY   selres.shovid,
                                            selres.shiftdate,
                                            selres.shiftnum,
                                            psc.poly_work_cat_name )sel
           inner join shovels d
           ON         d.shovid = sel.shovid
           AND        d.site='ТОО Комаровское' ),
----------------------
avw AS
(
       -- Транспонируем таблицу с загрузками а/с г.м.
       SELECT *
       FROM   avwq pivot (SUM(avweight) FOR category IN ( 'вскрыша скальная' avwrockstrip,
                                                         'вскрыша рыхлая' avwloosestrip,
                                                         'ПРС в контуре карьера' avwprs,
                                                         'вскрыша транзитная' avwtransstrip,
                                                         'руда скальная' avwrockore,
                                                         'руда рыхлая' avwlooseore,
                                                         'руда транзитная' avwtransore )) )
--main query!!!
SELECT    category   AS category,
          smsv.model    model,
          --POSITION,
          techid AS tech_id,
          --trim(SHIFTDATE) SHIFTDATE,
          --SHIFTNUM,
          --SHIFTDATE || ' ' || SHIFTNUM || ' смена' PERD,
          ------------
          --null OTSKONS,
          --MH,
          --HORIZONT,
          --1. Календарное время всегда 12 !!! Если мы всегда к этому стеримимся то его логично принять как константу. готово
          --2. разница уходит в хоз работы прочие
          --KALENTIME,
          --REGNORM,
          --CHVHOZ,
          --WTALL,
          -----
          /*
WTGM,
WTROCKSTRIP,
WTLOOSESTRIP,
WTPRS,
WTTRANSSTRIP,
WTROCKORE,
WTLOOSEORE,
WTTRANSORE,
WTHR,
WTDRGHR,
WTDOBORHR,
WTIPTHR,
WTIWTHR,
WTPOSTBORTHR,
WTRESHOVHR,
WTRAZBZABHR,
WTPRSOUTHR,
WTOTHERSHR,
*/
          ----------------kirpnkfv------
          /*
(KALENTIME-
ITOGPLANREM-
ITOGTECHNOL-
ITOGEMERG-
ITOGORG)/KALENTIME KIRPNKFV,
*/
          ---------------kirpnor
          /*
CASE
--WHEN (KALENTIME-ITOGPLANREM-ITOGEMERG) = 0 THEN null
WHEN (KALENTIME * ((KALENTIME - s_ktg)/KALENTIME)) = 0 then null
ELSE
(KALENTIME * ((KALENTIME - s_ktg)/KALENTIME)-s_kio) / (KALENTIME * ((KALENTIME - s_ktg)/KALENTIME))
--(KALENTIME-ITOGPLANREM-ITOGEMERG-ITOGTECHNOL-ITOGORG-WEATHER) / (KALENTIME-ITOGPLANREM-ITOGEMERG)
-- (TIME_FUND * KTG - STOPPAGES_KIO) / (TIME_FUND * KTG)
END KIRPNOR,
*/
          -----------------ktg
          --(KALENTIME - s_ktg)/KALENTIME KTG,
          ---------------
          --ITOGPLANREM,
          /*
TR,
SERVICE,
KR,
ITOGTECHNOL,
DINNER,
BREAKS,
ETO,
REFUEL,
RELOCATION,
PERSNEED,
MOVEBLOCK,
WAITTRUCK,
PREPENTRANCE,
ZABOICLEAN,
BODYCLEAN,
BUCKETCLEAN,
VR,
VRRELOC,
TECHPER,
BREAKSHOTSEAT,
WEATHER,
ITOGEMERG,
ELECTRICAL,
DVS,
TRANSMISSION,
CHASSIS,
HINGE,
TIRES,
HYDRAULIC,
REPAIRRELOC,
ADJUSTMENT,
TURNMECH,
ROPESHUFFLE,
CABLEREPLACE,
ROPEREPLACE,
EMERGOTHERS,
AUXLACK,
PARTSLACK,
OTHERSREASON,
TOPPOIL,
ITOGORG,
REGAUTH,
FUELLACK,
SURVWORK,
GEOWORK,
GOBASE,
STAFFLACK,
ORGOTHERS,
CREWLACK,
WORKLACK,
*/
          /*
SUM(round(gmv, 3))           AS gmv,
SUM(round(gmvrock, 3))       AS gmvrock,
SUM(round(gmvloose, 3))      AS gmvloose,
SUM(round(gmvtrans, 3))      AS gmvtrans,
SUM(round(gmvrockstrip, 3))  AS gmvrockstrip,
SUM(round(gmvrockore, 3))    AS gmvrockore,
SUM(round(gmvlooseore, 3))   AS gmvlooseore,
SUM(round(gmvtransore, 3))   AS gmvtransore,
*/
          SUM(round((gmvrockore + gmvlooseore + gmvtransore) * 1000, 3)) AS val_rud,
          /* вскрыша скальная м3 */
          SUM(round(gmvrockstrip * 1000, 3)) AS val_sk,
          /* вскрыша скальная м3 */
          /* вскрыша транзитная м3 */
          SUM(round(gmvtransstrip * 1000, 3)) AS val_tr,
          /* вскрыша транзитная м3 */
          /* вскрыша рыхлая м3 */
          SUM(round(gmvloosestrip * 1000, 3)) AS val_rih,
          /* вскрыша рыхлая м3 */
          /* ПРС м3 */
          SUM(round(gmvprs * 1000, 3)) AS val_prs
          /* ПРС м3 */
          /*
GMV,
--GMW,
GMVROCK,
--GMWROCK,
GMVLOOSE,
--GMWLOOSE,
GMVTRANS,
--GMWTRANS,
GMVROCKSTRIP,
--GMWROCKSTRIP,
GMVLOOSESTRIP,
--GMWLOOSESTRIP,
GMVPRS,
--GMWPRS,
GMVTRANSSTRIP,
--GMWTRANSSTRIP,
GMVROCKORE,
--GMWROCKORE,
GMVLOOSEORE,
--GMWLOOSEORE,
GMVTRANSORE
--GMWTRANSORE,
--AVVPROD
*/
          /*
CASE WHEN NVL(GMV,0)=0 THEN 0
ELSE
(GMVROCKSTRIP*1000*OPVROCKSTRIP+
GMVLOOSESTRIP*1000*OPVLOOSESTRIP+
GMVPRS*1000*OPVPRS+
GMVTRANSSTRIP*1000*OPVTRANSSTRIP+
GMVROCKORE*1000*OPVROCKORE+
GMVLOOSEORE*1000*OPVLOOSEORE+
GMVTRANSORE*1000*OPVTRANSORE
)/(GMV*1000) END AVVPROD,
*/
          /*
--AVWPROD
CASE WHEN NVL(GMW,0)=0 THEN 0
ELSE
(GMWROCKSTRIP*1000*OPWROCKSTRIP+
GMWLOOSESTRIP*1000*OPWLOOSESTRIP+
GMWPRS*1000*OPWPRS+
GMWTRANSSTRIP*1000*OPWTRANSSTRIP+
GMWROCKORE*1000*OPWROCKORE+
GMWLOOSEORE*1000*OPWLOOSEORE+
GMWTRANSORE*1000*OPWTRANSORE
)/(GMW*1000) END AVWPROD,
*/
          /*
OPVROCKSTRIP,
OPWROCKSTRIP,
OPVLOOSESTRIP,
OPWLOOSESTRIP,
OPVPRS,
OPWPRS,
OPVTRANSSTRIP,
OPWTRANSSTRIP,
OPVROCKORE,
OPWROCKORE,
OPVLOOSEORE,
OPWLOOSEORE,
OPVTRANSORE,
OPWTRANSORE
*/
          ---------
          /*
TRGM,
TRROCKSTRIP,
TRLOOSESTRIP,
TRPRS,
TRTRANSSTRIP,
TRROCKORE,
TRLOOSEORE,
TRTRANSORE
*/
          --
          /*
CASE WHEN RASGM=0 THEN NULL ELSE RASGM END RASGM,
CASE WHEN RASROCKSTRIP=0 THEN NULL ELSE RASROCKSTRIP END RASROCKSTRIP,
CASE WHEN RASLOOSESTRIP=0 THEN NULL ELSE RASLOOSESTRIP END RASLOOSESTRIP,
CASE WHEN RASPRS=0 THEN NULL ELSE RASPRS END RASPRS,
CASE WHEN RASTRANSSTRIP=0 THEN NULL ELSE RASTRANSSTRIP END RASTRANSSTRIP,
CASE WHEN RASROCKORE=0 THEN NULL ELSE RASROCKORE END RASROCKORE,
CASE WHEN RASLOOSEORE=0 THEN NULL ELSE RASLOOSEORE END RASLOOSEORE,
CASE WHEN RASTRANSORE=0 THEN NULL ELSE RASTRANSORE END RASTRANSORE,
*/
          --
          /*
KOLSHOVLINE,
KOLSHOVWORK,
*/
          --
          /*
GMW*1000*RASGM FTITOG,
FTROCKSTRIP,
FTLOOSESTRIP,
FTPRS,
FTTRANSSTRIP,
FTROCKORE,
FTLOOSEORE,
FTTRANSORE,
*/
          --
          /*
CASE WHEN AVWGM=0 THEN NULL ELSE AVWGM END AVWGM,
CASE WHEN AVWROCKSTRIP=0 THEN NULL ELSE AVWROCKSTRIP END AVWROCKSTRIP,
CASE WHEN AVWLOOSESTRIP=0 THEN NULL ELSE AVWLOOSESTRIP END AVWLOOSESTRIP,
CASE WHEN AVWPRS=0 THEN NULL ELSE AVWPRS END AVWPRS,
CASE WHEN AVWTRANSSTRIP=0 THEN NULL ELSE AVWTRANSSTRIP END AVWTRANSSTRIP,
CASE WHEN AVWROCKORE=0 THEN NULL ELSE AVWROCKORE END AVWROCKORE,
CASE WHEN AVWLOOSEORE=0 THEN NULL ELSE AVWLOOSEORE END AVWLOOSEORE,
CASE WHEN AVWTRANSORE=0 THEN NULL ELSE AVWTRANSORE END AVWTRANSORE,
TOTALIDLE,
BALANCETIME
*/
FROM      (
                     SELECT     pt.position,
                                pt.category category,
                                t.vehid     techid,
                                sst.shiftdate,
                                sst.shiftnum,
                                nvl(shmh.mh,0) mh,
                                horq.horizont  horizont,
                                ----------kalen time --------
                                12 kalentime,
                                --REGNORM--
                                nvl(p.itogplanrem,0)+ nvl(p.itogtechnol,0) regnorm,
                                --------CHVHOZ-----
                                nvl(wt.wtgm,0)+ nvl(wt.wthr,0)+ nvl(p.itogtechnol,0) chvhoz,
                                --------------
                                nvl(wt.wtgm,0)+ nvl(wt.wthr,0) wtall,
                                --------------
                                nvl(wt.wtgm,0)         wtgm,
                                nvl(wt.wtrockstrip,0)  wtrockstrip,
                                nvl(wt.wtloosestrip,0) wtloosestrip,
                                nvl(wt.wtprs,0)        wtprs,
                                nvl(wt.wttransstrip,0) wttransstrip,
                                nvl(wt.wtrockore,0)    wtrockore,
                                nvl(wt.wtlooseore,0)   wtlooseore,
                                nvl(wt.wttransore,0)   wttransore,
                                -------------
                                nvl(wt.wthr,0) wthr,
                                -------
                                nvl(wt.wtdrghr,0)      wtdrghr,
                                nvl(wt.wtdoborhr,0)    wtdoborhr,
                                nvl(wt.wtipthr,0)      wtipthr,
                                nvl(wt.wtiwthr,0)      wtiwthr,
                                nvl(wt.wtpostborthr,0) wtpostborthr,
                                nvl(wt.wtreshovhr,0)   wtreshovhr,
                                nvl(wt.wtrazbzabhr,0)  wtrazbzabhr,
                                nvl(wt.wtprsouthr,0)   wtprsouthr,
                                nvl(wt.wtothershr,0)   wtothershr,
                                ---
                                nvl(p.itogplanrem,0)             itogplanrem,
                                nvl(p.tr,0)                      tr,
                                nvl(p.service,0)                 service,
                                nvl(p.kr,0)                      kr,
                                nvl(p.itogtechnol,0)             itogtechnol,
                                nvl(p.dinner,0)                  dinner,
                                nvl(p.breaks,0)                  breaks,
                                nvl(p.eto,0)                     eto,
                                nvl(p.refuel,0)                  refuel,
                                nvl(p.relocation,0)              relocation,
                                nvl(p.persneed,0)                persneed,
                                nvl(p.moveblock,0)               moveblock,
                                nvl(p.waittruck,0)               waittruck,
                                nvl(p.prepentrance,0)            prepentrance,
                                nvl(p.zaboiclean,0)              zaboiclean,
                                nvl(p.bodyclean,0)               bodyclean,
                                nvl(p.bucketclean,0)             bucketclean,
                                nvl(p.vr,0)+ nvl(p.reconcable,0) vr, --перецепка кабеля на ВР добавил к ВР
                                nvl(p.vrreloc,0)                 vrreloc,
                                nvl(p.techper,0)                 techper,
                                nvl(p.breakshotseat,0)           breakshotseat,
                                nvl(p.weather,0)                 weather,
                                nvl(p.itogemerg,0)               itogemerg,
                                nvl(p.electrical,0)              electrical,
                                nvl(p.dvs,0)                     dvs,
                                nvl(p.transmission,0)            transmission,
                                nvl(p.chassis,0)                 chassis,
                                nvl(p.hinge,0)                   hinge,
                                nvl(p.tires,0)                   tires,
                                nvl(p.hydraulic,0)               hydraulic,
                                nvl(p.repairreloc,0)             repairreloc,
                                nvl(p.adjustment,0)              adjustment,
                                nvl(p.turnmech,0)                turnmech,
                                nvl(p.ropeshuffle,0)             ropeshuffle,
                                nvl(p.cablereplace,0)            cablereplace,
                                nvl(p.ropereplace,0)             ropereplace,
                                nvl(p.emergothers,0)             emergothers,
                                nvl(p.auxlack,0)                 auxlack,
                                nvl(p.partslack,0)               partslack,
                                nvl(p.othersreason,0)            othersreason,
                                nvl(p.toppoil,0)                 toppoil,
                                nvl(p.itogorg,0)                 itogorg,
                                nvl(p.regauth,0)                 regauth,
                                nvl(p.fuellack,0)                fuellack,
                                nvl(p.survwork,0)                survwork,
                                nvl(p.geowork,0)                 geowork,
                                nvl(p.gobase,0)                  gobase,
                                nvl(p.stafflack,0)               stafflack,
                                nvl(p.orgothers,0)               orgothers,
                                nvl(p.crewlack,0)                crewlack,
                                nvl(p.worklack,0)                worklack,
                                nvl(p.s_kio,0)                   s_kio,
                                nvl(p.s_ktg,0)                   s_ktg,
                                --объем работ--
                                --GMV
                                nvl(gmv.gmvrock,0)+ nvl(gmv.gmvloose,0)+ nvl(gmv.gmvtrans,0) gmv,
                                --GMW
                                nvl(gmw.gmwrock,0)+ nvl(gmw.gmwloose,0)+ nvl(gmw.gmwtrans,0) gmw,
                                nvl(gmv.gmvrock,0)                                           gmvrock,
                                nvl(gmw.gmwrock,0)                                           gmwrock,
                                nvl(gmv.gmvloose,0)                                          gmvloose,
                                nvl(gmw.gmwloose,0)                                          gmwloose,
                                nvl(gmv.gmvtrans,0)                                          gmvtrans,
                                nvl(gmw.gmwtrans,0)                                          gmwtrans,
                                nvl(gmv.gmvrockstrip,0)                                      gmvrockstrip,
                                nvl(gmw.gmwrockstrip,0)                                      gmwrockstrip,
                                nvl(gmv.gmvloosestrip,0)                                     gmvloosestrip,
                                nvl(gmw.gmwloosestrip,0)                                     gmwloosestrip,
                                nvl(gmv.gmvprs,0)                                            gmvprs,
                                nvl(gmw.gmwprs,0)                                            gmwprs,
                                nvl(gmv.gmvtransstrip,0)                                     gmvtransstrip,
                                nvl(gmw.gmwtransstrip,0)                                     gmwtransstrip,
                                nvl(gmv.gmvrockore,0)                                        gmvrockore,
                                nvl(gmw.gmwrockore,0)                                        gmwrockore,
                                nvl(gmv.gmvlooseore,0)                                       gmvlooseore,
                                nvl(gmw.gmwlooseore,0)                                       gmwlooseore,
                                nvl(gmv.gmvtransore,0)                                       gmvtransore,
                                nvl(gmw.gmwtransore,0)                                       gmwtransore,
                                --эксплуатационная производительность
                                --OPVROCKSTRIP
                                CASE
                                           WHEN nvl(wt.wtrockstrip,0)=0 THEN 0
                                           ELSE nvl(gmv.gmvrockstrip,0)*1000/nvl(wt.wtrockstrip,0)
                                END opvrockstrip,
                                --OPWROCKSTRIP
                                CASE
                                           WHEN nvl(wt.wtrockstrip,0)=0 THEN 0
                                           ELSE nvl(gmw.gmwrockstrip,0)*1000/nvl(wt.wtrockstrip,0)
                                END opwrockstrip,
                                --OPVLOOSESTRIP
                                CASE
                                           WHEN nvl(wt.wtloosestrip,0)=0 THEN 0
                                           ELSE nvl(gmv.gmvloosestrip,0)*1000/nvl(wt.wtloosestrip,0)
                                END opvloosestrip,
                                --OPWLOOSESTRIP
                                CASE
                                           WHEN nvl(wt.wtloosestrip,0)=0 THEN 0
                                           ELSE nvl(gmw.gmwloosestrip,0)*1000/nvl(wt.wtloosestrip,0)
                                END opwloosestrip,
                                --OPVPRS
                                CASE
                                           WHEN nvl(wt.wtprs,0)=0 THEN 0
                                           ELSE nvl(gmv.gmvprs,0)*1000/nvl(wt.wtprs,0)
                                END opvprs,
                                --OPWPRS
                                CASE
                                           WHEN nvl(wt.wtprs,0)=0 THEN 0
                                           ELSE nvl(gmw.gmwprs,0)*1000/nvl(wt.wtprs,0)
                                END opwprs,
                                --OPVTRANSSTRIP
                                CASE
                                           WHEN nvl(wt.wttransstrip,0)=0 THEN 0
                                           ELSE nvl(gmv.gmvtransstrip,0)*1000/nvl(wt.wttransstrip,0)
                                END opvtransstrip,
                                --OPWTRANSSTRIP
                                CASE
                                           WHEN nvl(wt.wttransstrip,0)=0 THEN 0
                                           ELSE nvl(gmw.gmwtransstrip,0)*1000/nvl(wt.wttransstrip,0)
                                END opwtransstrip,
                                --OPVROCKORE
                                CASE
                                           WHEN nvl(wt.wtrockore,0)=0 THEN 0
                                           ELSE nvl(gmv.gmvrockore,0)*1000/nvl(wt.wtrockore,0)
                                END opvrockore,
                                --OPWROCKORE
                                CASE
                                           WHEN nvl(wt.wtrockore,0)=0 THEN 0
                                           ELSE nvl(gmw.gmwrockore,0)*1000/nvl(wt.wtrockore,0)
                                END opwrockore,
                                --OPVLOOSEORE
                                CASE
                                           WHEN nvl(wt.wtlooseore,0)=0 THEN 0
                                           ELSE nvl(gmv.gmvlooseore,0)*1000/nvl(wt.wtlooseore,0)
                                END opvlooseore,
                                --OPWLOOSEORE
                                CASE
                                           WHEN nvl(wt.wtlooseore,0)=0 THEN 0
                                           ELSE nvl(gmw.gmwlooseore,0)*1000/nvl(wt.wtlooseore,0)
                                END opwlooseore,
                                --OPVTRANSORE
                                CASE
                                           WHEN nvl(wt.wttransore,0)=0 THEN 0
                                           ELSE nvl(gmv.gmvtransore,0)/nvl(wt.wttransore,0)
                                END opvtransore,
                                --OPWTRANSORE
                                CASE
                                           WHEN nvl(wt.wttransore,0)=0 THEN 0
                                           ELSE nvl(gmw.gmwtransore,0)*1000/nvl(wt.wttransore,0)
                                END opwtransore,
                                --кол-во рейсов
                                nvl(tr.trgm,0)         trgm,
                                nvl(tr.trrockstrip,0)  trrockstrip,
                                nvl(tr.trloosestrip,0) trloosestrip,
                                nvl(tr.trprs,0)        trprs,
                                nvl(tr.trtransstrip,0) trtransstrip,
                                nvl(tr.trrockore,0)    trrockore,
                                nvl(tr.trlooseore,0)   trlooseore,
                                nvl(tr.trtransore,0)   trtransore,
                                --расстояние
                                nvl(rasgm.rasgm,0)       rasgm,
                                nvl(ras.rasrockstrip,0)  rasrockstrip,
                                nvl(ras.rasloosestrip,0) rasloosestrip,
                                nvl(ras.rasprs,0)        rasprs,
                                nvl(ras.rastransstrip,0) rastransstrip,
                                nvl(ras.rasrockore,0)    rasrockore,
                                nvl(ras.raslooseore,0)   raslooseore,
                                nvl(ras.rastransore,0)   rastransore,
                                --Кол-во экскаватора на линии
                                --KOLSHOVLINE
                                (12-nvl(p.itogplanrem,0)-nvl(p.itogemerg,0))/12 kolshovline,
                                --Кол-во экскаватора в работе
                                --KOLSHOVWORK
                                (nvl(wt.wtgm,0)+nvl(wt.wthr,0)+nvl(p.itogtechnol,0))/12 kolshovwork,
                                --грузооборот
                                nvl(gmw.gmwrockstrip,0) *1000*nvl(ras.rasrockstrip,0)  ftrockstrip,
                                nvl(gmw.gmwloosestrip,0)*1000*nvl(ras.rasloosestrip,0) ftloosestrip,
                                nvl(gmw.gmwprs,0)       *1000*nvl(ras.rasprs,0)        ftprs,
                                nvl(gmw.gmwtransstrip,0)*1000*nvl(ras.rastransstrip,0) fttransstrip,
                                nvl(gmw.gmwrockore,0)   *1000*nvl(ras.rasrockore,0)    ftrockore,
                                nvl(gmw.gmwlooseore,0)  *1000*nvl(ras.raslooseore,0)   ftlooseore,
                                nvl(gmw.gmwtransore,0)  *1000*nvl(ras.rastransore,0)   fttransore,
                                --загрузка а/с
                                nvl(avwgm.avwgm,0)         avwgm,
                                nvl(avw.avwrockstrip,0)    avwrockstrip,
                                nvl(avw.avwloosestrip,0)   avwloosestrip,
                                nvl(avw.avwprs,0)          avwprs,
                                nvl(avw.avwtransstrip,0)   avwtransstrip,
                                nvl(avw.avwrockore,0)      avwrockore,
                                nvl(avw.avwlooseore,0)     avwlooseore,
                                nvl(avw.avwtransore,0)     avwtransore,
                                nvl(totalstop.totalidle,0) totalidle,
                                -----------
                                12- ( nvl(wt.wtgm,0)+ nvl(wt.wthr,0)+ nvl(totalstop.totalidle,0) ) balancetime
                                ----------
                     FROM       dispatcher.allvehs t
                     inner join kgp.pto_tech pt
                     ON         pt.controlid = t.controlid
                     AND        pt.category='Экскаваторы'
                     left join  sst
                     ON         sst.tech_key = t.controlid
                     left join  wt
                     ON         wt.tech_key = t.controlid
                     AND        wt.shiftdate=sst.shiftdate
                     AND        wt.shiftnum=sst.shiftnum
                     left join  p
                     ON         p.tech_key=t.controlid
                     AND        p.shiftdate=sst.shiftdate
                     AND        p.shiftnum=sst.shiftnum
                     left join  gmv
                     ON         gmv.tech_key=t.controlid
                     AND        gmv.shiftdate=sst.shiftdate
                     AND        gmv.shiftnum=sst.shiftnum
                     left join  gmw
                     ON         gmw.tech_key=t.controlid
                     AND        gmw.shiftdate=sst.shiftdate
                     AND        gmw.shiftnum=sst.shiftnum
                     left join  tr
                     ON         tr.tech_key=t.controlid
                     AND        tr.shiftdate=sst.shiftdate
                     AND        tr.shiftnum=sst.shiftnum
                     left join  rasgm
                     ON         rasgm.tech_key=t.controlid
                     AND        rasgm.shiftdate=sst.shiftdate
                     AND        rasgm.shiftnum=sst.shiftnum
                     left join  ras
                     ON         ras.tech_key=t.controlid
                     AND        ras.shiftdate=sst.shiftdate
                     AND        ras.shiftnum=sst.shiftnum
                     left join  avw
                     ON         avw.tech_key=t.controlid
                     AND        avw.shiftdate=sst.shiftdate
                     AND        avw.shiftnum=sst.shiftnum
                     left join  avwgm
                     ON         avwgm.tech_key=t.controlid
                     AND        avwgm.shiftdate=sst.shiftdate
                     AND        avwgm.shiftnum=sst.shiftnum
                     left join  totalstop
                     ON         totalstop.tech_key=t.controlid
                     AND        totalstop.shiftdate=sst.shiftdate
                     AND        totalstop.shiftnum=sst.shiftnum
                     left join  shmh
                     ON         shmh.tech_key=t.controlid
                     AND        shmh.shiftdate=sst.shiftdate
                     AND        shmh.shiftnum=sst.shiftnum
                     left join  horq
                     ON         horq.tech_key=t.controlid
                     AND        horq.shiftdate=sst.shiftdate
                     AND        horq.shiftnum=sst.shiftnum ) src
left join
          (
                 SELECT *
                 FROM   shovels
                 WHERE  site='ТОО Комаровское' )smsv
ON        src.techid=smsv.shovid
WHERE     (
                    techid = :paramSelectTechId
          OR        :paramSelectTechId = 'Все')
GROUP BY  model,
          techid,
          category
ORDER BY  techid,
          model,
          category





";
        }
        public static string Get_Bogdan_custom_OperPoroda_Shov2()
        {
            return @"



select  category   AS category,
          model    model,
          techid AS tech_id,
          SUM(round((gmvrockore + gmvlooseore + gmvtransore) * 1000, 5)) AS val_rud,
          /* вскрыша скальная м3 */
          SUM(round(gmvrockstrip * 1000, 5)) AS val_sk,
          /* вскрыша скальная м3 */
          /* вскрыша транзитная м3 */
          SUM(round(gmvtransstrip * 1000, 5)) AS val_tr,
          /* вскрыша транзитная м3 */
          /* вскрыша рыхлая м3 */
          SUM(round(gmvloosestrip * 1000, 5)) AS val_rih,
          /* вскрыша рыхлая м3 */
          /* ПРС м3 */
          SUM(round(gmvprs * 1000, 5)) AS val_prs
 from (with
sst as
(
select
d.controlid tech_key,
d.shovid,
sst.task_date shiftdate,
sst.shift shiftnum
from shov_shift_tasks sst 
inner JOIN DISPATCHER.shovels d ON d.SHOVID = sst.SHOV_ID and d.site='ТОО Комаровское' 
where
 ((sst.task_date = :ParamDateFrom and sst.Shift >= :ParamShiftFrom)or(sst.task_date > :ParamDateFrom))
            and ((sst.task_date = :ParamDateTo and :ParamShiftTo >= sst.Shift)or(:ParamDateTo > sst.task_date ))
),
stpgs AS ( -- Простои берем из отчетов по завершению смены
SELECT d.CONTROLID TECH_KEY,
  d.shovid,
  s.shiftdate,
  s.shiftnum,
  psc.POLY_STOP_CAT_NAME CATEGORY, 
  s.TIMEGO,
  s.TIMESTOP
  FROM DISPATCHER.SHIFTSTOPPAGES_SHOV s
  inner JOIN DISPATCHER.shovels d ON d.SHOVID = s.VEHID and d.site='ТОО Комаровское' 
  inner JOIN DISPATCHER.POLY_USER_STOPPAGES_SHOV ps ON ps.POLY_STOP_BINDINGS_ID = 104 AND (ps.CODE = s.IDLESTOPTYPE AND ps.POLY_STOP_CAT_ID IS NOT NULL)
  inner JOIN DISPATCHER.POLY_STOP_CATEGORIES psc ON psc.POLY_STOP_CAT_ID = ps.POLY_STOP_CAT_ID
  WHERE 
  ((shiftDate = :ParamDateFrom and Shiftnum >= :ParamShiftFrom)or(shiftDate > :ParamDateFrom))
            and ((shiftDate = :ParamDateTo and :ParamShiftTo >= Shiftnum)or(:ParamDateTo > shiftDate ))
   and (s.TIMEGO - s.TIMESTOP) * 24 * 60 >=5   
     and s.TIMESTOP is not null and s.TIMEGO is not null
  and psc.POLY_STOP_CAT_NAME is not null
  
  ),
  
detalq AS
(
select
selres.tech_key,
selres.shovid,
selres.SHIFTDATE,
selres.SHIFTNUM,
psc.POLY_WORK_CAT_NAME CATEGORY,
sum(selres.detaltime) detaltime,
sum(idletime) idletime,
sum(selres.detaltime-selres.idletime) detaldur

from
(select
sel.tech_key,
sel.shovid,
sel.SHIFTDATE,
sel.SHIFTNUM,
sel.worktype,
sum((sel.FINISH_TIME_MANUAL-sel.START_TIME_MANUAL)*24) detaltime,

    sum(
    NVL(
    (SELECT
    SUM(
            NVL(
            (least(stpgs.TIMEGO,sel.FINISH_TIME_MANUAL) - 
            greatest(sel.START_TIME_MANUAL,stpgs.TIMESTOP)
            ),0) * 24)  IDLE_TIME 
    FROM
    stpgs
    WHERE 
              (stpgs.TIMESTOP BETWEEN sel.START_TIME_MANUAL AND sel.FINISH_TIME_MANUAL
              or stpgs.TIMEGO BETWEEN sel.START_TIME_MANUAL AND sel.FINISH_TIME_MANUAL)
              and stpgs.shovid=sel.shovid
              and sel.shiftdate=stpgs.shiftdate and sel.shiftnum=stpgs.shiftnum
    
    )
    ,0)
    ) idletime
   
from 
(SELECT d.CONTROLID TECH_KEY,
  d.shovid,
  st.TASK_DATE shiftdate,
  st.SHIFT shiftnum,
  ssra.worktype,
  ssra.START_TIME_MANUAL, 
  ssra.FINISH_TIME_MANUAL
  FROM 
  shov_shift_tasks st 
  inner join shov_shift_reports ssr on ssr.TASK_ID=st.ID
  inner join shov_shift_reports_adv ssra on ssra.REPORT_ID=ssr.ID		
  inner JOIN DISPATCHER.shovels d ON d.SHOVID = st.SHOV_ID and d.site='ТОО Комаровское' 
  WHERE 
  ((st.TASK_DATE = :ParamDateFrom and st.SHIFT >= :ParamShiftFrom)or(st.TASK_DATE > :ParamDateFrom))
            and ((st.TASK_DATE = :ParamDateTo and :ParamShiftTo >= st.SHIFT)or(:ParamDateTo > st.TASK_DATE ))
   and ssra.FINISH_TIME_MANUAL is not null
   and ssra.START_TIME_MANUAL is not null 
) sel          
group by 
sel.tech_key,
sel.shovid,
sel.worktype,
sel.shiftdate,
sel.shiftnum 
)selres 
inner JOIN DISPATCHER.POLY_USER_WORKS_SHOV ps ON ps.POLY_WORK_BINDINGS_ID = 162 
AND (ps.ID = KGP_SHOVWTTOWTID(selres.WORKTYPE) 
AND ps.POLY_WORK_CAT_ID IS NOT NULL)
inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
 
group by 
selres.tech_key,
  selres.shovid,
  selres.shiftdate,
  selres.shiftnum, 
  psc.POLY_WORK_CAT_NAME
),
---------------простои экскаваторов------------------------
s AS ( -- Простои берем из отчетов по завершению смены
SELECT s.TECH_KEY,
  s.shovid,
  s.shiftdate,
  s.shiftnum,
  s.CATEGORY, 
  SUM((s.TIMEGO - s.TIMESTOP) * 24) TIME
  FROM stpgs s   
  GROUP BY s.TECH_KEY,
  s.shovid,
  s.shiftdate,
  s.shiftnum, 
  s.CATEGORY
  
  union all

SELECT d.TECH_KEY,
  d.shovid,
  d.shiftdate,
  d.shiftnum,
  d.CATEGORY, 
  SUM(d.DETALDUR) TIME
  FROM detalq d
  where d.CATEGORY in(
   'перегон',
   'подготовка подъезда',
   'зачистка забоя',
    'чистка кузова',
    'чистка ковша',
	'перегон на ВР',
	'перегон на ремонт',
	'перецепка кабеля на ВР',
	'перемещение по забою'
   )
  group by d.TECH_KEY,
  d.shovid,
  d.shiftdate,
  d.shiftnum,
  d.CATEGORY
  ),
 totalstop
 as
 (
 select
 TECH_KEY,
 SHIFTDATE,
 SHIFTNUM,
 SUM(TIME) TOTALIDLE
 from s
 where 
 CATEGORY is not null
 group by 
 TECH_KEY,
 SHIFTDATE,
 SHIFTNUM 
 ),
 psub AS ( -- Транспонируем таблицу с простоями
  SELECT * FROM s
  PIVOT (SUM(TIME) FOR CATEGORY IN (
    'ТР' TR,
    'Т1,Т2,Т3,Т4,Т5' SERVICE, 
    'КР' KR,
    'Обед' DINNER,
    'Прием/передача смены' BREAKS,
    'ЕТО' ETO,
    'заправка(ДТ,вода)' REFUEL,
    'перегон' RELOCATION,
    'личные нужды' PERSNEED,
    'перемещение по забою' MOVEBLOCK,
    'ожидание а/с' WAITTRUCK,
    'подготовка подъезда' PREPENTRANCE,
    'зачистка забоя' ZABOICLEAN,
    'чистка кузова' BODYCLEAN,
    'чистка ковша' BUCKETCLEAN,
	'ВЗРЫВНЫЕ РАБОТЫ' VR,
	'перегон на ВР' VRRELOC,
	'ТЕХНИЧЕСКИЙ ПЕРЕРЫВ' TECHPER,
	'Прием/передача смены ХотСит' BREAKSHOTSEAT,
	'Климатические условия' WEATHER,
	'Рем.элек.оборуд.' ELECTRICAL,
	'ДВС' DVS,
	'Трансмиссия' TRANSMISSION,
	'Ходовая часть' CHASSIS,
	'Навесное оборудование' HINGE,
	'ремонт а/ш' TIRES,
	'Гидравлическая часть' HYDRAULIC,
	'перегон на ремонт' REPAIRRELOC,
	'Наладочные работы' ADJUSTMENT,
	'Механизм поворота' TURNMECH,
	'Перепасовка каната' ROPESHUFFLE,
	'Замена кабеля и перемычек' CABLEREPLACE,
	'Замена каната' ROPEREPLACE,
	'Аварийные прочие' EMERGOTHERS, 
    'отсутствие вспомогательной техники' AUXLACK, 
    'Отсут.зап.частей' PARTSLACK,
    'Прочие' OTHERSREASON,
    'доливка масла/антифриза' TOPPOIL,
    'Остановка контролирующими органами' REGAUTH,
    'Отсутствие диз.топлива' FUELLACK,
    'Работа с маркшейдерами' SURVWORK,
    'Работа с геологами' GEOWORK,
    'Очистка ходовой базы' GOBASE,
    'отсутствие оператора' STAFFLACK,
    'Организац.прочие' ORGOTHERS,
    'Отсутствие экипажа (без бригады)' CREWLACK,
    'Отсут.фронта работ' WORKLACK,
	'перецепка кабеля на ВР' RECONCABLE 

  ))
),
p as 
(
SELECT
psub.TECH_KEY,
psub.shovid,
psub.SHIFTDATE,
psub.SHIFTNUM,

psub.TR,
psub.SERVICE,
psub.KR,

psub.DINNER,
psub.BREAKS,
psub.ETO,
psub.REFUEL,
psub.RELOCATION,
psub.PERSNEED,
psub.MOVEBLOCK,
psub.WAITTRUCK,
psub.PREPENTRANCE,
psub.ZABOICLEAN,
psub.BODYCLEAN,
psub.BUCKETCLEAN,
psub.VR,
psub.VRRELOC,
psub.TECHPER,
psub.BREAKSHOTSEAT,
psub.WEATHER,
psub.ELECTRICAL,
psub.DVS,
psub.TRANSMISSION,
psub.CHASSIS,
psub.HINGE,
psub.TIRES,
psub.HYDRAULIC,
psub.REPAIRRELOC,
psub.ADJUSTMENT,
psub.TURNMECH,
psub.ROPESHUFFLE,
psub.CABLEREPLACE,
psub.ROPEREPLACE,
psub.EMERGOTHERS,
psub.AUXLACK,
psub.PARTSLACK,
psub.OTHERSREASON,
psub.TOPPOIL,

psub.REGAUTH,
psub.FUELLACK,
psub.SURVWORK,
psub.GEOWORK,
psub.GOBASE,
psub.STAFFLACK,
psub.ORGOTHERS,
psub.CREWLACK,
psub.WORKLACK,
psub.RECONCABLE,

--ITOGPLANREM
NVL(psub.TR,0)+
NVL(psub.SERVICE,0)+
NVL(psub.KR,0) ITOGPLANREM,

--ITOGTECHNOL
NVL(psub.DINNER,0)+
NVL(psub.BREAKS,0)+
NVL(psub.ETO,0)+
NVL(psub.REFUEL,0)+
NVL(psub.RELOCATION,0)+
NVL(psub.PERSNEED,0)+
NVL(psub.MOVEBLOCK,0)+
NVL(psub.WAITTRUCK,0)+
NVL(psub.PREPENTRANCE,0)+
NVL(psub.ZABOICLEAN,0)+
NVL(psub.BODYCLEAN,0)+
NVL(psub.BUCKETCLEAN,0)+
NVL(psub.VR,0)+
NVL(psub.VRRELOC,0)+
NVL(psub.TECHPER,0)+
NVL(psub.BREAKSHOTSEAT,0)+
NVL(psub.RECONCABLE,0) ITOGTECHNOL,

--ITOGEMERG
NVL(psub.ELECTRICAL,0)+
NVL(psub.DVS,0)+
NVL(psub.TRANSMISSION,0)+
NVL(psub.CHASSIS,0)+
NVL(psub.HINGE,0)+
NVL(psub.TIRES,0)+
NVL(psub.HYDRAULIC,0)+
NVL(psub.REPAIRRELOC,0)+
NVL(psub.ADJUSTMENT,0)+
NVL(psub.TURNMECH,0)+
NVL(psub.ROPESHUFFLE,0)+
NVL(psub.CABLEREPLACE,0)+
NVL(psub.ROPEREPLACE,0)+
NVL(psub.EMERGOTHERS,0)+
NVL(psub.AUXLACK,0)+
NVL(psub.PARTSLACK,0)+
NVL(psub.OTHERSREASON,0)+
NVL(psub.TOPPOIL,0) ITOGEMERG,

--ITOGORG
NVL(psub.REGAUTH,0)+
NVL(psub.FUELLACK,0)+
NVL(psub.SURVWORK,0)+
NVL(psub.GEOWORK,0)+
NVL(psub.GOBASE,0)+
NVL(psub.STAFFLACK,0)+
NVL(psub.ORGOTHERS,0)+
NVL(psub.CREWLACK,0)+
NVL(psub.WORKLACK,0) ITOGORG,
NVL(psub.DINNER,0)+
NVL(psub.BREAKS,0)+
NVL(psub.ETO,0)+
NVL(psub.REFUEL,0)+
NVL(psub.RELOCATION,0)+
NVL(psub.PERSNEED,0)+
NVL(psub.MOVEBLOCK,0)+
NVL(psub.WAITTRUCK,0)+
NVL(psub.PREPENTRANCE,0)+
NVL(psub.ZABOICLEAN,0)+
NVL(psub.BODYCLEAN,0)+
NVL(psub.BUCKETCLEAN,0)+
NVL(psub.VR,0)+
NVL(psub.VRRELOC,0)+
NVL(psub.TECHPER,0)+
NVL(psub.BREAKSHOTSEAT,0)+
NVL(psub.WEATHER,0)+
NVL(psub.REGAUTH,0)+
NVL(psub.FUELLACK,0)+
NVL(psub.SURVWORK,0)+
NVL(psub.GEOWORK,0)+
NVL(psub.GOBASE,0)+
NVL(psub.STAFFLACK,0)+
NVL(psub.ORGOTHERS,0)+
NVL(psub.CREWLACK,0)+
NVL(psub.WORKLACK,0)+
NVL(psub.RECONCABLE,0) s_kio,

NVL(psub.TR,0)+
NVL(psub.SERVICE,0)+
NVL(psub.KR,0)+
NVL(psub.ELECTRICAL,0)+
NVL(psub.DVS,0)+
NVL(psub.TRANSMISSION,0)+
NVL(psub.CHASSIS,0)+
NVL(psub.HINGE,0)+
NVL(psub.TIRES,0)+
NVL(psub.HYDRAULIC,0)+
NVL(psub.REPAIRRELOC,0)+
NVL(psub.ADJUSTMENT,0)+
NVL(psub.TURNMECH,0)+
NVL(psub.ROPESHUFFLE,0)+
NVL(psub.CABLEREPLACE,0)+
NVL(psub.ROPEREPLACE,0)+
NVL(psub.EMERGOTHERS,0)+
NVL(psub.AUXLACK,0)+
NVL(psub.PARTSLACK,0)+
NVL(psub.OTHERSREASON,0)+
NVL(psub.TOPPOIL,0) s_ktg

FROM psub
),
detal as
(
SELECT TECH_KEY,
  shovid,
  shiftdate,
  shiftnum,
  CATEGORY, 
  sum(detaldur) TIME
  FROM 
  detalq
  where
	category in (
	'дренажные работы',
	'доборы',
	'постановка борта в конеч.положение',
	'переэкскавация',
	'с разборкой забоя',
	'хоз.работы прочие'
	)
  group by TECH_KEY,
  shovid,
  shiftdate,
  shiftnum,
  CATEGORY   
  
),
totaldetal as
 (
 select
 TECH_KEY,
 SHIFTDATE,
 SHIFTNUM,
 SUM(TIME) TOTALDETAL
 from detal
 where 
 CATEGORY is not null
 group by 
 TECH_KEY,
 SHIFTDATE,
 SHIFTNUM 
 ),
wtdetal as
(
SELECT * FROM detal
  PIVOT (SUM(TIME) FOR CATEGORY IN (
    'дренажные работы' WTDRGHR, 
	'доборы' WTDOBORHR,
	'постановка борта в конеч.положение' WTPOSTBORTHR,
	'переэкскавация' WTRESHOVHR,
	'с разборкой забоя' WTRAZBZABHR,
	'хоз.работы прочие' WTOTHERSHR
  ))
),
ttlstpdetal as
(
select
TECH_KEY,
shiftdate,
shiftnum,
sum(TIME) ALLSTPDETALTIME
from
(select
TECH_KEY,
shiftdate,
shiftnum,
TOTALDETAL TIME
from totaldetal
union all
select
TECH_KEY,
shiftdate,
shiftnum,
TOTALIDLE TIME
from totalstop
)
group by 
TECH_KEY,
shiftdate,
shiftnum
),
--------кол-во рейсов, вес, объем--------
tripssra AS
(
SELECT d.CONTROLID TECH_KEY,
  d.shovid,
  sra.TASKDATE shiftdate,
  sra.SHIFT shiftnum,
  psc.POLY_WORK_CAT_NAME CATEGORY, 
  SUM(sra.TRIPNUMBERMANUAL) TRIPSKOL
  FROM 
  shiftreportsadv sra
  inner JOIN DISPATCHER.shovels d ON d.SHOVID = sra.SHOVID and d.site='ТОО Комаровское' 
  inner JOIN DISPATCHER.POLY_USER_WORKS_DUMP ps ON ps.POLY_WORK_BINDINGS_ID = 161 AND (ps.ID = KGP_DUMPWTTOWTID(sra.WORKTYPE) AND ps.POLY_WORK_CAT_ID IS NOT NULL)
  inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
  WHERE 
  ((sra.TASKDATE = :ParamDateFrom and sra.SHIFT >= :ParamShiftFrom)or(sra.TASKDATE > :ParamDateFrom))
            and ((sra.TASKDATE = :ParamDateTo and :ParamShiftTo >= sra.SHIFT)or(:ParamDateTo > sra.TASKDATE ))
AND 
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         )			
   
  GROUP BY d.CONTROLID,
  d.shovid,
  sra.TASKDATE,
  sra.SHIFT, 
  psc.POLY_WORK_CAT_NAME
),
weightsra AS
(
SELECT d.CONTROLID TECH_KEY,
  d.shovid,
  selres.shiftdate,
  selres.shiftnum,
  psc.POLY_WORK_CAT_NAME CATEGORY, 
  SUM(selres.tripnumbermanual*selres.avweight)/1000 WEIGHT
  FROM 
  (
select
sra.shovid,
sra.taskdate SHIFTDATE,
sra.shift SHIFTNUM,
wt.ID WORKTYPE_ID,
sra.TRIPNUMBERMANUAL,
CASE WHEN NVL(sra.AVWEIGHT,0) = 0 THEN sra.WEIGHTRATE ELSE sra.AVWEIGHT END AVWEIGHT
from shiftreportsadv sra inner join
worktypes wt on sra.WORKTYPE=wt.NAME
where 

        ((sra.taskDate = :ParamDateFrom and sra.Shift >= :ParamShiftFrom)or(sra.taskDate > :ParamDateFrom))
            and ((sra.taskDate = :ParamDateTo and :ParamShiftTo >= sra.Shift)or(:ParamDateTo > sra.taskDate ))
AND 
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         ) 
)selres
  inner JOIN DISPATCHER.shovels d ON d.SHOVID = selres.SHOVID and d.site='ТОО Комаровское' 
  inner JOIN DISPATCHER.POLY_USER_WORKS_DUMP ps ON ps.POLY_WORK_BINDINGS_ID = 161 AND (ps.ID = selres.WORKTYPE_ID AND ps.POLY_WORK_CAT_ID IS NOT NULL)
  inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
  GROUP BY d.CONTROLID,
  d.shovid,
  selres.SHIFTDATE,
  selres.SHIFTNUM, 
  psc.POLY_WORK_CAT_NAME
),
volsra AS
(
SELECT d.CONTROLID TECH_KEY,
  d.shovid,
  selres.shiftdate,
  selres.shiftnum,
  psc.POLY_WORK_CAT_NAME CATEGORY, 
  SUM(decode(nvl(selres.avweight,0),0,0,selres.avweight*selres.tripnumbermanual/nvl(decode(selres.weightrate,0,selres.avweight,selres.weightrate),selres.avweight)*selres.volumerate))/1000 VOL
  FROM 
  (
select
sra.shovid,
sra.taskdate SHIFTDATE,
sra.shift SHIFTNUM,
wt.ID WORKTYPE_ID,
sra.TRIPNUMBERMANUAL,
CASE WHEN NVL(sra.AVWEIGHT,0) = 0 THEN sra.WEIGHTRATE ELSE sra.AVWEIGHT END AVWEIGHT,
sra.WEIGHTRATE,
sra.VOLUMERATE
from shiftreportsadv sra inner join
worktypes wt on sra.WORKTYPE=wt.NAME
where 
 ((sra.taskDate = :ParamDateFrom and sra.Shift >= :ParamShiftFrom)or(sra.taskDate > :ParamDateFrom))
  and ((sra.taskDate = :ParamDateTo and :ParamShiftTo >= sra.Shift)or(:ParamDateTo > sra.taskDate ))
AND 
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         )  
)selres
  inner JOIN DISPATCHER.shovels d ON d.SHOVID = selres.SHOVID and d.site='ТОО Комаровское' 
  inner JOIN DISPATCHER.POLY_USER_WORKS_DUMP ps ON ps.POLY_WORK_BINDINGS_ID = 161 AND (ps.ID = selres.WORKTYPE_ID AND ps.POLY_WORK_CAT_ID IS NOT NULL)
  inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
  GROUP BY d.CONTROLID,
  d.shovid,
  selres.SHIFTDATE,
  selres.SHIFTNUM, 
  psc.POLY_WORK_CAT_NAME
),
wtsra as
(
-------транспонируем рейсы-------
SELECT * FROM tripssra
  PIVOT (SUM(TRIPSKOL) FOR CATEGORY IN (
    'вскрыша скальная' WTROCKSTRIP, 
	'вскрыша рыхлая' WTLOOSESTRIP,
	'ПРС в контуре карьера' WTPRS,
	'вскрыша транзитная' WTTRANSSTRIP,
	'руда скальная' WTROCKORE,
	'руда рыхлая' WTLOOSEORE,
	'руда транзитная' WTTRANSORE,
	'ВКП' WTIPTHR,
	'ВСП' WTIWTHR,
    'ПРС вне контура карьера' WTPRSOUTHR,
    'хоз.работы прочие' WTSNEG
  ))
),
wtsub as (
select 
sel.tech_key,
sel.shovid,
sel.shiftdate,
sel.shiftnum,
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTROCKSTRIP,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTROCKSTRIP, ----
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTLOOSESTRIP,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTLOOSESTRIP,----
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTPRS,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTPRS,---
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTTRANSSTRIP,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTTRANSSTRIP, --
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTROCKORE,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTROCKORE, ---
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTLOOSEORE,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTLOOSEORE,--
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTTRANSORE,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTTRANSORE, --
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTIPTHR,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTIPTHR,---
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTIWTHR,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTIWTHR,---
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTPRSOUTHR,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTPRSOUTHR,
(case when (NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)=0 then 0 
else
NVL(sel.WTSNEG,0)/
(NVL(sel.WTROCKSTRIP,0)+
NVL(sel.WTLOOSESTRIP,0)+
NVL(sel.WTPRS,0)+
NVL(sel.WTTRANSSTRIP,0)+
NVL(sel.WTROCKORE,0)+
NVL(sel.WTLOOSEORE,0)+
NVL(sel.WTTRANSORE,0)+
NVL(sel.WTIPTHR,0)+
NVL(sel.WTIWTHR,0)+
NVL(sel.WTPRSOUTHR,0)+
NVL(sel.WTSNEG,0)
)
end * sel.TOTALWORKTIME
) WTOTHERSHR,
NVL(sel.ALLSTPDETALTIME,0) ALLSTPDETALTIME,
sel.TOTALWORKTIME
from
(select
wtsra.tech_key,
wtsra.shovid,
wtsra.shiftdate,
wtsra.shiftnum,
wtsra.WTROCKSTRIP, 
wtsra.WTLOOSESTRIP,
wtsra.WTPRS,
wtsra.WTTRANSSTRIP,
wtsra.WTROCKORE,
wtsra.WTLOOSEORE,
wtsra.WTTRANSORE,
wtsra.WTIPTHR,
wtsra.WTIWTHR,
wtsra.WTPRSOUTHR,
wtsra.WTSNEG,
ttlstpdetal.ALLSTPDETALTIME,
12-NVL(ttlstpdetal.ALLSTPDETALTIME,0) TOTALWORKTIME
from wtsra
left join ttlstpdetal on wtsra.tech_key=ttlstpdetal.tech_key and wtsra.shiftdate=ttlstpdetal.shiftdate and wtsra.shiftnum=ttlstpdetal.shiftnum
)sel
),
wt as 
(
SELECT
wtsub.TECH_KEY,
wtsub.shovid,
wtsub.SHIFTDATE,
wtsub.SHIFTNUM,
--WTGM
NVL(wtsub.WTROCKSTRIP,0)+
NVL(wtsub.WTLOOSESTRIP,0)+
NVL(wtsub.WTPRS,0)+
NVL(wtsub.WTTRANSSTRIP,0)+
NVL(wtsub.WTROCKORE,0)+
NVL(wtsub.WTLOOSEORE,0)+
NVL(wtsub.WTTRANSORE,0) WTGM,

wtsub.WTROCKSTRIP,
wtsub.WTLOOSESTRIP,
wtsub.WTPRS,
wtsub.WTTRANSSTRIP,
wtsub.WTROCKORE,
wtsub.WTLOOSEORE,
wtsub.WTTRANSORE,

--WTHR
NVL(wtdetal.WTDRGHR,0)+
NVL(wtdetal.WTDOBORHR,0)+
NVL(wtsub.WTIPTHR,0)+
NVL(wtsub.WTIWTHR,0)+
NVL(wtdetal.WTPOSTBORTHR,0)+
NVL(wtdetal.WTRESHOVHR,0)+
NVL(wtdetal.WTRAZBZABHR,0)+
NVL(wtsub.WTPRSOUTHR,0)+
NVL(wtsub.WTOTHERSHR,0)+
NVL(wtdetal.WTOTHERSHR,0)
 WTHR,

wtdetal.WTDRGHR,
wtdetal.WTDOBORHR,
wtsub.WTIPTHR,
wtsub.WTIWTHR,
wtdetal.WTPOSTBORTHR,
wtdetal.WTRESHOVHR,
wtdetal.WTRAZBZABHR,
wtsub.WTPRSOUTHR,
NVL(wtsub.WTOTHERSHR,0)+
NVL(wtdetal.WTOTHERSHR,0) WTOTHERSHR

FROM wtsub 
left join wtdetal on wtdetal.tech_key=wtsub.tech_key and wtdetal.shiftdate=wtsub.shiftdate and wtdetal.shiftnum=wtsub.shiftnum 
),
-----------моточасы экск--
shmh as
(
select
d.controlid TECH_KEY,
d.shovid,
shiftdate,
shiftnum,
NVL(sel.MH,0) MH
from
(select
selres.shovid,
selres.SHIFTDATE,
selres.SHIFTNUM,
SUM(selres.MOTOHOURSEND-selres.MOTOHOURSBEGIN) MH
from
(
select
sst.SHOV_ID shovid,
sst.TASK_DATE SHIFTDATE,
sst.shift SHIFTNUM,
ssr.MOTOHOURSBEGIN,
ssr.MOTOHOURSEND
from shov_shift_reports ssr 
inner join shov_shift_tasks sst on sst.id=ssr.TASK_ID
where 
 ((task_date = :ParamDateFrom and shift >= :ParamShiftFrom)or(task_date > :ParamDateFrom))
  and ((task_date = :ParamDateTo and :ParamShiftTo >= shift)or(:ParamDateTo > task_date ))
)selres 
group by 
  selres.shovid,
  selres.shiftdate,
  selres.shiftnum 
 )sel inner join shovels d on d.shovid = sel.shovid and d.site='ТОО Комаровское'

),
-----горизонты г.м.------
horq AS
(
select
d.controlid TECH_KEY,
d.shovid,
shiftdate,
shiftnum,
sel.hors HORIZONT
from
(select
selres.SHOVID,
selres.SHIFTDATE,
selres.SHIFTNUM,
selres.hors
from
(
select
sra.shovid,
sra.taskdate SHIFTDATE,
sra.shift SHIFTNUM,
listagg(sra.area,', ') within group(order by sra.area) hors 
from 
(select
shovid,
taskdate,
shift,
area
from shiftreportsadv
WHERE
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         )
group by
shovid,
taskdate,
shift,
area
) sra
where 
 ((sra.taskDate = :ParamDateFrom and sra.Shift >= :ParamShiftFrom)or(sra.taskDate > :ParamDateFrom))
  and ((sra.taskDate = :ParamDateTo and :ParamShiftTo >= sra.Shift)or(:ParamDateTo > sra.taskDate ))
  
group by sra.shovid,
sra.taskdate,
sra.shift
)selres 
group by 
  selres.shovid,
  selres.shiftdate,
  selres.shiftnum, 
  selres.hors
 )sel 
 inner join shovels d on d.shovid = sel.shovid and d.site='ТОО Комаровское'
),
gmwsub AS
(
-------транспонируем рейсы-------
SELECT * FROM weightsra
  PIVOT (SUM(WEIGHT) FOR CATEGORY IN (
    'вскрыша скальная' GMWROCKSTRIP, 
	'вскрыша рыхлая' GMWLOOSESTRIP,
	'ПРС в контуре карьера' GMWPRS,
	'вскрыша транзитная' GMWTRANSSTRIP,
	'руда скальная' GMWROCKORE,
	'руда рыхлая' GMWLOOSEORE,
	'руда транзитная' GMWTRANSORE
  ))
),
gmw as
(
select
gmwsub.TECH_KEY,
gmwsub.shovid,
gmwsub.SHIFTDATE,
gmwsub.SHIFTNUM,
--GMWROCK
NVL(gmwsub.GMWROCKSTRIP,0)+
NVL(gmwsub.GMWROCKORE,0) GMWROCK,
--GMWLOOSE
NVL(gmwsub.GMWLOOSESTRIP,0)+
NVL(gmwsub.GMWPRS,0)+
NVL(gmwsub.GMWLOOSEORE,0) GMWLOOSE,
--GMWTRANS
NVL(gmwsub.GMWTRANSSTRIP,0)+
NVL(gmwsub.GMWTRANSORE,0) GMWTRANS,
GMWROCKSTRIP,
GMWLOOSESTRIP,
GMWPRS,
GMWTRANSSTRIP,
GMWROCKORE,
GMWLOOSEORE,
GMWTRANSORE
from gmwsub
),
gmvsub AS
(
-------транспонируем рейсы-------
SELECT * FROM volsra
  PIVOT (SUM(VOL) FOR CATEGORY IN (
    'вскрыша скальная' GMVROCKSTRIP, 
	'вскрыша рыхлая' GMVLOOSESTRIP,
	'ПРС в контуре карьера' GMVPRS,
	'вскрыша транзитная' GMVTRANSSTRIP,
	'руда скальная' GMVROCKORE,
	'руда рыхлая' GMVLOOSEORE,
	'руда транзитная' GMVTRANSORE
  ))
),
gmv as
(
select
gmvsub.TECH_KEY,
gmvsub.shovid,
gmvsub.SHIFTDATE,
gmvsub.SHIFTNUM,
--GMVROCK
NVL(gmvsub.GMVROCKSTRIP,0)+
NVL(gmvsub.GMVROCKORE,0) GMVROCK,
--GMVLOOSE
NVL(gmvsub.GMVLOOSESTRIP,0)+
NVL(gmvsub.GMVPRS,0)+
NVL(gmvsub.GMVLOOSEORE,0) GMVLOOSE,
--GMVTRANS
NVL(gmvsub.GMVTRANSSTRIP,0)+
NVL(gmvsub.GMVTRANSORE,0) GMVTRANS,
GMVROCKSTRIP,
GMVLOOSESTRIP,
GMVPRS,
GMVTRANSSTRIP,
GMVROCKORE,
GMVLOOSEORE,
GMVTRANSORE
from gmvsub
),
trsub as
(
-------транспонируем рейсы-------
SELECT * FROM tripssra
  PIVOT (SUM(TRIPSKOL) FOR CATEGORY IN (
    'вскрыша скальная' TRROCKSTRIP, 
	'вскрыша рыхлая' TRLOOSESTRIP,
	'ПРС в контуре карьера' TRPRS,
	'вскрыша транзитная' TRTRANSSTRIP,
	'руда скальная' TRROCKORE,
	'руда рыхлая' TRLOOSEORE,
	'руда транзитная' TRTRANSORE
  ))
),
tr as
(
select
trsub.TECH_KEY,
trsub.shovid,
trsub.SHIFTDATE,
trsub.SHIFTNUM,
--TRGM
NVL(trsub.TRROCKSTRIP,0)+
NVL(trsub.TRLOOSESTRIP,0)+
NVL(trsub.TRPRS,0)+
NVL(trsub.TRTRANSSTRIP,0)+
NVL(trsub.TRROCKORE,0)+
NVL(trsub.TRLOOSEORE,0)+
NVL(trsub.TRTRANSORE,0) TRGM,
trsub.TRROCKSTRIP,
trsub.TRLOOSESTRIP,
trsub.TRPRS,
trsub.TRTRANSSTRIP,
trsub.TRROCKORE,
trsub.TRLOOSEORE,
trsub.TRTRANSORE
from trsub
),
-------------------------------ср.взв.расстояние г.м.------
rasgmq AS
(
select
d.controlid TECH_KEY,
d.shovid,
sel.shiftdate,
sel.shiftnum,
sel.poly_work_cat_name CATEGORY,
NVL(sel.length,0) LENGTH
from
(select
selres.shovid,
selres.SHIFTDATE,
selres.SHIFTNUM,
psc.POLY_WORK_CAT_NAME,
decode(sum(selres.tripnumbermanual),0,0,sum(selres.tripnumbermanual*selres.AVLENGTH)/sum(selres.tripnumbermanual)) LENGTH
from
(
select
sra.shovid,
sra.taskdate SHIFTDATE,
sra.shift SHIFTNUM,
wt.ID WORKTYPE_ID,
sra.TRIPNUMBERMANUAL,
CASE WHEN NVL(sra.AVLENGTH,0) = 0 THEN sra.LENGTHMANUAL ELSE sra.AVLENGTH END AVLENGTH 
from shiftreportsadv sra inner join
worktypes wt on sra.WORKTYPE=wt.NAME
where 
 ((sra.taskDate = :ParamDateFrom and sra.Shift >= :ParamShiftFrom)or(sra.taskDate > :ParamDateFrom))
  and ((sra.taskDate = :ParamDateTo and :ParamShiftTo >= sra.Shift)or(:ParamDateTo > sra.taskDate ))
AND 
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         )  
)selres 
inner JOIN DISPATCHER.POLY_USER_WORKS_DUMP ps ON ps.POLY_WORK_BINDINGS_ID = 61 --категория ср.взв.расстояние г.м.
AND (ps.ID = selres.WORKTYPE_ID AND ps.POLY_WORK_CAT_ID IS NOT NULL)
inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
group by 
  selres.shovid,
  selres.shiftdate,
  selres.shiftnum, 
  psc.POLY_WORK_CAT_NAME
 )sel inner join shovels d on d.shovid = sel.shovid and d.site='ТОО Комаровское'
),

----------------------
rasgm AS
(
-- Транспонируем таблицу с ср.взв.расстояниями
  SELECT * FROM rasgmq
  PIVOT (SUM(LENGTH) FOR CATEGORY IN (
    'ср.взв.расст.г.м.' RASGM
  ))
),
-------Расстояние--------
rasq AS
(
select
d.controlid TECH_KEY,
d.shovid,
sel.shiftdate,
sel.shiftnum,
sel.poly_work_cat_name CATEGORY,
NVL(sel.length,0) LENGTH
from
(select
selres.shovid,
selres.SHIFTDATE,
selres.SHIFTNUM,
psc.POLY_WORK_CAT_NAME,
decode(sum(selres.tripnumbermanual),0,0,sum(selres.tripnumbermanual*selres.AVLENGTH)/sum(selres.tripnumbermanual)) LENGTH
from
(
select
sra.shovid,
sra.taskdate SHIFTDATE,
sra.shift SHIFTNUM,
wt.ID WORKTYPE_ID,
sra.TRIPNUMBERMANUAL,
CASE WHEN NVL(sra.AVLENGTH,0) = 0 THEN sra.LENGTHMANUAL ELSE sra.AVLENGTH END AVLENGTH 
from shiftreportsadv sra inner join
worktypes wt on sra.WORKTYPE=wt.NAME
where 
 ((sra.taskDate = :ParamDateFrom and sra.Shift >= :ParamShiftFrom)or(sra.taskDate > :ParamDateFrom))
  and ((sra.taskDate = :ParamDateTo and :ParamShiftTo >= sra.Shift)or(:ParamDateTo > sra.taskDate ))
AND 
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         )  
)selres 
inner JOIN DISPATCHER.POLY_USER_WORKS_DUMP ps ON ps.POLY_WORK_BINDINGS_ID = 5 -- категория расстрояние
AND (ps.ID = selres.WORKTYPE_ID AND ps.POLY_WORK_CAT_ID IS NOT NULL)
inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
group by 
  selres.shovid,
  selres.shiftdate,
  selres.shiftnum, 
  psc.POLY_WORK_CAT_NAME
 )sel inner join shovels d on d.shovid = sel.shovid and d.site='ТОО Комаровское'
),
----------------------
ras AS
(
-- Транспонируем таблицу с расстояниями
  SELECT * FROM rasq
  PIVOT (SUM(LENGTH) FOR CATEGORY IN (
    'ПРС в контуре карьера' RASPRS,
    'вскрыша скальная' RASROCKSTRIP, 
    'вскрыша рыхлая' RASLOOSESTRIP,
    'вскрыша транзитная' RASTRANSSTRIP,
    'руда скальная' RASROCKORE, 
    'руда рыхлая' RASLOOSEORE,
    'руда транзитная' RASTRANSORE
  ))
),
---------------загрузка г.м. средневзв----
avwgmq AS
(
select
d.controlid TECH_KEY,
d.shovid,
sel.shiftdate,
sel.shiftnum,
sel.poly_work_cat_name CATEGORY,
NVL(sel.avweight,0) AVWEIGHT
from
(select
selres.shovid,
selres.SHIFTDATE,
selres.SHIFTNUM,
psc.POLY_WORK_CAT_NAME,
decode(sum(selres.tripnumbermanual),0,0,sum(selres.tripnumbermanual*selres.avweight)/sum(selres.tripnumbermanual)) AVWEIGHT 
from
(
select
sra.shovid,
sra.taskdate SHIFTDATE,
sra.shift SHIFTNUM,
wt.ID WORKTYPE_ID,
sra.TRIPNUMBERMANUAL,
CASE WHEN NVL(sra.AVWEIGHT,0) = 0 THEN sra.WEIGHTRATE ELSE sra.AVWEIGHT END AVWEIGHT
from shiftreportsadv sra inner join
worktypes wt on sra.WORKTYPE=wt.NAME
where 
 ((sra.taskDate = :ParamDateFrom and sra.Shift >= :ParamShiftFrom)or(sra.taskDate > :ParamDateFrom))
 and ((sra.taskDate = :ParamDateTo and :ParamShiftTo >= sra.Shift)or(:ParamDateTo > sra.taskDate ))
AND 
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         ) 
)selres 
inner JOIN DISPATCHER.POLY_USER_WORKS_DUMP ps ON ps.POLY_WORK_BINDINGS_ID = 41 --категория загрузка а/с г.м.
AND (ps.ID = selres.WORKTYPE_ID AND ps.POLY_WORK_CAT_ID IS NOT NULL)
inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
group by 
  selres.shovid,
  selres.shiftdate,
  selres.shiftnum, 
  psc.POLY_WORK_CAT_NAME
 )sel inner join shovels d on d.shovid = sel.shovid and d.site='ТОО Комаровское'
),
----------------------
avwgm AS
(
-- Транспонируем таблицу с загрузками а/с г.м.
  SELECT * FROM avwgmq
  PIVOT (sum(AVWEIGHT) FOR CATEGORY IN (
    'ср.взв.загр.г.м.' AVWGM
  ))
),
-------Загрузка а/с г.м.--------
avwq AS
(
select
d.controlid TECH_KEY,
d.shovid,
sel.shiftdate,
sel.shiftnum,
sel.poly_work_cat_name CATEGORY,
NVL(sel.avweight,0) AVWEIGHT
from
(select
selres.shovid,
selres.SHIFTDATE,
selres.SHIFTNUM,
psc.POLY_WORK_CAT_NAME,
decode(sum(selres.tripnumbermanual),0,0,sum(selres.tripnumbermanual*selres.avweight)/sum(selres.tripnumbermanual)) AVWEIGHT 
from
(
select
sra.shovid,
sra.taskdate SHIFTDATE,
sra.shift SHIFTNUM,
wt.ID WORKTYPE_ID,
sra.TRIPNUMBERMANUAL,
CASE WHEN NVL(sra.AVWEIGHT,0) = 0 THEN sra.WEIGHTRATE ELSE sra.AVWEIGHT END AVWEIGHT
from shiftreportsadv sra inner join
worktypes wt on sra.WORKTYPE=wt.NAME
where 
 ((sra.taskDate = :ParamDateFrom and sra.Shift >= :ParamShiftFrom)or(sra.taskDate > :ParamDateFrom))
 and ((sra.taskDate = :ParamDateTo and :ParamShiftTo >= sra.Shift)or(:ParamDateTo > sra.taskDate ))
AND 
     NOT (
             (
             TRIM(UPPER(unloadid)) LIKE ('%АВТОДОРОГА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ВНЕ ОТВАЛА%')
             OR  TRIM(UPPER(unloadid)) LIKE ('%ДОРОГА ОБЩЕГО ПОЛЬЗОВАНИЯ%')
             )
     AND       
           (
           TRIM(UPPER(worktype)) LIKE '%ПРС%'
           OR TRIM(UPPER(worktype)) LIKE '%ВСКРЫША%'
           OR TRIM(UPPER(worktype)) LIKE '%РУДА%'
           )
         ) 
)selres 
inner JOIN DISPATCHER.POLY_USER_WORKS_DUMP ps ON ps.POLY_WORK_BINDINGS_ID = 181 --категория загрузка экскаваторов
AND (ps.ID = selres.WORKTYPE_ID AND ps.POLY_WORK_CAT_ID IS NOT NULL)
inner JOIN DISPATCHER.POLY_WORK_CATEGORIES psc ON psc.POLY_WORK_CAT_ID = ps.POLY_WORK_CAT_ID
group by 
  selres.shovid,
  selres.shiftdate,
  selres.shiftnum, 
  psc.POLY_WORK_CAT_NAME
 )sel inner join shovels d on d.shovid = sel.shovid and d.site='ТОО Комаровское'
),
----------------------
avw AS
(
-- Транспонируем таблицу с загрузками а/с г.м.
  SELECT * FROM avwq
  PIVOT (sum(AVWEIGHT) FOR CATEGORY IN (
    'вскрыша скальная' AVWROCKSTRIP,
    'вскрыша рыхлая' AVWLOOSESTRIP, 
    'ПРС в контуре карьера' AVWPRS,
    'вскрыша транзитная' AVWTRANSSTRIP,
    'руда скальная' AVWROCKORE,
    'руда рыхлая' AVWLOOSEORE,
    'руда транзитная' AVWTRANSORE
    
  ))
)


--main query!!!

SELECT
'Экскаваторы' CATEGORY,
smsv.MODEL MODEL,
POSITION,
TECHID,
trim(SHIFTDATE) SHIFTDATE,
SHIFTNUM,
SHIFTDATE || ' ' || SHIFTNUM || ' смена' PERD,
------------
GMV,
GMW,
GMVROCK,
GMWROCK,
GMVLOOSE,
GMWLOOSE,
GMVTRANS,
GMWTRANS,
GMVROCKSTRIP,
GMWROCKSTRIP,
GMVLOOSESTRIP,
GMWLOOSESTRIP,
GMVPRS,
GMWPRS,
GMVTRANSSTRIP,
GMWTRANSSTRIP,
GMVROCKORE,
GMWROCKORE,
GMVLOOSEORE,
GMWLOOSEORE,
GMVTRANSORE,
GMWTRANSORE
  from
(SELECT
  pt.POSITION,
  t.VEHID TECHID,
  sst.shiftdate,
  sst.shiftnum,
  NVL(shmh.MH,0) MH,
  horq.HORIZONT HORIZONT,
  ----------kalen time --------
 12 KALENTIME,   
  --REGNORM--
NVL(p.ITOGPLANREM,0)+
NVL(p.ITOGTECHNOL,0) REGNORM,
  --------CHVHOZ-----
  NVL(wt.WTGM,0)+
  NVL(wt.WTHR,0)+
  NVL(p.ITOGTECHNOL,0) CHVHOZ,
  --------------
  NVL(wt.WTGM,0)+
  NVL(wt.WTHR,0) WTALL,
  --------------
  NVL(wt.WTGM,0) WTGM,
  NVL(wt.WTROCKSTRIP,0) WTROCKSTRIP,
  NVL(wt.WTLOOSESTRIP,0) WTLOOSESTRIP,
  NVL(wt.WTPRS,0) WTPRS,
  NVL(wt.WTTRANSSTRIP,0) WTTRANSSTRIP,
  NVL(wt.WTROCKORE,0) WTROCKORE,
  NVL(wt.WTLOOSEORE,0) WTLOOSEORE,
  NVL(wt.WTTRANSORE,0) WTTRANSORE,
  -------------
  NVL(wt.WTHR,0) WTHR,
  -------
NVL(wt.WTDRGHR,0) WTDRGHR,
NVL(wt.WTDOBORHR,0) WTDOBORHR,
NVL(wt.WTIPTHR,0) WTIPTHR,
NVL(wt.WTIWTHR,0) WTIWTHR,
NVL(wt.WTPOSTBORTHR,0) WTPOSTBORTHR,
NVL(wt.WTRESHOVHR,0) WTRESHOVHR,
NVL(wt.WTRAZBZABHR,0) WTRAZBZABHR,
NVL(wt.WTPRSOUTHR,0) WTPRSOUTHR,
NVL(wt.WTOTHERSHR,0) WTOTHERSHR,
 --- 
NVL(p.ITOGPLANREM,0) ITOGPLANREM,
NVL(p.TR,0) TR,
NVL(p.SERVICE,0) SERVICE,
NVL(p.KR,0) KR,
NVL(p.ITOGTECHNOL,0) ITOGTECHNOL,
NVL(p.DINNER,0) DINNER,
NVL(p.BREAKS,0) BREAKS,
NVL(p.ETO,0) ETO,
NVL(p.REFUEL,0) REFUEL,
NVL(p.RELOCATION,0) RELOCATION,
NVL(p.PERSNEED,0) PERSNEED,
NVL(p.MOVEBLOCK,0) MOVEBLOCK,
NVL(p.WAITTRUCK,0) WAITTRUCK,
NVL(p.PREPENTRANCE,0) PREPENTRANCE,
NVL(p.ZABOICLEAN,0) ZABOICLEAN,
NVL(p.BODYCLEAN,0) BODYCLEAN,
NVL(p.BUCKETCLEAN,0) BUCKETCLEAN,
NVL(p.VR,0)+
NVL(p.RECONCABLE,0) VR, --перецепка кабеля на ВР добавил к ВР
NVL(p.VRRELOC,0) VRRELOC,
NVL(p.TECHPER,0) TECHPER,
NVL(p.BREAKSHOTSEAT,0) BREAKSHOTSEAT,
NVL(p.WEATHER,0) WEATHER,
NVL(p.ITOGEMERG,0) ITOGEMERG,
NVL(p.ELECTRICAL,0) ELECTRICAL,
NVL(p.DVS,0) DVS,
NVL(p.TRANSMISSION,0) TRANSMISSION,
NVL(p.CHASSIS,0) CHASSIS,
NVL(p.HINGE,0) HINGE,
NVL(p.TIRES,0) TIRES,
NVL(p.HYDRAULIC,0) HYDRAULIC,
NVL(p.REPAIRRELOC,0) REPAIRRELOC,
NVL(p.ADJUSTMENT,0) ADJUSTMENT,
NVL(p.TURNMECH,0) TURNMECH,
NVL(p.ROPESHUFFLE,0) ROPESHUFFLE,
NVL(p.CABLEREPLACE,0) CABLEREPLACE,
NVL(p.ROPEREPLACE,0) ROPEREPLACE,
NVL(p.EMERGOTHERS,0) EMERGOTHERS,
NVL(p.AUXLACK,0) AUXLACK,
NVL(p.PARTSLACK,0) PARTSLACK,
NVL(p.OTHERSREASON,0) OTHERSREASON,
NVL(p.TOPPOIL,0) TOPPOIL,
NVL(p.ITOGORG,0) ITOGORG,
NVL(p.REGAUTH,0) REGAUTH,
NVL(p.FUELLACK,0) FUELLACK,
NVL(p.SURVWORK,0) SURVWORK,
NVL(p.GEOWORK,0) GEOWORK,
NVL(p.GOBASE,0) GOBASE,
NVL(p.STAFFLACK,0) STAFFLACK,
NVL(p.ORGOTHERS,0) ORGOTHERS,
NVL(p.CREWLACK,0) CREWLACK,
NVL(p.WORKLACK,0) WORKLACK,
NVL(p.s_kio,0) s_kio,
NVL(p.s_ktg,0) s_ktg,

--объем работ--
--GMV
NVL(gmv.GMVROCK,0)+
NVL(gmv.GMVLOOSE,0)+
NVL(gmv.GMVTRANS,0) GMV,
--GMW
NVL(gmw.GMWROCK,0)+
NVL(gmw.GMWLOOSE,0)+
NVL(gmw.GMWTRANS,0) GMW,

NVL(gmv.GMVROCK,0) GMVROCK,
NVL(gmw.GMWROCK,0) GMWROCK,
NVL(gmv.GMVLOOSE,0) GMVLOOSE,
NVL(gmw.GMWLOOSE,0) GMWLOOSE,
NVL(gmv.GMVTRANS,0) GMVTRANS,
NVL(gmw.GMWTRANS,0) GMWTRANS,
NVL(gmv.GMVROCKSTRIP,0) GMVROCKSTRIP,
NVL(gmw.GMWROCKSTRIP,0) GMWROCKSTRIP,
NVL(gmv.GMVLOOSESTRIP,0) GMVLOOSESTRIP,
NVL(gmw.GMWLOOSESTRIP,0) GMWLOOSESTRIP,
NVL(gmv.GMVPRS,0) GMVPRS,
NVL(gmw.GMWPRS,0) GMWPRS,
NVL(gmv.GMVTRANSSTRIP,0) GMVTRANSSTRIP,
NVL(gmw.GMWTRANSSTRIP,0) GMWTRANSSTRIP,
NVL(gmv.GMVROCKORE,0) GMVROCKORE,
NVL(gmw.GMWROCKORE,0) GMWROCKORE,
NVL(gmv.GMVLOOSEORE,0) GMVLOOSEORE,
NVL(gmw.GMWLOOSEORE,0) GMWLOOSEORE,
NVL(gmv.GMVTRANSORE,0) GMVTRANSORE,
NVL(gmw.GMWTRANSORE,0) GMWTRANSORE,

--эксплуатационная производительность
--OPVROCKSTRIP
CASE WHEN NVL(wt.WTROCKSTRIP,0)=0 THEN 0
ELSE NVL(gmv.GMVROCKSTRIP,0)*1000/NVL(wt.WTROCKSTRIP,0) END OPVROCKSTRIP,
--OPWROCKSTRIP
CASE WHEN NVL(wt.WTROCKSTRIP,0)=0 THEN 0
ELSE NVL(gmw.GMWROCKSTRIP,0)*1000/NVL(wt.WTROCKSTRIP,0) END OPWROCKSTRIP,
--OPVLOOSESTRIP
CASE WHEN NVL(wt.WTLOOSESTRIP,0)=0 THEN 0
ELSE NVL(gmv.GMVLOOSESTRIP,0)*1000/NVL(wt.WTLOOSESTRIP,0) END OPVLOOSESTRIP,
--OPWLOOSESTRIP
CASE WHEN NVL(wt.WTLOOSESTRIP,0)=0 THEN 0
ELSE NVL(gmw.GMWLOOSESTRIP,0)*1000/NVL(wt.WTLOOSESTRIP,0) END OPWLOOSESTRIP,
--OPVPRS
CASE WHEN NVL(wt.WTPRS,0)=0 THEN 0
ELSE NVL(gmv.GMVPRS,0)*1000/NVL(wt.WTPRS,0) END OPVPRS,
--OPWPRS
CASE WHEN NVL(wt.WTPRS,0)=0 THEN 0
ELSE NVL(gmw.GMWPRS,0)*1000/NVL(wt.WTPRS,0) END OPWPRS,
--OPVTRANSSTRIP
CASE WHEN NVL(wt.WTTRANSSTRIP,0)=0 THEN 0
ELSE NVL(gmv.GMVTRANSSTRIP,0)*1000/NVL(wt.WTTRANSSTRIP,0) END OPVTRANSSTRIP,
--OPWTRANSSTRIP
CASE WHEN NVL(wt.WTTRANSSTRIP,0)=0 THEN 0
ELSE NVL(gmw.GMWTRANSSTRIP,0)*1000/NVL(wt.WTTRANSSTRIP,0) END OPWTRANSSTRIP,
--OPVROCKORE
CASE WHEN NVL(wt.WTROCKORE,0)=0 THEN 0
ELSE NVL(gmv.GMVROCKORE,0)*1000/NVL(wt.WTROCKORE,0) END OPVROCKORE,
--OPWROCKORE
CASE WHEN NVL(wt.WTROCKORE,0)=0 THEN 0
ELSE NVL(gmw.GMWROCKORE,0)*1000/NVL(wt.WTROCKORE,0) END OPWROCKORE,
--OPVLOOSEORE
CASE WHEN NVL(wt.WTLOOSEORE,0)=0 THEN 0
ELSE NVL(gmv.GMVLOOSEORE,0)*1000/NVL(wt.WTLOOSEORE,0) END OPVLOOSEORE,
--OPWLOOSEORE
CASE WHEN NVL(wt.WTLOOSEORE,0)=0 THEN 0
ELSE NVL(gmw.GMWLOOSEORE,0)*1000/NVL(wt.WTLOOSEORE,0) END OPWLOOSEORE,
--OPVTRANSORE
CASE WHEN NVL(wt.WTTRANSORE,0)=0 THEN 0
ELSE NVL(gmv.GMVTRANSORE,0)/NVL(wt.WTTRANSORE,0) END OPVTRANSORE,
--OPWTRANSORE
CASE WHEN NVL(wt.WTTRANSORE,0)=0 THEN 0
ELSE NVL(gmw.GMWTRANSORE,0)*1000/NVL(wt.WTTRANSORE,0) END OPWTRANSORE,

--кол-во рейсов
NVL(tr.TRGM,0) TRGM,
NVL(tr.TRROCKSTRIP,0) TRROCKSTRIP,
NVL(tr.TRLOOSESTRIP,0) TRLOOSESTRIP,
NVL(tr.TRPRS,0) TRPRS,
NVL(tr.TRTRANSSTRIP,0) TRTRANSSTRIP,
NVL(tr.TRROCKORE,0) TRROCKORE,
NVL(tr.TRLOOSEORE,0) TRLOOSEORE,
NVL(tr.TRTRANSORE,0) TRTRANSORE,
--расстояние
NVL(rasgm.RASGM,0) RASGM,
NVL(ras.RASROCKSTRIP,0) RASROCKSTRIP,
NVL(ras.RASLOOSESTRIP,0) RASLOOSESTRIP,
NVL(ras.RASPRS,0) RASPRS,
NVL(ras.RASTRANSSTRIP,0) RASTRANSSTRIP,
NVL(ras.RASROCKORE,0) RASROCKORE,
NVL(ras.RASLOOSEORE,0) RASLOOSEORE,
NVL(ras.RASTRANSORE,0) RASTRANSORE,
--Кол-во экскаватора на линии
--KOLSHOVLINE
(12-NVL(p.ITOGPLANREM,0)-NVL(p.ITOGEMERG,0))/12 KOLSHOVLINE,
--Кол-во экскаватора в работе
--KOLSHOVWORK
  (NVL(wt.WTGM,0)+NVL(wt.WTHR,0)+NVL(p.ITOGTECHNOL,0))/12 KOLSHOVWORK,

--грузооборот
NVL(gmw.GMWROCKSTRIP,0)*1000*NVL(ras.RASROCKSTRIP,0) FTROCKSTRIP,
NVL(gmw.GMWLOOSESTRIP,0)*1000*NVL(ras.RASLOOSESTRIP,0) FTLOOSESTRIP,
NVL(gmw.GMWPRS,0)*1000*NVL(ras.RASPRS,0) FTPRS,
NVL(gmw.GMWTRANSSTRIP,0)*1000*NVL(ras.RASTRANSSTRIP,0) FTTRANSSTRIP,
NVL(gmw.GMWROCKORE,0)*1000*NVL(ras.RASROCKORE,0) FTROCKORE,
NVL(gmw.GMWLOOSEORE,0)*1000*NVL(ras.RASLOOSEORE,0) FTLOOSEORE,
NVL(gmw.GMWTRANSORE,0)*1000*NVL(ras.RASTRANSORE,0) FTTRANSORE,
--загрузка а/с
NVL(avwgm.AVWGM,0) AVWGM,
NVL(avw.AVWROCKSTRIP,0) AVWROCKSTRIP,
NVL(avw.AVWLOOSESTRIP,0) AVWLOOSESTRIP,
NVL(avw.AVWPRS,0) AVWPRS,
NVL(avw.AVWTRANSSTRIP,0) AVWTRANSSTRIP,
NVL(avw.AVWROCKORE,0) AVWROCKORE,
NVL(avw.AVWLOOSEORE,0) AVWLOOSEORE,
NVL(avw.AVWTRANSORE,0) AVWTRANSORE,

  NVL(totalstop.TOTALIDLE,0) TOTALIDLE,
  -----------
  12-
  (
  NVL(wt.WTGM,0)+
  NVL(wt.WTHR,0)+ 
  NVL(totalstop.TOTALIDLE,0)
  ) BALANCETIME
  
  ----------
FROM 
DISPATCHER.ALLVEHS t
INNER JOIN KGP.PTO_TECH pt ON pt.CONTROLID = t.CONTROLID AND pt.CATEGORY='Экскаваторы'
LEFT JOIN sst ON sst.TECH_KEY = t.CONTROLID
LEFT JOIN wt ON wt.TECH_KEY = t.CONTROLID and wt.shiftdate=sst.shiftdate and wt.shiftnum=sst.shiftnum
LEFT JOIN p ON p.TECH_KEY=t.CONTROLID and p.shiftdate=sst.shiftdate and p.shiftnum=sst.shiftnum
LEFT JOIN gmv ON gmv.TECH_KEY=t.CONTROLID and gmv.shiftdate=sst.shiftdate and gmv.shiftnum=sst.shiftnum
LEFT JOIN gmw ON gmw.TECH_KEY=t.CONTROLID and gmw.shiftdate=sst.shiftdate and gmw.shiftnum=sst.shiftnum
LEFT JOIN tr ON tr.TECH_KEY=t.CONTROLID and tr.shiftdate=sst.shiftdate and tr.shiftnum=sst.shiftnum
LEFT JOIN rasgm ON rasgm.TECH_KEY=t.CONTROLID and rasgm.shiftdate=sst.shiftdate and rasgm.shiftnum=sst.shiftnum
LEFT JOIN ras ON ras.TECH_KEY=t.CONTROLID and ras.shiftdate=sst.shiftdate and ras.shiftnum=sst.shiftnum
LEFT JOIN avw ON avw.TECH_KEY=t.CONTROLID and avw.shiftdate=sst.shiftdate and avw.shiftnum=sst.shiftnum
LEFT JOIN avwgm ON avwgm.TECH_KEY=t.CONTROLID and avwgm.shiftdate=sst.shiftdate and avwgm.shiftnum=sst.shiftnum
LEFT JOIN totalstop ON totalstop.TECH_KEY=t.CONTROLID and totalstop.shiftdate=sst.shiftdate and totalstop.shiftnum=sst.shiftnum
LEFT JOIN shmh ON shmh.TECH_KEY=t.CONTROLID and shmh.shiftdate=sst.shiftdate and shmh.shiftnum=sst.shiftnum
LEFT JOIN horq ON horq.TECH_KEY=t.CONTROLID and horq.shiftdate=sst.shiftdate and horq.shiftnum=sst.shiftnum

) src left join 
(select
*
from
shovels where site='ТОО Комаровское'
)smsv on src.techid=smsv.shovid
where (TECHID = :paramSelectTechId or :paramSelectTechId = 'Все')
ORDER BY POSITION, TECHID,shiftdate,shiftnum desc
		) GROUP BY  model,
          techid,
          category
ORDER BY  techid,
          model,
          category



";
        }


        public static string Get_Bogdan_custom_ShovDriverPerfomance_Shov()
        {
            return @"




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
        }

        public static string Get_Bogdan_custom_ShovDriverPerfomance_DownloadReference()
        {
            return @"



SELECT *
FROM   SHOV_DRIVER_PLAN
ORDER  BY SHOV_DRIVER_PLAN.SHOVID 




";
        }

        public static string Get_Bogdan_custom_AuxStoppages()
        {
            return @"



SELECT t1.*, :paramSelectTechId as TECH
FROM  (SELECT TIME,
              SPEED,
              FUEL
       FROM   AUXEVENTARCHIVE t1
              left join AUXTECHNICS t2
                     ON t1.AUXID = t2.AUXID
       WHERE  ( t1.AUXID = :paramSelectTechId )
              AND ( TIME BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', :paramShiftFrom, :paramDateFrom) AND GETPREDEFINEDTIMETO('за указанную смену', :paramShiftTo, :paramDateTo) )
       ORDER  BY TIME DESC) t1



";
        }

        public static string Get_Bogdan_custom_Vehtrips_By_Shovel()
        {
            return @"


SELECT 
VEHID, TIMELOAD, TIMEUNLOAD, FUELLOAD, FUELUNLOAD, WEIGHT, LENGTH, AVSPEED, UNLOADLENGTH, UNLOADID,  MOVETIME, WORKTYPE, TIME_INSERTING, VRATE, BUCKETCOUNT, LOADHEIGHT, UNLOADHEIGHT, ZLOAD, ZUNLOAD, TIMEGOAFTERLOAD
FROM   VEHTRIPS t1
WHERE  ( t1.SHOVID = :paramSelectTechId )
       AND ( TIMELOAD BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', GETCURSHIFTNUM (0, SYSDATE), GETCURSHIFTDATE (0, SYSDATE)) 
                          AND GETPREDEFINEDTIMETO('за указанную смену', GETCURSHIFTNUM (0, SYSDATE), GETCURSHIFTDATE (0, SYSDATE)) )
ORDER  BY TIMELOAD DESC 



";
        }

        public static string Get_Bogdan_custom_Last_Shovel_State()
        {
            return @"


SELECT SHOVID,
       t1.TIME,
       FUEL,
       SPEED,
       X,
       Y,
       STATECODE,
       LASTACTIVETIME
FROM   SHOVSTAT_PO_EVENT t1
       join (SELECT MAX(TIME) AS TIME
             FROM   SHOVSTAT_PO_EVENT
             WHERE  TIME IS NOT NULL
                    AND SHOVID = :paramSelectTechId
             GROUP  BY TIME
             ORDER  BY TIME DESC) t2
         ON t1.TIME = t2.TIME 
         WHERE  SHOVID = :paramSelectTechId



";
        }

        public static string Get_Bogdan_custom_Last_Shovel_Arhive_Record()
        {
            return @"



select SHOVID,
       EVENTTYPE,
       t1.TIME,
       X,
       Y,
       SYSTEMTIME,
       FUEL,
       SPEED,
       MOTOHOURS,
       Z
        from SHOVEVENTSTATEARCHIVE t1
       join (SELECT max(time) as time
             FROM   SHOVEVENTSTATEARCHIVE
             WHERE  SHOVID = :paramSelectTechId
             GROUP  BY SHOVID
             ORDER  BY time DESC) t2
         ON t1.time = t2.time 
         WHERE  SHOVID = :paramSelectTechId



";
        }

        public static string Get_Bogdan_custom_Vehtrips_Analyze_By_Shovel()
        {
            return @"


SELECT MAX(PASSED)                                                 AS passed,
       MAX(STILL)                                                  AS still,
       ROUND(SUM(t2.WEIGHT), 0)                                                                                                      AS SUM_MASS,
       ROUND(SUM(t2.WEIGHT) + (SUM(t2.WEIGHT) * MAX(still)), 0)                                                                 AS PROG_SUM_MASS,
     ROUND(AVG(t2.WEIGHT), 1)                                                                                                      AS AVG_MASS,
       ROUND(SUM(t2.BUCKETCOUNT), 0)                                                                                                 AS SUM_BUCKET,
       ROUND(SUM(t2.BUCKETCOUNT) + (SUM(t2.BUCKETCOUNT) * MAX(still)), 0)                                                       AS PROG_SUM_BUCKET,
     ROUND(AVG(t2.BUCKETCOUNT), 1)                                                                                                 AS AVG_BUCKET,
       ROUND(SUM(t2.LENGTH), 0)                                                                                                      AS SUM_LENGTH,
       ROUND(SUM(t2.LENGTH) + (SUM(t2.LENGTH) * MAX(still)), 0)                                                                 AS PROG_SUM_LENGTH,
     ROUND(AVG(t2.LENGTH), 1)                                                                                                      AS AVG_LENGTH,
       ROUND(SUM((t2.TIMEUNLOAD - t2.TIMELOAD) * 24 * 60), 0)                                                                      AS SUM_POGR_TIME,
     ROUND(SUM((t2.TIMEUNLOAD - t2.TIMELOAD) * 24 * 60) + (SUM((t2.TIMEUNLOAD - t2.TIMELOAD) * 24 * 60) * MAX(still)), 0) AS PROG_SUM_POGR_TIME,
ROUND(AVG((t2.TIMEUNLOAD - t2.TIMELOAD) * 24 * 60), 1)                                                                      AS AVG_POGR_TIME,
       ROUND(SUM(t2.FUELLOAD - t2.FUELUNLOAD), 0)                                                                                    AS SUM_POGR_FUEL,
       ROUND(SUM(t2.FUELLOAD - t2.FUELUNLOAD) + (SUM(t2.FUELLOAD - t2.FUELUNLOAD) * MAX(still)), 0)                             AS PROG_SUM_POGR_FUEL,
     ROUND(AVG(t2.FUELLOAD - t2.FUELUNLOAD), 1)                                                                                    AS AVG_POGR_FUEL,
       COUNT(*)                                                                                                                      AS SUM_TRIPS,
       ROUND(COUNT(*) + (COUNT(*) * (MAX(still))), 0)                                                                         AS PROG_SUM_TRIPS,
   ROUND(COUNT(*) + (COUNT(*) * (MAX(still) * 1.5)), 0)                                              AS PROG_SUM_TRIPS_CORRECT
FROM(SELECT t1.*,
               ROUND(((SYSDATE - GETPREDEFINEDTIMEFROM('за указанную смену', GETCURSHIFTNUM(0, SYSDATE), GETCURSHIFTDATE(0, SYSDATE))) * 24 / 12), 3)       AS passed,
               1 - ROUND(((SYSDATE - GETPREDEFINEDTIMEFROM('за указанную смену', GETCURSHIFTNUM(0, SYSDATE), GETCURSHIFTDATE(0, SYSDATE))) * 24 / 12), 3)       AS still
        FROM   VEHTRIPS t1
        WHERE(t1.SHOVID = :paramSelectTechId)
               AND(TIMELOAD BETWEEN GETPREDEFINEDTIMEFROM('за указанную смену', GETCURSHIFTNUM(0, SYSDATE), GETCURSHIFTDATE(0, SYSDATE)) AND GETPREDEFINEDTIMETO('за указанную смену', GETCURSHIFTNUM(0, SYSDATE), GETCURSHIFTDATE(0, SYSDATE)))
        ORDER  BY TIMELOAD DESC) t2



";
        }

    }

    public class Study
    {
        public class HotKeyClass
        {
            /*
            CTRL+K, CTRL+C Закомментировать выделенный фрагмент

            CTRL+K, CTRL+F Форматировать выделенный фрагмент (CTRL+K CTRL+F - весь документ)

            CTRL+M CTRL+M Свернуть/Развернуть выделенный фрагмент (CTRL+M CTRL+L - весь документ)
            */
        }

        public class VariablesClass
        {
            public static byte ByteClass()
            {
                System.Byte variable_1 = 64;
                MessageBox.Show($"ByteClass: {variable_1.GetType().Name} | {variable_1}");

                byte variable_2 = 0x10;
                MessageBox.Show($"ByteClass: {variable_2.GetType().Name} | {variable_2}");

                return 0x14;
            }

            public static bool BooleanClass()
            {
                System.Boolean variable_1 = false;
                MessageBox.Show($"BooleanClass: {variable_1.GetType().Name} | {variable_1}");

                bool variable_2 = true;
                MessageBox.Show($"BooleanClass: {variable_2.GetType().Name} | {variable_2}");

                return true;
            }

            public static char CharClass()
            {
                System.Char variable_1 = 'Z';
                MessageBox.Show($"CharClass: {variable_1.GetType().Name} | {variable_1}");

                char variable_2 = '1';
                MessageBox.Show($"CharClass: {variable_2.GetType().Name} | {variable_2}");

                return 'A';
            }

            public static string StringClass()
            {
                System.String variable_1 = "Язык 1";
                MessageBox.Show($"StringClass: {variable_1.GetType().Name} | {variable_1}");

                string variable_2 = "Язык 2";
                MessageBox.Show($"StringClass: {variable_2.GetType().Name} | {variable_2}");

                return "Пример текста";
            }

            public static int IntegerClass()
            {
                System.Int32 variable_1 = -10;
                MessageBox.Show($"IntegerClass: {variable_1.GetType().Name} | {variable_1}");

                int variable_2 = 10;
                MessageBox.Show($"IntegerClass: {variable_2.GetType().Name} | {variable_2}");

                return 32;
            }

            public static float FloatClass()
            {
                System.Single variable_1 = -10.5F;
                MessageBox.Show($"FloatClass: {variable_1.GetType().Name} | {variable_1}");

                float variable_2 = 10.5F;
                MessageBox.Show($"FloatClass: {variable_2.GetType().Name} | {variable_2}");

                return 32.6F;
            }

            public static double DoubleClass()
            {
                System.Double variable_1 = -10.5D;
                MessageBox.Show($"DoubleClass: {variable_1.GetType().Name} | {variable_1}");

                double variable_2 = 10.5D;
                MessageBox.Show($"DoubleClass: {variable_2.GetType().Name} | {variable_2}");

                return 32.6D;
            }

            public static int[] Array1DClass()
            {
                int[] variable_1 = new int[5] { 10, 20, 30, 40, 50 };
                int[] variable_2 = new int[] { 10, 20, 30, 40, 50 };
                int[] variable_3 = { 10, 20, 30, 40, 50 };

                string variable_3_ = "";
                foreach (int i in variable_3) //for (int i = 0; i < variable_3.Length; i++)  
                {
                    variable_3_ += $" {i} ";
                }
                MessageBox.Show($"Array1DClass: {variable_3.GetType().Name} | {variable_3_}");

                return new int[] { 10, 20, 30, 40, 50 };
            }

            public static int[,] Array2MDClass()
            {
                int[,] variable_1 = new int[3, 3] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
                int[,] variable_2 = new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
                int[,] variable_3 = { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };

                string variable_3_ = "";
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        variable_3_ += $" {variable_3[i, j]} ";
                    }
                }
                MessageBox.Show($"Array2MDClass: {variable_3.GetType().Name} | {variable_3_}");

                return new int[,] { { 1, 2, 3 }, { 4, 5, 6 }, { 7, 8, 9 } };
            }

            public static int[][] Array2DClass()
            {
                int[][] variable_1 = new int[][] {
                    new int[] { 11, 21, 56, 78 },
                    new int[] { 2, 5, 6, 7, 98, 5 },
                    new int[] { 2, 5 }
                };
                int[][] variable_2 = {
                    new int[] { 11, 21, 56, 78 },
                    new int[] { 2, 5, 6, 7, 98, 5 },
                    new int[] { 2, 5 }
                };

                string variable_1_ = "";
                for (int i = 0; i < variable_1.Length; i++)
                {
                    for (int j = 0; j < variable_1[i].Length; j++)
                    {
                        variable_1_ += $" {variable_1[i][j]} ";
                    }
                    variable_1_ += $" | ";
                }
                MessageBox.Show($"Array2DClass: {variable_1.GetType().Name} | {variable_1_}");

                string variable_2_ = "";
                foreach (int[] i in variable_2.AsEnumerable())
                {
                    foreach (int j in i.AsEnumerable())
                    {
                        variable_2_ += $" {j} ";
                    }
                    variable_2_ += $" | ";
                }
                MessageBox.Show($"Array2DClass: {variable_2.GetType().Name} | {variable_2_}");

                return new int[][] {
                    new int[] { 11, 21, 56, 78 },
                    new int[] { 2, 5, 6, 7, 98, 5 },
                    new int[] { 2, 5 }
                };
            }

            public static object ObjectClass()
            {
                System.Object variable_1 = -10;
                MessageBox.Show($"ObjectClass: {variable_1.GetType().Name} | {variable_1}");

                object variable_2 = 15.0F;
                MessageBox.Show($"ObjectClass: {variable_2.GetType().Name} | {variable_2}");

                return "i'm a object";
            }

            public static void VarClass()
            {
                var variable_1 = 10;
                var variable_2 = "String";
                var variable_3 = -7.6F;

                MessageBox.Show($"VarClass: {variable_1.GetType().Name} | {variable_1}");
                MessageBox.Show($"VarClass: {variable_2.GetType().Name} | {variable_2}");
                MessageBox.Show($"VarClass: {variable_3.GetType().Name} | {variable_3}");
            }

            public static Dictionary<string, int> DictionaryClass()
            {
                System.Collections.Generic.Dictionary<int, string> variable_1 = new Dictionary<int, string>(){
                    { 12, "СУММ" },
                    { 13, "СРЗНАЧ" },
                };
                variable_1.Add(14, "СРЕД");
                variable_1.Remove(12);
                string val = variable_1[13];
                foreach (KeyValuePair<int, string> pairDictionary in variable_1)
                {
                    MessageBox.Show($"DictionaryClass: {pairDictionary.GetType().Name} | {pairDictionary}");
                    MessageBox.Show($"DictionaryClass: {pairDictionary.Key} | {pairDictionary.Value}");
                }
                MessageBox.Show($"DictionaryClass: {variable_1.GetType().Name} | {variable_1}");

                Dictionary<string, float> variable_2 = new Dictionary<string, float>(){
                    { "First", 12.9F },
                    { "Second", 15.0F },
                };
                MessageBox.Show($"DictionaryClass: {variable_2.GetType().Name} | {variable_2}");

                return new Dictionary<string, int>(){
                        { "First", 12 },
                        { "Second", 15 },
                    };
            }

            public static List<int> ListClass()
            {
                System.Collections.Generic.List<string> variable_1 = new List<string>(){
                    { "СУММ" },
                    { "СРЗНАЧ" },
                };
                variable_1.Add("СРЕД");
                variable_1.Remove("СРЗНАЧ");
                variable_1.RemoveAt(1);
                variable_1[1] = "ЗАМЕНА";
                string val = variable_1[0];
                foreach (string value in variable_1)
                {
                    MessageBox.Show($"ListClass: {value.GetType().Name} | {value}");
                }
                for (int value = 0; value < variable_1.Count - 1; value += 1)
                {
                    MessageBox.Show($"ListClass: {variable_1[value].GetType().Name} | {variable_1[value]}");
                }
                MessageBox.Show($"ListClass: {variable_1.GetType().Name} | {variable_1}");

                List<float> variable_2 = new List<float>(){
                    { 12.8F },
                    { 14.0F },
                };
                MessageBox.Show($"ListClass: {variable_2.GetType().Name} | {variable_2}");

                return new List<int>(){
                        { 12 },
                        { 15 },
                    };
            }

            public static Tuple<int, string, bool> TupleClass()
            {
                System.Tuple<int, string> variable_1 = new Tuple<int, string>(12, "12");
                Tuple<float, bool> variable_2 = Tuple.Create(17.8F, false);

                MessageBox.Show($"TupleClass: {variable_1.GetType().Name} | {variable_1}");
                MessageBox.Show($"TupleClass: {variable_2.GetType().Name} | {variable_2}");

                return Tuple.Create(10, "10", true);
            }

            public static DateTime DateTimeClass()
            {
                System.DateTime variable_1 = DateTime.Now;
                DateTime variable_2 = DateTime.Now.AddMonths(-1);

                MessageBox.Show($"DateTimeClass: {variable_1.GetType().Name} | {variable_1}");
                MessageBox.Show($"DateTimeClass: {variable_2.GetType().Name} | {variable_2}");

                return DateTime.Now;
            }

            public static DataSet DataSetClass()
            {
                System.Data.DataSet variable_1 = new DataSet();
                MessageBox.Show($"DataSetClass: {variable_1.GetType().Name} | {variable_1}");

                DataSet variable_2 = new DataSet();
                MessageBox.Show($"DataSetClass: {variable_2.GetType().Name} | {variable_2}");

                return new DataSet();
            }
        }

        public class OperatorsClass
        {
            /*
            Unary	+ - ! ~ ++ -- (type)* & sizeof	Right to Left
            Additive	+ -	Left to Right
            Multiplicative	% / *	Left to Right
            Relational	< > <= >=	Left to Right
            Shift	<< >>	Left to Right
            Equality	== !=	Right to Left
            Logical AND	&	Left to Right
            Logical OR	|	Left to Right
            Logical XOR	^	Left to Right
            Conditional OR	||	Left to Right
            Conditional AND	&&	Left to Right
            Null Coalescing	??	Left to Right
            Ternary	?:	Right to Left
            Assignment	= *= /= %= += - = <<= >>= &= ^= |= =>	Right to Left
            */
        }

        public class LogicClass
        {
            public static void IfWithoutElseMethod()
            {
                int variable_1 = 10;
                string variable_1_ = "сообщение 0: не изменилось";
                if (variable_1 > 5)
                {
                    variable_1_ = "сообщение 1: больше 5";
                }
                MessageBox.Show($"IfWithoutElseMethod: {variable_1_}");
            }

            public static void IfWithElseMethod()
            {
                int variable_1 = 10;
                string variable_1_ = "сообщение 0: не изменилось";
                if (variable_1 < 5)
                {
                    variable_1_ = "сообщение 1: меньше 5";
                } else if (variable_1 > 5)
                {
                    variable_1_ = "сообщение 2: больше 5";
                } else
                {
                    variable_1_ = "сообщение 3: остальные случаи(равно 5)";
                }
                MessageBox.Show($"IfWithElseMethod: {variable_1_}");
            }

            public static void SwitchCaseMethod()
            {
                string variable_1 = "Яблоко";
                string variable_1_ = "сообщение 0: не изменилось";
                switch (variable_1)
                {
                    case "Груша":
                        {
                            variable_1_ = "сообщение 1: Это груша";
                            break;
                        }
                    case "Яблоко":
                        {
                            variable_1_ = "сообщение 2: Это Яблоко";
                            break;
                        }
                    case "Дыня":
                        {
                            variable_1_ = "сообщение 3: Это Дыня";
                            break;
                        }
                    default:
                        {
                            variable_1_ = "сообщение 4: Не совпадает";
                            break;
                        }
                }
                MessageBox.Show($"SwitchCaseMethod: {variable_1_}");
            }
        }

        public class LoopsClass
        {
            public static void ForMethod()
            {
                int variable_1 = 10;
                for (int i = 1; i <= 10; i += 1)
                {
                    variable_1 += i * 2;
                    // continue; // skip this lopp
                    // break; // close all loops
                    // return null; // close all loops and return value
                }
                MessageBox.Show($"ForMethod: {variable_1}");

                /*
                List<List<int>> lists = new List<List<int>>();
                for (int i = 1; i < 10; i++)
                {
                    List<int> list = new List<int>();
                    for (int j = 1; j < 15; j++) {
                        list.Add(j);
                    }
                    lists.Add(list);
                }

                int loop1 = 0;
                foreach (List<int> list in lists)
                {
                    loop1 += 1;
                    int loop2 = 0;
                    foreach (int value in list)
                    {
                        loop2 += 1;
                        Console.WriteLine($"loop1: {loop1}, loop2: {loop2}, value: {value}");
                    }
                }
                */
            }

            public static void ForeachMethod()
            {
                int variable_1 = 10;
                int[] loop = { 1, 2, 3, 4, 5, 6, 7, 8, 10 };
                foreach (int i in loop)
                {
                    variable_1 += i * 2;
                    // continue; // skip this lopp
                    // break; // close all loops
                    // return null; // close all loops and return value
                }
                MessageBox.Show($"ForeachMethod: {variable_1}");


                /*
                List<List<int>> lists = new List<List<int>>();
                for (int i = 1; i < 10; i++)
                {
                    List<int> list = new List<int>();
                    for (int j = 1; j < 15; j++) {
                        list.Add(j);
                    }
                    lists.Add(list);
                }

                int loop1 = 0;
                foreach (List<int> list in lists)
                {
                    loop1 += 1;
                    int loop2 = 0;
                    foreach (int value in list)
                    {
                        loop2 += 1;
                        Console.WriteLine($"loop1: {loop1}, loop2: {loop2}, value: {value}");
                    }
                }
                */
            }

            public static void WhileMethod()
            {
                int variable_1 = 10;
                int i = 1;
                while (i <= 10)
                {
                    variable_1 += i * 2;
                    // continue; // skip this lopp
                    // break; // close all loops
                    // return null; // close all loops and return value

                    i += 1;
                }
                MessageBox.Show($"WhileMethod: {variable_1}");
            }
        }

        public class FunctionsClass
        {
            public static void FunctionWithoutArgsWithoutReturnMethod()
            {
                void FunctionWithoutArgsWithoutReturn()
                {
                    MessageBox.Show($"FunctionWithoutArgsWithoutReturn event!");
                }
                FunctionWithoutArgsWithoutReturn();
            }

            public static void FunctionWithoutArgsMethod()
            {
                string FunctionWithoutArgs()
                {
                    MessageBox.Show($"FunctionWithoutArgs event!");
                    return "successfully!";
                }
                string variable_1 = FunctionWithoutArgs();
                MessageBox.Show($"FunctionWithoutArgs: {variable_1}");
            }

            public static void FunctionMethod()
            {
                string Function(string message)
                {
                    MessageBox.Show($"FunctionMethod event! {message}");
                    return "successfully!";
                }
                string variable_1 = Function(message: "Hellow world!");
                MessageBox.Show($"FunctionMethod: {variable_1}");
            }
        }

        public class ClassesClass
        {
            public class SimpleClass
            {
                public bool a = true;
                public int b = 10;
                public float c = 12.6F;
                public string d = $"stroke";

                public void SetA(bool newValue)
                {
                    this.a = newValue;
                }
                public string GetD()
                {
                    return this.d;
                }
                public static float GetSum(float Val1, int Val2)
                {
                    return Val1 + Val2;
                }
            }

            public class ParentClass
            {
                #region public variables
                public string name = "";
                public int age = 0;
                #endregion public variables

                #region private variables
                private readonly bool isMarried = false;
                #endregion private variables

                #region protected variables
                protected string fullName = ": 0";
                #endregion protected variables

                #region constructor
                public ParentClass(string name, int age)
                {
                    this.name = name;
                    this.age = age;
                    if (age > 25)
                    {
                        this.isMarried = true;
                    } else
                    {
                        this.isMarried = false;
                    }
                    this.fullName = $"{name}: {age}";
                }
                #endregion constructor

                #region public methods
                public string GetNameMethod()
                {
                    return name;
                }

                public int GetAgeMethod()
                {
                    return age;
                }
                #endregion public methods

                #region private methods
                private bool GetIsMarriedMethod()
                {
                    return isMarried;
                }
                #endregion private methods

                #region protected methods
                protected string GetFullNameMethod()
                {
                    return fullName;
                }
                #endregion protected methods

                #region static methods
                public static int GetSumValueMethod(float val1, float val2)
                {
                    return (int)Math.Round(val1 + val2);
                }
                #endregion static methods
            }
        }

        public class FilesActionsClass
        {
            public static void WriteTxtMethod()
            {
                /*  $"{Directory.GetCurrentDirectory()}\\new.text"; */
                int WriteToFile(string filePath = @"C:\", string fileName = "temp", string fileExtension = ".txt", List<string> lines = null)
                {
                    try
                    {
                        if (lines.Count <= 0)
                        {
                            lines = new List<string> { "First line\n", "Second line\n", "Third line" };
                        }
                        using (StreamWriter outputFile = new StreamWriter(Path.Combine(filePath, fileName, fileExtension)))
                        {
                            foreach (string line in lines)
                            {
                                outputFile.WriteLine(line);
                            }
                        }
                        return 1;
                    } catch (Exception error)
                    {
                        MessageBox.Show($"error: {error}");
                        return -1;
                    }
                }
                int result = WriteToFile(fileName: "temp2");
                MessageBox.Show($"ForMethod: {result}");
            }
        }

        public class ExcelClass
        {
            public class UtilsClass
            {
                public static string GetExcelColumnNameMethod(int columnNumber)
                {
                    string columnName = "";

                    while (columnNumber > 0)
                    {
                        int modulo = (columnNumber - 1) % 26;
                        columnName = Convert.ToChar('A' + modulo) + columnName;
                        columnNumber = (columnNumber - modulo) / 26;
                    }

                    return columnName;
                }

                public static int ColumnNumberMethod(string colAdress)
                {
                    int[] digits = new int[colAdress.Length];
                    for (int i = 0; i < colAdress.Length; ++i)
                    {
                        digits[i] = Convert.ToInt32(colAdress[i]) - 64;
                    }
                    int mul = 1;
                    int res = 0;
                    for (int pos = digits.Length - 1; pos >= 0; --pos)
                    {
                        res += digits[pos] * mul;
                        mul *= 26;
                    }
                    return res;
                }

                public static DataSet ExcelReadMethod(string fullFilePath)
                {
                    try
                    {
                        Excel.Application application = new Excel.Application();
                        Excel.Workbook workbook = application.Workbooks.Open(fullFilePath); //(Path.Combine(filePath, fileName + fileExtension));
                        Excel.Worksheet worksheet = workbook.Worksheets[1];  // workbook.Name; workbook.Worksheets.Count; worksheet.Name;
                        Excel.Range usedRange = worksheet.UsedRange;
                        int lastRow = usedRange.Rows.Count;
                        int lastColumn = usedRange.Columns.Count;

                        DataTable newDataTable = new System.Data.DataTable("Table 1");
                        for (int j = 1; j <= lastColumn; j += 1)
                        {
                            DataColumn newColumn = new DataColumn(worksheet.Cells[1, j].Value) {
                                ColumnName = $"{j}"
                            };
                            /*
                            DataColumn column1 = new DataColumn();
                            column1.DataType = System.Type.GetType("System.Int32"); // GetType("System.String")
                            column1.ColumnName = "id";
                            column1.DefaultValue = "";
                            column1.AutoIncrement = true;

                            DataColumn [] keys = new DataColumn [1];
                            keys[0] = column1;
                            newDataTable.PrimaryKey = keys;
                            */
                            newDataTable.Columns.Add(newColumn);
                        }

                        for (int i = 1 + 1; i <= lastRow; i += 1)
                        {
                            DataRow newRow = newDataTable.NewRow();
                            for (int j = 1; j <= lastColumn; j += 1)
                            {
                                //MessageBox.Show($"Cell {worksheet.Cells[i, j].Value}");
                                newRow[j - 1] = worksheet.Cells[i, j].Value;
                            }
                            newDataTable.Rows.Add(newRow);
                        }

                        workbook.Close();
                        application.Quit();

                        DataSet newDataSet = new DataSet();
                        newDataSet.Tables.Add(newDataTable);
                        //MessageBox.Show($"newDataSet {dataSet1.Tables[0]}");
                        return newDataSet;
                    } catch (Exception error)
                    {
                        MessageBox.Show($"error: {error}");
                        return null;
                    }
                }

                public static List<string> ExcelViewMethod(DataSet dataSet, int charLength = 20, bool printToConsole = true) // screen width ~ 8 col * 30 chars = 240 chars
                {
                    List<List<object>> rows = new List<List<object>>();
                    foreach (System.Data.DataRow dataRow in dataSet.Tables[0].AsEnumerable())
                    {
                        List<object> row = new List<object>(){
                            dataRow["1"],
                            dataRow["2"],
                            dataRow["3"],
                            dataRow["4"],
                            dataRow["5"],
                            dataRow["6"],
                            dataRow["7"],
                            dataRow["8"]
                        };
                        rows.Add(row);
                    }
                    List<string> lines = new List<string>();
                    foreach (List<object> row in rows)
                    {
                        string line = "| ";
                        foreach (object cell in row)
                        {
                            line += $"{cell}" + " |";
                        }
                        if (printToConsole)
                        {
                            Console.WriteLine(line);
                        }
                        lines.Add(line);
                    }
                    return lines;
                }

                public static int ExcelWriteMethod(DataSet dataSetArg, string fullTemplatePath = "")
                {
                    try
                    {
                        Excel.Application excelApplication = new Excel.Application();
                        Excel.Workbook workbook;
                        Excel.Worksheet worksheet;

                        excelApplication.DisplayAlerts = false;
                        excelApplication.Visible = false;

                        if (fullTemplatePath.Length <= 0)
                        {
                            fullTemplatePath = $"{Directory.GetCurrentDirectory()}\\templates\\Рабочая книга .xlsx";
                        }
                        workbook = excelApplication.Workbooks.Add(fullTemplatePath);
                        Excel.Worksheet templateWorksheet = workbook.Worksheets[1];
                        templateWorksheet.Copy(templateWorksheet);
                        worksheet = workbook.Worksheets[2];
                        workbook.Worksheets[1].Delete();
                        worksheet.Name = "Лист 1";

                        //MessageBox.Show($"Length: {dataSetArg.Tables[0].AsEnumerable().ToArray().Length}");

                        List<List<object>> rows = new List<List<object>>();
                        foreach (System.Data.DataRow dataRow in dataSetArg.Tables[0].AsEnumerable())
                        {
                            List<object> row = new List<object>(){
                                dataRow["1"],
                                dataRow["5"],
                                dataRow["2"],
                                dataRow["6"],
                                dataRow["3"],
                                dataRow["4"],
                                dataRow["7"],
                                dataRow["8"]
                            };
                            rows.Add(row);
                        }

                        int startRow = 1;
                        foreach (List<object> row in rows)
                        {
                            int startColumn = 1;
                            foreach (object item in row)
                            {
                                worksheet.Cells[startRow, startColumn].Value = item;
                                startColumn += 1;
                            }
                            startRow += 1;
                        }

                        excelApplication.DisplayAlerts = true;
                        excelApplication.Visible = true;

                        //workbook1.Save();
                        //workbook1.Close();
                        //application1.Quit();

                        return 1;
                    } catch (Exception error)
                    {
                        MessageBox.Show($"error: {error}");

                        return -1;
                    }
                }
            }

            public static void ExcelReadWithOpenDialogMethod()
            {
                void ExcelRead()
                {
                    string filePath = string.Empty;
                    string fileExt = string.Empty;
                    OpenFileDialog file = new OpenFileDialog(); //open dialog to choose file
                    if (file.ShowDialog() == System.Windows.Forms.DialogResult.OK) //if there is a file chosen by the user
                    {
                        filePath = file.FileName; //get the path of the file
                        fileExt = Path.GetExtension(filePath); //get the file extension
                        if (fileExt.CompareTo(".xls") == 0 || fileExt.CompareTo(".xlsx") == 0)
                        {
                            try
                            {
                                System.Data.DataTable dtExcel = new System.Data.DataTable();
                                //dtExcel = ReadExcel(filePath, fileExt); //read excel file
                                //dataGridView1.Visible = true;
                                //dataGridView1.DataSource = dtExcel;
                            } catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message.ToString());
                            }
                        } else
                        {
                            MessageBox.Show("Please choose .xls or .xlsx file only.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error); //custom messageBox to show error
                        }
                    }
                    //create a instance for the Excel object  
                    Excel.Application oExcel = new Excel.Application();

                    //specify the file name where its actually exist  
                    string filepath = $"{Directory.GetCurrentDirectory()}\\new.xlsx";

                    //pass that to workbook object  
                    Excel.Workbook WB = oExcel.Workbooks.Open(filepath);

                    // statement get the workbookname  
                    string ExcelWorkbookname = WB.Name;

                    // statement get the worksheet count  
                    int worksheetcount = WB.Worksheets.Count;

                    Excel.Worksheet wks = (Excel.Worksheet)WB.Worksheets[1];

                    // statement get the firstworksheetname  

                    string firstworksheetname = wks.Name;

                    //statement get the first cell value  
                    var firstcellvalue = ((Excel.Range)wks.Cells[1, 1]).Value;
                    MessageBox.Show($"firstcellvalue: {firstcellvalue}");
                }
                ExcelRead();
            }

            public static void ExcelFormattingMethod()
            {
                /*
                
                rangeHeader.PasteSpecial(XlPasteType.xlPasteFormats, XlPasteSpecialOperation.xlPasteSpecialOperationNone);


                // Объединяем и присваиваем 1 в первом столбце таблицы
                ws.Range[$"B11:B{rowTo - 2}"].Merge();
                ws.Cells[11, "B"] = 1;
                */

                /*
                // Делаем вокруг тела таблицы жирную рамку
                var tableBody = ws.Range[$"A1:I{rowTo - 1}"];
                tableBody.Borders[XlBordersIndex.xlEdgeLeft].Weight = XlBorderWeight.xlMedium;
                tableBody.Borders[XlBordersIndex.xlEdgeRight].Weight = XlBorderWeight.xlMedium;
                tableBody.Borders[XlBordersIndex.xlEdgeTop].Weight = XlBorderWeight.xlMedium;
                tableBody.Borders[XlBordersIndex.xlEdgeBottom].Weight = XlBorderWeight.xlMedium;
                */


                /*
                // Выводим заголовок группы
                Range rangeHeader = ws.Range[$"C{rowFrom - 1}", $"P{rowFrom - 1}"];
                groupRow.Copy();
                rangeHeader.PasteSpecial(XlPasteType.xlPasteFormats, XlPasteSpecialOperation.xlPasteSpecialOperationNone);
                */

                //ws.Cells[rowFrom - 1, "C"] = group.First()["TEHTYPE"];
                /*
                ws.Range[$"E{rowFrom - 1}", $"E{rowFrom - 1}"].FormulaLocal = $"=СУММ(E{rowFrom}:E{rowTo})";
                ws.Range[$"F{rowFrom - 1}", $"F{rowFrom - 1}"].FormulaLocal = $"=СУММ(F{rowFrom}:F{rowTo})";
                ws.Range[$"G{rowFrom - 1}", $"G{rowFrom - 1}"].FormulaLocal = $"=СУММ(G{rowFrom}:G{rowTo})";
                ws.Range[$"H{rowFrom - 1}", $"H{rowFrom - 1}"].FormulaLocal = $"=СУММ(H{rowFrom}:H{rowTo})";
                ws.Range[$"K{rowFrom - 1}", $"K{rowFrom - 1}"].FormulaLocal = $"=СУММ(K{rowFrom}:K{rowTo})";
                ws.Range[$"M{rowFrom - 1}", $"M{rowFrom - 1}"].FormulaLocal = $"=СУММ(M{rowFrom}:M{rowTo})";
                ws.Range[$"N{rowFrom - 1}", $"N{rowFrom - 1}"].FormulaLocal = $"=СУММ(N{rowFrom}:N{rowTo})";
                ws.Range[$"O{rowFrom - 1}", $"O{rowFrom - 1}"].FormulaLocal = $"=СУММ(O{rowFrom}:O{rowTo})";
                ws.Range[$"P{rowFrom - 1}", $"P{rowFrom - 1}"].FormulaLocal = $"=СУММ(P{rowFrom}:P{rowTo})";
                */
            }
        }

        public class HttpRequestClass
        {
        }

        public class SQLClass
        {
            public static void ExcelReadWithOpenDialogMethod()
            {
                /* OracleConnection */ /*
                OracleConnection connection;
                connection = new OracleConnection("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=172.28.254.215)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=pitenew))); User Id=dispatcher; Password=disp");
                if(connection.State == ConnectionState.Closed) connection.Open();
                string query = "";
                OracleCommand command = new OracleCommand(query, connection);
                command.BindByName = true;
                command.Parameters.Add("ParamDateFrom", OracleDbType.Date, dateFrom, ParameterDirection.Input);
                command.Parameters.Add("ParamShiftFrom", OracleDbType.Number, sFrom, ParameterDirection.Input);
                command.Parameters.Add("ParamDateTo", OracleDbType.Date, dateTo, ParameterDirection.Input);
                command.Parameters.Add("ParamShiftTo", OracleDbType.Number, sTo, ParameterDirection.Input);
                OracleDataAdapter adapter = new OracleDataAdapter(command);
                adapter.Fill(table);
                connection.Close();
                */

                /* Set combobox items */ /*
                combvehs.Properties.Items.Clear();
                LoadShov(vehs);
                foreach (DataRow row in vehs.Rows)
                {
                    combvehs.Properties.Items.Add(row["paramvehid"].ToString());
                }
                combvehs.SelectedIndex = 0;
                */
            }
        }

        public class UserInterfaceClass
        {
            public void ShowMethod(string message = "внимание", bool isXtra = false)
            {
                if (isXtra)
                {
                    DevExpress.XtraEditors.XtraMessageBox.Show($"{message}");
                }
                MessageBox.Show($"{message}");
            }

            //Form fullScreenForm = new Form();
            //fullScreenForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            //fullScreenForm.WindowState = FormWindowState.Maximized;
            //fullScreenForm.ShowInTaskbar = false;
            //this.Dock = DockStyle.Fill;
            //fullScreenForm.Controls.Add(this);
            //fullScreenForm.Show();
        }

        public class ThreadingClass
        {
            public static void ThreadingMethod()
            {
                /*
                Task t1 = Task.Run(() => {
                    while (true)
                    {
                        Console.WriteLine("Task.Run");

                        //m_xrtl.SetParameter("database_DataTable_Report", "paramTimeDifferent", (float)uiControl_SpinEdit_TimeDifferent.Value);
                        //database_DataTable_Report.Clear();
                        //GodClass.UtilsClass.XrtlFill(xrtl: m_xrtl, database_DataTable_Report, "database_DataTable_Report");

                        //DataSet dataSet = GodClass.TestingClass.CheckDatabase2(database_DataTable_Report);
                        //DataSet dataSet = GodClass.TestingClass.CheckDatabase2(database_DataTable_Report);
                        this.database_DataSet_Set1 = GodClass.TestingClass.CheckDatabase2();
                        this.uiView_GridControl_Control1.DataSource = database_DataSet_Set1.Tables[0];

                        this.bandedGridView1.BestFitColumns(true);
                        DateTime now = DateTime.Now;
                        this.uiControl_LabelControl_Legend.Text = $"обновлено: {now.Hour}:{now.Minute}:{now.Second}";
                        Thread.Sleep(2000);
                    }
                });
                
                t1.RunSynchronously();
                t1.Wait();
                */

                /*
                async Task<string> getRes(int aux)
                {
                   string result = Main.GodClass.ReportClass.RunMethodMonitoringAuxStoppages(
                                aux, (int)numericUpDown3.Value, dateTimePicker3.Value
                            );
                    return result;
                }  
                */
            }
        }

        public class AsyncClass
        {
            public static void AsyncMethod()
            {
                /*
                async Task Load(string[] args)
                {
                    database_DataTable_Report.Clear();
                    try
                    {
                        m_xrtl.SetParameter("database_DataTable_Report", "paramTimeDifferent", (float)uiControl_SpinEdit_TimeDifferent.Value);
                        GodClass.UtilsClass.XrtlFill(xrtl: m_xrtl, database_DataTable_Report, "database_DataTable_Report");

                        DateTime now = DateTime.Now;
                        uiControl_LabelControl_Legend.Text = $"обновлено: {now.Hour}:{now.Minute}:{now.Second}";
                        await LoadData();
                    Console.WriteLine("eggs are ready");
                }
                */
            }
        }
    }

    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Forms.AppContainer());

            //GodClass.TestingClass.RunMethod(702);

        }
    }
}
