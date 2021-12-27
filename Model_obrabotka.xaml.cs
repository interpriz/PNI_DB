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
    /// Логика взаимодействия для Model_obrabotka.xaml
    /// </summary>
    public partial class Model_obrabotka : Page
    {
        //Experiment_add exp_wind = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
        //int id_chan;
        public Model_obrabotka(int chan)
        {
            InitializeComponent();
            //id_chan = chan;// индекс канала в списке данных обработки каналов
            //page_name.Text += chan + 1;
            //batt_.Visibility = Visibility.Hidden;

            //if (Data.chans_obr[id_chan].rezh_par == null)
            //{
            //    Data.chans_obr[id_chan] = new Obrabotka_of_fiz_exp(Data.constr[id_chan], id_chan + 1);
            //}
            //else
            //{
            //    Data.chans_obr[id_chan].update(Data.constr[id_chan]);
            //}
            //datagrid0.ItemsSource = Data.chans_obr[id_chan].rezh_num;
            //datagrid1.ItemsSource = Data.chans_obr[id_chan].sreda;
            //parametrs_table_build(datagrid2, Data.chans_obr[id_chan].rezh_par);
            //parametrs_table_build(datagrid3, Data.chans_obr[id_chan].integr_par);
            //foreach (section sec in Data.chans_obr[id_chan].sections)
            //{
            //    section_table_build(sec, Data.chans_obr[id_chan], sections_cols);
            //}
            //Data.chans_obr[id_chan].func_to_value(id_chan);
        }

        private void batt_save_Click(object sender, RoutedEventArgs e)// сохранение измнений в БД
        {
            //Data.chans_obr[id_chan].save_in_DB(id_chan + 1);
            //messtxt.Text = "Данные сохранены успешно!";
            //messbar.IsActive = true;
        }

        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

        void parametrs_table_build(DataGrid gr, parametrs par)
        {
            gr.ItemsSource = par.table;
            gr.IsReadOnly = true;
            gr.MouseDown += datagrid_MouseDown;
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

        private void datagrid_MouseDown(object sender, MouseButtonEventArgs e)
        {

            //DataGrid gr = sender as DataGrid;
            //int id_row = ((ObservableCollection<row>)gr.ItemsSource).IndexOf((row)gr.CurrentItem);
            //string name = gr.CurrentCell.Column.Header.ToString().Split(',')[0];
            //string grid_name = gr.Name;
            //Dictionary<string, Param> arguments = new Dictionary<string, Param>();
            //Dictionary<string, double> arg_value = new Dictionary<string, double>();
            //List<Dictionary<string, double>> arg_i_value = new List<Dictionary<string, double>>();// список словарей с параметрами(обозначениями) и их значениями для каждой точки траверсирования
            //switch (grid_name)
            //{
            //    // интегральные параметры
            //    case "datagrid3":
            //        //геометрические параметры
            //        foreach (string par in Data.channels[id_chan].column_headers)
            //        {
            //            Param p = Parametrs.geom_pars[par];
            //            arguments.Add(par, p);
            //            string value = Data.channels[id_chan].par_value(par, Data.id_realization);
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }
            //        //режимные параметры
            //        foreach (string par in Data.chans_results[id_chan].rezh_par.column_headers)
            //        {
            //            Param p = Parametrs.reg_pars[par];
            //            arguments.Add(par, p);
            //            string value = Data.chans_results[id_chan].rezh_par.par_value(par, id_row).value;
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }
            //        foreach (string par in Data.chans_obr[id_chan].rezh_par.column_headers)
            //        {
            //            Param p = Parametrs.reg_pars[par];
            //            if (!arguments.ContainsKey(par))
            //            {
            //                string value = Data.chans_obr[id_chan].rezh_par.par_value(par, id_row).value;
            //                if (value != "")
            //                {
            //                    arguments.Add(par, p);
            //                    arg_value.Add(p.short_name, Convert.ToDouble(value));
            //                }
            //            }
            //        }
            //        //параметры обработки результатов всех сечений 
            //        foreach (section sec in Data.chans_obr[id_chan].sections)
            //        {
            //            int id = Data.chans_obr[id_chan].sections.IndexOf(sec);
            //            foreach (string par in sec.pars.column_headers)
            //            {
            //                Param p = Parametrs.phys_pars[par];

            //                p.short_name += $"_sec{id + 1}";

            //                string value = sec.pars.par_value(par, id_row).value;
            //                if (value != "")
            //                {

            //                    arguments.Add(par + $"_sec{id + 1}", p);
            //                    arg_value.Add(p.short_name, Convert.ToDouble(value));
            //                }
            //            }
            //        }
            //        //прочие параметры
            //        foreach (string par in Data.chans_results[id_chan].prochie_par.column_headers)
            //        {
            //            Param p = Parametrs.phys_pars[par];
            //            arguments.Add(par, p);
            //            string value = Data.chans_results[id_chan].prochie_par.par_value(par, id_row).value;
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }
            //        Vvod_func.par = Data.chans_obr[id_chan].integr_par.par_value(name, id_row);

            //        break;

            //    // режимные параметры
            //    case "datagrid2":
            //        //геометрические параметры
            //        foreach (string par in Data.channels[id_chan].column_headers)
            //        {
            //            Param p = Parametrs.geom_pars[par];
            //            arguments.Add(par, p);
            //            string value = Data.channels[id_chan].par_value(par, Data.id_realization);
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }
            //        //режимные параметры
            //        foreach (string par in Data.chans_results[id_chan].rezh_par.column_headers)
            //        {
            //            Param p = Parametrs.reg_pars[par];
            //            arguments.Add(par, p);
            //            string value = Data.chans_results[id_chan].rezh_par.par_value(par, id_row).value;
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }
            //        //прочие параметры
            //        foreach (string par in Data.chans_results[id_chan].prochie_par.column_headers)
            //        {
            //            Param p = Parametrs.phys_pars[par];
            //            arguments.Add(par, p);
            //            string value = Data.chans_results[id_chan].prochie_par.par_value(par, id_row).value;
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }

            //        Vvod_func.par = Data.chans_obr[id_chan].rezh_par.par_value(name, id_row);

            //        break;

            //    // сечения
            //    default:
            //        int sec_id = Convert.ToInt32(grid_name.Split('_')[1]) - 1;
            //        //параметры сечения
            //        if (Data.chans_results[id_chan].sections[sec_id].pars == null)
            //        {
            //            foreach (string par in Data.chans_results[id_chan].sections[sec_id].coordinates.column_headers)
            //            {
            //                Param p = new Param() { short_name = par + "_i", unit = "" };
            //                arguments.Add(par, p);
            //                int travers_points_count = Data.chans_results[id_chan].sections[sec_id].coordinates.travers_table[id_row].cols[0].Count;

            //                for (int i = 0; i < travers_points_count; i++)
            //                {
            //                    if (Data.chans_results[id_chan].sections[sec_id].coordinates.column_headers.IndexOf(par) == 0)
            //                    {
            //                        Dictionary<string, double> arg_j_value = new Dictionary<string, double>();
            //                        arg_i_value.Add(arg_j_value);
            //                    }
            //                    string value = Data.chans_results[id_chan].sections[sec_id].coordinates.par_value(par, id_row, i).value;
            //                    arg_i_value[i].Add(p.short_name, Convert.ToDouble(value));
            //                }
            //            }
            //            foreach (string par in Data.chans_results[id_chan].sections[sec_id].trav_pars.column_headers)
            //            {
            //                Param p = Parametrs.phys_pars[par];
            //                p.short_name += "_i";
            //                arguments.Add(par + $"_sec{sec_id + 1}", p);
            //                int travers_points_count = Data.chans_results[id_chan].sections[sec_id].trav_pars.travers_table[id_row].cols[0].Count;

            //                for (int i = 0; i < travers_points_count; i++)
            //                {
            //                    //if (Data.chans_results[id_chan].sections[sec_id].trav_pars.column_headers.IndexOf(par) == 0)
            //                    //{
            //                    //    Dictionary<string, double> arg_j_value = new Dictionary<string, double>();
            //                    //    arg_i_value.Add(arg_j_value);
            //                    //}
            //                    string value = Data.chans_results[id_chan].sections[sec_id].trav_pars.par_value(par, id_row, i).value;
            //                    arg_i_value[i].Add(p.short_name, Convert.ToDouble(value));
            //                }
            //            }
            //        }
            //        else
            //        {
            //            foreach (string par in Data.chans_results[id_chan].sections[sec_id].pars.column_headers)
            //            {
            //                Param p = Parametrs.phys_pars[par];
            //                p.short_name += $"_sec{sec_id + 1}";
            //                arguments.Add(par + $"_sec{sec_id + 1}", p);
            //                string value = Data.chans_results[id_chan].sections[sec_id].pars.par_value(par, id_row).value;
            //                arg_value.Add(p.short_name, Convert.ToDouble(value));
            //            }
            //        }
            //        //геометрические параметры
            //        foreach (string par in Data.channels[id_chan].column_headers)
            //        {
            //            Param p = Parametrs.geom_pars[par];
            //            arguments.Add(par, p);

            //            string value = Data.channels[id_chan].par_value(par, Data.id_realization);
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }
            //        //режимные параметры
            //        foreach (string par in Data.chans_results[id_chan].rezh_par.column_headers)
            //        {
            //            Param p = Parametrs.reg_pars[par];
            //            arguments.Add(par, p);

            //            string value = Data.chans_results[id_chan].rezh_par.par_value(par, id_row).value;
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }
            //        //прочие параметры
            //        foreach (string par in Data.chans_results[id_chan].prochie_par.column_headers)
            //        {
            //            Param p = Parametrs.phys_pars[par];
            //            arguments.Add(par, p);
            //            string value = Data.chans_results[id_chan].prochie_par.par_value(par, id_row).value;
            //            arg_value.Add(p.short_name, Convert.ToDouble(value));
            //        }

            //        Vvod_func.par = Data.chans_obr[id_chan].sections[sec_id].pars.par_value(name, id_row);


            //        break;





            //}

            //Vvod_func.arguments = arguments;
            //Vvod_func.arg_value = arg_value;
            //Vvod_func.arg_i_value = arg_i_value;
            //Vvod_func.name_par = name;
            //exp_wind.Hide();
            //Window newin1 = new Vvod_func();
            //newin1.ShowDialog();
        }

    }
}
