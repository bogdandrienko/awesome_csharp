using _WinForms = System.Windows.Forms;
using _Forms = Main.Forms;
using _Reports = Main.Reports;

namespace Main.Forms
{
    public partial class AppContainer : _WinForms.Form
    {
        public AppContainer()
        {
            InitializeComponent();
            //Main.GodClass.TestingClass.RunMethod();

            this.Controls.Add(new _Reports.Bogdan_custom_ContainerModal_AuxMonitoringStoppages());  // Мониторинг простоев бульдозеров
            // this.Controls.Add(new _Reports.Bogdan_custom_Container_AuxReportStoppages());  // Отчёт по простоям бульдозеров

            //this.Controls.Add(new _Reports.Bogdan_custom_ContainerModal_ShovPredictiveAnalyze());  // Предиктивный анализ производительности



            // this.Controls.Add(new _Reports.Bogdan_custom_Container_OperPoroda()); // Оперативный учёт по автосамосвалам и экскаваторам - объёмы, расстояния и тип породы
            // this.Controls.Add(new _Reports.Bogdan_custom_Container_ShovDriverPerfomance()); // Объёмы г.м. и производительность по машинистам экскаваторов
            // this.Controls.Add(new _Reports.Bogdan_custom_TruckTripTicket()); // Путевой лист диспетчера АТЦ
            // this.Controls.Add(new _Reports.Bogdan_custom_Container_FindTech()); // Поиск техники в координатах

            // визуализация мгновенных показателей
            //_Forms.TechMonitoringForm1 form3 = new _Forms.TechMonitoringForm1();
            //form3.Show();
            //_Forms.TechMonitoringForm2 form4 = new _Forms.TechMonitoringForm2();
            //form4.Show();

            //_Forms.TechMonitoringForm3 form5 = new _Forms.TechMonitoringForm3();
            //form5.Show();
        }

        private void AppContainer_Load(object sender, System.EventArgs e)
        {

        }
    }
}