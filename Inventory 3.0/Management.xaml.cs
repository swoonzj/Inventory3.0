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
    /// Interaction logic for Management.xaml
    /// </summary>
    /// 
    public partial class Management : Window
    {
        public List<string> UPCsToDelete;

        Item managedItem = new Item();
        ObservableCollection<Item> managedItems = new ObservableCollection<Item>();
        ObservableCollection<Item> searchResults = new ObservableCollection<Item>();

        public Management()
        {
            //// For testing
            //List<int> quant = new List<int> { 1, 1, 1 };
            //List<string> upcs = new List<string> { "1111", "1" };
            //Item item1 = new Item("Item1", "Test", "12.99", quant, "3", "4", upcs);
            //Item item2 = new Item("Item2", "Test", "15.99", quant, "5", "6", upcs);
            //searchResults.Add(item1);
            //searchResults.Add(item2);

            UPCsToDelete = new List<string>();
            managedItems.Add(managedItem);
            //dgQuantities.ItemsSource = managedItems;
            InitializeComponent();

            //lvList.ItemsSource = searchResults;
            DataContext = managedItem;
            
            Search();
        }

        private void Search()
        {
            try
            {
                searchResults = new ObservableCollection<Item>(DBAccess.SQLTableToList(searchtext: txtSearch.Text));                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
            lvList.ItemsSource = searchResults;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void DetectEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }

        private void lvList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UPCsToDelete.Clear();
            List<Item> items = new List<Item>();
            foreach (Item item in lvList.SelectedItems)
            {
                items.Add(item);
            }            

            if (items.Count == 1)
            {
                managedItem = items[0].Clone();
                //lvUPC.IsEnabled = true;
                //btnAdd.IsEnabled = true;
                //btnRemove.IsEnabled = true;
            }
            else
            {
                managedItem = CompareSelection(items); 
                //lvUPC.IsEnabled = false;
                //btnAdd.IsEnabled = false;
                //btnRemove.IsEnabled = false;
            }
            DataContext = managedItem;
            if (managedItem != null) lvUPC.ItemsSource = managedItem.UPCs;
        }

        private Item CompareSelection(List<Item> items)
        {
            if (items.Count == 0)
            {
                return null;
            }

            if (items.Count == 1) 
                return items[0].Clone();

            Item selection = items[0].Clone();
            
            foreach (Item item in items)
            {
                if (item.name != selection.name) selection.name = null;
                if (item.system != selection.system) selection.system = null;
                if (item.price != selection.price) selection.price = decimal.MinValue;
                if (item.tradeCash != selection.tradeCash) selection.tradeCash = decimal.MinValue;
                if (item.tradeCredit != selection.tradeCredit) selection.tradeCredit = decimal.MinValue;
                if (item.SQLid != selection.SQLid) selection.SQLid = 0;
                // Quantities
                if (item.quantity[0] != selection.quantity[0]) selection.quantity[0] = int.MinValue;
                if (item.quantity[1] != selection.quantity[1]) selection.quantity[1] = int.MinValue;
                if (item.quantity[2] != selection.quantity[2]) selection.quantity[2] = int.MinValue;
                // UPCs
                foreach(string upc in item.UPCs){
                    if (selection.UPCs.Contains(upc))
                }
            }           

            return selection;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {              
            // Save item information
            MessageBoxResult result = MessageBox.Show("Save changes?", "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {                
                foreach (Item item in lvList.SelectedItems)
                {
                    Item newItem = item.Clone();
                    if (newItem.quantity.Count != 3) newItem.quantity = new List<int> { 0, 0, 0 };

                    if (txtName.IsEnabled == true && !String.IsNullOrWhiteSpace(txtName.Text))
                        newItem.name = txtName.Text;
                    if (!String.IsNullOrWhiteSpace(txtSystem.Text))
                        newItem.system = txtSystem.Text;
                    if (!String.IsNullOrWhiteSpace(txtPrice.Text))
                        newItem.price = Convert.ToDecimal(txtPrice.Text);
                    if (!String.IsNullOrWhiteSpace(txtStore.Text))
                        newItem.quantity[0] = Convert.ToInt32(txtStore.Text);
                    if (!String.IsNullOrWhiteSpace(txtOutBack.Text))
                        newItem.quantity[1] = Convert.ToInt32(txtOutBack.Text);
                    if (!String.IsNullOrWhiteSpace(txtStorage.Text))
                        newItem.quantity[2] = Convert.ToInt32(txtStorage.Text);
                    if (!String.IsNullOrWhiteSpace(txtCash.Text))
                        newItem.tradeCash = Convert.ToDecimal(txtCash.Text);
                    if (!String.IsNullOrWhiteSpace(txtCredit.Text))
                        newItem.tradeCredit = Convert.ToDecimal(txtCredit.Text);

                    DBAccess.SaveItemChanges(newItem);

                    // Add new UPCs
                    List<string> newUPCs = new List<string>();
                    foreach (string UPC in lvUPC.Items) // Get all UPCs from table
                    {
                        newUPCs.Add(UPC);
                    }

                    DBAccess.AddUPCs(newUPCs, item.SQLid);

                    if (UPCsToDelete.Count > 0)
                    {
                        DBAccess.RemoveUPCs(UPCsToDelete, item.SQLid);
                        UPCsToDelete.Clear();
                    }
                    
                    
                }

                MessageBox.Show("Saved.");
                Search();
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Make sure there is text in the field. If not, do nothing.
            if (txtUPC.Text == "") return;

            managedItem.UPCs.Add(txtUPC.Text);
            CollectionViewSource.GetDefaultView(lvUPC.ItemsSource).Refresh();
            txtUPC.Text = "";
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
                managedItem.UPCs.Remove(upc);
            }

            CollectionViewSource.GetDefaultView(lvUPC.ItemsSource).Refresh();
        }

        private void menuAddNewItem_Click(object sender, RoutedEventArgs e)
        {
            Window addNewItem = new AddNewItem();
            addNewItem.Show();
        }        
    }
}
