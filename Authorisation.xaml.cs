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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
//using System.Threading;
using System.Timers;
using Npgsql;

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class Authorisation : Window
    {
        int count = 0; //количество попыток ввода
        int sec = 5;
        DispatcherTimer timer1 = new DispatcherTimer();
        public Authorisation()
        {
            InitializeComponent();
            timer1.Tick += new EventHandler(timer1_Tick);
            timer1.Interval = new TimeSpan(0, 0, 1);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
            if ((LogBox.Text != "") && (PassBox.Password != ""))
            {
                User.login = LogBox.Text;
                User.password = PassBox.Password;
                User.Connection_string = "";
                bool f = false;
                NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
                try
                {

                    sqlconn.Open();
                    NpgsqlCommand com = new NpgsqlCommand($"select main_block.user('{LogBox.Text}');", sqlconn);//вывод уровня по логину и паролю

                    User.lev = com.ExecuteScalar().ToString();//уровень пользователя

                    f = true;
                }
                catch
                {
                    count++;

                    if (count <= 2)
                    {
                        messtxt.Text = "Неправильное имя пользователя или пароль!\r\n" +
                            $"Осталось попыток входа: {3 - count}.\r\n" +
                            "Возможно отсутствует интернет соединение. Проверьте и повторите попытку позже.";
                        messbar.IsActive = true;

                    }
                    else
                    {
                        buttenter.IsEnabled = false;
                        LogBox.IsEnabled = false;
                        PassBox.IsEnabled = false;
                        messtxt.Text = $"Неправильных попыток входа: {count}.\r\n" + $"Повторите попытку через {sec} сек.\r\n" +
                            "Возможно отсутствует интернет соединение. Проверьте и повторите попытку позже.";
                        messbar.IsActive = true;
                        timer1.Start();
                    }


                }
                finally
                {
                    sqlconn.Close();
                }

                if (f)
                {
                    count = 0;
                    //List<string> us = User.LogIN($"{User.login}_online");
                    //if (us != null)
                    //{
                    //    if (us.Count == 0)
                    //    {
                    //        MyTimer.loging.TimerOn();
                    //        this.Hide();
                    //        Window newin = new Menu();
                    //        LogBox.Clear();
                    //        PassBox.Clear();
                    //        newin.ShowDialog();
                    //    }
                    //    else
                    //    {
                    //        messtxt.Text = "В данный момент под этим логином уже работает другой пользователь.\r\nПовторите попытку позже.";
                    //        messbar.IsActive = true;
                    //    }
                    //}

                    this.Hide();
                    Window newin = new Menu();
                    LogBox.Clear();
                    PassBox.Clear();
                    newin.ShowDialog();

                }
            }
            else
            {
                messtxt.Text = "Не все поля заполнены!";
                messbar.IsActive = true;

            }
            //this.Hide();
            //Window newin = new Menu();
            //newin.ShowDialog();
        }

        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            sec--;
            if (sec > 0)
            {
                messtxt.Text = $"Неправильных попыток входа: {count}.\r\n" + $"Повторите попытку через {sec} сек.\r\n" +
                              "Возможно отсутствует интернет соединение. Проверьте и повторите попытку позже.";
            }
            else
            {
                timer1.Stop();
                buttenter.IsEnabled = true;
                LogBox.IsEnabled = true;
                PassBox.IsEnabled = true;
                messbar.IsActive = false;
                count = 0;
                sec = 5;
            }
        }

        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

    }
}
