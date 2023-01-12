using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

using WpfToolkit = Xceed.Wpf.Toolkit;
using _WinControls = System.Windows.Controls;
using _Utils = Main.Utils;


namespace Main.Reports
{
    #region wrapper

    public partial class Bogdan_custom_Container_AuxReportStoppages : _Utils.WinForms.WinForms_Container
    {
        #region constructor

        public Bogdan_custom_Container_AuxReportStoppages()
        {
            this.Report = new Bogdan_custom_AuxReportStoppages(Xrtl_Container: this);  // set report to wrap
        }

        #endregion constructor
    }

    #endregion wrapper



    public partial class Bogdan_custom_AuxReportStoppages : _WinControls.UserControl
    {
        #region variables

        public _Utils.WinForms.WinForms_Container Xrtl_Container;
        List<List<object>> matrix = new List<List<object>>() { };

        #endregion variables



        #region constructor

        public Bogdan_custom_AuxReportStoppages(_Utils.WinForms.WinForms_Container Xrtl_Container)
        {
            InitializeComponent();



            #region IXrtlControl Interface Realization

            this.Xrtl_Container = Xrtl_Container;

            #endregion IXrtlControl Interface Realization



            #region Initialize User Interface settings

            DateTime now = _Utils.DateTime_.Get_Now_DateTime();

            // aux
            _Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: this.DatePicker_DateFrom_Aux,
                    dateTime: _Utils.DateTime_.Get_PlusDayCount_DateTime(dayCount: -1, dateTime: now)
                );
            _Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: this.DatePicker_DateTo_Aux,
                    dateTime: _Utils.DateTime_.Get_PlusDayCount_DateTime(dayCount: -1, dateTime: now)
                );
            _Utils.Wpf.Set_Combobox_List(comboBox: this.ComboBox_SelectTechId_Truck, list: _Utils.Report.Get_Aux_List());

            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_From_5,
                    dateTime: now
                );
            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_To_5,
                    dateTime: now
                );

            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_From_6,
                    dateTime: now
                );
            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_To_6,
                    dateTime: now
                );

            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_From_7,
                    dateTime: now
                );
            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_To_7,
                    dateTime: now
                );

            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_From_8,
                    dateTime: now
                );
            _Utils.Wpf.Set_DataTimePicker_DateTime(
                    dateTimePicker: DateTimePicker_To_8,
                    dateTime: now
                );

            #endregion Initialize User Interface settings
        }

        #endregion constructor



        #region Report

        private async void Button_StartAnalyze_Truck_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DateTime start = _Utils.DateTime_.Get_Now_DateTime();
                _Utils.Wpf.Set_ButtonText_String(button: Button_StartAnalyze_Truck, text: $"ОБНОВИТЬ \n-загрузка-");



                #region GET INPUT FROM USER INTERFACE

                DateTime paramDateFrom = this.DatePicker_DateFrom_Aux.SelectedDate.Value;
                int paramShiftFrom = int.Parse(this.ComboBox_ShiftTo_Truck.Text);
                DateTime paramDateTo = this.DatePicker_DateTo_Aux.SelectedDate.Value;
                int paramShiftTo = int.Parse(this.ComboBox_ShiftTo_Truck.Text);
                string paramSelectTechId = $"{this.ComboBox_SelectTechId_Truck.Text}";
                int timeDiff = int.Parse(this.ComboBox_RoundedPoint_Truck.Text);

                List<Tuple<DateTime, DateTime>> excludes = new List<Tuple<DateTime, DateTime>>() { };

                List<Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>> tuples = new List<Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>>()
                    {
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_1, DateTimePicker_From_1, DateTimePicker_To_1),
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_2, DateTimePicker_From_2, DateTimePicker_To_2),
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_3, DateTimePicker_From_3, DateTimePicker_To_3),
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_4, DateTimePicker_From_4, DateTimePicker_To_4),
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_5, DateTimePicker_From_5, DateTimePicker_To_5),
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_6, DateTimePicker_From_6, DateTimePicker_To_6),
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_7, DateTimePicker_From_7, DateTimePicker_To_7),
                        new Tuple<_WinControls.CheckBox, WpfToolkit.DateTimePicker, WpfToolkit.DateTimePicker>(CheckBox_Active_8, DateTimePicker_From_8, DateTimePicker_To_8),
                    };

                foreach (var tuple in tuples)
                {
                    if ((bool)tuple.Item1.IsChecked)
                    {
                        excludes.Add(new Tuple<DateTime, DateTime>((DateTime)tuple.Item2.Value, (DateTime)tuple.Item3.Value));
                    }
                }

                #endregion GET INPUT FROM USER INTERFACE



                #region get data

                this.matrix = await Task.Run(() => _Utils.Report.AuxStoppages.GetData(
                        paramDateFrom: paramDateFrom,
                        paramShiftFrom: paramShiftFrom,
                        paramDateTo: paramDateTo,
                        paramShiftTo: paramShiftTo,
                        paramSelectTechId: paramSelectTechId,
                        timeDiff: timeDiff,
                        excludes: excludes,
                        Xrtl_Container: Xrtl_Container
                    ));
                DataTable result = _Utils.DataTable_.Get_ConvertList_DataTable(new List<string>() { "1", "2", "3", "4" }, matrix: this.matrix);
                _Utils.Wpf.Set_DataGridUpdate_DataTable(dataGrid: DataGrid_Stoppages, dataTable: result);
                TextBlock_Stoppages_3.Text = $"результат: {result.Rows.Count}";

                #endregion get data



                _Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Truck,
                        text: $"ОБНОВИТЬ \n{_Utils.DateTime_.Get_FormatOnlyTime_String(_Utils.DateTime_.Get_Now_DateTime())} " +
                              $"({Math.Truncate((_Utils.DateTime_.Get_Now_DateTime() - start).TotalSeconds)} сек)"
                    );
            } catch (Exception exception)
            {
                _Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                _Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Truck,
                        text: $"ОБНОВИТЬ \n{_Utils.DateTime_.Get_FormatOnlyTime_String(_Utils.DateTime_.Get_Now_DateTime())} (ошибка)"
                    );
            }
        }

        private void Button_CreateReport_Truck_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                #region FILL EXCEL

                if (this.matrix.Count < 2)
                {
                    _Utils.Debug.Set_ShowToWindowScreen(text: $"Данных нет! Обновите данные или изменить фильтр!", isXrtl: false);
                } else
                {
                   _Utils.Excel_ excelClass = new _Utils.Excel_();
                    excelClass.Method_Hide_Application(isVisible: false);
                    excelClass.Method_Load_Template(
                            templateName: "template1",
                            templateNameDebug: "Отчёт простои бульдозеров.xlsx",
                            Xrtl_Container: Xrtl_Container
                        );
                    excelClass.Method_Fill_Sheet_From_List(
                            matrix:_Utils.Extra.Get_Sliced_List(matrix: this.matrix, startIndex: 1, stopIndex: -1),
                            startRow: 6,
                            startCol: 1
                        );
                    excelClass.Method_Set_Cell_Value(
                            rowIndex: 3,
                            colIndex: 1,
                            value: $"сформировано: {Utils.DateTime_.Get_FormatOnlyDateTime_String(Utils.DateTime_.Get_Now_DateTime())}"
                        );
                    excelClass.Method_Hide_Application(isVisible: true);
                }

                #endregion FILL EXCEL
            } catch (Exception exception)
            {
                _Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                _Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Truck,
                        text: $"ОБНОВИТЬ \n{_Utils.DateTime_.Get_FormatOnlyTime_String(_Utils.DateTime_.Get_Now_DateTime())} (ошибка)"
                    );
            }
        }
        
        #endregion Report
    }
}
