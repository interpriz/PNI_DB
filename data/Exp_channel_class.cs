using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace БД_НТИ
{
    class channel
    {
        public ObservableCollection<str> table { get; set; }                // таблица со строками, в которых содержатся значения параметров
        public ObservableCollection<string> column_headers { get; set; }    // коллекция заголовков строк
        public string age { get; set; }                                     // новый или старый канал (new|old)

        // конструктор канала
        public channel(ObservableCollection<str> table, ObservableCollection<string> column_headers)
        {
            this.table = table;
            this.column_headers = column_headers;
            if (column_headers.Count == 0)
            {
                age = "new";
            }
            else age = "old";
        }

        // сохранение данных канала в БД
        public bool save_changes(int chn_num)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(User.Connection_string);
            sqlconn.Open();
            //int chn_num = Data.channels.IndexOf(chn) + 1;                      // номер канала
            bool f1 = true;
            foreach (str row in table)
            {
                if (age == "new" || row.age == "new")
                {
                    NpgsqlCommand com_add1 = new NpgsqlCommand($"INSERT INTO main_block.\"Realization_channel\"(\"Id$\", \"Realization\", \"Channel\") VALUES({Data.id_obj}, {row.realization}, {chn_num});", sqlconn);
                    com_add1.ExecuteNonQuery();
                    row.age = "old";
                    age = "old";
                }
                int r = row.realization;                                    // номер исполнения 
                foreach (List<string> lst in row.cols)
                {
                    string par = column_headers[row.cols.IndexOf(lst)]; //название параметра
                    string value = lst[0];                                  // само значение параметра
                    try
                    {
                        double d = Convert.ToDouble(value);
                    }
                    catch { f1 = false; }
                    if (f1)
                    {
                        switch (lst[1])
                        {
                            case "update":
                                NpgsqlCommand com_add1 = new NpgsqlCommand($"UPDATE main_block.\"Geometric_parametrs\" SET value_number = {value.Replace(',', '.')} WHERE \"Id_R_C\" = (select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" ={Data.id_obj} and \"Realization\" = {r} and \"Channel\" = {chn_num}) and id_param = (select id_param from main_block.\"Parametrs\" where name_param = '{par}');", sqlconn);
                                com_add1.ExecuteNonQuery();
                                lst[1] = "";
                                break;
                            case "new":
                                NpgsqlCommand com_add2 = new NpgsqlCommand($"INSERT INTO main_block.\"Geometric_parametrs\"(\"Id_R_C\", id_param, value_number) VALUES((select \"Id_R_C\" from main_block.\"Realization_channel\" where \"Id$\" = {Data.id_obj} and \"Realization\" = {r} and \"Channel\" = {chn_num}),(select id_param from main_block.\"Parametrs\" where name_param = '{par}'),{value.Replace(',','.')});", sqlconn);
                                com_add2.ExecuteNonQuery();
                                lst[1] = "";
                                break;
                            case "delete":
                                break;
                        }
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show(
                       $"Некорректное значение \"{value}\" параметра \"{par}\", в канале №{chn_num}, исполнении №{table.IndexOf(row) + 1} !", "Caution", MessageBoxButton.OK);
                        sqlconn.Close();
                        return f1;
                    }
                }
            }
            sqlconn.Close();
            return f1;
        }

        // получение значения параметра по его названию и номеру строки
        public string par_value(string name, int id_row)
        {
            return this.table[id_row].cols[this.column_headers.IndexOf(name)][0];
        }
    }

    class str // строка таблицы
    {
        public List<List<string>> cols { get; set; } //список значений параметров в виде списка ("Значение"/"new|old")
        public string age { get; set; }              // новая или старая строка (new|old)
        public int realization { get; set; }         // номер исполнения в базе данных

        // конструктор строки
        public str(List<List<string>> v1, int v2)
        {
            this.cols = v1;
            this.realization = v2;
            if (v1[0][0] == "" && v1[0][1] == "new")
            {
                age = "new";
            }
            else age = "old";
        }


    }

    public class rowheader // заголовок строк таблиц
    {
        public string header { get; set; }      //имя заголовка (номер)
        public int realization { get; set; }    // номер строки (реализации)  в базе данных

        public static string mode = "Experiment";              //Modeling Experiment

        public string button_name { get; set; }

        // конструктор заголовка
        public rowheader(string v1, int v2)
        {
            this.header = v1;
            this.realization = v2;
            switch (mode)
            {
                case "Experiment":
                    button_name = "Добавить результаты эксперимента";
                    break;

                case "Modeling":
                    button_name = "Добавить результаты моделирования";
                    break;
            }
        }
    }
}
