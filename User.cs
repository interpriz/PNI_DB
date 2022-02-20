using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace БД_НТИ
{
    static class User
    {
        public static string lev { get; set; }
        public static string login { get; set; }
        public static string password { get; set; }
        private static string connection_string;
        public static bool flag;

        public static string Connection_string
        {
            get
            {
                return connection_string;
            }

            set
            {
                connection_string = $"Server= localhost; Port=5432; User Id={login}; Password={password}; Database= \"test\";";
            }
        }

        public static List<string> LogIN(string message)
        {
            String conn_str = User.Connection_string;
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            try
            {

                sqlconn.Open();
                NpgsqlCommand com_logs = new NpgsqlCommand($"select * from main_block.test('{message}')", sqlconn);
                NpgsqlDataReader rdr = com_logs.ExecuteReader();
                List<string> logs = new List<string>();
                while (rdr.Read())
                {
                    logs.Add(rdr[0].ToString());
                }
                rdr.Close();
                if (logs.Count == 0)
                {
                    NpgsqlCommand com = new NpgsqlCommand($"INSERT INTO main_block.\"Log_table\" (\"user\", action) VALUES ('{User.login}','{message}');", sqlconn);
                    com.ExecuteNonQuery();
                }

                return logs;
            }
            catch
            {
                //Messbox.ConnectionError();
                return null;
            }
            finally
            {
                sqlconn.Close();
            }
        }

        public static void LogOUT(string message)
        {
            String conn_str = User.Connection_string;
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);

            try
            {
                sqlconn.Open();
                NpgsqlCommand com = new NpgsqlCommand($"DELETE FROM main_block.\"Log_table\" WHERE action ='{message}';", sqlconn);
                com.ExecuteNonQuery();
                flag = true;
            }
            catch
            {
                flag = false;
            }
            finally
            {
                sqlconn.Close();
            }
        }
    }
}
