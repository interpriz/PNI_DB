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

namespace БД_НТИ
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        bool close = true;
        public Menu()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (close)
            {
                User.LogOUT($"{User.login}_online");
                Application.Current.Shutdown();
            }
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            switch (item.Name)
            {
                case "Sprav":

                    break;

                case "Polz":
                    this.Hide();
                    Window newin1 = new Users();
                    newin1.ShowDialog();
                    break;

                case "ExpOldTask":
                case "ExpNewTask":
                    this.Hide();
                    Window newin2 = new Experiment_add(item.Name);
                    newin2.ShowDialog();
                    break;

                case "ExpSearch":
                    this.Hide();
                    Window newin4 = new Experiment_search();
                    newin4.ShowDialog();
                    break;

                case "ModelOldTask":
                case "ModelNewTask":
                    this.Hide();
                    Window newin5 = new Modeling_add(item.Name);
                    newin5.ShowDialog();
                    break;
            }
            
        }

    }
}
