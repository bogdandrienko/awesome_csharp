using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

using _WinForms = System.Windows.Forms;
using _WinControls = System.Windows.Controls;
using _Reports = Main.Reports;
using _Utils = Main.Utils;


namespace Main.Reports
{
    #region modal

    // TODO move add to Main.cs
    public partial class Bogdan_custom_ContainerModal_AuxMonitoringStoppages : _WinForms.UserControl
    {
        #region constructor

        public Bogdan_custom_ContainerModal_AuxMonitoringStoppages()
        {
            _WinForms.Form form = new _WinForms.Form();
            int Height = 390;
            int Width = 270;
            form.Height = Height;
            form.Width = Width;

            form.Controls.Add(new _Reports.Bogdan_custom_Container_AuxMonitoringStoppages());
            form.Show();
            form.Activate();
        }

        #endregion constructor
    }
    // TODO move add to Main.cs

    #endregion modal



    #region wrapper

    public partial class Bogdan_custom_Container_AuxMonitoringStoppages : _Utils.WinForms.WinForms_Container
    {
        #region constructor

        public Bogdan_custom_Container_AuxMonitoringStoppages()
        {
            this.Report = new Bogdan_custom_AuxMonitoringStoppages(Xrtl_Container: this);  // set report to wrap
        }

        #endregion constructor
    }

    #endregion wrapper



    public partial class Bogdan_custom_AuxMonitoringStoppages : _WinControls.UserControl
    {
        #region variables

        public _Utils.WinForms.WinForms_Container Xrtl_Container;
        public Dictionary<string, DateTime> TimeSkips = new Dictionary<string, DateTime>() { };

        #endregion variables



        #region constructor

        public Bogdan_custom_AuxMonitoringStoppages(_Utils.WinForms.WinForms_Container Xrtl_Container)
        {
            InitializeComponent();



            #region IXrtlControl Interface Realization

            this.Xrtl_Container = Xrtl_Container;

            #endregion IXrtlControl Interface Realization



            #region Initialize User Interface settings

            #endregion Initialize User Interface settings
        }

        #endregion constructor



        #region Monitoring
        private void ListBox_Menu_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            Start_Monitoring();
        }

        private void Skip(string key, bool isDelete)
        {
            if (isDelete)
            {
                this.TimeSkips.Remove(key: key);
            } else
            {
                this.TimeSkips[key] = _Utils.DateTime_.Get_Now_DateTime();
            }
        }

