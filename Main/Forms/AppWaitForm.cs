using Oracle;
using Excel = Microsoft.Office.Interop.Excel;
using XrtlExplorer;
using DevExpress;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Xml;

namespace Main.Forms
{
    public partial class AppWaitForm : DevExpress.XtraWaitForm.WaitForm
    {
        public AppWaitForm()
        {
            InitializeComponent();
            this.progressPanel1.AutoHeight = true;
        }

        public override void SetCaption(string caption)
        {
            base.SetCaption(caption);
            this.progressPanel1.Caption = caption;
        }
        public override void SetDescription(string description)
        {
            base.SetDescription(description);
            this.progressPanel1.Description = description;
        }
        public override void ProcessCommand(Enum cmd, object arg)
        {
            base.ProcessCommand(cmd, arg);
        }
    }
}
