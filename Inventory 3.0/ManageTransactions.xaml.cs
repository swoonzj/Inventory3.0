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

        ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
        DateTime startDate = DateTime.Today, endDate = DateTime.Today.AddDays(1);

        public ManageTransactions()
        {
            //ObservableCollection<Item> list = new ObservableCollection<Item>();
            //list.Add(new Item("Test1", "System", 10m, 0, 0, 0, 0.ToString()));
            //list.Add(new Item("Test2", "System", 10m, 0, 0, 0, 0.ToString()));
            //Transaction testTransaction = new Transaction(55, "Sale", DateTime.Today);
            //testTransaction.items = list;
            //transactions.Add(testTransaction);

            InitializeComponent();
            lvList.ItemsSource = transactions;
        }

        private void Search()
        {
            transactions = new ObservableCollection<Transaction>(DBAccess.GetTransactions(startDate, endDate));
            lvList.ItemsSource = transactions;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void DetectEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                int n;
                if (int.TryParse(txtSearch.Text, out n))
                {
                    transactions = new ObservableCollection<Transaction>(DBAccess.GetTransactions(startDate, endDate, n));
                }
                else
                {
                    MessageBox.Show("\"" + txtSearch.Text + "\" is not a valid number. Try again.");
                }
            }
        }

        private void lvList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView selection = sender as ListView;
            if (selection.SelectedItem == null) return;
            //foreach (Item item in (selection.SelectedItem as Transaction).items)
            //{
            //    contents += item.ToString() + "\n";
            //}
            //MessageBox.Show(contents);
            lvDetails.ItemsSource = (selection.SelectedItem as Transaction).items;
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
            Calendar cal = (sender as Calendar);
            startDate = (DateTime)cal.SelectedDates[0];
            endDate = (DateTime)cal.SelectedDates[cal.SelectedDates.Count - 1];

            if (startDate > endDate)
            {
                DateTime temp = startDate;
                startDate = endDate;
                endDate = temp;
            }
            endDate = endDate.AddDays(1); // Add 1 day to end date, to include the selected end date in search

            Search();
            // MessageBox.Show(startDate.ToString() + "\n" + endDate.ToString()); // For debugging date selection
            calPopup.IsOpen = false;
        }

        private void btnSelectDate_Click(object sender, RoutedEventArgs e)
        {
            calPopup.IsOpen = true;
        }
    }
}