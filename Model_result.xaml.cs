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

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Model_rezult.xaml
    /// </summary>

    public partial class Model_result : Page
    {
        Experiment_add exp_wind = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
        //public static int id_chan { get; set; }// номер выбранного канала
        public static bool save = true;//проверка сохранения перед добавлением результатов моделирования

        //int changes = 0;//количество изменений в таблице
        public Model_result()
        {
            InitializeComponent();


            //sections_cols.Children.Clear();
            //sections_cols.ColumnDefinitions.Clear();
            //datagrid2.Columns.Clear();
            //datagrid3.Columns.Clear();

            //Data.id_obj = "6";
            //Data.current_mode = 1;
            //Data.current_realization = "1";
            //Data.current_channel = 1;

            Data.modelling_results = new Results_of_modelling(false);
            Data.modelling_results.add_rezhim();
            Data.modelling_results.add_rezhim();
            Data.modelling_results.add_rezhim();

            datagrid0.ItemsSource = Data.modelling_results.rezh_num;
            datagrid1.ItemsSource = Data.modelling_results.sreda;
            parametrs_table_build(datagrid2, Data.modelling_results.rezh_par);
            parametrs_table_build(datagrid3, Data.modelling_results.prochie_par);
            foreach (section sec in Data.modelling_results.sections)
            {
                section_table_build(sec, Data.modelling_results, sections_cols);
            }

            double row_height = Data.modelling_results.max_travers_points * 40 + 2;
            datagrid0.RowHeight = row_height;
            datagrid1.RowHeight = row_height;
            datagrid2.RowHeight = row_height;
            datagrid3.RowHeight = row_height;

        }

        void parametrs_table_build(DataGrid gr, parametrs par)
        {
            gr.ItemsSource = par.table;
            gr.CellEditEnding += datagrid2_CellEditEnding;
            gr.KeyDown += TxtBox_chan_KeyDown;
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

        void section_table_build(section sec, Results_of_fiz_exp exp_results, Grid main_gr)
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
                data_gr.RowHeight = exp_results.max_travers_points * 40 + 2;
                data_gr.ColumnHeaderHeight = 40;
                data_gr.AutoGenerateColumns = false;
                data_gr.CanUserSortColumns = false;  //добавлено!!!!
                data_gr.BorderBrush = Brushes.Black;
                data_gr.BorderThickness = new Thickness(1);
                data_gr.CanUserReorderColumns = false;
                data_gr.CanUserResizeColumns = false;
                data_gr.CanUserAddRows = false;
                ScrollViewer.SetCanContentScroll(data_gr, false);
                main_gr.Children.Add(data_gr);
                Grid.SetRow(data_gr, 2);
                Grid.SetColumn(data_gr, main_gr.ColumnDefinitions.Count - 2);
                parametrs_table_build(data_gr, sec.pars);
            }
            else
            {
                lbl = new Label();
                lbl.Content = $"Координаты";
                lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                lbl.VerticalContentAlignment = VerticalAlignment.Center;

                br = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };
                br.Child = lbl;
                main_gr.Children.Add(br);
                Grid.SetRow(br, 1);
                Grid.SetColumn(br, main_gr.ColumnDefinitions.Count - 2);

                lbl = new Label();
                lbl.Content = $"Результаты траверсирования";
                lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                lbl.VerticalContentAlignment = VerticalAlignment.Center;

                br = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };
                br.Child = lbl;
                main_gr.Children.Add(br);
                Grid.SetRow(br, 1);
                Grid.SetColumn(br, main_gr.ColumnDefinitions.Count - 1);


                Grid cord = new Grid();
                travers_parametrs_table_buid(cord, sec.coordinates, exp_results);
                main_gr.Children.Add(cord);
                Grid.SetRow(cord, 2);
                Grid.SetColumn(cord, main_gr.ColumnDefinitions.Count - 2);

                Grid trav_pars = new Grid();
                travers_parametrs_table_buid(trav_pars, sec.trav_pars, exp_results);
                main_gr.Children.Add(trav_pars);
                Grid.SetRow(trav_pars, 2);
                Grid.SetColumn(trav_pars, main_gr.ColumnDefinitions.Count - 1);
            }
        }

        void travers_parametrs_table_buid(Grid grid, travers_parametrs tr_par, Results_of_fiz_exp exp_results)
        {
            for (int j = 0; j < tr_par.column_headers.Count; j++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });

                Label lbl = new Label();
                lbl.Content = $"{tr_par.column_headers[j]}, {Parametrs.get_param(tr_par.column_headers[j]).unit}";
                lbl.HorizontalContentAlignment = HorizontalAlignment.Center;
                lbl.VerticalContentAlignment = VerticalAlignment.Center;

                Border br = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.Black };
                br.Child = lbl;
                grid.Children.Add(br);
                Grid.SetRow(br, 0);
                Grid.SetColumn(br, j);

                for (int i = 1; i <= tr_par.travers_table.Count; i++)
                {
                    if (grid.RowDefinitions.Count - 1 < tr_par.travers_table.Count)
                        grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(exp_results.max_travers_points * 40 + 2) });//здесь нужно писать максимальное число строк имеющееся при траверсировании

                    DataGrid data_gr = new DataGrid();
                    data_gr.RowHeight = 40;
                    data_gr.AutoGenerateColumns = false;
                    data_gr.CanUserSortColumns = false;  //добавлено!!!!
                    data_gr.BorderBrush = Brushes.Black;
                    data_gr.BorderThickness = new Thickness(1);
                    data_gr.CanUserReorderColumns = false;
                    data_gr.CanUserResizeColumns = false;
                    data_gr.HeadersVisibility = DataGridHeadersVisibility.None;
                    data_gr.CanUserAddRows = false;
                    data_gr.CellEditEnding += datagrid1_CellEditEnding;
                    data_gr.KeyDown += TxtBox_chan_KeyDown;
                    ScrollViewer.SetCanContentScroll(data_gr, false);
                    var style = new Style();
                    style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                    style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
                    data_gr.Columns.Add(new DataGridTextColumn
                    {
                        Binding = new Binding($"value") { Mode = BindingMode.TwoWay },
                        ElementStyle = style
                    });
                    data_gr.ItemsSource = tr_par.travers_table[i - 1].cols[j];
                    grid.Children.Add(data_gr);
                    Grid.SetRow(data_gr, i);
                    Grid.SetColumn(data_gr, j);
                }

            }

        }


        private void datagrid1_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var el = e.EditingElement as TextBox;
                    int rowIndex = e.Row.GetIndex();
                    int columnIndex = e.Column.DisplayIndex;
                    parametr data = (parametr)e.Row.DataContext;
                    if (el.Text != data.value && data.mode == "old")
                    {
                        try
                        {
                            double d = Convert.ToDouble(el.Text);
                            data.mode = "update";
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
                    //if (el.Text != data.cols[columnIndex][0] && data.cols[columnIndex][1] != "new")
                    //{
                    //    data.cols[columnIndex][1] = "update";
                    //}
                    /**/
                }
            }
            save = false;
        }// среда

        private void datagrid2_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var el = e.EditingElement as TextBox;
                    int rowIndex = e.Row.GetIndex();
                    int columnIndex = e.Column.DisplayIndex;
                    row data = (row)e.Row.DataContext;
                    if (el.Text != data.cols[columnIndex].value && data.cols[columnIndex].mode == "old")
                    {
                        try
                        {
                            double d = Convert.ToDouble(el.Text);
                            data.cols[columnIndex].mode = "update";
                            //changes++;
                            //batt_obr_rez.IsEnabled = false;
                        }
                        catch
                        {
                            messtxt.Text = $"Ошибка ввода!";
                            messbar.IsActive = true;
                            el.Text = data.cols[columnIndex].value;
                        }
                    }
                }
            }
            save = false;
        }// остальные таблицы

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

        private void batt_save_Click(object sender, RoutedEventArgs e)// сохранение измнений в БД
        {
            //if (Data.chans_results[id_chan].save_in_DB(id_chan + 1))
            //{
            //    //changes = 0;
            //    //batt_obr_rez.IsEnabled = true;
            //    messtxt.Text = "Данные сохранены успешно!";
            //    messbar.IsActive = true;
            //    save = true;
            //    //Data.constr.Clear();
            //    DB_constr.Del();
            //    DB_constr.Create();
            //    //exp_wind.new_Construct = new Exp_construct();
            //}
            //else
            //{
            //    messtxt.Text = "Не все поля заполнены!";
            //    messbar.IsActive = true;
            //}
        }

        private void batt_obr_rez_Click(object sender, RoutedEventArgs e)
        {
            //bool f1 = Data.constr[id_chan].rezh_obr_7.Count != 0;       //количество режимных параметров
            //bool f2 = Data.constr[id_chan].integr_obr.Count != 0;       //количество интегральных параметров
            //bool f3 = Data.constr[id_chan].sechen.Count != 0;           // количество сечений
            //bool f4 = true;// число параметров сечений не 0
            //foreach (Obr_parametrs_sech_7 i in Data.constr[id_chan].obr_params_sech)
            //{
            //    if (i.phys_obr_7.Count == 0)
            //    {
            //        f4 = false;
            //        break;
            //    }
            //}

            //if (f1 && f2 && f3 && f4)
            //{
            //    if (save)
            //    {
            //        exp_wind.item5.IsSelected = false;
            //        exp_wind.item6.IsEnabled = true;
            //        exp_wind.item6.IsSelected = true;
            //    }
            //    else
            //    {
            //        messtxt.Text = "Данные изменены! Для продолжения сохраните их!";
            //        messbar.IsActive = true;
            //    }
            //}
            //else
            //{
            //    messtxt.Text = $"Таблица обработки результатов канала №{id_chan + 1} не сконфигурирована! \r\nВернитесь на страницу конфигуратора!";
            //    messbar.IsActive = true;
            //}
        }

        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }
    }
}
