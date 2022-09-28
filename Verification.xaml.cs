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
    /// Логика взаимодействия для Verification.xaml
    /// </summary>
    public partial class Verification : Window
    {
        string conn_str = User.Connection_string;
        string step = "step1";
        Page new_Task_class;
        Page new_Geom;
        Page new_Verif_model;
        Page new_Verif_search;

        public Verification()
        {
            InitializeComponent();
            new_Task_class = new Task_class("Verification");
            frame.Navigate(new_Task_class);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Menu_wind").FirstOrDefault().Show();
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

        private void Butt_next_Click(object sender, RoutedEventArgs e)
        {
            switch (step)
            {
                case "step1":
                    NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                    sqlconn.Open();

                    NpgsqlCommand comm_chan_count = new NpgsqlCommand($"select count(rc.\"Channel\") from main_block.\"Realization_channel\" rc join main_block.\"Stand_ID*\" s " +
                        $"on rc.\"Id$\"=s.\"Id$\" where s.\"ID*\"={Data.id} group by rc.\"Realization\"", sqlconn); //есть ли данные о результатах эксперимента, если есть, то вернуть число каналов
                    string chan_count = "";
                    NpgsqlDataReader rdr_chan_count = comm_chan_count.ExecuteReader();
                    if (rdr_chan_count.HasRows)
                    {
                        rdr_chan_count.Close();
                        //chan_count = comm_chan_count.ExecuteScalar().ToString();
                        //new_Geom = new Exp_search_geom(chan_count, "Verification");
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
                    break;

                case "step2":
                    item3.IsEnabled = true;
                    item2.IsSelected = false;
                    item3.IsSelected = true;
                    break;

                case "step3":
                    item4.IsEnabled = true;
                    item3.IsSelected = false;
                    item4.IsSelected = true;
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
                    step = "step1";
                    item2.IsSelected = false;
                    item1.IsSelected = true;
                    break;

                case "step3":
                    step = "step2";
                    item2.IsSelected = true;
                    item3.IsSelected = false;

                    Butt_next.Visibility = Visibility.Visible;
                    break;

                //case "step4":
                //    step = "step3";
                //    item3.IsSelected = true;
                //    item4.IsSelected = false;
                //    Butt_next.Visibility = Visibility.Visible;
                //    break;
            }
        }

        private void item1_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void item2_Selected(object sender, RoutedEventArgs e)
        {
            //frame.Navigate(new_Geom);
            //item3.IsEnabled = false;
            //item4.IsEnabled = false;
            //step = "step2";

            Butt_next.IsEnabled = false;
            new_Verif_model = new Verif_model();
            frame.Navigate(new_Verif_model);
            step = "step2";
        }

        private void item3_Selected(object sender, RoutedEventArgs e)
        {
            //Butt_next.IsEnabled = false;
            //new_Verif_model = new Verif_model();
            //frame.Navigate(new_Verif_model);
            //step = "step3";

            Butt_next.Visibility = Visibility.Hidden;
            new_Verif_search = new Verif_search();
            frame.Navigate(new_Verif_search);
            step = "step3";
        }

        private void item4_Selected(object sender, RoutedEventArgs e)
        {
            //Butt_next.Visibility = Visibility.Hidden;
            //new_Verif_search = new Verif_search();
            //frame.Navigate(new_Verif_search);
            //step = "step4";
        }
    }
}
