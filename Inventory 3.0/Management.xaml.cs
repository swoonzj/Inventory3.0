﻿using Inventory_3._0.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for Management.xaml
    /// </summary>
    /// 
    public partial class Management : SortableListViews
    {
        public List<string> UPCsToDelete;

        Item managedItem = new Item();
        ObservableCollection<Item> managedItems = new ObservableCollection<Item>();
        ObservableCollection<Item> searchResults = new ObservableCollection<Item>();

        public Management()
        {
            //// For testing
            //List<string> upcs = new List<string> { "1111", "1" };
            //Item item1 = new Item("Item1", "Test", "12.99", new List<int> { 1, 2, 3 }, "3", "4", upcs);
            //Item item2 = new Item("Item2", "Test", "15.99", new List<int> { 10, 20, 30 }, "5", "6", upcs);
            //searchResults.Add(item1);
            //searchResults.Add(item2);

            UPCsToDelete = new List<string>();
            managedItems.Add(managedItem);
            //dgQuantities.ItemsSource = managedItems;
            InitializeComponent();

            //lvList.ItemsSource = searchResults;
            DataContext = managedItem;
            lvList.ContextMenu = new ListViewContextMenu(lvList);
            lvList.PreviewMouseRightButtonDown += LvList_PreviewMouseRightButtonDown;
            //Search();
        }

        private void LvList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void Search()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                searchResults = new ObservableCollection<Item>(await DBAccess.GetItemsAsList(searchtext: txtSearch.Text, limitResults: Settings.Default.limitSearchResults));                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
            lvList.ItemsSource = searchResults;
            Mouse.OverrideCursor = Cursors.Arrow;

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
            if (lvUPC.ItemsSource != null) CollectionViewSource.GetDefaultView(lvUPC.ItemsSource).Refresh();
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
            
            foreach (Item item in items.Skip(1))
            {
                List<string> upcsToRemove = new List<string>();
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
                if (item.quantity[3] != selection.quantity[3]) selection.quantity[3] = int.MinValue;
                if (item.quantity[4] != selection.quantity[4]) selection.quantity[4] = int.MinValue;
                // UPCs
                foreach (string upc in selection.UPCs){
                    if (!item.UPCs.Contains(upc)) upcsToRemove.Add(upc);
                }
                foreach (string upc in upcsToRemove)
                {
                    selection.UPCs.Remove(upc);
                }
            }           

            return selection;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {              
            // Save item information
            btnSave.Focus();
            MessageBoxResult result = MessageBox.Show("Save changes?", "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {                
                foreach (Item item in lvList.SelectedItems)
                {
                    Item newItem = item.Clone();
                    if (newItem.quantity.Count != 5) newItem.quantity = new ObservableCollection<int> { 0, 0, 0, 0, 0 };

                    if (txtName.IsEnabled == true && !String.IsNullOrWhiteSpace(txtName.Text))
                        newItem.name = managedItem.name;
                    if (!String.IsNullOrWhiteSpace(txtSystem.Text))
                        newItem.system = managedItem.system;
                    if (!String.IsNullOrWhiteSpace(txtPrice.Text))
                        newItem.price = managedItem.price;
                    if (!String.IsNullOrWhiteSpace(txtStore.Text))
                        newItem.quantity[0] = managedItem.quantity[0];
                    if (!String.IsNullOrWhiteSpace(txtOutBack.Text))
                        newItem.quantity[1] = managedItem.quantity[1];
                    if (!String.IsNullOrWhiteSpace(txtStorage.Text))
                        newItem.quantity[2] = managedItem.quantity[2];
                    if (!String.IsNullOrWhiteSpace(txtWebsite.Text))
                        newItem.quantity[3] = managedItem.quantity[3];
                    if (!String.IsNullOrWhiteSpace(txtOther.Text))
                        newItem.quantity[4] = managedItem.quantity[4];
                    if (!String.IsNullOrWhiteSpace(txtCash.Text))
                        newItem.tradeCash = managedItem.tradeCash;
                    if (!String.IsNullOrWhiteSpace(txtCredit.Text))
                        newItem.tradeCredit = managedItem.tradeCredit;

                    await DBAccess.SaveItemChanges(newItem);

                    // Add new UPCs
                    await DBAccess.AddUPCs(managedItem.UPCs, item.SQLid);

                    if (UPCsToDelete.Count > 0)
                    {
                        DBAccess.RemoveUPCs(UPCsToDelete, item.SQLid);                        
                    }                    
                }
                UPCsToDelete.Clear();
                MessageBox.Show("Saved.");
                lvList.UnselectAll();
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
        
        // Remove UPCs from selected items
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            foreach (string upc in lvUPC.SelectedItems)
            {
               UPCsToDelete.Add(upc);
               managedItem.UPCs.Remove(upc);
            }

            CollectionViewSource.GetDefaultView(lvUPC.ItemsSource).Refresh();
        }

        private void menuAddNewItem_Click(object sender, RoutedEventArgs e)
        {
            Window addNewItem = new AddNewItem();
            addNewItem.Show();
        }    
    
        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Permanently delete selected items?", "This could be a big deal.", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result != MessageBoxResult.Yes) return;

            foreach (Item item in lvList.SelectedItems)
            {
                await DBAccess.DeleteItem(item.SQLid);
            }

            Search();
        }

        private void TextBox_GotFocus(object sender, object e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void menuImportFromCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog CSVbrowse = new OpenFileDialog();
            CSVbrowse.Title = "Open .csv File";
            CSVbrowse.Filter = "Comma Separated Values, brah|*.csv";
            //CSVbrowse.InitialDirectory = @".";

            try
            {
                if (CSVbrowse.ShowDialog() == true)
                {
                    // Display 1st line of file to verify,
                    // then choose whether to import all columns, or only first (name) column



                    System.IO.StreamReader file = new System.IO.StreamReader(CSVbrowse.FileName.ToString());
                    string firstLine = file.ReadLine();
                    var result = MessageBox.Show("First Line of File: \n\t" + firstLine +
                                                    "\nExample Item:\n" + ImportCSV.CreateItemFromCSVLine(firstLine) +
                                                    "\n\nClick 'Yes' to import (Make sure everything looks good. This could be a pain to undo.)\n" +
                                                    "Click 'No' to do nothing, and go double-check!\n", "Oh God!", MessageBoxButton.YesNoCancel);

                    if (result == MessageBoxResult.Yes) // Click Yes, import only 1st column
                    {
                        ImportCSV.LoadCSV(CSVbrowse.FileName.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in menuImportFromCSV_Click():\n" + ex.Message);
                MessageBox.Show("As a reminder, the proper format for a .csv file is:\n"
                     + "Name,System,Price,Inventory Out Front, Inventory Out Back, Inventory in Storage, Inventory on Website, Trade value: Cash, Trade Value: Credit, UPC, [any additional UPCs (optional)]");
            }
        }

        private void btnAutoTradeValue_Click(object sender, RoutedEventArgs e)
        {
            
            managedItem.AutoTradeValues();
            
        }

        private void menuMoveInventory_Click(object sender, RoutedEventArgs e)
        {
            MoveInventory moveInventory = new MoveInventory();
            moveInventory.Show();
        }

        private void txtUPC_GotFocus(object sender, RoutedEventArgs e)
        {
            btnAdd.IsDefault = true;
            btnSave.IsDefault = false;
        }

        private void txtUPC_LostFocus(object sender, RoutedEventArgs e)
        {
            btnAdd.IsDefault = false;
            btnSave.IsDefault = true;
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            btnSave.IsDefault = false;
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            btnSave.IsDefault = true;
        }

        private void menuManageTransactions_Click(object sender, RoutedEventArgs e)
        {
            ManageTransactions mt = new ManageTransactions();
            mt.Show();
        }

        private void menuAddCustomerTable_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Soon.");
            //DBAccess.CreateCustomerTable();
        }

        private void menuAddWebsiteColumn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Add the SQL table column for Website storage?", "Add Website column?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (DBAccess.CreateWebsiteQuantityColumn())
                {
                    MessageBox.Show("Success.");
                }
            }
        }

        private void menuAddOtherColumn_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Add the SQL table column for \"Other\" storage?", "Add new inventory column?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                if (DBAccess.CreateOtherQuantityColumn())
                {
                    MessageBox.Show("Success.");
                }
            }
        }

        private void menuMoveStorageToWebsite_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Move all inventory from Storage to Website?", "Move to Website?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DBAccess.MoveInventoryFromStorageToWebsite();
                MessageBox.Show("Done.");
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            DBAccess.CloseSQLConnection();
            MessageBox.Show("SQL Connection Closed.");
        }

        private void menuZeroOutStorage_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Set Storage inventory to zero?", "Zero out storage?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DBAccess.ZeroOutStorageInventory();
                MessageBox.Show("Done.");
            }
        }

        private void menuChangeRewardsToStoreCredit_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Change Rewards payments to Store Credit?", "Change the books, boss?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DBAccess.ChangeRewardsToStoreCredit();
                MessageBox.Show("Done.");
            }
        }
    }
}
