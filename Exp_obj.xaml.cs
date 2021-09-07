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
using Npgsql;

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Exp_obj.xaml
    /// </summary>
    public partial class Exp_obj : Page
    {
        string php_name;
        string tpe_name;
        string ar1_name;
        string conn_str = User.Connection_string;
        string contrpar;
        string ID;
        bool close = true;
        //Experiment exp_wind = (Experiment)Application.Current.Windows.OfType<Window>().ToList()[2];
        Experiment_add exp_wind_add;
        Experiment_search exp_wind_search;
        public Exp_obj(string contr)
        {
            InitializeComponent();
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            contrpar = contr;
            switch (contrpar)
            {
                case "ExpOldTask":
                    exp_wind_add = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();

                    Txtblock_obj_or_pc.Text = "3. Выбор экспериментального объекта";
                    Combox_exp_obj.IsEditable = false;
                    Combox_phys_proc.IsEditable = false;
                    Combox_type_equip.IsEditable = false;

                    NpgsqlCommand comm_php_old = new NpgsqlCommand("select * from main_block.select_name_php()", sqlconn);//заполнение физических процессов
                    NpgsqlDataReader reader_php_old = comm_php_old.ExecuteReader();
                    while (reader_php_old.Read())
                    {
                        //Combox_phys_proc.Items.Add($"{reader_php[0]}");
                        //comboBox1.Items.Add(string.Format("{0}", reader_php[0]));
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_php_old[0]}";
                        //item.Background = Brushes.LightCyan;
                        Combox_phys_proc.Items.Add(item);
                    }
                    reader_php_old.Close();

                    break;

                case "ExpNewTask":
                    exp_wind_add = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();

                    Txtblock_obj_or_pc.Text = "3. Выбор экспериментального объекта";
                    Combox_exp_obj.IsEditable = true;
                    Combox_phys_proc.IsEditable = true;
                    Combox_type_equip.IsEditable = true;
                    
                    NpgsqlCommand comm_php_new = new NpgsqlCommand("select name from main_block.\"Physical_process\";", sqlconn); //заполнение физических процессов
                    NpgsqlDataReader reader_php_new = comm_php_new.ExecuteReader();
                    while (reader_php_new.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_php_new[0]}";
                        Combox_phys_proc.Items.Add(item);
                        Php_tpe_ar1.php_names_list.Add(reader_php_new[0].ToString());
                    }
                    reader_php_new.Close();

                    NpgsqlCommand comm_tpe = new NpgsqlCommand($"select name from main_block.\"Type_of_power_equipment\";", sqlconn); //заполнение типов оборудования
                    NpgsqlDataReader reader_tpe = comm_tpe.ExecuteReader();
                    while (reader_tpe.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_tpe[0]}";
                        Combox_type_equip.Items.Add(item);
                        Php_tpe_ar1.tpe_names_list.Add(reader_tpe[0].ToString());
                    }
                    reader_tpe.Close();

                    NpgsqlCommand comm_ar1 = new NpgsqlCommand($"SELECT name_subarea FROM main_block.\"Areas_tree\" where id_area=1;", sqlconn); //заполнение экспериментального объекта
                    NpgsqlDataReader reader_ar1 = comm_ar1.ExecuteReader();
                    while (reader_ar1.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_ar1[0]}";
                        Combox_exp_obj.Items.Add(item);
                        Php_tpe_ar1.ar1_names_list.Add(reader_ar1[0].ToString());
                    }
                    reader_ar1.Close();

                    break;

                case "ExpSearch":
                    //exp_wind_search = (Experiment_search)Application.Current.Windows[5];

                    exp_wind_search = (Experiment_search)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_search_wind").FirstOrDefault();
                    
                    Txtblock_obj_or_pc.Text = "3. Выбор экспериментального объекта";
                    Combox_exp_obj.IsEditable = false;
                    Combox_phys_proc.IsEditable = false;
                    Combox_type_equip.IsEditable = false;

                    NpgsqlCommand comm_php_serch = new NpgsqlCommand("select * from main_block.select_name_php()", sqlconn);//заполнение физических процессов
                    NpgsqlDataReader reader_php_serch = comm_php_serch.ExecuteReader();
                    while (reader_php_serch.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_php_serch[0]}";
                        Combox_phys_proc.Items.Add(item);
                    }
                    reader_php_serch.Close();
                    break;
            }

            sqlconn.Close();
            
        }
        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

        private void Combox_phys_proc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            switch (contrpar)
            {
                case "ExpOldTask":
                    ComboBoxItem comitem = new ComboBoxItem();
                    comitem = (ComboBoxItem)Combox_phys_proc.SelectedItem;
                    if (comitem != null)
                    {
                        php_name = comitem.Content.ToString();
                    }
                    else
                    {
                        php_name = Combox_phys_proc.Text;
                    }


                    Combox_type_equip.Items.Clear();
                    Combox_exp_obj.Items.Clear();
                    Butt_image.IsEnabled = false;
                    exp_wind_add.Butt_next.IsEnabled = false;
                    exp_wind_add.item2.IsEnabled = false;
                    exp_wind_add.item3.IsEnabled = false;
                    bool_exp.obj = true;
                    Data.id = null;
                    Data.id_obj = null;

                    sqlconn.Open();
                    NpgsqlCommand comm_tpe_add = new NpgsqlCommand($"select * from main_block.select_name_tpe('{php_name}')", sqlconn);
                    NpgsqlDataReader reader_tpe_add = comm_tpe_add.ExecuteReader();
                    //заполнение типов энерг. оборудования
                    while (reader_tpe_add.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_tpe_add[0]}";
                        //item.Background = Brushes.LightCyan;
                        Combox_type_equip.Items.Add(item);
                    }
                    reader_tpe_add.Close();
                    sqlconn.Close();
                    
                    break;
                
                case "ExpNewTask":
                    Data.id = null;
                    ComboBoxItem comitem_2 = new ComboBoxItem();
                    comitem_2 = (ComboBoxItem)Combox_phys_proc.SelectedItem;
                    if (comitem_2 != null)
                    {
                        Data.php_name = comitem_2.Content.ToString();
                    }
                    else
                    {
                        Data.php_name = Combox_phys_proc.Text;
                    }
                    if ((Combox_type_equip.Text != "") && (Combox_exp_obj.Text != ""))
                    {
                        exp_wind_add.Butt_next.IsEnabled = true;
                    }
                    else
                    {
                        exp_wind_add.Butt_next.IsEnabled = false;
                    }
                    break;

                case "ExpSearch":
                    ComboBoxItem comitem2 = new ComboBoxItem();
                    comitem2 = (ComboBoxItem)Combox_phys_proc.SelectedItem;
                    if (comitem2 != null)
                    {
                        php_name = comitem2.Content.ToString();
                    }
                    else
                    {
                        php_name = Combox_phys_proc.Text;
                    }

                    Combox_type_equip.Items.Clear();
                    Combox_exp_obj.Items.Clear();
                    Butt_image.IsEnabled = false;
                    exp_wind_search.Butt_next.IsEnabled = false;
                    exp_wind_search.item2.IsEnabled = false;
                    exp_wind_search.item3.IsEnabled = false;
                    exp_wind_search.item4.IsEnabled = false;
                    exp_wind_search.item5.IsEnabled = false;
                    Data.id = null;
                    Data.id_obj = null;

                    sqlconn.Open();
                    NpgsqlCommand comm_tpe_serch = new NpgsqlCommand($"select * from main_block.select_name_tpe('{php_name}')", sqlconn);
                    NpgsqlDataReader reader_tpe_serch = comm_tpe_serch.ExecuteReader();
                    //заполнение типов энерг. оборудования
                    while (reader_tpe_serch.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_tpe_serch[0]}";
                        Combox_type_equip.Items.Add(item);
                    }
                    reader_tpe_serch.Close();
                    sqlconn.Close();
                    
                    break;
            }
        }

        private void Combox_type_equip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            switch (contrpar)
            {
                case "ExpOldTask":
                    ComboBoxItem comitem = new ComboBoxItem();
                    comitem = (ComboBoxItem)Combox_type_equip.SelectedItem;
                    if (comitem != null)
                    {
                        tpe_name = comitem.Content.ToString();
                    }
                    else
                    {
                        tpe_name = Combox_type_equip.Text;
                    }


                    Combox_exp_obj.Items.Clear();
                    Butt_image.IsEnabled = false;
                    exp_wind_add.Butt_next.IsEnabled = false;
                    exp_wind_add.item2.IsEnabled = false;
                    exp_wind_add.item3.IsEnabled = false;
                    bool_exp.obj = true;
                    Data.id = null;
                    Data.id_obj = null;

                    sqlconn.Open();
                    try
                    {
                        NpgsqlCommand comm_ar1_add = new NpgsqlCommand($"select * from main_block.select_name_subarea('{php_name}','{tpe_name}','Область расчетного случая');", sqlconn);
                        NpgsqlDataReader reader_ar1_add = comm_ar1_add.ExecuteReader();
                        //заполнение областей рассчетного случая
                        while (reader_ar1_add.Read())
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = $"{reader_ar1_add[0]}";
                            // item.Background = Brushes.LightCyan;
                            Combox_exp_obj.Items.Add(item);
                        }
                        reader_ar1_add.Close();
                    }
                    catch
                    {
                        messtxt.Text = "Возникла ошибка при подключении к базе данных.\r\nПроверьте ваше интернет соединение.\r\n" +
                                   "Повторите попытку позже.";
                        messbar.IsActive = true;
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                    break;

                case "ExpNewTask":
                    Data.id = null;
                    ComboBoxItem comitem_2 = new ComboBoxItem();
                    comitem_2 = (ComboBoxItem)Combox_type_equip.SelectedItem;
                    if (comitem_2 != null)
                    {
                        Data.tpe_name = comitem_2.Content.ToString();
                    }
                    else
                    {
                        Data.tpe_name = Combox_type_equip.Text;
                    }
                    if ((Combox_phys_proc.Text != "") && (Combox_exp_obj.Text != ""))
                    {
                        exp_wind_add.Butt_next.IsEnabled = true;
                    }
                    else
                    {
                        exp_wind_add.Butt_next.IsEnabled = false;
                    }
                    break;

                case "ExpSearch":
                    ComboBoxItem comitem2 = new ComboBoxItem();
                    comitem2 = (ComboBoxItem)Combox_type_equip.SelectedItem;
                    if (comitem2 != null)
                    {
                        tpe_name = comitem2.Content.ToString();
                    }
                    else
                    {
                        tpe_name = Combox_type_equip.Text;
                    }


                    Combox_exp_obj.Items.Clear();
                    Butt_image.IsEnabled = false;
                    exp_wind_search.Butt_next.IsEnabled = false;
                    exp_wind_search.item2.IsEnabled = false;
                    exp_wind_search.item3.IsEnabled = false;
                    exp_wind_search.item4.IsEnabled = false;
                    exp_wind_search.item5.IsEnabled = false;
                    Data.id = null;
                    Data.id_obj = null;

                    sqlconn.Open();
                    NpgsqlCommand comm_ar1_search = new NpgsqlCommand($"select * from main_block.select_name_subarea('{php_name}','{tpe_name}','Область расчетного случая');", sqlconn);
                    NpgsqlDataReader reader_ar1_search = comm_ar1_search.ExecuteReader();
                    //заполнение областей рассчетного случая
                    while (reader_ar1_search.Read())
                    {
                        ComboBoxItem item = new ComboBoxItem();
                        item.Content = $"{reader_ar1_search[0]}";
                        Combox_exp_obj.Items.Add(item);
                    }
                    reader_ar1_search.Close();
                    sqlconn.Close();
                    break;

            }
               
        }

        private void Combox_exp_obj_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            switch (contrpar)
            {
                case "ExpOldTask":
                    Data.id = null;
                    Data.id_obj = null;
                    Butt_image.IsEnabled = true;
                    ComboBoxItem comitem = new ComboBoxItem();
                    comitem = (ComboBoxItem)Combox_exp_obj.SelectedItem;
                    if (comitem != null)
                    {
                        ar1_name = comitem.Content.ToString();
                    }
                    else
                    {
                        ar1_name = Combox_exp_obj.Text;
                    }

                    sqlconn.Open();
                    NpgsqlCommand comm_id_add = new NpgsqlCommand($"select main_block.select_id('{php_name}','{tpe_name}','{ar1_name}')", sqlconn);
                    Data.id = comm_id_add.ExecuteScalar().ToString();
                    sqlconn.Close();

                    exp_wind_add.item2.IsEnabled = true;
                    exp_wind_add.Butt_next.IsEnabled = true;
                    //exp_wind_add.item3.IsEnabled = false;
                    bool_exp.obj = true;
                    break;

                case "ExpNewTask":
                    Data.id = null;
                    ComboBoxItem comitem_2 = new ComboBoxItem();
                    comitem_2 = (ComboBoxItem)Combox_exp_obj.SelectedItem;
                    if (comitem_2 != null)
                    {
                        Data.ar1_name = comitem_2.Content.ToString();
                    }
                    else
                    {
                        Data.ar1_name = Combox_exp_obj.Text;
                    }
                    if ((Combox_phys_proc.Text != "") && (Combox_type_equip.Text != ""))
                    {
                        exp_wind_add.Butt_next.IsEnabled = true;
                    }
                    else
                    {
                        exp_wind_add.Butt_next.IsEnabled = false;
                    }
                    break;

                case "ExpSearch":
                    Data.id = null;
                    Data.id_obj = null;
                    Butt_image.IsEnabled = true;
                    ComboBoxItem comitem2 = new ComboBoxItem();
                    comitem2 = (ComboBoxItem)Combox_exp_obj.SelectedItem;
                    if (comitem2 != null)
                    {
                        ar1_name = comitem2.Content.ToString();
                    }
                    else
                    {
                        ar1_name = Combox_exp_obj.Text;
                    }

                    sqlconn.Open();
                    NpgsqlCommand comm_id_serch = new NpgsqlCommand($"select main_block.select_id('{php_name}','{tpe_name}','{ar1_name}')", sqlconn);
                    Data.id = comm_id_serch.ExecuteScalar().ToString();
                    sqlconn.Close();

                    bool_exp_search_update.bool_obj = true;
                    exp_wind_search.Butt_next.IsEnabled = true;
                    break;

            }
                
        }

        private void Butt_image_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Combox_phys_proc_KeyUp(object sender, KeyEventArgs e)
        {
            Data.id = null;
            if ((Combox_phys_proc.Text != "") && (Combox_type_equip.Text != "") && (Combox_exp_obj.Text != ""))
            {
                exp_wind_add.Butt_next.IsEnabled = true;
                Data.php_name = Combox_phys_proc.Text;
                Data.tpe_name = Combox_type_equip.Text;
                Data.ar1_name = Combox_exp_obj.Text;
            }
            else
            {
                exp_wind_add.Butt_next.IsEnabled = false;
            }
        }

        private void Combox_type_equip_KeyUp(object sender, KeyEventArgs e)
        {
            Data.id = null;
            if ((Combox_phys_proc.Text != "") && (Combox_type_equip.Text != "") && (Combox_exp_obj.Text != ""))
            {
                exp_wind_add.Butt_next.IsEnabled = true;
                Data.php_name = Combox_phys_proc.Text;
                Data.tpe_name = Combox_type_equip.Text;
                Data.ar1_name = Combox_exp_obj.Text;
            }
            else
            {
                exp_wind_add.Butt_next.IsEnabled = false;
            }
        }

        private void Combox_exp_obj_KeyUp(object sender, KeyEventArgs e)
        {
            Data.id = null;
            if ((Combox_phys_proc.Text != "") && (Combox_type_equip.Text != "") && (Combox_exp_obj.Text != ""))
            {
                exp_wind_add.Butt_next.IsEnabled = true;
                Data.php_name = Combox_phys_proc.Text;
                Data.tpe_name = Combox_type_equip.Text;
                Data.ar1_name = Combox_exp_obj.Text;
            }
            else
            {
                exp_wind_add.Butt_next.IsEnabled = false;
            }
        }
    }

    public static class Php_tpe_ar1
    {
        public static List<string> php_names_list = new List<string>();
        public static List<string> tpe_names_list = new List<string>();
        public static List<string> ar1_names_list = new List<string>();
    }
}
