using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace NppDB.MSSQL
{
    public class MSSQLSystemTableGroup : MSSQLTableGroup
    {
        public MSSQLSystemTableGroup()
        {
            this.Text = "SystemTables";
        }

        protected override string QueryString
        {
            get
            {
                return string.Format("select o.name as obj_name, s.name as obj_schema  from  {0}.sys.objects o join {0}.sys.schemas s on o.schema_id = s.schema_id where type = 'S' order by o.name", this.Parent.Text);
            }
        }

        protected override TreeNode CreateTreeNode(System.Data.DataRow dataRow)
        {
            var tbl = new MSSQLSystemTable();
            tbl.Title = dataRow["obj_name"].ToString();
            tbl.DBSchema = dataRow["obj_schema"].ToString();
            return tbl;
        }

    }
}
