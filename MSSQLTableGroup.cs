using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    public class MSSQLTableGroup : TreeNode, IRefreshable, IMenuProvider
    {

        public MSSQLTableGroup()
        {
            this.Text = "Tables";
            this.SelectedImageKey = this.ImageKey = "Group";
        }

        public string Title { set { this.Text = value; } get { return this.Text; } }

        protected virtual string QueryString
        {
            get
            {
                return "select distinct table_schema as obj_schema, table_name as obj_name from " + this.Parent.Text + ".information_schema.tables ";
            }
        }

        protected virtual TreeNode CreateTreeNode(System.Data.DataRow dataRow)
        {
            var tbl = new MSSQLTable();
            tbl.Title = dataRow["obj_name"].ToString();
            tbl.DBSchema = dataRow["obj_schema"].ToString();
            return tbl;
        }

        public void Refresh()
        {
            var conn = (MSSQLConnect)this.Parent.Parent;
            using (var cnn = conn.GetConnection())
            {
                this.TreeView.Cursor = Cursors.WaitCursor;
                this.TreeView.Enabled = false;
                try
                {
                    cnn.Open();
                    SqlCommand cmd = new SqlCommand(QueryString, cnn);
                    var reader = cmd.ExecuteReader();
                    var dt = new System.Data.DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        this.Nodes.Clear();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            this.Nodes.Add(CreateTreeNode(row));
                        }
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

        public virtual ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }
    }
}
