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
using Npgsql;

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Model_obrabotka.xaml
    /// </summary>
    public partial class Model_obrabotka : Page
    {
        //Experiment_add exp_wind = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
        //int id_chan;
        public Construct constr_model_obr = new Construct();
        string conn_str = User.Connection_string;

        int sech7 = 0;
        bool is_obr_be = true;

        ObservableCollection<Integr> integr_db = new ObservableCollection<Integr>();
        ObservableCollection<Rezh> rezh_db = new ObservableCollection<Rezh>();
        ObservableCollection<ObservableCollection<Phys>> phys_db = new ObservableCollection<ObservableCollection<Phys>>();
        public Model_obrabotka()
        {
            Parametrs.update_parametrs();
            InitializeComponent();

            Data.id_obj = "6";
            Data.current_mode = 1;
            Data.current_realization = "1";
            Data.current_channel = 1;

            Data.modeling_obrabotka = new Obrabotka_of_modeling(true);

            //Data.modeling_obrabotka.integr_par.column_headers;

            //Data.modeling_obrabotka.rezh_par.column_headers

            //Data.modeling_obrabotka.sections.count

            //Data.modeling_obrabotka.sections[0].pars.column_headers

            constr_model_obr.integr_obr = new ObservableCollection<Integr>();
            constr_model_obr.integr_all = new ObservableCollection<Integr>();
            constr_model_obr.rezh_obr_7 = new ObservableCollection<Rezh>();
            constr_model_obr.rezh_all_7 = new ObservableCollection<Rezh>();

            constr_model_obr.obr_params_sech = new ObservableCollection<Obr_parametrs_sech_7>();

            constr_model_obr.sechen = new ObservableCollection<string>();


            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            constr_model_obr.count_sech = Data.modeling_results.sections.Count.ToString();

            
            if (Data.modeling_obrabotka.sections.Count == 0)    //есть ли данные об обработке в базе (минимум 1 сечение дб, если данные есть)
            {
                is_obr_be = false;
            }
            
            if (is_obr_be)  //если в базе есть данные
            {
                for (int k = 0; k < Data.modeling_obrabotka.integr_par.column_headers.Count; k++)
                {
                    Integr integ_par = new Integr();
                    integ_par.integr = Data.modeling_obrabotka.integr_par.column_headers[k];
                    constr_model_obr.integr_obr.Add(integ_par);
                    integr_db.Add(integ_par);
                }

                for (int k = 0; k < Data.modeling_obrabotka.rezh_par.column_headers.Count; k++)
                {
                    Rezh str = new Rezh();
                    str.rezh = Data.modeling_obrabotka.rezh_par.column_headers[k];
                    constr_model_obr.rezh_obr_7.Add(str);
                    rezh_db.Add(str);
                }
            }
            
            
            NpgsqlCommand comm_integ_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса');", sqlconn);    //все интегральные параметры (пункт 7)
            NpgsqlDataReader rdr_integ_7 = comm_integ_7.ExecuteReader();
            Parametrs.integral_pars = new Dictionary<string, Param>();
            while (rdr_integ_7.Read())
            {
                bool fl = true;
                if (is_obr_be)
                {
                    for (int q = 0; q < constr_model_obr.integr_obr.Count; q++)
                    {
                        if (constr_model_obr.integr_obr[q].integr == rdr_integ_7[0].ToString())
                        {
                            fl = false;
                            q = constr_model_obr.integr_obr.Count;
                        }
                    }
                }

                if (fl)
                {
                    Integr str = new Integr();
                    str.integr = rdr_integ_7[0].ToString();
                    constr_model_obr.integr_all.Add(str);
                }
                //Param p = new Param() { short_name = rdr_integ_7[1].ToString(), unit = rdr_integ_7[2].ToString() };
                //Parametrs.integral_pars.Add(rdr_integ_7[0].ToString(), p);
            }
            rdr_integ_7.Close();

            

            NpgsqlCommand comm_rezh_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные параметры (пункт 7)
            NpgsqlDataReader rdr_rezh_7 = comm_rezh_7.ExecuteReader();
            while (rdr_rezh_7.Read())
            {
                bool fl = true;
                if (is_obr_be)
                {
                    for (int q = 0; q < constr_model_obr.rezh_obr_7.Count; q++)
                    {
                        if (constr_model_obr.rezh_obr_7[q].rezh == rdr_rezh_7[0].ToString())
                        {
                            fl = false;
                            q = constr_model_obr.rezh_obr_7.Count;
                        }
                    }
                }
                
                if (fl)
                {
                    Rezh str = new Rezh();
                    str.rezh = rdr_rezh_7[0].ToString();
                    constr_model_obr.rezh_all_7.Add(str);
                }

            }
            rdr_rezh_7.Close();

            for (int i = 0; i < Convert.ToInt32(constr_model_obr.count_sech); i++)
            {
                constr_model_obr.sechen.Add($"{i+1}");

                Obr_parametrs_sech_7 phys_pars = new Obr_parametrs_sech_7();
                phys_pars.phys_obr_7 = new ObservableCollection<Phys>();
                phys_pars.phys_all_7 = new ObservableCollection<Phys>();

                if (is_obr_be)
                {
                    for (int k = 0; k < Data.modeling_obrabotka.sections[i].pars.column_headers.Count; k++)
                    {
                        Phys par = new Phys();
                        par.phys = Data.modeling_obrabotka.integr_par.column_headers[k];
                        phys_pars.phys_obr_7.Add(par);
                    }
                    phys_db.Add(phys_pars.phys_obr_7);
                }

                NpgsqlCommand comm_phys = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);
                NpgsqlDataReader rdr_phys = comm_phys.ExecuteReader();
                while (rdr_phys.Read())
                {
                    if (rdr_phys[0].ToString() != "Среда")
                    {
                        bool fl = true;
                        if (is_obr_be)
                        {
                            for (int q = 0; q < phys_pars.phys_obr_7.Count; q++)
                            {
                                if (rdr_phys[0].ToString() == phys_pars.phys_obr_7[q].phys)
                                {
                                    fl = false;
                                    q = phys_pars.phys_obr_7.Count;
                                }
                            }
                        }
                           
                        if (fl)
                        {
                            Phys str = new Phys();
                            str.phys = rdr_phys[0].ToString();
                            phys_pars.phys_all_7.Add(str);
                        }
                    }

                }
                rdr_phys.Close();

                constr_model_obr.obr_params_sech.Add(phys_pars);
            }



            sqlconn.Close();

            Dialog_construct.DataContext = constr_model_obr;

            Dialog_construct.IsOpen = true;
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

        private void Dialog_construct_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {

        }

        private void combox_7_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (combox_7.SelectedItem != null)
            {
                string comitem = combox_7.SelectedItem.ToString();
                sech7 = Convert.ToInt32(comitem);
                grid_7_phys.DataContext = constr_model_obr.obr_params_sech[sech7 - 1];
            }
            else
            {
                grid_7_phys.DataContext = null;
            }
        }

        private void butt_phys_addnew_6_Click(object sender, RoutedEventArgs e)
        {
            dialog_add_param.Tag = "Теплофизические параметры";
            dialog_add_param.IsOpen = true;
        }

        private void butt_obr_phys_add_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_obr_phys_all_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_phys_all_7.SelectedItems.Count; i++)
                {
                    Phys phys = datagrid_obr_phys_all_7.SelectedItems[i] as Phys;
                    constr_model_obr.obr_params_sech[sech7 - 1].phys_all_7.Remove(phys);
                    constr_model_obr.obr_params_sech[sech7 - 1].phys_obr_7.Add(phys);
                }
            }
        }

        private void butt_obr_phys_del_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_obr_phys_sech_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_phys_sech_7.SelectedItems.Count; i++)
                {
                    Phys phys = datagrid_obr_phys_sech_7.SelectedItems[i] as Phys;
                    bool fl = true;
                    if (is_obr_be)
                    {
                        for (int j = 0; j < phys_db[sech7 - 1].Count; j++)
                        {
                            if (phys.phys == phys_db[sech7 - 1][j].phys)
                            {
                                fl = false;
                                j = phys_db[sech7 - 1].Count;
                            }
                        }
                    }

                    if (fl)
                    {
                        constr_model_obr.obr_params_sech[sech7 - 1].phys_all_7.Add(phys);
                        constr_model_obr.obr_params_sech[sech7 - 1].phys_obr_7.Remove(phys);
                    }
                }
            }
        }

        private void butt_rezh_addnew_6_Click(object sender, RoutedEventArgs e)
        {
            dialog_add_param.Tag = "Режимные параметры";
            dialog_add_param.IsOpen = true;
        }

        private void butt_obr_rezh_add_7_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_obr_rezh_all_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_rezh_all_7.SelectedItems.Count; i++)
                {
                    Rezh rezh = datagrid_obr_rezh_all_7.SelectedItems[i] as Rezh;
                    constr_model_obr.rezh_all_7.Remove(rezh);
                    constr_model_obr.rezh_obr_7.Add(rezh);
                }
            }
        }

        private void butt_obr_rezh_del_7_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_obr_rezh_obr_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_rezh_obr_7.SelectedItems.Count; i++)
                {
                    Rezh rezh = datagrid_obr_rezh_obr_7.SelectedItems[i] as Rezh;
                    bool fl = true;
                    if (is_obr_be)
                    {
                        for (int j = 0; j < rezh_db.Count; j++)
                        {
                            if (rezh.rezh == rezh_db[j].rezh)
                            {
                                fl = false;
                                j = rezh_db.Count;
                            }
                        }
                    }
                        
                    if (fl)
                    {
                        constr_model_obr.rezh_all_7.Add(rezh);
                        constr_model_obr.rezh_obr_7.Remove(rezh);
                    }

                }
            }
        }

        private void butt_obr_int_addnew_Click(object sender, RoutedEventArgs e)
        {
            dialog_add_param.Tag = "Интегральные характеристики физического процесса";
            dialog_add_param.IsOpen = true;
        }

        private void butt_obr_int_add_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_obr_int_all.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_int_all.SelectedItems.Count; i++)
                {
                    Integr integ = datagrid_obr_int_all.SelectedItems[i] as Integr;
                    constr_model_obr.integr_all.Remove(integ);
                    constr_model_obr.integr_obr.Add(integ);
                }
            }
        }

        private void butt_obr_int_del_Click(object sender, RoutedEventArgs e)
        {
            if (datagrid_obr_int_sech.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_int_sech.SelectedItems.Count; i++)
                {
                    Integr integ = datagrid_obr_int_sech.SelectedItems[i] as Integr;
                    bool fl = true;
                    if (is_obr_be)
                    {
                        for (int j = 0; j < integr_db.Count; j++)
                        {
                            if (integ.integr == integr_db[j].integr)
                            {
                                fl = false;
                                j = integr_db.Count;
                            }
                        }
                    }
                    
                    if (fl)
                    {
                        constr_model_obr.integr_all.Add(integ);
                        constr_model_obr.integr_obr.Remove(integ);
                    }

                }
            }
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

                    Parametrs.insert_parametr(dialog_add_param.Tag.ToString(), name, short_name, unit);
                    switch (dialog_add_param.Tag)
                    {
                        case "Теплофизические параметры":
                            Phys newparphys = new Phys();
                            newparphys.phys = txt_par_name.Text;

                            for (int i = 0; i < Data.constr.Count; i++)
                            {
                                constr_model_obr.obr_params_sech[i].phys_all_7.Add(newparphys);
                            }
                            break;

                        case "Режимные параметры":
                            Rezh newparrezh = new Rezh();
                            newparrezh.rezh = txt_par_name.Text;
                            constr_model_obr.rezh_all_7.Add(newparrezh);
                            break;

                        case "Интегральные характеристики физического процесса":
                            Integr newparint = new Integr();
                            newparint.integr = txt_par_name.Text;
                            constr_model_obr.integr_all.Add(newparint);
                            break;
                    }


                    dialog_add_param.IsOpen = false;
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

        private void messbut_dialog_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
            txt_par_name.IsEnabled = true;
            txt_par_symb.IsEnabled = true;
            txt_par_unit.IsEnabled = true;
        }
    }
}
