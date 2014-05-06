using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    public class MSSQLTable : TreeNode, IRefreshable, IMenuProvider, IMSSQLObject
    {
        public MSSQLTable()
        {
            this.SelectedImageKey = this.ImageKey = "Table";
        }
        public string Title { set { this.Text = value; } get { return this.Text; } }
        public string DBSchema { get; set; }
        public string FullName { get { return DBSchema + "." + Title; } }

        protected virtual string QueryString
        {
            get
            {
                return "select distinct column_name, data_type, character_maximum_length from " + this.Parent.Parent.Text + ".information_schema.columns where table_name='" + Title + "'";
            }
        }

        public virtual void Refresh()
        {
            var conn = (MSSQLConnect)this.Parent.Parent.Parent;
            using (var cnn = conn.GetConnection())
            {
                this.TreeView.Cursor = Cursors.WaitCursor;
                this.TreeView.Enabled = false;
                try
                {
                    cnn.Open();
                    var cmd = new SqlCommand(QueryString, cnn);
                    var reader = cmd.ExecuteReader();
                    var dt = new System.Data.DataTable();
                    dt.Load(reader);
                    int maxLen = -1;
                    if (dt.Rows.Count > 0)
                    {
                        this.Nodes.Clear();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            var col = new MSSQLColumnInfo(row["column_name"].ToString(),row["data_type"].ToString() + (row["character_maximum_length"] is DBNull ? "" : "(" + row["character_maximum_length"].ToString() + ")"));
                            maxLen = Math.Max(col.ColumnName.Length, maxLen);
                            this.Nodes.Add(col);
                        }
                        if(maxLen > -1)
                            foreach (var colInfo in this.Nodes)
                                ((MSSQLColumnInfo)colInfo).AdjustColumnNameFixedWidth(maxLen);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception");
                    return;
                }
                finally
                {
                    this.TreeView.Enabled = true;
                    this.TreeView.Cursor = null;
                }
            }
        }

        protected ISQLExecutor CreateSQLExecutor(string connectionString, string Database)
        {
            var connStr = new SqlConnectionStringBuilder(connectionString);
            connStr.InitialCatalog = Database;
            return new MSSQLExecutor(connStr.ConnectionString);
        }

        public virtual ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip(){ShowImageMargin= false};
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));
            var connect = this.Parent.Parent.Parent as MSSQLConnect;
            if (connect != null && connect.CommandHost !=null )
            {
                var host = connect.CommandHost;
                menuList.Items.Insert(0,new ToolStripButton("Select top 100 ...", null, (s, e) =>
                {
                    host.Execute(NppDBCommandType.NewFile, null);
                    var id = host.Execute(NppDBCommandType.GetActivatedBufferID, null);
                    var query = "Select Top 100 * From " + this.FullName + "\n";
                    host.Execute(NppDBCommandType.AppendToCurrentView, new object[]{query});
                host.Execute(NppDBCommandType.CreateResultView, new object[] { id, connect, CreateSQLExecutor(connect.GetConnectionString(), ((MSSQLDatabase)this.Parent.Parent).Title) });
                    host.Execute(NppDBCommandType.ExecuteSQL, new object[] { id, query });
                }));
                menuList.Items.Insert(1, new ToolStripSeparator());
            }
            return menuList;
        }
    }
}
