using Microsoft.Win32;
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
using System.Windows.Shapes;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for Management.xaml
    /// </summary>
    /// 
    public partial class ManageTransactions : SortableListViews
    {
        public List<string> UPCsToDelete;

        ObservableCollection<Transaction> Transactions = new ObservableCollection<Transaction>();
        ObservableCollection<Item> searchResults = new ObservableCollection<Item>();

        public ManageTransactions()
        {
            UPCsToDelete = new List<string>();
            //dgQuantities.ItemsSource = managedItems;
            InitializeComponent();

            //lvList.ItemsSource = searchResults;
            
            //Search();
        }

        private void Search()
        {
            try
            {
                searchResults = new ObservableCollection<Item>(DBAccess.SQLTableToList(searchtext: txtSearch.Text, limitResults: menuLimitSearchResults.IsChecked));                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
            lvList.ItemsSource = searchResults;
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
            
        }        

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {              
            
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Permanently delete selected items?", "This could be a big deal.", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result != MessageBoxResult.Yes) return;

            foreach (Item item in lvList.SelectedItems)
            {
                // DELETE  TRANSACTION
            }

            Search();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void menuMoveInventory_Click(object sender, RoutedEventArgs e)
        {
            MoveInventory moveInventory = new MoveInventory();
            moveInventory.Show();
        }
        
        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime? date = (sender as DatePicker).SelectedDate;
        }
    }
}