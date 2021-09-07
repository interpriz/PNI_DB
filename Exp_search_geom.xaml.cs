using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Логика взаимодействия для Exp_search_geom.xaml
    /// </summary>
    public partial class Exp_search_geom : Page
    {
        Experiment_search exp_wind_search = (Experiment_search) Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_search_wind").FirstOrDefault();
        
        List<double> par0_values = new List<double>();  //список возможных значений для параметра 1
        //List<RadioButton> radio_list = new List<RadioButton>();

        ObservableCollection<Chan_params> chan_params = new ObservableCollection<Chan_params>();

        string conn_str = User.Connection_string;
        public static int count = 0;    // количество каналов
        int chan;
        public Exp_search_geom(string chan_count)
        {
            InitializeComponent();
            exp_wind_search.Butt_next.IsEnabled = false;

            count = Convert.ToInt32(chan_count);
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            
            for (int i = 0; i < count; i++)
            {
                // создание радиобаттонов для каналов начиная со 2ого (если он есть), т.к. 1ый есть всегда
                if (i+2 <= count)
                {
                    RadioButton radio = new RadioButton { Content = $"Канал {i + 2}", BorderBrush = new SolidColorBrush(Color.FromRgb(0, 14, 153))};
                    radio.Checked += new RoutedEventHandler(RadioButton_Checked);
                    radio.IsEnabled = false;
                    //radio_list.Add(radio);
                    //Binding bind = new Binding() { Path = new PropertyPath($"chan_enable")};
                    //radio.SetBinding(RadioButton.IsEnabledProperty, bind);
                    stackpanel_chan.Children.Add(radio);
                }

                Chan_params chan_i = new Chan_params();
                chan_i.geom_pars = new ObservableCollection<Geom_pars_chan>();
                chan_i.chan_enable = false;
                chan_i.realization = new List<List<int>>();

                // выборка геометрических параметров для поиска по номеру канала и номеру классса задачи
                NpgsqlCommand comm_geom = new NpgsqlCommand($"select * from main_block.\"exp_search_geom\"({Data.id},{i+1});", sqlconn);
                NpgsqlDataReader rdr_geom = comm_geom.ExecuteReader();
                while (rdr_geom.Read())
                {
                    Geom_pars_chan par = new Geom_pars_chan();
                    par.par_name = rdr_geom[0].ToString();
                    par.par_name_unit = $"{rdr_geom[0]} ({rdr_geom[1]}), {rdr_geom[2]}";
                    chan_i.geom_pars.Add(par);
                    chan_i.realization.Add(new List<int>());
                }
                rdr_geom.Close();


                chan_params.Add(chan_i);

            }

            //grid.DataContext = chan_params[0];

            chan_params[0].geom_pars[0].enable = true;  //разрешить доступ к первой ячейке ввода
            //chan_params[0].chan_enable = true;  //разрешить доступ к радиобаттону с каналом 1
            radiobut_chan1.IsChecked = true;

            List<double> list_val_par1 = new List<double>();    //список значений параметра 1 (с дублирующими элементами)
            //List<int> realiz_nums = new List<int>();    //список номеров исполнений для первого параметра (т.е. вообще все)

            NpgsqlCommand comm_par1 = new NpgsqlCommand("select rc.\"Realization\", gp.\"value_number\" from main_block.\"Realization_channel\" rc join main_block.\"Geometric_parametrs\" gp "+ 
                 $"on rc.\"Id_R_C\" = gp.\"Id_R_C\" where rc.\"Channel\" = {1} and rc.\"Id$\" = (select \"Id$\" from main_block.\"Stand_ID*\" where \"ID*\" = {Data.id}) and "+
                 $"gp.\"id_param\" = (select id_param from main_block.\"Parametrs\" where name_param = '{chan_params[0].geom_pars[0].par_name}')", sqlconn);
            NpgsqlDataReader rdr_par1 = comm_par1.ExecuteReader();
            while (rdr_par1.Read())
            {
                chan_params[0].realization[0].Add(Convert.ToInt32(rdr_par1[0]));
                list_val_par1.Add(Math.Round(Convert.ToDouble(rdr_par1[1]), 7));
            }
            rdr_par1.Close();

            //chan_params[0].realization.Add(realiz_nums); //список списков номеров исполнений для каждого параметра
            //par1_values = str_values.Split(';').Distinct().ToList();
            par0_values = list_val_par1.Distinct().ToList();    //список значений параметра 1 (без дублирующих элементов)
            

            sqlconn.Close();
        }
        

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            string[] names = radio.Content.ToString().Split(' ');
            chan = Convert.ToInt32(names[1]) - 1;
            datagrid.ItemsSource = chan_params[chan].geom_pars;
            //grid.DataContext = chan_params[chan];
        }

        private void datagrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            exp_wind_search.Butt_next.IsEnabled = false;
            int row = e.Row.GetIndex();// индекс текущего параметра

            // 
            if (row == 0)   //первый параметр в таблице (обращение к предыдущему каналу)
            {
                if (chan != 0) // не первый канал
                {
                    string spisok = "";
                    // считывание всех испонений у последнего параметра предыдущего канала
                    for (int i = 0; i < chan_params[chan-1].realization[chan_params[chan - 1].realization.Count - 1].Count; i++)
                    {
                        spisok += $"{{{chan_params[chan - 1].realization[chan_params[chan - 1].realization.Count - 1][i]}}},"; //заполнение строки с номерами исполнений
                    }
                    spisok = spisok.Remove(spisok.Length - 1); // удаление последней запятой


                    for (int i = 0; i < chan_params[chan].realization.Count; i++)
                    {
                        chan_params[chan].realization[i].Clear();   //очищение всех списков с исполнениями начиная со списка для текущего параметра
                    }

                    List<double> par1_values = new List<double>();

                    // фильтрация (считывание номеров исполнений и возможных вариантов значений текущего  с учетом значения предыдущего параметра)
                    NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                    sqlconn.Open();
                    NpgsqlCommand comm_par1 = new NpgsqlCommand($"select * from main_block.experiment_filter({Data.id}, {chan}, {chan + 1}, '{{{spisok}}}', '{chan_params[chan-1].geom_pars[chan_params[chan - 1].geom_pars.Count-1].par_name}', {chan_params[chan-1].geom_pars[chan_params[chan - 1].geom_pars.Count - 1].par_value.Replace(',', '.')},'{chan_params[chan].geom_pars[row].par_name}')", sqlconn);
                    NpgsqlDataReader rdr_par1 = comm_par1.ExecuteReader();
                    while (rdr_par1.Read())
                    {
                        chan_params[chan].realization[0].Add(Convert.ToInt32(rdr_par1[0]));
                        par1_values.Add(Convert.ToDouble(rdr_par1[1]));
                    }
                    sqlconn.Close();

                    NotEmptyNumericRule.value_list = par1_values.Distinct().ToList();
                }
                else
                {
                    NotEmptyNumericRule.value_list = par0_values;
                }            
            }
            else   //не первые параметры в таблице (обращение к текущему каналу)
            {
                string spisok = "";
                // считывание всех исполнений у предыдущего параметра текущего канала
                for (int i = 0; i < chan_params[chan].realization[row-1].Count; i++)
                {
                    spisok += $"{{{chan_params[chan].realization[row-1][i]}}},";  //заполнение строки с номерами исполнений
                }
                spisok = spisok.Remove(spisok.Length - 1);

                for(int i = row; i < chan_params[chan].realization.Count; i++)
                {
                    chan_params[chan].realization[i].Clear();   //очищение всех списков с исполнениями начиная со списка для текущего параметра
                }

                List<double> par_values = new List<double>();
                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();
                NpgsqlCommand com_par = new NpgsqlCommand($"select * from main_block.experiment_filter({Data.id}, {chan + 1}, {chan + 1}, '{{{spisok}}}', '{chan_params[chan].geom_pars[row-1].par_name}', {chan_params[chan].geom_pars[row - 1].par_value.Replace(',','.')},'{chan_params[chan].geom_pars[row].par_name}')", sqlconn);
                NpgsqlDataReader rdr_par = com_par.ExecuteReader();
                while (rdr_par.Read())
                {
                    chan_params[chan].realization[row].Add(Convert.ToInt32(rdr_par[0]));
                    par_values.Add(Convert.ToDouble(rdr_par[1]));
                }

                sqlconn.Close();

                NotEmptyNumericRule.value_list = par_values.Distinct().ToList();
            }

            if (row != chan_params[chan].geom_pars.Count - 1)   //если не последняя строка в списке
            {
                if (chan != count - 1)  //если не последний канал
                {
                    for(int i =chan+1; i< count; i++)
                    {
                        stackpanel_chan.Children[i].IsEnabled = false; //блокировка следующих каналов
                    }
                }
                
                chan_params[chan].geom_pars[row + 1].enable = true; // разблокировка следущего параметра для ввода
                chan_params[chan].geom_pars[row + 1].par_value = null;
                // блокировка оставшихся параметров (после следующего, который разблокировали)
                for (int i = row + 2; i < chan_params[chan].geom_pars.Count; i++)
                {
                    chan_params[chan].geom_pars[i].enable = false;
                    chan_params[chan].geom_pars[i].par_value = null;
                }
                // блокировка и очистка параметров следующих каналов
                for (int i = chan + 1; i < count; i++)
                {
                    for (int j = 0; j < chan_params[i].geom_pars.Count; j++)
                    {
                        chan_params[i].geom_pars[j].enable = false;
                        chan_params[i].geom_pars[j].par_value = null;
                        chan_params[i].realization[j].Clear();
                    }
                }
            }
            else  //если последний параметр в таблице
            {
                if (chan != count - 1)  //если не последний канал
                {
                    for (int i = chan + 1; i < count; i++)
                    {
                        stackpanel_chan.Children[i].IsEnabled = false; //блокировка следующих каналов
                    }
                    var txt = e.EditingElement as TextBox;
                    double val = Convert.ToDouble(txt.Text);
                    bool fl = false;
                    // проверка введенного значения на наличие его в базе
                    for (int i = 0; i < NotEmptyNumericRule.value_list.Count; i++)
                    {
                        if (val == NotEmptyNumericRule.value_list[i])
                        {
                            fl = true;
                            i = NotEmptyNumericRule.value_list.Count;
                        }
                    }
                    // блокировка и очистка параметров следующих каналов
                    for (int i = chan + 1; i < count; i++)
                    {
                        for (int j = 0; j < chan_params[i].geom_pars.Count; j++)
                        {
                            chan_params[i].geom_pars[j].enable = false;
                            chan_params[i].geom_pars[j].par_value = null;
                            chan_params[i].realization[j].Clear();
                        }
                    }

                    // разблокировка следующего канала и его первого параметра
                    if (fl)
                    {
                        stackpanel_chan.Children[chan + 1].IsEnabled = true;
                        chan_params[chan + 1].geom_pars[0].enable = true;
                    }

                }
                else  //если последний канал
                {
                    var txt = e.EditingElement as TextBox;
                    double val = Convert.ToDouble(txt.Text);
                    bool fl = false;
                    // проверка введенного значения на наличие его в базе
                    for (int i = 0; i < NotEmptyNumericRule.value_list.Count; i++)
                    {
                        if (val == NotEmptyNumericRule.value_list[i])
                        {
                            fl = true;
                            i = NotEmptyNumericRule.value_list.Count;
                        }
                    }
                    // конечная фильтрация (поиск необходимого исполнения)
                    if (fl)
                    {
                        exp_wind_search.Butt_next.IsEnabled = true;
                        string spisok = "";

                        for (int i = 0; i < chan_params[chan].realization[chan_params[chan].realization.Count - 1].Count; i++)
                        {
                            spisok += $"{chan_params[chan].realization[chan_params[chan].realization.Count - 1][i]},";
                        }
                        spisok = spisok.Remove(spisok.Length - 1);

                        NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                        sqlconn.Open();
                        NpgsqlCommand comm_par1 = new NpgsqlCommand("select rc.\"Realization\" from main_block.\"Realization_channel\" rc join main_block.\"Geometric_parametrs\" gp " +
                            $"on rc.\"Id_R_C\" = gp.\"Id_R_C\" where rc.\"Channel\" = {chan+1} and rc.\"Id$\" = (select \"Id$\" from main_block.\"Stand_ID*\" where \"ID*\" = {Data.id}) and " +
                            $"gp.\"id_param\" = (select id_param from main_block.\"Parametrs\" where name_param = '{chan_params[chan].geom_pars[chan_params[chan].geom_pars.Count-1].par_name}') and " +
                            $"gp.\"value_number\" = {val.ToString().Replace(',', '.')} and array[rc.\"Realization\"] <@ array[{spisok}]", sqlconn);
                        Data.current_realization = comm_par1.ExecuteScalar().ToString();

                        sqlconn.Close();

                    }

                }
            }
            
        }

    }
    public class Geom_pars_chan : INotifyPropertyChanged
    {
        private string Par_value;       //  значение параметра
        private string Par_name;        //  название параметра
        private string Par_name_unit;   //  полное название с обозначением и единицами измерения
        private bool Enable;            //  доступность поля для ввода параметра
        public string par_value
        {
            get { return Par_value; }
            set
            {
                Par_value = value;
                OnPropertyChanged("par_value");
            }
        }
        public string par_name
        {
            get { return Par_name; }
            set
            {
                Par_name = value;
                OnPropertyChanged("par_name");
            }
        }
        public string par_name_unit
        {
            get { return Par_name_unit; }
            set
            {
                Par_name_unit = value;
                OnPropertyChanged("par_name_unit");
            }
        }
        public bool enable
        {
            get { return Enable; }
            set
            {
                Enable = value;
                OnPropertyChanged("enable");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
    public class Chan_params
    {
        public bool chan_enable { get; set; }
        public ObservableCollection<Geom_pars_chan> geom_pars { get; set; } //список параметров

        public List<List<int>> realization { get; set; }    //список фильтрующихся списков номеров исполнений для каждого параметра
    }
}