        private void Skip_Stoppage(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender.Equals(Button_Skip_601))
            {
                Skip(key: "601", isDelete: false);
            }
            if (sender.Equals(Button_Skip_603))
            {
                Skip(key: "603", isDelete: false);
            }
            if (sender.Equals(Button_Skip_607))
            {
                Skip(key: "607", isDelete: false);
            }
            if (sender.Equals(Button_Skip_608))
            {
                Skip(key: "608", isDelete: false);
            }
            if (sender.Equals(Button_Skip_609))
            {
                Skip(key: "609", isDelete: false);
            }
            if (sender.Equals(Button_Skip_702))
            {
                Skip(key: "702", isDelete: false);
            }
        }

        private async void Start_Monitoring()
        {
            List<Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>> tuples =
                new List<Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>>() {
                    new Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>(CheckBox_601_Active, TextBlock_601_Stoppage, Button_Skip_601),
                    new Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>(CheckBox_603_Active, TextBlock_603_Stoppage, Button_Skip_603),
                    new Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>(CheckBox_607_Active, TextBlock_607_Stoppage, Button_Skip_607),
                    new Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>(CheckBox_608_Active, TextBlock_608_Stoppage, Button_Skip_608),
                    new Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>(CheckBox_609_Active, TextBlock_609_Stoppage, Button_Skip_609),
                    new Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button>(CheckBox_702_Active, TextBlock_702_Stoppage, Button_Skip_702),
                };

            float speed = 7.0F;
            int localLoopTime = (int)(1000 / speed * 1);
            int globalLoopTime = (int)(localLoopTime * 10 / speed);
            bool alarm = false;
            while (true)
            {
                DateTime loopStart = _Utils.DateTime_.Get_Now_DateTime();
                DateTime CurrentDate = _Utils.DateTime_.Get_PlusDayCount_DateTime(dayCount: 0, dateTime: _Utils.DateTime_.Get_Now_DateTime());
                int CurrentShift = _Utils.DateTime_.Get_NowShift_Int();
                int timeDiff = 15;
                switch ((string)this.ComboBox_StoppageLimit.Text)
                {
                    case "Ультра-низкий (больше 1 мин)":
                        timeDiff = 1;
                        break;
                    case "Низкий (больше 10 мин)":
                        timeDiff = 10;
                        break;
                    case "Средний (больше 15 мин)":
                        timeDiff = 15;
                        break;
                    case "Высокий (больше 20 мин)":
                        timeDiff = 20;
                        break;
                    case "Ультра-высокий (больше 60 мин)":
                        timeDiff = 60;
                        break;
                    default:
                        timeDiff = 15;
                        break;
                }

                int timePast = 3;
                switch ((string)this.ComboBox_PastLimit.Text)
                {
                    case "Мгновенные (меньше 1 мин)":
                        timePast = 1;
                        break;
                    case "Свежие (меньше 3 мин)":
                        timePast = 3;
                        break;
                    case "Недавние (меньше 5 мин)":
                        timePast = 5;
                        break;
                    case "Устаревшие (меньше 10 мин)":
                        timePast = 10;
                        break;
                    case "Старые (меньше 60 мин)":
                        timePast = 60;
                        break;
                    default:
                        timePast = 3;
                        break;
                }

                int timeToSkip = 10;
                switch ((string)this.ComboBox_SkipLimit.Text)
                {
                    case "Минимальный (1 мин)":
                        timeToSkip = 1;
                        break;
                    case "Небольшой (5 мин)":
                        timeToSkip = 5;
                        break;
                    case "Средний (10 мин)":
                        timeToSkip = 10;
                        break;
                    case "Высокий (15 мин)":
                        timeToSkip = 15;
                        break;
                    case "Очень высокий (60 мин)":
                        timeToSkip = 60;
                        break;
                    default:
                        timeToSkip = 10;
                        break;
                }

                alarm = false;

                foreach (Tuple<_WinControls.CheckBox, _WinControls.TextBlock, _WinControls.Button> tuple in tuples)
                {
                    DateTime forStart = _Utils.DateTime_.Get_Now_DateTime();
                    int auxid = int.Parse((string)tuple.Item1.Content);
                    bool skipTime = false;

                    DateTime timeSkip = _Utils.DateTime_.Get_Now_DateTime();
                    if (this.TimeSkips.TryGetValue($"{auxid}", out timeSkip))
                    {
                        double res = (_Utils.DateTime_.Get_Now_DateTime() - timeSkip).TotalSeconds - timeToSkip * 60;
                        if (res < 0)
                        {
                            skipTime = true;
                            tuple.Item3.Content = $"{(int)res}";
                            _Utils.Wpf.Set_BackgroundControl(element: tuple.Item3, alfa: 0xBB, red: 0xBB, green: 0xBB, blue: 0xBB);
                        }
                        else {
                            skipTime = false;
                            tuple.Item3.Content = $"пропустить";
                            _Utils.Wpf.Set_BackgroundControl(element: tuple.Item3, alfa: 0xEE, red: 0xEE, green: 0xEE, blue: 0xEE);
                            Skip(key: $"{auxid}", isDelete: true);
                        }
                    }

                    if (!(bool)tuple.Item1.IsChecked || skipTime)
                    {
                        tuple.Item2.Text = "";
                        _Utils.Wpf.Set_BackgroundTextBlock(element: tuple.Item2, alfa: 0, red: 0xFF, green: 0, blue: 0xFF);
                        continue;
                    }

                    DataTable result1 = await Task.Run(() => _Utils.Report.AuxStoppages.GetDataFirst(
                        paramDateFrom: CurrentDate,
                        paramShiftFrom: CurrentShift,
                        paramDateTo: CurrentDate,
                        paramShiftTo: CurrentShift,
                        paramSelectTechId: auxid
                    ));
                    DataTable result2 = await Task.Run(() => _Utils.Report.AuxStoppages.GetDataSecond(dataTable: result1, timeDiff: timeDiff));

                    if (result2.Rows.Count > 0)
                    {
                        DateTime lastDateTime = DateTime.ParseExact((string)result2.Rows[0]["3"], "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        if ((forStart - lastDateTime).TotalMinutes < timePast)
                        {
                            tuple.Item2.Text = $"{((string)result2.Rows[0]["2"]).Split(new string[] { " " }, StringSplitOptions.None)[1]} - " +
                                $"{((string)result2.Rows[0]["3"]).Split(new string[] { " " }, StringSplitOptions.None)[1]} " +
                                $"({result2.Rows[0]["4"]} мин | {result2.Rows.Count} шт)";
                            _Utils.Wpf.Set_BackgroundTextBlock(element: tuple.Item2, alfa: 0x66, red: 0xFF, green: 0, blue: 0);

                            alarm = true;
                        } else
                        {
                            tuple.Item2.Text = "";
                            _Utils.Wpf.Set_BackgroundTextBlock(element: tuple.Item2, alfa: 0, red: 0xFF, green: 0, blue: 0xFF);
                        }
                    } else
                    {
                        if (result1.Rows.Count > 0)
                        {
                            tuple.Item2.Text = "";
                            _Utils.Wpf.Set_BackgroundTextBlock(element: tuple.Item2, alfa: 0, red: 0xFF, green: 0, blue: 0xFF);
                        } else
                        {
                            tuple.Item2.Text = "нет данных";
                            _Utils.Wpf.Set_BackgroundTextBlock(element: tuple.Item2, alfa: 0x66, red: 0x66, green: 0x66, blue: 0x66);
                        }
                    }

                    if (alarm)
                    {
                        for (int i = 0; i < 10; i += 1)
                        {
                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Header, alfa: 0xFF, red: 0xFF, green: 0x32, blue: 0x32);
                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer1, alfa: 0xFF, red: 0xFF, green: 0x32, blue: 0x32);
                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer2, alfa: 0xFF, red: 0xFF, green: 0x32, blue: 0x32);
                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer3, alfa: 0xFF, red: 0xFF, green: 0x32, blue: 0x32);
                            _Utils.Wpf.Set_Visibility(element: this.Label_Alarm, visibility: true);

                            await _Utils.Debug.DelayAsync(milliseconds: 50);

                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Header, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer1, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer2, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                            _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer3, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                            _Utils.Wpf.Set_Visibility(element: this.Label_Alarm, visibility: false);
                        }
                    } else
                    {
                        _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Header, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                        _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer1, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                        _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer2, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                        _Utils.Wpf.Set_BackgroundControl(element: this.ListBox_Footer3, alfa: 0, red: 0xFF, green: 0xFF, blue: 0xFF);
                        _Utils.Wpf.Set_Visibility(element: this.Label_Alarm, visibility: false);
                    }

                    if (_Utils.DateTime_.Get_Difference_Milliseconds(dateTime1: _Utils.DateTime_.Get_Now_DateTime(), dateTime2: forStart) < localLoopTime)
                    {
                        await _Utils.Debug.DelayAsync(milliseconds: localLoopTime);
                    } else
                    {
                        await _Utils.Debug.DelayAsync(milliseconds: 25);
                    }
                }
                if (_Utils.DateTime_.Get_Difference_Milliseconds(dateTime1: _Utils.DateTime_.Get_Now_DateTime(), dateTime2: loopStart) < globalLoopTime)
                {
                    await _Utils.Debug.DelayAsync(milliseconds: globalLoopTime);
                } else
                {
                    await _Utils.Debug.DelayAsync(milliseconds: 25);
                }
            }
        }

        #endregion Monitoring
    }
}
