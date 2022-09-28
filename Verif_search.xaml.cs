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
    /// Логика взаимодействия для Verif_search.xaml
    /// </summary>
    public partial class Verif_search : Page
    {
        double accuracy = 0;
        public List<TextBox> list_txtbox = new List<TextBox>();
        public Verif_search()
        {
            InitializeComponent();
            List<string[]> list_params = new List<string[]>();
            string[] new_par1 = new string[2] { "Относительная длина", "txtbox_RelLength" };
            list_params.Add(new_par1);
            string[] new_par2 = new string[2] { "Число Рейнольдса", "txtbox_Re" };
            list_params.Add(new_par2);
            string[] new_par3 = new string[2] { "Высота ячейки первого призматического слоя", "txtbox_CellHeight" };
            list_params.Add(new_par3);
            string[] new_par4 = new string[2] { "Отношение высот слоев", "txtbox_LHRatio" };
            list_params.Add(new_par4);
            string[] new_par5 = new string[2] { "Количество элементов", "txtbox_ElNumber" };
            list_params.Add(new_par5);

            for (int i = 0; i < list_params.Count; i++)
            {
                add_param(list_params[i][0], list_params[i][1]);
            }

            if (Data.verif_Options.task)
            {
                txtblock_header.Text += "Прямая задача";
                //txtbox_accuracy.IsEnabled = false;
                butt_calculate_or_exit.Content = "Выход";
                txtblock_parname.Text += "Коэффициент потерь (погрешность)";
            }
            else
            {
                butt_calculate_accuracy.Visibility = Visibility.Hidden;
                //stackpanel.IsEnabled = false;
                txtblock_header.Text += "Обратная задача";
                txtbox_accuracy.IsEnabled = true;
                butt_calculate_or_exit.Content = "Рассчитать";
                dialog_accuracy.IsOpen = true;

                switch (Data.verif_Options.model)
                {
                    case "BLHeightModel_miltPred":
                        txtblock_parname.Text += "Общая высота пограничного слоя";
                        break;
                    case "GCSizeModel_multiPred":
                        txtblock_parname.Text += " Величина глобальной ячейки";
                        break;
                    case "LayerNumberModel_multiPred":
                        txtblock_parname.Text += "Число слоёв";
                        break;
                    case "TurbModel_multiPred":
                        txtblock_parname.Text += "Модель турбулентности";
                        break;
                    case "YModel_multiPred":
                        txtblock_parname.Text += "Y+";
                        break;
                    case "Все модели":
                        txtblock_parname.Text += "все заданные параметры";
                        break;
                }
            }
            
        }

        public void add_param(string txtblock_text, string txtbox_name)
        {
            TextBlock txt_block = new TextBlock();
            txt_block.Text = txtblock_text;

            TextBox txt_box = new TextBox();
            txt_box.BorderBrush = txtbox_y.BorderBrush;
            txt_box.Name = txtbox_name;
            list_txtbox.Add(txt_box);

            StackPanel stack = new StackPanel();
            stack.HorizontalAlignment = HorizontalAlignment.Right;
            stack.Orientation = Orientation.Horizontal;
            stack.Children.Add(txt_block);
            stack.Children.Add(txt_box);

            if (Data.verif_Options.task)
            {
                stackpanel.Children.Add(stack);
            }
            else
            {
                stackpanel_dialog.Children.Add(stack);
            }
        }

        private void butt_calculate_or_exit_Click(object sender, RoutedEventArgs e)
        {
            if (Data.verif_Options.task)
            {
                dialog_accuracy.IsOpen = false;
            }
            else
            {
                try
                {
                    accuracy = Convert.ToSingle(txtbox_accuracy.Text);
                    MultiTarget_prediction predictor = new MultiTarget_prediction(
                        testFile_path: "trubi.csv",
                        y_model: "YModel_multiPred.zip",
                        BLHeight_model: "BLHeightModel_miltPred.zip",
                        GCSize_model: "GCSizeModel_multiPred.zip",
                        LayerNumber_model: "LayerNumberModel_multiPred.zip",
                        TurbModel_model: "TurbModel_multiPred.zip"
                    );

                    MultiTarget_prediction.ModelInput input = new MultiTarget_prediction.ModelInput()
                    {
                        RelLength = Convert.ToSingle(list_txtbox[0].Text),
                        CellHeight = Convert.ToSingle(list_txtbox[2].Text),
                        LHRatio = Convert.ToSingle(list_txtbox[3].Text),
                        ElNumber = Convert.ToSingle(list_txtbox[4].Text),
                        LossFactorCFX = Convert.ToSingle(txtbox_accuracy.Text),
                        Re = Convert.ToSingle(list_txtbox[1].Text),
                    };
                    var res = predictor.Predict(input);

                    switch (Data.verif_Options.model)
                    {
                        case "BLHeightModel_miltPred":
                            txtbox_BLHeight.Text = res.BLHeight.ToString();
                            stackpanel_GCSize.IsEnabled = false;
                            stackpanel_LayerNumber.IsEnabled = false;
                            stackpanel_TurbModel.IsEnabled = false;
                            stackpanel_Y.IsEnabled = false;
                            break;
                        case "GCSizeModel_multiPred":
                            txtbox_GCSize.Text = res.GCSize.ToString();
                            stackpanel_LayerNumber.IsEnabled = false;
                            stackpanel_TurbModel.IsEnabled = false;
                            stackpanel_Y.IsEnabled = false;
                            stackpanel_BLHeight.IsEnabled = false;
                            break;
                        case "LayerNumberModel_multiPred":
                            txtbox_LayerNumber.Text = res.LayerNumber.ToString();
                            stackpanel_GCSize.IsEnabled = false;
                            stackpanel_TurbModel.IsEnabled = false;
                            stackpanel_Y.IsEnabled = false;
                            stackpanel_BLHeight.IsEnabled = false;
                            break;
                        case "TurbModel_multiPred":
                            switch (res.TurbModel)
                            {
                                case "type1":
                                    txtbox_TurbModel.Text = "k-ε";
                                    break;
                                case "type2":
                                    txtbox_TurbModel.Text = "SST";
                                    break;
                                case "type3":
                                    txtbox_TurbModel.Text = "K-ω";
                                    break;
                            }
                            stackpanel_GCSize.IsEnabled = false;
                            stackpanel_LayerNumber.IsEnabled = false;
                            stackpanel_Y.IsEnabled = false;
                            stackpanel_BLHeight.IsEnabled = false;
                            break;
                        case "YModel_multiPred":
                            txtbox_y.Text = res.y.ToString();
                            stackpanel_GCSize.IsEnabled = false;
                            stackpanel_LayerNumber.IsEnabled = false;
                            stackpanel_TurbModel.IsEnabled = false;
                            stackpanel_BLHeight.IsEnabled = false;
                            break;
                        case "Все модели":
                            txtbox_BLHeight.Text = res.BLHeight.ToString();
                            txtbox_GCSize.Text = res.GCSize.ToString();
                            txtbox_LayerNumber.Text = res.LayerNumber.ToString();
                            switch (res.TurbModel)
                            {
                                case "type1":
                                    txtbox_TurbModel.Text = "k-ε";
                                    break;
                                case "type2":
                                    txtbox_TurbModel.Text = "SST";
                                    break;
                                case "type3":
                                    txtbox_TurbModel.Text = "K-ω";
                                    break;
                            }
                            txtbox_y.Text = res.y.ToString();

                            break;
                    }

                    dialog_accuracy.IsOpen = false;
                }
                catch
                {
                    MessageBox.Show("Проверьте корректность введенных данных");
                }
            }
            

        }

        private void txtbox_accuracy_KeyDown(object sender, KeyEventArgs e)
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
               || (e.Key.ToString() == "OemComma")
               )
            {
                e.Handled = false;
                //return;
            }
            else
                e.Handled = true;
        }

        private void butt_calculate_accuracy_Click(object sender, RoutedEventArgs e)
        {
            StackPanel stack = (StackPanel)stackpanel.Children[5];
            var example = new Prediction.ModelInput()
            {
                RelLength = Convert.ToSingle(list_txtbox[0].Text),
                CellHeight = Convert.ToSingle(list_txtbox[2].Text),
                Y = Convert.ToSingle(txtbox_y.Text),
                LHRatio = Convert.ToSingle(list_txtbox[3].Text),
                LayerNumber = Convert.ToSingle(txtbox_LayerNumber.Text),
                BLHeight = Convert.ToSingle(txtbox_BLHeight.Text),
                ElNumber = Convert.ToSingle(list_txtbox[4].Text),
                GCSize = Convert.ToSingle(txtbox_GCSize.Text),
                TurbModel = txtbox_TurbModel.Text,
                Re = Convert.ToSingle(list_txtbox[1].Text),
            };

            Prediction pred = new Prediction("LossCoefModel.zip");
            var result = pred.Predict(example);
            txtbox_accuracy.Text = result.Score.ToString();
            dialog_accuracy.IsOpen = true;
        }
    }
}
