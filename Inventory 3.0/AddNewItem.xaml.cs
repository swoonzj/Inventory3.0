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
    /// Interaction logic for AddNewItem.xaml
    /// </summary>
    public partial class AddNewItem : Window
    {
        Item item = new Item();
        ObservableCollection<Item> items = new ObservableCollection<Item>();

        public AddNewItem()
        {
            items.Add(item);

            InitializeComponent();
            dgQuantities.ItemsSource = items;
            this.DataContext = item;
        }

        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save item information
            MessageBoxResult result = MessageBox.Show("Save changes?", "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {

                if (await DBAccess.AddNewItem(item))
                {
                    MessageBox.Show("Saved.");
                    this.Close();
                }
                else
                {
                    MessageBox.Show("An error occurred. Data was NOT saved.");
                }
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {            
            // Make sure there is text in the field. If not, do nothing.
            // Make sure there are no doubles
            if (txtUPC.Text == "") return;
            if (item.UPCs.Contains(txtUPC.Text)) return;

            item.UPCs.Add(txtUPC.Text);
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
                item.UPCs.Remove(upc);
            }

            CollectionViewSource.GetDefaultView(lvUPC.ItemsSource).Refresh();
        }

        private void btnAutoTradeValue_Click(object sender, RoutedEventArgs e)
        {
            if (item.price < 5 && item.price > 3)
            {
                item.tradeCash = .5m;
                item.tradeCredit = 1m;
            }
            if (item.price > 5)
            {
                item.tradeCash = Math.Round(item.price / 4);
                item.tradeCredit = Math.Round(item.price / 3);
            }
        }

        private void textbox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void textbox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void txtUPC_GotFocus(object sender, RoutedEventArgs e)
        {
            btnAdd.IsDefault = true;
        }

        private void txtUPC_LostFocus(object sender, RoutedEventArgs e)
        {
            btnAdd.IsDefault = false;
        }
    }
}
