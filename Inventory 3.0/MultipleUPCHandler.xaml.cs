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
    public partial class MultipleUPCHandler : SortableListViews
    {
        public Item selectedItem;
        public MultipleUPCHandler(List<Item> items)
        {
            InitializeComponent();
            lvList.ItemsSource = items;
            MessageBox.Show("This UPC has multiple items associated with it.\nPlease select the correct item.");
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (lvList.SelectedItems.Count != 1) MessageBox.Show("Please select one item.");
            else
            {
                selectedItem = (Item) lvList.SelectedItem;
                this.DialogResult = true;
            }
        }
    }
}
