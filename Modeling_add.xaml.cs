using Npgsql;
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

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Modeling_add.xaml
    /// </summary>
    public partial class Modeling_add : Window
    {
        string task;
        public string condition;
        string conn_str = User.Connection_string;
        Page new_Task_class;
        Page new_Geom_param;
        public Model_settings new_Model_settings;
        Page new_Model_result;
        public Model_rezh new_Model_rezh;
        public Model_obrabotka new_Model_Obrabotka;

        public Modeling_add(string zadacha)
        {
            task = zadacha;
            InitializeComponent();
            new_Task_class = new Task_class(task);
            frame.Navigate(new_Task_class);
            condition = "step1";
            //item4.IsEnabled = true;
            item6.IsEnabled = true;
        }
        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButOpenMenu.Visibility = Visibility.Visible;
            ButCloseMenu.Visibility = Visibility.Collapsed;
            frame.IsEnabled = true;
            Panel.SetZIndex(grid2, 0);
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButOpenMenu.Visibility = Visibility.Collapsed;
            ButCloseMenu.Visibility = Visibility.Visible;
            frame.IsEnabled = false;
            Panel.SetZIndex(grid2, 1);

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Menu_wind").FirstOrDefault().Show();
        }
        private void Butt_next_Click(object sender, RoutedEventArgs e)
        {
            switch (condition)
            {
                case "step1":
                    switch (task)
                    {
                        case "ModelOldTask":
                            //item1.IsSelected = false;
                            item2.IsEnabled = true;
                            item2.IsSelected = true;
                            break;

                        case "ModelNewTask":
                            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                            sqlconn.Open();

                            //проверка на существование класса задачи
                            NpgsqlCommand comm_ex1 = new NpgsqlCommand($"select main_block.exist_experiment_class('{Data.php_name}','{Data.tpe_name}','{Data.ar1_name}')", sqlconn);
                            string fl = comm_ex1.ExecuteScalar().ToString();
                            if (fl != "0")
                            {
                                //вывод сообщения "Класс задачи уже существует в базе! Перейдите в режим \"Существующая задача\""
                                MessageBox.Show("Класс задачи уже существует в базе! Перейдите в режим \"Существующая задача\"", "Ошибка", MessageBoxButton.OK);
                            }
                            else
                            {
                                bool check_including_combobox(string a, List<string> b) //проверка на наличие введенного значения в выпадающих списках
                                {
                                    bool f = false;
                                    for (int i = 0; i < b.Count; i++)
                                    {
                                        if (a == b[i])
                                        {
                                            f = true;
                                        }
                                    }
                                    return f;
                                }
                                bool f1 = check_including_combobox(Data.php_name, Php_tpe_ar1.php_names_list);
                                bool f2 = check_including_combobox(Data.tpe_name, Php_tpe_ar1.tpe_names_list);
                                bool f3 = check_including_combobox(Data.ar1_name, Php_tpe_ar1.ar1_names_list);

                                string message = "";

                                if (f1)
                                {
                                    if (f2)
                                    {
                                        NpgsqlCommand comm_ex = new NpgsqlCommand($"select main_block.exist_php_tpe('{Data.php_name}','{Data.tpe_name}')", sqlconn); //Проверка на наличие пары : (физ пар и тип эн об)
                                        string flag = comm_ex.ExecuteScalar().ToString();
                                        if (flag == "0")
                                        {
                                            message += $"<|+|> В базу будет добавлена комбинация физического процесса и типа энергетического оборудования:\r\n \"{Data.php_name}, {Data.tpe_name}\" \r\n\r\n";
                                        }
                                        else
                                        {
                                            message += $"<|!|> Kомбинация физического процесса и типа энергетического оборудования:\r\n \"{Data.php_name}, {Data.tpe_name}\" уже есть в базе \r\n\r\n";
                                        }
                                    }
                                    else
                                    {
                                        message += $"<|+|> Тип энергетического оборудования \"{Data.tpe_name}\" будет добавлен в базу \r\n\r\n";

                                        message += $"<|+|> В базу будет добавлена комбинация физического процесса и типа энергетического оборудования:\r\n \"{Data.php_name}, {Data.tpe_name}\" \r\n\r\n";
                                    }
                                }
                                else
                                {
                                    message += $"<|+|> Физический процесс \"{Data.php_name}\" будет добавлен в базу \r\n\r\n";
                                    if (f2)
                                    {
                                        //message += "*Tpe в списке \r\n";
                                    }
                                    else
                                    {
                                        message += $"<|+|> Тип энергетического оборудования \"{Data.tpe_name}\" будет добавлен в базу \r\n\r\n";
                                    }

                                    message += $"<|+|> В базу будет добавлена комбинация физического процесса и типа энергетического оборудования:\r\n \"{Data.php_name}, {Data.tpe_name}\" \r\n\r\n";
                                }
                                if (f3)
                                {
                                    //message += "*Ar в списке \r\n";
                                }
                                else
                                {
                                    message += $"<|+|> Экспериментальный объект \"{Data.ar1_name}\" будет добавлен в базу \r\n\r\n";
                                    //добавление области в базу
                                    //NpgsqlCommand com_add1 = new NpgsqlCommand($"call main_block.insert_name_areastree('{ar1_name}');", sqlconn);
                                    //com_add1.ExecuteNonQuery();
                                }
                                message += $"<|+|> В базу данных будет добавлен новый класс задачи:\r\n \"{Data.php_name}, {Data.tpe_name}, {Data.ar1_name}\" \r\n\r\n";

                                MessageBoxResult res = MessageBox.Show(message, "Question", MessageBoxButton.YesNo, MessageBoxImage.Question);

                                if (res == MessageBoxResult.Yes) //вызов процедур из базы по добавлению нового класса задач
                                {
                                    if (f1)
                                    {
                                        if (f2)
                                        {
                                            NpgsqlCommand comm_ex = new NpgsqlCommand($"select main_block.exist_php_tpe('{Data.php_name}','{Data.tpe_name}')", sqlconn);
                                            string flag = comm_ex.ExecuteScalar().ToString();
                                            if (flag == "0")
                                            {
                                                //добавление комбинации физ процесса и типа энерг оборуд в базу
                                                NpgsqlCommand com_add1 = new NpgsqlCommand($"call main_block.insert_PhpTpe('{Data.php_name}','{Data.tpe_name}');", sqlconn);
                                                com_add1.ExecuteNonQuery();
                                            }
                                        }
                                        else
                                        {
                                            //добавление типа энерг оборуд в базу
                                            NpgsqlCommand com_add = new NpgsqlCommand($"call main_block.insert_name_tpe('{Data.tpe_name}');", sqlconn);
                                            com_add.ExecuteNonQuery();

                                            //добавление комбинации физ процесса и типа энерг оборуд в базу
                                            NpgsqlCommand com_add1 = new NpgsqlCommand($"call main_block.insert_PhpTpe('{Data.php_name}','{Data.tpe_name}');", sqlconn);
                                            com_add1.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        //добавление физ процесса в базу
                                        NpgsqlCommand com_add = new NpgsqlCommand($"call main_block.insert_name_php('{Data.php_name}');", sqlconn);
                                        com_add.ExecuteNonQuery();

                                        if (f2)
                                        {
                                            //textBox1.Text += "*Tpe в списке \r\n";
                                        }
                                        else
                                        {
                                            //добавление типа энерг оборуд в базу
                                            NpgsqlCommand com_add1 = new NpgsqlCommand($"call main_block.insert_name_tpe('{Data.tpe_name}');", sqlconn);
                                            com_add1.ExecuteNonQuery();
                                        }

                                        //добавление комбинации физ процесса и типа энерг оборуд в базу
                                        NpgsqlCommand com_add2 = new NpgsqlCommand($"call main_block.insert_PhpTpe('{Data.php_name}','{Data.tpe_name}');", sqlconn);
                                        com_add2.ExecuteNonQuery();
                                    }
                                    if (f3)
                                    {
                                        //textBox1.Text += "*Ar в списке \r\n";
                                    }
                                    else
                                    {
                                        //добавление области в базу
                                        NpgsqlCommand com_add1 = new NpgsqlCommand($"call main_block.insert_name_areastree('{Data.ar1_name}');", sqlconn);
                                        com_add1.ExecuteNonQuery();
                                    }
                                    //добавление класса в базу
                                    NpgsqlCommand com_add9 = new NpgsqlCommand($"call main_block.insert_Experiment('{Data.php_name}','{Data.tpe_name}','{Data.ar1_name}');", sqlconn);
                                    com_add9.ExecuteNonQuery();

                                    //вызов ID*
                                    NpgsqlCommand comm_id = new NpgsqlCommand($"select main_block.select_id('{Data.php_name}','{Data.tpe_name}','{Data.ar1_name}')", sqlconn);
                                    Data.id = comm_id.ExecuteScalar().ToString();

                                    Data.exec = false;
                                    
                                    item1.IsSelected = false;
                                    item2.IsEnabled = true;
                                    bool_model.obj = true;
                                    item2.IsSelected = true;
                                }
                            }
                            break;
                    }

                    break;
                case "step2":
                    //item2.IsSelected = false;
                    item3.IsEnabled = true;
                    item3.IsSelected = true;
                    break;
                case "step3":
                    //item2.IsSelected = false;
                    if (new_Model_rezh.check_rezh_pars())
                    {
                        item4.IsEnabled = true;
                        item4.IsSelected = true;
                    }

                    break;
                case "step4":
                    //item2.IsSelected = false;
                    item5.IsEnabled = true;
                    item5.IsSelected = true;
                    break;
            }
        }

        private void Butt_back_Click(object sender, RoutedEventArgs e)
        {
            switch (condition)
            {
                case "step1":
                    //close = false;
                    //timer1.Stop();
                    this.Close();
                    break;
                case "step2":
                    //item2.IsSelected = false;
                    item1.IsSelected = true;
                    break;
                case "step3":
                   //item3.IsSelected = false;
                    item2.IsSelected = true;
                    break;
                case "step4":
                    //item3.IsSelected = false;
                    item3.IsSelected = true;
                    break;
                case "step5":
                    //item3.IsSelected = false;
                    item4.IsSelected = true;
                    break;

            }

        }

        private void Check_id_obj()
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            //проверка: есть ли привязанные стенды
            NpgsqlCommand comm_try = new NpgsqlCommand($"select \"Id$\" from main_block.\"Stand_ID*\" where \"ID*\"={Data.id}", sqlconn);
            NpgsqlDataReader read_try = comm_try.ExecuteReader();
            while (read_try.Read())
            {
                Data.id_obj = read_try[0].ToString();
            }
            read_try.Close();

            if (Data.id_obj == null)
            {
                NpgsqlCommand comm_ins_id = new NpgsqlCommand($"insert into main_block.\"Stand_ID*\"(\"ID*\",\"Stand_id\") values ({Data.id}, 0); ", sqlconn);  //добавление id
                comm_ins_id.ExecuteNonQuery();

                NpgsqlCommand comm_try2 = new NpgsqlCommand($"select \"Id$\" from main_block.\"Stand_ID*\" where \"ID*\"={Data.id}", sqlconn);
                Data.id_obj = comm_try2.ExecuteScalar().ToString();
            }

            sqlconn.Close();
        }

        private void item1_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Visible;
            condition = "step1";
            frame.Navigate(new_Task_class);
            item2.IsSelected = false;
            item3.IsSelected = false;
            item4.IsSelected = false;
            item5.IsSelected = false;
            item6.IsSelected = false;
        }

        private void item2_Selected(object sender, RoutedEventArgs e)
        {
            if (bool_model.obj)
            {
                
                Check_id_obj();
                new_Geom_param = new Geom_param("Modeling");
                bool_model.obj = false;
            }
            frame.Navigate(new_Geom_param);
            condition = "step2";
            Butt_next.Visibility = Visibility.Hidden;
            
            item1.IsSelected = false;
            item3.IsSelected = false;
            item4.IsSelected = false;
            item5.IsSelected = false;
            item6.IsSelected = false;
        }

        private void item3_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Visible;
            frame.Navigate(new_Model_rezh);
            condition = "step3";
            
            item1.IsSelected = false;
            item2.IsSelected = false;
            item4.IsSelected = false;
            item5.IsSelected = false;
            item6.IsSelected = false;
        }

        private void item4_Selected(object sender, RoutedEventArgs e)
        {
            new_Model_settings = new Model_settings();
            frame.Navigate(new_Model_settings);
            //Butt_next.IsEnabled = false;
            //Butt_back.IsEnabled = false;
            condition = "step4";

            item1.IsSelected = false;
            item2.IsSelected = false;
            item3.IsSelected = false;
            item5.IsSelected = false;
            item6.IsSelected = false;
        }

        private void item5_Selected(object sender, RoutedEventArgs e)
        {
            new_Model_result = new Model_result();
            frame.Navigate(new_Model_result);
            //Butt_next.IsEnabled = false;
            //Butt_back.IsEnabled = false;
            condition = "step5";

            item1.IsSelected = false;
            item2.IsSelected = false;
            item3.IsSelected = false;
            item4.IsSelected = false;
            item6.IsSelected = false;
        }

        private void item6_Selected(object sender, RoutedEventArgs e)
        {
            new_Model_Obrabotka = new Model_obrabotka();
            frame.Navigate(new_Model_Obrabotka);
            Butt_next.IsEnabled = false;
            Butt_back.IsEnabled = false;
            condition = "step6";
        }
    }
    public class bool_model
    {
        public static bool obj;//true - страница обновилась, false - страница не менялась
        public static bool geom;
    }
}
