using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для Verif_model.xaml
    /// </summary>
    public partial class Verif_model : Page
    {
        Verification verif_wind;
        public Verif_model()
        {
            InitializeComponent();
            verif_wind = (Verification)Application.Current.Windows.OfType<Window>().Where(x => x.Name == "Verification_wind").FirstOrDefault();
            Data.verif_Options.list_model = new ObservableCollection<string>();
            //Data.verif_Options.list_model.Add("модель 1");
            //Data.verif_Options.list_model.Add("модель 2");
            grid_verif_task.DataContext = Data.verif_Options;
            Data.verif_Options.task = true;
            Data.verif_Options.count_pars = false;
        }

        private void combox_model_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            txtbox_desription.Text = "";
            if (combox_model.SelectedItem != null)
            {
                Data.verif_Options.model = combox_model.SelectedItem.ToString();
                verif_wind.Butt_next.IsEnabled = true;

                switch (combox_model.SelectedItem)
                {
                    case "LossCoefModel":
                        txtbox_desription.Text = "Предсказываемый параметр - Коэффициент потерь, полученный в CFX (погрешность). Тип задачи - регрессия (предсказывает число). "+
                            "Название метода - LightGbmRegression. Метрика 'R-квадрат' - 0,89. Метрика 'Абсолютная потеря' - 0,001.";
                        break;
                    case "BLHeightModel_miltPred":
                        txtbox_desription.Text = "Предсказываемый параметр - Общая высота пограничного слоя. Тип задачи - регрессия (предсказывает число). " +
                            "Название метода - LightGbmRegression. Метрика 'R-квадрат' - 0,98. Метрика 'Абсолютная потеря' - 0,03.";
                        break;
                    case "GCSizeModel_multiPred":
                        txtbox_desription.Text = "Предсказываемый параметр - Величина глобальной ячейки. Тип задачи - регрессия (предсказывает число). " +
                            "Название метода - FastTreeRegression. Метрика 'R-квадрат' - 0,97. Метрика 'Абсолютная потеря' - 0,11.";
                        break;
                    case "LayerNumberModel_multiPred":
                        txtbox_desription.Text = "Предсказываемый параметр - Число слоёв. Тип задачи - Классификация. " +
                            "Название метода - FastTreeOva. Точность - 93,96%.";
                        break;
                    case "TurbModel_multiPred":
                        txtbox_desription.Text = "Предсказываемый параметр - Модель турбулентности. Тип задачи - Классификация. " +
                            "Название метода - FastTreeOva. Точность - 50,59%.";
                        break;
                    case "YModel_multiPred":
                        txtbox_desription.Text = "Предсказываемый параметр - Y+. Тип задачи - регрессия (предсказывает число). " +
                            "Название метода - LbfgsPoissonRegression. Метрика 'R-квадрат' - 0,98. Метрика 'Абсолютная потеря' - 1,12.";
                        break;

                }
                txtbox_desription.Text += "\r\n\r\nR-квадрат (R2) или коэффициент детерминации обозначает совокупную прогнозирующую способность модели в диапазоне от -inf до 1,00. Чем ближе к 1,00, тем выше качество.\r\n" +
                            "Абсолютная потеря, или средняя абсолютная погрешность (MAE) , измеряет, насколько прогнозы близки к фактическим результатам. Это среднее значение всех ошибок модели, " +
                            "где ошибка модели — абсолютное расстояние между значением прогнозируемой метки и значением правильной метки. Чем ближе к 0,00, тем выше качество.";
            }
           
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radiobut = sender as RadioButton;
            if (Data.verif_Options.model != null)
            {
                verif_wind.Butt_next.IsEnabled = true;
            }

            Data.verif_Options.list_model.Clear();
            switch (radiobut.Content)
            {
                case "Прямая задача":
                    Data.verif_Options.list_model.Add("LossCoefModel");
                    break;
                case "Обратная задача (все параметры)":
                    Data.verif_Options.list_model.Add("Все модели");
                    break;
                case "Обратная задача (один параметр)":
                    Data.verif_Options.list_model.Add("BLHeightModel_miltPred");
                    Data.verif_Options.list_model.Add("GCSizeModel_multiPred");
                    Data.verif_Options.list_model.Add("LayerNumberModel_multiPred");
                    Data.verif_Options.list_model.Add("TurbModel_multiPred");
                    Data.verif_Options.list_model.Add("YModel_multiPred");
                    break;
            }
            
        }

    }
    public class Verif_options
    {
        public bool task { get; set; }  //true - прямая задача, false - обратная
        public bool count_pars { get; set; }    //true - много параметров, false - один параметр

        public string model { get; set; }   //название модели

        public ObservableCollection<string> list_model { get; set; }    //список моделей
    }
}
