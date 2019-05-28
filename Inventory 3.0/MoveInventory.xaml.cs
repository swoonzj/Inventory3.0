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
    /// Interaction logic for MoveInventory.xaml
    /// </summary>
    public partial class MoveInventory : Window
    {
        ObservableCollection<Item> searchResults = new ObservableCollection<Item>();
        ObservableCollection<Item> movingItems = new ObservableCollection<Item>();

        string keyboardInput;

        public MoveInventory()
        {
            InitializeComponent();
            lvMove.ItemsSource = movingItems;
            this.DataContext = movingItems;
            
            //Search();

            lvList.ItemsSource = searchResults;

            searchResults.Add(new Item("Test1", "Test System", 9.99m, 5,2m,3m,"12345"));
            searchResults.Add(new Item("Test2", "Test System", 9.99m, 5, 2m, 3m, "12345"));
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

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvList.SelectedItems)
            {
                AddItem(item);
            }            
        }

        private void AddItem(Item item)
        {
            if (movingItems.Contains(item))
            {
                movingItems[movingItems.IndexOf(item)].quantity[0] += 1;
            }
            else
            {
                item.quantity[0] = 1;
                movingItems.Add(item);
            }
            item.NotifyPropertyChanged("quantity");
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in movingItems)
            {
                // Remove FROM inventory
                if (radioFromOutBack.IsChecked == true)
                    DBAccess.IncrementQuantities(item.SQLid, -item.quantity[0], ColumnNames.OUTBACK);
                else if (radioFromSalesFloor.IsChecked == true)
                    DBAccess.IncrementQuantities(item.SQLid, -item.quantity[0], ColumnNames.STORE);
                else if (radioFromStorage.IsChecked == true)
                    DBAccess.IncrementQuantities(item.SQLid, -item.quantity[0], ColumnNames.STORAGE);
                else if (radioNewItem.IsChecked == true) { } // If New Item is checked, don't do anything.
                else
                    MessageBox.Show("Please choose a source under the \"FROM:\".", "You didn't choose an option.", MessageBoxButton.OK, MessageBoxImage.Hand);

                // Add TO inventory
                if (radioToOutBack.IsChecked == true)
                    DBAccess.IncrementQuantities(item.SQLid, item.quantity[0], ColumnNames.OUTBACK);
                else if (radioToSalesFloor.IsChecked == true)
                    DBAccess.IncrementQuantities(item.SQLid, item.quantity[0], ColumnNames.STORE);
                else if (radioToStorage.IsChecked == true)
                    DBAccess.IncrementQuantities(item.SQLid, item.quantity[0], ColumnNames.STORAGE);
                else
                    MessageBox.Show("Please choose a destination under the \"TO:\".", "You didn't choose an option.", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Item> itemsToRemove = new ObservableCollection<Item>();
            foreach (Item item in lvList.SelectedItems)
            {
                itemsToRemove.Add(item);
            }
            foreach (Item item in itemsToRemove)
            {
                movingItems.Remove(item);
            }
        }

        private void btnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            movingItems.Clear();
        }

        private void btnChangeQuantity_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvList.SelectedItems)
            {
                try
                {
                    item.quantity[0] = Convert.ToInt32(txtChangeQuantity.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            movingItems[0].NotifyPropertyChanged("quantity");
        }


        #region UPC Scanner Methods


        // Handle input from Price Scanner
        private void lvCart_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.InputSource.ToString());
            // Keep accepting input until "RETURN" is hit
            if (e.Key != Key.Return)
            {
                keyboardInput += processScannerInput(e.Key);
            }
            // When "RETURN" is hit, look up UPC & add item to currently focused cart
            else
            {
                if (keyboardInput == "") return; // Return if the input is empty. Prevents a SQL error.

                List<Item> items = DBAccess.UPCLookup(keyboardInput); // Returns NULL if UPC does not match an item                

                // HANDLE MULTIPLE ITEMS !!!!!!!!!!!!!
                if (items.Count != 0)
                {
                    movingItems.Add(items[0]);
                }
                else
                    MessageBox.Show("Unknown UPC");

                keyboardInput = "";
            }
        }

        // UPC Scanner registers Digits differently from the normal keyboard digits
        private string processScannerInput(Key key)
        {
            switch (key)
            {
                case Key.D0:
                    return "0";
                case Key.D1:
                    return "1";
                case Key.D2:
                    return "2";
                case Key.D3:
                    return "3";
                case Key.D4:
                    return "4";
                case Key.D5:
                    return "5";
                case Key.D6:
                    return "6";
                case Key.D7:
                    return "7";
                case Key.D8:
                    return "8";
                case Key.D9:
                    return "9";
                default:
                    return key.ToString();
            }
        }

        private void lvList_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Give focus to lvCart, so that the KeyDown Event actually works. (Only works if you click on the column headers, otherwise)
            lvList.Focus();
        }

        #endregion
    }
}
