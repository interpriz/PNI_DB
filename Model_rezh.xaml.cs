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
                Param p = new Param() { short_name = reader_par1[1].ToString(), unit = reader_par1[2].ToString() };
                Parametrs.geom_pars.Add(reader_par1[0].ToString(), p);
            }
            reader_par1.Close();
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

            ObservableCollection<exp_rezh_number> exp_rezh_numbers = new ObservableCollection<exp_rezh_number>();
            parametrs rezh_pars = new parametrs();
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
            int rezh = Convert.ToInt32(radio.Content.ToString());
        }
    }
}
