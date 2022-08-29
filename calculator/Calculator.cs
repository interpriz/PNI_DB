using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;
using System.Text.RegularExpressions;
using System.Windows;

namespace БД_НТИ
{
    static class Calculator
    {
        // проверка строки на формат формулы
        // ИЗМЕНИТЬ С УЧЕТОМ СКОБОК
        static public string chek(string str)
        {
            Regex regex = new Regex(@"^(-?(ln\((\d+\,?\d*)\)|log\[([\d]+)\]\((\d+\,?\d*)\)|(\d+\,?\d*(\*E\^-?\d+)?)|Sum\{(\S+)\}|Mul\{(\S+)\}|\w+))((\^|\*|/|\+|\-)[+-]?(ln\((\d+\,?\d*)\)|log\[([\d]+)\]\((\d+\,?\d*)\)|(\d+\,?\d*(\*E\^-?\d+)?)|Sum\{(\S+)\}|Mul\{(\S+)\}|\w+))*");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                str = str.Replace(match.Value, "");
            }
            str = "";
            return str;
        }

        // метод преобразование натурального логарифма
        static string ln(string str)
        {
            //ln(12) - натуральный логарифм
            Regex regex = new Regex(@"ln\((\d+\,?\d*)\)");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                //textBox3.Text += "_ln_: " + match.Value + "\r\n";
                string[] words = match.Value.Split(new char[] { '(', ')' });
                double a = Convert.ToDouble(words[1]);
                str = str.Replace(match.Value, Convert.ToString(Log(a)));
                //textBox3.Text += "_ln_res_: " + str + "\r\n";
            }
            return str;
        }

        // метод преобразование логарифма по основанию
        static string log(string str)
        {
            //log[3](9)=2 логарифм 9 по основанию 3
            Regex regex = new Regex(@"log\[(\d+\,?\d*)\]\((\d+\,?\d*)\)");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                //textBox3.Text += "_log_: " + match.Value + "\r\n";
                string[] words = match.Value.Split(new char[] { '(', ')', '[', ']' });
                double a = Convert.ToDouble(words[3]);
                double c = Convert.ToDouble(words[1]); //основание
                str = str.Replace(match.Value, Convert.ToString(Log(a, c)));
                //textBox3.Text += "_log_res_: " + str + "\r\n";
            }
            return str;
        }

        // метод преобразования оператора возведения в степень
        static string exp(string str)
        {
            // a^c
            Regex regex = new Regex(@"-?(\d+\,?\d*)\^[-,+]?(\d+\,?\d*)");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                //textBox3.Text += "_exp_: " + match.Value + "\r\n";
                string[] words = match.Value.Split(new char[] { '^' });
                double a = Convert.ToDouble(words[0]);
                double c = Convert.ToDouble(words[1]); //степень
                //if (Convert.ToString(Pow(a, c)).IndexOf('E') == -1) 
                double result = Pow(a, c);
                if (str[str.IndexOf(match.Value)] == '-' && result > 0)
                {
                    str = str.Replace(match.Value, "+" + Convert.ToString(result));
                }
                else
                {
                    str = str.Replace(match.Value, Convert.ToString(result));
                }
                //textBox3.Text += "_exp_res_: " + str + "\r\n";
            }
            return str;
        }

        // метод преобразования оператора умножения
        static string mul(string str)
        {
            //*
            //regex = new Regex(@"-?([\w,\,]*)\*-?([\w,\,]*)");
            //regex = new Regex(@"-?\d*E?-?\d*,?\d*\,?\d*\*-?\d*E?-?\d*,?\d*");
            // @"\d+\,?\d*"   @"-?E-?\d+"
            while (str.IndexOf('*') != -1)
            {
                Regex regex = new Regex(@"-?((\d+\,?\d*)(E-?\d+)?)\*[-,+]?((\d+\,?\d*)(E-?\d+)?)");
                MatchCollection matches = regex.Matches(str);
                if (matches.Count == 0) break;
                foreach (Match match in matches)
                {
                    //textBox3.Text += "_mul_: " + match.Value + "\r\n";
                    string[] words = match.Value.Split(new char[] { '*' });
                    double a = Convert.ToDouble(words[0]);
                    double c = Convert.ToDouble(words[1]);
                    double result = a * c;
                    if (str[str.IndexOf(match.Value)] == '-' && result > 0)
                    {
                        str = str.Replace(match.Value, "+" + Convert.ToString(result));
                    }
                    else
                    {
                        str = str.Replace(match.Value, Convert.ToString(result));
                    }
                    //textBox3.Text += "_mul_res_: " + str + "\r\n";
                }
            }

            return str;
        }

        // метод преобразования оператора деления
        static string div(string str)
        {
            // a/c
            while (str.IndexOf('/') != -1)
            {
                Regex regex = new Regex(@"-?((\d+\,?\d*)(E-?\d+)?)/[-,+]?((\d+\,?\d*)(E-?\d+)?)");
                MatchCollection matches = regex.Matches(str);
                if (matches.Count == 0) break;
                foreach (Match match in matches)
                {
                    //textBox3.Text += "_div_: " + match.Value + "\r\n";
                    string[] words = match.Value.Split(new char[] { '/' });
                    double a = Convert.ToDouble(words[0]);
                    double c = Convert.ToDouble(words[1]);
                    double result = a / c;
                    if (str[str.IndexOf(match.Value)] == '-' && result > 0)
                    {
                        str = str.Replace(match.Value, "+" + Convert.ToString(result));
                    }
                    else
                    {
                        str = str.Replace(match.Value, Convert.ToString(result));
                    }
                    //textBox3.Text += "_div_res_: " + str + "\r\n";
                }
            }
            return str;
        }

        // метод преобразования оператора суммы и разности
        static string plus_minus(string str)
        {
            //+-
            Regex regex = new Regex(@"([+-]+[+-]+)");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                //textBox3.Text += "_+-_: " + match.Value + "\r\n";
                int count = 0;
                for (int i = 0; i < match.Value.Length; i++)
                {
                    if (match.Value[i] == '-')
                    {
                        count++;
                    }
                }
                if (count % 2 == 0)
                {
                    str = str.Replace(match.Value, "+");
                }
                else
                {
                    str = str.Replace(match.Value, "-");
                }
                //textBox3.Text += "_+-_res_: " + str + "\r\n";
            }
            return str;
        }

        static string Sum(string str, List<Dictionary<string, double>> arg_i_value)
        {
            //Sum( a_i+b_i) - сумма
            Regex regex1 = new Regex(@"Sum\{[^{}]+\}");
            MatchCollection matches1 = regex1.Matches(str);
            int coll = matches1.Count;
            while (coll != 0)
            {
                foreach (Match match in matches1)
                {
                    double sum = 0;
                    //textBox3.Text += "_Sum{}_: " + match.Value + "\r\n";
                    string[] words = match.Value.Split(new char[] { '{', '}' });
                    foreach (Dictionary<string, double> i in arg_i_value)
                    {
                        string sub_str = words[1];
                        List<Dictionary<string, double>> j = new List<Dictionary<string, double>>();
                        sum += func_v5(ref sub_str, i, j);
                        if (sub_str != "") return sub_str;//если преобразование не прошло успешно
                    }
                    str = str.Replace(match.Value, Convert.ToString(sum));
                    //textBox3.Text += "_Sum{}_res_: " + str + "\r\n";
                }

                matches1 = regex1.Matches(str);
                coll = matches1.Count;
            }
            return str;
        }

        static string Mull(string str, List<Dictionary<string, double>> arg_i_value)
        {
            //Mull( a_i+b_i) - произведение
            Regex regex1 = new Regex(@"Mull\{[^{}]+\}");
            MatchCollection matches1 = regex1.Matches(str);
            int coll = matches1.Count;
            while (coll != 0)
            {
                foreach (Match match in matches1)
                {
                    double mul = 1;
                    //textBox3.Text += "__Mull{}_: " + match.Value + "\r\n";
                    string[] words = match.Value.Split(new char[] { '{', '}' });
                    foreach (Dictionary<string, double> i in arg_i_value)
                    {
                        string sub_str = words[1];
                        List<Dictionary<string, double>> j = new List<Dictionary<string, double>>();
                        mul *= func_v5(ref sub_str, i, j);
                        if (sub_str != "") return sub_str;//если преобразование не прошло успешно
                    }
                    str = str.Replace(match.Value, Convert.ToString(mul));
                    //textBox3.Text += "_Mull{}_res_: " + str + "\r\n";
                }

                matches1 = regex1.Matches(str);
                coll = matches1.Count;
            }

            return str;
        }




        // метод преобразования строки в число с учетом операторов Sum и Mull и механизмом скобок
        public static double func_v5(ref string str, Dictionary<string, double> arg_value, List<Dictionary<string, double>> arg_i_value)
        {
            double res = 0;

            // преобразование скалярных переменных
            foreach (KeyValuePair<string, double> a in arg_value)
            {
                if (a.Key != "e" && a.Key != "E" && a.Key != "Pi")
                {
                    Regex regex1 = new Regex($@"([+,\-,\*,/,(,\^]|^){a.Key}([^_]|$)");
                    MatchCollection matches1 = regex1.Matches(str);
                    foreach (Match match in matches1)
                    {
                        //textBox3.Text += "_!_" + match.Value + "\r\n";
                        Regex regex2 = new Regex($@"{a.Key}");
                        string result = regex2.Replace(match.Value, Convert.ToString(a.Value));
                        str = str.Replace(match.Value, result);
                    }
                }
                //str = str.Replace(a.Key, Convert.ToString(a.Value));
            }

            Regex regex3 = new Regex(@"([\[,+,\-,\*,/,(,\^]|^)[e,Pi,E]([\],+,\-,\*,/,),\^]|$)");
            MatchCollection matches3 = regex3.Matches(str);
            foreach (Match match in matches3)
            {
                //textBox3.Text += "_!_" + match.Value + "\r\n";
                string s = match.Value;
                s = s.Replace("e", Convert.ToString(Exp(1)));
                s = s.Replace("E", "10");
                s = s.Replace("Pi", Convert.ToString(PI));
                str = str.Replace(match.Value, s);
            }

            //textBox3.Text += "_!scal!_: " + str + "\r\n";
            //___________________________________________________________________________________________

            // операторы sum mul
            if (arg_i_value.Count != 0)
            {
                str = Sum(str, arg_i_value);
                str = Mull(str, arg_i_value);
            }

            //скобки (начиная с самых нижних и заканчивая верхними)
            //textBox3.Text += "_!scob!_: " + str + "\r\n";
            Regex regex0 = new Regex(@"\([^()]+\)");
            MatchCollection matches0 = regex0.Matches(str);
            int coll = matches0.Count;
            while (coll != 0)
            {
                foreach (Match match0 in matches0)
                {
                    string[] words = match0.Value.Split(new char[] { '(', ')' });
                    //textBox3.Text += "_scob_: " + words[1] + "\r\n";
                    double a = func_v4(ref words[1], arg_value, arg_i_value);
                    if (words[1] == "")//если преобразование внутри скобок произошло без ошибок, то 
                    {
                        if (str.IndexOf(match0.Value) != 0)
                        {
                            if (str[str.IndexOf(match0.Value) - 1] == 'n' || str[str.IndexOf(match0.Value) - 1] == ']')
                                str = str.Replace(match0.Value, $"({a})");
                            else str = str.Replace(match0.Value, $"{a}");
                        }
                        else { str = str.Replace(match0.Value, $"{a}"); }
                    }
                    else
                    {
                        str = words[1];
                        return res;
                    }
                    

                    //textBox3.Text += "_scob_res_: " + str + "\r\n";
                }

                str = ln(str);
                str = log(str);
                matches0 = regex0.Matches(str);
                coll = matches0.Count;
            }


            //textBox3.Text += "_!scob_res!_: " + str + "\r\n";

            //str = ln(str);

            //str = log(str);

            str = exp(str);

            str = div(str);

            str = mul(str);

            str = plus_minus(str);


            // калькулятор
            //textBox3.Text += "Итого: " + str + "\r\n";
            //regex = new Regex(@"([+-])?([\w,\,]+)"); E-?\d+?
            Regex regex = new Regex(@"([+-])?((\d+\,?\d*)(E-?\d+)?)");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                //textBox3.Text += "_sum_: " + match.Value + "\r\n";
                res += Convert.ToDouble(match.Value);
                str = str.Remove(str.IndexOf(match.Value), match.Value.Length);
                //str = str.Replace(match.Value, "");
                //textBox3.Text += "res= " + res + "\r\n";
            }

            if (str != "")
            {
                //MessageBoxResult result = MessageBox.Show($"Необработанная ошибка ввода функции. Обратитесь к администратору! \"{str}\"", "Caution", MessageBoxButton.OK);
                //textBox3.Text += $"Необработанная ошибка ввода функции. Обратитесь к администратору! \"{str}\"";//, "Caution", MessageBoxButton.OK);
            }

            return res;
        }

        // метод преобразования строки в число с учетом операторов Sum и Mull и без скобок
        static double func_v4(ref string str, Dictionary<string, double> arg_value, List<Dictionary<string, double>> arg_i_value)
        {
            double res = 0;

            if (arg_i_value.Count != 0)
            {
                str = Sum(str, arg_i_value);
                str = Mull(str, arg_i_value);
            }

            //textBox3.Text += "_v4(sum+mul)!!_: " + str + "\r\n";

            str = ln(str);

            str = log(str);

            str = exp(str);

            str = div(str);

            str = mul(str);

            str = plus_minus(str);


            // калькулятор
            //textBox3.Text += "Итого:" + str + "\r\n";
            //regex = new Regex(@"([+-])?([\w,\,]+)"); E-?\d+?
            Regex regex = new Regex(@"([+-])?((\d+\,?\d*)(E-?\d+)?)");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                //textBox3.Text += "__" + match.Value + "\r\n";
                res += Convert.ToDouble(match.Value);
                str = str.Remove(str.IndexOf(match.Value), match.Value.Length);
                //str = str.Replace(match.Value, "");
                //textBox3.Text += "v4_res = " + res + "\r\n";
            }

            if (str != "")
            {
                //MessageBoxResult result = MessageBox.Show($"Необработанная ошибка ввода функции. Обратитесь к администратору! \"{str}\"", "Caution", MessageBoxButton.OK);
                //textBox3.Text += $"Необработанная ошибка ввода функции. Обратитесь к администратору! \"{str}\"";//, "Caution", MessageBoxButton.OK);
            }

            return res;
        }

        // метод преобразования строки в число без учета операторов Sum и Mull
        static double func_v3(string str, Dictionary<string, double> arg_value)
        {
            double res = 0;
            foreach (KeyValuePair<string, double> a in arg_value)
            {
                if (a.Key != "e" && a.Key != "E" && a.Key != "Pi")
                {
                    Regex regex1 = new Regex($@"([+,\-,\*,/,(,),\^]+|^){a.Key}");
                    MatchCollection matches1 = regex1.Matches(str);
                    foreach (Match match in matches1)
                    {
                        //textBox3.Text += "_!_" + match.Value + "\r\n";
                        Regex regex2 = new Regex($@"{a.Key}");
                        string result = regex2.Replace(match.Value, Convert.ToString(a.Value));
                        str = str.Replace(match.Value, result);
                    }
                }
                //str = str.Replace(a.Key, Convert.ToString(a.Value));
            }

            str = str.Replace("e", Convert.ToString(Exp(1)));
            str = str.Replace("E", "10");
            str = str.Replace("Pi", Convert.ToString(PI));

            //textBox3.Text += "_!scal_v3!_: " + str + "\r\n";



            str = ln(str);

            str = log(str);

            str = exp(str);

            //string[] words = str.Split('+');
            //foreach(string word in words)
            //{

            //}

            str = div(str);

            str = mul(str);

            str = plus_minus(str);


            // калькулятор
            //textBox3.Text += "Итого:" + str + "\r\n";
            //regex = new Regex(@"([+-])?([\w,\,]+)"); E-?\d+?
            Regex regex = new Regex(@"([+-])?((\d+\,?\d*)(E-?\d+)?)");
            MatchCollection matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                //textBox3.Text += "__" + match.Value + "\r\n";
                res += Convert.ToDouble(match.Value);
                str = str.Remove(0, match.Value.Length);
                //str = str.Replace(match.Value, "");
                //textBox3.Text += "v3_res = " + res + "\r\n";
                //textBox3.Text += str;
            }

            return res;
        }
    }

}
