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
    /// Логика взаимодействия для Model_settings.xaml
    /// </summary>
    /// 

    public class setting_number // заголовок строк таблиц
    {
        public string number { get; set; }      //отображаемый номер режима
        public int db_number { get; set; }    // номер режима в базе данных

        // конструктор заголовка
        public setting_number(string number, int db_number)
        {
            this.number = number;
            this.db_number = db_number;
        }
    }

    public partial class Model_settings : Page
    {
        Modeling_add model_wind = (Modeling_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault();

        public static MaterialDesignThemes.Wpf.DialogHost dialog = new MaterialDesignThemes.Wpf.DialogHost();

        String conn_str = User.Connection_string;       //строка подключения

        public ObservableCollection<setting_number> setting_Numbers = new ObservableCollection<setting_number>(); // список заголовков строк (номера настроек)
        ObservableCollection<string> cmb_reshatel_parametrs = new ObservableCollection<string>();   // все параметры решателя в комбобоксе
        ObservableCollection<string> cmb_setka_parametrs = new ObservableCollection<string>();      // все параметры сетки в комбобоксе

        parametrs reshatel_pars = new parametrs();  // параметры решателя выводимые в таблицу
        parametrs setka_pars = new parametrs();     // параметры сетки выводимые в таблицу

        public Model_settings() 
        {
            InitializeComponent();

            // окошко добавления нового параметра
            bool_exp.geom = true;
            dialog = Dialog_add_param;

            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            //заполнение комбобоксов параметров для выбора при добавлении нового столбца
            cmb_reshatel_parametrs.Add("Новый параметр");
            NpgsqlCommand com_params = new NpgsqlCommand($"select * from main_block.select_parametrs('Настройки решателя') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par = com_params.ExecuteReader();
            Parametrs.reshatel_pars = new Dictionary<string, Param>();
            while (reader_par.Read())
            {
                cmb_reshatel_parametrs.Add(reader_par[0].ToString());
                Param p = new Param() { short_name = reader_par[1].ToString(), unit = reader_par[2].ToString() };
                Parametrs.reshatel_pars.Add(reader_par[0].ToString(), p);
            }
            reader_par.Close();
            cmb_reshatel_params.ItemsSource = cmb_reshatel_parametrs;

            cmb_setka_parametrs.Add("Новый параметр");
            NpgsqlCommand com_params1 = new NpgsqlCommand($"select * from main_block.select_parametrs('Параметры сетки') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par1 = com_params1.ExecuteReader();
            Parametrs.setka_pars = new Dictionary<string, Param>();
            while (reader_par1.Read())
            {
                cmb_setka_parametrs.Add(reader_par1[0].ToString());
                Param p = new Param() { short_name = reader_par1[1].ToString(), unit = reader_par1[2].ToString() };
                Parametrs.setka_pars.Add(reader_par1[0].ToString(), p);
            }
            reader_par1.Close();
            cmb_setka_params.ItemsSource = cmb_setka_parametrs;


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

            for(int i = 0; i< setting_numbers.Count; i++)
            {
                setting_Numbers.Add(new setting_number((i+1).ToString(),Convert.ToInt32(setting_numbers[i])));
            }

            datagrid0.ItemsSource = setting_Numbers;


            parametrs.parametrs_table_build(reshatel, reshatel_pars);
            parametrs.parametrs_table_build(setka, setka_pars);
        }




        #region Add_column

        // параметры для Dialog_add_param

        public string type_of_param { get; set; } // тип параметра ("Настройки решателя" или "Параметры сетки") 
        public string name_param { get; set; } // название параметра
        public bool new_par { get; set; } // добавлять новый параметр в базу?
        public List<string> values_list { get; set; } // список возможных значений строк для параметра

        //---------------------------------

        private void tog_reshatel_Checked(object sender, RoutedEventArgs e) // выбор удаления столбца
        {
            System.Windows.Controls.Primitives.ToggleButton tog = sender as System.Windows.Controls.Primitives.ToggleButton;
            if(tog == tog_reshatel)
                cmb_reshatel_params.ItemsSource = reshatel_pars.column_headers;
            else
                cmb_setka_params.ItemsSource = setka_pars.column_headers;
        }

        private void tog_reshatel_Unchecked(object sender, RoutedEventArgs e)// выбор добавления столбца
        {
            System.Windows.Controls.Primitives.ToggleButton tog = sender as System.Windows.Controls.Primitives.ToggleButton;
            if (tog == tog_reshatel)
                cmb_reshatel_params.ItemsSource = cmb_reshatel_parametrs;
            else
                cmb_setka_params.ItemsSource = cmb_setka_parametrs;
            tog.Background = new SolidColorBrush(Colors.White);
        }


        // выбор параметра для добавления столбца
        private void cmb_reshatel_params_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = sender as ComboBox;
            parametrs pars = new parametrs();
            DataGrid gr = new DataGrid();

            if (cmb == cmb_reshatel_params)
                type_of_param = "Настройки решателя";
            else
                type_of_param = "Параметры сетки";

            switch (type_of_param)
            {
                case "Настройки решателя":
                    pars = reshatel_pars;
                    gr = reshatel;
                    break;

                case "Параметры сетки":
                    pars = setka_pars;
                    gr = setka;
                    break;
            }

            if (cmb.SelectedItem != null)
            {
                string select = cmb.SelectedItem.ToString();
                cmb.SelectedItem = null;
                if (tog_reshatel.IsChecked == false)//добавить столбец
                {
                    values_list = new List<string>();
                    if (select == "Новый параметр")
                    {
                        new_par = true;
                        info_parametr.IsEnabled = true;
                        name_param = "";
                        listbox_values.ItemsSource = values_list;
                        dialog.IsOpen = true;
                        Modeling_add model_wind = (Modeling_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault();
                        model_wind.Butt_back.IsEnabled = false;
                        model_wind.listview_items.IsEnabled = false;

                    }
                    else
                    {
                        // если параметра еще нет в базе
                        if (pars.column_headers.IndexOf(select) == -1)
                        {
                            new_par = false;
                            info_parametr.IsEnabled = false;
                            name_param = select;
                            Param p = Parametrs.get_param(name_param);
                            txt_par_name.Text = name_param;
                            txt_par_symb.Text = p.short_name;
                            txt_par_unit.Text = p.unit;

                            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                            sqlconn.Open();

                            NpgsqlCommand com_params = new NpgsqlCommand($"select * from main_block.select_string_values('{name_param}');", sqlconn);
                            NpgsqlDataReader reader_par = com_params.ExecuteReader();
                            while (reader_par.Read())
                            {
                                values_list.Add(reader_par[0].ToString());
                            }
                            sqlconn.Close();

                            listbox_values.ItemsSource = values_list;

                            dialog.IsOpen = true;
                        }
                    }
                }
                else//удалить столбец
                {
                    int i = pars.column_headers.IndexOf(select);
                    pars.column_headers.RemoveAt(i);
                    pars.column_drop_lists.RemoveAt(i);
                    //cmb.ItemsSource = null;
                    //cmb.ItemsSource = headers;
                    foreach (row r in pars.table)
                    {
                        r.cols.RemoveAt(i);
                    }
                    gr.Columns.RemoveAt(i);
                }
            }
        }

        //!! добавить сохранение данных в базу
        private void Butt_add_value_Click(object sender, RoutedEventArgs e) //добавление новых значений в выподающий список добавляемого столбца 
        {
            if (values_list.IndexOf(new_value.Text) == -1)
            {
                values_list.Add(new_value.Text);
                listbox_values.ItemsSource = null;
                listbox_values.ItemsSource = values_list;
            }
        }

        // занесение нового параметра в таблицу параметров в базе и добавление нового столбца
        private void Butt_OK_Click(object sender, RoutedEventArgs e)
        {
            bool flag = true;//показывает, что новый параметр успешно добавлен в базу
            if (new_par)
            {
                if (txt_par_name.Text == "" || txt_par_symb.Text == "" || txt_par_unit.Text == "")
                {
                    messtxt.Text = "Не все поля заполнены!";
                    messbar.IsActive = true;
                    txt_par_name.IsEnabled = false;
                    txt_par_symb.IsEnabled = false;
                    txt_par_unit.IsEnabled = false;

                    flag = false;
                }
                else
                {
                    string name = txt_par_name.Text;
                    string short_name = txt_par_symb.Text;
                    string unit = txt_par_unit.Text;
                    Parametrs.update_parametrs();
                    string check = Parametrs.check_new_param(name, short_name);
                    if (check == "")
                    {
                        Parametrs.insert_parametr($"{type_of_param}", name, short_name, unit);
                        switch (type_of_param)
                        {
                            case "Настройки решателя":
                                cmb_reshatel_parametrs.Add(txt_par_name.Text);
                                break;

                            case "Параметры сетки":
                                cmb_setka_parametrs.Add(txt_par_name.Text);
                                break;
                        }
                    }
                    else
                    {
                        if (check == name)
                        {
                            messtxt.Text = $"Параметр \"{name}\" уже есть в базе!";
                            messbar.IsActive = true;
                        }
                        else
                        {
                            messtxt.Text = $"Обозначение \"{short_name}\" уже есть в базе!";
                            messbar.IsActive = true;
                        }
                        txt_par_name.IsEnabled = false;
                        txt_par_symb.IsEnabled = false;
                        txt_par_unit.IsEnabled = false;

                        flag = false;
                    }

                }
            }

            if (flag)//если параметр новый и он добавился в базу без проблем или параметр старый
            {
                parametrs pars = new parametrs();
                parametrs pars1 = new parametrs();
                DataGrid gr = new DataGrid();
                switch (type_of_param)
                {
                    case "Настройки решателя":
                        pars = reshatel_pars;
                        pars1 = setka_pars;
                        gr = reshatel;
                        break;

                    case "Параметры сетки":
                        pars = setka_pars;
                        pars1 = reshatel_pars;
                        gr = setka;
                        break;
                }

                //если строк в таблице нет, то добавить столько же сколько и в соседней
                if (pars.table.Count == 0)
                {
                    for(int i =0; i< pars1.table.Count; i++)
                    {
                        pars.add_row();
                    }
                }
                //-------------------------------------------------

                if (radio_value_number.IsChecked == true)
                {
                    pars.add_parametr(name_param);
                }
                else
                {
                    pars.add_parametr(name_param);
                    pars.column_drop_lists[pars.column_headers.Count - 1] = values_list;
                }
                gr.Columns.Clear();
                gr.ItemsSource = null;
                parametrs.parametrs_table_build(gr, pars);

                txt_par_name.Text ="";
                txt_par_symb.Text = "";
                txt_par_unit.Text = "";

                dialog.IsOpen = false;
                //model_wind.Butt_back.IsEnabled = true;
                //model_wind.listview_items.IsEnabled = true;
            }
        }

        private void Dialog_add_param_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            model_wind.Butt_back.IsEnabled = true;
            model_wind.listview_items.IsEnabled = true;
            //switch (page_mode)
            //{
            //    case "Experiment":
            //        exp_wind.Butt_back.IsEnabled = true;
            //        exp_wind.listview_items.IsEnabled = true;
            //        break;

            //    case "Modeling":
            //        model_wind.Butt_back.IsEnabled = true;
            //        model_wind.listview_items.IsEnabled = true;
            //        break;
            //}

        }

        #endregion      

        //Добавление строки
        private void Btn_AddRow_Click(object sender, RoutedEventArgs e)
        {
            reshatel_pars.add_row();
            setka_pars.add_row();
            if (setting_Numbers.Count != 0)
            {
                setting_Numbers.Add(new setting_number((setting_Numbers.Count + 1).ToString(), setting_Numbers[setting_Numbers.Count - 1].db_number + 1));
            }
            else
            {
                setting_Numbers.Add(new setting_number("1",1));
            }
            
        }



        private void batt_save_Click(object sender, RoutedEventArgs e)//сохранить изменения
        {

        }

        private void messbut2_Click(object sender, RoutedEventArgs e)
        {
            messbar2.IsActive = false;
        }
        
        private void TxtBox_chan_KeyDown(object sender, KeyEventArgs e)
        {
            if (
               (e.Key.ToString() == "D0")
               || (e.Key.ToString() == "D1")
               || (e.Key.ToString() == "D2")
               || (e.Key.ToString() == "D3")
               || (e.Key.ToString() == "D4")
               || (e.Key.ToString() == "D5")
               || (e.Key.ToString() == "D6")
               || (e.Key.ToString() == "D7")
               || (e.Key.ToString() == "D8")
               || (e.Key.ToString() == "D9")
               )
            {
                e.Handled = false;
                //return;
            }
            else
                e.Handled = true;
        }

        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
            txt_par_name.IsEnabled = true;
            txt_par_symb.IsEnabled = true;
            txt_par_unit.IsEnabled = true;
        }

        
    }
}
