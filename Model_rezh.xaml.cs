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

        Modeling_add model_wind = (Modeling_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault();
        
        class Rezh_value
        {
            public string rezh { get; set; }
            public string value { get; set; }
            public string age { get; set; }
            public int reg_number { get; set; }// номер режима для режимных параметров из БД моделирования
        }
        
        class Rezh_list
        {
            public ObservableCollection<Rezh_value> rezh_all = new ObservableCollection<Rezh_value>();
            public ObservableCollection<Rezh_value> rezh_izm = new ObservableCollection<Rezh_value>();
            public ObservableCollection<Rezh_value> Reg_pars = new ObservableCollection<Rezh_value>();
        }

        ObservableCollection<Rezh_list> rezh_list = new ObservableCollection<Rezh_list>();  //список по каналам

        //ObservableCollection<exp_rezh_number> exp_rezh_numbers = new ObservableCollection<exp_rezh_number>();

        // список по каналам списков номеров режимов из базы экспериментов
        ObservableCollection<ObservableCollection<exp_rezh_number>> exp_rezh_numbers_list = new ObservableCollection<ObservableCollection<exp_rezh_number>>();
        //parametrs rezh_pars = new parametrs();
        ObservableCollection<parametrs> rezh_pars_list = new ObservableCollection<parametrs>();

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

            for (int ind_chan = 0; ind_chan < Data.channels.Count; ind_chan++)
            {
                Rezh_list rezh_list_exmp = new Rezh_list();
                

                #region заполнение третьей таблицы (режимы из экспериментов)

                NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {Data.current_realization} and \"Channel\" = {ind_chan + 1}", sqlconn);
                string Id_r_c = comm_id.ExecuteScalar().ToString();

                NpgsqlCommand com_params = new NpgsqlCommand($"select* from main_block.select_exp_rezh_params({Id_r_c})", sqlconn);
                NpgsqlDataReader reader_par = com_params.ExecuteReader();
                
                parametrs rezh_pars = new parametrs();

                // список пар (из БД и в приложении) номеров  режимов для заголовков строк таблицы с выбором режима
                ObservableCollection<exp_rezh_number> exp_rezh_numbers = new ObservableCollection<exp_rezh_number>();


                while (reader_par.Read())
                {
                    int id_mode = (int)reader_par[0]; //номер режима в БД 
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


                #endregion


                for (int i = 0; i < rezh_pars.column_headers.Count; i++)
                {
                    Rezh_value rez = new Rezh_value();
                    rez.rezh = rezh_pars.column_headers[i];
                    rezh_list_exmp.rezh_izm.Add(rez);
                }

                #region запрос из бд дополнительных режимных параметров моделирования

                NpgsqlCommand com_params1 = new NpgsqlCommand($"select* from main_block.select_Reg_pars({Id_r_c})", sqlconn);
                NpgsqlDataReader reader_par1 = com_params1.ExecuteReader();

                ObservableCollection<Rezh_value> Reg_pars = new ObservableCollection<Rezh_value>();


                while (reader_par1.Read())
                {
                    int id_mode = (int)reader_par1[0]; //номер режима в БД 
                    string name_par = reader_par1[1].ToString();
                    string value_par = Convert.ToDouble(reader_par1[2].ToString().Replace('.', ',')).ToString();

                    Rezh_value par = new Rezh_value { rezh = name_par, value = value_par, age = "old", reg_number = id_mode };

                    Reg_pars.Add(par);
                }
                reader_par1.Close();

                rezh_list_exmp.Reg_pars = Reg_pars;

                #endregion


                #region заполнение всех режимных параметров

                NpgsqlCommand com_rezh_params = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры') order by name_param;", sqlconn);
                NpgsqlDataReader reader_par2 = com_rezh_params.ExecuteReader();
                Parametrs.geom_pars = new Dictionary<string, Param>();
                while (reader_par2.Read())
                {
                    Rezh_value str = new Rezh_value();
                    str.rezh = reader_par2[0].ToString();
                    bool flag = true;
                    for (int i = 0; i < rezh_list_exmp.rezh_izm.Count; i++)
                    {
                        if (rezh_list_exmp.rezh_izm[i].rezh == str.rezh)
                        {
                            flag = false;
                        }
                        if (!flag)
                        {
                            i = rezh_list_exmp.rezh_izm.Count;
                        }
                    }
                    if (flag)
                    {
                        rezh_list_exmp.rezh_all.Add(str);
                    }
                        
                    Param p = new Param() { short_name = reader_par2[1].ToString(), unit = reader_par2[2].ToString() };
                    Parametrs.geom_pars.Add(reader_par2[0].ToString(), p);
                }
                reader_par2.Close();

                #endregion


                rezh_list.Add(rezh_list_exmp);
                rezh_pars_list.Add(rezh_pars);
                exp_rezh_numbers_list.Add(exp_rezh_numbers);

            }
            sqlconn.Close();
            radiobut_chan1.IsChecked = true;
        }

        ObservableCollection<Rezh_value> Reg_izm = new ObservableCollection<Rezh_value>();
        ObservableCollection<Rezh_value> Reg_all = new ObservableCollection<Rezh_value>();
        private void RadioButton_Checked(object sender, RoutedEventArgs e)  //переключение по каналам
        {
            rezh_select = false;

            add.IsEnabled = false;
            del.IsEnabled = false;


            var radio = sender as RadioButton;
            string[] names = radio.Content.ToString().Split(' ');
            chan = Convert.ToInt32(names[1]) - 1; //номер канала (начиная с 0 (т.е. №-1))

            Reg_izm.Clear();
            foreach (Rezh_value r in rezh_list[chan].rezh_izm)
            {
                Reg_izm.Add(r);
            }

            Reg_all.Clear();
            foreach (Rezh_value r in rezh_list[chan].rezh_all)
            {
                Reg_all.Add(r);
            }

            datagrid_rezh_all.ItemsSource = Reg_all;
            datagrid_rezh_izm.ItemsSource = Reg_izm;

            exp_rezh_params.Columns.Clear();
            column_rezh_numbers.ItemsSource = exp_rezh_numbers_list[chan];
            parametrs.parametrs_table_build(exp_rezh_params, rezh_pars_list[chan]);

            Data.current_channel = chan + 1;

            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();
            NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {Data.current_realization} and \"Channel\" = {chan + 1}", sqlconn);
            Data. id_R_C = comm_id.ExecuteScalar().ToString();
            sqlconn.Close();

        }

        bool rezh_select = false;
        
        private void RadioButton_Checked_rezh(object sender, RoutedEventArgs e)// выбор режима
        {
            //активация кнопок переноса параметров
            add.IsEnabled = true;
            del.IsEnabled = true;

            rezh_select = true;
            var radio = sender as RadioButton;
            int rezh = Convert.ToInt32(radio.Content.ToString()) - 1;

            Data.current_mode = exp_rezh_numbers_list[Data.current_channel - 1][rezh].db_number;

            for (int i = 0; i < rezh_pars_list[chan].column_headers.Count; i++)
            {
                rezh_list[chan].rezh_izm[i].value = rezh_pars_list[chan].par_value(rezh_list[chan].rezh_izm[i].rezh, rezh).value;
                rezh_list[chan].rezh_izm[i].age = "old";
            }

            Reg_izm.Clear();
            foreach(Rezh_value r in rezh_list[chan].rezh_izm)
            {
                Reg_izm.Add(r);
            }

            Reg_all.Clear();
            foreach (Rezh_value r in rezh_list[chan].rezh_all)
            {
                Reg_all.Add(r);
            }

            foreach (Rezh_value par in rezh_list[chan].Reg_pars)
            {
                if(rezh+1 == par.reg_number)
                {
                    par.reg_number = Data.current_mode;
                    Reg_izm.Add(par);
                    //удаление параметров из левой таблицы (тех, что загрузились из базы)
                    for(int i = 0; i< Reg_all.Count; i++)
                    {
                        if (Reg_all[i].rezh == par.rezh) 
                        {
                            Reg_all.Remove(Reg_all[i]);
                            i--;
                        }
                        
                    }
                }
                
            }

            datagrid_rezh_all.ItemsSource = null;
            datagrid_rezh_all.ItemsSource = Reg_all;


            datagrid_rezh_izm.ItemsSource = null;
            datagrid_rezh_izm.ItemsSource = Reg_izm;

        }

        private void butt_rezh_add_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_rezh_all.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_rezh_all.SelectedItems.Count; i++)
                {
                    Rezh_value rezh = datagrid_rezh_all.SelectedItems[i] as Rezh_value;
                    rezh_list[chan].rezh_all.Remove(rezh);
                    rezh.age = "new";
                    rezh.reg_number = Data.current_mode;
                    rezh_list[chan].Reg_pars.Add(rezh);
                    Reg_izm.Add(rezh);
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
                    bool flag = false;

                    for (int j = 0; j < rezh_pars_list[chan].column_headers.Count; j++)
                    {
                        if (rezh.rezh == rezh_pars_list[chan].column_headers[j])
                        {
                            flag = true;
                        }
                        if (flag)
                        {
                            j = rezh_pars_list[chan].column_headers.Count;
                        }
                    }
                    if (!flag)
                    {
                        rezh_list[chan].Reg_pars.Remove(rezh);
                        Reg_izm.Remove(rezh);
                        if (rezh.age =="old")
                        rezh.age = "del";
                        rezh_list[chan].rezh_all.Add(rezh);
                    }
                    

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) //кнопка "добавить параметр"
        {
            model_wind.Butt_back.IsEnabled = false;
            model_wind.Butt_next.IsEnabled = false;
            model_wind.listview_items.IsEnabled = false;
            dialog_add_param.IsOpen = true;
        }

        private void Butt_OK_Click(object sender, RoutedEventArgs e)
        {
            if (txt_par_name.Text == "" || txt_par_symb.Text == "" || txt_par_unit.Text == "")
            {
                messtxt.Text = "Не все поля заполнены!";
                messbar.IsActive = true;
                txt_par_name.IsEnabled = false;
                txt_par_symb.IsEnabled = false;
                txt_par_unit.IsEnabled = false;
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

                    Parametrs.insert_parametr("Режимные параметры", name, short_name, unit);
                    Rezh_value newparrezh = new Rezh_value();
                    newparrezh.rezh = txt_par_name.Text;
                    for (int i = 0; i < Data.channels.Count; i++)
                    {
                        rezh_list[i].rezh_all.Add(newparrezh);
                    }

                    Reg_all.Clear();
                    foreach (Rezh_value r in rezh_list[chan].rezh_all)
                    {
                        Reg_all.Add(r);
                    }

                    dialog_add_param.IsOpen = false;
                    model_wind.Butt_back.IsEnabled = true;
                    model_wind.Butt_next.IsEnabled = true;
                    model_wind.listview_items.IsEnabled = true;
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
                }
            }
        }
        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
            txt_par_name.IsEnabled = true;
            txt_par_symb.IsEnabled = true;
            txt_par_unit.IsEnabled = true;
        }

        private void messbut2_Click(object sender, RoutedEventArgs e)
        {
            messbar2.IsActive = false;
        }

        private void dialog_add_param_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            model_wind.Butt_back.IsEnabled = true;
            model_wind.Butt_next.IsEnabled = true;
            model_wind.listview_items.IsEnabled = true;
        }

        public bool check_rezh_pars()
        {
            bool check = true;
            if (rezh_select)
            {
                foreach (Rezh_value value in rezh_list[chan].rezh_izm)
                {
                    if (value.value == null)
                    {
                        check = false;
                        messtxt2.Text = "Не все поля заполнены!";
                        messbar2.IsActive = true;
                        break;
                    }
                }
            }
            else
            {
                check = false;
                messtxt2.Text = "Режим не выбран!";
                messbar2.IsActive = true;
            }
            
            return check;
        }

        private void datagrid_rezh_izm_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var el = e.EditingElement as TextBox;
                    int rowIndex = e.Row.GetIndex();
                    int columnIndex = e.Column.DisplayIndex;
                    Rezh_value data = (Rezh_value)e.Row.DataContext;
                    if (el.Text != data.value && data.age == "old")
                    {
                        try
                        {
                            double d = Convert.ToDouble(el.Text);
                            data.age = "update";
                            //changes++;
                            //batt_obr_rez.IsEnabled = false;
                        }
                        catch
                        {
                            messtxt.Text = $"Ошибка ввода!";
                            messbar.IsActive = true;
                            el.Text = data.value;
                        }
                    }
                }
            }
        }

        public void save_in_DB()
        {
            foreach(Rezh_value par in rezh_list[chan].Reg_pars)
            {
                if (Data.current_mode == par.reg_number)
                {
                    switch (par.age)
                    {
                        case "new":
                            DB_proc_func.insert_Reg_pars(par.rezh, null, par.value);
                            break;

                        case "update":
                            DB_proc_func.update_Reg_pars(par.rezh, null, par.value);
                            break;
                    }

                    par.age = "old";
                    
                }
            }


        }
    }
}
