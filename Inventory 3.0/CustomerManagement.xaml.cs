using Inventory_3._0.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for CustomerManagement.xaml
    /// </summary>
    public partial class CustomerManagement : SortableListViews
    {
        Customer managedItem = new Customer();
        ObservableCollection<Customer> searchResults = new ObservableCollection<Customer>();

        public CustomerManagement()
        {
            InitializeComponent();
        }

        private void menuAddNewCustomer_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menuDeleteCustomer_Click(object sender, RoutedEventArgs e)
        {

        }
    

        private void lvList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Customer> customers = new List<Customer>();
            foreach (Customer customer in lvList.SelectedItems)
            {
                customers.Add(customer);
            }

            if (customers.Count == 1)
            {
                managedItem = customers[0].Clone();
                //lvUPC.IsEnabled = true;
                //btnAdd.IsEnabled = true;
                //btnRemove.IsEnabled = true;
            }
            else
            {
                // TODO: Disable save/textboxes
            }
            DataContext = managedItem;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            btnSave.IsDefault = false;
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            btnSave.IsDefault = true;
        }

        private void TextBox_GotFocus(object sender, object e)
        {
            ((TextBox)sender).SelectAll();
        }


        private async void Search()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                searchResults = new ObservableCollection<Customer>(await DBAccess.SQLTableToCustomerList(searchtext: txtSearch.Text, limitResults: Settings.Default.limitSearchResults));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
            lvList.ItemsSource = searchResults;
            Mouse.OverrideCursor = Cursors.Arrow;

        }

        private void DetectEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search();
            }
        }
    }
}
