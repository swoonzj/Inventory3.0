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
        List<string> quantities = new List<string> {"0","0","0"};

        public AddNewItem()
        {
            InitializeComponent();
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
                newItem.quantity = Convert.ToInt32(txtQuantity.Text);
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
            if (txtUPC.Text == "") return;

            lvUPC.Items.Add(txtUPC.Text);
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            List<string> upcs = new List<string>(lvUPC.ItemsSource as List<string>); //!!!!!!
            foreach (string upc in lvUPC.SelectedItems)
            {
                upcs.Remove(upc);
            }
            lvUPC.ItemsSource = upcs;
        }

        private void cmboInventorySelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Array Indexes :
            // Store = 0
            // OutBack = 1
            // Storage = 2

            // Assign changed values
            if (e.RemovedItems.Count > 0)
            {
                if (e.RemovedItems[0] == cmboStore)
                {
                    quantities[0] = txtQuantity.Text;
                }
                if (e.RemovedItems[0] == cmboOutBack)
                {
                    quantities[1] = txtQuantity.Text;
                }
                if (e.RemovedItems[0] == cmboStorage)
                {
                    quantities[2] = txtQuantity.Text;
                }
            }
            // Display selected values
            if (e.AddedItems[0] == cmboStore)
            {
                txtQuantity.Text = quantities[0];
            }
            if (e.AddedItems[0] == cmboOutBack)
            {
                txtQuantity.Text = quantities[1];
            }
            if (e.AddedItems[0] == cmboStorage)
            {
                txtQuantity.Text = quantities[2];
            }
        }


    }
}
