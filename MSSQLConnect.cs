using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SqlClient;
using System.Xml.Serialization;
using System.Windows.Forms;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    [XmlRoot]
    [ConnectAttr(Id = "MSSQLConnect", Title = "MS SQL Server")]
    public class MSSQLConnect : TreeNode, IDBConnect, IRefreshable, IMenuProvider, IIconProvider, INppDBCommandClient
    {

        //only one
        [XmlElement]
        public string Title { set { this.Text = value; } get { return this.Text;} }
        //ip[:port]
        [XmlElement]
        public string ServerAddress { set; get; }
        [XmlElement]
        public string Account { set; get; }
        [XmlIgnore]
        public string Password { set; get; }

        [XmlElement]
        public string InitialCatalog { set; get; }
        [XmlElement]
        public int ConnectTimeout { set; get; }

        private SqlConnection _conn = null;

        public string GetDefaultTitle()
        {
            return string.IsNullOrEmpty(ServerAddress) ? "" : ServerAddress;
        }

        public System.Drawing.Bitmap GetIcon()
        {
            return Properties.Resources.SqlServer;
        }

        public bool CheckLogin()
        {
            this.ToolTipText = this.ServerAddress;
            frmMSSQLConnect dlg = new frmMSSQLConnect();
            dlg.ServerAddress = this.ServerAddress;
            dlg.LoginID = this.Account;
            dlg.Password = this.Password;
            dlg.InitCatalog = this.InitialCatalog;
            dlg.ConnectTimeout = this.ConnectTimeout;

            if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK) return false;
            this.ServerAddress = dlg.ServerAddress.Trim();
            this.Account = dlg.LoginID.Trim();
            this.Password = dlg.Password.Trim();
            this.InitialCatalog = dlg.InitCatalog.Trim();
            this.ConnectTimeout = dlg.ConnectTimeout;
            
            return true;
        }

        public void Connect()
        {
            if (_conn == null) _conn = new SqlConnection();
            string curConnStr = GetConnectionString();
            if (_conn.ConnectionString != curConnStr) _conn.ConnectionString = curConnStr;
            if (_conn.State != System.Data.ConnectionState.Open)
            {
                try { _conn.Open(); }
                catch (Exception ex)
                { throw new ApplicationException("connect fail", ex); }
            }
        }

        public void Disconnect()
        {
            if (_conn == null) return;
            if (_conn.State != System.Data.ConnectionState.Closed) _conn.Close();
        }

        public bool IsOpened { get { return _conn != null && _conn.State == System.Data.ConnectionState.Open; } }

        internal SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        internal ISQLExecutor CreateSQLExecutor() { return new MSSQLExecutor(GetConnectionString()); }

        public void Refresh()
        {
            using (var conn = GetConnection())
            {
                this.TreeView.Cursor = Cursors.WaitCursor;
                this.TreeView.Enabled = false;
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("EXEC sp_databases", _conn);
                    var reader = cmd.ExecuteReader();
                    var dt = new System.Data.DataTable();
                    dt.Load(reader);
                    if (dt.Rows.Count > 0)
                    {
                        this.Nodes.Clear();
                        foreach (System.Data.DataRow row in dt.Rows)
                        {
                            var db = new MSSQLDatabase() { Title = row["DATABASE_NAME"].ToString() };
                            this.Nodes.Add(db);
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

        public ContextMenuStrip GetMenu()
        {
            var menuList = new ContextMenuStrip() { ShowImageMargin = false };
            menuList.Items.Add(new ToolStripButton("Connect", null, (s, e) =>
            {
                if (this.IsOpened || this.Password != null) return;
                if (!CheckLogin()) return;
                try
                {
                    Connect();
                    Refresh();
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message + (ex.InnerException != null ? " : " + ex.InnerException.Message : ""));
                    return;
                }
            }) { Enabled = !this.IsOpened });
            menuList.Items.Add(new ToolStripButton("Disconnect", null, (s, e) => { Disconnect(); }) { Enabled = this.IsOpened });
            menuList.Items.Add(new ToolStripButton("Refresh", null, (s, e) => { Refresh(); }));

            return menuList;
        }

        internal string GetConnectionString()
        {
            var cSBuilder = new SqlConnectionStringBuilder();
            cSBuilder["server"] = ServerAddress;
            cSBuilder["user id"] = Account;
            cSBuilder["password"] = Password;
            if (!string.IsNullOrEmpty(InitialCatalog)) cSBuilder["initial catalog"] = InitialCatalog;
            if (ConnectTimeout > 0) cSBuilder["Connect Timeout"] = ConnectTimeout;

            return cSBuilder.ConnectionString;
        }

        public void Reset()
        {
            Title = ""; ServerAddress = ""; Account = ""; Password = ""; InitialCatalog = ""; ConnectTimeout = -1;
            Disconnect();
            _conn = null;
        }


        internal INppDBCommandHost CommandHost { get; private set; }

        public void SetCommandHost(INppDBCommandHost host)
        {
            CommandHost = host;
        }
    }

}
