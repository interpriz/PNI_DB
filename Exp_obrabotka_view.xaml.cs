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
    /// Логика взаимодействия для Exp_obrabotka_view.xaml
    /// </summary>
    public partial class Exp_obrabotka_view : Page
    {

        Experiment_search exp_wind_search = (Experiment_search)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_search_wind").FirstOrDefault();
        int id_chan;

        String conn_str = User.Connection_string;       //строка подключения
        public Exp_obrabotka_view(int chan)
        {
            InitializeComponent();
            page_name.Text += chan + 1;
            id_chan = chan;// индекс канала в списке данных обработки каналов
            Data.chans_obr = new List<Obrabotka_of_fiz_exp>();

            //=-=====================

            Data.channels = new List<channel>();
            string id_obj = Data.id_obj;


            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();
            //заполнение комбобокса параметров для выбора при добавлении нового столбца
            NpgsqlCommand com_params = new NpgsqlCommand($"select * from main_block.select_parametrs('Геометрические параметры') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par = com_params.ExecuteReader();
            Parametrs.geom_pars = new Dictionary<string, Param>();
            while (reader_par.Read())
            {
                Param p = new Param() { short_name = reader_par[1].ToString(), unit = reader_par[2].ToString() };
                Parametrs.geom_pars.Add(reader_par[0].ToString(), p);
            }
            reader_par.Close();

            NpgsqlCommand com_params1 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par1 = com_params1.ExecuteReader();
            Parametrs.reg_pars = new Dictionary<string, Param>();
            while (reader_par1.Read())
            {
                Param p = new Param() { short_name = reader_par1[1].ToString(), unit = reader_par1[2].ToString() };
                Parametrs.reg_pars.Add(reader_par1[0].ToString(), p);
            }
            reader_par1.Close();

            NpgsqlCommand com_params2 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par2 = com_params2.ExecuteReader();
            Parametrs.phys_pars = new Dictionary<string, Param>();
            while (reader_par2.Read())
            {
                Param p = new Param() { short_name = reader_par2[1].ToString(), unit = reader_par2[2].ToString() };
                Parametrs.phys_pars.Add(reader_par2[0].ToString(), p);
            }
            reader_par2.Close();

            NpgsqlCommand com_params3 = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par3 = com_params3.ExecuteReader();
            Parametrs.integral_pars = new Dictionary<string, Param>();
            while (reader_par3.Read())
            {
                Param p = new Param() { short_name = reader_par3[1].ToString(), unit = reader_par3[2].ToString() };
                Parametrs.integral_pars.Add(reader_par3[0].ToString(), p);
            }
            reader_par3.Close();


            //заполнение и создание таблиц каналов из базы данных
            NpgsqlCommand comm_main = new NpgsqlCommand($"SELECT \"Channel\", \"Realization\", \"name_param\", \"value_number\"  from main_block.\"Realization_channel\" r join main_block.\"Geometric_parametrs\" g on  r.\"Id_R_C\" = g.\"Id_R_C\" join main_block.\"Parametrs\" p on p.\"id_param\" = g.\"id_param\" where \"Id$\" = {id_obj} order by \"Channel\", \"Realization\", g.\"id_param\"; ", sqlconn);
            NpgsqlDataReader reader_main = comm_main.ExecuteReader();
            while (reader_main.Read())
            {
                int chn = (int)reader_main[0];
                int isp = (int)reader_main[1];
                string par_name = reader_main[2].ToString();
                string value = Convert.ToDouble(reader_main[3]).ToString();//убираем лишние нули

                if (Data.channels.Count == 0)
                {
                    ObservableCollection<str> table = new ObservableCollection<str>();
                    List<List<string>> lst0 = new List<List<string>>();
                    List<string> lst1 = new List<string>();
                    lst1.Add(value);
                    lst1.Add("");
                    lst0.Add(lst1);
                    table.Add(new str(lst0, isp));
                    //table.CollectionChanged += Table_CollectionChanged;

                    ObservableCollection<string> column_headers = new ObservableCollection<string>();
                    column_headers.Add(par_name);
                    channel ch = new channel(table, column_headers);
                    Data.channels.Add(ch);

                }
                else
                {
                    if (chn == Data.channels.Count)
                    {
                        //if (isp == Data.channels[chn - 1].table.Count)
                        if (isp == Data.channels[chn - 1].table[Data.channels[chn - 1].table.Count - 1].realization)//текущий канал - таблица значений - последняя строка в таблице - номер ее реализации из базы
                        {
                            if (Data.channels[chn - 1].column_headers.IndexOf(par_name) == -1)
                            {
                                Data.channels[chn - 1].column_headers.Add(par_name);
                            }
                            //Data.channels[chn - 1].table[isp - 1].cols.Add(value);
                            Data.channels[chn - 1].table[Data.channels[chn - 1].table.Count - 1].cols.Add(new List<string> { value, "" });//текущий канал - таблица значений - последняя строка в таблице - массив значений строки - добавить новое
                        }
                        else
                        {
                            List<List<string>> lst0 = new List<List<string>>();
                            List<string> lst1 = new List<string>();
                            lst1.Add(value);
                            lst1.Add("");
                            lst0.Add(lst1);
                            Data.channels[chn - 1].table.Add(new str(lst0, isp));
                        }
                    }
                    else
                    {
                        ObservableCollection<str> table = new ObservableCollection<str>();
                        List<List<string>> lst0 = new List<List<string>>();
                        List<string> lst1 = new List<string>();
                        lst1.Add(value);
                        lst1.Add("");
                        lst0.Add(lst1);
                        table.Add(new str(lst0, isp));
                        ObservableCollection<string> column_headers = new ObservableCollection<string>();
                        column_headers.Add(par_name);
                        channel ch = new channel(table, column_headers);
                        Data.channels.Add(ch);
                    }
                }
            }
            reader_main.Close();

            //=============================
            if (Data.chans_obr.Count < id_chan + 1)
            {
                for (int i = Data.chans_obr.Count; i < id_chan + 1; i++)
                {
                    Data.chans_obr.Add(new Obrabotka_of_fiz_exp(true, id_chan + 1));
                }
            }
            datagrid0.ItemsSource = Data.chans_obr[id_chan].rezh_num;
            datagrid1.ItemsSource = Data.chans_obr[id_chan].sreda;
            parametrs_table_build(datagrid2, Data.chans_obr[id_chan].rezh_par);
            parametrs_table_build(datagrid3, Data.chans_obr[id_chan].integr_par);
            foreach (section sec in Data.chans_obr[id_chan].sections)
            {
                section_table_build(sec, Data.chans_obr[id_chan], sections_cols);
            }
            Data.chans_obr[id_chan].func_to_value(id_chan);

            sqlconn.Close();
        }



        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

        void parametrs_table_build(DataGrid gr, parametrs par)
        {
            gr.ItemsSource = par.table;
            gr.IsReadOnly = true;
            //gr.MouseDown += datagrid_MouseDown;
            gr.IsReadOnly = true;
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

        void section_table_build(section sec, Obrabotka_of_fiz_exp exp_results, Grid main_gr)
        {
            main_gr.ColumnDefinitions.Add(new ColumnDefinition());
            main_gr.ColumnDefinitions.Add(new ColumnDefinition());

            Label lbl = new Label();
            lbl.Content = $"Сечение № {exp_results.sections.IndexOf(sec) + 1}";
            lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
            lbl.VerticalContentAlignment = VerticalAlignment.Center;

            Border br = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };
            br.Child = lbl;
            main_gr.Children.Add(br);
            Grid.SetRow(br, 0);
            Grid.SetColumn(br, main_gr.ColumnDefinitions.Count - 2);
            Grid.SetColumnSpan(br, 2);
            if (sec.pars != null)
            {
                Grid.SetRowSpan(br, 2);
                DataGrid data_gr = new DataGrid();
                data_gr.ColumnHeaderHeight = 40;
                data_gr.RowHeight = 67;
                data_gr.AutoGenerateColumns = false;
                data_gr.CanUserSortColumns = false;  //добавлено!!!!
                data_gr.BorderBrush = Brushes.Black;
                data_gr.BorderThickness = new Thickness(1);
                data_gr.CanUserReorderColumns = false;
                data_gr.CanUserResizeColumns = false;
                data_gr.CanUserAddRows = false;
                data_gr.Name = $"sec_{exp_results.sections.IndexOf(sec) + 1}";
                ScrollViewer.SetCanContentScroll(data_gr, false);
                main_gr.Children.Add(data_gr);
                Grid.SetRow(data_gr, 2);
                Grid.SetColumn(data_gr, main_gr.ColumnDefinitions.Count - 2);
                parametrs_table_build(data_gr, sec.pars);
            }
        }

    }
}
