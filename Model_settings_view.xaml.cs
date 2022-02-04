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
    /// Логика взаимодействия для Model_settings_view.xaml
    /// </summary>
    public partial class Model_settings_view : Page
    {

        Modeling_add model_wind = (Modeling_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault();


        String conn_str = User.Connection_string;       //строка подключения

        public ObservableCollection<setting_number> setting_Numbers = new ObservableCollection<setting_number>(); // список заголовков строк (номера настроек)
        ObservableCollection<string> cmb_reshatel_parametrs = new ObservableCollection<string>();   // все параметры решателя в комбобоксе
        ObservableCollection<string> cmb_setka_parametrs = new ObservableCollection<string>();      // все параметры сетки в комбобоксе

        parametrs reshatel_pars = new parametrs();  // параметры решателя выводимые в таблицу
        parametrs setka_pars = new parametrs();     // параметры сетки выводимые в таблицу

        public Model_settings_view()
        {
            InitializeComponent();

            // окошко добавления нового параметра
            bool_exp.geom = true;

            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_rcm\" from main_block.\"Mode\" where \"Id_R_C\"={Data.id_R_C} and \"Id_mode\"={Data.current_mode}", sqlconn);
            string Id_rcm = comm_id.ExecuteScalar().ToString();

            List<string> setting_numbers = new List<string>();
            NpgsqlCommand comm_main = new NpgsqlCommand($"select* from main_block.select_settings_values({Id_rcm}); ", sqlconn);
            NpgsqlDataReader reader_main = comm_main.ExecuteReader();
            while (reader_main.Read())
            {
                setting_numbers = reader_main[0].ToString().Split(',').ToList();
                int id_type = Convert.ToInt32(reader_main[1]);
                string par_name = reader_main[2].ToString();
                string[] par_values_number = reader_main[3].ToString().Split(',');
                string[] par_values_string = reader_main[4].ToString().Split(',');
                List<string> drop_list = reader_main[5].ToString().Split(',').ToList();

                parametrs pars = new parametrs();
                switch (id_type)
                {
                    //настройки решателя
                    case 5:
                        pars = reshatel_pars;
                        break;
                    //настройки сетки
                    case 4:
                        pars = setka_pars;
                        break;
                }

                // если значения параметра - не числа, то заполнить выпадающий список возможных строковых значений
                if (reader_main[4].ToString() != "")
                {
                    pars.add_parametr(par_name, par_values_string);
                    pars.column_drop_lists.Add(drop_list);
                }
                else // иначе создать пустой список
                {
                    pars.add_parametr(par_name, par_values_number);
                    pars.column_drop_lists.Add(new List<string>());
                }
            }
            sqlconn.Close();

            for (int i = 0; i < setting_numbers.Count; i++)
            {
                setting_Numbers.Add(new setting_number((i + 1).ToString(), Convert.ToInt32(setting_numbers[i]),"view"));
            }

            datagrid0.ItemsSource = setting_Numbers;


            parametrs.parametrs_table_build(reshatel, reshatel_pars);
            parametrs.parametrs_table_build(setka, setka_pars);
        }

        private void messbut2_Click(object sender, RoutedEventArgs e)
        {
            messbar2.IsActive = false;
        }
    }
}
