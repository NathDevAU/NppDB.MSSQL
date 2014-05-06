using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace NppDB.MSSQL
{
    public class MSSQLSystemTable : MSSQLTable
    {
        public MSSQLSystemTable()
        {
            this.SelectedImageKey = this.ImageKey = "Table";
        }
        protected override string QueryString
        {
            get
            {
                return "select distinct column_name, data_type, character_maximum_length from " + this.Parent.Parent.Text + ".information_schema.columns where table_name='" + Title + "'";
            }
        }

    }
}
