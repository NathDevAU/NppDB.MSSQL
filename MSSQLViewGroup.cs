using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace NppDB.MSSQL
{
    public class MSSQLViewGroup : MSSQLTableGroup
    {
        public MSSQLViewGroup()
        {
            this.Text = "Views";
        }
        protected override string QueryString
        {
            get
            {
                return string.Format("select distinct table_schema as obj_schema, table_name as obj_name from " + this.Parent.Text + ".information_schema.views ");
            }
        }

        protected override TreeNode CreateTreeNode(System.Data.DataRow dataRow)
        {
            var vw = new MSSQLView();
            vw.Title = dataRow["obj_name"].ToString();
            vw.DBSchema = dataRow["obj_schema"].ToString();
            return vw;
        }
    }
}
