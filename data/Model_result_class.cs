﻿using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace БД_НТИ
{
    class Results_of_modeling : Results_of_fiz_exp
    {
        public Results_of_modeling(ObservableCollection<num_rezh> rezh_num,
            ObservableCollection<parametr> sreda,
            parametrs rezh_par,
            parametrs prochie_par,
            ObservableCollection<section> sections)
        {
            this.rezh_num = rezh_num;
            this.sreda = sreda;
            this.rezh_par = rezh_par;
            this.prochie_par = prochie_par;
            this.sections = sections;

            int max = 1;
            foreach (section sec in sections)
            {
                if (sec.pars == null)
                {
                    int num_rows = sec.coordinates.travers_table[0].cols[0].Count;
                    if (num_rows > max) max = num_rows;
                }
            }
            this.max_travers_points = max;
        }

        public Results_of_modeling(bool DB)// заполнение из БД
        {
            if (DB)
            {

                //this.id_chan = chan; ;

                String conn_str = User.Connection_string;       //строка подключения

                ObservableCollection<num_rezh> setting_num = new ObservableCollection<num_rezh>();

                ObservableCollection<parametr> sreda = new ObservableCollection<parametr>();

                parametrs rezh_par = new parametrs();

                parametrs prochie_par = new parametrs();

                ObservableCollection<section> sections = new ObservableCollection<section>();

                //Results_of_fiz_exp exp_results;

                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                //заполнение и создание таблиц каналов из базы данных
                NpgsqlCommand comm_main = new NpgsqlCommand($"select * from main_block.\"select_chan_results_modelling\"({Data.id_obj},{Data.current_mode},{Data.current_realization},{Data.current_channel},1);", sqlconn);
                NpgsqlDataReader reader_main = comm_main.ExecuteReader();
                while (reader_main.Read())
                {
                    int setting = (int)reader_main[0];               //номер настройки
                    int id_section = (int)reader_main[1];           //номер сечения
                    int id_traversing = (int)reader_main[2];        //номер траверсирования
                    int id_type = (int)reader_main[3];              //тип параметра (2-режимные 3-теплофизические 7,8-координаты)
                    string par_name = reader_main[4].ToString();    //название параметра
                    int id_data = Convert.ToInt32(reader_main[9]);    //тип данных (1 - значение, 3 - строка, 4 -функция)
                    //string value_str = reader_main[5].ToString();//строковое значение параметра (среда)
                    //string value = Convert.ToDouble(reader_main[6]).ToString();//убираем лишние нули

                    parametr par = new parametr();
                    par.mode = "old";
                    par.id_traversing = id_traversing;
                    switch (id_data)
                    {
                        //значение
                        case 1:
                            par.value = Convert.ToDouble(reader_main[8].ToString().Replace('.', ',')).ToString();
                            break;
                        //строка
                        case 3:
                            par.value = reader_main[7].ToString();  //строковое значение параметра (среда)
                            break;
                        //функция
                        case 4:
                            break;
                    }

                    if (setting_num.Count < setting)
                    {
                        num_rezh rezh = new num_rezh() { BD_num = setting, visual_num = Convert.ToString(setting_num.Count + 1), age = "old" };
                        setting_num.Add(rezh);
                    }
                    if (setting_num.Count == setting)
                    {
                        if (id_section == 0)
                        {
                            if (id_traversing == 0)
                            {
                                parametrs parametrs = new parametrs();
                                switch (id_type)
                                {
                                    case 2:
                                        parametrs = rezh_par;
                                        break;
                                    case 3:
                                        if (par_name == "Среда") sreda.Add(par);
                                        else
                                            parametrs = prochie_par;
                                        break;
                                }
                                if (parametrs.column_headers.IndexOf(par_name) == -1) parametrs.column_headers.Add(par_name);
                                if (parametrs.table.Count < setting_num.Count)
                                {
                                    ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols = cols;
                                    r.rezh = setting;
                                    r.age = "old";
                                    parametrs.table.Add(r);
                                }
                                else
                                {
                                    parametrs.table[setting_num.Count - 1].cols.Add(par);
                                }
                            }//нет траверсирования в 0ом сечении
                        }
                        else
                        {

                            if (id_traversing == 0)
                            {
                                if (sections.Count < id_section)
                                {
                                    section sec = new section(false, id_section, "old");
                                    sections.Add(sec);
                                }
                                if (sections[id_section - 1].pars.column_headers.IndexOf(par_name) == -1)
                                {
                                    sections[id_section - 1].pars.column_headers.Add(par_name);
                                }
                                if (sections[id_section - 1].pars.table.Count < setting_num.Count)
                                {
                                    //ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols.Add(par);
                                    r.rezh = setting;
                                    r.age = "old";
                                    sections[id_section - 1].pars.table.Add(r);
                                }
                                else
                                {
                                    sections[id_section - 1].pars.table[setting_num.Count - 1].cols.Add(par);
                                }
                            }
                            else
                            {
                                if (sections.Count < id_section)
                                {
                                    section sec = new section(true, id_section, "old");
                                    sections.Add(sec);
                                }
                                travers_parametrs tr_pr = new travers_parametrs();

                                switch (id_type)
                                {
                                    case 7:
                                        tr_pr = sections[id_section - 1].coordinates;
                                        break;
                                    case 3:
                                        tr_pr = sections[id_section - 1].trav_pars;
                                        break;
                                }
                                if (tr_pr.column_headers.IndexOf(par_name) == -1)
                                {
                                    tr_pr.column_headers.Add(par_name);
                                }
                                if (tr_pr.travers_table.Count < setting_num.Count)
                                {
                                    travers_row r = new travers_row();
                                    r.rezh = setting;
                                    r.age = "old";
                                    //ObservableCollection<parametr> col_par = new ObservableCollection<parametr>() { par};
                                    r.cols.Add(new ObservableCollection<parametr>() { par });
                                    tr_pr.travers_table.Add(r);
                                }
                                else
                                {
                                    int rezhims = tr_pr.travers_table.Count;//количество режимов на текущий момент
                                    int number_columns = tr_pr.travers_table[rezhims - 1].cols.Count;//количество столбцов в координатах
                                    int column_index = tr_pr.column_headers.IndexOf(par_name);//номер столбца параметра
                                    if (column_index + 1 > number_columns)
                                    {
                                        tr_pr.travers_table[rezhims - 1].cols.Add(new ObservableCollection<parametr>() { par });
                                    }
                                    else
                                    {
                                        tr_pr.travers_table[rezhims - 1].cols[column_index].Add(par);
                                    }
                                }
                            }
                        }
                    }
                    //else
                    //{

                    //}
                }
                reader_main.Close();
                sqlconn.Close();

                //exp_results = new Results_of_fiz_exp(rezh_num, sreda, rezh_par, prochie_par, sections);

                this.rezh_num = setting_num;
                this.sreda = sreda;
                this.rezh_par = rezh_par;
                this.prochie_par = prochie_par;
                this.sections = sections;

                int max = 1;
                foreach (section sec in sections)
                {
                    if (sec.pars == null)
                    {
                        int num_rows = sec.coordinates.travers_table[0].cols[0].Count;
                        if (num_rows > max) max = num_rows;
                    }
                }
                this.max_travers_points = max;

            }
            else
            {
                //this.id_chan = chan; ;

                String conn_str = User.Connection_string;       //строка подключения

                ObservableCollection<num_rezh> setting_numbers = new ObservableCollection<num_rezh>();

                ObservableCollection<parametr> sreda = new ObservableCollection<parametr>();

                parametrs rezh_par = new parametrs();

                parametrs prochie_par = new parametrs();

                ObservableCollection<section> sections = new ObservableCollection<section>();

                //Results_of_fiz_exp exp_results;

                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                //заполнение и создание таблиц каналов из базы данных
                NpgsqlCommand comm_main = new NpgsqlCommand($"select * from main_block.\"select_chan_mode_exp_results\"({Data.id_obj},{Data.current_realization},{Data.current_channel},1,1);", sqlconn);
                NpgsqlDataReader reader_main = comm_main.ExecuteReader();
                while (reader_main.Read())
                {
                    int setting_number = (int)reader_main[0];               //номер настройки
                    int id_section = (int)reader_main[1];           //номер сечения
                    int id_traversing = (int)reader_main[2];        //номер траверсирования
                    int id_type = (int)reader_main[3];              //тип параметра (2-режимные 3-теплофизические 7,8-координаты)
                    string par_name = reader_main[4].ToString();    //название параметра
                    int id_data = Convert.ToInt32(reader_main[9]);    //тип данных (1 - значение, 3 - строка, 4 -функция)
                    //string value_str = reader_main[5].ToString();//строковое значение параметра (среда)
                    //string value = Convert.ToDouble(reader_main[6]).ToString();//убираем лишние нули

                    parametr par = new parametr();
                    par.mode = "new";
                    par.id_traversing = id_traversing;
                    switch (id_data)
                    {
                        //значение
                        case 1:
                            par.value = "";
                            break;
                        //строка
                        case 3:
                            if (par_name == "Среда")
                                par.value = reader_main[7].ToString();  //строковое значение параметра (среда)
                            else
                                par.value = "";
                            break;
                        //функция
                        case 4:
                            break;
                    }

                    if (setting_numbers.Count < setting_number)
                    {
                        num_rezh sett = new num_rezh() { BD_num = setting_number, visual_num = Convert.ToString(setting_numbers.Count + 1), age = "new" };
                        setting_numbers.Add(sett);
                    }
                    if (setting_numbers.Count == setting_number)
                    {
                        if (id_section == 0)
                        {
                            if (id_traversing == 0)
                            {
                                parametrs parametrs = new parametrs();
                                switch (id_type)
                                {
                                    case 2:
                                        parametrs = rezh_par;
                                        break;
                                    case 3:
                                        if (par_name == "Среда") sreda.Add(par);
                                        else
                                            parametrs = prochie_par;
                                        break;
                                }
                                if (parametrs.column_headers.IndexOf(par_name) == -1) parametrs.column_headers.Add(par_name);
                                if (parametrs.table.Count < setting_numbers.Count)
                                {
                                    ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols = cols;
                                    r.rezh = setting_number;
                                    r.age = "new";
                                    parametrs.table.Add(r);
                                }
                                else
                                {
                                    parametrs.table[setting_numbers.Count - 1].cols.Add(par);
                                }
                            }//нет траверсирования в 0ом сечении
                        }
                        else
                        {

                            if (id_traversing == 0)
                            {
                                if (sections.Count < id_section)
                                {
                                    section sec = new section(false, id_section, "new");
                                    sections.Add(sec);
                                }
                                if (sections[id_section - 1].pars.column_headers.IndexOf(par_name) == -1)
                                {
                                    sections[id_section - 1].pars.column_headers.Add(par_name);
                                }
                                if (sections[id_section - 1].pars.table.Count < setting_numbers.Count)
                                {
                                    //ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols.Add(par);
                                    r.rezh = setting_number;
                                    r.age = "new";
                                    sections[id_section - 1].pars.table.Add(r);
                                }
                                else
                                {
                                    sections[id_section - 1].pars.table[setting_numbers.Count - 1].cols.Add(par);
                                }
                            }
                            else
                            {
                                if (sections.Count < id_section)
                                {
                                    section sec = new section(true, id_section, "new");
                                    sections.Add(sec);
                                }
                                travers_parametrs tr_pr = new travers_parametrs();

                                switch (id_type)
                                {
                                    case 7:
                                        tr_pr = sections[id_section - 1].coordinates;
                                        break;
                                    case 3:
                                        tr_pr = sections[id_section - 1].trav_pars;
                                        break;
                                }
                                if (tr_pr.column_headers.IndexOf(par_name) == -1)
                                {
                                    tr_pr.column_headers.Add(par_name);
                                }
                                if (tr_pr.travers_table.Count < setting_numbers.Count)
                                {
                                    travers_row r = new travers_row();
                                    r.rezh = setting_number;
                                    r.age = "new";
                                    //ObservableCollection<parametr> col_par = new ObservableCollection<parametr>() { par};
                                    r.cols.Add(new ObservableCollection<parametr>() { par });
                                    tr_pr.travers_table.Add(r);
                                }
                                else
                                {
                                    int rezhims = tr_pr.travers_table.Count;//количество режимов на текущий момент
                                    int number_columns = tr_pr.travers_table[rezhims - 1].cols.Count;//количество столбцов в координатах
                                    int column_index = tr_pr.column_headers.IndexOf(par_name);//номер столбца параметра
                                    if (column_index + 1 > number_columns)
                                    {
                                        tr_pr.travers_table[rezhims - 1].cols.Add(new ObservableCollection<parametr>() { par });
                                    }
                                    else
                                    {
                                        tr_pr.travers_table[rezhims - 1].cols[column_index].Add(par);
                                    }
                                }
                            }
                        }
                    }
                    //else
                    //{

                    //}
                }
                reader_main.Close();
                sqlconn.Close();

                //exp_results = new Results_of_fiz_exp(rezh_num, sreda, rezh_par, prochie_par, sections);

                this.rezh_num = setting_numbers;
                this.sreda = sreda;
                this.rezh_par = rezh_par;
                this.prochie_par = prochie_par;
                this.sections = sections;

                int max = 1;
                foreach (section sec in sections)
                {
                    if (sec.pars == null)
                    {
                        int num_rows = sec.coordinates.travers_table[0].cols[0].Count;
                        if (num_rows > max) max = num_rows;
                    }
                }
                this.max_travers_points = max;
            }
        }
        
        // сохранение данных структуры классов в базе данных
        public bool save_in_DB()
        {
            bool f1 = this.rezh_par.check_empty_parametr();
            bool f2 = this.prochie_par.check_empty_parametr();
            bool f3 = true;
            //проверка пустоты параметров
            foreach (section sec in this.sections)
            {
                if (sec.pars != null)
                {
                    if (sec.pars.check_empty_parametr() == false)
                    {
                        f3 = false;
                        break;
                    }
                }
                else
                {
                    if (sec.trav_pars.check_empty_parametr() == false || sec.coordinates.check_empty_parametr() == false)
                    {
                        f3 = false;
                        break;
                    }
                }
            }


            if (f1 && f2 && f3)
            {
                NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
                sqlconn.Open();

                string Id_r_c = Data.id_R_C;

                foreach (num_rezh setting in this.rezh_num)
                {
                    int row_ind = this.rezh_num.IndexOf(setting); // индекс настройки (индекс большой строки в таблице)
                    if (setting.age == "new")
                    {
                        //добавить в mode_setting_cros_section запись c сечением 0 mode
                        DB_proc_func.insert_setting_cros_section(setting.BD_num, 0);
                        //rezh.age = "old";
                        if (setting.visual_num == "1")
                        {
                            //добавить параметр среда в таблицу parametrs_modelling
                            DB_proc_func.insert_parametrs_modelling("Среда", 3);
                        }

                    }

                    switch (this.sreda[row_ind].mode)
                    {
                        case "new":
                            // добавить параметр среда в таблицу values_modelling

                            //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
                            DB_proc_func.insert_values_mod(1,Data.current_mode ,setting.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());

                            this.sreda[row_ind].mode = "old";
                            break;
                        case "update":
                            // обновить параметр среда в таблице values_modelling
                            DB_proc_func.update_values_mod(1, Data.current_mode, setting.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());
                            this.sreda[row_ind].mode = "old";
                            break;
                    }


                    int id_par = 0;// номер параметра в списке заголовков(и не только)
                    foreach (parametr rezh_par in this.rezh_par.table[row_ind].cols)
                    {
                        string name = this.rezh_par.column_headers[id_par];// название параметра
                        switch (rezh_par.mode)
                        {
                            case "new":
                                if (row_ind == 0)
                                {
                                    NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                    string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                    if (Id_pe == "0")
                                    {
                                        // добавить параметр в таблицу parametr modelling
                                        DB_proc_func.insert_parametrs_modelling(name, 1);
                                    }
                                }
                                // добавить строку в таблицу values_modelling
                                //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)

                                DB_proc_func.insert_values_mod(1, Data.current_mode, setting.BD_num, 0, Id_r_c, 1, name, 0, rezh_par.DB_value(), null);
                                rezh_par.mode = "old";
                                break;

                            case "update":
                                // обновить запись в таблице values_modelling
                                DB_proc_func.update_values_mod(1, Data.current_mode, setting.BD_num, 0, Id_r_c, 1, name, 0, rezh_par.DB_value(), null);
                                rezh_par.mode = "old";
                                break;
                        }
                        id_par++;
                    }

                    id_par = 0;// номер параметра в списке заголовков(и не только)
                    foreach (parametr proch_par in this.prochie_par.table[row_ind].cols)
                    {
                        string name = this.prochie_par.column_headers[id_par];// название параметра
                        switch (proch_par.mode)
                        {
                            case "new":

                                if (row_ind == 0)
                                {
                                    NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                    string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                    if (Id_pe == "0")
                                    {
                                        // добавить параметр в таблицу parametr modelling
                                        DB_proc_func.insert_parametrs_modelling(name, 1);
                                    }
                                }
                                // добавить строку в таблицу values_modelling
                                DB_proc_func.insert_values_mod(1, Data.current_mode, setting.BD_num, 0, Id_r_c, 1, name, 0, proch_par.DB_value(), null);
                                proch_par.mode = "old";
                                break;

                            case "update":
                                // обновить запись в таблице values_modelling
                                DB_proc_func.update_values_mod(1, Data.current_mode, setting.BD_num, 0, Id_r_c, 1, name, 0, proch_par.DB_value(), null);
                                proch_par.mode = "old";
                                break;
                        }
                        id_par++;
                    }

                    int id_cros_section = 1; // индекс ceчения (1, 2, 3 ...)
                    foreach (section sec in this.sections)
                    {
                        if (sec.age == "new" && setting.age == "new")
                        {
                            // добавить запись в таблицу setting_cros_section
                            DB_proc_func.insert_setting_cros_section(setting.BD_num, id_cros_section);
                            if (setting.visual_num == this.rezh_num.Count.ToString())
                            {
                                sec.age = "old";
                            }
                            if(id_cros_section == this.sections.Count)
                                setting.age = "old";

                        }
                        if (sec.pars != null)
                        {
                            id_par = 0;// номер параметра в списке заголовков(и не только)
                            foreach (parametr par in sec.pars.table[row_ind].cols)
                            {
                                string name = sec.pars.column_headers[id_par];// название параметра
                                switch (par.mode)
                                {
                                    case "new":
                                        if (row_ind == 0)
                                        {
                                            NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                            string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                            if (Id_pe == "0")
                                            {
                                                // добавить параметр в таблицу parametr modelling
                                                DB_proc_func.insert_parametrs_modelling(name, 1);
                                            }
                                        }
                                        // добавить строку в таблицу values_modelling

                                        DB_proc_func.insert_values_mod(1, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 1, name, 0, par.DB_value(), null);
                                        par.mode = "old";
                                        break;

                                    case "update":
                                        // обновить запись в таблице values_modelling
                                        DB_proc_func.update_values_mod(1, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 1, name, 0, par.DB_value(), null);
                                        par.mode = "old";
                                        break;
                                }
                                id_par++;
                            }
                        }
                        else
                        {
                            id_par = 0;// номер параметра в списке заголовков(и не только)
                            foreach (ObservableCollection<parametr> col_par in sec.trav_pars.travers_table[row_ind].cols)
                            {
                                string name = sec.trav_pars.column_headers[id_par]; // название параметра
                                int id_trav = 1;                                    // номер траверсрования (номер точки траверсиирования)

                                foreach (parametr par in col_par)
                                {
                                    switch (par.mode)
                                    {
                                        case "new":
                                            if (row_ind == 0 && par.id_traversing == 1)
                                            {
                                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                                if (Id_pe == "0")
                                                {
                                                    // добавить параметр в таблицу parametr modelling
                                                    DB_proc_func.insert_parametrs_modelling(name, 1);
                                                }
                                            }
                                            // добавить строку в таблицу values_modelling
                                            DB_proc_func.insert_values_mod(1, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
                                            par.mode = "old";
                                            break;

                                        case "update":
                                            // обновить запись в таблице values_modelling
                                            DB_proc_func.update_values_mod(1, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
                                            par.mode = "old";
                                            break;
                                    }
                                    id_trav++;
                                }
                                id_par++;
                            }

                            id_par = 0;// номер параметра в списке заголовков(и не только)
                            foreach (ObservableCollection<parametr> col_par in sec.coordinates.travers_table[row_ind].cols)
                            {
                                string name = sec.coordinates.column_headers[id_par]; // название параметра
                                int id_trav = 1;                                    // номер траверсрования (номер точки траверсиирования)

                                foreach (parametr par in col_par)
                                {
                                    switch (par.mode)
                                    {
                                        case "new":
                                            if (row_ind == 0 && par.id_traversing == 1)
                                            {
                                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                                if (Id_pe == "0")
                                                {
                                                    // добавить параметр в таблицу parametr modelling
                                                    DB_proc_func.insert_parametrs_modelling(name, 1);
                                                }
                                            }
                                            // добавить строку в таблицу values_modelling
                                            DB_proc_func.insert_values_mod(1, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
                                            par.mode = "old";
                                            break;

                                        case "update":
                                            // обновить запись в таблице values_modelling
                                            DB_proc_func.update_values_mod(1, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
                                            par.mode = "old";
                                            break;
                                    }
                                    id_trav++;
                                }
                                id_par++;
                            }
                        }
                        id_cros_section++;
                    }
                }

                sqlconn.Close();

            }
            else
            {
                //MessageBoxResult result = MessageBox.Show(
                //           $"Не все поля заполнены!", "Caution", MessageBoxButton.OK);
                return false;
            }

            return true;
        }
    }

    class Obrabotka_of_modeling : Obrabotka_of_fiz_exp
    {
       
        public Obrabotka_of_modeling(ObservableCollection<num_rezh> rezh_num,
            ObservableCollection<parametr> sreda,
            parametrs rezh_par,
            parametrs prochie_par,
            parametrs integr_par,
            ObservableCollection<section> sections)
        {
            this.rezh_num = rezh_num;
            this.sreda = sreda;
            this.rezh_par = rezh_par;
            this.sections = sections;
            this.integr_par = integr_par;
        }

        public Obrabotka_of_modeling(bool DB)// заполнение из БД
        {
            if (DB)
            {
                //this.id_chan = chan;

                String conn_str = User.Connection_string;       //строка подключения

                ObservableCollection<num_rezh> setting_num = new ObservableCollection<num_rezh>();

                ObservableCollection<parametr> sreda = new ObservableCollection<parametr>();

                parametrs rezh_par = new parametrs();

                parametrs integr_par = new parametrs();

                ObservableCollection<section> sections = new ObservableCollection<section>();

                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                //заполнение и создание таблиц каналов из базы данных
                NpgsqlCommand comm_main = new NpgsqlCommand($"select * from main_block.\"select_chan_results_modelling\"({Data.id_obj},{Data.current_mode},{Data.current_realization},{Data.current_channel},0);", sqlconn);
                NpgsqlDataReader reader_main = comm_main.ExecuteReader();
                while (reader_main.Read())
                {
                    int setting = (int)reader_main[0];           //номер настройки
                    int id_section = (int)reader_main[1];       //номер сечения
                    int id_traversing = (int)reader_main[2];    //номер траверсирования
                    int id_type = (int)reader_main[3];          // тип параметра (2-режимные 3-теплофизические 7,8-координаты)
                    string par_name = reader_main[4].ToString();//название параметра
                    int id_data = Convert.ToInt32(reader_main[9]);    //тип данных (1 - значение, 3 - строка, 4 -функция)
                                                                      //string value_str = reader_main[5].ToString();//строковое значение параметра (среда)
                                                                      //string value = Convert.ToDouble(reader_main[6]).ToString();//убираем лишние нули

                    parametr par = new parametr();
                    par.mode = "old";
                    par.id_traversing = id_traversing;
                    switch (id_data)
                    {
                        //значение
                        case 1:
                            par.value = Convert.ToDouble(reader_main[8].ToString().Replace('.', ',')).ToString();
                            break;
                        //строка
                        case 3:
                            par.value = reader_main[7].ToString();  //строковое значение параметра (среда)
                            break;
                        //функция
                        case 4:
                            par.function_value = reader_main[7].ToString();
                            // обработка функции

                            break;
                    }


                    if (setting_num.Count < setting)
                    {
                        num_rezh rezh = new num_rezh() { BD_num = setting, visual_num = Convert.ToString(setting_num.Count + 1), age = "old" };
                        setting_num.Add(rezh);
                    }
                    if (setting_num.Count == setting)
                    {
                        if (id_section == 0)
                        {
                            if (id_traversing == 0)
                            {
                                parametrs parametrs = new parametrs();
                                switch (id_type)
                                {
                                    case 2:
                                        parametrs = rezh_par;
                                        break;
                                    case 3:
                                        if (par_name == "Среда") sreda.Add(par);
                                        break;
                                    case 8:
                                        parametrs = integr_par;
                                        break;
                                }
                                if (parametrs.column_headers.IndexOf(par_name) == -1) parametrs.column_headers.Add(par_name);
                                if (parametrs.table.Count < setting_num.Count)
                                {
                                    ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols = cols;
                                    r.rezh = setting;
                                    r.age = "old";
                                    parametrs.table.Add(r);
                                }
                                else
                                {
                                    parametrs.table[setting_num.Count - 1].cols.Add(par);
                                }

                            }//нет траверсирования в 0ом сечении
                        }
                        else
                        {
                            if (id_traversing == 0)
                            {
                                if (sections.Count < id_section)
                                {
                                    section sec = new section(false, id_section, "old");
                                    sections.Add(sec);
                                }
                                if (sections[id_section - 1].pars.column_headers.IndexOf(par_name) == -1) sections[id_section - 1].pars.column_headers.Add(par_name);
                                if (sections[id_section - 1].pars.table.Count < setting_num.Count)
                                {
                                    //ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols.Add(par);
                                    r.rezh = setting;
                                    r.age = "old";
                                    sections[id_section - 1].pars.table.Add(r);
                                }
                                else
                                {
                                    sections[id_section - 1].pars.table[setting_num.Count - 1].cols.Add(par);
                                }
                            }
                        }
                    }
                    //else
                    //{

                    //}
                }
                reader_main.Close();
                sqlconn.Close();

                //exp_results = new Results_of_fiz_exp(rezh_num, sreda, rezh_par, prochie_par, sections);

                this.rezh_num = setting_num;
                this.sreda = sreda;
                this.rezh_par = rezh_par;
                this.integr_par = integr_par;
                this.sections = sections;
            }
        }

        public Obrabotka_of_modeling(Construct constr) // создание заполненноей структтуры классов по параметрам конструктора
        {
            ObservableCollection<num_rezh> rezh_num = new ObservableCollection<num_rezh>();
            num_rezh rezhim = new num_rezh() { BD_num = 1, visual_num = "1", age = "new" };
            rezh_num.Add(rezhim);

            ObservableCollection<parametr> sreda = new ObservableCollection<parametr>();
            parametr sreda1 = new parametr() { value = constr.sreda };
            sreda.Add(sreda1);

            parametrs rezh_par = new parametrs();
            ObservableCollection<string> rezh_headers = new ObservableCollection<string>();
            foreach (Rezh r6 in constr.rezh_obr_7)
            {
                rezh_headers.Add(r6.rezh);
            }
            rezh_par.column_headers = rezh_headers;
            rezh_par.add_row();

            parametrs integr_par = new parametrs();
            ObservableCollection<string> integr_headers = new ObservableCollection<string>();
            foreach (Integr p in constr.integr_obr)
            {
                integr_headers.Add(p.integr);
            }
            integr_par.column_headers = integr_headers;
            integr_par.add_row();

            ObservableCollection<section> sections = new ObservableCollection<section>();
            foreach (Obr_parametrs_sech_7 par in constr.obr_params_sech)
            {
                int i = constr.obr_params_sech.IndexOf(par); // индекс сечения
                int id_section = Convert.ToInt32(constr.sechen[i]);// номер сечения

                ObservableCollection<string> parametrs = new ObservableCollection<string>();
                foreach (Phys p in constr.obr_params_sech[i].phys_obr_7)
                {
                    parametrs.Add(p.phys);
                }

                sections.Add(new section(id_section, parametrs, 1, "new"));
            }

            this.rezh_num = rezh_num;
            this.sreda = sreda;
            this.rezh_par = rezh_par;
            this.integr_par = integr_par;
            this.sections = sections;

            for (int i = 1; i < Convert.ToInt32(constr.rezh_count); i++)
            {
                add_rezhim();
            }
        }

        public override void update(Construct constr) // обновление структуры классов по параметрам конструктора
        {
            foreach (parametr sred in this.sreda)
            {
                if (sred.value != constr.sreda)
                {
                    sred.mode = "update";
                    sred.value = constr.sreda;
                }
            }

            List<string> rezh_par = new List<string>();
            foreach (Rezh r6 in constr.rezh_obr_7)
            {
                if (this.rezh_par.column_headers.IndexOf(r6.rezh) == -1)
                {
                    this.rezh_par.add_parametr(r6.rezh);
                }
                rezh_par.Add(r6.rezh);
            }
            for (int i = 0; i < this.rezh_par.column_headers.Count; i++)
            {
                if (rezh_par.IndexOf(this.rezh_par.column_headers[i]) == -1)
                {
                    this.rezh_par.delete_parametr(this.rezh_par.column_headers[i]);
                }
            }

            List<string> integr_par = new List<string>();
            foreach (Integr p in constr.integr_obr)
            {
                if (this.integr_par.column_headers.IndexOf(p.integr) == -1)
                {
                    this.integr_par.add_parametr(p.integr);
                }
                integr_par.Add(p.integr);
            }
            for (int i = 0; i < this.integr_par.column_headers.Count; i++)
            {
                if (integr_par.IndexOf(this.integr_par.column_headers[i]) == -1)
                {
                    this.integr_par.delete_parametr(this.integr_par.column_headers[i]);
                }
            }



            int col_sech = this.sections.Count;
            int new_col_sech = Convert.ToInt32(constr.count_sech);
            // добавление\удаление новых сечений
            if (new_col_sech - col_sech > 0)
            {
                for (int i = col_sech; i < new_col_sech; i++)
                {
                    int id_section = Convert.ToInt32(constr.sechen[i]);
                    ObservableCollection<string> parametrs = new ObservableCollection<string>();
                    foreach (Phys p in constr.obr_params_sech[i].phys_obr_7)
                    {
                        parametrs.Add(p.phys);
                    }
                    this.sections.Add(new section(id_section, parametrs, this.rezh_num.Count, "new"));
                }
            }
            else
            {
                for (int i = 0; i < (new_col_sech - col_sech) * -1; i++)
                {
                    this.sections.RemoveAt(this.sections.Count - 1);
                }
            }

            // обновление старых сечений
            foreach (section sec in this.sections)
            {
                int sech_id = this.sections.IndexOf(sec);

                List<string> phiz_par = new List<string>();
                foreach (Phys p in constr.obr_params_sech[sech_id].phys_obr_7)
                {
                    if (this.sections[sech_id].pars.column_headers.IndexOf(p.phys) == -1)
                    {
                        this.sections[sech_id].pars.add_parametr(p.phys);
                    }
                    phiz_par.Add(p.phys);
                }
                for (int i = 0; i < this.sections[sech_id].pars.column_headers.Count; i++)
                {
                    if (phiz_par.IndexOf(this.sections[sech_id].pars.column_headers[i]) == -1)
                    {
                        this.sections[sech_id].pars.delete_parametr(this.sections[sech_id].pars.column_headers[i]);
                    }
                }

            }

            // добавление новых режимов
            int col_rezh = this.rezh_num.Count;
            int new_col_rezh = Convert.ToInt32(constr.rezh_count);
            if (new_col_rezh > col_rezh)
            {
                for (int i = 0; i < new_col_rezh - col_rezh; i++)
                {
                    this.add_rezhim();
                }
            }

        }

        //подсчет функций 
        public void func_to_value(int id_chan)
        {
            foreach (row r in this.rezh_par.table)
            {
                int id_row = this.rezh_par.table.IndexOf(r);
                foreach (parametr pr in r.cols)
                {
                    Dictionary<string, double> arg_value = new Dictionary<string, double>();
                    List<Dictionary<string, double>> arg_i_value = new List<Dictionary<string, double>>();// список словарей с параметрами(обозначениями) и их значениями для каждой точки траверсирования
                    //геометрические параметры
                    foreach (string par in Data.channels[id_chan].column_headers)
                    {
                        Param p = Parametrs.geom_pars[par];
                        string value = Data.channels[id_chan].par_value(par, Data.id_realization);
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }
                    //режимные параметры
                    foreach (string par in Data.modeling_results.rezh_par.column_headers)
                    {
                        Param p = Parametrs.reg_pars[par];
                        string value = Data.modeling_results.rezh_par.par_value(par, id_row).value;
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }
                    //прочие параметры
                    foreach (string par in Data.modeling_results.prochie_par.column_headers)
                    {
                        Param p = Parametrs.phys_pars[par];
                        string value = Data.modeling_results.prochie_par.par_value(par, id_row).value;
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }
                    pr.func_to_value(arg_value, arg_i_value);
                }
            }

            foreach (section sec in this.sections)
            {
                int sec_id = this.sections.IndexOf(sec);
                foreach (row r in sec.pars.table)
                {
                    int id_row = sec.pars.table.IndexOf(r);
                    foreach (parametr pr in r.cols)
                    {
                        Dictionary<string, double> arg_value = new Dictionary<string, double>();
                        List<Dictionary<string, double>> arg_i_value = new List<Dictionary<string, double>>();// список словарей с параметрами(обозначениями) и их значениями для каждой точки траверсирования


                        //параметры сечения
                        if (Data.modeling_results.sections[sec_id].pars == null)
                        {
                            foreach (string par in Data.modeling_results.sections[sec_id].coordinates.column_headers)
                            {
                                Param p = new Param() { short_name = par + "_i", unit = "" };
                                int travers_points_count = Data.modeling_results.sections[sec_id].coordinates.travers_table[id_row].cols[0].Count;

                                for (int i = 0; i < travers_points_count; i++)
                                {
                                    if (Data.modeling_results.sections[sec_id].coordinates.column_headers.IndexOf(par) == 0)
                                    {
                                        Dictionary<string, double> arg_j_value = new Dictionary<string, double>();
                                        arg_i_value.Add(arg_j_value);
                                    }
                                    string value = Data.modeling_results.sections[sec_id].coordinates.par_value(par, id_row, i).value;
                                    arg_i_value[i].Add(p.short_name, Convert.ToDouble(value));
                                }
                            }
                            foreach (string par in Data.modeling_results.sections[sec_id].trav_pars.column_headers)
                            {
                                Param p = Parametrs.phys_pars[par];
                                p.short_name += "_i";
                                int travers_points_count = Data.modeling_results.sections[sec_id].trav_pars.travers_table[id_row].cols[0].Count;

                                for (int i = 0; i < travers_points_count; i++)
                                {
                                    //if (Data.modeling_results.sections[sec_id].trav_pars.column_headers.IndexOf(par) == 0)
                                    //{
                                    //    Dictionary<string, double> arg_j_value = new Dictionary<string, double>();
                                    //    arg_i_value.Add(arg_j_value);
                                    //}
                                    string value = Data.modeling_results.sections[sec_id].trav_pars.par_value(par, id_row, i).value;
                                    arg_i_value[i].Add(p.short_name, Convert.ToDouble(value));
                                }
                            }
                        }
                        else
                        {
                            foreach (string par in Data.modeling_results.sections[sec_id].pars.column_headers)
                            {
                                Param p = Parametrs.phys_pars[par];
                                string value = Data.modeling_results.sections[sec_id].pars.par_value(par, id_row).value;
                                arg_value.Add(p.short_name + $"_sec{sec_id + 1}", Convert.ToDouble(value));
                            }
                        }
                        //геометрические параметры
                        foreach (string par in Data.channels[id_chan].column_headers)
                        {
                            Param p = Parametrs.geom_pars[par];

                            string value = Data.channels[id_chan].par_value(par, Data.id_realization);
                            arg_value.Add(p.short_name, Convert.ToDouble(value));
                        }
                        //режимные параметры
                        foreach (string par in Data.modeling_results.rezh_par.column_headers)
                        {
                            Param p = Parametrs.reg_pars[par];

                            string value = Data.modeling_results.rezh_par.par_value(par, id_row).value;
                            arg_value.Add(p.short_name, Convert.ToDouble(value));
                        }
                        //прочие параметры
                        foreach (string par in Data.modeling_results.prochie_par.column_headers)
                        {
                            Param p = Parametrs.phys_pars[par];
                            string value = Data.modeling_results.prochie_par.par_value(par, id_row).value;
                            arg_value.Add(p.short_name, Convert.ToDouble(value));
                        }


                        pr.func_to_value(arg_value, arg_i_value);
                    }
                }
            }

            foreach (row r in this.integr_par.table)
            {
                int id_row = this.integr_par.table.IndexOf(r);
                foreach (parametr pr in r.cols)
                {
                    Dictionary<string, double> arg_value = new Dictionary<string, double>();
                    List<Dictionary<string, double>> arg_i_value = new List<Dictionary<string, double>>();// список словарей с параметрами(обозначениями) и их значениями для каждой точки траверсирования

                    //геометрические параметры
                    foreach (string par in Data.channels[id_chan].column_headers)
                    {
                        Param p = Parametrs.geom_pars[par];
                        string value = Data.channels[id_chan].par_value(par, Data.id_realization);
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }
                    //режимные параметры
                    foreach (string par in Data.modeling_results.rezh_par.column_headers)
                    {
                        Param p = Parametrs.reg_pars[par];
                        string value = Data.modeling_results.rezh_par.par_value(par, id_row).value;
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }
                    foreach (string par in Data.modeling_obrabotka.rezh_par.column_headers)
                    {
                        Param p = Parametrs.reg_pars[par];
                        if (!arg_value.ContainsKey(p.short_name))
                        {
                            string value = Data.modeling_obrabotka.rezh_par.par_value(par, id_row).value;
                            if (value != "")
                            {
                                arg_value.Add(p.short_name, Convert.ToDouble(value));
                            }
                        }
                    }
                    //параметры обработки результатов всех сечений 
                    foreach (section sec in Data.modeling_obrabotka.sections)
                    {
                        int id = Data.modeling_obrabotka.sections.IndexOf(sec);
                        foreach (string par in sec.pars.column_headers)
                        {
                            Param p = Parametrs.phys_pars[par];

                            p.short_name += $"_sec{id + 1}";

                            string value = sec.pars.par_value(par, id_row).value;
                            if (value != "")
                            {
                                arg_value.Add(p.short_name, Convert.ToDouble(value));
                            }
                        }
                    }
                    //прочие параметры
                    foreach (string par in Data.modeling_results.prochie_par.column_headers)
                    {
                        Param p = Parametrs.phys_pars[par];
                        string value = Data.modeling_results.prochie_par.par_value(par, id_row).value;
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }


                    pr.func_to_value(arg_value, arg_i_value);
                }
            }

        }

        // сохранение данных структуры классов в базе данных
        public void save_in_DB()
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();


            string Id_r_c = Data.id_R_C;

            foreach (num_rezh setting in this.rezh_num)
            {
                int row_ind = this.rezh_num.IndexOf(setting); // индекс настройки (индекс большой строки в таблице)
                //if (rezh.age == "new")
                //{
                //    //добавить в mode_cros_section запись c сечением 0 mode
                //    //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Mode_cros_section\" (\"Id_R_C\", \"Id_mode\", id_cros_section) VALUES( {Id_r_c},{rezh.BD_num},0); ", sqlconn);
                //    //com_add.ExecuteNonQuery();
                //    rezh.age = "old";
                //    if (rezh.visual_num == "1")
                //    {
                //        //добавить параметр среда в таблицу parametrs_experiment
                //        NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда'),3); ", sqlconn);
                //        com_add1.ExecuteNonQuery();
                //    }
                //}

                switch (this.sreda[row_ind].mode)
                {
                    case "new":
                        // добавить параметр среда в таблицу values_modelling
                        DB_proc_func.insert_values_mod(0, Data.current_mode, setting.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());
                        this.sreda[row_ind].mode = "old";
                        break;
                    case "update":
                        // обновить параметр среда в таблице values_modelling
                        DB_proc_func.update_values_mod(0, Data.current_mode, setting.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());
                        this.sreda[row_ind].mode = "old";
                        break;
                }


                int id_par = 0;// номер параметра в списке заголовков(и не только)
                foreach (parametr rezh_par in this.rezh_par.table[row_ind].cols)
                {
                    string name = this.rezh_par.column_headers[id_par];// название параметра

                    switch (rezh_par.mode)
                    {
                        case "new":
                            if (row_ind == 0)
                            {
                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                if (Id_pe == "0")
                                {
                                    // добавить параметр в таблицу parametr modelling
                                    DB_proc_func.insert_parametrs_modelling(name, 4);
                                }
                            }
                            // добавить строку в таблицу values_modelling
                            DB_proc_func.insert_values_mod(0, Data.current_mode, setting.BD_num, 0, Id_r_c, 4, name, 0, rezh_par.DB_value(), rezh_par.function_value);
                            rezh_par.mode = "old";
                            break;

                        case "update":
                            // обновить запись в таблице values_modelling
                            DB_proc_func.update_values_mod(0, Data.current_mode, setting.BD_num, 0, Id_r_c, 4, name, 0, rezh_par.DB_value(), rezh_par.function_value);
                            rezh_par.mode = "old";
                            break;
                    }
                    id_par++;
                }

                id_par = 0;// номер параметра в списке заголовков(и не только)
                foreach (parametr integr_par in this.integr_par.table[row_ind].cols)
                {
                    string name = this.integr_par.column_headers[id_par];// название параметра

                    switch (integr_par.mode)
                    {
                        case "new":
                            if (row_ind == 0)
                            {
                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                if (Id_pe == "0")
                                {
                                    // добавить параметр в таблицу parametr modelling
                                    DB_proc_func.insert_parametrs_modelling(name, 4);
                                }
                            }
                            // добавить строку в таблицу values_modelling
                            DB_proc_func.insert_values_mod(0, Data.current_mode, setting.BD_num, 0, Id_r_c, 4, name, 0, integr_par.DB_value(), integr_par.function_value);
                            integr_par.mode = "old";
                            break;

                        case "update":
                            // обновить запись в таблице values_modelling
                            DB_proc_func.update_values_mod(0, Data.current_mode, setting.BD_num, 0, Id_r_c, 4, name, 0, integr_par.DB_value(), integr_par.function_value);
                            integr_par.mode = "old";
                            break;
                    }
                    id_par++;
                }

                int id_cros_section = 1; // индекс ceчения (1, 2, 3 ...)
                foreach (section sec in this.sections)
                {
                    if (sec.age == "new")
                    {
                        if (setting.visual_num == this.rezh_num.Count.ToString())
                            sec.age = "old";
                    }
                    if (sec.pars != null)
                    {
                        id_par = 0;// номер параметра в списке заголовков(и не только)
                        foreach (parametr par in sec.pars.table[row_ind].cols)
                        {
                            string name = sec.pars.column_headers[id_par];// название параметра
                            switch (par.mode)
                            {
                                case "new":
                                    if (row_ind == 0)
                                    {
                                        NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_modelling\" where \"Id_R_C\" ={Id_r_c} and Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                        string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                        if (Id_pe == "0")
                                        {
                                            // добавить параметр в таблицу parametr modelling
                                            DB_proc_func.insert_parametrs_modelling(name, 4);
                                        }
                                    }
                                    // добавить строку в таблицу values_modelling
                                    DB_proc_func.insert_values_mod(0, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 4, name, 0, par.DB_value(), par.function_value);
                                    par.mode = "old";
                                    break;

                                case "update":
                                    // обновить запись в таблице values_modelling
                                    DB_proc_func.update_values_mod(0, Data.current_mode, setting.BD_num, id_cros_section, Id_r_c, 4, name, 0, par.DB_value(), par.function_value);
                                    par.mode = "old";
                                    break;
                            }
                            id_par++;
                        }
                    }
                    id_cros_section++;
                }
            }
            sqlconn.Close();
        }
    }
}
