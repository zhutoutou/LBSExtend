using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZIT.LBSExtend.Utility;

namespace ZIT.LBSExtend.UI
{
    public partial class frmAbout : Form
    {
        public frmAbout()
        {
            InitializeComponent();
            this.Text = "关于-" + SysParameters.SoftName;
            this.label3.Text = string.Format("中兴120急救指挥调度系统-{0}", SysParameters.SoftName);
        }
    }
}
