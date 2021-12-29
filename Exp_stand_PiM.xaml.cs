using Npgsql;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Stand_PiM.xaml
    /// </summary>
    //комментарий
    public partial class Exp_stand_PiM : Page
    {
        String conn_str = User.Connection_string;
        List<table> docs = new List<table>();
        string sear = ""; //переменная для проверки есть ли привязанные стенды для выбранного объекта
        //static Experiment_add exp_add = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
        Experiment_search exp_search = (Experiment_search)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_search").FirstOrDefault();
        Experiment_add exp_add = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();
        string mode;
        string txt_stand_descr;
        public Exp_stand_PiM(string mode)
        {
            InitializeComponent();
            this.mode = mode;
            switch (mode)
            {
                case "add":
                    exp_add.Butt_next.IsEnabled = false;
                    break;
                case "search":
                    //exp_search.Butt_next.IsEnabled = false;
                    Butt_Save.Visibility = Visibility.Hidden;
                    page_name.Text = "3. Просмотр информации о стенде и ПиМ";
                    Combox_stand.IsEnabled = false;
                    Txt_namestand.IsEnabled = false;
                    Txt_stand.IsEnabled = false;
                    break;
            }
            docs.Add(new table("ПиМ", "ПиМ.pdf"));
            docs.Add(new table("Доп. док-ты", "ПиМ2.pdf"));

            dataGrid_docs.ItemsSource = docs;

            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            //проверка: есть ли привязанные стенды
            NpgsqlCommand comm_try = new NpgsqlCommand($"select \"Stand_id\",\"Id$\" from main_block.\"Stand_ID*\" where \"ID*\"={Data.id}", sqlconn);
            NpgsqlDataReader read_try = comm_try.ExecuteReader();
            while (read_try.Read())
            {
                sear = read_try[0].ToString();
                Data.id_obj = read_try[1].ToString();
            }
            read_try.Close();

            if (sear != "")  //если привязанный стенд есть, то считать информацию о нем в контролы
            {
                NpgsqlCommand comm_search = new NpgsqlCommand($"select \"name\",\"description\" from main_block.\"Stands\" where \"Stand_id\"={sear}", sqlconn);
                NpgsqlDataReader reader_search = comm_search.ExecuteReader();
                while (reader_search.Read())
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = $"{reader_search[0]}";
                    Combox_stand.Items.Add(item);
                    Combox_stand.SelectedItem = item;
                    txt_stand_descr = reader_search[1].ToString();
                    Txt_stand.Text = reader_search[1].ToString();
                }
                reader_search.Close();

                Txt_namestand.Text = Combox_stand.Text;

                switch (mode)
                {
                    case "add":
                        Txt_stand.IsEnabled = true;
                        Txt_namestand.IsEnabled = true;

                        NpgsqlCommand comm_nam = new NpgsqlCommand($"select \"name\" from main_block.\"Stands\" where \"Stand_id\"!={sear}", sqlconn);  //заполнить комбобокс остальными названиями стендов
                        NpgsqlDataReader reader_nam = comm_nam.ExecuteReader();
                        while (reader_nam.Read())
                        {
                            ComboBoxItem item = new ComboBoxItem();
                            item.Content = $"{reader_nam[0]}";
                            Combox_stand.Items.Add(item);
                        }
                        reader_nam.Close();

                        exp_add.Butt_next.IsEnabled = true;
                        exp_add.item3.IsEnabled = true;
                        bool_exp.stand = true;
                        break;
                    case "search":
                        //exp_search.Butt_next.IsEnabled = true;
                        //exp_add.item3.IsEnabled = true;
                        Txt_namestand.IsEnabled = false;
                        //Txt_stand.IsEnabled = false;
                        break;
                }
                //exp_add.Butt_next.IsEnabled = true;
                //exp_add.item3.IsEnabled = true;
                //bool_exp.stand = true;
            }
            else //если привязанных стендов нет, то считать все названия стендов в комбобокс
            {
                Data.id_obj = null;
                NpgsqlCommand comm_nam = new NpgsqlCommand("select \"name\" from main_block.\"Stands\"", sqlconn);
                NpgsqlDataReader reader_nam = comm_nam.ExecuteReader();
                //заполнение комбобокса со стендами
                while (reader_nam.Read())
                {
                    ComboBoxItem item = new ComboBoxItem();
                    item.Content = $"{reader_nam[0]}";
                    //item.Background = Brushes.LightCyan;
                    Combox_stand.Items.Add(item);
                }
                reader_nam.Close();
            }

            sqlconn.Close();

        }

        private void Combox_stand_SelectionChanged(object sender, SelectionChangedEventArgs e)  //изменение выбранного стенда в комбобоксе
        {
            Txt_namestand.IsEnabled = true;
            Txt_stand.IsEnabled = true;
            ComboBoxItem comitem = new ComboBoxItem();
            comitem = (ComboBoxItem)Combox_stand.SelectedItem;
            if (comitem != null)
            {
                Txt_namestand.Text = comitem.Content.ToString();
                if (comitem.Content.ToString() == "Новый стенд")
                {
                    Txt_stand.Text = "";
                }
                else
                {
                    NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                    sqlconn.Open();
                    NpgsqlCommand comm_desc = new NpgsqlCommand($"select \"description\" from main_block.\"Stands\" where \"name\"='{comitem.Content}'", sqlconn);
                    Txt_stand.Text = comm_desc.ExecuteScalar().ToString();
                    sqlconn.Close();
                }

            }
            switch (mode)
            {
                case "add":
                    exp_add.Butt_next.IsEnabled = false;
                    exp_add.item3.IsEnabled = false;
                    bool_exp.stand = true;
                    break;
                case "search":
                    //exp_search.Butt_next.IsEnabled = false;
                    break;
            }
            //exp_add.Butt_next.IsEnabled = false;
            //exp_add.item3.IsEnabled = false;
            //bool_exp.stand = true;
        }

        private void Butt_Save_Click(object sender, RoutedEventArgs e) //сохранить изменения
        {
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();
            if (sear == "") //если привязанных стендов нет (нет id$)
            {
                if (Combox_stand.Text == "Новый стенд") //добавление нового стенда
                {
                    stands.ins_stan_ins_id(Data.id, Txt_namestand.Text, Txt_stand.Text);
                }
                else
                {
                    stands.upd_stan_ins_id(Data.id, Combox_stand.Text, Txt_namestand.Text, Txt_stand.Text);
                }
            }
            else //если привязанные стенды есть (есть id$)
            {
                if (Combox_stand.Text == "Новый стенд")
                {
                    stands.ins_stan_upd_id(Data.id, Txt_namestand.Text, Txt_stand.Text);
                }
                else
                {
                    stands.upd_stan_upd_id(Data.id, Combox_stand.Text, Txt_namestand.Text, Txt_stand.Text);
                }
            }
            if (stands.flag)
            {
                NpgsqlCommand comm_try = new NpgsqlCommand($"select \"Id$\" from main_block.\"Stand_ID*\" where \"ID*\"={Data.id}", sqlconn);
                Data.id_obj = comm_try.ExecuteScalar().ToString();
                messtxt.Text = "Сохранение данных произошло успешно!";
                messbar.IsActive = true;
                exp_add.Butt_next.IsEnabled = true;
                bool_exp.stand = true;
                exp_add.item3.IsEnabled = true;
            }
            else
            {
                messtxt.Text = "Произошла ошибка! Данные не сохранены.";
                messbar.IsActive = true;
            }
        }
        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
        }

        private void Txt_stand_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mode == "search")
            {
                Txt_stand.Text = txt_stand_descr;
            }
        }
    }
    public struct table
    {
        public string doc { get; set; }
        public string file { get; set; }

        public table(string v1, string v2)
        {
            this.doc = v1;
            this.file = v2;
        }
    }

    public class stands
    {
        static String conn_str = User.Connection_string;
        public static bool flag;

        public static void ins_stan_ins_id(string id_, string st_name, string st_desc) //добавление нового стенда и нового id
        {
            try
            {
                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                NpgsqlCommand comm_ins_stan = new NpgsqlCommand($"insert into main_block.\"Stands\"(\"name\", \"description\") values ('{st_name}', '{st_desc}');", sqlconn);  //добавление стенда
                comm_ins_stan.ExecuteNonQuery();

                NpgsqlCommand comm_ins_id = new NpgsqlCommand($"insert into main_block.\"Stand_ID*\"(\"ID*\",\"Stand_id\") values ({id_}, (select \"Stand_id\" from main_block.\"Stands\" where \"name\"='{st_name}')); ", sqlconn);  //добавление id
                comm_ins_id.ExecuteNonQuery();
                sqlconn.Close();
                flag = true;
            }
            catch
            {
                flag = false;
            }
        }

        public static void ins_stan_upd_id(string id_, string st_name, string st_desc) //добавление нового стенда и обновление id
        {
            try
            {
                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                NpgsqlCommand comm_ins_stan = new NpgsqlCommand($"insert into main_block.\"Stands\"(\"name\", \"description\") values ('{st_name}', '{st_desc}');", sqlconn);  //добавление стенда
                comm_ins_stan.ExecuteNonQuery();

                NpgsqlCommand comm_upd_id = new NpgsqlCommand($"update main_block.\"Stand_ID*\" set \"Stand_id\" = (select \"Stand_id\" from main_block.\"Stands\" where \"name\" = '{st_name}') where \"ID*\" = {id_};", sqlconn);  //обновление id
                comm_upd_id.ExecuteNonQuery();
                sqlconn.Close();
                flag = true;
            }
            catch
            {
                flag = false;
            }

        }

        public static void upd_stan_ins_id(string id_, string st_old_name, string st_new_name, string st_desc) //обновление стенда и добавление нового id
        {
            try
            {
                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                NpgsqlCommand comm_upd_stan = new NpgsqlCommand($"update main_block.\"Stands\" set \"name\" = '{st_new_name}', \"description\" = '{st_desc}' where \"name\"='{st_old_name}'", sqlconn);  //обновление стенда
                comm_upd_stan.ExecuteNonQuery();

                NpgsqlCommand comm_ins_id = new NpgsqlCommand($"insert into main_block.\"Stand_ID*\"(\"ID*\",\"Stand_id\") values ({id_}, (select \"Stand_id\" from main_block.\"Stands\" where \"name\"='{st_new_name}')); ", sqlconn);  //добавление id
                comm_ins_id.ExecuteNonQuery();
                sqlconn.Close();
                flag = true;
            }
            catch
            {
                flag = false;
            }

        }

        public static void upd_stan_upd_id(string id_, string st_old_name, string st_new_name, string st_desc) //обновление стенда и id
        {
            try
            {
                NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                sqlconn.Open();

                NpgsqlCommand comm_upd_stan = new NpgsqlCommand($"update main_block.\"Stands\" set \"name\" = '{st_new_name}', \"description\" = '{st_desc}' where \"name\"='{st_old_name}'", sqlconn);  //обновление стенда
                comm_upd_stan.ExecuteNonQuery();

                NpgsqlCommand comm_upd_id = new NpgsqlCommand($"update main_block.\"Stand_ID*\" set \"Stand_id\" = (select \"Stand_id\" from main_block.\"Stands\" where \"name\" = '{st_new_name}') where \"ID*\" = {id_}; ", sqlconn);  //добавление id
                comm_upd_id.ExecuteNonQuery();
                sqlconn.Close();
                flag = true;
            }
            catch
            {
                flag = false;
            }

        }

    }
}
