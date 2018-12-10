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
        List<int> quantities = new List<int> {0,0,0};

        public AddNewItem()
        {
            InitializeComponent();
            dgQuantities.ItemsSource = quantities;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save item information
            MessageBoxResult result = MessageBox.Show("Save changes?", "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {

                Item newItem = new Item();

                newItem.name = txtName.Text;
                newItem.system = txtSystem.Text;
                newItem.price = Convert.ToDecimal(txtPrice.Text);
                newItem.tradeCash = Convert.ToDecimal(txtCash.Text);
                newItem.tradeCredit = Convert.ToDecimal(txtCredit.Text);

                // Add new UPCs
                List<string> newUPCs = new List<string>();
                foreach (string UPC in lvUPC.Items) // Get all UPCs from table
                {
                    newUPCs.Add(UPC);
                }

                newItem.UPCs = newUPCs;

                DBAccess.AddNewItem(newItem, Properties.Settings.Default.CurrentInventory);

                MessageBox.Show("Saved.");
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {            
            // Make sure there is text in the field. If not, do nothing.
            // Make sure there are no doubles
            if (txtUPC.Text == "") return;
            if (lvUPC.Items.Contains(txtUPC.Text)) return;

            lvUPC.Items.Add(txtUPC.Text);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            List<string> selectedItems = new List<string>();
            foreach (string itemSelected in lvUPC.SelectedItems)
            {
                selectedItems.Add(itemSelected);
            }

            foreach (string item in selectedItems)
            {
                lvUPC.Items.Remove(item);
            }
        }        
    }
}
