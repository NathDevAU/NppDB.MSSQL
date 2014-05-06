using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    public class MSSQLParametaInfo : TreeNode
    {
        public string Title { set { this.Text = value; } get { return this.Text; } }
        public string ParamType { get; set; }
    }
}
