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
    public partial class PointOfSales : SortableListViews
    {
        decimal total = 0;

        ObservableCollection<Item> cart = new ObservableCollection<Item>();

        public PointOfSales()
        {
            try
            {
                InitializeComponent();
                lvCart.ItemsSource = cart;
                cart.CollectionChanged += (e, v) => UpdateTotals();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

        public PointOfSales(List<Item> items) : this()
        {
            foreach (Item item in items) cart.Add(item);
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
                items = DBAccess.SQLTableToList(searchtext: searchString, limitResults: menuLimitSearchResults.IsChecked);
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
                item.quantity[0] = 1;
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
            txtTotal.Text = "Total:\t\t$" + total.ToString("0.00");
            txtItemCount.Text = "Items: " + lvCart.Items.Count.ToString();
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(((ListView)sender).ItemsSource);
        }
        
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void DetectUPCEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtUPCInput.Text == "") return; // Return if the input is empty. Prevents a SQL error.

                List<Item> items = DBAccess.UPCLookup(txtUPCInput.Text); // Returns NULL if UPC does not match an item                

                if (items.Count != 0)
                {
                    if (items.Count > 1)
                    {
                        MultipleUPCHandler handler = new MultipleUPCHandler(items);
                        if (handler.ShowDialog() == true)
                        {
                            cart.Insert(0, handler.selectedItem);
                            handler.Close();
                        }
                    }
                    else
                    {
                        cart.Insert(0, items[0]);
                    }
                    UpdateTotals();
                }
                else
                    MessageBox.Show("Unknown UPC");

                txtUPCInput.Text = "";
            }
        }

        private void DetectEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search(txtSearch.Text);
            }
        }
        private void lvCart_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            txtUPCInput.Focus();
        }

        private void GiveFocusTo_txtUPCInput(object sender, MouseButtonEventArgs e)
        {
            // Give focus to lvCart, so that the KeyDown Event actually works. (Only works if you click on the column headers, otherwise)
            txtUPCInput.Focus();
        }

        private void lvList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as Item;
            if (item != null)
            {
                item.quantity[0] = 1;
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
            if (cart.Count == 0)
            {
                MessageBox.Show("There is nothing in the cart.");
                return;
            }
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

                // Print Receipt!!!!
                ReceiptGenerator generator = new ReceiptGenerator(cart.ToList<Item>(), checkout.checkout.ToList<Item>(), date, transactionNumber.ToString());
                ReceiptPrinter printer = new ReceiptPrinter(generator.flowDoc);
                printer.Print();
                checkout.Close();
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


        #endregion

        private void btnAddUnlistedItem_Click(object sender, RoutedEventArgs e)
        {
            UnlistedItemPrompt prompt = new UnlistedItemPrompt();
            if (prompt.ShowDialog() == true)
            {
                cart.Add(prompt.item);
                prompt.Close();
            }
        }

        


    }
}
