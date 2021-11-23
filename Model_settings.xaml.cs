﻿using Npgsql;
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
    public partial class Model_settings : Page
    {
        public static MaterialDesignThemes.Wpf.DialogHost dialog = new MaterialDesignThemes.Wpf.DialogHost();

        String conn_str = User.Connection_string;       //строка подключения

        ObservableCollection<string> cmb_reshatel_parametrs = new ObservableCollection<string>();   // все параметры решателя в комбобоксе
        ObservableCollection<string> cmb_setka_parametrs = new ObservableCollection<string>();      // все параметры сетки в комбобоксе

        parametrs reshatel_pars = new parametrs();
        parametrs setka_pars = new parametrs();



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


            //NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {Data.current_realization} and \"Channel\" = {chan + 1}", sqlconn);
            //string Id_r_c = comm_id.ExecuteScalar().ToString();
            //string Id_r_c = "1";

            NpgsqlCommand comm_main = new NpgsqlCommand($"select* from main_block.select_settings_values(1); ", sqlconn);
            NpgsqlDataReader reader_main = comm_main.ExecuteReader();
            while (reader_main.Read())
            {
                string par_name = reader_main[0].ToString();
                string[] par_values_string = reader_main[1].ToString().Split(',');
                string[] par_values_number = reader_main[2].ToString().Split(',');
                int id_type = Convert.ToInt32(reader_main[3]);

                switch (id_type)
                {
                    //настройки решателя
                    case 5:
                        if(reader_main[1].ToString()!="")
                        reshatel_pars.add_parametr(par_name, par_values_string);
                        else
                        reshatel_pars.add_parametr(par_name, par_values_number);
                        break;
                    
                    //настройки сетки
                    case 4:
                        if (reader_main[1].ToString() != "")
                            setka_pars.add_parametr(par_name, par_values_string);
                        else
                            setka_pars.add_parametr(par_name, par_values_number);
                        break;
                }
            }



            sqlconn.Close();


            
           
            //reshatel.ItemsSource = rows;


            //DataGridComboBoxColumn col = new DataGridComboBoxColumn();

            
            




        }


        void parametrs_table_build(DataGrid gr, parametrs par)
        {
            gr.ItemsSource = par.table;
            //gr.CellEditEnding += datagrid2_CellEditEnding;
            //gr.KeyDown += TxtBox_chan_KeyDown;
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

                //col.Header = "test";
                //col.TextBinding = new Binding("Str") { Mode = BindingMode.TwoWay };
                //col.ItemsSource = test.lst;
                //var style = new Style();
                //style.Setters.Add(new Setter(ComboBox.IsEditableProperty, true));
                //col.EditingElementStyle = style;
                //reshatel.Columns.Add(col);
            }
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
           
        }

        private void Butt_OK_Click(object sender, RoutedEventArgs e)// занесение нового параметра в таблицу параметров в базе
        {
            //if (txt_par_name.Text == "" || txt_par_symb.Text == "" || txt_par_unit.Text == "")
            //{
            //    messtxt.Text = "Не все поля заполнены!";
            //    messbar.IsActive = true;
            //    txt_par_name.IsEnabled = false;
            //    txt_par_symb.IsEnabled = false;
            //    txt_par_unit.IsEnabled = false;
            //}
            //else
            //{
            //    string name = txt_par_name.Text;
            //    string short_name = txt_par_symb.Text;
            //    string unit = txt_par_unit.Text;
            //    Parametrs.update_parametrs();
            //    string check = Parametrs.check_new_param(name, short_name);
            //    if (check == "")
            //    {
            //        Parametrs.insert_parametr("Геометрические параметры", name, short_name, unit);
            //        dialog.IsOpen = false;
            //        switch (page_mode)
            //        {
            //            case "Experiment":
            //                exp_wind.Butt_back.IsEnabled = true;
            //                exp_wind.listview_items.IsEnabled = true;
            //                break;

            //            case "Modeling":
            //                model_wind.Butt_back.IsEnabled = true;
            //                model_wind.listview_items.IsEnabled = true;
            //                break;
            //        }
            //        cmb_parametrs.Add(txt_par_name.Text);
            //    }
            //    else
            //    {
            //        if (check == name)
            //        {
            //            messtxt.Text = $"Параметр \"{name}\" уже есть в базе!";
            //            messbar.IsActive = true;
            //        }
            //        else
            //        {
            //            messtxt.Text = $"Обозначение \"{short_name}\" уже есть в базе!";
            //            messbar.IsActive = true;
            //        }
            //        txt_par_name.IsEnabled = false;
            //        txt_par_symb.IsEnabled = false;
            //        txt_par_unit.IsEnabled = false;
            //    }

            //}


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
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)// добавление результатов экспериментов/моделирования
        {
            
        }

        private void Dialog_add_param_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
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

        private void messbut2_Click(object sender, RoutedEventArgs e)
        {
            messbar2.IsActive = false;
        }

        private void cmb_reshatel_params_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (cmb_reshatel_params.SelectedItem != null)
            {
                string select = cmb_reshatel_params.SelectedItem.ToString();
                cmb_reshatel_params.SelectedItem = null;
                if (tog_reshatel.IsChecked == false)//добавить столбец
                {
                    if (select == "Новый параметр")
                    {
                        dialog.IsOpen = true;
                        Modeling_add model_wind = (Modeling_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault();
                        model_wind.Butt_back.IsEnabled = false;
                        model_wind.listview_items.IsEnabled = false;

                    }
                    else
                    {

                        //if (table.Count != 0)
                        //{
                        //    if (headers.Count != 0)
                        //    {
                        //        foreach (str s in table)
                        //        {
                        //            s.cols.Add(new List<string> { "", "new" });//изменено для возможности удаления и добавления
                        //        }
                        //    }
                        //}
                        //if (headers.IndexOf(select) == -1)
                        //{
                        //    headers.Add(select);
                        //    DataTableExpGeom.AddHeader(gr, select);
                        //}
                    }
                }
                else//удалить столбец
                {
                    ////gr.ItemsSource = null;
                    //int i = headers.IndexOf(select);
                    //headers.RemoveAt(i);
                    ////cmb.ItemsSource = null;
                    ////cmb.ItemsSource = headers;
                    //foreach (str s in table)
                    //{
                    //    s.cols.RemoveAt(i);
                    //}
                    //gr.Columns.RemoveAt(i);
                    ////gr.ItemsSource = table;
                }
            }
        }
    }
}