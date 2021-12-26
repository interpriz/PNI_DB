using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace БД_НТИ
{
    class Results_of_modelling : Results_of_fiz_exp
    {
       

        public Results_of_modelling(ObservableCollection<num_rezh> rezh_num,
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

        public Results_of_modelling(bool DB)// заполнение из БД
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
        public override bool save_in_DB(int chan)
        {
            //bool f1 = this.rezh_par.check_empty_parametr();
            //bool f2 = this.prochie_par.check_empty_parametr();
            //bool f3 = true;
            ////проверка пустоты параметров
            //foreach (section sec in this.sections)
            //{
            //    if (sec.pars != null)
            //    {
            //        if (sec.pars.check_empty_parametr() == false)
            //        {
            //            f3 = false;
            //            break;
            //        }
            //    }
            //    else
            //    {
            //        if (sec.trav_pars.check_empty_parametr() == false || sec.coordinates.check_empty_parametr() == false)
            //        {
            //            f3 = false;
            //            break;
            //        }
            //    }
            //}


            //if (f1 && f2 && f3)
            //{
            //    NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            //    sqlconn.Open();

            //    NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {Data.current_realization} and \"Channel\" = {chan}", sqlconn);
            //    string Id_r_c = comm_id.ExecuteScalar().ToString();

            //    foreach (num_rezh rezh in this.rezh_num)
            //    {
            //        int row_ind = this.rezh_num.IndexOf(rezh); // индекс режима (индекс большой строки в таблице)
            //        if (rezh.age == "new")
            //        {
            //            //добавить в mode_cros_section запись c сечением 0 mode
            //            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Mode_cros_section\" (\"Id_R_C\", \"Id_mode\", id_cros_section) VALUES( {Id_r_c},{rezh.BD_num},0); ", sqlconn);
            //            //com_add.ExecuteNonQuery();
            //            insert_mode_cros_section(Id_r_c, rezh.BD_num, 0);
            //            rezh.age = "old";
            //            if (rezh.visual_num == "1")
            //            {
            //                //добавить параметр среда в таблицу parametrs_experiment
            //                //NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда'),3); ", sqlconn);
            //                //com_add1.ExecuteNonQuery();
            //                insert_parametrs_experiment(Id_r_c,"Среда",3);
            //            }

            //        }

            //        switch (this.sreda[row_ind].mode)
            //        {
            //            case "new":
            //                // добавить параметр среда в таблицу values_experiment
            //                //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_string) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 3 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда') and \"Id_R_C\" = {Id_r_c}),0,'{this.sreda[row_ind].DB_value()}'); ", sqlconn);

            //                //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
            //                //NpgsqlCommand com_add = new NpgsqlCommand($"main_block.insert_values_exp(1,{rezh.BD_num}, 0, {Id_r_c}, 3, 'Среда',0, Null ,'{this.sreda[row_ind].DB_value()}');", sqlconn);
            //                //com_add.ExecuteNonQuery();
            //                insert_values_exp(1, rezh.BD_num,0, Id_r_c,3, "Среда", 0, null, this.sreda[row_ind].DB_value());

            //                this.sreda[row_ind].mode = "old";
            //                break;
            //            case "update":
            //                // обновить параметр среда в таблице values_experiment
            //                //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_string = '{this.sreda[row_ind].DB_value()}',date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 3 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
            //                //com_add1.ExecuteNonQuery();
            //                update_values_exp(1, rezh.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());
            //                this.sreda[row_ind].mode = "old";
            //                break;
            //        }


            //        int id_par = 0;// номер параметра в списке заголовков(и не только)
            //        foreach (parametr rezh_par in this.rezh_par.table[row_ind].cols)
            //        {
            //            string name = this.rezh_par.column_headers[id_par];// название параметра
            //            switch (rezh_par.mode)
            //            {
            //                case "new":
            //                    if (row_ind == 0)
            //                    {
            //                        NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
            //                        string Id_pe = comm.ExecuteScalar().ToString();//Id#
            //                        if (Id_pe == "0")
            //                        {
            //                            // добавить параметр в таблицу parametr experiment
            //                            //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
            //                            //com_add2.ExecuteNonQuery();
            //                            insert_parametrs_experiment(Id_r_c, name, 1);
            //                        }
            //                    }
            //                    // добавить строку в таблицу values_experiment
            //                    //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,{rezh_par.DB_value()}); ", sqlconn);
                                
            //                    //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
            //                    //NpgsqlCommand com_add = new NpgsqlCommand($"main_block.insert_values_exp(1,{rezh.BD_num}, 0, {Id_r_c}, 1, '{name}',0, {rezh_par.DB_value()} ,null);", sqlconn);
            //                    //com_add.ExecuteNonQuery();

            //                    insert_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, rezh_par.DB_value(), null);
            //                    rezh_par.mode = "old";
            //                    break;

            //                case "update":
            //                    // обновить запись в таблице values_experiment
            //                    //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {rezh_par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
            //                    //com_add1.ExecuteNonQuery();
            //                    update_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, rezh_par.DB_value(), null);
            //                    rezh_par.mode = "old";
            //                    break;
            //            }
            //            id_par++;
            //        }

            //        id_par = 0;// номер параметра в списке заголовков(и не только)
            //        foreach (parametr proch_par in this.prochie_par.table[row_ind].cols)
            //        {
            //            string name = this.prochie_par.column_headers[id_par];// название параметра
            //            switch (proch_par.mode)
            //            {
            //                case "new":

            //                    if (row_ind == 0)
            //                    {
            //                        NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
            //                        string Id_pe = comm.ExecuteScalar().ToString();//Id#
            //                        if (Id_pe == "0")
            //                        {
            //                            // добавить параметр в таблицу parametr experiment
            //                            //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
            //                            //com_add2.ExecuteNonQuery();
            //                            insert_parametrs_experiment(Id_r_c, name, 1);
            //                        }
            //                    }
            //                    // добавить строку в таблицу values_experiment
            //                    //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,{proch_par.DB_value()}); ", sqlconn);
            //                    //com_add.ExecuteNonQuery();
            //                    insert_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, proch_par.DB_value(), null);
            //                    proch_par.mode = "old";
            //                    break;

            //                case "update":
            //                    // обновить запись в таблице values_experiment
            //                    //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {proch_par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
            //                    //com_add1.ExecuteNonQuery();
            //                    update_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, proch_par.DB_value(), null);
            //                    proch_par.mode = "old";
            //                    break;
            //            }
            //            id_par++;
            //        }

            //        int id_cros_section = 1; // индекс ceчения (1, 2, 3 ...)
            //        foreach (section sec in this.sections)
            //        {
            //            if (sec.age == "new")
            //            {
            //                // добавить запись в таблицу mode_cros_section
            //                //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Mode_cros_section\" (\"Id_R_C\", \"Id_mode\", id_cros_section) VALUES( {Id_r_c},{rezh.BD_num},{id_cros_section}); ", sqlconn);
            //                //com_add.ExecuteNonQuery();
            //                insert_mode_cros_section(Id_r_c, rezh.BD_num, id_cros_section);
            //                if (rezh.visual_num == this.rezh_num.Count.ToString())
            //                    sec.age = "old";
            //            }
            //            if (sec.pars != null)
            //            {
            //                id_par = 0;// номер параметра в списке заголовков(и не только)
            //                foreach (parametr par in sec.pars.table[row_ind].cols)
            //                {
            //                    string name = sec.pars.column_headers[id_par];// название параметра
            //                    switch (par.mode)
            //                    {
            //                        case "new":
            //                            if (row_ind == 0)
            //                            {
            //                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
            //                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
            //                                if (Id_pe == "0")
            //                                {
            //                                    // добавить параметр в таблицу parametr experiment
            //                                    //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
            //                                    //com_add2.ExecuteNonQuery();
            //                                    insert_parametrs_experiment(Id_r_c, name, 1);
            //                                }
            //                            }
            //                            // добавить строку в таблицу values_experiment
            //                            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = {id_cros_section} and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,{par.DB_value()}); ", sqlconn);
            //                            //com_add.ExecuteNonQuery();

            //                            insert_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, 0, par.DB_value(), null);
            //                            par.mode = "old";
            //                            break;

            //                        case "update":
            //                            // обновить запись в таблице values_experiment
            //                            //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = {id_cros_section} and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
            //                            //com_add1.ExecuteNonQuery();
            //                            update_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, 0, par.DB_value(), null);
            //                            par.mode = "old";
            //                            break;
            //                    }
            //                    id_par++;
            //                }
            //            }
            //            else
            //            {
            //                id_par = 0;// номер параметра в списке заголовков(и не только)
            //                foreach (ObservableCollection<parametr> col_par in sec.trav_pars.travers_table[row_ind].cols)
            //                {
            //                    string name = sec.trav_pars.column_headers[id_par]; // название параметра
            //                    int id_trav = 1;                                    // номер траверсрования (номер точки траверсиирования)

            //                    foreach (parametr par in col_par)
            //                    {
            //                        switch (par.mode)
            //                        {
            //                            case "new":
            //                                if (row_ind == 0 && par.id_traversing == 1)
            //                                {
            //                                    NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
            //                                    string Id_pe = comm.ExecuteScalar().ToString();//Id#
            //                                    if (Id_pe == "0")
            //                                    {
            //                                        // добавить параметр в таблицу parametr experiment
            //                                        //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
            //                                        //com_add2.ExecuteNonQuery();
            //                                        insert_parametrs_experiment(Id_r_c, name, 1);
            //                                    }
            //                                }
            //                                // добавить строку в таблицу values_experiment
            //                                //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = {id_cros_section} and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),{id_trav},{par.DB_value()}); ", sqlconn);
            //                                //com_add.ExecuteNonQuery();

            //                                insert_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
            //                                par.mode = "old";
            //                                break;

            //                            case "update":
            //                                // обновить запись в таблице values_experiment
            //                                //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = {id_cros_section} and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = {id_trav}; ", sqlconn);
            //                                //com_add1.ExecuteNonQuery();
            //                                update_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
            //                                par.mode = "old";
            //                                break;
            //                        }
            //                        id_trav++;
            //                    }
            //                    id_par++;
            //                }

            //                id_par = 0;// номер параметра в списке заголовков(и не только)
            //                foreach (ObservableCollection<parametr> col_par in sec.coordinates.travers_table[row_ind].cols)
            //                {
            //                    string name = sec.coordinates.column_headers[id_par]; // название параметра
            //                    int id_trav = 1;                                    // номер траверсрования (номер точки траверсиирования)

            //                    foreach (parametr par in col_par)
            //                    {
            //                        switch (par.mode)
            //                        {
            //                            case "new":
            //                                if (row_ind == 0 && par.id_traversing == 1)
            //                                {
            //                                    NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
            //                                    string Id_pe = comm.ExecuteScalar().ToString();//Id#
            //                                    if (Id_pe == "0")
            //                                    {
            //                                        // добавить параметр в таблицу parametr experiment
            //                                        //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
            //                                        //com_add2.ExecuteNonQuery();
            //                                        insert_parametrs_experiment(Id_r_c, name, 1);
            //                                    }
            //                                }
            //                                // добавить строку в таблицу values_experiment
            //                                //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = {id_cros_section} and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),{id_trav},{par.DB_value()}); ", sqlconn);
            //                                //com_add.ExecuteNonQuery();

            //                                insert_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
            //                                par.mode = "old";
            //                                break;

            //                            case "update":
            //                                // обновить запись в таблице values_experiment
            //                                //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = {id_cros_section} and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = {id_trav}; ", sqlconn);
            //                                //com_add1.ExecuteNonQuery();
            //                                update_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
            //                                par.mode = "old";
            //                                break;
            //                        }
            //                        id_trav++;
            //                    }
            //                    id_par++;
            //                }
            //            }
            //            id_cros_section++;
            //        }
            //    }

            //    sqlconn.Close();

            //}
            //else
            //{
            //    //MessageBoxResult result = MessageBox.Show(
            //    //           $"Не все поля заполнены!", "Caution", MessageBoxButton.OK);
            //    return false;
            //}

            return true;
        }
    }
}
