using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Controls;
using Npgsql;

namespace БД_НТИ
{
    class Data
    {
        public static string id { get; set; }   //ID*
        public static string id_obj { get; set; }   //id$
        public static bool exec { get; set; }//(для добавления параметров); существует ли уже данный класс задач в базе данных

        public static List<channel> channels { get; set; }  //список каналов

        public static string current_realization { get; set; }   //текущее исполнение (номер из базы данных)

        public static int id_realization { get; set; } // номер строки в таблице геометрических параметров (номер реализации в приложении от 0)

        public static int current_channel { get; set; } //текущий канал (номер из базы данных)

        public static int id_channel { get; set; }      //номер канала (в приложении) 



        public static ObservableCollection<Construct> constr = new ObservableCollection<Construct>(); //см. страница Exp_construct - список каналов с настройками конструктора

        public static List<Results_of_fiz_exp> chans_results { get; set; }

        public static List<Obrabotka_of_fiz_exp> chans_obr { get; set; }
        public static string php_name { get; set; }//тип физического процесса
        public static string tpe_name { get; set; }//тип энергетического оборудования
        public static string ar1_name { get; set; }//область расчетного случая/экспериментальный объект
    }

    public struct Param
    {
        public string short_name { get; set; }  // обозначение параметра
        public string unit { get; set; }        // единицы измерения
    }

    static class Parametrs // все параметры из базы данных
    {
        static String conn_str = User.Connection_string;       //строка подключения

        public static Dictionary<string, Param> geom_pars { get; set; }     //Геометрические параметры
        public static Dictionary<string, Param> reg_pars { get; set; }      //Режимные параметры
        public static Dictionary<string, Param> phys_pars { get; set; }     //Теплофизические параметры
        public static Dictionary<string, Param> integral_pars { get; set; } //Интегральные характеристики физического процесса

        public static Dictionary<string, Param> coord_pars { get; set; } //координаты
        public static Dictionary<string, Param> reshatel_pars { get; set; } //Настройки решателя
        public static Dictionary<string, Param> setka_pars { get; set; } //Параметры сетки

        public static Param get_param(string name)
        {
            Param p = new Param() { short_name = "", unit = ""};
            List<Dictionary<string, Param>> lst = new List<Dictionary<string, Param>> { geom_pars, reg_pars, phys_pars, integral_pars, coord_pars, reshatel_pars, setka_pars };
            foreach (Dictionary<string, Param> dic in lst)
            {
                if (dic != null)
                {
                    if (dic.ContainsKey(name))
                        return dic[name];
                }
            }
            return p;
        }

        public static string check_new_param (string name,string short_name)
        {
            string res = "";//если новый параметр уникален, то возврат "" (если подобное название или обозначение уже есть, то - возврат имеющегося названия или обозначенния)

            if (get_param(name).short_name != "") return name;

            List<Dictionary<string, Param>> lst = new List<Dictionary<string, Param>> { geom_pars, reg_pars, phys_pars, integral_pars, coord_pars, reshatel_pars, setka_pars };
            foreach(Dictionary<string, Param> dic in lst)
            {
                foreach (KeyValuePair<string, Param> p in dic)
                {
                    if (dic != null)
                    {
                        if (p.Value.short_name == short_name) 
                            return short_name;
                    }
                }
            }
            return res;
        }

