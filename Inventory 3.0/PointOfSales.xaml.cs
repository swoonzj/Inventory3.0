using Inventory_3._0.Properties;
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

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for PointOfSales.xaml
    /// </summary>
    public partial class PointOfSales : Window
    {
        decimal total = 0;

        ObservableCollection<Item> cart = new ObservableCollection<Item>();
        string keyboardInput;

        public PointOfSales()
        {
            try
            {
                //DBAccess.MigrateDatabase();
                InitializeComponent();
                //Search(String.Empty);
                lvCart.ItemsSource = cart;
                cart.CollectionChanged += (e, v) => UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

        private void Search(string searchString)
        {
            try
            {
                List<Item> items = new List<Item>();
                items = DBAccess.SQLTableToList(searchtext: searchString);
                lvList.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvList.SelectedItems)
            {
                cart.Insert(0, item);
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Item> itemsToRemove = new ObservableCollection<Item>();
            foreach (Item item in lvCart.SelectedItems)
            {
                itemsToRemove.Add(item);
            }
            foreach (Item item in itemsToRemove)
            {
                cart.Remove(item);
            }
        }

        private void UpdateTotals()
        {
            total = 0;
            foreach (Item item in lvCart.Items)
            {
                total += item.price;
            }
            txtTotal.Text = "Cash:\t\t$" + total.ToString("0.00");
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(((ListView)sender).ItemsSource);
        }

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
                    cart.Insert(0, items[0]);
                    UpdateTotals();
                }
                else
                    MessageBox.Show("Unknown UPC");

                keyboardInput = "";
            }
        }

        // UPC Scanner registers Digits differently from the normal keyboard digits
        private string processScannerInput(Key key)
        {
            switch (key){
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void DetectEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search(txtSearch.Text);
            }
        }

        private void lvCart_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Give focus to lvCart, so that the KeyDown Event actually works. (Only works if you click on the column headers, otherwise)
            lvCart.Focus();
        }

        private void lvList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as Item;
            if (item != null)
            {
                cart.Insert(0, item);
            }
        }

        private void ManagementMenu_Click(object sender, RoutedEventArgs e)
        {
            Window management = new Management();
            management.Show();
        }

        private void TradeMenu_Click(object sender, RoutedEventArgs e)
        {
            Window trade = new TradeWindow();
            trade.Show();
        }

        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            cart.Clear();
        }
        
        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            Checkout checkout = new Checkout(total);
            if (checkout.ShowDialog() == true)
            {
                MessageBox.Show("Great.");
                // Log transaction
                
                string date = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
                int transactionNumber = DBAccess.GetNextUnusedTransactionNumber();
                foreach (Item item in cart)
                {
                    DBAccess.AddTransaction(item, TransactionTypes.SALE, transactionNumber, date);
                }
                DBAccess.IncrementTransactionNumber();
                cart.Clear();
            }
        }


        #region EditCart Methods

        private void btnChangePrice_Click(object sender, RoutedEventArgs e)
        {
            decimal newValue;
            // verify textbox value
            if (decimal.TryParse(txtEdit.Text, out newValue))
            {
                foreach (var item in lvCart.SelectedItems)
                {
                    ((Item)item).price = newValue;
                }
            }
            else
            {
                MessageBox.Show("\"" + txtEdit.Text + "\" is not a valid number. Try again.", "Not a number. Try again, fool.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            UpdateTotals();
            txtEdit.Clear();
        }

        
        #endregion

        private void btnSixForTen_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvCart.SelectedItems)
            {
                item.price = 10m / lvCart.SelectedItems.Count;
            }
            UpdateTotals();
        }

        private void btnTenForTwenty_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvCart.SelectedItems)
            {
                item.price = 20m / lvCart.SelectedItems.Count;
            }
            UpdateTotals();
        }

        
    }
}
