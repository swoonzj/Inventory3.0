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
using System.Windows.Shapes;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for MultipleUPCHandler.xaml
    /// </summary>
    public partial class MultipleUPCHandler : Window
    {
        public MultipleUPCHandler(ObservableCollection<Item> items)
        {
            InitializeComponent();
            lvList.ItemsSource = items;
        }

        private Item btnOK_Click(object sender, RoutedEventArgs e)
        {
            return (Item)lvList.SelectedItem;
        }
    }
}
