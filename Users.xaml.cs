using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using Npgsql;

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Users.xaml
    /// </summary>

    public partial class Users : Window
    {
        String conn_str = User.Connection_string; //строка подключения

        string oldlvl; // старый уровень пользователя для изменения

        List<Login> logins = new List<Login>(); // список пользователей 

        List<Log> logs = new List<Log>(); // список строк таблицы "Log_table" в БД

        public Users()
        {
            InitializeComponent();

            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            NpgsqlCommand com1 = new NpgsqlCommand("select usename as login, substr(groname, 11, 1) as level_ from pg_group  join pg_user  on usesysid = ANY(grolist)", sqlconn);
            NpgsqlDataReader rdr1 = com1.ExecuteReader();
            while (rdr1.Read())
            {
                logins.Add(new Login(rdr1[0].ToString(), rdr1[1].ToString()));
            }
            datagrid_logins.ItemsSource = logins;
            rdr1.Close();

            NpgsqlCommand com2 = new NpgsqlCommand("SELECT \"user\", action, date_trunc('second', (current_timestamp - date))  FROM main_block.\"Log_table\";", sqlconn);
            NpgsqlDataReader rdr2 = com2.ExecuteReader();
            while (rdr2.Read())
            {
                logs.Add(new Log(rdr2[0].ToString(), rdr2[1].ToString(), rdr2[2].ToString()));
            }
            datagrid_logs.ItemsSource = logs;
            rdr2.Close();

            sqlconn.Close();
        }

        private void Butt_back_Click(object sender, RoutedEventArgs e)// обаботчик кнопки назад
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)// обаботчик закрытия окна
        {
            User.LogOUT($"Update_users");
            //Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Menu").FirstOrDefault().Show();
            Application.Current.Windows[2].Show();
        }

        private void butt_update_Click(object sender, RoutedEventArgs e)//обновить
        {
            datagrid_logs.ItemsSource = null;
            logs.Clear();
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();
            NpgsqlCommand com1 = new NpgsqlCommand("SELECT \"user\", action, date_trunc('second', (current_timestamp - date))  FROM main_block.\"Log_table\";", sqlconn);
            NpgsqlDataReader rdr1 = com1.ExecuteReader();
            while (rdr1.Read())
            {
                logs.Add(new Log(rdr1[0].ToString(), rdr1[1].ToString(), rdr1[2].ToString()));
            }
            rdr1.Close();
            datagrid_logs.ItemsSource = logs;
            sqlconn.Close();
        }

        private void butt_delete_Click(object sender, RoutedEventArgs e)// удалить запись из логов
        {
            if (datagrid_logs.SelectedItems.Count == 1)
            {
                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();
                Log delete_log = datagrid_logs.SelectedItem as Log;
                datagrid_logs.ItemsSource = null;
                datagrid_logs.ItemsSource = logs;
                logs.Remove(delete_log);
                NpgsqlCommand com1 = new NpgsqlCommand($"DELETE FROM main_block.\"Log_table\" where \"user\" = '{delete_log.login}' and \"action\" = '{delete_log.message}' ;", sqlconn);
                com1.ExecuteNonQuery();
                sqlconn.Close();
            }
        }

        private void butt_add_User_Click(object sender, EventArgs e)//Добавить пользователя
        {
            if (txtbox_login.Text != "" && txtbox_password.Text != "" && combox_level.Text != "")
            {
                bool f = true;
                int i = 0;
                while (f && i < logins.Count)
                {
                    if (logins[i].login == txtbox_login.Text) f = false;
                    i++;
                }

                if (f)
                {
                    NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                    sqlconn.Open();
                    NpgsqlCommand com_add = new NpgsqlCommand($"call main_block.create_user('{txtbox_login.Text}','{txtbox_password.Text}','{combox_level.Text}')", sqlconn);
                    com_add.ExecuteNonQuery();

                    datagrid_logins.ItemsSource = null;
                    logins.Add(new Login(txtbox_login.Text, combox_level.Text));
                    datagrid_logins.ItemsSource = logins;
                    sqlconn.Close();
                }
                else
                {
                    messtxt.Text = "Пользователь с данным логином уже существует! ";
                    messbar.IsActive = true;
                }
            }
            else
            {
                messtxt.Text = "Не все поля заполнены!";
                messbar.IsActive = true;
            }
        }

        //обработчик нажатия на кнопку ОК всплывающего окна с сообщением
        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

        // обаботчик события изменения выбора строки в таблице с пользователями
        private void datagrid_logins_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            Login selected_login = datagrid_logins.SelectedItem as Login;
            if (selected_login != null)
            {
                txtbox_login1.Text = selected_login.login;
                combox_level1.Text = selected_login.level;
                oldlvl = selected_login.level;
            }
        }

        private void butt_delete_User_Click(object sender, EventArgs e)//удалить выбранного пользователя
        {
            Login selected_login = datagrid_logins.SelectedItem as Login;
            if (selected_login != null)
            {
                //button1.Enabled = true;
                MessageBoxResult result = MessageBox.Show(
                            $"Вы уверены, что хотите удалить пользователя \"{selected_login.login}\" ?", "Question", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    if (selected_login.login != User.login)
                    {
                        NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                        sqlconn.Open();
                        NpgsqlCommand com_dell =
                                            new NpgsqlCommand($"drop role \"{selected_login.login}\"", sqlconn);
                        com_dell.ExecuteNonQuery();
                        sqlconn.Close();
                        datagrid_logins.ItemsSource = null;
                        logins.Remove(selected_login);
                        datagrid_logins.ItemsSource = logins;
                    }
                    else
                    {
                        messtxt.Text = "Ошибка удаления! Пользователь не может удалить сам себя! ";
                        messbar.IsActive = true;
                    }
                }
            }

        }

        private void butt_save_Click(object sender, EventArgs e)//сохранить изменения
        {
            string newlvl = combox_level1.Text;
            if (txtbox_password1.Text != "" && txtbox_login1.Text != "")
            {
                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();
                NpgsqlCommand com_proverca = new NpgsqlCommand($"select * from main_block.\"Log_table\" where \"action\" = '{txtbox_login1.Text}_online'", sqlconn);
                if (com_proverca.ExecuteScalar() == null || txtbox_login1.Text == User.login)//
                {
                    NpgsqlCommand com_pass = new NpgsqlCommand($"ALTER ROLE \"{txtbox_login1.Text}\" PASSWORD '{txtbox_password1.Text}'; ", sqlconn);
                    com_pass.ExecuteNonQuery();
                    if (txtbox_password1.Text != User.password)
                    {
                        User.password = txtbox_password1.Text;
                        txtbox_password1.Clear();
                        User.Connection_string = "";
                    }
                }
                else
                {
                    messtxt.Text = "Данный пользователь сейчас онлайн. \r\nДля редактирования его профиля дождитесь выхода его из системы. ";
                    messbar.IsActive = true;
                }



                sqlconn.Close();
            }


            NpgsqlConnection sqlconn1 = new NpgsqlConnection(conn_str);
            sqlconn1.Open();
            if (newlvl != oldlvl && newlvl != "")
            {
                NpgsqlCommand com_lvl = new NpgsqlCommand($"REVOKE \"Users_LVL_{oldlvl}\" FROM \"{txtbox_login1.Text}\"; GRANT \"Users_LVL_{newlvl}\" TO \"{txtbox_login1.Text}\"; ", sqlconn1);
                com_lvl.ExecuteNonQuery();
                if (newlvl == "1")
                {
                    NpgsqlCommand com_lvl_ = new NpgsqlCommand($"ALTER ROLE \"{txtbox_login1.Text}\" CREATEROLE; ", sqlconn1);
                    com_lvl_.ExecuteNonQuery();
                }
                else
                {
                    NpgsqlCommand com_lvl_ = new NpgsqlCommand($"ALTER ROLE \"{txtbox_login1.Text}\" NOCREATEROLE; ", sqlconn1);
                    com_lvl_.ExecuteNonQuery();
                }
                oldlvl = newlvl;
            }

            datagrid_logins.ItemsSource = null;
            logins.Clear();
            NpgsqlCommand com1 = new NpgsqlCommand("select usename as login, substr(groname, 11, 1) as level_ from pg_group  join pg_user  on usesysid = ANY(grolist)", sqlconn1);
            NpgsqlDataReader rdr1 = com1.ExecuteReader();
            while (rdr1.Read())
            {
                logins.Add(new Login(rdr1[0].ToString(), rdr1[1].ToString()));
            }

            datagrid_logins.ItemsSource = logins;
            rdr1.Close();
            sqlconn1.Close();
        }


    }

    // классы для заполнения таблицы с пользователями и таблицы "Log_table"
    public class Login
    {
        public string login { get; set; } // логин пользователя
        public string level { get; set; } // уровень доступа

        public Login(string v1, string v2)
        {
            this.login = v1;
            this.level = v2;
        }
    }
    public class Log
    {
        public string login { get; set; } // логин пользователя
        public string message { get; set; }// сообщение
        public string date { get; set; } // время жизни сообщения

        public Log(string v1, string v2, string v3)
        {
            this.login = v1;
            this.message = v2;
            this.date = v3;
        }
    }
}
