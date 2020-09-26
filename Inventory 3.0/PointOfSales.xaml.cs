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
            //foreach (Item item in items) cart.Add(item);
            lvList.ItemsSource = items;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

        private async void Search(string searchString)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                List<Item> items = new List<Item>();
                items = await DBAccess.SQLTableToList(searchtext: searchString, limitResults: Settings.Default.limitSearchResults);
                lvList.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void addItemToCart(Item itemToAdd) 
        {
            int index = cart.IndexOf(itemToAdd);
            Item item = itemToAdd.Clone();
            if (index >= 0)
            {
                cart[index].quantity[0]++;
                ((Item)cart[index]).NotifyPropertyChanged("quantity");
                ((Item)cart[index]).NotifyPropertyChanged("priceTotal");
            }
            else
            {
                item.quantity[0] = 1;
                cart.Insert(0, item);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvList.SelectedItems)
            {
                addItemToCart(item);
            }
            UpdateTotals();
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
            int quant = 0;
            foreach (Item item in lvCart.Items)
            {
                total += item.priceTotal;
                quant += item.quantity[0];
            }
            txtTotal.Text = "Total:\t\t$" + total.ToString("0.00");
            txtItemCount.Text = "Items: " + quant.ToString();
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(((ListView)sender).ItemsSource);
        }
        
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private async void DetectUPCEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtUPCInput.Text == "") return; // Return if the input is empty. Prevents a SQL error.

                List<Item> items = await DBAccess.UPCLookup(txtUPCInput.Text); // Returns NULL if UPC does not match an item                

                if (items.Count != 0)
                {
                    if (items.Count > 1)
                    {
                        MultipleUPCHandler handler = new MultipleUPCHandler(items);
                        if (handler.ShowDialog() == true)
                        {
                            addItemToCart(handler.selectedItem);
                            handler.Close();
                        }
                    }
                    else
                    {
                        addItemToCart(items[0]);
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
                addItemToCart(item);
                UpdateTotals();
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
        
        private async void btnCheckout_Click(object sender, RoutedEventArgs e)
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
                    if (Settings.Default.deductSalesFromInventory)
                    {
                        await DBAccess.IncrementQuantities(item.SQLid, 0 - item.quantity[0], ColumnNames.STORE);
                    }
                }
                DBAccess.IncrementTransactionNumber();

                // Print Receipt
                if (Settings.Default.printReceipts)
                {
                    ReceiptGenerator generator = new ReceiptGenerator(cart.ToList<Item>(), checkout.checkout.ToList<Item>(), date, transactionNumber.ToString());
                    ReceiptPrinter printer = new ReceiptPrinter(generator.flowDoc);
                    printer.Print();
                }
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

        private void btnChangeQuantity_Click(object sender, RoutedEventArgs e)
        {
            int newValue;
            // verify textbox value
            if (int.TryParse(txtEdit.Text, out newValue))
            {
                foreach (var item in lvCart.SelectedItems)
                {
                    ((Item)item).quantity[0] = newValue;
                    ((Item)item).NotifyPropertyChanged("quantity");
                    ((Item)item).NotifyPropertyChanged("priceTotal");
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
            int count = getListQuantity(lvCart.SelectedItems);
            foreach (Item item in lvCart.SelectedItems)
            {
                item.price = 10m / count;
            }
            UpdateTotals();
        }

        private void btnFifteenForTwenty_Click(object sender, RoutedEventArgs e)
        {
            int count = getListQuantity(lvCart.SelectedItems);
            foreach (Item item in lvCart.SelectedItems)
            {
                item.price = 20m / count;
            }
            UpdateTotals();
        }

        private int getListQuantity(System.Collections.IList list)
        {
            int count = 0;
            foreach (Item item in list)
            {
                count += item.quantity[0];
            }
            return count;
        }

        private void btnAddUnlistedItem_Click(object sender, RoutedEventArgs e)
        {
            UnlistedItemPrompt prompt = new UnlistedItemPrompt();
            if (prompt.ShowDialog() == true)
            {
                cart.Insert(0, prompt.item);
                prompt.Close();
            }
        }

        private void btn10PercentOff_Click(object sender, RoutedEventArgs e)
        {
            // Apply discount to specific items
            if (lvCart.SelectedItems.Count != 0)
            {
                foreach (Item item in lvCart.SelectedItems)
                {
                    decimal newPrice = item.priceTotal * -.1m;
                    Item newItem = new Item(RegisterStrings.TEN_PERCENT + item.name, RegisterStrings.DISCOUNT, newPrice, 1, 0m, 0m, "0");
                    cart.Insert(lvCart.SelectedItems.IndexOf(item) + 1, newItem);
                }
                UpdateTotals();
            }
            // Apply discount to entire cart
            else
            {
                decimal newPrice = total * -.1m;
                Item newItem = new Item(RegisterStrings.TEN_PERCENT, RegisterStrings.DISCOUNT, newPrice, 1, 0m, 0m, "0");
                cart.Add(newItem);
            }
        }

        private void btn20PercentOff_Click(object sender, RoutedEventArgs e)
        {
            // Apply discount to specific items
            if (lvCart.SelectedItems.Count != 0)
            {
                foreach (Item item in lvCart.SelectedItems)
                {
                    decimal newPrice = item.priceTotal * -.2m;
                    Item newItem = new Item(RegisterStrings.TWENTY_PERCENT + item.name, RegisterStrings.DISCOUNT, newPrice, 1, 0m, 0m, "0");
                    cart.Insert(lvCart.SelectedItems.IndexOf(item) + 1, newItem);
                }
                UpdateTotals();
            }
            // Apply discount to entire cart
            else
            {
                decimal newPrice = total * -.2m;
                Item newItem = new Item(RegisterStrings.TWENTY_PERCENT, RegisterStrings.DISCOUNT, newPrice, 1, 0m, 0m, "0");
                cart.Add(newItem);
            }
        }

        private void btnPercentOff_Click(object sender, RoutedEventArgs e)
        {
            // Get percentage from textbox
            int input;
            decimal percentage;
            if (!int.TryParse(txtEdit.Text, out input))
            {
                MessageBox.Show("\"" + txtEdit.Text + "\" is not a valid percentage. Input must be a whole number (i.e. \"15\")");
                return;
            }
            percentage = input / 100m;

            // Apply discount to specific items
            if (lvCart.SelectedItems.Count != 0)
            {
                foreach (Item item in lvCart.SelectedItems)
                {
                    decimal newPrice = item.priceTotal * -percentage;
                    Item newItem = new Item(input.ToString() + "% off " + item.name, RegisterStrings.DISCOUNT, newPrice, 1, 0m, 0m, "0");
                    cart.Insert(lvCart.SelectedItems.IndexOf(item) + 1, newItem);
                }
                UpdateTotals();
            }
            // Apply discount to entire cart
            else
            {
                decimal newPrice = total * -percentage;
                Item newItem = new Item(input.ToString() + "% off", RegisterStrings.DISCOUNT, newPrice, 1, 0m, 0m, "0");
                cart.Add(newItem);
            }
        }

        #endregion
    }
}
