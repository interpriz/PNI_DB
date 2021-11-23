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

namespace БД_НТИ
{



    class Results_of_fiz_exp : DB_proc_func
    {
        public ObservableCollection<num_rezh> rezh_num { get; set; }// список номеров режимов
        public ObservableCollection<parametr> sreda { get; set; }   // список значений параметра среда
        public parametrs rezh_par { get; set; }                     // режимные параметры
        public parametrs prochie_par { get; set; }                  // прочие параметры
        public ObservableCollection<section> sections { get; set; } // список сечений
        public int max_travers_points { get; set; }                 // максимальное количество точек траверсирования

        public int id_chan { get; set; }                            // номер канала

        public Results_of_fiz_exp(ObservableCollection<num_rezh> rezh_num,
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

        public Results_of_fiz_exp(bool DB, int chan)// заполнение из БД
        {
            if (DB)
            {

                this.id_chan = chan; ;

                String conn_str = User.Connection_string;       //строка подключения

                ObservableCollection<num_rezh> rezh_num = new ObservableCollection<num_rezh>();

                ObservableCollection<parametr> sreda = new ObservableCollection<parametr>();

                parametrs rezh_par = new parametrs();

                parametrs prochie_par = new parametrs();

                ObservableCollection<section> sections = new ObservableCollection<section>();

                //Results_of_fiz_exp exp_results;

                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                //заполнение и создание таблиц каналов из базы данных
                NpgsqlCommand comm_main = new NpgsqlCommand($"select * from main_block.\"select_chan_results\"({Data.id_obj},{Data.current_realization},{this.id_chan},1);", sqlconn);
                NpgsqlDataReader reader_main = comm_main.ExecuteReader();
                while (reader_main.Read())
                {
                    int rezhim = (int)reader_main[0];               //номер режима
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

                    if (rezh_num.Count < rezhim)
                    {
                        num_rezh rezh = new num_rezh() { BD_num = rezhim, visual_num = Convert.ToString(rezh_num.Count + 1), age = "old" };
                        rezh_num.Add(rezh);
                    }
                    if (rezh_num.Count == rezhim)
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
                                if (parametrs.table.Count < rezh_num.Count)
                                {
                                    ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols = cols;
                                    r.rezh = rezhim;
                                    r.age = "old";
                                    parametrs.table.Add(r);
                                }
                                else
                                {
                                    parametrs.table[rezh_num.Count - 1].cols.Add(par);
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
                                if (sections[id_section - 1].pars.table.Count < rezh_num.Count)
                                {
                                    //ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols.Add(par);
                                    r.rezh = rezhim;
                                    r.age = "old";
                                    sections[id_section - 1].pars.table.Add(r);
                                }
                                else
                                {
                                    sections[id_section - 1].pars.table[rezh_num.Count - 1].cols.Add(par);
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
                                if (tr_pr.travers_table.Count < rezh_num.Count)
                                {
                                    travers_row r = new travers_row();
                                    r.rezh = rezhim;
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
        }

        public Results_of_fiz_exp(Construct constr, int chan) // создание заполненноей структуры классов по параметрам конструктора
        {

            this.id_chan = chan;

            ObservableCollection<num_rezh> rezh_num = new ObservableCollection<num_rezh>();
            num_rezh rezhim = new num_rezh() { BD_num = 1, visual_num = "1", age = "new" };
            rezh_num.Add(rezhim);

            ObservableCollection<parametr> sreda = new ObservableCollection<parametr>();
            parametr sreda1 = new parametr() { value = constr.sreda };
            sreda.Add(sreda1);

            parametrs rezh_par = new parametrs();
            ObservableCollection<string> rezh_headers = new ObservableCollection<string>();
            foreach (Rezh r6 in constr.rezh_izm_6)
            {
                rezh_headers.Add(r6.rezh);
            }
            rezh_par.column_headers = rezh_headers;
            rezh_par.add_row();

            parametrs prochie_par = new parametrs();
            ObservableCollection<string> prochie_headers = new ObservableCollection<string>();
            foreach (Proch p in constr.proch_izm)
            {
                prochie_headers.Add(p.proch);
            }
            prochie_par.column_headers = prochie_headers;
            prochie_par.add_row();

            ObservableCollection<section> sections = new ObservableCollection<section>();
            foreach (Traversir tr in constr.travers)
            {
                int i = constr.travers.IndexOf(tr);// номер сечения
                if (tr.sys_coord != null)
                {
                    int id_section = Convert.ToInt32(constr.sechen[i]);

                    ObservableCollection<string> coord = new ObservableCollection<string>();
                    if (tr.sys_coord == "Декартова")
                    {
                        coord.Add("X"); coord.Add("Y");
                    }
                    else
                    {
                        coord.Add("a"); coord.Add("R");
                    }

                    ObservableCollection<string> parametrs = new ObservableCollection<string>();
                    foreach (Phys p in constr.params_sech[i].phys_izm_6)
                    {
                        parametrs.Add(p.phys);
                    }

                    int travers_points_count = Convert.ToInt32(tr.count);

                    sections.Add(new section(id_section, coord, parametrs, travers_points_count, 1, "new"));
                }
                else
                {
                    int id_section = Convert.ToInt32(constr.sechen[i]);

                    ObservableCollection<string> parametrs = new ObservableCollection<string>();
                    foreach (Phys p in constr.params_sech[i].phys_izm_6)
                    {
                        parametrs.Add(p.phys);
                    }

                    sections.Add(new section(id_section, parametrs, 1, "new"));
                }
            }

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

            for (int i = 1; i < Convert.ToInt32(constr.rezh_count); i++)
            {
                add_rezhim();
            }

        }

        public bool update(Construct constr) // обновление структуры классов по параметрам конструктора (если изменения произошли, то вернет true иначе false)
        {
            bool change = false;

            foreach (parametr sred in this.sreda)
            {
                if (sred.value != constr.sreda)
                {
                    sred.mode = "update";
                    sred.value = constr.sreda;
                    change = true;
                }
            }

            //добавление/удаление столбцов (параметров)
            List<string> rezh_par = new List<string>();
            foreach (Rezh r6 in constr.rezh_izm_6)
            {
                if (this.rezh_par.column_headers.IndexOf(r6.rezh) == -1)
                {
                    this.rezh_par.add_parametr(r6.rezh);
                    change = true;
                }
                rezh_par.Add(r6.rezh);
            }
            for (int i = 0; i < this.rezh_par.column_headers.Count; i++)
            {
                if (rezh_par.IndexOf(this.rezh_par.column_headers[i]) == -1)
                {
                    this.rezh_par.delete_parametr(this.rezh_par.column_headers[i]);
                    change = true;
                }
            }


            List<string> prochie_par = new List<string>();
            foreach (Proch p in constr.proch_izm)
            {
                if (this.prochie_par.column_headers.IndexOf(p.proch) == -1)
                {
                    this.prochie_par.add_parametr(p.proch);
                    change = true;
                }
                prochie_par.Add(p.proch);
            }
            for (int i = 0; i < this.prochie_par.column_headers.Count; i++)
            {
                if (prochie_par.IndexOf(this.prochie_par.column_headers[i]) == -1)
                {
                    this.prochie_par.delete_parametr(this.prochie_par.column_headers[i]);
                    change = true;
                }
            }
            //------------------------------------------------

            int col_sech = this.sections.Count;
            int new_col_sech = Convert.ToInt32(constr.count_sech);
            // добавление\удаление новых сечений
            if (new_col_sech - col_sech > 0)
            {
                change = true;
                for (int i = col_sech; i < new_col_sech; i++)
                {
                    int id_section = Convert.ToInt32(constr.sechen[i]);
                    if (constr.travers[i].sys_coord == null)
                    {
                        ObservableCollection<string> parametrs = new ObservableCollection<string>();
                        foreach (Phys p in constr.params_sech[i].phys_izm_6)
                        {
                            parametrs.Add(p.phys);
                        }
                        this.sections.Add(new section(id_section, parametrs, this.rezh_num.Count, "new"));
                    }
                    else
                    {
                        ObservableCollection<string> coord = new ObservableCollection<string>();
                        if (constr.travers[i].sys_coord == "Декартова")
                        {
                            coord.Add("X"); coord.Add("Y");
                        }
                        else
                        {
                            coord.Add("a"); coord.Add("R");
                        }

                        ObservableCollection<string> parametrs = new ObservableCollection<string>();
                        foreach (Phys p in constr.params_sech[i].phys_izm_6)
                        {
                            parametrs.Add(p.phys);
                        }

                        int travers_points_count = Convert.ToInt32(constr.travers[i].count);

                        this.sections.Add(new section(id_section, coord, parametrs, travers_points_count, this.rezh_num.Count, "new"));
                    }
                }
            }
            else
            {
                for (int i = 0; i < (new_col_sech - col_sech) * -1; i++)
                {
                    change = true;
                    this.sections.RemoveAt(this.sections.Count - 1);
                }
            }

            // обновление старых сечений
            foreach (section sec in this.sections)
            {
                int sech_id = this.sections.IndexOf(sec);
                bool f1 = (constr.travers[sech_id].sys_coord == null && sec.coordinates == null);
                bool f2 = (constr.travers[sech_id].sys_coord != null && sec.coordinates != null);
                if (f1 || f2)// если тип сечения не поменялся(с траверсированием или без)
                {
                    if (constr.travers[sech_id].sys_coord == null)
                    {
                        List<string> phiz_par = new List<string>();
                        foreach (Phys p in constr.params_sech[sech_id].phys_izm_6)
                        {
                            if (this.sections[sech_id].pars.column_headers.IndexOf(p.phys) == -1)
                            {
                                this.sections[sech_id].pars.add_parametr(p.phys);
                                change = true;
                            }
                            phiz_par.Add(p.phys);
                        }
                        for (int i = 0; i < this.sections[sech_id].pars.column_headers.Count; i++)
                        {
                            if (phiz_par.IndexOf(this.sections[sech_id].pars.column_headers[i]) == -1)
                            {
                                this.sections[sech_id].pars.delete_parametr(this.sections[sech_id].pars.column_headers[i]);
                                change = true;
                            }
                        }
                    }
                    else
                    {
                        List<string> phiz_par = new List<string>();
                        foreach (Phys p in constr.params_sech[sech_id].phys_izm_6)
                        {
                            if (this.sections[sech_id].trav_pars.column_headers.IndexOf(p.phys) == -1)
                            {
                                this.sections[sech_id].trav_pars.add_col_into_travers_row(p.phys);
                                change = true;
                            }
                            phiz_par.Add(p.phys);
                        }
                        for (int i = 0; i < this.sections[sech_id].trav_pars.column_headers.Count; i++)
                        {
                            if (phiz_par.IndexOf(this.sections[sech_id].trav_pars.column_headers[i]) == -1)
                            {
                                this.sections[sech_id].trav_pars.delete_col_from_travers_row(this.sections[sech_id].trav_pars.column_headers[i]);
                                change = true;
                            }
                        }

                        // обновление количества точек траверсирования
                        int col_p = sec.coordinates.travers_table[0].cols[0].Count;//старое количество точек траверсирования
                        int new_col_p = Convert.ToInt32(constr.travers[sech_id].count);
                        if (col_p < new_col_p)
                        {
                            change = true;
                            for (int i = 0; i < new_col_p - col_p; i++)
                            {
                                sec.add_travers_point();
                            }
                        }
                        else
                        {
                            for (int i = 0; i < col_p- new_col_p; i++)
                            {
                                sec.delete_travers_point();
                                change = true;
                            }
                        }

                        //обновление системы координат
                        ObservableCollection<string> coord = new ObservableCollection<string>();
                        bool constr_decart = constr.travers[sech_id].sys_coord == "Декартова";
                        bool cur_coord_decart = sec.coordinates.column_headers[0] == "X" || sec.coordinates.column_headers[0] == "Y";

                        if (constr_decart && !cur_coord_decart)
                        {
                            coord.Add("X"); coord.Add("Y");
                            sec.coordinates.column_headers = coord;
                            change = true;
                        }

                        if (!constr_decart && cur_coord_decart)
                        {
                            coord.Add("a"); coord.Add("R");
                            sec.coordinates.column_headers = coord;
                            change = true;
                        }
                    }
                }
                else // если тип сечения поменялся(с траверсированием или без)
                {
                    change = true;
                    if (constr.travers[sech_id].sys_coord == null)
                    {
                        sec.coordinates = null;
                        sec.trav_pars = null;
                        ObservableCollection<string> parametrs = new ObservableCollection<string>();
                        foreach (Phys p in constr.params_sech[sech_id].phys_izm_6)
                        {
                            parametrs.Add(p.phys);
                        }
                        sec.pars = new parametrs() { column_headers = parametrs };
                        for (int i = 0; i < this.rezh_num.Count; i++)
                        {
                            sec.pars.add_row();
                        }
                    }
                    else
                    {
                        sec.pars = null;
                        ObservableCollection<string> coord = new ObservableCollection<string>();
                        if (constr.travers[sech_id].sys_coord == "Декартова")
                        {
                            coord.Add("X"); coord.Add("Y");
                        }
                        else
                        {
                            coord.Add("a"); coord.Add("R");
                        }

                        ObservableCollection<string> parametrs = new ObservableCollection<string>();
                        foreach (Phys p in constr.params_sech[sech_id].phys_izm_6)
                        {
                            parametrs.Add(p.phys);
                        }

                        int travers_points_count = Convert.ToInt32(constr.travers[sech_id].count);


                        sec.coordinates = new travers_parametrs() { column_headers = coord };
                        sec.trav_pars = new travers_parametrs() { column_headers = parametrs };
                        for (int i = 0; i < this.rezh_num.Count; i++)
                        {
                            sec.coordinates.add_travers_row(travers_points_count);
                            sec.trav_pars.add_travers_row(travers_points_count);
                        }
                    }
                }
            }

            


            // добавление/удаление новых режимов
            int col_rezh = this.rezh_num.Count;
            int new_col_rezh = Convert.ToInt32(constr.rezh_count);
            if (new_col_rezh > col_rezh)
            {
                change = true;
                for (int i = 0; i < new_col_rezh - col_rezh; i++)
                {
                    this.add_rezhim();
                }
            }
            else
            {
                for (int i = 0; i < col_rezh - new_col_rezh; i++)
                {
                    this.delete_rezhim();
                    change = true;
                }
            }

            update_max_travers_points();

            return change;
        }

        // добавление режима (большой строки)
        public void add_rezhim()
        {
            rezh_num.Add(new num_rezh()
            {
                BD_num = this.rezh_num[this.rezh_num.Count - 1].BD_num + 1,
                visual_num = (rezh_num.Count + 1).ToString(),
                age = "new"
            });

            sreda.Add(new parametr() { value = sreda[0].value });

            rezh_par.add_row();

            prochie_par.add_row();

            foreach (section sec in sections)
            {
                if (sec.pars == null)
                {
                    int travers_point_count = sec.coordinates.travers_table[0].cols[0].Count;
                    sec.coordinates.add_travers_row(travers_point_count);
                    sec.trav_pars.add_travers_row(travers_point_count);
                }
                else
                {
                    sec.pars.add_row();
                }
                sec.age = "new";
            }
        }

        //удаление режима
        public void delete_rezhim()
        {
            if (rezh_num.Count >= 1)
            {
                rezh_num.RemoveAt(rezh_num.Count - 1);

                sreda.RemoveAt(sreda.Count - 1);

                rezh_par.delete_row();

                prochie_par.delete_row();

                foreach (section sec in sections)
                {
                    if (sec.pars == null)
                    {
                        sec.coordinates.delete_travers_row();
                        sec.trav_pars.delete_travers_row();
                    }
                    else
                    {
                        sec.pars.delete_row();
                    }
                }
            }
        }

        // добавление точки траверсирования в сечение i (по одной строчке в каждую большую строку)
        public void section_add_travers_point(int i)
        {
            if (sections[i].pars == null)
            {
                sections[i].coordinates.add_row_into_travers_row();
                sections[i].trav_pars.add_row_into_travers_row();
            }
            update_max_travers_points();
        }

        // добавление нового сечения с траверсированием
        public void sections_add_travers_section(int id_section, ObservableCollection<string> coord, ObservableCollection<string> parametrs, int travers_points_count, int rezhims_count, string age)
        {
            sections.Add(new section(id_section, coord, parametrs, travers_points_count, rezhims_count, age));
            update_max_travers_points();
        }

        // добавление нового сечения без траверсированием
        public void sections_add_section(int id_section, ObservableCollection<string> parametrs, int rezhims_count, string age)
        {
            sections.Add(new section(id_section, parametrs, rezhims_count, age));
            update_max_travers_points();
        }

        // обновления поля максимального кол-ва точек траверсирования
        public void update_max_travers_points()
        {
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

        // сохранение данных структуры классов в базе данных
        public bool save_in_DB(int chan)
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

                NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {Data.current_realization} and \"Channel\" = {chan}", sqlconn);
                string Id_r_c = comm_id.ExecuteScalar().ToString();

                foreach (num_rezh rezh in this.rezh_num)
                {
                    int row_ind = this.rezh_num.IndexOf(rezh); // индекс режима (индекс большой строки в таблице)
                    if (rezh.age == "new")
                    {
                        //добавить в mode_cros_section запись c сечением 0 mode
                        //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Mode_cros_section\" (\"Id_R_C\", \"Id_mode\", id_cros_section) VALUES( {Id_r_c},{rezh.BD_num},0); ", sqlconn);
                        //com_add.ExecuteNonQuery();
                        insert_mode_cros_section(Id_r_c, rezh.BD_num, 0);
                        rezh.age = "old";
                        if (rezh.visual_num == "1")
                        {
                            //добавить параметр среда в таблицу parametrs_experiment
                            //NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда'),3); ", sqlconn);
                            //com_add1.ExecuteNonQuery();
                            insert_parametrs_experiment(Id_r_c,"Среда",3);
                        }

                    }

                    switch (this.sreda[row_ind].mode)
                    {
                        case "new":
                            // добавить параметр среда в таблицу values_experiment
                            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_string) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 3 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда') and \"Id_R_C\" = {Id_r_c}),0,'{this.sreda[row_ind].DB_value()}'); ", sqlconn);

                            //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
                            //NpgsqlCommand com_add = new NpgsqlCommand($"main_block.insert_values_exp(1,{rezh.BD_num}, 0, {Id_r_c}, 3, 'Среда',0, Null ,'{this.sreda[row_ind].DB_value()}');", sqlconn);
                            //com_add.ExecuteNonQuery();
                            insert_values_exp(1, rezh.BD_num,0, Id_r_c,3, "Среда", 0, null, this.sreda[row_ind].DB_value());

                            this.sreda[row_ind].mode = "old";
                            break;
                        case "update":
                            // обновить параметр среда в таблице values_experiment
                            //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_string = '{this.sreda[row_ind].DB_value()}',date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 3 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                            //com_add1.ExecuteNonQuery();
                            update_values_exp(1, rezh.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());
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
                                    NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                    string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                    if (Id_pe == "0")
                                    {
                                        // добавить параметр в таблицу parametr experiment
                                        //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
                                        //com_add2.ExecuteNonQuery();
                                        insert_parametrs_experiment(Id_r_c, name, 1);
                                    }
                                }
                                // добавить строку в таблицу values_experiment
                                //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,{rezh_par.DB_value()}); ", sqlconn);
                                
                                //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
                                //NpgsqlCommand com_add = new NpgsqlCommand($"main_block.insert_values_exp(1,{rezh.BD_num}, 0, {Id_r_c}, 1, '{name}',0, {rezh_par.DB_value()} ,null);", sqlconn);
                                //com_add.ExecuteNonQuery();

                                insert_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, rezh_par.DB_value(), null);
                                rezh_par.mode = "old";
                                break;

                            case "update":
                                // обновить запись в таблице values_experiment
                                //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {rezh_par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                                //com_add1.ExecuteNonQuery();
                                update_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, rezh_par.DB_value(), null);
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
                                    NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                    string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                    if (Id_pe == "0")
                                    {
                                        // добавить параметр в таблицу parametr experiment
                                        //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
                                        //com_add2.ExecuteNonQuery();
                                        insert_parametrs_experiment(Id_r_c, name, 1);
                                    }
                                }
                                // добавить строку в таблицу values_experiment
                                //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,{proch_par.DB_value()}); ", sqlconn);
                                //com_add.ExecuteNonQuery();
                                insert_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, proch_par.DB_value(), null);
                                proch_par.mode = "old";
                                break;

                            case "update":
                                // обновить запись в таблице values_experiment
                                //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {proch_par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                                //com_add1.ExecuteNonQuery();
                                update_values_exp(1, rezh.BD_num, 0, Id_r_c, 1, name, 0, proch_par.DB_value(), null);
                                proch_par.mode = "old";
                                break;
                        }
                        id_par++;
                    }

