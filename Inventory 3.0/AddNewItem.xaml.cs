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
    /// Interaction logic for AddNewItem.xaml
    /// </summary>
    public partial class AddNewItem : Window
    {
        Item item = new Item();
        ObservableCollection<Item> items = new ObservableCollection<Item>();

        public AddNewItem()
        {
            items.Add(item);

            InitializeComponent();
            dgQuantities.ItemsSource = items;
            this.DataContext = item;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save item information
            MessageBoxResult result = MessageBox.Show("Save changes?", "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {

                if (DBAccess.AddNewItem(item))
                {
                    MessageBox.Show("Saved.");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("An error occurred. Data was NOT saved.");
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {            
            // Make sure there is text in the field. If not, do nothing.
            // Make sure there are no doubles
            if (txtUPC.Text == "") return;
            if (item.UPCs.Contains(txtUPC.Text)) return;

            item.UPCs.Add(txtUPC.Text);
            CollectionViewSource.GetDefaultView(lvUPC.ItemsSource).Refresh();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            List<string> selectedItems = new List<string>();
            foreach (string itemSelected in lvUPC.SelectedItems)
            {
                selectedItems.Add(itemSelected);
            }

            foreach (string upc in selectedItems)
            {
                item.UPCs.Remove(upc);
            }

            CollectionViewSource.GetDefaultView(lvUPC.ItemsSource).Refresh();
        }
    }
}
