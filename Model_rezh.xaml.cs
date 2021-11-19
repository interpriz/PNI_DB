using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Model_rezh.xaml
    /// </summary>
    /// 

   

    public partial class Model_rezh : Page
    {
        int chan = 0; //номер канала (начиная с 0 (т.е. №-1))

        String conn_str = User.Connection_string;       //строка подключения

        class Rezh_value
        {
            public string rezh { get; set; }
            public string value { get; set; }
        }

        ObservableCollection<Rezh_value>  rezh_all = new ObservableCollection<Rezh_value>();
        ObservableCollection<Rezh_value>  rezh_izm = new ObservableCollection<Rezh_value>();

        ObservableCollection<exp_rezh_number> exp_rezh_numbers = new ObservableCollection<exp_rezh_number>();
        parametrs rezh_pars = new parametrs();

        public class exp_rezh_number // заголовок строк таблиц
        {
            public string number { get; set; }      //отображаемый номер режима
            public int db_number{ get; set; }    // номер режима в базе данных

            // конструктор заголовка
            public exp_rezh_number(string v1, int v2)
            {
                this.number = v1;
                this.db_number = v2;
            }
        }

        public Model_rezh()
        {
            InitializeComponent();
            for (int i = 1; i < Data.channels.Count; i++)
            {
                RadioButton radio = new RadioButton { Content = $"Канал {i + 1}", BorderBrush = new SolidColorBrush(Color.FromRgb(0, 14, 153)) };
                radio.Checked += new RoutedEventHandler(RadioButton_Checked);
                stackpanel_chan.Children.Add(radio);
            }

            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            NpgsqlCommand com_rezh_params = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par1 = com_rezh_params.ExecuteReader();
            Parametrs.geom_pars = new Dictionary<string, Param>();
            while (reader_par1.Read())
            {
                Rezh_value str = new Rezh_value();
                str.rezh = reader_par1[0].ToString();
                rezh_all.Add(str);

                Param p = new Param() { short_name = reader_par1[1].ToString(), unit = reader_par1[2].ToString() };
                Parametrs.geom_pars.Add(reader_par1[0].ToString(), p);
            }
            reader_par1.Close();
            datagrid_rezh_all.ItemsSource = rezh_all;
            datagrid_rezh_izm.ItemsSource = rezh_izm;
            sqlconn.Close();
            radiobut_chan1.IsChecked = true;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            string[] names = radio.Content.ToString().Split(' ');
            chan = Convert.ToInt32(names[1]) - 1; //номер канала (начиная с 0 (т.е. №-1))


            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {Data.current_realization} and \"Channel\" = {chan+1}", sqlconn);
            string Id_r_c = comm_id.ExecuteScalar().ToString();

            NpgsqlCommand com_params = new NpgsqlCommand($"select* from main_block.select_exp_rezh_params({Id_r_c})", sqlconn);
            NpgsqlDataReader reader_par = com_params.ExecuteReader();
            rezh_pars = new parametrs();
            exp_rezh_numbers = new ObservableCollection<exp_rezh_number>();


            while (reader_par.Read())
            {
                int id_mode = (int)reader_par[0];
                string name_par = reader_par[1].ToString();
                string value_par = Convert.ToDouble(reader_par[2].ToString().Replace('.', ',')).ToString();

                parametr par = new parametr();
                par.value = value_par;

                if (id_mode > exp_rezh_numbers.Count)
                {
                    exp_rezh_numbers.Add(new exp_rezh_number((exp_rezh_numbers.Count + 1).ToString(), id_mode));
                    rezh_pars.add_row();
                }
                if (!rezh_pars.column_headers.Contains(name_par))
                {
                    rezh_pars.add_parametr(name_par);
                }
                rezh_pars.table[exp_rezh_numbers.Count - 1].cols[rezh_pars.column_headers.IndexOf(name_par)] = par;
            }
            reader_par.Close();

            exp_rezh_params.Columns.Clear();
            column_rezh_numbers.ItemsSource = exp_rezh_numbers;
            parametrs_table_build(exp_rezh_params, rezh_pars);

            //перенос элементов из правой таблицы в левую
            if (rezh_izm.Count != 0)
            {
                for (int i = 0; i < rezh_pars.column_headers.Count; i++)
                {
                    foreach (Rezh_value par in rezh_izm)
                    {
                        if (par.rezh != rezh_pars.column_headers[i])
                        {
                            datagrid_rezh_izm.SelectedItem = datagrid_rezh_izm.Items[rezh_izm.IndexOf(par)];
                            butt_rezh_del_Click(null, null);
                            break;
                        }
                    }
                }
            }
            // перенос элементов из левой в правую(тех, что есть в таблицу с режимами)
            for (int i = 0; i < rezh_pars.column_headers.Count; i++)
            {
                foreach (Rezh_value par in datagrid_rezh_all.ItemsSource)
                {
                    if (par.rezh == rezh_pars.column_headers[i])
                    {
                        datagrid_rezh_all.SelectedItem = datagrid_rezh_all.Items[datagrid_rezh_all.Items.IndexOf(par)];
                        butt_rezh_add_Click(null, null);
                        break;
                    }
                }
            }

            sqlconn.Close();
        }

        void parametrs_table_build(DataGrid gr, parametrs par)
        {
            gr.ItemsSource = par.table;
            //gr.CellEditEnding += datagrid2_CellEditEnding;
            //gr.KeyDown += TxtBox_chan_KeyDown;
            foreach (string name in par.column_headers)
            {
                int i = par.column_headers.IndexOf(name); // номер текущего столбца
                var style = new Style();
                style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
                gr.Columns.Add(new DataGridTextColumn
                {
                    Header = $"{name}, {Parametrs.get_param(name).unit}",
                    Binding = new Binding($"cols[{i}].value") { Mode = BindingMode.TwoWay },
                    ElementStyle = style

                });
            }
        }

        private void RadioButton_Checked_rezh(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            int rezh = Convert.ToInt32(radio.Content.ToString())-1;
            for(int i =0; i<rezh_izm.Count; i++)
            {
                rezh_izm[i].value = rezh_pars.par_value(rezh_izm[i].rezh, rezh).value;
            }
            datagrid_rezh_izm.ItemsSource = null;
            datagrid_rezh_izm.ItemsSource = rezh_izm;


        }

        private void butt_rezh_add_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_rezh_all.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_rezh_all.SelectedItems.Count; i++)
                {
                    Rezh_value rezh = datagrid_rezh_all.SelectedItems[i] as Rezh_value;
                    rezh_all.Remove(rezh);
                    rezh_izm.Add(rezh);
                }
            }
        }

        private void butt_rezh_del_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_rezh_izm.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_rezh_izm.SelectedItems.Count; i++)
                {
                    Rezh_value rezh = datagrid_rezh_izm.SelectedItems[i] as Rezh_value;
                    //bool fl = true;
                    //for (int j = 0; j < DB_constr.rezhs_6_db[chan].Count; j++)
                    //{
                    //    if (rezh.rezh == DB_constr.rezhs_6_db[chan][j].rezh)
                    //    {
                    //        fl = false;
                    //        j = DB_constr.rezhs_6_db[chan].Count;
                    //    }
                    //}
                    //if (fl)
                    //{
                        
                    //}
                    rezh_izm.Remove(rezh);
                    rezh_all.Add(rezh);

                }
            }
        }
    }
}
