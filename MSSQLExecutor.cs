using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using NppDB.Comm;

namespace NppDB.MSSQL
{
    public class MSSQLExecutor : ISQLExecutor 
    {
        private System.Threading.Thread _execTh = null;
        private string _connStr = null;
        private SqlConnection _conn = null;
        private string _sid = "";
        private bool _isExecuting = false;
        private bool _stop = false;

        public MSSQLExecutor(string connectionString)
        {
            _connStr = connectionString;

            //_cmd = connector connection.CreateCommand();
        }

        public virtual void Execute(string sqlQuery, Action<Exception,DataTable> callback)
        {
            _isExecuting = true;
            _stop = false;
            _execTh = new System.Threading.Thread(new System.Threading.ThreadStart(
                delegate
                {
                    if (_conn == null) _conn = new SqlConnection(_connStr);
                    if (_conn.State == ConnectionState.Closed)
                    {
                        try
                        {
                            _conn.Open();
                            var cmd = new SqlCommand("select @@spid", _conn);
                            var sid = cmd.ExecuteScalar();
                            _sid = Convert.ToInt32(sid).ToString();
                        }
                        catch (Exception ex)
                        {
                            callback(ex, null);
                            return;
                        }
                    }

                    var dt = new DataTable();
                    var keyCounts = new Dictionary<string,int>();
                    try
                    {
                        var cmd = new SqlCommand(sqlQuery, _conn);
                        var rd = cmd.ExecuteReader();
                        try
                        {
                            for (int i = 0; i < rd.FieldCount; i++)
                            {
                                var colNm = rd.GetName(i);
                                if(!keyCounts.ContainsKey(colNm)) keyCounts[colNm] = 0;
                                if(dt.Columns.Contains(colNm)) keyCounts[colNm]++;
                                var colType = rd.GetFieldType(i);
                                var colKey = dt.Columns.Contains(colNm) ? colNm + keyCounts[colNm] : colNm;
                                dt.Columns.Add(new DataColumn { Caption = colNm, DataType = colType, ColumnName = colKey });
                            }
                            var row = new object[rd.FieldCount];
                            
                            while (rd.Read())
                            {
                                if (_stop)
                                {
                                    _isExecuting = false;
                                    _stop = false;
                                    callback(new ApplicationException("stoped"), null);
                                    return;
                                }
                                for (int i = 0; i < rd.FieldCount; i++)
                                {
                                    row[i] = rd.GetValue(i);
                                }
                                dt.Rows.Add(row);
                            }
                        }
                        finally
                        {
                            cmd.Cancel();
                            rd.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        _isExecuting = false;
                        _stop = false;
                        callback(ex, null);
                        return;
                    }
                    _isExecuting = false;
                    _stop = false;
                    callback(null, dt);
                }));
            _execTh.IsBackground = true;
            _execTh.Start();
        }

        public virtual Boolean CanExecute()
        {
            return !CanStop();
        }

        public virtual void Stop()
        {
            if (!CanStop()) return;
            if (_execTh != null && (_execTh.ThreadState & System.Threading.ThreadState.Running) == System.Threading.ThreadState.Running)
            {
                _stop = true;
                while (CanStop()) System.Threading.Thread.Sleep(10);
            }
            _execTh = null;
        }

        public virtual bool CanStop()
        {
            return _isExecuting;
        }

    }
}
