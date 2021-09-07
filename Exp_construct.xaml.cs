using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Логика взаимодействия для Exp_construct.xaml
    /// </summary>
    /// 


    public partial class Exp_construct : Page
    {

        //public static ObservableCollection<Construct> constr = new ObservableCollection<Construct>();
        string conn_str = User.Connection_string;
        int chan = 0;
        int sech2 = 0;
        int sech6 = 0;
        int sech7 = 0;

        Experiment_add exp_wind = (Experiment_add)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Experiment_wind").FirstOrDefault();

        bool flag_sys_coord = true; //флаг для отмены выбора системы координат, если значение есть в базе

        public Exp_construct()
        {
            Data.chans_results = new List<Results_of_fiz_exp>();
            Data.chans_obr = new List<Obrabotka_of_fiz_exp>();

            InitializeComponent();
            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            NpgsqlCommand comm_chan_dann = new NpgsqlCommand($"select distinct rc.\"Channel\", mcs.\"Id_R_C\" from main_block.\"Mode_cros_section\" mcs right join main_block.\"Realization_channel\" rc on mcs.\"Id_R_C\"=rc.\"Id_R_C\" where rc.\"Realization\" = {Data.current_realization} and \"Id$\"={Data.id_obj} order by rc.\"Channel\";", sqlconn);
            NpgsqlDataReader rdr_chan_dann = comm_chan_dann.ExecuteReader();
            while (rdr_chan_dann.Read())
            {
                if (rdr_chan_dann[1].ToString() != "")
                {
                    string chan_d = rdr_chan_dann[0].ToString();
                    DB_constr.chan_dann.Add(chan_d);
                }
                else
                {
                    string chan_d = rdr_chan_dann[1].ToString();
                    //chan_d[1] = rdr_chan_dann[1].ToString();
                    DB_constr.chan_dann.Add(chan_d);
                }
            }
            rdr_chan_dann.Close();

            NpgsqlCommand comm_chan_obr = new NpgsqlCommand($"select \"Channel\" , count(\"id_obr0/rez1\") from (select distinct rc.\"Channel\", mcs.\"Id_R_C\", ve.\"id_obr0/rez1\" from " +
                "main_block.\"Mode_cros_section\" mcs right join main_block.\"Realization_channel\" rc on mcs.\"Id_R_C\" = rc.\"Id_R_C\" " +
                "left join main_block.\"Values_experiment\" ve on mcs.\"Id_m_c\" = ve.\"Id_m_c\" " +
                $"where  rc.\"Realization\" = {Data.current_realization} and \"Id$\" = {Data.id_obj} order by rc.\"Channel\") s " +
                "group by \"Channel\"; ", sqlconn);
            NpgsqlDataReader rdr_chan_obr = comm_chan_obr.ExecuteReader();
            while (rdr_chan_obr.Read())
            {
                DB_constr.chan_obr.Add(Convert.ToInt32(rdr_chan_obr[1])); //есть или нет обработанные результаты (2 - есть, 1 - нет обработанных, 0 - нет результатов вообще)
            }
            rdr_chan_obr.Close();


            Construct const_chan1 = new Construct();
            const_chan1.sechen = new ObservableCollection<string>();
            const_chan1.travers = new ObservableCollection<Traversir>();
            const_chan1.params_sech = new ObservableCollection<Parametrs_sech_6>();

            const_chan1.rezh_all_6 = new ObservableCollection<Rezh>();
            const_chan1.rezh_izm_6 = new ObservableCollection<Rezh>();

            const_chan1.proch_all = new ObservableCollection<Proch>();
            const_chan1.proch_izm = new ObservableCollection<Proch>();

            const_chan1.obr_params_sech = new ObservableCollection<Obr_parametrs_sech_7>();

            const_chan1.rezh_all_7 = new ObservableCollection<Rezh>();
            const_chan1.rezh_obr_7 = new ObservableCollection<Rezh>();

            const_chan1.integr_all = new ObservableCollection<Integr>();
            const_chan1.integr_obr = new ObservableCollection<Integr>();

            if (DB_constr.chan_dann[0] == "")   //если данных в базе нет
            {
                NpgsqlCommand comm_rezh1_6 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные параметры для канала 1 (пункт 6+7)
                NpgsqlDataReader rdr_rezh1_6 = comm_rezh1_6.ExecuteReader();
                Parametrs.reg_pars = new Dictionary<string, Param>();
                while (rdr_rezh1_6.Read())
                {
                    Rezh str = new Rezh();
                    str.rezh = rdr_rezh1_6[0].ToString();
                    const_chan1.rezh_all_6.Add(str);
                    const_chan1.rezh_all_7.Add(str);

                    Param p = new Param() { short_name = rdr_rezh1_6[1].ToString(), unit = rdr_rezh1_6[2].ToString() };
                    Parametrs.reg_pars.Add(rdr_rezh1_6[0].ToString(), p);
                }
                rdr_rezh1_6.Close();



                NpgsqlCommand comm_proch1 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);    //все прочие параметры для канала 1 (пункт 6)
                NpgsqlDataReader rdr_proch1 = comm_proch1.ExecuteReader();
                Parametrs.phys_pars = new Dictionary<string, Param>();
                while (rdr_proch1.Read())
                {
                    if (rdr_proch1[0].ToString() != "Среда")
                    {
                        Proch str = new Proch();
                        str.proch = rdr_proch1[0].ToString();
                        const_chan1.proch_all.Add(str);
                    }
                    Param p = new Param() { short_name = rdr_proch1[1].ToString(), unit = rdr_proch1[2].ToString() };
                    Parametrs.phys_pars.Add(rdr_proch1[0].ToString(), p);

                }
                rdr_proch1.Close();

                NpgsqlCommand comm_integr1 = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса');", sqlconn);    //все Интегральные характеристики физического процесса для канала 1 (пункт 7)
                NpgsqlDataReader rdr_integr1 = comm_integr1.ExecuteReader();
                Parametrs.integral_pars = new Dictionary<string, Param>();
                while (rdr_integr1.Read())
                {
                    Integr str = new Integr();
                    str.integr = rdr_integr1[0].ToString();
                    const_chan1.integr_all.Add(str);

                    Param p = new Param() { short_name = rdr_integr1[1].ToString(), unit = rdr_integr1[2].ToString() };
                    Parametrs.integral_pars.Add(rdr_integr1[0].ToString(), p);
                }
                rdr_integr1.Close();


                DB_constr.sech_count_db.Add(0);//число сечений, которые есть в базе
                Data.chans_results.Add(new Results_of_fiz_exp(false, 1));
                Data.chans_obr.Add(new Obrabotka_of_fiz_exp(false, 1));

                List<Traversir> trav_lst_for_db_constr = new List<Traversir>();
                //Traversir sechenie = new Traversir();
                //trav_lst_for_db_constr.Add(sechenie);
                DB_constr.chans_trav_db.Add(trav_lst_for_db_constr);//список списков сечений с точками траверсирования для каждого канала, которые хранятся в базе (если в базе пусто, то список пустой)

                DB_constr.rezh_count.Add(0);    //число режимов, которые есть в базе

                List<List<Phys>> sech_ph_6_db = new List<List<Phys>>();//список по сечениям списков параметров
                DB_constr.phys_6_db.Add(sech_ph_6_db);
                DB_constr.phys_7_db.Add(sech_ph_6_db);

                List<Rezh> rezhs_6 = new List<Rezh>();
                DB_constr.rezhs_6_db.Add(rezhs_6);
                DB_constr.rezhs_7_db.Add(rezhs_6);

                List<Proch> proches_6 = new List<Proch>();
                DB_constr.proches_6_db.Add(proches_6);

                List<Integr> integrs_7 = new List<Integr>();
                DB_constr.integrs_7_db.Add(integrs_7);
            }
            else
            {
                Data.chans_results.Add(new Results_of_fiz_exp(true, 1));
                //Data.chans_obr.Add(new Obrabotka_of_fiz_exp(true));


                foreach (section sec in Data.chans_results[0].sections)//есть траверсирвоание или нет
                {
                    if (sec.coordinates != null)
                    {
                        const_chan1.yes_no = true;
                    }
                }

                const_chan1.count_sech = Data.chans_results[0].sections.Count.ToString();//число сечений
                DB_constr.sech_count_db.Add(Data.chans_results[0].sections.Count);

                List<List<Phys>> sech_ph_6_db = new List<List<Phys>>();//список по сечениям списков параметров

                List<Traversir> trav_lst_for_db_constr = new List<Traversir>();
                for (int i = 0; i < Data.chans_results[0].sections.Count; i++)
                {
                    Traversir sechenie = new Traversir();

                    List<Phys> ph_6_db = new List<Phys>();

                    if (Data.chans_results[0].sections[i].pars == null)
                    {
                        sechenie.count = Data.chans_results[0].sections[i].coordinates.travers_table[0].cols[0].Count.ToString();
                        ObservableCollection<string> coord = Data.chans_results[0].sections[i].coordinates.column_headers;
                        if ((coord[0] == "X") || (coord[1] == "Y"))
                        {
                            sechenie.sys_coord = "Декартова";
                        }
                        if ((coord[0] == "a") || (coord[1] == "R"))
                        {
                            sechenie.sys_coord = "Сферическая";
                        }

                        Parametrs_sech_6 param = new Parametrs_sech_6();
                        param.phys_all_6 = new ObservableCollection<Phys>();
                        param.phys_izm_6 = new ObservableCollection<Phys>();
                        for (int j = 0; j < Data.chans_results[0].sections[i].trav_pars.column_headers.Count; j++)
                        {
                            Phys str = new Phys();
                            str.phys = Data.chans_results[0].sections[i].trav_pars.column_headers[j];
                            param.phys_izm_6.Add(str);
                            ph_6_db.Add(str);   //список физических параметров, которые есть в БД
                        }

                        NpgsqlCommand comm_phys = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);
                        NpgsqlDataReader rdr_phys = comm_phys.ExecuteReader();
                        while (rdr_phys.Read())
                        {
                            if (rdr_phys[0].ToString() != "Среда")
                            {
                                bool fl = true;
                                for (int q = 0; q < param.phys_izm_6.Count; q++)
                                {
                                    if (rdr_phys[0].ToString() == param.phys_izm_6[q].phys)
                                    {
                                        fl = false;
                                        q = param.phys_izm_6.Count;
                                    }
                                }
                                if (fl)
                                {
                                    Phys str = new Phys();
                                    str.phys = rdr_phys[0].ToString();
                                    param.phys_all_6.Add(str);
                                }
                            }

                        }
                        rdr_phys.Close();
                        const_chan1.params_sech.Add(param);
                    }
                    else
                    {
                        Parametrs_sech_6 param = new Parametrs_sech_6();
                        param.phys_all_6 = new ObservableCollection<Phys>();
                        param.phys_izm_6 = new ObservableCollection<Phys>();

                        for (int j = 0; j < Data.chans_results[0].sections[i].pars.column_headers.Count; j++)
                        {
                            Phys str = new Phys();
                            str.phys = Data.chans_results[0].sections[i].pars.column_headers[j];
                            param.phys_izm_6.Add(str);
                            ph_6_db.Add(str);   //список физических параметров, которые есть в БД
                        }

                        NpgsqlCommand comm_phys = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);
                        NpgsqlDataReader rdr_phys = comm_phys.ExecuteReader();
                        while (rdr_phys.Read())
                        {
                            if (rdr_phys[0].ToString() != "Среда")
                            {
                                bool fl = true;
                                for (int q = 0; q < param.phys_izm_6.Count; q++)
                                {
                                    if (rdr_phys[0].ToString() == param.phys_izm_6[q].phys)
                                    {
                                        fl = false;
                                        q = param.phys_izm_6.Count;
                                    }
                                }
                                if (fl)
                                {
                                    Phys str = new Phys();
                                    str.phys = rdr_phys[0].ToString();
                                    param.phys_all_6.Add(str);
                                }
                            }

                        }
                        rdr_phys.Close();

                        const_chan1.params_sech.Add(param);
                    }
                    int s = i + 1;
                    const_chan1.sechen.Add(s.ToString());
                    const_chan1.travers.Add(sechenie);

                    Traversir sech_db = new Traversir();
                    sech_db.count = sechenie.count;
                    sech_db.sys_coord = sechenie.sys_coord;
                    trav_lst_for_db_constr.Add(sech_db);

                    sech_ph_6_db.Add(ph_6_db);//список по сечениям со списками параметров
                }
                DB_constr.chans_trav_db.Add(trav_lst_for_db_constr);//список списков сечений с точками траверсирования для каждого канала, которые хранятся в базе (если в базе пусто, то список пустой)

                DB_constr.phys_6_db.Add(sech_ph_6_db);  //список по каналам со списками по сечениям списков параметров

                const_chan1.rezh_count = Data.chans_results[0].rezh_num.Count.ToString();//число режимов
                DB_constr.rezh_count.Add(Data.chans_results[0].rezh_num.Count);    //число режимов, которые есть в базе

                const_chan1.sreda = Data.chans_results[0].sreda[0].value;
                combox_sreda.SelectedItem = const_chan1.sreda;

                List<Rezh> rezhs_6 = new List<Rezh>();  //список режимных параметров

                for (int k = 0; k < Data.chans_results[0].rezh_par.column_headers.Count; k++)
                {
                    Rezh str = new Rezh();
                    str.rezh = Data.chans_results[0].rezh_par.column_headers[k];
                    const_chan1.rezh_izm_6.Add(str);
                    rezhs_6.Add(str);
                }
                DB_constr.rezhs_6_db.Add(rezhs_6);  //список по каналам со списками режимных параметров

                NpgsqlCommand comm_rezh_6 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные измеряемые параметры (пункт 6)
                NpgsqlDataReader rdr_rezh_6 = comm_rezh_6.ExecuteReader();
                Parametrs.reg_pars = new Dictionary<string, Param>();
                while (rdr_rezh_6.Read())
                {
                    bool fl = true;
                    for (int q = 0; q < const_chan1.rezh_izm_6.Count; q++)
                    {
                        if (const_chan1.rezh_izm_6[q].rezh == rdr_rezh_6[0].ToString())
                        {
                            fl = false;
                            q = const_chan1.rezh_izm_6.Count;
                        }
                    }
                    if (fl)
                    {
                        Rezh str = new Rezh();
                        str.rezh = rdr_rezh_6[0].ToString();
                        const_chan1.rezh_all_6.Add(str);
                    }

                    Param p = new Param() { short_name = rdr_rezh_6[1].ToString(), unit = rdr_rezh_6[2].ToString() };
                    Parametrs.reg_pars.Add(rdr_rezh_6[0].ToString(), p);

                }
                rdr_rezh_6.Close();

                List<Proch> proches_6 = new List<Proch>();

                for (int k = 0; k < Data.chans_results[0].prochie_par.column_headers.Count; k++)
                {
                    Proch str = new Proch();
                    str.proch = Data.chans_results[0].prochie_par.column_headers[k];
                    const_chan1.proch_izm.Add(str);
                    proches_6.Add(str);
                }
                DB_constr.proches_6_db.Add(proches_6);

                NpgsqlCommand comm_proch_6 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);    //все прочие измеряемые параметры (пункт 6)
                NpgsqlDataReader rdr_proch_6 = comm_proch_6.ExecuteReader();
                Parametrs.phys_pars = new Dictionary<string, Param>();
                while (rdr_proch_6.Read())
                {
                    if (rdr_proch_6[0].ToString() != "Среда")
                    {
                        bool fl = true;
                        for (int q = 0; q < const_chan1.proch_izm.Count; q++)
                        {
                            if (const_chan1.proch_izm[q].proch == rdr_proch_6[0].ToString())
                            {
                                fl = false;
                                q = const_chan1.proch_izm.Count;
                            }

                        }
                        if (fl)
                        {
                            Proch str = new Proch();
                            str.proch = rdr_proch_6[0].ToString();
                            const_chan1.proch_all.Add(str);
                        }
                    }
                    Param p = new Param() { short_name = rdr_proch_6[1].ToString(), unit = rdr_proch_6[2].ToString() };
                    Parametrs.phys_pars.Add(rdr_proch_6[0].ToString(), p);

                }
                rdr_proch_6.Close();

                List<List<Phys>> sech_ph_7_db = new List<List<Phys>>();  //список физических параметров по сечениям
                List<Rezh> rezhs_7 = new List<Rezh>();
                List<Integr> integrs_7 = new List<Integr>();

                if (DB_constr.chan_obr[0] == 1) //обработанных результатов нет
                {
                    for (int i = 0; i < Convert.ToInt32(const_chan1.count_sech); i++)
                    {
                        Obr_parametrs_sech_7 obr_param = new Obr_parametrs_sech_7();
                        obr_param.phys_all_7 = new ObservableCollection<Phys>();
                        obr_param.phys_obr_7 = new ObservableCollection<Phys>();

                        NpgsqlCommand comm_phys_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);    //все теплофизические параметры (пункт 7)
                        NpgsqlDataReader rdr_phys_7 = comm_phys_7.ExecuteReader();
                        while (rdr_phys_7.Read())
                        {
                            if (rdr_phys_7[0].ToString() != "Среда")
                            {
                                Phys str = new Phys();
                                str.phys = rdr_phys_7[0].ToString();
                                obr_param.phys_all_7.Add(str);
                            }
                        }
                        rdr_phys_7.Close();
                        const_chan1.obr_params_sech.Add(obr_param);
                    }


                    NpgsqlCommand comm_rezh1_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные параметры для канала 1 (пункт 7)
                    NpgsqlDataReader rdr_rezh1_7 = comm_rezh1_7.ExecuteReader();
                    while (rdr_rezh1_7.Read())
                    {
                        Rezh str = new Rezh();
                        str.rezh = rdr_rezh1_7[0].ToString();
                        const_chan1.rezh_all_7.Add(str);
                    }
                    rdr_rezh1_7.Close();

                    NpgsqlCommand comm_integr1 = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса');", sqlconn);    //все Интегральные характеристики физического процесса для канала 1 (пункт 7)
                    NpgsqlDataReader rdr_integr1 = comm_integr1.ExecuteReader();
                    Parametrs.integral_pars = new Dictionary<string, Param>();
                    while (rdr_integr1.Read())
                    {
                        Integr str = new Integr();
                        str.integr = rdr_integr1[0].ToString();
                        const_chan1.integr_all.Add(str);

                        Param p = new Param() { short_name = rdr_integr1[1].ToString(), unit = rdr_integr1[2].ToString() };
                        Parametrs.integral_pars.Add(rdr_integr1[0].ToString(), p);
                    }
                    rdr_integr1.Close();


                    Data.chans_obr.Add(new Obrabotka_of_fiz_exp(false, 1));
                }
                else if (DB_constr.chan_obr[0] == 2)    //обработанные результаты есть
                {
                    Data.chans_obr.Add(new Obrabotka_of_fiz_exp(true, 1));

                    for (int i = 0; i < Convert.ToInt32(const_chan1.count_sech)  ; i++)   //считывание физических параметров в сечениях
                    {
                        Obr_parametrs_sech_7 param = new Obr_parametrs_sech_7();
                        param.phys_all_7 = new ObservableCollection<Phys>();
                        param.phys_obr_7 = new ObservableCollection<Phys>();

                        List<Phys> ph_7_db = new List<Phys>();  //список физических параметров

                        if (i < Data.chans_obr[0].sections.Count)
                        {
                            for (int j = 0; j < Data.chans_obr[0].sections[i].pars.column_headers.Count; j++)   //заполнение списка обрабатываемых параметров
                            {
                                Phys str = new Phys();
                                str.phys = Data.chans_obr[0].sections[i].pars.column_headers[j];
                                param.phys_obr_7.Add(str);
                                ph_7_db.Add(str);
                            }
                        }
                        
                        NpgsqlCommand comm_phys_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn); //все физические параметры (пункт 7)
                        NpgsqlDataReader rdr_phys_7 = comm_phys_7.ExecuteReader();
                        while (rdr_phys_7.Read())
                        {
                            if (rdr_phys_7[0].ToString() != "Среда")
                            {
                                bool fl = true;
                                for (int q = 0; q < param.phys_obr_7.Count; q++)
                                {
                                    if (rdr_phys_7[0].ToString() == param.phys_obr_7[q].phys)
                                    {
                                        fl = false;
                                        q = param.phys_obr_7.Count;
                                    }
                                }
                                if (fl)
                                {
                                    Phys str = new Phys();
                                    str.phys = rdr_phys_7[0].ToString();
                                    param.phys_all_7.Add(str);
                                }
                            }

                        }
                        rdr_phys_7.Close();

                        const_chan1.obr_params_sech.Add(param);
                        sech_ph_7_db.Add(ph_7_db);
                    }

                    for (int k = 0; k < Data.chans_obr[0].rezh_par.column_headers.Count; k++)   //заполнение обрабатываемых режимных параметров
                    {
                        Rezh str = new Rezh();
                        str.rezh = Data.chans_obr[0].rezh_par.column_headers[k];
                        const_chan1.rezh_obr_7.Add(str);
                        rezhs_7.Add(str);
                    }

                    NpgsqlCommand comm_rezh_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные параметры (пункт 7)
                    NpgsqlDataReader rdr_rezh_7 = comm_rezh_7.ExecuteReader();
                    while (rdr_rezh_7.Read())
                    {
                        bool fl = true;
                        for (int q = 0; q < const_chan1.rezh_obr_7.Count; q++)
                        {
                            if (const_chan1.rezh_obr_7[q].rezh == rdr_rezh_7[0].ToString())
                            {
                                fl = false;
                                q = const_chan1.rezh_obr_7.Count;
                            }
                        }
                        if (fl)
                        {
                            Rezh str = new Rezh();
                            str.rezh = rdr_rezh_7[0].ToString();
                            const_chan1.rezh_all_7.Add(str);
                        }

                    }
                    rdr_rezh_7.Close();

                    for (int k = 0; k < Data.chans_obr[0].integr_par.column_headers.Count; k++)   //заполнение обрабатываемых интегральных параметров
                    {
                        Integr str = new Integr();
                        str.integr = Data.chans_obr[0].integr_par.column_headers[k];
                        const_chan1.integr_obr.Add(str);
                        integrs_7.Add(str);
                    }

                    NpgsqlCommand comm_integ_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса');", sqlconn);    //все интегральные параметры (пункт 7)
                    NpgsqlDataReader rdr_integ_7 = comm_integ_7.ExecuteReader();
                    Parametrs.integral_pars = new Dictionary<string, Param>();
                    while (rdr_integ_7.Read())
                    {
                        bool fl = true;
                        for (int q = 0; q < const_chan1.integr_obr.Count; q++)
                        {
                            if (const_chan1.integr_obr[q].integr == rdr_integ_7[0].ToString())
                            {
                                fl = false;
                                q = const_chan1.integr_obr.Count;
                            }
                        }
                        if (fl)
                        {
                            Integr str = new Integr();
                            str.integr = rdr_integ_7[0].ToString();
                            const_chan1.integr_all.Add(str);
                        }
                        Param p = new Param() { short_name = rdr_integ_7[1].ToString(), unit = rdr_integ_7[2].ToString() };
                        Parametrs.integral_pars.Add(rdr_integ_7[0].ToString(), p);
                    }
                    rdr_integ_7.Close();


                }

                DB_constr.phys_7_db.Add(sech_ph_7_db);
                DB_constr.rezhs_7_db.Add(rezhs_7);
                DB_constr.integrs_7_db.Add(integrs_7);
            }

            Data.constr.Add(const_chan1);

            for (int i = 1; i < Data.channels.Count; i++)
            {
                RadioButton radio = new RadioButton { Content = $"Канал {i + 1}", BorderBrush = new SolidColorBrush(Color.FromRgb(0, 14, 153)) };
                radio.Checked += new RoutedEventHandler(RadioButton_Checked);
                stackpanel_chan.Children.Add(radio);
                Construct const_chan = new Construct();
                const_chan.sechen = new ObservableCollection<string>();
                const_chan.travers = new ObservableCollection<Traversir>();
                const_chan.params_sech = new ObservableCollection<Parametrs_sech_6>();

                const_chan.rezh_all_6 = new ObservableCollection<Rezh>();
                const_chan.rezh_izm_6 = new ObservableCollection<Rezh>();

                const_chan.proch_all = new ObservableCollection<Proch>();
                const_chan.proch_izm = new ObservableCollection<Proch>();

                const_chan.obr_params_sech = new ObservableCollection<Obr_parametrs_sech_7>();

                const_chan.rezh_all_7 = new ObservableCollection<Rezh>();
                const_chan.rezh_obr_7 = new ObservableCollection<Rezh>();

                const_chan.integr_all = new ObservableCollection<Integr>();
                const_chan.integr_obr = new ObservableCollection<Integr>();

                if (DB_constr.chan_dann[i] == "")   //если данных в базе нет
                {
                    NpgsqlCommand comm_rezh_6 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные параметры для остальных каналов (пункт 6)
                    NpgsqlDataReader rdr_rezh_6 = comm_rezh_6.ExecuteReader();
                    while (rdr_rezh_6.Read())
                    {
                        Rezh str = new Rezh();
                        str.rezh = rdr_rezh_6[0].ToString();
                        const_chan.rezh_all_6.Add(str);
                        const_chan.rezh_all_7.Add(str);
                    }
                    rdr_rezh_6.Close();


                    NpgsqlCommand comm_proch = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);    //все прочие параметры для остальных каналов (пункт 6)
                    NpgsqlDataReader rdr_proch = comm_proch.ExecuteReader();
                    while (rdr_proch.Read())
                    {
                        if (rdr_proch[0].ToString() != "Среда")
                        {
                            Proch str = new Proch();
                            str.proch = rdr_proch[0].ToString();
                            const_chan.proch_all.Add(str);
                        }

                    }
                    rdr_proch.Close();

                    NpgsqlCommand comm_integr = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса');", sqlconn);    //все Интегральные характеристики физического процесса для канала 1 (пункт 6)
                    NpgsqlDataReader rdr_integr = comm_integr.ExecuteReader();
                    while (rdr_integr.Read())
                    {
                        Integr str = new Integr();
                        str.integr = rdr_integr[0].ToString();
                        const_chan.integr_all.Add(str);
                    }
                    rdr_integr.Close();

                    DB_constr.sech_count_db.Add(0);//число сечений, которые есть в базе
                    Data.chans_results.Add(new Results_of_fiz_exp(false, i + 1));
                    Data.chans_obr.Add(new Obrabotka_of_fiz_exp(false, i + 1));

                    List<Traversir> trav_lst_for_db_constr = new List<Traversir>();
                    //Traversir sechenie = new Traversir();
                    //trav_lst_for_db_constr.Add(sechenie);
                    DB_constr.chans_trav_db.Add(trav_lst_for_db_constr);//список списков сечений с точками траверсирования для каждого канала, которые хранятся в базе (если в базе пусто, то список пустой)

                    DB_constr.rezh_count.Add(0);    //число режимов, которые есть в базе

                    List<List<Phys>> sech_ph_6_db = new List<List<Phys>>();//список по сечениям списков параметров
                    DB_constr.phys_6_db.Add(sech_ph_6_db);
                    DB_constr.phys_7_db.Add(sech_ph_6_db);

                    List<Rezh> rezhs_6 = new List<Rezh>();
                    DB_constr.rezhs_6_db.Add(rezhs_6);
                    DB_constr.rezhs_7_db.Add(rezhs_6);

                    List<Proch> proches_6 = new List<Proch>();
                    DB_constr.proches_6_db.Add(proches_6);

                    List<Integr> integrs_7 = new List<Integr>();
                    DB_constr.integrs_7_db.Add(integrs_7);
                }
                else
                {
                    Data.chans_results.Add(new Results_of_fiz_exp(true, i + 1));

                    foreach (section sec in Data.chans_results[i].sections)//есть траверсирвоание или нет
                    {
                        if (sec.coordinates != null)
                        {
                            const_chan.yes_no = true;
                        }
                    }

                    const_chan.count_sech = Data.chans_results[i].sections.Count.ToString();//число сечений
                    DB_constr.sech_count_db.Add(Data.chans_results[i].sections.Count);

                    List<List<Phys>> sech_ph_6_db = new List<List<Phys>>();//список по сечениям списков параметров

                    List<Traversir> trav_lst_for_db = new List<Traversir>();
                    for (int j = 0; j < Data.chans_results[i].sections.Count; j++)
                    {
                        Traversir sechenie = new Traversir();

                        List<Phys> ph_6_db = new List<Phys>();

                        if (Data.chans_results[i].sections[j].pars == null)
                        {
                            sechenie.count = Data.chans_results[i].sections[j].coordinates.travers_table[0].cols[0].Count.ToString();
                            ObservableCollection<string> coord = Data.chans_results[i].sections[j].coordinates.column_headers;
                            if ((coord[0] == "X") || (coord[1] == "Y"))
                            {
                                sechenie.sys_coord = "Декартова";
                            }
                            if ((coord[0] == "a") || (coord[1] == "R"))
                            {
                                sechenie.sys_coord = "Сферическая";
                            }

                            Parametrs_sech_6 param = new Parametrs_sech_6();
                            param.phys_all_6 = new ObservableCollection<Phys>();
                            param.phys_izm_6 = new ObservableCollection<Phys>();
                            for (int l = 0; l < Data.chans_results[i].sections[j].trav_pars.column_headers.Count; l++)
                            {
                                Phys str = new Phys();
                                str.phys = Data.chans_results[i].sections[j].trav_pars.column_headers[l];
                                param.phys_izm_6.Add(str);
                                ph_6_db.Add(str); //список физических параметров, которые есть в БД
                            }

                            NpgsqlCommand comm_phys = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);
                            NpgsqlDataReader rdr_phys = comm_phys.ExecuteReader();
                            while (rdr_phys.Read())
                            {
                                if (rdr_phys[0].ToString() != "Среда")
                                {
                                    bool fl = true;
                                    for (int q = 0; q < param.phys_izm_6.Count; q++)
                                    {
                                        if (rdr_phys[0].ToString() == param.phys_izm_6[q].phys)
                                        {
                                            fl = false;
                                            q = param.phys_izm_6.Count;
                                        }
                                    }
                                    if (fl)
                                    {
                                        Phys str = new Phys();
                                        str.phys = rdr_phys[0].ToString();
                                        param.phys_all_6.Add(str);
                                    }
                                }

                            }
                            rdr_phys.Close();
                            const_chan.params_sech.Add(param);
                        }
                        else
                        {
                            Parametrs_sech_6 param = new Parametrs_sech_6();
                            param.phys_all_6 = new ObservableCollection<Phys>();
                            param.phys_izm_6 = new ObservableCollection<Phys>();

                            for (int l = 0; l < Data.chans_results[i].sections[j].pars.column_headers.Count; l++)
                            {
                                Phys str = new Phys();
                                str.phys = Data.chans_results[i].sections[j].pars.column_headers[l];
                                param.phys_izm_6.Add(str);
                                ph_6_db.Add(str); //список физических параметров, которые есть в БД
                            }

                            NpgsqlCommand comm_phys = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);
                            NpgsqlDataReader rdr_phys = comm_phys.ExecuteReader();
                            while (rdr_phys.Read())
                            {
                                if (rdr_phys[0].ToString() != "Среда")
                                {
                                    bool fl = true;
                                    for (int q = 0; q < param.phys_izm_6.Count; q++)
                                    {
                                        if (rdr_phys[0].ToString() == param.phys_izm_6[q].phys)
                                        {
                                            fl = false;
                                            q = param.phys_izm_6.Count;
                                        }
                                    }
                                    if (fl)
                                    {
                                        Phys str = new Phys();
                                        str.phys = rdr_phys[0].ToString();
                                        param.phys_all_6.Add(str);
                                    }
                                }

                            }
                            rdr_phys.Close();

                            const_chan.params_sech.Add(param);
                        }
                        int s = j + 1;
                        const_chan.sechen.Add(s.ToString());
                        const_chan.travers.Add(sechenie);

                        Traversir sech_db = new Traversir();
                        sech_db.count = sechenie.count;
                        sech_db.sys_coord = sechenie.sys_coord;
                        trav_lst_for_db.Add(sech_db);

                        sech_ph_6_db.Add(ph_6_db);//список по сечениям со списками параметров
                    }
                    DB_constr.chans_trav_db.Add(trav_lst_for_db);//список списков сечений с точками траверсирования для каждого канала, которые хранятся в базе (если в базе пусто, то список пустой)

                    DB_constr.phys_6_db.Add(sech_ph_6_db);  //список по каналам со списками по сечениям списков параметров

                    const_chan.rezh_count = Data.chans_results[i].rezh_num.Count.ToString();//число режимов
                    DB_constr.rezh_count.Add(Data.chans_results[i].rezh_num.Count);    //число режимов, которые есть в базе

                    const_chan.sreda = Data.chans_results[i].sreda[0].value;
                    combox_sreda.SelectedItem = const_chan.sreda;

                    List<Rezh> rezhs_6 = new List<Rezh>();  //список режимных параметров

                    for (int k = 0; k < Data.chans_results[i].rezh_par.column_headers.Count; k++)
                    {
                        Rezh str = new Rezh();
                        str.rezh = Data.chans_results[i].rezh_par.column_headers[k];
                        const_chan.rezh_izm_6.Add(str);
                        rezhs_6.Add(str);
                    }
                    DB_constr.rezhs_6_db.Add(rezhs_6);  //список по каналам со списками режимных параметров

                    NpgsqlCommand comm_rezh_6 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные измеряемые параметры (пункт 6)
                    NpgsqlDataReader rdr_rezh_6 = comm_rezh_6.ExecuteReader();
                    while (rdr_rezh_6.Read())
                    {
                        bool fl = true;
                        for (int q = 0; q < const_chan.rezh_izm_6.Count; q++)
                        {
                            if (const_chan.rezh_izm_6[q].rezh == rdr_rezh_6[0].ToString())
                            {
                                fl = false;
                                q = const_chan.rezh_izm_6.Count;
                            }
                        }
                        if (fl)
                        {
                            Rezh str = new Rezh();
                            str.rezh = rdr_rezh_6[0].ToString();
                            const_chan.rezh_all_6.Add(str);
                        }
                    }
                    rdr_rezh_6.Close();

                    List<Proch> proches_6 = new List<Proch>();

                    for (int k = 0; k < Data.chans_results[i].prochie_par.column_headers.Count; k++)
                    {
                        Proch str = new Proch();
                        str.proch = Data.chans_results[i].prochie_par.column_headers[k];
                        const_chan.proch_izm.Add(str);
                        proches_6.Add(str);
                    }
                    DB_constr.proches_6_db.Add(proches_6);

                    NpgsqlCommand comm_proch_6 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);    //все прочие измеряемые параметры (пункт 6)
                    NpgsqlDataReader rdr_proch_6 = comm_proch_6.ExecuteReader();
                    while (rdr_proch_6.Read())
                    {
                        if (rdr_proch_6[0].ToString() != "Среда")
                        {
                            bool fl = true;
                            for (int q = 0; q < const_chan.proch_izm.Count; q++)
                            {
                                if (const_chan.proch_izm[q].proch == rdr_proch_6[0].ToString())
                                {
                                    fl = false;
                                    q = const_chan.proch_izm.Count;
                                }

                            }
                            if (fl)
                            {
                                Proch str = new Proch();
                                str.proch = rdr_proch_6[0].ToString();
                                const_chan.proch_all.Add(str);
                            }
                        }


                    }
                    rdr_proch_6.Close();

                    List<List<Phys>> sech_ph_7_db = new List<List<Phys>>();  //список физических параметров по сечениям

                    List<Rezh> rezhs_7 = new List<Rezh>();
                    List<Integr> integrs_7 = new List<Integr>();

                    if (DB_constr.chan_obr[i] == 1) //обработанных результатов нет
                    {
                        for (int n = 0; n < Convert.ToInt32(const_chan.count_sech); n++)
                        {
                            Obr_parametrs_sech_7 obr_param = new Obr_parametrs_sech_7();
                            obr_param.phys_all_7 = new ObservableCollection<Phys>();
                            obr_param.phys_obr_7 = new ObservableCollection<Phys>();

                            NpgsqlCommand comm_phys_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);    //все теплофизические параметры (пункт 6)
                            NpgsqlDataReader rdr_phys_7 = comm_phys_7.ExecuteReader();
                            while (rdr_phys_7.Read())
                            {
                                if (rdr_phys_7[0].ToString() != "Среда")
                                {
                                    Phys str = new Phys();
                                    str.phys = rdr_phys_7[0].ToString();
                                    obr_param.phys_all_7.Add(str);
                                }
                            }
                            rdr_phys_7.Close();
                            const_chan.obr_params_sech.Add(obr_param);
                        }


                        NpgsqlCommand comm_rezh_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные параметры для остальных каналов (пункт 6)
                        NpgsqlDataReader rdr_rezh_7 = comm_rezh_7.ExecuteReader();
                        while (rdr_rezh_7.Read())
                        {
                            Rezh str = new Rezh();
                            str.rezh = rdr_rezh_7[0].ToString();
                            const_chan.rezh_all_7.Add(str);
                        }
                        rdr_rezh_7.Close();

                        NpgsqlCommand comm_integr = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса');", sqlconn);    //все Интегральные характеристики физического процесса для канала 1 (пункт 6)
                        NpgsqlDataReader rdr_integr = comm_integr.ExecuteReader();
                        while (rdr_integr.Read())
                        {
                            Integr str = new Integr();
                            str.integr = rdr_integr[0].ToString();
                            const_chan.integr_all.Add(str);
                        }
                        rdr_integr.Close();

                        Data.chans_obr.Add(new Obrabotka_of_fiz_exp(false, i + 1));
                    }
                    else if (DB_constr.chan_obr[i] == 2)    //обработанные результаты есть
                    {
                        Data.chans_obr.Add(new Obrabotka_of_fiz_exp(true, i + 1));

                        for (int t = 0; t < Convert.ToInt32(const_chan.count_sech); t++)   //считывание физических параметров в сечениях
                        {
                            Obr_parametrs_sech_7 param = new Obr_parametrs_sech_7();
                            param.phys_all_7 = new ObservableCollection<Phys>();
                            param.phys_obr_7 = new ObservableCollection<Phys>();

                            List<Phys> ph_7_db = new List<Phys>();  //список физических параметров

                            if (t < Data.chans_obr[i].sections.Count)
                            {
                                for (int j = 0; j < Data.chans_obr[i].sections[t].pars.column_headers.Count; j++)   //заполнение списка обрабатываемых параметров
                                {
                                    Phys str = new Phys();
                                    str.phys = Data.chans_obr[i].sections[t].pars.column_headers[j];
                                    param.phys_obr_7.Add(str);
                                    ph_7_db.Add(str);
                                }
                            }
                            

                            NpgsqlCommand comm_phys_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn); //все физические параметры (пункт 7)
                            NpgsqlDataReader rdr_phys_7 = comm_phys_7.ExecuteReader();
                            while (rdr_phys_7.Read())
                            {
                                if (rdr_phys_7[0].ToString() != "Среда")
                                {
                                    bool fl = true;
                                    for (int q = 0; q < param.phys_obr_7.Count; q++)
                                    {
                                        if (rdr_phys_7[0].ToString() == param.phys_obr_7[q].phys)
                                        {
                                            fl = false;
                                            q = param.phys_obr_7.Count;
                                        }
                                    }
                                    if (fl)
                                    {
                                        Phys str = new Phys();
                                        str.phys = rdr_phys_7[0].ToString();
                                        param.phys_all_7.Add(str);
                                    }
                                }

                            }
                            rdr_phys_7.Close();

                            const_chan.obr_params_sech.Add(param);
                            sech_ph_7_db.Add(ph_7_db);
                        }


                        for (int k = 0; k < Data.chans_obr[i].rezh_par.column_headers.Count; k++)   //заполнение обрабатываемых режимных параметров
                        {
                            Rezh str = new Rezh();
                            str.rezh = Data.chans_obr[i].rezh_par.column_headers[k];
                            const_chan.rezh_obr_7.Add(str);
                            rezhs_7.Add(str);
                        }

                        NpgsqlCommand comm_rezh_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Режимные параметры');", sqlconn);    //все режимные параметры (пункт 7)
                        NpgsqlDataReader rdr_rezh_7 = comm_rezh_7.ExecuteReader();
                        while (rdr_rezh_7.Read())
                        {
                            bool fl = true;
                            for (int q = 0; q < const_chan.rezh_obr_7.Count; q++)
                            {
                                if (const_chan.rezh_obr_7[q].rezh == rdr_rezh_7[0].ToString())
                                {
                                    fl = false;
                                    q = const_chan.rezh_obr_7.Count;
                                }
                            }
                            if (fl)
                            {
                                Rezh str = new Rezh();
                                str.rezh = rdr_rezh_7[0].ToString();
                                const_chan.rezh_all_7.Add(str);
                            }

                        }
                        rdr_rezh_7.Close();

                        for (int k = 0; k < Data.chans_obr[i].integr_par.column_headers.Count; k++)   //заполнение обрабатываемых интегральных параметров
                        {
                            Integr str = new Integr();
                            str.integr = Data.chans_obr[i].integr_par.column_headers[k];
                            const_chan.integr_obr.Add(str);
                            integrs_7.Add(str);
                        }

                        NpgsqlCommand comm_integ_7 = new NpgsqlCommand($"select * from main_block.select_parametrs('Интегральные характеристики физического процесса');", sqlconn);    //все интегральные параметры (пункт 7)
                        NpgsqlDataReader rdr_integ_7 = comm_integ_7.ExecuteReader();
                        while (rdr_integ_7.Read())
                        {
                            bool fl = true;
                            for (int q = 0; q < const_chan.integr_obr.Count; q++)
                            {
                                if (const_chan.integr_obr[q].integr == rdr_integ_7[0].ToString())
                                {
                                    fl = false;
                                    q = const_chan.integr_obr.Count;
                                }
                            }
                            if (fl)
                            {
                                Integr str = new Integr();
                                str.integr = rdr_integ_7[0].ToString();
                                const_chan.integr_all.Add(str);
                            }

                        }
                        rdr_integ_7.Close();


                    }

                    DB_constr.phys_7_db.Add(sech_ph_7_db);
                    DB_constr.rezhs_7_db.Add(rezhs_7);
                    DB_constr.integrs_7_db.Add(integrs_7);
                }



                Data.constr.Add(const_chan);
            }



            radiobut_chan1.IsChecked = true;


            NpgsqlCommand comm = new NpgsqlCommand("select \"value\" from main_block.\"String_values\" where \"id_param\" = (select \"id_param\" from main_block.\"Parametrs\" where \"name_param\"='Среда')", sqlconn);
            NpgsqlDataReader reader = comm.ExecuteReader();
            while (reader.Read())
            {
                combox_sreda.Items.Add(reader[0]);
            }
            reader.Close();




            sqlconn.Close();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)//выбор канала
        {
            var radio = sender as RadioButton;
            string[] names = radio.Content.ToString().Split(' ');
            chan = Convert.ToInt32(names[1]) - 1;
            grid_chan.DataContext = Data.constr[chan];
            if (Data.constr[chan].yes_no == false)
            {
                chek_no.IsChecked = true;
            }
            combox_trav_sech.SelectedIndex = -1;
            txt_trav_count.Clear();
            flag_sys_coord = false;
            combox_trav_coord.SelectedIndex = -1;
            flag_sys_coord = true;

            for (int i = 0; i < combox_sreda.Items.Count; i++)
            {
                if (combox_sreda.Items[i].ToString() == Data.constr[chan].sreda)
                {
                    combox_sreda.SelectedIndex = i;
                    i = combox_sreda.Items.Count;
                }
                else if (i == combox_sreda.Items.Count - 1)
                {
                    combox_sreda.SelectedIndex = -1;
                }
            }
            combox_6.SelectedIndex = -1;
            combox_7.SelectedIndex = -1;

        }

        private void txt_KeyDown(object sender, KeyEventArgs e)
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

        private void txt_count_sech_KeyUp(object sender, KeyEventArgs e)//количество сечений
        {
            if (txt_count_sech.Text != "")
            {
                if (DB_constr.chan_dann[chan] == "")
                {
                    if (Data.constr[chan].sechen.Count < Convert.ToInt32(txt_count_sech.Text))
                    {
                        NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                        sqlconn.Open();

                        for (int i = Data.constr[chan].sechen.Count + 1; i <= Convert.ToInt32(txt_count_sech.Text); i++)
                        {
                            Traversir sechenie = new Traversir();
                            Data.constr[chan].sechen.Add(i.ToString());
                            Data.constr[chan].travers.Add(sechenie);

                            Parametrs_sech_6 param = new Parametrs_sech_6();
                            param.phys_all_6 = new ObservableCollection<Phys>();
                            param.phys_izm_6 = new ObservableCollection<Phys>();

                            Obr_parametrs_sech_7 obr_param = new Obr_parametrs_sech_7();
                            obr_param.phys_all_7 = new ObservableCollection<Phys>();
                            obr_param.phys_obr_7 = new ObservableCollection<Phys>();

                            NpgsqlCommand comm_phys = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);
                            NpgsqlDataReader rdr_phys = comm_phys.ExecuteReader();
                            while (rdr_phys.Read())
                            {
                                if (rdr_phys[0].ToString() != "Среда")
                                {
                                    Phys str = new Phys();
                                    str.phys = rdr_phys[0].ToString();
                                    param.phys_all_6.Add(str);
                                    obr_param.phys_all_7.Add(str);
                                }

                            }
                            rdr_phys.Close();
                            Data.constr[chan].params_sech.Add(param);
                            Data.constr[chan].obr_params_sech.Add(obr_param);
                        }


                        sqlconn.Close();
                    }
                    else
                    {
                        for (int i = Data.constr[chan].sechen.Count; i > Convert.ToInt32(txt_count_sech.Text); i--)
                        {
                            Data.constr[chan].sechen.Remove(Data.constr[chan].sechen[i - 1]);
                            Data.constr[chan].travers.Remove(Data.constr[chan].travers[i - 1]);
                            Data.constr[chan].params_sech.Remove(Data.constr[chan].params_sech[i - 1]);
                            Data.constr[chan].obr_params_sech.Remove(Data.constr[chan].obr_params_sech[i - 1]);
                        }
                    }
                }
                else
                {
                    if ((DB_constr.sech_count_db[chan] < Convert.ToInt32(txt_count_sech.Text)))
                    {
                        if (Data.constr[chan].sechen.Count < Convert.ToInt32(txt_count_sech.Text))
                        {
                            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
                            sqlconn.Open();

                            for (int i = Data.constr[chan].sechen.Count + 1; i <= Convert.ToInt32(txt_count_sech.Text); i++)
                            {
                                Traversir sechenie = new Traversir();
                                Data.constr[chan].sechen.Add(i.ToString());
                                Data.constr[chan].travers.Add(sechenie);

                                Parametrs_sech_6 param = new Parametrs_sech_6();
                                param.phys_all_6 = new ObservableCollection<Phys>();
                                param.phys_izm_6 = new ObservableCollection<Phys>();

                                Obr_parametrs_sech_7 obr_param = new Obr_parametrs_sech_7();
                                obr_param.phys_all_7 = new ObservableCollection<Phys>();
                                obr_param.phys_obr_7 = new ObservableCollection<Phys>();


                                NpgsqlCommand comm_phys = new NpgsqlCommand($"select * from main_block.select_parametrs('Теплофизические параметры');", sqlconn);
                                NpgsqlDataReader rdr_phys = comm_phys.ExecuteReader();
                                while (rdr_phys.Read())
                                {
                                    if (rdr_phys[0].ToString() != "Среда")
                                    {
                                        Phys str = new Phys();
                                        str.phys = rdr_phys[0].ToString();
                                        param.phys_all_6.Add(str);
                                        obr_param.phys_all_7.Add(str);
                                    }

                                }
                                rdr_phys.Close();
                                Data.constr[chan].params_sech.Add(param);
                                Data.constr[chan].obr_params_sech.Add(obr_param);
                            }


                            sqlconn.Close();
                        }
                        else
                        {
                            for (int i = Data.constr[chan].sechen.Count; i > Convert.ToInt32(txt_count_sech.Text); i--)
                            {
                                Data.constr[chan].sechen.Remove(Data.constr[chan].sechen[i - 1]);
                                Data.constr[chan].travers.Remove(Data.constr[chan].travers[i - 1]);
                                Data.constr[chan].params_sech.Remove(Data.constr[chan].params_sech[i - 1]);
                                Data.constr[chan].obr_params_sech.Remove(Data.constr[chan].obr_params_sech[i - 1]);
                            }
                        }
                    }
                    else
                    {
                        txt_count_sech.Text = DB_constr.sech_count_db[chan].ToString();
                        for (int i = Data.constr[chan].sechen.Count; i > Convert.ToInt32(txt_count_sech.Text); i--)
                        {
                            Data.constr[chan].sechen.Remove(Data.constr[chan].sechen[i - 1]);
                            Data.constr[chan].travers.Remove(Data.constr[chan].travers[i - 1]);
                            Data.constr[chan].params_sech.Remove(Data.constr[chan].params_sech[i - 1]);
                            Data.constr[chan].obr_params_sech.Remove(Data.constr[chan].obr_params_sech[i - 1]);
                        }
                    }
                }


            }

            txt_trav_count.Clear();
            flag_sys_coord = false;
            combox_trav_coord.SelectedIndex = -1;
            flag_sys_coord = true;
            combox_trav_sech.SelectedIndex = -1;
        }

        private void combox_trav_sech_SelectionChanged(object sender, SelectionChangedEventArgs e)//выбор сечения, в котором проводилось траверсирование
        {
            //ComboBoxItem comitem = new ComboBoxItem();
            if (combox_trav_sech.SelectedItem != null)
            {
                string comitem = combox_trav_sech.SelectedItem.ToString();
                sech2 = Convert.ToInt32(comitem);
                stackpanel_trav.DataContext = Data.constr[chan].travers[sech2 - 1];
                if (Data.constr[chan].travers[sech2 - 1].sys_coord != null)
                {
                    flag_sys_coord = false;
                    switch (Data.constr[chan].travers[sech2 - 1].sys_coord)
                    {
                        case "Декартова":
                            combox_trav_coord.SelectedIndex = 0;
                            break;
                        case "Сферическая":
                            combox_trav_coord.SelectedIndex = 1;
                            break;
                    }
                }
                else
                {
                    flag_sys_coord = false;
                    combox_trav_coord.SelectedIndex = -1;

                }
                if (Data.constr[chan].travers[sech2 - 1].count != null)
                {
                    txt_trav_count.Text = Data.constr[chan].travers[sech2 - 1].count;
                }
                flag_sys_coord = true;
            }

        }

        private void txt_trav_count_KeyUp(object sender, KeyEventArgs e)    //количество точек траверсирования
        {
            if ((txt_trav_count.Text != "") && (DB_constr.chans_trav_db[chan].Count != 0) && (Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1 < DB_constr.chans_trav_db[chan].Count))
            {
                if (Convert.ToInt32(txt_trav_count.Text) < Convert.ToInt32(DB_constr.chans_trav_db[chan][Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1].count))
                {
                    txt_trav_count.Text = DB_constr.chans_trav_db[chan][Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1].count;   //нельзя задавать т-к трав-я меньше чем есть в БД
                }
                else if (DB_constr.chans_trav_db[chan][Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1].count == null)
                {
                    txt_trav_count.Text = "";   //если в БД указано, что т-к траверсирования нет, то нельзя задавать точки траверсирования
                }

            }
        }

        private void combox_trav_coord_SelectionChanged(object sender, SelectionChangedEventArgs e)//система координат
        {
            if ((flag_sys_coord) && (combox_trav_sech.SelectedIndex != -1))
            {
                if ((Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1 < DB_constr.chans_trav_db[chan].Count) && (DB_constr.chans_trav_db[chan].Count != 0))
                {
                    ComboBox combo = (ComboBox)sender;
                    if (e.RemovedItems.Count != 0)
                    {
                        flag_sys_coord = false;
                        combo.SelectedItem = e.RemovedItems[0]; //отменить выбор
                    }
                    else if (DB_constr.chans_trav_db[chan][Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1].sys_coord == null)
                    {
                        flag_sys_coord = false;
                        combo.SelectedIndex = -1; //нельзя выбирать если в базе нет данных
                    }
                }
                else
                {
                    ComboBoxItem comitem = new ComboBoxItem();
                    comitem = (ComboBoxItem)combox_trav_coord.SelectedItem;
                    if (comitem != null)
                    {
                        Data.constr[chan].travers[sech2 - 1].sys_coord = comitem.Content.ToString();
                    }
                }
            }
            else if (combox_trav_sech.SelectedIndex == -1)
            {
                ComboBox combo = (ComboBox)sender;
                combo.SelectedIndex = -1;
            }
            flag_sys_coord = true;

        }

        private void butt_syscoord_del_Click(object sender, RoutedEventArgs e)  //очистить систему координат
        {
            if ((combox_trav_sech.SelectedIndex != -1) && ((DB_constr.chans_trav_db[chan].Count == 0) || (Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1 >= DB_constr.chans_trav_db[chan].Count)))
            {
                flag_sys_coord = false;
                combox_trav_coord.SelectedIndex = -1;
                Data.constr[chan].travers[Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1].sys_coord = null;
                txt_trav_count.Text = "";
                Data.constr[chan].travers[Convert.ToInt32(combox_trav_sech.SelectedItem.ToString()) - 1].count = null;
                flag_sys_coord = true;
            }
        }

        private void txt_rezh_KeyUp(object sender, KeyEventArgs e)  //количество режимов
        {
            if (txt_rezh.Text != "")
            {
                if ((DB_constr.rezh_count[chan] != 0) && (Convert.ToInt32(txt_rezh.Text) < DB_constr.rezh_count[chan]))
                {
                    txt_rezh.Text = DB_constr.rezh_count[chan].ToString();
                }
            }
        }

        private void combox_sreda_SelectionChanged(object sender, SelectionChangedEventArgs e)  //выбор среды
        {
            if (combox_sreda.SelectedItem != null)
            {
                Data.constr[chan].sreda = combox_sreda.SelectedItem.ToString();
            }
        }

        private void combox_6_SelectionChanged(object sender, SelectionChangedEventArgs e)  //выбор сечения при выборе физических параметров пункта 6
        {
            if (combox_6.SelectedItem != null)
            {
                string comitem = combox_6.SelectedItem.ToString();
                sech6 = Convert.ToInt32(comitem);
                grid_6_phys.DataContext = Data.constr[chan].params_sech[sech6 - 1];
            }
            else
            {
                grid_6_phys.DataContext = null;
            }
        }

        private void combox_7_SelectionChanged(object sender, SelectionChangedEventArgs e)  //выбор сечения при выборе обрабатываемых физических параметров пункта 7
        {
            if (combox_7.SelectedItem != null)
            {
                string comitem = combox_7.SelectedItem.ToString();
                sech7 = Convert.ToInt32(comitem);
                grid_7_phys.DataContext = Data.constr[chan].obr_params_sech[sech7 - 1];
            }
            else
            {
                grid_7_phys.DataContext = null;
            }
        }

        private void butt_phys_add_6_Click(object sender, RoutedEventArgs e)//пункт 6 - добавить физич. параметр в измеряемые
        {
            if (datagrid_phys_all_6.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_phys_all_6.SelectedItems.Count; i++)
                {
                    Phys phys = datagrid_phys_all_6.SelectedItems[i] as Phys;
                    Data.constr[chan].params_sech[sech6 - 1].phys_all_6.Remove(phys);

                    Data.constr[chan].params_sech[sech6 - 1].phys_izm_6.Add(phys);
                }
            }
        }

        private void butt_phys_del_6_Click(object sender, RoutedEventArgs e)//пункт 6 - убрать физич. параметр из измеряемых
        {
            if (datagrid_phys_izm_6.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_phys_izm_6.SelectedItems.Count; i++)
                {
                    Phys phys = datagrid_phys_izm_6.SelectedItems[i] as Phys;
                    bool fl = true;
                    if ((DB_constr.phys_6_db[chan].Count != 0) && (Convert.ToInt32(combox_6.SelectedItem.ToString()) - 1 < DB_constr.phys_6_db[chan].Count))
                    {
                        for (int j = 0; j < DB_constr.phys_6_db[chan][Convert.ToInt32(combox_6.SelectedItem.ToString()) - 1].Count; j++)
                        {
                            if (phys.phys == DB_constr.phys_6_db[chan][Convert.ToInt32(combox_6.SelectedItem.ToString()) - 1][j].phys)
                            {
                                fl = false;
                                j = DB_constr.phys_6_db[chan][Convert.ToInt32(combox_6.SelectedItem.ToString()) - 1].Count;
                            }
                        }
                    }
                    if (fl)
                    {
                        Data.constr[chan].params_sech[sech6 - 1].phys_izm_6.Remove(phys);
                        Data.constr[chan].params_sech[sech6 - 1].phys_all_6.Add(phys);
                    }

                }
            }

        }

        private void butt_rezh_add_6_Click(object sender, RoutedEventArgs e)//пункт 6 - добавить режимный параметр в измеряемые
        {
            if (datagrid_rezh_all_6.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_rezh_all_6.SelectedItems.Count; i++)
                {
                    Rezh rezh = datagrid_rezh_all_6.SelectedItems[i] as Rezh;
                    Data.constr[chan].rezh_all_6.Remove(rezh);
                    Data.constr[chan].rezh_izm_6.Add(rezh);
                }
            }
        }

        private void butt_rezh_del_6_Click(object sender, RoutedEventArgs e)//пункт 6 - убрать режимный параметр из измеряемых
        {
            if (datagrid_rezh_izm_6.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_rezh_izm_6.SelectedItems.Count; i++)
                {
                    Rezh rezh = datagrid_rezh_izm_6.SelectedItems[i] as Rezh;
                    bool fl = true;
                    for (int j = 0; j < DB_constr.rezhs_6_db[chan].Count; j++)
                    {
                        if (rezh.rezh == DB_constr.rezhs_6_db[chan][j].rezh)
                        {
                            fl = false;
                            j = DB_constr.rezhs_6_db[chan].Count;
                        }
                    }
                    if (fl)
                    {
                        Data.constr[chan].rezh_izm_6.Remove(rezh);
                        Data.constr[chan].rezh_all_6.Add(rezh);
                    }

                }
            }
        }

        private void butt_proch_add_Click(object sender, RoutedEventArgs e)//пункт 6 - добавить прочий параметр в измеряемые
        {
            if (datagrid_proch_all.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_proch_all.SelectedItems.Count; i++)
                {
                    Proch proch = datagrid_proch_all.SelectedItems[i] as Proch;
                    Data.constr[chan].proch_all.Remove(proch);
                    Data.constr[chan].proch_izm.Add(proch);
                }
            }
        }

        private void butt_proch_del_Click(object sender, RoutedEventArgs e)//пункт 6 - убрать прочий параметр из измеряемых
        {
            if (datagrid_proch_izm.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_proch_izm.SelectedItems.Count; i++)
                {
                    Proch proch = datagrid_proch_izm.SelectedItems[i] as Proch;
                    bool fl = true;
                    for (int j = 0; j < DB_constr.proches_6_db[chan].Count; j++)
                    {
                        if (proch.proch == DB_constr.proches_6_db[chan][j].proch)
                        {
                            fl = false;
                            j = DB_constr.proches_6_db[chan].Count;
                        }
                    }
                    if (fl)
                    {
                        Data.constr[chan].proch_izm.Remove(proch);
                        Data.constr[chan].proch_all.Add(proch);
                    }

                }
            }

        }

        private void butt_obr_phys_add_Click(object sender, RoutedEventArgs e)//пункт 7 - добавить физический параметр в обрабатываемые
        {
            if (datagrid_obr_phys_all_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_phys_all_7.SelectedItems.Count; i++)
                {
                    Phys phys = datagrid_obr_phys_all_7.SelectedItems[i] as Phys;
                    Data.constr[chan].obr_params_sech[sech7 - 1].phys_all_7.Remove(phys);
                    Data.constr[chan].obr_params_sech[sech7 - 1].phys_obr_7.Add(phys);
                }
            }
        }

        private void butt_obr_phys_del_Click(object sender, RoutedEventArgs e)//пункт 7 - убрать физический параметр из обрабатываемых
        {
            if (datagrid_obr_phys_sech_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_phys_sech_7.SelectedItems.Count; i++)
                {
                    Phys phys = datagrid_obr_phys_sech_7.SelectedItems[i] as Phys;
                    bool fl = true;
                    if ((DB_constr.phys_7_db[chan].Count != 0) && (Convert.ToInt32(combox_7.SelectedItem.ToString()) - 1 < DB_constr.phys_7_db[chan].Count))
                    {
                        for (int j = 0; j < DB_constr.phys_7_db[chan][Convert.ToInt32(combox_7.SelectedItem.ToString()) - 1].Count; j++)
                        {
                            if (phys.phys == DB_constr.phys_7_db[chan][Convert.ToInt32(combox_7.SelectedItem.ToString()) - 1][j].phys)
                            {
                                fl = false;
                                j = DB_constr.phys_7_db[chan][Convert.ToInt32(combox_7.SelectedItem.ToString()) - 1].Count;
                            }
                        }
                    }
                    if (fl)
                    {
                        Data.constr[chan].obr_params_sech[sech7 - 1].phys_all_7.Add(phys);
                        Data.constr[chan].obr_params_sech[sech7 - 1].phys_obr_7.Remove(phys);
                    }
                }
            }
        }

        private void butt_obr_rezh_add_7_Click(object sender, RoutedEventArgs e)//пункт 7 - добавить режимный параметр в обрабатываемые
        {
            if (datagrid_obr_rezh_all_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_rezh_all_7.SelectedItems.Count; i++)
                {
                    Rezh rezh = datagrid_obr_rezh_all_7.SelectedItems[i] as Rezh;
                    Data.constr[chan].rezh_all_7.Remove(rezh);
                    Data.constr[chan].rezh_obr_7.Add(rezh);
                }
            }
        }

        private void butt_obr_rezh_del_7_Click(object sender, RoutedEventArgs e)//пункт 7 - убрать режимный параметр из обрабатываемых
        {
            if (datagrid_obr_rezh_obr_7.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_rezh_obr_7.SelectedItems.Count; i++)
                {
                    Rezh rezh = datagrid_obr_rezh_obr_7.SelectedItems[i] as Rezh;
                    bool fl = true;
                    for (int j = 0; j < DB_constr.rezhs_7_db[chan].Count; j++)
                    {
                        if (rezh.rezh == DB_constr.rezhs_7_db[chan][j].rezh)
                        {
                            fl = false;
                            j = DB_constr.rezhs_7_db[chan].Count;
                        }
                    }
                    if (fl)
                    {
                        Data.constr[chan].rezh_all_7.Add(rezh);
                        Data.constr[chan].rezh_obr_7.Remove(rezh);
                    }

                }
            }
        }

        private void butt_obr_int_add_Click(object sender, RoutedEventArgs e)//пункт 7 - добавить интегральный параметр в обрабатываемые
        {
            if (datagrid_obr_int_all.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_int_all.SelectedItems.Count; i++)
                {
                    Integr integ = datagrid_obr_int_all.SelectedItems[i] as Integr;
                    Data.constr[chan].integr_all.Remove(integ);
                    Data.constr[chan].integr_obr.Add(integ);
                }
            }
        }

        private void butt_obr_int_del_Click(object sender, RoutedEventArgs e)//пункт 7 - убрать интегральный параметр из обрабатываемых
        {
            if (datagrid_obr_int_sech.SelectedItems.Count != 0)
            {
                for (int i = 0; i < datagrid_obr_int_sech.SelectedItems.Count; i++)
                {
                    Integr integ = datagrid_obr_int_sech.SelectedItems[i] as Integr;
                    bool fl = true;
                    for (int j = 0; j < DB_constr.integrs_7_db[chan].Count; j++)
                    {
                        if (integ.integr == DB_constr.integrs_7_db[chan][j].integr)
                        {
                            fl = false;
                            j = DB_constr.integrs_7_db[chan].Count;
                        }
                    }
                    if (fl)
                    {
                        Data.constr[chan].integr_all.Add(integ);
                        Data.constr[chan].integr_obr.Remove(integ);
                    }

                }
            }
        }

        private void Butt_OK_Click(object sender, RoutedEventArgs e)//добавление нового параметра в базу
        {
            if (txt_par_name.Text == "" || txt_par_symb.Text == "" || txt_par_unit.Text == "")
            {
                messtxt.Text = "Не все поля заполнены!";
                messbar.IsActive = true;
                txt_par_name.IsEnabled = false;
                txt_par_symb.IsEnabled = false;
                txt_par_unit.IsEnabled = false;
            }
            else
            {
                string name = txt_par_name.Text;
                string short_name = txt_par_symb.Text;
                string unit = txt_par_unit.Text;
                Parametrs.update_parametrs();
                string check = Parametrs.check_new_param(name, short_name);
                if (check == "")
                {

                    Parametrs.insert_parametr(dialog_add_param.Tag.ToString(), name, short_name, unit);
                    switch (dialog_add_param.Tag)
                    {
                        case "Теплофизические параметры":
                            Phys newparphys = new Phys();
                            newparphys.phys = txt_par_name.Text;

                            Proch newparproch = new Proch();
                            newparproch.proch = txt_par_name.Text;
                            for (int i = 0; i < Data.constr.Count; i++)
                            {
                                for (int j = 0; j < Convert.ToInt32(Data.constr[i].count_sech); j++)
                                {
                                    Data.constr[i].params_sech[j].phys_all_6.Add(newparphys);
                                    Data.constr[i].obr_params_sech[j].phys_all_7.Add(newparphys);
                                }
                                Data.constr[i].proch_all.Add(newparproch);
                            }
                            break;

                        case "Режимные параметры":
                            Rezh newparrezh = new Rezh();
                            newparrezh.rezh = txt_par_name.Text;
                            for (int i = 0; i < Data.constr.Count; i++)
                            {
                                Data.constr[i].rezh_all_6.Add(newparrezh);
                                Data.constr[i].rezh_all_7.Add(newparrezh);
                            }
                            break;

                        case "Интегральные характеристики физического процесса":
                            Integr newparint = new Integr();
                            newparint.integr = txt_par_name.Text;
                            for (int i = 0; i < Data.constr.Count; i++)
                            {
                                Data.constr[i].integr_all.Add(newparint);
                            }
                            break;
                    }


                    dialog_add_param.IsOpen = false;
                    exp_wind.Butt_back.IsEnabled = true;
                    exp_wind.Butt_next.IsEnabled = true;
                    exp_wind.listview_items.IsEnabled = true;
                }
                else
                {
                    if (check == name)
                    {
                        messtxt.Text = $"Параметр \"{name}\" уже есть в базе!";
                        messbar.IsActive = true;
                    }
                    else
                    {
                        messtxt.Text = $"Обозначение \"{short_name}\" уже есть в базе!";
                        messbar.IsActive = true;
                    }
                    txt_par_name.IsEnabled = false;
                    txt_par_symb.IsEnabled = false;
                    txt_par_unit.IsEnabled = false;
                }
            }
        }

        private void messbut_Click(object sender, RoutedEventArgs e)
        {
            messbar.IsActive = false;
            txt_par_name.IsEnabled = true;
            txt_par_symb.IsEnabled = true;
            txt_par_unit.IsEnabled = true;
        }

        private void dialog_add_param_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
            exp_wind.Butt_back.IsEnabled = true;
            exp_wind.Butt_next.IsEnabled = true;
            exp_wind.listview_items.IsEnabled = true;
        }

        private void butt_phys_addnew_6_Click(object sender, RoutedEventArgs e)//добавить новый теплофизический, прочий (пукнт 6) и теплофизический (пункт 7) параметр
        {
            exp_wind.Butt_back.IsEnabled = false;
            exp_wind.Butt_next.IsEnabled = false;
            exp_wind.listview_items.IsEnabled = false;
            dialog_add_param.Tag = "Теплофизические параметры";
            dialog_add_param.IsOpen = true;
        }

        private void butt_rezh_addnew_6_Click(object sender, RoutedEventArgs e)//добавить новый режимный параметр для пунктов 6 и 7
        {
            exp_wind.Butt_back.IsEnabled = false;
            exp_wind.Butt_next.IsEnabled = false;
            exp_wind.listview_items.IsEnabled = false;
            dialog_add_param.Tag = "Режимные параметры";
            dialog_add_param.IsOpen = true;
        }

        private void butt_obr_int_addnew_Click(object sender, RoutedEventArgs e)//добавить новые интегральные характеристики физического процесса
        {
            exp_wind.Butt_back.IsEnabled = false;
            exp_wind.Butt_next.IsEnabled = false;
            exp_wind.listview_items.IsEnabled = false;
            dialog_add_param.Tag = "Интегральные характеристики физического процесса";
            dialog_add_param.IsOpen = true;
        }


    }

    public class Construct
    {
        //private bool yes_no_str;
        public bool yes_no { get; set; }                                            //проводилось ли траверсирование в канале
        public string count_sech { get; set; }                                      // число сечений
        public ObservableCollection<string> sechen { get; set; }                    // список номеров сечений из базы (1,2,3 и тд)
        public ObservableCollection<Traversir> travers { get; set; }                // список сечений (количества точек траверсирования и системы коррдинат) (если без траверсирования, то null)
        public string rezh_count { get; set; }                                      // количество режимов
        public string sreda { get; set; }                                           // среда
        public ObservableCollection<Parametrs_sech_6> params_sech { get; set; }     // список параметров для каждого сечения
        public ObservableCollection<Rezh> rezh_all_6 { get; set; }                  // список параметров для добавления в режимные параметры
        public ObservableCollection<Rezh> rezh_izm_6 { get; set; }                  // список добавленных в режимные (измеряемые)
        public ObservableCollection<Proch> proch_all { get; set; }                  // список параметров для добавления в прочие параметры
        public ObservableCollection<Proch> proch_izm { get; set; }                  // список добавленных в прочие (измеряемые)
        public ObservableCollection<Obr_parametrs_sech_7> obr_params_sech { get; set; }  //список обрабатываемых физических параметров для каждого сечения
        public ObservableCollection<Rezh> rezh_all_7 { get; set; }                  // список параметров для добавления в обрабатываемые режимные параметры
        public ObservableCollection<Rezh> rezh_obr_7 { get; set; }                  // список добавленных в режимные (обрабатываемые)
        public ObservableCollection<Integr> integr_all { get; set; }                // список параметров для добавления в Интегральные характеристики физического процесса
        public ObservableCollection<Integr> integr_obr { get; set; }                // список добавленных в интегральные (обрабатываемые)
    }
    public class Traversir
    {
        //public string sech { get; set; }
        public string count { get; set; }   // число точек траверсирования
        public string sys_coord { get; set; }   // система координат(Декартова, Сферическая)

    }
    public class Parametrs_sech_6
    {
        public ObservableCollection<Phys> phys_all_6 { get; set; }    // список параметров для добавления в сечение 
        public ObservableCollection<Phys> phys_izm_6 { get; set; }    // список параметров добавленных в сечение (измеряемые)

    }
    public class Obr_parametrs_sech_7
    {
        public ObservableCollection<Phys> phys_all_7 { get; set; }    // список параметров для добавления в сечение 
        public ObservableCollection<Phys> phys_obr_7 { get; set; }    // список параметров добавленных в сечение (измеряемые)
    }

    public class Phys
    {
        public string phys { get; set; }
    }
    public class Rezh
    {
        public string rezh { get; set; }
    }
    public class Proch
    {
        public string proch { get; set; }
    }
    public class Integr
    {
        public string integr { get; set; }
    }

    public static class DB_constr
    {
        public static List<string> chan_dann = new List<string>();  //есть ли данные в БД ("" - данных нет) 
        public static List<int> sech_count_db = new List<int>();
        public static List<List<Traversir>> chans_trav_db = new List<List<Traversir>>();
        public static List<int> chan_obr = new List<int>();    //есть ли обработанные результаты у канала (2 - есть, 1 - нет обработанных, 0 - нет результатов вообще)
        public static List<int> rezh_count = new List<int>();

        public static List<List<List<Phys>>> phys_6_db = new List<List<List<Phys>>>();    //список по каналам списков физич. параметров по сечениям (список по каналам списков по сечениям списков параметров)
        public static List<List<Rezh>> rezhs_6_db = new List<List<Rezh>>(); //список по каналам списков с режимными параметрами
        public static List<List<Proch>> proches_6_db = new List<List<Proch>>(); //список по каналам списков с прочими параметрами

        public static List<List<List<Phys>>> phys_7_db = new List<List<List<Phys>>>();
        public static List<List<Rezh>> rezhs_7_db = new List<List<Rezh>>();
        public static List<List<Integr>> integrs_7_db = new List<List<Integr>>();

        public static void Del()
        {
            chan_dann.Clear();
            sech_count_db.Clear();
            chans_trav_db.Clear();
            chan_obr.Clear();
            rezh_count.Clear();

            phys_6_db.Clear();
            rezhs_6_db.Clear();
            proches_6_db.Clear();

            phys_7_db.Clear();
            rezhs_7_db.Clear();
            integrs_7_db.Clear();
        }

        public static void Create()
        {
            string conn_str = User.Connection_string;

            NpgsqlConnection sqlconn = new NpgsqlConnection(conn_str);
            sqlconn.Open();

            NpgsqlCommand comm_chan_dann = new NpgsqlCommand($"select distinct rc.\"Channel\", mcs.\"Id_R_C\" from main_block.\"Mode_cros_section\" mcs right join main_block.\"Realization_channel\" rc on mcs.\"Id_R_C\"=rc.\"Id_R_C\" where rc.\"Realization\" = {Data.current_realization} and \"Id$\"={Data.id_obj} order by rc.\"Channel\";", sqlconn);
            NpgsqlDataReader rdr_chan_dann = comm_chan_dann.ExecuteReader();
            while (rdr_chan_dann.Read())
            {
                if (rdr_chan_dann[1].ToString() != "")
                {
                    string chan_d = rdr_chan_dann[0].ToString();
                    chan_dann.Add(chan_d);
                }
                else
                {
                    string chan_d = rdr_chan_dann[1].ToString();
                    chan_dann.Add(chan_d);
                }
            }
            rdr_chan_dann.Close();

            NpgsqlCommand comm_chan_obr = new NpgsqlCommand($"select \"Channel\" , count(\"id_obr0/rez1\") from (select distinct rc.\"Channel\", mcs.\"Id_R_C\", ve.\"id_obr0/rez1\" from " +
                "main_block.\"Mode_cros_section\" mcs right join main_block.\"Realization_channel\" rc on mcs.\"Id_R_C\" = rc.\"Id_R_C\" " +
                "left join main_block.\"Values_experiment\" ve on mcs.\"Id_m_c\" = ve.\"Id_m_c\" " +
                $"where  rc.\"Realization\" = {Data.current_realization} and \"Id$\" = {Data.id_obj} order by rc.\"Channel\") s " +
                "group by \"Channel\"; ", sqlconn);
            NpgsqlDataReader rdr_chan_obr = comm_chan_obr.ExecuteReader();
            while (rdr_chan_obr.Read())
            {
                chan_obr.Add(Convert.ToInt32(rdr_chan_obr[1])); //есть или нет обработанные результаты (2 - есть, 1 - нет обработанных, 0 - нет результатов вообще)
            }
            rdr_chan_obr.Close();

            for (int i = 0; i < Data.channels.Count; i++)
            {
                if (chan_dann[i] == "") //если данных в базе нет
                {
                    sech_count_db.Add(0);//число сечений, которые есть в базе

                    List<Traversir> trav_lst_for_db_constr = new List<Traversir>();
                    chans_trav_db.Add(trav_lst_for_db_constr);//список списков сечений с точками траверсирования для каждого канала, которые хранятся в базе (если в базе пусто, то список пустой)

                    rezh_count.Add(0);    //число режимов, которые есть в базе

                    List<List<Phys>> sech_ph_6_db = new List<List<Phys>>();//список по сечениям списков параметров
                    phys_6_db.Add(sech_ph_6_db);
                    phys_7_db.Add(sech_ph_6_db);

                    List<Rezh> rezhs_6 = new List<Rezh>();
                    rezhs_6_db.Add(rezhs_6);
                    rezhs_7_db.Add(rezhs_6);

                    List<Proch> proches_6 = new List<Proch>();
                    proches_6_db.Add(proches_6);

                    List<Integr> integrs_7 = new List<Integr>();
                    integrs_7_db.Add(integrs_7);
                }
                else
                {
                    sech_count_db.Add(Data.chans_results[i].sections.Count);
                    List<List<Phys>> sech_ph_6_db = new List<List<Phys>>();//список по сечениям списков параметров

                    List<Traversir> trav_lst_for_db = new List<Traversir>();
                    for (int j = 0; j < Data.chans_results[i].sections.Count; j++)
                    {
                        Traversir sechenie = new Traversir();

                        List<Phys> ph_6_db = new List<Phys>();

                        if (Data.chans_results[i].sections[j].pars == null)
                        {
                            sechenie.count = Data.chans_results[i].sections[j].coordinates.travers_table[0].cols[0].Count.ToString();
                            ObservableCollection<string> coord = Data.chans_results[i].sections[j].coordinates.column_headers;
                            if ((coord[0] == "X") || (coord[1] == "Y"))
                            {
                                sechenie.sys_coord = "Декартова";
                            }
                            if ((coord[0] == "a") || (coord[1] == "R"))
                            {
                                sechenie.sys_coord = "Сферическая";
                            }

                            for (int l = 0; l < Data.chans_results[i].sections[j].trav_pars.column_headers.Count; l++)
                            {
                                Phys str = new Phys();
                                str.phys = Data.chans_results[i].sections[j].trav_pars.column_headers[l];
                                ph_6_db.Add(str); //список физических параметров, которые есть в БД
                            }

                        }
                        else
                        {

                            for (int l = 0; l < Data.chans_results[i].sections[j].pars.column_headers.Count; l++)
                            {
                                Phys str = new Phys();
                                str.phys = Data.chans_results[i].sections[j].pars.column_headers[l];
                                ph_6_db.Add(str); //список физических параметров, которые есть в БД
                            }

                        }

                        Traversir sech_db = new Traversir();
                        sech_db.count = sechenie.count;
                        sech_db.sys_coord = sechenie.sys_coord;
                        trav_lst_for_db.Add(sech_db);

                        sech_ph_6_db.Add(ph_6_db);//список по сечениям со списками параметров
                    }
                    chans_trav_db.Add(trav_lst_for_db);//список списков сечений с точками траверсирования для каждого канала, которые хранятся в базе (если в базе пусто, то список пустой)

                    phys_6_db.Add(sech_ph_6_db);  //список по каналам со списками по сечениям списков параметров

                    rezh_count.Add(Data.chans_results[i].rezh_num.Count);    //число режимов, которые есть в базе

                    List<Rezh> rezhs_6 = new List<Rezh>();  //список режимных параметров

                    for (int k = 0; k < Data.chans_results[i].rezh_par.column_headers.Count; k++)
                    {
                        Rezh str = new Rezh();
                        str.rezh = Data.chans_results[i].rezh_par.column_headers[k];
                        rezhs_6.Add(str);
                    }
                    rezhs_6_db.Add(rezhs_6);  //список по каналам со списками режимных параметров

                    List<Proch> proches_6 = new List<Proch>();

                    for (int k = 0; k < Data.chans_results[i].prochie_par.column_headers.Count; k++)
                    {
                        Proch str = new Proch();
                        str.proch = Data.chans_results[i].prochie_par.column_headers[k];
                        proches_6.Add(str);
                    }
                    proches_6_db.Add(proches_6);

                    List<List<Phys>> sech_ph_7_db = new List<List<Phys>>();  //список физических параметров по сечениям

                    List<Rezh> rezhs_7 = new List<Rezh>();
                    List<Integr> integrs_7 = new List<Integr>();

                    if (chan_obr[i] == 2) //обработанные результаты есть
                    {
                        for (int t = 0; t < sech_count_db[i]; t++)   //считывание физических параметров в сечениях
                        {
                            List<Phys> ph_7_db = new List<Phys>();  //список физических параметров

                            if (t < Data.chans_obr[i].sections.Count)
                            {
                                for (int j = 0; j < Data.chans_obr[i].sections[t].pars.column_headers.Count; j++)   //заполнение списка обрабатываемых параметров
                                {
                                    Phys str = new Phys();
                                    str.phys = Data.chans_obr[i].sections[t].pars.column_headers[j];
                                    ph_7_db.Add(str);
                                }
                            }
                            
                            sech_ph_7_db.Add(ph_7_db);
                        }

                        for (int k = 0; k < Data.chans_obr[i].rezh_par.column_headers.Count; k++)   //заполнение обрабатываемых режимных параметров
                        {
                            Rezh str = new Rezh();
                            str.rezh = Data.chans_obr[i].rezh_par.column_headers[k];
                            rezhs_7.Add(str);
                        }

                        for (int k = 0; k < Data.chans_obr[i].integr_par.column_headers.Count; k++)   //заполнение обрабатываемых интегральных параметров
                        {
                            Integr str = new Integr();
                            str.integr = Data.chans_obr[i].integr_par.column_headers[k];
                            integrs_7.Add(str);
                        }


                    }

                    phys_7_db.Add(sech_ph_7_db);
                    rezhs_7_db.Add(rezhs_7);
                    integrs_7_db.Add(integrs_7);

                }
            }


            sqlconn.Close();
        }
    }
}
