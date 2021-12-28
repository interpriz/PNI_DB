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
    /// Логика взаимодействия для Experiment_search.xaml
    /// </summary>
    public partial class Experiment_search : Window
    {
        string conn_str = User.Connection_string;
        string step = "step1";
        Page new_Task_class;
        Page new_Geom;
        Page new_Stand_PiM;
        Page new_result_view;
        Page new_obrabotka_view;
        public Model_settings_view new_Model_settings_view;
        Model_result_view new_Model_result_view;
        public Experiment_search()
        {
            InitializeComponent();
            new_Task_class = new Task_class("ExpSearch");
            frame.Navigate(new_Task_class);
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
            //Application.Current.Windows[2].Show();
            Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Menu_wind").FirstOrDefault().Show();
        }

        private void Butt_next_Click(object sender, RoutedEventArgs e)
        {
            switch (step)
            {
                case "step1":
                    if (bool_exp_search_update.bool_obj)
                    {
                        NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                        sqlconn.Open();

                        NpgsqlCommand comm_chan_count = new NpgsqlCommand($"select count(rc.\"Channel\") from main_block.\"Realization_channel\" rc join main_block.\"Stand_ID*\" s " +
                            $"on rc.\"Id$\"=s.\"Id$\" where s.\"ID*\"={Data.id} group by rc.\"Realization\"", sqlconn); //есть ли данные о результатах эксперимента, если есть, то вернуть число каналов
                        string chan_count = "";
                        NpgsqlDataReader rdr_chan_count = comm_chan_count.ExecuteReader();
                        if (rdr_chan_count.HasRows)
                        {
                            rdr_chan_count.Close();
                            chan_count = comm_chan_count.ExecuteScalar().ToString();
                            new_Geom = new Exp_search_geom(chan_count);
                            item1.IsSelected = false;
                            item2.IsEnabled = true;
                            item2.IsSelected = true;
                            bool_exp_search_update.bool_obj = false;
                        }
                        else
                        {
                            MessageBox.Show("Результатов экспериментов с данным объектом нет.", "Данных нет", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        sqlconn.Close();
                    }
                    else
                    {
                        item2.IsSelected = true;
                    }
                    break;

                case "step2":
                    item3.IsEnabled = true;
                    item4.IsEnabled = true;
                    item5.IsEnabled = true;
                    item2.IsSelected = false;
                    item3.IsSelected = true;
                    //Butt_next.Visibility = Visibility.Hidden;
                    break;

                case "step3":
                    item4.IsEnabled = true;
                    item3.IsSelected = false;
                    item4.IsSelected = true;
                    break;

                case "step4":
                    //new_Add_result = new Exp_result();
                    //frame.Navigate(new_Add_result);
                    item5.IsEnabled = true;
                    item4.IsSelected = false;
                    item5.IsSelected = true;
                    break;

                case "step5":
                    break;

                case "step6":
                    item7.IsEnabled = true;
                    item6.IsSelected = false;
                    item7.IsSelected = true;
                    break;

            }
        }

        private void Butt_back_Click(object sender, RoutedEventArgs e)
        {
            switch (step)
            {
                case "step1":
                    this.Close();
                    break;
                case "step2":
                    item1.IsSelected = true;
                    item2.IsEnabled = false;
                    item2.IsSelected = false;
                    break;

                case "step3":
                    item2.IsSelected = true;
                    item3.IsEnabled = false;
                    item3.IsSelected = false;
                    break;

                case "step4":
                    item3.IsSelected = true;
                    //item4.IsEnabled = false;
                    item4.IsSelected = false;
                    break;

                case "step5":
                    item4.IsSelected = true;
                    item5.IsEnabled = false;
                    item5.IsSelected = false;
                    break;

                case "step6":
                    item4.IsSelected = true;
                    item6.IsEnabled = false;
                    item6.IsSelected = false;
                    break;

                case "step7":
                    item6.IsSelected = true;
                    item7.IsEnabled = false;
                    item7.IsSelected = false;
                    break;
            }
        }

        private void item1_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Visible;
            new_Task_class = new Task_class("ExpSearch");
            frame.Navigate(new_Task_class);
            item2.IsEnabled = false;
            item3.IsEnabled = false;
            item4.IsEnabled = false;
            item5.IsEnabled = false;
            step = "step1";
        }

        private void item2_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Visible;
            frame.Navigate(new_Geom);
            item3.IsEnabled = false;
            item4.IsEnabled = false;
            item5.IsEnabled = false;
            step = "step2";
        }

        private void item3_Selected(object sender, RoutedEventArgs e)
        {
            Butt_next.Visibility = Visibility.Visible;
            //if (Data.id_obj == null)
            //{
            //    Butt_next.IsEnabled = false;
            //}
            //if (bool_exp.obj)
            //{
            //    new_Stand_PiM = new Exp_stand_PiM();
            //    bool_exp.obj = false;
            //}
            new_Stand_PiM = new Exp_stand_PiM("search");
            frame.Navigate(new_Stand_PiM);
            //item4.IsEnabled = false;
            //item5.IsEnabled = false;
            step = "step3";
        }

        private void item4_Selected(object sender, RoutedEventArgs e)
        {
            Exp_result_view.chan_count = Exp_search_geom.count;
            new_result_view = new Exp_result_view();
            frame.Navigate(new_result_view);
            Butt_next.Visibility = Visibility.Hidden;
            //item5.IsEnabled = false;
            step = "step4";
        }

        private void item5_Selected(object sender, RoutedEventArgs e)
        {
            new_obrabotka_view = new Exp_obrabotka_view(Exp_result_view.id_chan);
            frame.Navigate(new_obrabotka_view);
            step = "step5";
        }

        private void item6_Selected(object sender, RoutedEventArgs e)
        {
            new_Model_settings_view = new Model_settings_view();
            frame.Navigate(new_Model_settings_view);
            Butt_next.Visibility = Visibility.Visible;
            step = "step6";

        }

        private void item7_Selected(object sender, RoutedEventArgs e)
        {
            new_Model_result_view = new Model_result_view();
            frame.Navigate(new_Model_result_view);
            Butt_next.Visibility = Visibility.Hidden;
            step = "step7";
        }

        private void item8_Selected(object sender, RoutedEventArgs e)
        {
            
        }
    }
    public static class bool_exp_search_update
    {
        public static bool bool_obj { get; set; }
    }
}
