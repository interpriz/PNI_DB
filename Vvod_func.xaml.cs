using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using static System.Math;
using System.Text.RegularExpressions;

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Calculator.xaml
    /// </summary>




    public partial class Vvod_func : Window
    {
        List<string> bufer = new List<string>();

        public static Dictionary<string, Param> arguments { get; set; }

        public static Dictionary<string, double> arg_value { get; set; }

        public static List<Dictionary<string, double>> arg_i_value { get; set; }

        public static parametr par { get; set; }

        public static string name_par { get; set; }

        public static string work_mode { get; set; }

        public Vvod_func()
        {
            InitializeComponent();
            foreach (UIElement el in Claviatura.Children)
            {
                if (el is Button)
                {
                    ((Button)el).Click += Button_Click;
                }
            }

            foreach (KeyValuePair<string, Param> a in arguments)
            {
                args.Items.Add($"{a.Value.short_name}| {a.Key}, {a.Value.unit}");
            }
            short_name.Text = $"{Parametrs.get_param(name_par).short_name} = ";
            pole_vvvoda.Text = par.function_value;
            textBlock.Text += $"{name_par}, {Parametrs.get_param(name_par).unit}"; 
        } 

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int k = pole_vvvoda.SelectionStart; // положение каретки
            string str = (string)((Button)e.OriginalSource).Content;
            switch (str)
            {
                case "log[]()":
                case "ln()":
                case "Pi":
                case "e":
                case "E":
                case "+":
                case "-":
                case "*":
                case "/":
                case "Mull{}":
                case "Sum{}":
                case "()":
                    bufer.Add(pole_vvvoda.Text);
                    pole_vvvoda.Text = pole_vvvoda.Text.Insert(k, str);
                    pole_vvvoda.SelectionStart = k + str.Length;
                    ; break;
                case "x^n":
                    bufer.Add(pole_vvvoda.Text);
                    pole_vvvoda.Text = pole_vvvoda.Text.Insert(k, "^");
                    pole_vvvoda.SelectionStart = k + str.Length;
                    break;
                case "Аргумент":
                    bufer.Add(pole_vvvoda.Text);
                    break;
            }
        }

        private void but_clear_Click(object sender, EventArgs e)//очистить
        {
            bufer.Add(pole_vvvoda.Text);
            pole_vvvoda.Clear();
        }

        private void but_prev_Click(object sender, EventArgs e)//предыдущий шаг
        {
            if (bufer.Count != 0)
            {
                pole_vvvoda.Text = bufer[bufer.Count - 1];
                bufer.RemoveAt(bufer.Count - 1);
            }
        }

        private void but_arg_Click(object sender, RoutedEventArgs e)//аргумент
        {
            bufer.Add(pole_vvvoda.Text);
            int k = pole_vvvoda.SelectionStart; // положение каретки
            if (args.SelectedItem != null)
                pole_vvvoda.Text = pole_vvvoda.Text.Insert(k, args.SelectedItem.ToString().Split('|')[0]);
            pole_vvvoda.SelectionStart = k + args.SelectedItem.ToString().Split('|')[0].Length;
        }

        private void but_submit_Click(object sender, RoutedEventArgs e)//принять
        {
            if (pole_vvvoda.Text.Length != 0)
            {
                string str = pole_vvvoda.Text;

                Regex regex1 = new Regex(@"\(");
                MatchCollection matches1 = regex1.Matches(str);
                Regex regex2 = new Regex(@"\)");
                MatchCollection matches2 = regex2.Matches(str);

                Regex regex3 = new Regex(@"\[");
                MatchCollection matches3 = regex3.Matches(str);
                Regex regex4 = new Regex(@"\]");
                MatchCollection matches4 = regex4.Matches(str);

                Regex regex5 = new Regex(@"\{");
                MatchCollection matches5 = regex5.Matches(str);
                Regex regex6 = new Regex(@"\}");
                MatchCollection matches6 = regex6.Matches(str);

                bool f1 = (matches1.Count == matches2.Count);
                if (matches1.Count == 0) f1 = true;
                bool f2 = (matches3.Count == matches4.Count);
                if (matches3.Count == 0) f2 = true;
                bool f3 = (matches5.Count == matches6.Count);
                if (matches5.Count == 0) f3 = true;

                if (f1 && f2 && f3) // проверка на количество открытых и закрытых скобок
                {
                    /*
                    string str1 = Calculator.chek(str);
                    if (str1 == "")// проверка общего выражения
                    {
                        // проверки операторов суммы и умножения
                        string str2 = "";
                        //Sum{ a_i+b_i} - сумма
                        Regex regex = new Regex(@"Sum\{(\S+)\}");
                        MatchCollection matches = regex.Matches(str);
                        foreach (Match match in matches)
                        {
                            string[] words = match.Value.Split(new char[] { '{', '}' });
                            words[1] = Calculator.chek(words[1]);
                            if (words[1] != "")
                            {
                                str2 += $"\"{words[1]}\"";
                                break;
                            }
                        }

                        //Mul{ a_i+b_i} - произведение
                        regex = new Regex(@"Mul\{(\S+)\}");
                        matches = regex.Matches(str);
                        foreach (Match match in matches)
                        {
                            string[] words = match.Value.Split(new char[] { '{', '}' });
                            words[1] = Calculator.chek(words[1]);
                            if (words[1] != "")
                            {
                                str2 += $"\"{words[1]}\"";
                                break;
                            }
                        }
                        if (str2 == "")
                        {
                            par.function_value = pole_vvvoda.Text;
                            par.func_to_value(arg_value, arg_i_value);
                            Experiment_add exp_wind = (Experiment_add)Application.Current.Windows[4];
                            exp_wind.item6.IsSelected = false;
                            exp_wind.item6.IsSelected = true;
                            this.Close();

                        }
                        else
                        {
                            messtxt.Text = $"Функция введена неверно! Проверте \"{str2}\" !";
                            messbar.IsActive = true;
                        }
                    }
                    else
                    {
                        messtxt.Text = $"Функция введена неверно! Проверте \"{str1}\" !";
                        messbar.IsActive = true;
                    }
                    */
                    par.function_value = pole_vvvoda.Text;
                    string check = par.func_to_value(arg_value, arg_i_value);
                    if (check == "")
                    {
                        if(par.mode == "old") par.mode = "update";
                        switch (work_mode)
                        {
                            case "experiment":
                                Experiment_add exp_wind = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
                                //Experiment_add exp_wind = (Experiment_add)Application.Current.Windows[4];
                                exp_wind.item6.IsSelected = false;
                                exp_wind.item6.IsSelected = true;
                                break;

                            case "modeling":
                                Modeling_add modeling_wind = (Modeling_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault();
                                modeling_wind.new_Model_Obrabotka.update_table();
                                break;
                        }
                        
                        this.Close();
                    }
                    else
                    {
                        messtxt.Text = $"Ошибка ввода функции! : \"{check}\" \r\nПроверьте правильность ввода и попробуйте снова!";
                        messbar.IsActive = true;
                    }
                    
                }
                else
                {
                    if (!f1)
                    {
                        messtxt.Text = $" Разное количество открывающих и закрывающих скобок \"()\"";
                        messbar.IsActive = true;
                    }
                    if (!f2)
                    {
                        messtxt.Text = $" Разное количество открывающих и закрывающих скобок \"[]\"";
                        messbar.IsActive = true;
                    }
                    if (!f3)
                    {
                        messtxt.Text = " Разное количество открывающих и закрывающих скобок \"{}\"";
                        messbar.IsActive = true;
                    }
                }
            }
        }



        private void esc_Click(object sender, RoutedEventArgs e)// отмена
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            switch (work_mode)
            {
                case "experiment":
                    Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault().Show();
                    break;

                case "modeling":
                    Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Modeling_wind").FirstOrDefault().Show();
                    break;
            }
        }

        private void args_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (args.SelectedItem != null)
            {
                string str = (string)args.SelectedItem;
                args.Text = str.Split('|')[0];
            }
        }

        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

        private void pole_vvvoda_KeyDown(object sender, KeyEventArgs e)
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
               || (e.Key.ToString() == "OemComma"))
            {
                e.Handled = false;
                //return;
            }
            else
                e.Handled = true;
        }
    }
}
