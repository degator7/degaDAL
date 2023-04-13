using dega.Sql;
using degaEncDec;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace dega
{
    public class degaDataAccessLayer
    {

        private Database dsn;
        private SecurityConnectionType securityConnectionType = SecurityConnectionType.Unconfigured;

        private enum SecurityConnectionType
        {
            All,
            Password,
            Unconfigured
        }

        public degaDataAccessLayer()
        {

            evaluateSecurityConnectionType();

            if ((ConfigurationManager.ConnectionStrings["dsn"] == null) && (securityConnectionType != SecurityConnectionType.Unconfigured))
            {
                dsn = new SqlDatabase(getConnectionString());
            }
            else
            {
                dsn = new DatabaseProviderFactory().Create("dsn");
            }

        }

        public degaDataAccessLayer(string connectionString)
        {
            dsn = new SqlDatabase(connectionString);
        }

        private string getConnectionString()
        {

            string mConnectionString = string.Empty;

            try
            {

                if (securityConnectionType == SecurityConnectionType.Unconfigured)
                {
                    mConnectionString = ConfigurationManager.ConnectionStrings["dsn"].ConnectionString;
                }

                if (securityConnectionType == SecurityConnectionType.All)
                {
                    mConnectionString = encdec.DecryptStringAES(ConfigurationManager.AppSettings["dsn"], "dega");
                }

                if (securityConnectionType == SecurityConnectionType.Password)
                {
                    mConnectionString = ConfigurationManager.ConnectionStrings["dsn"].ConnectionString;
                    SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(mConnectionString);
                    csb.Password = encdec.DecryptStringAES(csb.Password, "dega");
                    //csb.UserID = encdec.DecryptStringAES(csb.UserID, "dega2");
                    //csb.DataSource = encdec.DecryptStringAES(csb.DataSource, "dega3");
                    //csb.InitialCatalog = encdec.DecryptStringAES(csb.InitialCatalog, "dega4");

                    mConnectionString = csb.ToString();
                }

            }
            catch { }

            return mConnectionString;



        }

        public string getConnectionStringDSN()
        {
            return this.dsn.ConnectionString;
        }

        private void evaluateSecurityConnectionType()
        {
            string R = string.Empty;

            try
            {
                R = ConfigurationManager.AppSettings["SecurityConnectionType"];
            }
            catch
            {
            }

            if (R == string.Empty || R == null)
            {
                securityConnectionType = SecurityConnectionType.Unconfigured;
                return;
            }

            if (R.Trim().ToLower() == "all") securityConnectionType = SecurityConnectionType.All;
            if (R.Trim().ToLower().StartsWith("pass")) securityConnectionType = SecurityConnectionType.Password;



        }




        #region "DAL"



        public int SaveBinary(byte[] binary, string Table, string FieldName, string NameID, int ValueID)
        {

            string strSQL = "UPDATE " + Table + " SET " + FieldName + " = @binaryValue " + " WHERE " + NameID + " = " + ValueID.ToString();

            SqlConnection conexion = new SqlConnection(dsn.ConnectionString);

            conexion.Open();

            using (SqlCommand cmd = new SqlCommand(strSQL, conexion))
            {
                cmd.Parameters.Add("@binaryValue", SqlDbType.VarBinary).Value = binary;
                cmd.ExecuteNonQuery();
            }

            conexion.Close();


            return 0;

        }


        public List<DataRow> GetRowList(string NombreProcedimientoAlmacenado, params object[] Parametros)
        {
            return getListRows(GetTable(NombreProcedimientoAlmacenado, Parametros));
        }

        public List<DataRow> GetRowListTableDirect(string NombreTabla)
        {
            return getListRows(GetTableDirect(NombreTabla));
        }

        public List<DataRow> GetListRowSQL(string strSQL)
        {
            return getListRows(GetDataSetSQL(strSQL).Tables[0]);
        }

        public List<List<string>> GetListTable(string NombreProcedimientoAlmacenado, params object[] Parametros)
        {
            return getListTable(GetTable(NombreProcedimientoAlmacenado, Parametros));
        }
        public List<List<string>> GetListSQL(string strSQL)
        {
            return getListTable(GetDataSetSQL(strSQL).Tables[0]);
        }


        private List<List<string>> getListTable(DataTable dt)
        {

            List<List<string>> lstTable = new List<List<string>>();

            foreach (DataRow row in dt.Rows)
            {
                List<string> lstRow = new List<string>();
                foreach (var item in row.ItemArray)
                {
                    lstRow.Add(item.ToString().Replace("\r\n", string.Empty));
                }
                lstTable.Add(lstRow);
            }

            return lstTable;

        }

        private List<DataRow> getListRows(DataTable dt)
        {
            return dt.AsEnumerable().ToList();
            //IEnumerable<DataRow> sequence = dt.AsEnumerable();
        }


        public DataTable GetTable(string NombrePA, params object[] Parametros)
        {
            //string R;
            DataSet ds = null;
            DataTable dt = null;

            try
            {
                ds = dsn.ExecuteDataSet(NombrePA, Parametros);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
            try
            {
                dt = ds.Tables[0];
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return dt;
        }


        public DataTable GetTableTimeOut(string NombrePA, int TimeOut, params object[] Parametros)
        {
            DataSet ds = null;
            DataTable dt = null;

            using (DbCommand command = dsn.GetStoredProcCommand(NombrePA, Parametros))
            {
                command.CommandTimeout = TimeOut;

                try
                {
                    ds = dsn.ExecuteDataSet(command);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Print(e.Message);
                }
                try
                {
                    dt = ds.Tables[0];
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }

            }

            return dt;
        }

        public DataTable GetTableSQL(string SQL)
        {

            DataSet ds = null;
            DataTable dt = null;

            try
            {
                ds = GetDataSetSQL(SQL);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
            try
            {
                dt = ds.Tables[0];
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return dt;
        }


        public DataTable GetTableDirect(string NombreTabla)
        {

            string SQL = "SELECT * FROM " + NombreTabla;

            DataSet ds = null;
            DataTable dt = null;

            try
            {
                ds = GetDataSetSQL(SQL);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }
            try
            {
                dt = ds.Tables[0];
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return dt;
        }

        public void ExecSP(string nameSP, params object[] Parametros)
        {
            try
            {
                dsn.ExecuteNonQuery(nameSP, Parametros);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }

        }

        public void Execute(string SQL)
        {
            try
            {
                dsn.ExecuteNonQuery(CommandType.Text, SQL);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }


        }

        public void ExecuteNonCaptureError(string SQL)
        {
            dsn.ExecuteNonQuery(CommandType.Text, SQL);
        }



        public DataSet GetDataSet(string NombrePA, params object[] Parametros)
        {

            DataSet ds = null;

            try
            {

                ds = dsn.ExecuteDataSet(NombrePA, Parametros);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }


            return ds;

        }

        public DataSet GetDataSetSQL(string SQL)
        {

            DataSet ds = null;

            try
            {

                ds = dsn.ExecuteDataSet(CommandType.Text, SQL);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Print(e.Message);
            }


            return ds;

        }

        public DataRow GetDataRow(string NameSP, params object[] Parameters)
        {

            // Devuelve datos PrimeraFila

            DataTable dt = null;
            DataRow dr = null;

            try
            {


                dt = GetTable(NameSP, Parameters);
                dr = dt.Rows[0];
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return dr;

        }

        public DataRow GetDataRowSQL(string SQL)
        {

            // Devuelve datos PrimeraFila

            DataTable dt = null;
            DataRow dr = null;

            try
            {


                dt = GetTableSQL(SQL);
                dr = dt.Rows[0];
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return dr;

        }
        public string GetScalarString(string nameSP, params object[] Parametros)
        {
            string R = "";

            try
            {
                R = dsn.ExecuteScalar(nameSP, Parametros).ToString();
                //R = SqlHelper.ExecuteScalar(this.dsn, NombrePA, Parametros).ToString();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return R;
        }

        public int GetScalarInteger(string nameSP, params object[] Parametros)
        {
            int R = 0;

            try
            {
                R = Convert.ToInt32(dsn.ExecuteScalar(nameSP, Parametros));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return R;
        }


        public int GeScalarIntNonCaptureError(string nameSP, params object[] Parametros)
        {
            return Convert.ToInt32(dsn.ExecuteScalar(nameSP, Parametros));
        }

        public bool GetScalarBool(string NameSP, params object[] Parametros)
        {
            bool R = false;

            try
            {
                R = Convert.ToBoolean(dsn.ExecuteScalar(NameSP, Parametros));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return R;
        }

        public double GetScalarDouble(string NombrePA, params object[] Parametros)
        {
            double R = 0;

            try
            {
                R = Convert.ToDouble(dsn.ExecuteScalar(NombrePA, Parametros));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return R;
        }


        public string GetScalarStringBytes(string NombrePA, params object[] Parametros)
        {
            string R = "";

            try
            {

                byte[] Rb = (byte[])dsn.ExecuteScalar(NombrePA, Parametros);
                R = Encoding.Default.GetString(Rb);


            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return R;
        }


        public byte[] GetBytesSP(string NombrePA, params object[] Parametros)
        {
            byte[] Rb = null;

            try
            {

                Rb = (byte[])dsn.ExecuteScalar(NombrePA, Parametros);


            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }

            return Rb;
        }

        public int SaveBinaryStack(byte[] binario, string TablaCola, string NombreCampo)
        {
            string[] textArray1 = new string[] { "INSERT INTO  ", TablaCola, " (", NombreCampo, ") VALUES ( @binaryValue )" };
            string strSQL = string.Concat(textArray1);
            SqlConnection conexion = new SqlConnection(this.dsn.ConnectionString);
            conexion.Open();
            using (SqlCommand cmd = new SqlCommand(strSQL, conexion))
            {
                cmd.Parameters.Add("@binaryValue", SqlDbType.VarBinary).Value = binario;
                cmd.ExecuteNonQuery();
            }
            conexion.Close();
            return 0;
        }






        #endregion




    }
}
