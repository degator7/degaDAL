using dega;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace test
{




    class Program
    {
        static void Main(string[] args)
        {

            string degator = @"

                ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
                ░███░░████░░███░░██░░███░░██░░███░░
                ░█  █░█   ░█   ░█  █░ █ ░█  █░█  █░
                ░█░░█░███░░█░██░████░░█░░█░░█░███ ░
                ░█░░█░█  ░░█░ █░█  █░░█░░█░░█░█  █░
                ░███ ░████░███ ░█░░█░░█░░ ██ ░█░░█░
                ░   ░░    ░   ░░ ░░ ░░ ░░░  ░░ ░░ ░
                ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░

                                            ";
            Console.Write(degator);

            //string mConnectionString = encdec.DecryptStringAES(ConfigurationManager.ConnectionStrings["dsn"].ConnectionString, "dega");

            //var csb = new SqlConnectionStringBuilder(mConnectionString);
            //string NombreBaseDatos = csb.InitialCatalog;
            //degaDataAccessLayer db = new degaDataAccessLayer(mConnectionString);

            degaDataAccessLayer db = new degaDataAccessLayer();

            DateTime fecha = Convert.ToDateTime(db.GetDataRowSQL("select GETDATE() as fecha")["Fecha"]);
            Console.WriteLine(fecha.ToBinary().ToString());


            DataRow dr = db.GetDataRow("getLocal", 1);
            DataTable dt = db.GetTable("getLocales", "bebida giro", -33.4326083333333, -70.6149888888889, 5000);


            List<DataRow> rl = db.GetRowList("getLocales", "bebida giro", -33.4326083333333, -70.6149888888889, 5000);
            List<DataRow> rltd = db.GetRowListTableDirect("local");

            List<List<string>> lt = db.GetListTable("getLocales", "bebida giro", -33.4326083333333, -70.6149888888889, 5000);


            Console.ReadLine();
        }
    }
}
