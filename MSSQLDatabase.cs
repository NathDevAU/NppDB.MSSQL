using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    public class MSSQLDatabase : TreeNode, IRefreshable, IMenuProvider
    {

        public MSSQLDatabase()
        {
            this.SelectedImageKey = this.ImageKey = "Database";
            Refresh();
        }

        public string Title { set { this.Text = value; } get { return this.Text; } }

        public void Refresh()
        {
            this.Nodes.Clear();
            this.Nodes.Add(new MSSQLTableGroup());
            this.Nodes.Add(new MSSQLSystemTableGroup());
            this.Nodes.Add(new MSSQLViewGroup());
            this.Nodes.Add(new MSSQLStoredProcedureGroup());
        }

        private ISQLExecutor CreateSQLExecutor(string connectionString)
        {
            var connStr = new SqlConnectionStringBuilder(connectionString);
            connStr.InitialCatalog = this.Title;
            return new MSSQLExecutor(connStr.ConnectionString);
        }

        public ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            var connect = this.Parent as MSSQLConnect;
            if (connect != null && connect.CommandHost != null)
            {
                var host = connect.CommandHost;
                menuList.Items.Insert(0,new ToolStripButton("Open", null, (s, e) =>
                {
                    host.Execute(NppDBCommandType.NewFile, null);
                    var id = host.Execute(NppDBCommandType.GetActivatedBufferID, null);
                    host.Execute(NppDBCommandType.CreateResultView, new object[] { id, connect, CreateSQLExecutor(connect.GetConnectionString()) });
                }));
                menuList.Items.Insert(1,new ToolStripSeparator());
            }
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }
    }
}
