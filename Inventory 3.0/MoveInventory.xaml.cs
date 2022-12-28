using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for MoveInventory.xaml
    /// </summary>
    public partial class MoveInventory : SortableListViews
    {
        ObservableCollection<Item> searchResults = new ObservableCollection<Item>();
        ObservableCollection<Item> movingItems = new ObservableCollection<Item>();

        public MoveInventory()
        {
            InitializeComponent();
            lvMove.ItemsSource = movingItems;
            this.DataContext = movingItems;
            
            //Search();

            lvList.ItemsSource = searchResults;
            lvList.ContextMenu = new ListViewContextMenu(lvList);
            lvMove.ContextMenu = new ListViewContextMenu(lvMove);
            lvList.PreviewMouseRightButtonDown += LvList_PreviewMouseRightButtonDown;
            lvMove.PreviewMouseRightButtonDown += LvList_PreviewMouseRightButtonDown;
            getTotalItemsAndSetLabel();

            //searchResults.Add(new Item("Test1", "Test System", 9.99m, 5, 2m, 3m, "12345"));
            //searchResults.Add(new Item("Test2", "Test System", 9.99m, 5, 2m, 3m, "12345"));
        }

        private void LvList_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void getTotalItemsAndSetLabel()
        {
            int count = 0;
            foreach (Item item in movingItems)
            {
                count += item.cartQuantity;
            }

            lblTotalItems.Content = count.ToString();
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

        private async void Search()
        {
            try
            {
                searchResults = new ObservableCollection<Item>(await DBAccess.GetItemsAsList(searchtext: txtSearch.Text));
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
            int index = movingItems.IndexOf(item);
            if (index >= 0)
            {
                movingItems[index].cartQuantity++;
            }
            else
            {
                item.cartQuantity = 1;
                movingItems.Insert(0, item);
            }
            getTotalItemsAndSetLabel();
        }

        private string CreateVerificationString()
        {
            string destination, origin;
            if (radioFromOutBack.IsChecked == true)
                origin = "Back Room";
            else if (radioFromSalesFloor.IsChecked == true)
                origin = "Sales Floor";
            else if (radioFromStorage.IsChecked == true)
                origin = "Storage";
            else if (radioFromWebsite.IsChecked == true)
                origin = "Website";
            else if (radioFromOther.IsChecked == true)
                origin = "Other";
            else if (radioNewItem.IsChecked == true)
                origin = "New Item / Trade";
            else
                origin = "ORIGIN NOT SELECTED!!!!";

            if (radioToOutBack.IsChecked == true)
                destination = "Back Room";
            else if (radioToSalesFloor.IsChecked == true)
                destination = "Sales Floor";
            else if (radioToStorage.IsChecked == true)
                destination = "Storage";
            else if (radioToWebsite.IsChecked == true)
                destination = "Website";
            else if (radioToOther.IsChecked == true)
                destination = "Other";
            else if (radioRemove.IsChecked == true)
                destination = "Remove entirely.";
            else
                destination = "DESTINATION NOT SELECTED !!!!";

            return string.Format("Move item(s)?\nFrom: {0}\nTo: {1}", origin, destination);
        }

        private async void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            // Verify
            MessageBoxResult verify = MessageBox.Show(CreateVerificationString(), "Are You Sure?", MessageBoxButton.YesNo);
            if (verify != MessageBoxResult.Yes)
            {
                return;
            }

            // User verified, continue.
            foreach (Item item in movingItems)
            {
                // Remove FROM inventory
                if (radioFromOutBack.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, -item.cartQuantity, InventoryLocationColumnNames.OUTBACK);
                else if (radioFromSalesFloor.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, -item.cartQuantity, InventoryLocationColumnNames.STORE);
                else if (radioFromStorage.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, -item.cartQuantity, InventoryLocationColumnNames.STORAGE);
                else if (radioFromWebsite.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, -item.cartQuantity, InventoryLocationColumnNames.WEBSITE);
                else if (radioFromOther.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, -item.cartQuantity, InventoryLocationColumnNames.OTHER);
                else if (radioNewItem.IsChecked == true) { } // If New Item is checked, don't do anything.
                else
                {
                    MessageBox.Show("Please choose a source under the \"FROM:\".", "You didn't choose an option.", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                // Add TO inventory
                if (radioToOutBack.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, item.cartQuantity, InventoryLocationColumnNames.OUTBACK);
                else if (radioToSalesFloor.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, item.cartQuantity, InventoryLocationColumnNames.STORE);
                else if (radioToStorage.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, item.cartQuantity, InventoryLocationColumnNames.STORAGE);
                else if (radioToWebsite.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, item.cartQuantity, InventoryLocationColumnNames.WEBSITE);
                else if (radioToOther.IsChecked == true)
                    await DBAccess.IncrementQuantities(item.SQLid, item.cartQuantity, InventoryLocationColumnNames.OTHER);
                else if (radioRemove.IsChecked == true) { } // If Remove is checked, don't do anything
                else
                {
                    MessageBox.Show("Please choose a destination under the \"TO:\".", "You didn't choose an option.", MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
            }
            if (radioRemove.IsChecked == true)
            {
                Random rand = new Random();
                switch (rand.Next(1, 4))
                {
                    case 1:
                        MessageBox.Show("Get up on outta here!", "Eyehole Man");
                        break;
                    case 2:
                        MessageBox.Show("YEET!", "Bye bye");
                        break;
                    case 3:
                        MessageBox.Show("Bah-leeted!", "Homestar");
                        break;
                }
            }
            else
            {
                MessageBox.Show("Success!", "Success.");
            }
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<Item> itemsToRemove = new ObservableCollection<Item>();
            foreach (Item item in lvMove.SelectedItems)
            {
                itemsToRemove.Add(item);
            }
            foreach (Item item in itemsToRemove)
            {
                movingItems.Remove(item);
            }
            getTotalItemsAndSetLabel();
        }

        private void btnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            movingItems.Clear();
            getTotalItemsAndSetLabel();
        }

        private void btnChangeQuantity_Click(object sender, RoutedEventArgs e)
        {
            foreach (Item item in lvMove.SelectedItems)
            {
                try
                {
                    item.cartQuantity = Convert.ToInt32(txtChangeQuantity.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            } 
            getTotalItemsAndSetLabel();
        }
        
        #region UPC Scanner Methods

        private void lvMove_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Give focus to txtUPCInput, so UPC scanning will work.
            txtUPCInput.Focus();
        }

        private void lvMove_IsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            txtUPCInput.Focus();
        }

        private async void DetectUPCEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                List<Item> items = await DBAccess.UPCLookup(txtUPCInput.Text); // Returns NULL if UPC does not match an item                

                if (items.Count != 0)
                {
                    if (items.Count > 1)
                    {
                        MultipleUPCHandler handler = new MultipleUPCHandler(items);
                        if (handler.ShowDialog() == true)
                        {
                            AddItem(handler.selectedItem);
                            handler.Close();
                        }
                    }
                    else
                    {
                        AddItem(items[0]);
                    }
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

        #endregion
    }
}
