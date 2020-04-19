using IBM.Data.DB2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Odbc;

namespace DemandForecastImportCSV
{
    public static class InformixHelper
    {


        public static DB2DataReader ExecuteReader(string connectionString, CommandType commandType, string commandText)
        {
            DB2Connection conn = new DB2Connection(connectionString);
            try
            {
                //  conn = new DB2Connection(connectionString);
                conn.Open();
                DB2Command cmd = new DB2Command(commandText, conn);
                cmd.CommandType = CommandType.Text;
                return cmd.ExecuteReader();

            }
            catch (Exception ex)
            {
                Form1.WriteLog(ex.Message + "SQL Query " + commandText + "ExecuteReader");
                return null;                
            }
            finally
            {
                //if (conn.State == ConnectionState.Open)
                //{
                //    conn.Close();
                //    conn = null;
                //  //  conn.ClearUSRLIBLCache();
                //}
            }


        }

        public static int ExecuteScalar(string connectionString, CommandType commandType, string commandText)
        {
            DB2Connection conn = new DB2Connection(connectionString);
            try
            {
                conn.Open();
                DB2Command cmd = new DB2Command(commandText, conn);
                cmd.CommandType = CommandType.Text;
                return Convert.ToInt32(cmd.ExecuteScalar());

            }
            catch (Exception ex)
            {
                Form1.WriteLog(ex.Message + "SQL Query " + commandText + "ExecuteScalar");
                return 0;
                
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    conn = null;
                }
            }
        }

        public static int ExecuteNonQuery(string connectionString, CommandType commandType, string commandText)
        {
            DB2Connection conn = new DB2Connection(connectionString);
            //OdbcConnection conn = new OdbcConnection("DSN=informix117");
            try
            {
                conn.Open();
                DB2Command cmd = new DB2Command(commandText, conn);
                //OdbcCommand cmd = new OdbcCommand(commandText, conn);
                return cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                //Form1.WriteLog(ex.Message + "SQL Query " + commandText + "ExecuteScalar");
                throw ex;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                    conn = null;
                }
            }

        }

        public static DataTable ExecuteDatatable(string connectionString, CommandType commandType, string commandText)
        {
            DataTable Data = new DataTable();
            DB2Connection conn = new DB2Connection(connectionString);
            try
            {
                DB2Command cmd = new DB2Command(commandText, conn);
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;
                DB2DataAdapter da = new DB2DataAdapter(cmd);
                da.Fill(Data);

                return Data;
            }
            catch (Exception ex)
            {
                Form1.WriteLog(ex.Message + "SQL Query " + commandText + "ExecuteDataset");
                return null;
            }


        }




    }
}
