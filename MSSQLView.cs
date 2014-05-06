using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    public class MSSQLView : TreeNode, IRefreshable, IMenuProvider, IMSSQLObject
    {
        public MSSQLView()
        {
            this.SelectedImageKey = this.ImageKey = "Table";
        }
        public string Title { set { this.Text = value; } get { return this.Text; } }
        public string DBSchema { get; set; }
        public string FullName { get { return Title + "." + DBSchema; } }

        public virtual void Refresh()
        {
            
        }
        public ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }
    }
}
