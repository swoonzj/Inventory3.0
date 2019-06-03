﻿using Inventory_3._0.Properties;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class TradeWindow : Window
    {
        decimal cashTotal = 0;
        decimal creditTotal = 0;

        ObservableCollection<Item> cart = new ObservableCollection<Item>();
        string keyboardInput;

        public TradeWindow()
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
                    cart.Add(items[0]);
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

        private void ManagementMenu_Click(object sender, RoutedEventArgs e)
        {
            Window management = new Management();
            management.Show();
        }

        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            cart.Clear();
        }

        private void btnCash_Click(object sender, RoutedEventArgs e)
        {
            Checkout(TransactionTypes.TRADE_CASH);
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

        private void btnChangeCash_Click(object sender, RoutedEventArgs e)
        {
            decimal newValue;
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
            }
            UpdateTotals();
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
            }
            UpdateTotals();
        }

        #endregion

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
            cart.Add(new Item("Round Up", "Adjust Cart", 0, 1, cashup, creditup,"-1"));
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
            cart.Add(new Item("Round Down", "Adjust Cart", 0, 1, cashdown, creditdown, "-1"));
        }

        private void btnAddValue_Click(object sender, RoutedEventArgs e)
        {
            decimal newValue;
            // verify textbox value
            if (decimal.TryParse(txtEdit.Text, out newValue))
            {
                cart.Add(new Item("Add Value", "Adjust Cart", 0, 1, newValue, newValue, "-1"));
            }
            else
            {
                MessageBox.Show("\"" + txtEdit.Text + "\" is not a valid number. Try again.", "Not a number. Try again, fool.", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }
    }
}