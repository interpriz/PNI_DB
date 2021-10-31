using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Логика взаимодействия для Geom_param.xaml
    /// </summary>
    public partial class Geom_param : Page
    {

        int chanell_DB_count;

        Experiment_add exp_wind = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
        Modeling_add model_wind = (Modeling_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault();


        public static MaterialDesignThemes.Wpf.DialogHost dialog = new MaterialDesignThemes.Wpf.DialogHost();
        //List<channel> Data.channels = new List<channel>();   //список каналов
        
        public static ObservableCollection<rowheader> rowheaders;       //список заголовков строк (исполнения)
        //List<string> cmb_parametrs = new List<string>();//список параметров для выбора при добавлении нового столбца
        ObservableCollection<string> cmb_parametrs = new ObservableCollection<string>();
        String conn_str = User.Connection_string;       //строка подключения
        public static string vid_param { get; set; }


        public static bool save = true;//проверка сохранения перед добавлением результатов экспериента

        static string page_mode;

        public Geom_param(string page_mode) //Modeling - при работе с моделированием /Experiment - при работе с экспериментами
        {
            InitializeComponent();

            Geom_param.page_mode = page_mode;

            // окошко добавления нового параметра
            bool_exp.geom = true;
            dialog = Dialog_add_param;


            Data.channels = new List<channel>();
            rowheaders = new ObservableCollection<rowheader>();
            rowheader.mode = page_mode;
            string id_obj = Data.id_obj; //Id$ - в БД (идентификатор обьекта)


            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            //заполнение комбобокса параметров для выбора при добавлении нового столбца
            cmb_parametrs.Add("Новый параметр");
            NpgsqlCommand com_params = new NpgsqlCommand($"select * from main_block.select_parametrs('Геометрические параметры') order by name_param;", sqlconn);
            NpgsqlDataReader reader_par = com_params.ExecuteReader();
            Parametrs.geom_pars = new Dictionary<string, Param>();
            while (reader_par.Read())
            {
                cmb_parametrs.Add(reader_par[0].ToString());
                Param p = new Param() { short_name = reader_par[1].ToString(), unit = reader_par[2].ToString() };
                Parametrs.geom_pars.Add(reader_par[0].ToString(), p);
            }
            reader_par.Close();

            switch (page_mode)
            {
                case "Experiment":
                    page_name.Text = "3.Ввод геометрических параметров объекта";
                    break;

                case "Modeling":
                    page_name.Text = "2.Ввод геометрических параметров объекта";
                    break;
            }
            

            NpgsqlCommand com_name = new NpgsqlCommand($"select * from main_block.select_names_id({Data.id});", sqlconn);
            NpgsqlDataReader rdr1 = com_name.ExecuteReader();
            //заполнение лэйблов
            //загрузка изображения в picturebox3
            while (rdr1.Read())
            {
                txt_proc.Text = rdr1[0].ToString();
                txt_type.Text = rdr1[1].ToString();
                txt_obj.Text = rdr1[2].ToString();
            }
            rdr1.Close();


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

                    rowheaders.Add(new rowheader((rowheaders.Count + 1).ToString(), isp));
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

                            //if (isp > rowheaders.Count())
                            if (isp > rowheaders[rowheaders.Count - 1].realization)
                                rowheaders.Add(new rowheader((rowheaders.Count + 1).ToString().ToString(), isp));
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

            int i = 1;
            foreach (channel chn in Data.channels)
            {
                DataTableExpGeom.add_chanel(main_table, i, cmb_parametrs, chn.column_headers, chn.table);
                i++;
            }
            TxtBox_chan.Text = Data.channels.Count.ToString();
            chanell_DB_count = Data.channels.Count;


            datagrid0.ItemsSource = rowheaders;

            sqlconn.Close();
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
        private void Btn_AddRow_Click(object sender, RoutedEventArgs e)
        {
            if (Data.channels.Count != 0)
            {
                int i = 1;
                foreach (channel chn in Data.channels)
                {
                    if (chn.column_headers.Count == 0)
                    {
                        messtxt2.Text = $"Отсутствуют параметры канала№{i} !";
                        messbar2.IsActive = true;
                        return;
                    }
                    i++;
                }
            }
            else
            {
                messtxt2.Text = "Не создано ни одного канала!";
                messbar2.IsActive = true;
                return;
            }
            
            int r = 1;
            if (rowheaders.Count != 0) 
                r = rowheaders[rowheaders.Count - 1].realization + 1;
            rowheaders.Add(new rowheader((rowheaders.Count + 1).ToString(), r));
            //datagrid0.ItemsSource = null;
            //datagrid0.ItemsSource = rowheaders;
            foreach (channel chn in Data.channels)
            {
                List<List<string>> lst0 = new List<List<string>>();
                for (int i = 0; i < chn.column_headers.Count; i++)
                {
                    List<string> lst1 = new List<string>();
                    lst1.Add("");
                    lst1.Add("new");
                    lst0.Add(lst1);
                }
                chn.table.Add(new str(lst0, rowheaders[rowheaders.Count - 1].realization));
            }
            save = false;
        }

        private void Btn_Channel_Click(object sender, RoutedEventArgs e)// добавление\удаление каналов
        {
            int col = chanell_DB_count;
            int k = Convert.ToInt32(TxtBox_chan.Text) - Data.channels.Count;
            if (Convert.ToInt32(TxtBox_chan.Text) >= col)
            {
                if (k >= 0)
                {
                    for (int i = col + 1; i <= col + k; i++)
                    {
                        ObservableCollection<str> table = new ObservableCollection<str>();
                        //for (int j = 0; j < rowheaders.Count; j++)
                        //{
                        //    List<string> lst = new List<string>();
                        //    lst.Add("");
                        //    table.Add(new str(lst,0));
                        //}
                        foreach (rowheader row in rowheaders)
                        {
                            List<List<string>> lst0 = new List<List<string>>();
                            List<string> lst1 = new List<string>();
                            lst1.Add("");
                            lst1.Add("new");
                            lst0.Add(lst1);
                            table.Add(new str(lst0, row.realization));
                        }
                        ObservableCollection<string> column_headers = new ObservableCollection<string>();
                        channel ch = new channel(table, column_headers);
                        Data.channels.Add(ch);
                        DataTableExpGeom.add_chanel(main_table, i, cmb_parametrs, column_headers, table);
                    }
                }
                else
                {
                    for (int i = 0; i < k * -1; i++)
                    {
                        Data.channels.RemoveAt(Data.channels.Count - 1);
                        main_table.Children.RemoveAt(main_table.Children.Count - 1);
                        main_table.Children.RemoveAt(main_table.Children.Count - 1);
                        main_table.ColumnDefinitions.RemoveAt(main_table.ColumnDefinitions.Count - 1);
                    }
                }
            }
            else
            {
                TxtBox_chan.Text = col.ToString();
            }
            save = false;
        }
        class DataTableExpGeom
        {
            private static void TxtBox_chan_KeyDown(object sender, KeyEventArgs e)
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
                   || (e.Key.ToString() == "OemComma"))
                {
                    e.Handled = false;
                    //return;
                }
                else
                    e.Handled = true;
            }

            private static void datagrid0_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
            {
                if (e.EditAction == DataGridEditAction.Commit)
                {
                    var column = e.Column as DataGridBoundColumn;
                    if (column != null)
                    {
                        var el = e.EditingElement as TextBox;
                        int rowIndex = e.Row.GetIndex();
                        int columnIndex = e.Column.DisplayIndex;
                        str data = (str)e.Row.DataContext;

                        bool f1 = true;
                        string value = el.Text;                                  // само значение параметра
                        try
                        {
                            double d = Convert.ToDouble(value);
                        }
                        catch { f1 = false; }

                        if (f1)
                        {
                            if (el.Text != data.cols[columnIndex][0] && data.cols[columnIndex][1] != "new")
                            {
                                data.cols[columnIndex][1] = "update";
                                Geom_param.save = false;
                            }
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show(
                            $"Значение \"{value}\" не является числом!", "Caution", MessageBoxButton.OK);
                            el.Text = "";
                        }

                    }
                }
            }

            public static void add_chanel(Grid main_table, int chn_number, ObservableCollection<string> cmb_parametrs, ObservableCollection<string> headers, ObservableCollection<str> table)
            {
                Label lbl = new Label();
                lbl.Content = $"Канал №{chn_number}";
                lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                lbl.VerticalContentAlignment = VerticalAlignment.Center;

                ComboBox cmb = new ComboBox();
                cmb.Width = 150;
                cmb.ItemsSource = cmb_parametrs;
                MaterialDesignThemes.Wpf.HintAssist.SetHint(cmb, "Выберите параметр");
                MaterialDesignThemes.Wpf.HintAssist.SetHintOpacity(cmb, 0.4);



                StackPanel stack = new StackPanel() { Orientation = Orientation.Horizontal };
                TextBlock add = new TextBlock() { Text = "Добавить", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0) };
                System.Windows.Controls.Primitives.ToggleButton toggle = new System.Windows.Controls.Primitives.ToggleButton();
                MaterialDesignThemes.Wpf.ToggleButtonAssist.SetSwitchTrackOnBackground(toggle, new SolidColorBrush(Colors.Red));
                MaterialDesignThemes.Wpf.ToggleButtonAssist.SetSwitchTrackOffBackground(toggle, new SolidColorBrush(Colors.DarkGreen));
                TextBlock del = new TextBlock() { Text = "Удалить", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                //RadioButton add = new RadioButton() { Content = "Добавить" };
                //add.IsChecked = true;
                //RadioButton del = new RadioButton() { Content = "Удалить" };
                void ToggleButton_Checked(object sender, RoutedEventArgs e)
                {
                    System.Windows.Controls.Primitives.ToggleButton tog = sender as System.Windows.Controls.Primitives.ToggleButton;
                    cmb.ItemsSource = headers;
                    tog.Background = new SolidColorBrush(Colors.White);
                }
                void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
                {
                    System.Windows.Controls.Primitives.ToggleButton tog = sender as System.Windows.Controls.Primitives.ToggleButton;
                    cmb.ItemsSource = cmb_parametrs;
                }
                toggle.Checked += new RoutedEventHandler(ToggleButton_Checked);
                toggle.Unchecked += new RoutedEventHandler(ToggleButton_Unchecked);
                stack.Children.Add(add);
                stack.Children.Add(toggle);
                //add.Checked += new RoutedEventHandler(RadioButton_Checked);
                //del.Checked += new RoutedEventHandler(RadioButton_Checked);
                stack.Children.Add(del);

                Grid main_header = new Grid();
                main_header.ColumnDefinitions.Add(new ColumnDefinition());
                main_header.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150) });
                main_header.RowDefinitions.Add(new RowDefinition());
                main_header.RowDefinitions.Add(new RowDefinition());
                main_header.Children.Add(lbl);
                Grid.SetRow(lbl, 0);
                Grid.SetColumn(lbl, 0);
                Grid.SetRowSpan(lbl, 2);

                main_header.Children.Add(cmb);
                Grid.SetRow(cmb, 1);
                Grid.SetColumn(cmb, 1);

                main_header.Children.Add(stack);
                Grid.SetRow(stack, 0);
                Grid.SetColumn(stack, 1);

                DataGrid gr = new DataGrid();
                gr.ColumnHeaderHeight = 40;
                gr.RowHeight = 67;
                gr.AutoGenerateColumns = false;
                gr.CanUserSortColumns = false;  //добавлено!!!!
                gr.BorderBrush = Brushes.Black;
                gr.BorderThickness = new Thickness(1);
                gr.CanUserReorderColumns = false;
                gr.CanUserResizeColumns = false;
                ScrollViewer.SetCanContentScroll(gr, false);

                //запрет ввода символов кроме цифр
                gr.KeyDown += TxtBox_chan_KeyDown;
                //отлов момента изменения данных в ячейке
                gr.CellEditEnding += datagrid0_CellEditEnding;


                foreach (string head in headers)
                {
                    AddHeader(gr, head);
                }


                gr.ItemsSource = table;
                gr.Name = $"chanel{chn_number}";

                void cmb_SelectionChanged(object sender, SelectionChangedEventArgs e)
                {
                    if (cmb.SelectedItem != null)
                    {
                        string select = cmb.SelectedItem.ToString();
                        cmb.SelectedItem = null;
                        if (toggle.IsChecked == false)
                        {
                            if (select == "Новый параметр")
                            {
                                dialog.IsOpen = true;
                                switch (page_mode)
                                {
                                    case "Experiment":
                                        //Experiment_add exp_wind = (Experiment_add)Application.Current.Windows[4];
                                        Experiment_add exp_wind = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
                                        exp_wind.Butt_back.IsEnabled = false;
                                        exp_wind.listview_items.IsEnabled = false;
                                        break;

                                    case "Modeling":

                                        break;
                                }
                                
                            }
                            else
                            {
                                
                                if(table.Count != 0)
                                {
                                    if (headers.Count != 0)
                                    {
                                        foreach (str s in table)
                                        {
                                            s.cols.Add(new List<string> { "", "new" });//изменено для возможности удаления и добавления
                                        }
                                    }
                                }
                                if (headers.IndexOf(select) == -1)
                                {
                                    headers.Add(select);
                                    DataTableExpGeom.AddHeader(gr, select);
                                }
                            }
                        }
                        else
                        {
                            //gr.ItemsSource = null;
                            int i = headers.IndexOf(select);
                            headers.RemoveAt(i);
                            //cmb.ItemsSource = null;
                            //cmb.ItemsSource = headers;
                            foreach (str s in table)
                            {
                                s.cols.RemoveAt(i);
                            }
                            gr.Columns.RemoveAt(i);
                            //gr.ItemsSource = table;
                        }
                    }

                }
                cmb.SelectionChanged += new SelectionChangedEventHandler(cmb_SelectionChanged);

                ColumnDefinition newcol = new ColumnDefinition();

                Binding binding = new Binding();
                binding.ElementName = $"chanel{chn_number}"; // элемент-источник
                binding.Path = new PropertyPath("Width"); // свойство элемента-источника
                newcol.SetBinding(ColumnDefinition.WidthProperty, binding); // установка привязки для элемента-приемника
                main_table.ColumnDefinitions.Add(newcol);

                Border br = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black, Name = "test" };
                br.Child = main_header;
                main_table.Children.Add(br);
                Grid.SetRow(br, 0);
                Grid.SetColumn(br, chn_number);

                main_table.Children.Add(gr);
                Grid.SetRow(gr, 1);
                Grid.SetColumn(gr, chn_number);
            }

            public static void AddHeader(DataGrid gr, string name)
            {
                gr.Columns.Add(new DataGridTextColumn
                {
                    Header = $"{name}, {Parametrs.get_param(name).unit}",
                    Binding = new Binding($"cols[{gr.Columns.Count}][0]") { Mode = BindingMode.TwoWay },
                });
            }
        }

        private void Butt_OK_Click(object sender, RoutedEventArgs e)// занесение нового параметра в таблицу параметров в базе
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
                if (check  == "")
                {
                    Parametrs.insert_parametr("Геометрические параметры", name, short_name, unit);
                    dialog.IsOpen = false;
                    switch (page_mode)
                    {
                        case "Experiment":
                            exp_wind.Butt_back.IsEnabled = true;
                            exp_wind.listview_items.IsEnabled = true;
                            break;

                        case "Modeling":

                            break;
                    }
                    cmb_parametrs.Add(txt_par_name.Text);
                }
                else
                {
                    if(check == name)
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

        private void batt_save_Click(object sender, RoutedEventArgs e)//сохранить изменения
        {
            //проверка уникальности исполнений
            if (Data.channels.Count != 0)
            {
                List<string> realizations = new List<string>();
                for(int i = 0; i< Data.channels[0].table.Count; i++)
                {
                    string r = "";
                    foreach(channel chn in Data.channels)
                    {
                        foreach(List<string> item in chn.table[i].cols)
                        {
                            r += item[0];
                        }
                    }
                    if (realizations.Contains(r))
                    {
                        messtxt2.Text = $"Исполнение №{i+1} не уникально! \r\nСверьтесь с исполнением №{realizations.IndexOf(r)+1}";
                        messbar2.IsActive = true;
                        return;
                    }
                    else
                    {
                        realizations.Add(r);
                    }
                    
                }
            }
            
            
            bool f = true;
            foreach (channel chn in Data.channels)
            {
                if (!chn.save_changes(Data.channels.IndexOf(chn) + 1))
                {
                    f = false;
                    break;
                }   
            }
            if (f)
            {

                chanell_DB_count = Data.channels.Count;
                bool_exp.geom = true;
                messtxt2.Text = "Сохранение данных произошло успешно!";
                messbar2.IsActive = true;
                save = true;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)// добавление результатов экспериментов
        {
            if (save)
            {
                Button bt = sender as Button;
                int r = Convert.ToInt32(bt.Tag) - 1;
                Data.current_realization = Convert.ToString(rowheaders[r].realization); // номер реализации в базе
                Data.id_realization = r;    // номер строки (реализаци в приложении)
                Data.constr.Clear();
                if (Data.chans_results != null) Data.chans_results.Clear();
                if (Data.chans_obr != null) Data.chans_obr.Clear();
                //DB_constr.chan_dann.Clear();
                //DB_constr.sech_count_db.Clear();
                DB_constr.Del();
                switch (page_mode)
                {
                    case "Experiment":
                        exp_wind.new_Construct = new Exp_construct();
                        //exp_wind.condition = "step4";
                        exp_wind.item3.IsSelected = false;
                        exp_wind.item4.IsEnabled = true;
                        exp_wind.item4.IsSelected = true;
                        //exp_wind.frame.Navigate(exp_wind.new_Construct);
                        break;

                    case "Modeling":

                        break;
                }
            }
            else
            {
                messtxt2.Text = "Данные изменены! Для продолжения сохраните их!";
                messbar2.IsActive = true;
            }

        }

        private void Dialog_add_param_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            switch (page_mode)
            {
                case "Experiment":
                    exp_wind.Butt_back.IsEnabled = true;
                    exp_wind.listview_items.IsEnabled = true;
                    break;

                case "Modeling":

                    break;
            }
            
        }

        private void messbut2_Click(object sender, RoutedEventArgs e)
        {
            messbar2.IsActive = false;
        }
    }

}