                    int id_cros_section = 1; // индекс ceчения (1, 2, 3 ...)
                    foreach (section sec in this.sections)
                    {
                        if (sec.age == "new")
                        {
                            // добавить запись в таблицу mode_cros_section
                            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Mode_cros_section\" (\"Id_R_C\", \"Id_mode\", id_cros_section) VALUES( {Id_r_c},{rezh.BD_num},{id_cros_section}); ", sqlconn);
                            //com_add.ExecuteNonQuery();
                            insert_mode_cros_section(Id_r_c, rezh.BD_num, id_cros_section);
                            if (rezh.visual_num == this.rezh_num.Count.ToString())
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
                                            NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                            string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                            if (Id_pe == "0")
                                            {
                                                // добавить параметр в таблицу parametr experiment
                                                //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
                                                //com_add2.ExecuteNonQuery();
                                                insert_parametrs_experiment(Id_r_c, name, 1);
                                            }
                                        }
                                        // добавить строку в таблицу values_experiment
                                        //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = {id_cros_section} and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,{par.DB_value()}); ", sqlconn);
                                        //com_add.ExecuteNonQuery();

                                        insert_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, 0, par.DB_value(), null);
                                        par.mode = "old";
                                        break;

                                    case "update":
                                        // обновить запись в таблице values_experiment
                                        //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = {id_cros_section} and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                                        //com_add1.ExecuteNonQuery();
                                        update_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, 0, par.DB_value(), null);
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
                                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                                if (Id_pe == "0")
                                                {
                                                    // добавить параметр в таблицу parametr experiment
                                                    //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
                                                    //com_add2.ExecuteNonQuery();
                                                    insert_parametrs_experiment(Id_r_c, name, 1);
                                                }
                                            }
                                            // добавить строку в таблицу values_experiment
                                            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = {id_cros_section} and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),{id_trav},{par.DB_value()}); ", sqlconn);
                                            //com_add.ExecuteNonQuery();

                                            insert_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
                                            par.mode = "old";
                                            break;

                                        case "update":
                                            // обновить запись в таблице values_experiment
                                            //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = {id_cros_section} and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = {id_trav}; ", sqlconn);
                                            //com_add1.ExecuteNonQuery();
                                            update_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
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
                                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                                if (Id_pe == "0")
                                                {
                                                    // добавить параметр в таблицу parametr experiment
                                                    //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),1); ", sqlconn);
                                                    //com_add2.ExecuteNonQuery();
                                                    insert_parametrs_experiment(Id_r_c, name, 1);
                                                }
                                            }
                                            // добавить строку в таблицу values_experiment
                                            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_number) VALUES (1,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = {id_cros_section} and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),{id_trav},{par.DB_value()}); ", sqlconn);
                                            //com_add.ExecuteNonQuery();

                                            insert_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
                                            par.mode = "old";
                                            break;

                                        case "update":
                                            // обновить запись в таблице values_experiment
                                            //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_number = {par.DB_value()},date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 1 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = {id_cros_section} and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 1 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = {id_trav}; ", sqlconn);
                                            //com_add1.ExecuteNonQuery();
                                            update_values_exp(1, rezh.BD_num, id_cros_section, Id_r_c, 1, name, id_trav, par.DB_value(), null);
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

    class DB_proc_func
    {
        //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
        public void insert_values_exp(int obr0_rez1_, int rezh_ , int sec_ , string id_r_c_, int id_data_, string par_name_ , int  traver_, string value_n_ , string value_s_)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            if (value_n_ == null) value_n_ = "Null";
            if (value_s_ == null) value_s_ = "Null"; else value_s_ = $"'{value_s_}'";
            //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
            NpgsqlCommand com_add = new NpgsqlCommand($"call main_block.insert_values_exp({obr0_rez1_},{rezh_},{sec_}, {id_r_c_}, {id_data_}, '{par_name_}',{traver_},{ value_n_} ,{value_s_});", sqlconn);
            com_add.ExecuteNonQuery();

            sqlconn.Close();
        }

        public void update_values_exp(int obr0_rez1_, int rezh_, int sec_, string id_r_c_, int id_data_, string par_name_, int traver_, string value_n_, string value_s_)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            if (value_n_ == null) value_n_ = "Null";
            if (value_s_ == null) value_s_ = "Null"; else value_s_ = $"'{value_s_}'";
            //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
            NpgsqlCommand com_add = new NpgsqlCommand($"call main_block.update_values_exp({obr0_rez1_},{rezh_},{sec_}, {id_r_c_}, {id_data_}, '{par_name_}',{traver_},{ value_n_} ,{value_s_});", sqlconn);
            com_add.ExecuteNonQuery();

            sqlconn.Close();
        }

        public void insert_mode_cros_section(string id_r_c_, int rezh_, int sec_)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            //добавить в mode_cros_section запись c сечением 0 mode
            NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id_rcm\") from main_block.\"Mode\" where \"Id_R_C\" ={id_r_c_} and \"Id_mode\" = {rezh_}", sqlconn);
            string number_of_modes = comm.ExecuteScalar().ToString();
            if (number_of_modes == "0")
            {
                NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Mode\" (\"Id_R_C\", \"Id_mode\") VALUES( {id_r_c_},{rezh_}); ", sqlconn);
                com_add1.ExecuteNonQuery();
            }
            NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Cros_section\" (\"Id_rcm\", id_cros_section) VALUES( (select \"Id_rcm\" from main_block.\"Mode\" where \"Id_R_C\"={id_r_c_} and \"Id_mode\" ={rezh_}) ,{sec_}); ", sqlconn);
            com_add2.ExecuteNonQuery();

            sqlconn.Close();
        }

        public void insert_parametrs_experiment(string id_r_c_, string name_, int id_data_)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            //добавить параметр среда в таблицу parametrs_experiment
            NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({id_r_c_} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name_}'),{id_data_}); ", sqlconn);
            com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }



    }

    class Obrabotka_of_fiz_exp : DB_proc_func
    {
        public ObservableCollection<num_rezh> rezh_num { get; set; }// список номеров режимов
        public ObservableCollection<parametr> sreda { get; set; }   // список значений параметра среда
        public parametrs rezh_par { get; set; }                     // режимные параметры
        public ObservableCollection<section> sections { get; set; } // список сечений

        public parametrs integr_par { get; set; }                     // интегральные параметры

        public int id_chan { get; set; }                            // номер канала


        public Obrabotka_of_fiz_exp(ObservableCollection<num_rezh> rezh_num,
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

        public Obrabotka_of_fiz_exp(bool DB, int chan)// заполнение из БД
        {
            if (DB)
            {
                this.id_chan = chan;

                String conn_str = User.Connection_string;       //строка подключения

                ObservableCollection<num_rezh> rezh_num = new ObservableCollection<num_rezh>();

                ObservableCollection<parametr> sreda = new ObservableCollection<parametr>();

                parametrs rezh_par = new parametrs();

                parametrs integr_par = new parametrs();

                ObservableCollection<section> sections = new ObservableCollection<section>();

                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                //заполнение и создание таблиц каналов из базы данных
                NpgsqlCommand comm_main = new NpgsqlCommand($"select * from main_block.\"select_chan_results\"({Data.id_obj},{Data.current_realization},{this.id_chan},0);", sqlconn);
                NpgsqlDataReader reader_main = comm_main.ExecuteReader();
                while (reader_main.Read())
                {
                    int rezhim = (int)reader_main[0];           //номер режима
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


                    if (rezh_num.Count < rezhim)
                    {
                        num_rezh rezh = new num_rezh() { BD_num = rezhim, visual_num = Convert.ToString(rezh_num.Count + 1), age = "old" };
                        rezh_num.Add(rezh);
                    }
                    if (rezh_num.Count == rezhim)
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
                                if (parametrs.table.Count < rezh_num.Count)
                                {
                                    ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols = cols;
                                    r.rezh = rezhim;
                                    r.age = "old";
                                    parametrs.table.Add(r);
                                }
                                else
                                {
                                    parametrs.table[rezh_num.Count - 1].cols.Add(par);
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
                                if (sections[id_section - 1].pars.table.Count < rezh_num.Count)
                                {
                                    //ObservableCollection<parametr> cols = new ObservableCollection<parametr>() { par };
                                    row r = new row();
                                    r.cols.Add(par);
                                    r.rezh = rezhim;
                                    r.age = "old";
                                    sections[id_section - 1].pars.table.Add(r);
                                }
                                else
                                {
                                    sections[id_section - 1].pars.table[rezh_num.Count - 1].cols.Add(par);
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

                this.rezh_num = rezh_num;
                this.sreda = sreda;
                this.rezh_par = rezh_par;
                this.integr_par = integr_par;
                this.sections = sections;
            }
        }

        public Obrabotka_of_fiz_exp(Construct constr, int chan) // создание заполненноей структтуры классов по параметрам конструктора
        {
            this.id_chan = chan;

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

        public void update(Construct constr) // обновление структуры классов по параметрам конструктора
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

        // добавление режима (большой строки)
        public void add_rezhim()
        {
            rezh_num.Add(new num_rezh()
            {
                BD_num = this.rezh_num[this.rezh_num.Count - 1].BD_num + 1,
                visual_num = (rezh_num.Count + 1).ToString(),
                age = "new"
            });

            sreda.Add(new parametr() { value = sreda[0].value });

            rezh_par.add_row();

            integr_par.add_row();

            foreach (section sec in sections)
            {
                sec.pars.add_row();
                //sec.age = "new";???
            }
        }

        // добавление нового сечения без траверсированием
        public void sections_add_section(int id_section, ObservableCollection<string> parametrs, int rezhims_count, string age)
        {
            sections.Add(new section(id_section, parametrs, rezhims_count, age));
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
                    foreach (string par in Data.chans_results[id_chan].rezh_par.column_headers)
                    {
                        Param p = Parametrs.reg_pars[par];
                        string value = Data.chans_results[id_chan].rezh_par.par_value(par, id_row).value;
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }
                    //прочие параметры
                    foreach (string par in Data.chans_results[id_chan].prochie_par.column_headers)
                    {
                        Param p = Parametrs.phys_pars[par];
                        string value = Data.chans_results[id_chan].prochie_par.par_value(par, id_row).value;
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
                        if (Data.chans_results[id_chan].sections[sec_id].pars == null)
                        {
                            foreach (string par in Data.chans_results[id_chan].sections[sec_id].coordinates.column_headers)
                            {
                                Param p = new Param() { short_name = par + "_i", unit = "" };
                                int travers_points_count = Data.chans_results[id_chan].sections[sec_id].coordinates.travers_table[id_row].cols[0].Count;

                                for (int i = 0; i < travers_points_count; i++)
                                {
                                    if (Data.chans_results[id_chan].sections[sec_id].coordinates.column_headers.IndexOf(par) == 0)
                                    {
                                        Dictionary<string, double> arg_j_value = new Dictionary<string, double>();
                                        arg_i_value.Add(arg_j_value);
                                    }
                                    string value = Data.chans_results[id_chan].sections[sec_id].coordinates.par_value(par, id_row, i).value;
                                    arg_i_value[i].Add(p.short_name, Convert.ToDouble(value));
                                }
                            }
                            foreach (string par in Data.chans_results[id_chan].sections[sec_id].trav_pars.column_headers)
                            {
                                Param p = Parametrs.phys_pars[par];
                                p.short_name += "_i";
                                int travers_points_count = Data.chans_results[id_chan].sections[sec_id].trav_pars.travers_table[id_row].cols[0].Count;

                                for (int i = 0; i < travers_points_count; i++)
                                {
                                    //if (Data.chans_results[id_chan].sections[sec_id].trav_pars.column_headers.IndexOf(par) == 0)
                                    //{
                                    //    Dictionary<string, double> arg_j_value = new Dictionary<string, double>();
                                    //    arg_i_value.Add(arg_j_value);
                                    //}
                                    string value = Data.chans_results[id_chan].sections[sec_id].trav_pars.par_value(par, id_row, i).value;
                                    arg_i_value[i].Add(p.short_name, Convert.ToDouble(value));
                                }
                            }
                        }
                        else
                        {
                            foreach (string par in Data.chans_results[id_chan].sections[sec_id].pars.column_headers)
                            {
                                Param p = Parametrs.phys_pars[par];
                                string value = Data.chans_results[id_chan].sections[sec_id].pars.par_value(par, id_row).value;
                                arg_value.Add(p.short_name+$"_sec{sec_id+1}", Convert.ToDouble(value));
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
                        foreach (string par in Data.chans_results[id_chan].rezh_par.column_headers)
                        {
                            Param p = Parametrs.reg_pars[par];

                            string value = Data.chans_results[id_chan].rezh_par.par_value(par, id_row).value;
                            arg_value.Add(p.short_name, Convert.ToDouble(value));
                        }
                        //прочие параметры
                        foreach (string par in Data.chans_results[id_chan].prochie_par.column_headers)
                        {
                            Param p = Parametrs.phys_pars[par];
                            string value = Data.chans_results[id_chan].prochie_par.par_value(par, id_row).value;
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
                    foreach (string par in Data.chans_results[id_chan].rezh_par.column_headers)
                    {
                        Param p = Parametrs.reg_pars[par];
                        string value = Data.chans_results[id_chan].rezh_par.par_value(par, id_row).value;
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }
                    foreach (string par in Data.chans_obr[id_chan].rezh_par.column_headers)
                    {
                        Param p = Parametrs.reg_pars[par];
                        if (!arg_value.ContainsKey(p.short_name))
                        {
                            string value = Data.chans_obr[id_chan].rezh_par.par_value(par, id_row).value;
                            if (value != "")
                            {
                                arg_value.Add(p.short_name, Convert.ToDouble(value));
                            }
                        }
                    }
                    //параметры обработки результатов всех сечений 
                    foreach (section sec in Data.chans_obr[id_chan].sections)
                    {
                        int id = Data.chans_obr[id_chan].sections.IndexOf(sec);
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
                    foreach (string par in Data.chans_results[id_chan].prochie_par.column_headers)
                    {
                        Param p = Parametrs.phys_pars[par];
                        string value = Data.chans_results[id_chan].prochie_par.par_value(par, id_row).value;
                        arg_value.Add(p.short_name, Convert.ToDouble(value));
                    }


                    pr.func_to_value(arg_value, arg_i_value);
                }
            }

        }

        // сохранение данных структуры классов в базе данных
        public void save_in_DB(int chan)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();


            NpgsqlCommand comm_id = new NpgsqlCommand($"select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {Data.current_realization} and \"Channel\" = {chan}", sqlconn);
            string Id_r_c = comm_id.ExecuteScalar().ToString();

            foreach (num_rezh rezh in this.rezh_num)
            {
                int row_ind = this.rezh_num.IndexOf(rezh); // индекс режима (индекс большой строки в таблице)
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
                        // добавить параметр среда в таблицу values_experiment
                        //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_string) VALUES (0,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 3 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда') and \"Id_R_C\" = {Id_r_c}),0,'{this.sreda[row_ind].DB_value()}'); ", sqlconn);
                        //com_add.ExecuteNonQuery();
                        insert_values_exp(0, rezh.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());
                        this.sreda[row_ind].mode = "old";
                        break;
                    case "update":
                        // обновить параметр среда в таблице values_experiment
                        //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_string = '{this.sreda[row_ind].DB_value()}',date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 0 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 3 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = 'Среда') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                        //com_add1.ExecuteNonQuery();
                        update_values_exp(0, rezh.BD_num, 0, Id_r_c, 3, "Среда", 0, null, this.sreda[row_ind].DB_value());
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
                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                if (Id_pe == "0")
                                {
                                    // добавить параметр в таблицу parametr experiment
                                    //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),4); ", sqlconn);
                                    //com_add2.ExecuteNonQuery();
                                    insert_parametrs_experiment(Id_r_c, name, 4);
                                }
                            }
                            // добавить строку в таблицу values_experiment
                            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_string) VALUES (0,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,'{rezh_par.function_value}'); ", sqlconn);
                            //com_add.ExecuteNonQuery();
                            insert_values_exp(0, rezh.BD_num, 0, Id_r_c, 4, name, 0, null, rezh_par.function_value);
                            rezh_par.mode = "old";
                            break;

                        case "update":
                            // обновить запись в таблице values_experiment
                            //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_string = '{rezh_par.function_value}',date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 0 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                            //com_add1.ExecuteNonQuery();
                            update_values_exp(0, rezh.BD_num, 0, Id_r_c, 4, name, 0, null, rezh_par.function_value);
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
                                NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                if (Id_pe == "0")
                                {
                                    // добавить параметр в таблицу parametr experiment
                                    //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),4); ", sqlconn);
                                    //com_add2.ExecuteNonQuery();
                                    insert_parametrs_experiment(Id_r_c,name,4);
                                }
                            }
                            // добавить строку в таблицу values_experiment
                            //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_string) VALUES (0,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = 0 and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,'{integr_par.function_value}'); ", sqlconn);
                            //com_add.ExecuteNonQuery();
                            insert_values_exp(0, rezh.BD_num, 0, Id_r_c, 4, name, 0, null, integr_par.function_value);
                            integr_par.mode = "old";
                            break;

                        case "update":
                            // обновить запись в таблице values_experiment
                            NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_string = '{integr_par.function_value}',date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 0 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = 0 and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                            com_add1.ExecuteNonQuery();
                            update_values_exp(0, rezh.BD_num, 0, Id_r_c, 4, name, 0, null, integr_par.function_value);
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
                        // добавить запись в таблицу mode_cros_section
                        //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Mode_cros_section\" (\"Id_R_C\", \"Id_mode\", id_cros_section) VALUES( {Id_r_c},{rezh.BD_num},{id_cros_section}); ", sqlconn);
                        //com_add.ExecuteNonQuery();
                        if (rezh.visual_num == this.rezh_num.Count.ToString())
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
                                        NpgsqlCommand comm = new NpgsqlCommand($"select count(\"Id#\") from main_block.\"Parametrs_experiment\" where \"Id_R_C\" ={Id_r_c} and Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}')", sqlconn);
                                        string Id_pe = comm.ExecuteScalar().ToString();//Id#
                                        if (Id_pe == "0")
                                        {
                                            // добавить параметр в таблицу parametr experiment
                                            //NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({Id_r_c} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}'),4); ", sqlconn);
                                            //com_add2.ExecuteNonQuery();
                                            insert_parametrs_experiment(Id_r_c,name,4);
                                        }
                                    }
                                    // добавить строку в таблицу values_experiment
                                    //NpgsqlCommand com_add = new NpgsqlCommand($"INSERT INTO main_block.\"Values_experiment\"(\"id_obr0/rez1\", \"Id_m_c\", \"Id#\", id_traversing, value_string) VALUES (0,(select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" = {rezh.BD_num} and id_cros_section = {id_cros_section} and \"Id_R_C\" = {Id_r_c}),(select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = {Id_r_c}),0,'{par.function_value}'); ", sqlconn);
                                    //com_add.ExecuteNonQuery();
                                    insert_values_exp(0, rezh.BD_num, id_cros_section, Id_r_c, 4, name, 0, null, par.function_value);
                                    par.mode = "old";
                                    break;

                                case "update":
                                    // обновить запись в таблице values_experiment
                                    //NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Values_experiment\" SET value_string = '{par.function_value}',date = CURRENT_TIMESTAMP WHERE \"id_obr0/rez1\" = 0 and \"Id_m_c\" = (select \"Id_m_c\" from main_block.\"Mode_cros_section\" where \"Id_mode\" =  {rezh.BD_num} and \"id_cros_section\" = {id_cros_section} and \"Id_R_C\" ={Id_r_c}) and \"Id#\" = (select \"Id#\" from main_block.\"Parametrs_experiment\" where Id_data = 4 and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name}') and \"Id_R_C\" = { Id_r_c}) and id_traversing = 0; ", sqlconn);
                                    //com_add1.ExecuteNonQuery();
                                    update_values_exp(0, rezh.BD_num, id_cros_section, Id_r_c, 4, name, 0, null, par.function_value);
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
    class section
    {
        int id_section { get; set; }                        // номер сечения в БД
        public travers_parametrs coordinates { get; set; }  // координаты
        public travers_parametrs trav_pars { get; set; }    // параметры траверсирования
        public parametrs pars { get; set; }                 // параметры при условии, что сечение без траверсирования
        public string age { get; set; }                     // new, old


        // создание пустого сечения (для считывания данных из базы)
        public section(bool travers, int id_section, string age)
        {
            this.id_section = id_section;
            this.age = age;
            if (travers)
            {
                this.coordinates = new travers_parametrs();
                this.trav_pars = new travers_parametrs();
                this.pars = null;
            }
            else
            {
                this.coordinates = null;
                this.trav_pars = null;
                this.pars = new parametrs();
            }
        }

        // создание сечения с траверсированием
        public section(int id_section, ObservableCollection<string> coord, ObservableCollection<string> parametrs, int travers_points_count, int rezhims_count, string age)
        {
            this.id_section = id_section;
            this.age = age;
            this.coordinates = new travers_parametrs() { column_headers = coord };
            //coordinates.column_headers = coord;
            this.trav_pars = new travers_parametrs() { column_headers = parametrs };
            //trav_pars.column_headers = parametrs;
            for (int i = 0; i < rezhims_count; i++)
            {
                this.coordinates.add_travers_row(travers_points_count);
                this.trav_pars.add_travers_row(travers_points_count);
            }
            this.pars = null;

        }

        // создание сечения без траверсирования
        public section(int id_section, ObservableCollection<string> parametrs, int rezhims_count, string age)
        {
            this.id_section = id_section;
            this.age = age;
            this.pars = new parametrs() { column_headers = parametrs };
            for (int i = 0; i < rezhims_count; i++)
            {
                this.pars.add_row();
            }

            this.coordinates = null;
            this.trav_pars = null;

        }

        // добавление точки траверсирования (в каждую большую строку режима)
        public void add_travers_point()
        {
            if (pars == null)
            {
                coordinates.add_row_into_travers_row();
                trav_pars.add_row_into_travers_row();
            }
        }

        // удаление точки траверсирования (в каждой большой строке режима)
        public void delete_travers_point()
        {
            if (pars == null)
            {
                coordinates.delete_row_from_travers_row();
                trav_pars.delete_row_from_travers_row();
            }
        }


    }

    class travers_parametrs
    {
        public ObservableCollection<travers_row> travers_table { get; set; }    // список траверсированных строк(каждая строка относится к своему режиму и имеет в себе еще несколько строк (точки траверсирования))
        public ObservableCollection<string> column_headers { get; set; }        // список заголовков столбцов

        public travers_parametrs()
        {
            this.travers_table = new ObservableCollection<travers_row>();
            this.column_headers = new ObservableCollection<string>();

        }

        // добавление строки с точками траверсирования (большой строки для режима)
        public void add_travers_row(int travers_points_count)
        {
            travers_row tr = new travers_row();
            for (int i = 0; i < column_headers.Count; i++)// i - столбцы j - строки (т.к. travers_table)
            {
                ObservableCollection<parametr> pars = new ObservableCollection<parametr>();
                for (int j = 0; j < travers_points_count; j++)
                {
                    parametr p = new parametr();
                    if (travers_table.Count > 0)
                        p.id_traversing = travers_table[0].cols[i][j].id_traversing;
                    else p.id_traversing = j + 1;
                    pars.Add(p);
                }
                tr.cols.Add(pars);
            }
            travers_table.Add(tr);
        }

        // удаление строки с точками траверсирования (большой строки для режима)
        public void delete_travers_row()
        {
            if(travers_table.Count>=1)
            travers_table.RemoveAt(travers_table.Count-1);
        }

        // добавление точки траверсирования (в большую строку режима)
        public void add_row_into_travers_row()
        {
            foreach (travers_row r in travers_table)
            {
                foreach (ObservableCollection<parametr> col in r.cols)
                {
                    parametr p = new parametr();
                    p.id_traversing = col[col.Count - 1].id_traversing + 1;
                    col.Add(p);
                }
            }
        }

        // удаление точки траверсирования (в большой строке режима)
        public void delete_row_from_travers_row()
        {
            foreach (travers_row r in travers_table)
            {
                foreach (ObservableCollection<parametr> col in r.cols)
                {
                    if(col.Count>=1)
                    col.RemoveAt(col.Count-1);
                }
            }
        }

        // добавление столбца во все строки с точками траверсирования (большие строки для режимов)
        public void add_col_into_travers_row(string name)
        {
            this.column_headers.Add(name);
            foreach (travers_row r in travers_table)
            {
                ObservableCollection<parametr> col = new ObservableCollection<parametr>();
                for (int i = 0; i < r.cols[0].Count; i++)
                {
                    parametr p = new parametr();
                    p.id_traversing = r.cols[0][i].id_traversing;
                    p.mode = "new";
                    col.Add(p);
                }
                r.cols.Add(col);
            }
        }

        // удаление столбца из всех строк с точками траверсирования (большие строки для режимов)
        public void delete_col_from_travers_row(string name)
        {
            int i = column_headers.IndexOf(name);
            foreach (travers_row r in travers_table)
            {
                r.cols.RemoveAt(i);
            }
            this.column_headers.Remove(name);
        }

        // проверка на пустоту значений параметров
        public bool check_empty_parametr()
        {
            bool res = true;
            foreach (travers_row r in this.travers_table)
            {
                foreach (ObservableCollection<parametr> p in r.cols)
                {
                    foreach (parametr p1 in p)
                    {
                        if (p1.value == "")
                        {
                            res = false;
                            return res;
                        }
                    }
                }
            }
            return res;
        }

        // получение значения параметра по имени, номеру строки и номеру точки траверсирования
        public parametr par_value(string name, int id_row, int id_tr_row)
        {
            return this.travers_table[id_row].cols[this.column_headers.IndexOf(name)][id_tr_row];
        }


    }

    class travers_row
    {
        public ObservableCollection<ObservableCollection<parametr>> cols { get; set; }//список столбцов внутри одного режима, которые являются списками праметров (точки траверсирования)
        public string age { get; set; }
        public int rezh { get; set; }
        public travers_row()
        {
            this.cols = new ObservableCollection<ObservableCollection<parametr>>();
            this.rezh = new int();
            this.age = "new";
        }

    }
    class parametrs
    {
        // таблица параметров (список строк таблицы)
        public ObservableCollection<row> table { get; set; }

        // список заголовков таблицы (названий параметров)
        public ObservableCollection<string> column_headers { get; set; }

        // список выпадающих списков для столбцов
        public ObservableCollection<List<string>> column_drop_lists { get; set; } 

    public parametrs()
        {
            this.table = new ObservableCollection<row>();
            this.column_headers = new ObservableCollection<string>();
            this.column_drop_lists = new ObservableCollection<List<string>>();
        }

        // добавление строки в таблицу значений параметров
        public void add_row()
        {
            row r = new row();
            for (int i = 0; i < column_headers.Count; i++)
            {
                r.cols.Add(new parametr());
            }
            table.Add(r);
        }

        // удаление строки из таблицы значений параметров
        public void delete_row()
        {
            if(this.table.Count>=1)
            this.table.RemoveAt(this.table.Count-1);
        }

        // добавление нового параметра 
        public void add_parametr(string name)
        {
            this.column_headers.Add(name);
            foreach (row r in table)
            {
                parametr p = new parametr();
                p.mode = "new";
                r.cols.Add(p);
            }
        }

        public void add_parametr(string name, string[] pars)
        {
            this.column_headers.Add(name);
            if (table.Count != 0)
            {
                for (int i = 0; i < table.Count; i++)
                {
                    parametr p = new parametr();
                    p.mode = "old";
                    p.value = pars[i];
                    table[i].cols.Add(p);
                }
            }
            else
            {
                for (int i = 0; i < pars.Length; i++)
                {
                    parametr p = new parametr();
                    p.mode = "old";
                    p.value = pars[i];

                    row r = new row();
                    r.cols.Add(p);
                    r.age = "old";
                    table.Add(r);
                }
            }
            
        }

        // удаление параметра
        public void delete_parametr(string name)
        {
            int i = column_headers.IndexOf(name);
            foreach (row r in table)
            {
                r.cols.RemoveAt(i);
            }
            this.column_headers.Remove(name);
        }

        // проверка на пустоту значений параметров
        public bool check_empty_parametr()
        {
            bool res = true;
            foreach (row r in this.table)
            {
                foreach (parametr p in r.cols)
                {
                    if (p.value == "")
                    {
                        res = false;
                        return res;
                    }
                }
            }
            return res;
        }

        // получение значения параметра по имени и номеру строки
        public parametr par_value(string name, int id_row)
        {
            return this.table[id_row].cols[this.column_headers.IndexOf(name)];
        }

        // заполнение датагрида
        public static void parametrs_table_build(DataGrid gr, parametrs par)
        {
            gr.ItemsSource = par.table;
            //gr.CellEditEnding += datagrid2_CellEditEnding;
            //gr.KeyDown += TxtBox_chan_KeyDown;
            for (int i = 0; i < par.column_headers.Count; i++)
            {
                var style = new Style();
                style.Setters.Add(new Setter(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));

                if (par.column_drop_lists[i].Count == 0) // если без выпадающего списка
                {
                    gr.Columns.Add(new DataGridTextColumn
                    {
                        Header = $"{par.column_headers[i]}, {Parametrs.get_param(par.column_headers[i]).unit}",
                        Binding = new Binding($"cols[{i}].value") { Mode = BindingMode.TwoWay },
                        ElementStyle = style
                    });
                }
                else // если с выпадающим списком
                {
                    DataGridComboBoxColumn col = new DataGridComboBoxColumn();
                    col.Header = $"{par.column_headers[i]}, {Parametrs.get_param(par.column_headers[i]).unit}";
                    col.TextBinding = new Binding($"cols[{i}].value") { Mode = BindingMode.TwoWay };
                    col.ItemsSource = par.column_drop_lists[i];
                    var edit_style = new Style();
                    edit_style.Setters.Add(new Setter(ComboBox.IsEditableProperty, true));
                    col.EditingElementStyle = edit_style;
                    col.ElementStyle = style;
                    gr.Columns.Add(col);
                }
            }
        }
    }

    class row // строка таблицы
    {
        public ObservableCollection<parametr> cols { get; set; }//столбцы строки
        public int rezh { get; set; }                           // номер режима
        public string age { get; set; } //

        public row()
        {
            this.cols = new ObservableCollection<parametr>();
            this.rezh = new int();
            this.age = "new";
        }

        //public string age { get; set; }
        //public row(ObservableCollection<parametr> cols, int rezh)
        //{
        //    this.cols = cols;
        //    this.rezh = rezh;
        //    if (cols[0].value == "" && cols[0].mode == "new")
        //    {
        //        age = "new";
        //    }
        //    else age = "old";
        //}
    }
    class num_rezh
    {
        public int BD_num { get; set; }         //номер режима в базе
        public string visual_num { get; set; }  // отображаемый номер режима
        public string age { get; set; }         //new, old
    }
    public class parametr
    {
        public string function_value { get; set; } // значение функции
        public string value { get; set; }       //значение параметра
        public int id_traversing { get; set; }  // номер траверсирования в базе
        public string mode { get; set; }        //new, update, old
        public parametr()
        {
            value = "";
            function_value = "";
            id_traversing = 0;
            mode = "new";
        }

        // если формула обработалась корректно, то метод вернет пустую строку, иначе он вернет часть строки в которой, была обнаружена ошибка ввода и очистит значение функции
        public string func_to_value(Dictionary<string, double> arg_value, List<Dictionary<string, double>> arg_i_value)
        {
            string func_buf = this.function_value;
            double val = Calculator.func_v5(ref func_buf, arg_value, arg_i_value);
            if (func_buf == "")
                this.value = val.ToString();
            else this.function_value = "";

            return func_buf;
        }

        public string DB_value()
        {
            return this.value.Replace(",", ".");
        }
    }

}
