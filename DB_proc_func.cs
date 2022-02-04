using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace БД_НТИ
{
    static class DB_proc_func
    {
        //параметры процедуры по порядку: (обработка/результат), (режим), (сечение), (id_r_c_), (тип данных), (название параметра), (траверсирование), (значение), (строка)
        static public void insert_values_exp(int obr0_rez1_, int rezh_, int sec_, string id_r_c_, int id_data_, string par_name_, int traver_, string value_n_, string value_s_)
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

        static public void update_values_exp(int obr0_rez1_, int rezh_, int sec_, string id_r_c_, int id_data_, string par_name_, int traver_, string value_n_, string value_s_)
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

        static public void insert_mode_cros_section(string id_r_c_, int rezh_, int sec_)
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

        static public void insert_parametrs_experiment(string id_r_c_, string name_, int id_data_)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            //добавить параметр среда в таблицу parametrs_experiment
            NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Parametrs_experiment\"(\"Id_R_C\", \"Id_param\", id_data) VALUES ({id_r_c_} , (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name_}'),{id_data_}); ", sqlconn);
            com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }

        //добавить значение режимного параметра в таблицу Reg_pars
        static public void insert_Reg_pars(string name_, string value_string, string value_number)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            if (value_number == null) value_number = "Null";
            if (value_string == null) value_string = "Null"; else value_string = $"'{value_string}'";

            NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Reg_pars\"(\"Id_rcm\", \"Id_param\", value_string, value_number) VALUES ( (select \"Id_rcm\" from main_block.\"Mode\" where \"Id_R_C\"={Data.id_R_C} and \"Id_mode\"={Data.current_mode}), (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name_}'), {value_string}, {value_number}); ", sqlconn);
            com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }

        static public void update_Reg_pars(string name_, string value_string, string value_number)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            if (value_number == null) value_number = "Null";
            if (value_string == null) value_string = "Null"; else value_string = $"'{value_string}'";

            NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Reg_pars\" SET value_string = {value_string}, value_number =  {value_number} WHERE \"Id_rcm\" = (select \"Id_rcm\" from main_block.\"Mode\" where \"Id_R_C\"={Data.id_R_C} and \"Id_mode\"={Data.current_mode})  and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name_}'); ", sqlconn);
            com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }

        static public void insert_Settings_number(string Id_setting)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();

            NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Settings_number\"(\"Id_rcm\", \"Id_setting\") VALUES((select \"Id_rcm\" from main_block.\"Mode\" where \"Id_R_C\"={Data.id_R_C} and \"Id_mode\"={Data.current_mode}), {Id_setting}); ", sqlconn);com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }

        static public void insert_Settings_values(string name_, string value_string, string value_number, string id_setting)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            if (value_number == null) value_number = "Null";
            if (value_string == null) value_string = "Null"; else value_string = $"'{value_string}'";

            NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Settings_values\"(\"Id_ms\", \"Id_param\", value_string, value_number) VALUES ( (select \"Id_ms\" from main_block.\"Settings_number\" where \"Id_setting\" = {id_setting} and \"Id_rcm\" = (select \"Id_rcm\" from main_block.\"Mode\" where \"Id_R_C\"={Data.id_R_C} and \"Id_mode\"={Data.current_mode})), (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name_}'), {value_string}, {value_number}); ", sqlconn);
            com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }

        static public void update_Settings_values(string name_, string value_string, string value_number, string id_setting)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            if (value_number == null) value_number = "Null";
            if (value_string == null) value_string = "Null"; else value_string = $"'{value_string}'";

            NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Settings_values\" SET value_string = {value_string}, value_number =  {value_number} WHERE \"Id_ms\" = (select \"Id_ms\" from main_block.\"Settings_number\" where \"Id_setting\" = {id_setting} and \"Id_rcm\" = (select \"Id_rcm\" from main_block.\"Mode\" where \"Id_R_C\"={Data.id_R_C} and \"Id_mode\"={Data.current_mode}))  and \"Id_param\" = (select id_param from main_block.\"Parametrs\" where \"name_param\" = '{name_}'); ", sqlconn);
            com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }

        static public void insert_string_values(string name_, string value_string)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();

            NpgsqlCommand com_add1 = new NpgsqlCommand($"CALL main_block.insert_string_values('{name_}', '{value_string}')", sqlconn);
            com_add1.ExecuteNonQuery();

            sqlconn.Close();
        }



    }
}
