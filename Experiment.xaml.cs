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
    public partial class Experiment : Window
    {
        bool close = true;
        string task;
        public string condition;


        Page new_Stand_PiM;
        Page new_Exp_obj;
        Page new_Geom_par;
        public Page new_Construct;
        Page new_Add_result;

        int c1 = 0;//счетчики для создания окон в таймере
        int c2 = 0;
        int c3 = 0;

        DispatcherTimer timer1 = new DispatcherTimer();
        public Experiment(string zadacha)
        {
            task = zadacha;
            InitializeComponent();
            new_Exp_obj = new Exp_obj(task);
            frame.Content = new_Exp_obj;
            switch (task)
            {
                case "ExpOldTask":
                    timer1.Tick += new EventHandler(timer1_Tick);
                    timer1.Interval = new TimeSpan(50);     //50*100 нс = 5мкс
                    //timer1.Start();
                    condition = "step1";
                    break;
            }
            
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
            //Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Menu").FirstOrDefault().Show();
            Application.Current.Windows[2].Show();

        }

        private void Butt_next_Click(object sender, RoutedEventArgs e)
        {
            switch (condition)
            {
                case "step1":
                    //if (bool_exp.obj)
                    //{
                    //    new_Stand_PiM = new Exp_stand_PiM();
                    //    bool_exp.obj = false;
                    //}
                    //frame.Navigate(new_Stand_PiM);
                    item1.IsSelected = false;
                    item2.IsSelected = true;
                    condition = "step2";
                    break;
                case "step2":
                    //if (bool_exp.stand)
                    //{
                    //    new_Geom_par = new Exp_geom_param();
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
                    item5.IsEnabled = true;
                    item4.IsSelected = false;
                    item5.IsSelected = true;
                    condition = "step5";
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
                    //frame.Navigate(new_Exp_obj);
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
                    //Butt_next.Visibility = Visibility.Hidden;
                    //frame.Navigate(new_Geom_par);
                    c3 = 0;
                    
                    condition = "step3";
                    item3.IsSelected = true;
                    item4.IsSelected = false;
                    break;
                case "step5":
                    frame.Navigate(new_Construct);
                    condition = "step4";
                    item4.IsSelected = true;
                    item5.IsSelected = false;
                    break;
            }
                
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (condition)
            {
                case "step1":
                    if (Data.id != null)
                    {
                        if (c1 < 1)
                        {
                            Butt_next.IsEnabled = true;
                            item2.IsEnabled = true;
                            new_Stand_PiM = new Exp_stand_PiM();
                            c1++;
                        }
                    }
                    else
                    {
                        Butt_next.IsEnabled = false;
                        item2.IsEnabled = false;
                        item3.IsEnabled = false;
                        c1 = 0;
                    }
                    break;
                case "step2":
                    if (Data.id_obj != null)
                    {
                        if (c2 < 1)
                        {
                            Butt_next.IsEnabled = true;
                            item3.IsEnabled = true;
                            new_Geom_par = new Exp_geom_param();
                            c2++;
                        }                    
                    }
                    else
                    {
                        Butt_next.IsEnabled = false;
                        item3.IsEnabled = false;
                        c2 = 0;
                    }
                    break;
                case "step3":
                    if(Data.current_realization != null)
                    {
                        if (c3 < 1)
                        {
                            condition = "step4";
                            item4.IsEnabled = true;
                            new_Construct = new Exp_construct();
                            frame.Content = new_Construct;
                            Butt_next.Visibility = Visibility.Visible;
                            c3++;
                        }
                    }
                    else
                    {
                        Butt_next.Visibility = Visibility.Hidden;
                        item4.IsEnabled = false;
                        c3 = 0;
                    }
                    break;
            }
            
        }

        private void item1_Selected(object sender, RoutedEventArgs e)
        {
            frame.Navigate(new_Exp_obj);
            condition = "step1";
        }

        private void item2_Selected(object sender, RoutedEventArgs e)
        {
            if (bool_exp.obj)
            {
                new_Stand_PiM = new Exp_stand_PiM();
                bool_exp.obj = false;
            }
            frame.Navigate(new_Stand_PiM);
            condition = "step2";
        }

        private void item3_Selected(object sender, RoutedEventArgs e)
        {
            if (bool_exp.stand)
            {
                new_Geom_par = new Exp_geom_param();
                bool_exp.stand = false;
            }
            frame.Navigate(new_Geom_par);
            condition = "step3";
        }

        private void item4_Selected(object sender, RoutedEventArgs e)
        {
            frame.Navigate(new_Construct);
            condition = "step4";
        }

        private void item5_Selected(object sender, RoutedEventArgs e)
        {
            new_Add_result = new Exp_result();
            frame.Navigate(new_Add_result);
            condition = "step5";
        }
    }

    public class bool_exp
    {
        public static bool obj;//true - страница обновилась, false - страница не менялась
        public static bool stand;
        public static bool geom;
    }
}
