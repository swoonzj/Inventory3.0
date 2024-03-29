﻿using Inventory_3._0.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TradeWindow : SortableListViews
    {
        decimal cashTotal = 0;
        decimal creditTotal = 0;

        ObservableCollection<Item> cart = new ObservableCollection<Item>();        

        public TradeWindow()
        {
            try
            {
                //DBAccess.MigrateDatabase();
                InitializeComponent();
                //Search(String.Empty);
                lvCart.ItemsSource = cart;
                lvList.ContextMenu = new ListViewContextMenu(lvList);
                lvCart.ContextMenu = new ListViewContextMenu(lvCart);
                cart.CollectionChanged += (e, v) => UpdateTotals();
                lvList.PreviewMouseRightButtonDown += LvList_PreviewMouseRightButtonDown;
                lvCart.PreviewMouseRightButtonDown += LvList_PreviewMouseRightButtonDown;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

        private void LvList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default.Save();
            base.OnClosing(e);
        }

        private async void Search(string searchString)
        {
            try
            {
                List<Item> items = new List<Item>();
                items = await DBAccess.GetItemsAsList(searchtext: searchString, limitResults:Settings.Default.limitSearchResults);
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
            cashTotal = 0;
            creditTotal = 0;
            foreach (Item item in lvCart.Items)
            {
                cashTotal += item.tradeCash;
                creditTotal += item.tradeCredit;
            }
            txtTotalCash.Text = "Cash:\t\t$" + cashTotal.ToString("0.00");
            txtTotalCredit.Text = "\nCredit:\t\t$" + creditTotal.ToString("0.00");
        }

        private void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(((ListView)sender).ItemsSource);
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
                            handler.selectedItem.quantity[0] = 1;
                            cart.Insert(0, handler.selectedItem);
                            handler.Close();
                        }
                    }
                    else
                    {
                        items[0].quantity[0] = 1;
                        cart.Insert(0, items[0]);
                    }
                    UpdateTotals();
                }
                else
                    MessageBox.Show("Unknown UPC");

                txtUPCInput.Text = "";
            }
        }

        private void lvCart_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            txtUPCInput.Focus();
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

        private void SalesMenu_Click(object sender, RoutedEventArgs e)
        {
            Window sales = new PointOfSales();
            sales.Show();
        }

        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            cart.Clear();
        }

        private void btnCash_Click(object sender, RoutedEventArgs e)
        {
            Checkout(TransactionTypes.TRADE_CASH);
        }

        private void btnAddUnlistedItem_Click(object sender, RoutedEventArgs e)
        {
            // Prompt Unlisted Item Form !!!!!!!!!!!!!
            
            UnlistedItemPrompt prompt = new UnlistedItemPrompt(true);
            if (prompt.ShowDialog() == true)
            {
                cart.Insert(0, prompt.item);
                prompt.Close();
            }
        }

        private void btnCredit_Click(object sender, RoutedEventArgs e)
        {
            Checkout(TransactionTypes.TRADE_CREDIT);
        }

        private void Checkout(string transactionType)
        {
            try
            {
                int transactionNumber = DBAccess.GetNextUnusedTransactionNumber();
                string date = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
                foreach (Item item in cart)
                {
                    DBAccess.AddTransaction(item, transactionType, transactionNumber, date);
                }
                DBAccess.IncrementTransactionNumber();
                cart.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in Checkout:\n" + ex.Message);
            }
        }


        #region EditCart Methods

        private bool TestForMultipleValues(string s)
        {
            string[] newS = s.Split();
            if (newS.Length == 2)
            {
                return true;
            }
            return false;
        }

        private void HandleMultipleValues(Item item)
        {
            string[] values = txtEdit.Text.Split();

            decimal newCredit, newCash;
            // verify textbox value
            if (decimal.TryParse(values[0], out newCash))
            {                
                ((Item)item).tradeCash = newCash;                            
            }
            if (decimal.TryParse(values[1], out newCredit))
            {                
                ((Item)item).tradeCredit = newCredit;
            }
            else
            {
                MessageBox.Show("\"" + txtEdit.Text + "\" is not a valid number combo. Try again.", "Not a number. Try again, fool.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }            
        }

        private void btnChangeCash_Click(object sender, RoutedEventArgs e)
        {
            decimal newValue;
            
            // Test for multiple values
            if (TestForMultipleValues(txtEdit.Text))
            {
                foreach (Item item in lvCart.SelectedItems)
                {
                    HandleMultipleValues(item);
                }
                UpdateTotals();
                txtEdit.Clear();
                return;
            }
            // verify textbox value
            if (decimal.TryParse(txtEdit.Text, out newValue))
            {
                foreach (var item in lvCart.SelectedItems)
                {
                    ((Item)item).tradeCash = newValue;
                }
            }
            else
            {
                MessageBox.Show("\"" + txtEdit.Text + "\" is not a valid number. Try again.", "Not a number. Try again, fool.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            UpdateTotals();
            txtEdit.Clear();
        }

        private void btnChangeCredit_Click(object sender, RoutedEventArgs e)
        {
            decimal newValue;
            // verify textbox value
            if (decimal.TryParse(txtEdit.Text, out newValue))
            {
                foreach (var item in lvCart.SelectedItems)
                {
                    ((Item)item).tradeCredit = newValue;
                }
            }
            else
            {
                MessageBox.Show("\"" + txtEdit.Text + "\" is not a valid number. Try again.", "Not a number. Try again, fool.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }
            UpdateTotals();
            txtEdit.Clear();
        }
        
        private void btnRoundUp_Click(object sender, RoutedEventArgs e)
        {
            decimal cashup, creditup;
            decimal remainder = cashTotal % 5;
            if (remainder == 0) 
                cashup = 5;
            else 
                cashup = 5 - remainder;
            remainder = creditTotal % 5;
            if (remainder == 0)
                creditup = 5;
            else
                creditup = 5 - remainder;
            cart.Insert(0, new Item("Round Up", "Adjust Cart", 0, 1, cashup, creditup,"-1"));
        }

        private void btnRoundDown_Click(object sender, RoutedEventArgs e)
        {
            decimal cashdown, creditdown;
            decimal remainder = cashTotal % 5;
            if (remainder == 0)
                cashdown = -5;
            else
                cashdown = -remainder;
            remainder = creditTotal % 5;
            if (remainder == 0)
                creditdown = -5;
            else
                creditdown = -remainder;
            cart.Insert(0, new Item("Round Down", "Adjust Cart", 0, 1, cashdown, creditdown, "-1"));
        }

        private void btnAddValue_Click(object sender, RoutedEventArgs e)
        {
            decimal newValue = 0;
            // verify textbox value
            if (TestForMultipleValues(txtEdit.Text))
            {
                Item item = new Item("Add Value", "Adjust Cart", 0, 1, newValue, newValue, "-1");
                HandleMultipleValues(item);
                cart.Insert(0, item);
                UpdateTotals();
                txtEdit.Clear();
                return;
            }
            if (decimal.TryParse(txtEdit.Text, out newValue))
            {
                cart.Insert(0, new Item("Add Value", "Adjust Cart", 0, 1, newValue, newValue, "-1"));
                txtEdit.Clear();
            }
            else
            {
                MessageBox.Show("\"" + txtEdit.Text + "\" is not a valid number. Try again.", "Not a number. Try again, fool.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }


        #endregion

        private void btnAutoTrade_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvCart.SelectedItems)
            {
                item.AutoTradeValues();
            }
            UpdateTotals();
        }

        private void txtEdit_GotFocus(object sender, RoutedEventArgs e)
        {
            btnChangeCash.IsDefault = true;
        }

        private void txtEdit_LostFocus(object sender, RoutedEventArgs e)
        {
            btnChangeCash.IsDefault = false;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Adding Trade to Inventory is not yet implemented.");
        }        
    }
}
