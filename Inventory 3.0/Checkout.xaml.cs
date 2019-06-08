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
    /// Interaction logic for Checkout.xaml
    /// </summary>
    public partial class Checkout : Window
    {
        public bool success = false;
        private ObservableCollection<Item> checkout = new ObservableCollection<Item>();
        private decimal total = 0;
        public Checkout(decimal itemTotal)
        {
            InitializeComponent();
            txtCheckout.Focus();
            txtCheckout.LostKeyboardFocus += (s,e) => txtCheckout.Focus();
            lbCheckout.ItemsSource = checkout;

            checkout.Add(new Item("Item Total", "Checkout", itemTotal,1,0,0,"0"));
            UpdateTotal();
        }

        
        
        private void UpdateTotal()
        {
            total = 0;
            foreach (Item item in checkout)
            {
                total += item.price;
            }

            lblTotal.Content = total.ToString("C");
            if (total <= 0) FinalizeTransaction();
        }

        private void FinalizeTransaction()
        {
            if (total < 0) MessageBox.Show("Change Due: " + (0-total).ToString("C"));
            MessageBox.Show("Finalize?");
            success = true;

            //log payment
            int transactionNumber = DBAccess.GetNextUnusedTransactionNumber();
            string date = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
            for (int i = 1; i < checkout.Count; i++)
            {
                DBAccess.AddTransaction(checkout[i], checkout[i].name, transactionNumber, date);
            }
            this.Close();
        }

        private void Pay(string paymentType)
        {
            Item payment;
            decimal amount;
            if (txtCheckout.Text != string.Empty)
            {
                amount = GetPaymentAmount();
            }
            else
            {
                amount = total; ;
            }

            payment = new Item(paymentType, TransactionTypes.PAYMENT, -amount, 0, 0, 0, "0");
            checkout.Add(payment);
            UpdateTotal();
            txtCheckout.Clear();
        }

        private decimal GetPaymentAmount()
        {
            decimal amount = 0;
            try
            {
                amount = Convert.ToDecimal(txtCheckout.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in GetPaymentAmount:\n" + ex.Message);
            }
            return amount;
        }

        private void btnCash_Click(object sender, RoutedEventArgs e)
        {
            Pay(TransactionTypes.PAYMENT_CASH);
        }

        private void btnCredit_Click(object sender, RoutedEventArgs e)
        {
            Pay(TransactionTypes.PAYMENT_CREDITCARD);
        }

        private void btnStoreCredit_Click(object sender, RoutedEventArgs e)
        {
            Pay(TransactionTypes.PAYMENT_STORECREDIT);
        }

        private void btnRewards_Click(object sender, RoutedEventArgs e)
        {
            Pay(TransactionTypes.PAYMENT_REWARDS);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = success;
        }
    }
}
