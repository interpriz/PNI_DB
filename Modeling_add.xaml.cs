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

        public Modeling_add(string zadacha)
        {
            task = zadacha;
            InitializeComponent();
            new_Task_class = new Task_class(task);
            frame.Navigate(new_Task_class);
            condition = "step1";
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
                            item1.IsSelected = false;
                            item2.IsEnabled = true;
                            item2.IsSelected = true;
                            condition = "step2";
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
                                    item2.IsSelected = true;
                                    condition = "step2";
                                }
                            }
                            break;
                    }

                    break;
                case "step2":
                    item2.IsSelected = false;
                    item3.IsSelected = true;
                    condition = "step3";
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
                
            }

        }

        private void item1_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void item2_Selected(object sender, RoutedEventArgs e)
        {
            
        }
    }
    public class bool_model
    {
        public static bool obj;//true - страница обновилась, false - страница не менялась
        public static bool stand;
        public static bool geom;
    }
}
