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
using System.Windows.Threading;

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Experiment.xaml
    /// </summary>
    public partial class Experiment_add : Window
    {
        bool close = true;
        string task;
        public string condition;
        string conn_str = User.Connection_string;

        Page new_Stand_PiM;
        Page new_Task_class;
        Page new_Geom_par;
        public Page new_Construct;
        Page new_Add_result;
        public Page new_Obrabotka;

        int c1 = 0;//счетчики для создания окон в таймере
        int c2 = 0;
        int c3 = 0;

        //DispatcherTimer timer1 = new DispatcherTimer();
        public Experiment_add(string zadacha)
        {
            task = zadacha;
            InitializeComponent();
            new_Task_class = new Task_class(task);
            frame.Navigate(new_Task_class);
            condition = "step1";
            //switch (task)
            //{
            //    case "ExpOldTask":
            //        timer1.Tick += new EventHandler(timer1_Tick);
            //        timer1.Interval = new TimeSpan(50);     //50*100 нс = 5мкс
            //        //timer1.Start();
            //        //condition = "step1";
            //        break;
            //}
            
            Butt_next.IsEnabled = false;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButOpenMenu.Visibility = Visibility.Visible;
            ButCloseMenu.Visibility = Visibility.Collapsed;
            frame.IsEnabled = true;
            //grid1.Background = new SolidColorBrush(Colors.LightCyan);
            //grid2.Opacity = 0;
            Panel.SetZIndex(grid2, 0);
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButOpenMenu.Visibility = Visibility.Collapsed;
            ButCloseMenu.Visibility = Visibility.Visible;
            frame.IsEnabled = false;
            //grid2.Background = new SolidColorBrush(Color.FromRgb(184, 237, 255));
            //grid2.Opacity = 0.5;
            Panel.SetZIndex(grid2, 1);
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Menu_wind").FirstOrDefault().Show();
            //Application.Current.Windows[2].Show();

        }

        private void Butt_next_Click(object sender, RoutedEventArgs e)
        {
            switch (condition)
            {
                case "step1":
                    switch (task)
                    {
                        case "ExpOldTask":
                            item1.IsSelected = false;
                            item2.IsSelected = true;
                            condition = "step2";
                            break;

                        case "ExpNewTask":
                            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                            sqlconn.Open();

                            //проверка на существование класса задачи
                            NpgsqlCommand comm_ex1 = new NpgsqlCommand($"select main_block.exist_experiment_class('{Data.php_name}','{Data.tpe_name}','{Data.ar1_name}')", sqlconn);
                            string fl = comm_ex1.ExecuteScalar().ToString();
                            if (fl != "0")
                            {
                                //вывод сообщения "Класс задачи уже существует в базе! Перейдите в режим \"Существующая задача\""
                                MessageBox.Show("Класс задачи уже существует в базе! Перейдите в режим \"Существующая задача\"","Ошибка", MessageBoxButton.OK);
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
                                
                                if(res == MessageBoxResult.Yes) //вызов процедур из базы по добавлению нового класса задач
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

                                    bool_exp.obj = true;
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
                    //if (bool_exp.stand)
                    //{
                    //    new_Geom_par = new Geom_param();
                    //    bool_exp.stand = false;
                    //}
                    //frame.Navigate(new_Geom_par);
                    item2.IsSelected = false;
                    item3.IsSelected = true;
                    condition = "step3";
                    Butt_next.Visibility = Visibility.Hidden;
                    break;
                case "step3":
                    item3.IsSelected = false;
                    item4.IsSelected = true;
                    condition = "step4";
                    break;
                case "step4":
                    //new_Add_result = new Exp_result();
                    //frame.Navigate(new_Add_result);
                    bool flag1 = true;
                    int j = 1;
                    //foreach(Construct con in Data.constr)
                    //{
                    //    if (con.obr_params_sech.Count != 0)
                    //    {
                    //        if (con.obr_params_sech[0].phys_obr_7.Count != 0)
                    //        {
                    //            for (int i = 0; i < con.obr_params_sech.Count; i++)
                    //            {
                    //                if(con.obr_params_sech[i].phys_obr_7.Count == 0)
                    //                {
                    //                    MessageBoxResult result = MessageBox.Show(
                    //                               $"Нет параметров обработки в канале№{j} в сечении№{i+1}! Для продолжения добавьте их!", "Caution", MessageBoxButton.OK);
                    //                    flag1 = false;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //    }
                    //    j++;
                    //}
                    if (flag1)
                    {

                        item5.IsEnabled = true;
                        item4.IsSelected = false;
                        item5.IsSelected = true;
                        condition = "step5";
                    }
                    break;
                //case "step5":
                //    //new_Add_result = new Exp_result();
                //    //frame.Navigate(new_Add_result);
                //    item5.IsSelected = false;
                //    item6.IsEnabled = true;
                //    item6.IsSelected = true;
                //    condition = "step6";
                //    break;
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
                    //frame.Navigate(new_Task_class);
                    condition = "step1";
                    item1.IsSelected = true;
                    item2.IsSelected = false;
                    break;
                case "step3":
                    //frame.Navigate(new_Stand_PiM);
                    condition = "step2";
                    item2.IsSelected = true;
                    item3.IsSelected = false;
                    Butt_next.Visibility = Visibility.Visible;
                    break;
                case "step4":
                    Data.current_realization = null;
                    Butt_next.Visibility = Visibility.Hidden;
                    //frame.Navigate(new_Geom_par);
                    c3 = 0;
                    
                    condition = "step3";
                    item3.IsSelected = true;
                    item4.IsSelected = false;
                    break;
                case "step5":
                    //frame.Navigate(new_Construct);
                    condition = "step4";
                    item4.IsSelected = true;
                    item5.IsSelected = false;
                    break;
                case "step6":
                    condition = "step5";
                    item6.IsSelected = false;
                    item6.IsEnabled = false;
                    item5.IsSelected = true;
                    break;
            }
                
        }
        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    switch (condition)
        //    {
        //        case "step1":
        //            if (Data.id != null)
        //            {
        //                if (c1 < 1)
        //                {
        //                    Butt_next.IsEnabled = true;
        //                    item2.IsEnabled = true;
        //                    new_Stand_PiM = new Exp_stand_PiM("add");
        //                    c1++;
        //                }
        //            }
        //            else
        //            {
        //                Butt_next.IsEnabled = false;
        //                item2.IsEnabled = false;
        //                item3.IsEnabled = false;
        //                c1 = 0;
        //            }
        //            break;
        //        case "step2":
        //            if (Data.id_obj != null)
        //            {
        //                if (c2 < 1)
        //                {
        //                    Butt_next.IsEnabled = true;
        //                    item3.IsEnabled = true;
        //                    new_Geom_par = new Geom_param();
        //                    c2++;
        //                }                    
        //            }
        //            else
        //            {
        //                Butt_next.IsEnabled = false;
        //                item3.IsEnabled = false;
        //                c2 = 0;
        //            }
        //            break;
        //        case "step3":
        //            if(Data.current_realization != null)
        //            {
        //                if (c3 < 1)
        //                {
        //                    condition = "step4";
        //                    item4.IsEnabled = true;
        //                    new_Construct = new Exp_construct();
        //                    frame.Content = new_Construct;
        //                    Butt_next.Visibility = Visibility.Visible;
        //                    c3++;
        //                }
        //            }
        //            else
        //            {
        //                Butt_next.Visibility = Visibility.Hidden;
        //                item4.IsEnabled = false;
        //                c3 = 0;
        //            }
        //            break;
        //    }
            
        //}

        private void item1_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Visible;
            frame.Navigate(new_Task_class);
            condition = "step1";
            item4.IsEnabled = false;
            item5.IsEnabled = false;
            item6.IsEnabled = false;
            if (Data.id !=null)
            {
                Butt_next.IsEnabled = true;
            }
        }

        private void item2_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Visible;
            if (Data.id_obj == null)
            {
                Butt_next.IsEnabled = false;
            }
            if (bool_exp.obj)
            {
                new_Stand_PiM = new Exp_stand_PiM("add");
                bool_exp.obj = false;
            }
            frame.Navigate(new_Stand_PiM);
            condition = "step2";
            item4.IsEnabled = false;
            item5.IsEnabled = false;
            item6.IsEnabled = false;
        }

        private void item3_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Hidden;
            if (bool_exp.stand)
            {
                Geom_param.save = true;
                new_Geom_par = new Geom_param("Experiment");
                bool_exp.stand = false;
            }
            frame.Navigate(new_Geom_par);
            condition = "step3";
            item4.IsEnabled = false;
            item5.IsEnabled = false;
            item6.IsEnabled = false;
        }

        private void item4_Selected(object sender, RoutedEventArgs e)
        {
            frame.Navigate(new_Construct);
            condition = "step4";
            Butt_next.Visibility = Visibility.Visible;
            item6.IsEnabled = false;
        }

        private void item5_Selected(object sender, RoutedEventArgs e)
        {
            new_Add_result = new Exp_result();
            frame.Navigate(new_Add_result);
            condition = "step5";
            Butt_next.Visibility = Visibility.Hidden;
            item6.IsEnabled = false;
        }

        private void item6_Selected(object sender, RoutedEventArgs e)
        {
            new_Obrabotka = new Exp_obrabotka(Exp_result.id_chan);
            frame.Navigate(new_Obrabotka);
            condition = "step6";

        }
    }

    public class bool_exp
    {
        public static bool obj;//true - страница обновилась, false - страница не менялась
        public static bool stand;
        public static bool geom;
    }
}