        public static void update_parametrs()
        {
            List<string> types_of_param = new List<string> { "Геометрические параметры", "Режимные параметры", "Теплофизические параметры", "Интегральные характеристики физического процесса", "Координаты","Настройки решателя", "Параметры сетки"  };
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            Parametrs.geom_pars = new Dictionary<string, Param>();
            Parametrs.reg_pars = new Dictionary<string, Param>();
            Parametrs.phys_pars = new Dictionary<string, Param>();
            Parametrs.integral_pars = new Dictionary<string, Param>();
            Parametrs.coord_pars = new Dictionary<string, Param>();
            Parametrs.reshatel_pars = new Dictionary<string, Param>();
            Parametrs.setka_pars = new Dictionary<string, Param>();
            //заполнение комбобокса параметров для выбора при добавлении нового столбца
            foreach (string type in types_of_param)
            {
                NpgsqlCommand com_params = new NpgsqlCommand($"select * from main_block.select_parametrs('{type}') order by name_param;", sqlconn);
                NpgsqlDataReader reader_par = com_params.ExecuteReader();
                while (reader_par.Read())
                {
                    Param p = new Param() { short_name = reader_par[1].ToString(), unit = reader_par[2].ToString() };
                    switch (type)
                    {
                        case "Геометрические параметры":
                            Parametrs.geom_pars.Add(reader_par[0].ToString(), p);
                            break;

                        case "Теплофизические параметры":
                            Parametrs.phys_pars.Add(reader_par[0].ToString(), p);
                            break;

                        case "Режимные параметры":
                            Parametrs.reg_pars.Add(reader_par[0].ToString(), p);
                            break;

                        case "Интегральные характеристики физического процесса":
                            Parametrs.integral_pars.Add(reader_par[0].ToString(), p);
                            break;

                        case "Координаты":
                            Parametrs.coord_pars.Add(reader_par[0].ToString(), p);
                            break;

                        case "Настройки решателя":
                            Parametrs.reshatel_pars.Add(reader_par[0].ToString(), p);
                            break;

                        case "Параметры сетки":
                            Parametrs.setka_pars.Add(reader_par[0].ToString(), p);
                            break;


                    }
                }
                reader_par.Close();
            }
            sqlconn.Close();
        }

        public static void insert_parametr(string type_of_par, string name, string short_name, string unit)
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();
            NpgsqlCommand com_add1 = new NpgsqlCommand($"call main_block.insert_parametrs('{type_of_par}','{name}','{short_name}','{unit}');", sqlconn);
            com_add1.ExecuteNonQuery();
            Param p = new Param() { short_name = short_name, unit = unit };
            switch (type_of_par)
            {
                case "Геометрические параметры":
                    Parametrs.geom_pars.Add(name, p);
                    break;

                case "Теплофизические параметры":
                    Parametrs.phys_pars.Add(name, p);
                    break;

                case "Режимные параметры":
                    Parametrs.reg_pars.Add(name, p);
                    break;

                case "Интегральные характеристики физического процесса":
                    Parametrs.integral_pars.Add(name, p);
                    break;

                case "Координаты":
                    Parametrs.coord_pars.Add(name, p);
                    break;

                case "Настройки решателя":
                    Parametrs.reshatel_pars.Add(name, p);
                    break;

                case "Параметры сетки":
                    Parametrs.setka_pars.Add(name, p);
                    break;
            }
            sqlconn.Close();
        }
    }

    public class NotEmptyNumericRule : ValidationRule
    {
        public static int Min { get; set; }
        public static int Max { get; set; }
        public static List<double> value_list { get; set; }

        //public static bool flag = false;

        //public override ValidationResult Validate(object value, CultureInfo cultureInfo) //проверка, что поле не пустое
        //{
        //    return string.IsNullOrWhiteSpace((value ?? "").ToString())
        //        ? new ValidationResult(false, "Field is required.")
        //        : ValidationResult.ValidResult;
        //}

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            double val = 0;

            try
            {
                if (((string)value).Length > 0)
                    val = Convert.ToDouble((String)value);

                bool fl = false;
                for(int i = 0; i < value_list.Count; i++)
                {
                    if (val == value_list[i])
                    {
                        fl = true;
                        //flag = true;
                        i = value_list.Count;
                    }
                }

                if (!fl)
                {
                    string str = "Значение не входит в список:";
                    for (int i = 0; i < value_list.Count; i++)
                    {
                        str += $" {value_list[i]};";
                    }
                    str = str.Remove(str.Length - 1);
                    //flag = false;
                    return new ValidationResult(false, str);
                }
                //else
                //{
                //    string str = "Введите одно из значений:";
                //    for (int i = 0; i < value_list.Count; i++)
                //    {
                //        str += $" {value_list[i]};";
                //    }
                //    str = str.Remove(str.Length - 1);
                //    return new ValidationResult(true, str);
                //}
                
            }
            catch (Exception e)
            {
                //flag = false;
                return new ValidationResult(false, $"Недопустимое значение. {e.Message}");
            }

            
            return ValidationResult.ValidResult;
        }
    }

    

}
