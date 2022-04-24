using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for Management.xaml
    /// </summary>
    /// 
    public partial class ManageTransactions : SortableListViews
    {
        decimal tradeCash, tradeCredit, totalSales, netSales, cashPayment, creditPayment, netIncome, creditRedeemed, websitePayment, totalCashReturn, totalCreditReturn;
        ObservableCollection<Transaction> transactions = new ObservableCollection<Transaction>();
        DateTime startDate = DateTime.Today, endDate = DateTime.Today.AddDays(1);
        List<Transaction> payments = new List<Transaction>(); 

        public ManageTransactions()
        {
            //ObservableCollection<Item> list = new ObservableCollection<Item>();
            //list.Add(new Item("Test1", "System", 10m, 2, 2, 3, 0.ToString()));
            //list.Add(new Item("Test2", "System", 100m, 1, 20, 30, 0.ToString()));
            //Transaction testTransaction = new Transaction(55, "Sale", DateTime.Today);
            //testTransaction.items = list;
            //transactions.Add(testTransaction);

            InitializeComponent();
            lvList.ItemsSource = transactions;
        }

        private void Search()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            transactions = new ObservableCollection<Transaction>(DBAccess.GetTransactions(startDate, endDate));
            UpdateAfterSearch();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void UpdateAfterSearch()
        {
            lvList.ItemsSource = transactions;
            CalculateTotals();
            SetLabels();
        }

        private void CalculateTotals()
        {
            tradeCash = tradeCredit = totalSales = netSales = cashPayment = creditPayment = netIncome = websitePayment = creditRedeemed = totalCashReturn = totalCreditReturn = 0m;
            foreach (Transaction item in transactions)
            {
                switch (item.transactionType)
                {
                    case TransactionTypes.TRADE_CASH:
                        tradeCash += item.total;
                        break;
                    case TransactionTypes.TRADE_CREDIT:
                        tradeCredit += item.total;
                        break;
                    case TransactionTypes.SALE:
                        totalSales += item.total;
                        payments = DBAccess.GetPayments(item.transactionNumber);
                        foreach (Transaction payment in payments)
                        {
                            switch (payment.transactionType)
                            {
                                case TransactionTypes.PAYMENT_CASH:
                                    cashPayment += payment.total;
                                    break;
                                case TransactionTypes.PAYMENT_CREDITCARD:
                                    creditPayment += payment.total;
                                    break;
                                case TransactionTypes.PAYMENT_WEBSITE:
                                    websitePayment += payment.total;
                                    break;
                                case TransactionTypes.PAYMENT_STORECREDIT:
                                    creditRedeemed += payment.total;
                                    break;
                                // Change due is no longer included as a transation, but it WAS. Added here for legacy transactions.
                                case TransactionTypes.CHANGE_DUE:
                                    cashPayment -= payment.total;
                                    break;
                            }
                        }
                        break;
                    case TransactionTypes.RETURN:
                        payments = DBAccess.GetPayments(item.transactionNumber);
                        foreach (Transaction payment in payments)
                        {
                            switch (payment.transactionType)
                            {
                                case TransactionTypes.RETURN_CASH:
                                    totalCashReturn -= payment.total;
                                    break;
                                case TransactionTypes.RETURN_CREDIT:
                                    totalCreditReturn -= payment.total;
                                    break;
                            }
                        }
                        break;              
                }
            }
            netSales = (cashPayment + creditPayment);
            netIncome = netSales - tradeCash - totalCashReturn;
        }

        private void menuRefresh_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        private void SetLabels()
        {
            lblSalesTotal.Content = totalSales.ToString("C");
            lblTradeCash.Content = tradeCash.ToString("C");
            lblTradeCredit.Content = tradeCredit.ToString("C");
            lblNetSales.Content = netSales.ToString("C");
            lblNetIncome.Content = netIncome.ToString("C");
            lblRedeemedCredit.Content = creditRedeemed.ToString("C");
            lblWebsite.Content = websitePayment.ToString("C");
            lblCashSales.Content = cashPayment.ToString("C");
            lblCreditSales.Content = creditPayment.ToString("C");
            lblReturnCash.Content = totalCashReturn.ToString("C");
            lblReturnCredit.Content = totalCreditReturn.ToString("C");
        }

        private void PrintItem_Click(object sender, RoutedEventArgs e)
        {
            Transaction transaction = lvList.SelectedItem as Transaction;
            transaction.payment = new ObservableCollection<Transaction>(payments);
            MessageBox.Show(transaction.items[0].name + " " + transaction.date.ToShortDateString());

            MessageBoxResult result = MessageBox.Show("Print this transaction?", "Print transaction?", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            

            ReceiptGenerator receiptGen = new ReceiptGenerator(transaction);
            ReceiptPrinter printer = new ReceiptPrinter(receiptGen.flowDoc);
            printer.PrintSilently();
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
                    UpdateAfterSearch();
                }
                else
                {
                    transactions = new ObservableCollection<Transaction>(DBAccess.GetTransactions(startDate, endDate, 0, txtSearch.Text));
                    UpdateAfterSearch();
                }
            }
        }

        private void lvList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView selection = sender as ListView;
            if (selection.SelectedItem == null)
            {
                lvDetail.ItemsSource = null;
                return;
            }
            lvDetail.ItemsSource = (selection.SelectedItem as Transaction).items;
            payments = DBAccess.GetPayments((selection.SelectedItem as Transaction).transactionNumber);
            lvPayment.ItemsSource = payments;
        }        

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {              
            
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Permanently delete selected items? This can't be undone!", "This could be a big deal.", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result != MessageBoxResult.Yes) return;
            
            List<Transaction> transToRemove = new List<Transaction>();
            foreach (Transaction transaction in lvList.SelectedItems)
            {
                
                try
                {
                    // DELETE TRANSACTION
                    DBAccess.DeleteTransaction(transaction.transactionNumber);
                    // DELETE PAYMENT
                    DBAccess.DeleteAllTransactionPayments(transaction.transactionNumber);
                    transToRemove.Add(transaction);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in btnDelete_Click(): " + ex.Message);
                }
            }
            // Remove deleted transactions from listview
            //foreach (Transaction trans in transToRemove) lvList.Items.Remove(trans);
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