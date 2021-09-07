using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Npgsql;

namespace БД_НТИ
{
    class MyTimer
    {
        public static MyTimer loging = new MyTimer();

        private DispatcherTimer timer = null;

        public void TimerOn()
        {
            this.timer.IsEnabled = true;
            this.timer.Start();
        }

        public void TimerOff()
        {
            this.timer.Stop();
        }

        public MyTimer()
        {
            this.timer = new DispatcherTimer();
            this.timer.Interval = new TimeSpan(0, 10, 0);
            this.timer.Tick += new EventHandler(Count);
        }

        private void Count(object sender, EventArgs e)
        {
            String conn_str = User.Connection_string;
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();
            NpgsqlCommand com = new NpgsqlCommand($"UPDATE main_block.\"Log_table\" set \"date\"=current_timestamp WHERE \"user\"='{User.login}' and \"action\" = '{User.login}_online';", sqlconn);
            com.ExecuteNonQuery();
            sqlconn.Close();
        }
    }
}
