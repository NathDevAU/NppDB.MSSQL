using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    public class MSSQLColumnInfo : TreeNode
    {
        public MSSQLColumnInfo(string columnName, string columnType)
        {
            this.ColumnName = columnName;
            this.ColumnType = columnType;
            this.Title = this.Title + (string.IsNullOrEmpty(this.ColumnType) ? "" : "  : " + this.ColumnType);
            //monospace font
            var font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular);
            this.NodeFont = font;
        }
        public string Title
        {
            get { return this.Text; }
            private set { this.Text = value; }
        }
        public void AdjustColumnNameFixedWidth(int fixedWidth)
        {
            this.Title = this.ColumnName.PadRight(fixedWidth) + (string.IsNullOrEmpty(this.ColumnType) ? "" : "  " + this.ColumnType);
        }
        public string ColumnName { get; private set; }
        public string ColumnType { get; private set; }
    }

}
