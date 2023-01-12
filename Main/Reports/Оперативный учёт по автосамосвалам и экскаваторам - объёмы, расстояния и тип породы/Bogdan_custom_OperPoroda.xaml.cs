using System;
using System.Data;
using System.Threading.Tasks;

using _WinControls = System.Windows.Controls;
using _Utils = Main.Utils;


namespace Main.Reports
{
    #region wrapper

    public partial class Bogdan_custom_Container_OperPoroda : _Utils.WinForms.WinForms_Container
    {
        #region constructor

        public Bogdan_custom_Container_OperPoroda()
        {
            this.Report = new Bogdan_custom_OperPoroda(Xrtl_Container: this);  // set report to wrap
        }

        #endregion constructor
    }

    #endregion wrapper



    public partial class Bogdan_custom_OperPoroda : _WinControls.UserControl
    {
        #region variables

        public _Utils.WinForms.WinForms_Container Xrtl_Container;

        #endregion variables



        #region constructor

        public Bogdan_custom_OperPoroda(_Utils.WinForms.WinForms_Container Xrtl_Container)
        {
            InitializeComponent();



            #region IXrtlControl Interface Realization

            this.Xrtl_Container = Xrtl_Container;

            #endregion IXrtlControl Interface Realization



            #region Initialize User Interface settings

            DateTime previousMonth = _Utils.DateTime_.Get_PlusMonthCount_DateTime(monthCount: -1, dateTime: _Utils.DateTime_.Get_Now_DateTime());

            // truck
            _Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: DatePicker_DateFrom_Truck,
                    dateTime: new DateTime(previousMonth.Year, previousMonth.Month, 1)
                );
            _Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: DatePicker_DateTo_Truck,
                    dateTime: new DateTime(
                        previousMonth.Year,
                        previousMonth.Month,
                        _Utils.DateTime_.Get_LastDayInSelectMonth_Int(dateTime: previousMonth)
                    )
                );
            _Utils.Wpf.Set_Combobox_List(comboBox: ComboBox_SelectTechId_Truck, list: _Utils.Report.Get_Dumtruck_List());

            // shov
            _Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: DatePicker_DateFrom_Shov,
                    dateTime: new DateTime(previousMonth.Year, previousMonth.Month, 1)
                );
            _Utils.Wpf.Set_DataPicker_DateTime(
                    datePicker: DatePicker_DateTo_Shov,
                    dateTime: new DateTime(
                        previousMonth.Year,
                        previousMonth.Month,
                        _Utils.DateTime_.Get_LastDayInSelectMonth_Int(dateTime: previousMonth)
                    )
                );
            _Utils.Wpf.Set_Combobox_List(comboBox: ComboBox_SelectTechId_Shov, list: _Utils.Report.Get_Shovel_List());

            #endregion Initialize User Interface settings
        }

        #endregion constructor



        #region Analyse Truck

        private async void Button_StartAnalyze_Truck_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DateTime start = _Utils.DateTime_.Get_Now_DateTime();
                _Utils.Wpf.Set_ButtonText_String(button: Button_StartAnalyze_Truck, text: $"ОБНОВИТЬ \n-загрузка-");



                #region GET INPUT FROM USER INTERFACE

                DateTime paramDateFrom = _Utils.Wpf.Get_DataPicker_DateTime(datePicker: DatePicker_DateFrom_Truck);
                int paramShiftFrom = _Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_ShiftFrom_Truck);
                DateTime paramDateTo = _Utils.Wpf.Get_DataPicker_DateTime(datePicker: DatePicker_DateTo_Truck);
                int paramShiftTo = _Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_ShiftTo_Truck);
                string paramSelectTechId = _Utils.Wpf.Get_Combobox_String(comboBox: ComboBox_SelectTechId_Truck);
                int roundPoint = _Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_RoundedPoint_Truck);

                #endregion GET INPUT FROM USER INTERFACE



                #region ASYNC SELECT DATA

                DataTable dataTable = await Task.Run(() => _Utils.Report.OperPoroda.AnalyseTruck(
                        paramDateFrom: paramDateFrom,
                        paramShiftFrom: paramShiftFrom,
                        paramDateTo: paramDateTo,
                        paramShiftTo: paramShiftTo,
                        paramSelectTechId: paramSelectTechId,
                        roundPoint: roundPoint
                    ));

                #endregion ASYNC SELECT DATA



                #region UPDATE DATASOURCE

                _Utils.Wpf.Set_DataGridUpdate_DataTable(dataGrid: DataGrid_Truck, dataTable: dataTable);

                #endregion UPDATE DATASOURCE



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

        #endregion Analyse Truck



        #region Analyse Shov

        private async void Button_StartAnalyze_Shov_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                DateTime start = _Utils.DateTime_.Get_Now_DateTime();
                _Utils.Wpf.Set_ButtonText_String(button: Button_StartAnalyze_Shov, text: $"ОБНОВИТЬ \n-загрузка-");



                #region GET INPUT FROM USER INTERFACE

                DateTime paramDateFrom = _Utils.Wpf.Get_DataPicker_DateTime(datePicker: DatePicker_DateFrom_Shov);
                int paramShiftFrom = _Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_ShiftFrom_Shov);
                DateTime paramDateTo = _Utils.Wpf.Get_DataPicker_DateTime(datePicker: DatePicker_DateTo_Shov);
                int paramShiftTo = _Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_ShiftTo_Shov);
                string paramSelectTechId = _Utils.Wpf.Get_Combobox_String(comboBox: ComboBox_SelectTechId_Shov);
                int roundPoint = _Utils.Wpf.Get_Combobox_Int(comboBox: ComboBox_RoundedPoint_Shov);

                #endregion GET INPUT FROM USER INTERFACE



                #region ASYNC SELECT DATA

                DataTable dataTable = await Task.Run(() => _Utils.Report.OperPoroda.AnalyseShov(
                        paramDateFrom: paramDateFrom,
                        paramShiftFrom: paramShiftFrom,
                        paramDateTo: paramDateTo,
                        paramShiftTo: paramShiftTo,
                        paramSelectTechId: paramSelectTechId,
                        roundPoint: roundPoint
                    ));

                #endregion ASYNC SELECT DATA



                #region UPDATE DATASOURCE

                _Utils.Wpf.Set_DataGridUpdate_DataTable(dataGrid: DataGrid_Shov, dataTable: dataTable);

                #endregion UPDATE DATASOURCE



                _Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Shov,
                        text: $"ОБНОВИТЬ \n{_Utils.DateTime_.Get_FormatOnlyTime_String(_Utils.DateTime_.Get_Now_DateTime())} " +
                        $"({Math.Truncate((_Utils.DateTime_.Get_Now_DateTime() - start).TotalSeconds)} сек)"
                    );
            } catch (Exception exception)
            {
                _Utils.Debug.Set_ExceptionPrintAndShow(exception: exception, isShowScreenWindow: true);

                _Utils.Wpf.Set_ButtonText_String(
                        button: Button_StartAnalyze_Shov,
                        text: $"ОБНОВИТЬ \n{_Utils.DateTime_.Get_FormatOnlyTime_String(_Utils.DateTime_.Get_Now_DateTime())} (ошибка)"
                    );
            }
        }

        #endregion Analyse Shov
    }
}