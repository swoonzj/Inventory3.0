﻿using System;
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
    public partial class MainWindow : Window
    {
        decimal cashTotal = 0;
        decimal creditTotal = 0;

        ObservableCollection<Item> cart = new ObservableCollection<Item>();
        string keyboardInput;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                lvCart.ItemsSource = cart;
                List<Item> items = new List<Item>();
                //items.Add(new Item("Game", "System!", 2.99m, 1, .5m, 1m, "1"));
                //items.Add(new Item("Game 2", "System!", 3.99m, 1, .5m, 1m, "2"));
                //items.Add(new Item("Game 47", "System!", 9.99m, 3, 2m, 3m, "000023"));
                items = DBAccess.SQLTableToList(TableNames.INVENTORY);
                lvList.ItemsSource = items;
                UpdateTotals();
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
            
            UpdateTotals();
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
            //MessageBox.Show(e.Key.ToString());
            // Keep accepting input until "RETURN" is hit
            if (e.Key != Key.Return)
            {
                keyboardInput += e.Key.ToString();
            }
            // When "RETURN" is hit, look up UPC & add item to currently focused cart
            else
            {
                MessageBox.Show(keyboardInput);
                //((Cart)((ListView)sender).Tag).AddItemFromUPC(TableNames.INVENTORY, keyboardInput);
                keyboardInput = "";
                UpdateTotals();
            }
        }
    }
}
