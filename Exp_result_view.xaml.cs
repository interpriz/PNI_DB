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
    /// Логика взаимодействия для Exp_result_view.xaml
    /// </summary>
    public partial class Exp_result_view : Page
    {


        Experiment_search exp_wind_search = (Experiment_search)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_search_wind").FirstOrDefault();
        public static int id_chan { get; set; }// номер выбранного канала

        public static int chan_count { get; set; }// количество каналов

        public Exp_result_view()
        {
            InitializeComponent();

            for (int i = 1; i < chan_count; i++)
            {
                RadioButton radio = new RadioButton { Content = $"Канал {i + 1}", BorderBrush = new SolidColorBrush(Color.FromRgb(0, 14, 153)) };
                radio.Checked += new RoutedEventHandler(RadioButton_Checked);
                stackpanel_chan.Children.Add(radio);
            }

            Data.chans_results = new List<Results_of_fiz_exp>();
            for (int i = 0; i < chan_count; i++)
            {
                Data.chans_results.Add(new Results_of_fiz_exp(true, i + 1));
            }
            radiobut_chan1.IsChecked = true;
            //sections_cols.Children.Clear();
        }

        void parametrs_table_build(DataGrid gr, parametrs par)
        {
            gr.ItemsSource = par.table;
            //gr.CellEditEnding += datagrid2_CellEditEnding;
            //gr.KeyDown += TxtBox_chan_KeyDown;
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
                data_gr.IsReadOnly = true;
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
                    data_gr.IsReadOnly = true;
                    data_gr.RowHeight = 40;
                    data_gr.AutoGenerateColumns = false;
                    data_gr.CanUserSortColumns = false;  //добавлено!!!!
                    data_gr.BorderBrush = Brushes.Black;
                    data_gr.BorderThickness = new Thickness(1);
                    data_gr.CanUserReorderColumns = false;
                    data_gr.CanUserResizeColumns = false;
                    data_gr.HeadersVisibility = DataGridHeadersVisibility.None;
                    data_gr.CanUserAddRows = false;
                    //data_gr.CellEditEnding += datagrid1_CellEditEnding;
                    //data_gr.KeyDown += TxtBox_chan_KeyDown;
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



            //Binding bind1 = new Binding();
            //bind1.Path = new PropertyPath("cols[0]");
            //bind1.Mode = BindingMode.TwoWay;
            //grid.SetBinding(Grid.DataContextProperty, bind1);


            //Binding bind = new Binding();
            //bind.Path = new PropertyPath("[0].value");
            //bind.Mode = BindingMode.TwoWay;
            //txt.SetBinding(TextBox.TextProperty, bind);


        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            string[] names = radio.Content.ToString().Split(' ');
            id_chan = Convert.ToInt32(names[1]) - 1;

            sections_cols.Children.Clear();
            sections_cols.ColumnDefinitions.Clear();
            datagrid2.Columns.Clear();
            datagrid3.Columns.Clear();

            datagrid0.ItemsSource = Data.chans_results[id_chan].rezh_num;
            datagrid1.ItemsSource = Data.chans_results[id_chan].sreda;
            parametrs_table_build(datagrid2, Data.chans_results[id_chan].rezh_par);
            parametrs_table_build(datagrid3, Data.chans_results[id_chan].prochie_par);
            foreach (section sec in Data.chans_results[id_chan].sections)
            {
                section_table_build(sec, Data.chans_results[id_chan], sections_cols);
            }

            datagrid0.RowHeight = Data.chans_results[id_chan].max_travers_points * 40 + 2;
            datagrid1.RowHeight = Data.chans_results[id_chan].max_travers_points * 40 + 2;
            datagrid2.RowHeight = Data.chans_results[id_chan].max_travers_points * 40 + 2;
            datagrid3.RowHeight = Data.chans_results[id_chan].max_travers_points * 40 + 2;
        }

        private void batt_obr_rez_Click(object sender, RoutedEventArgs e)
        {
            exp_wind_search.item4.IsSelected = false;
            exp_wind_search.item5.IsEnabled = true;
            exp_wind_search.item5.IsSelected = true;
        }

        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

    }
}
