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
        private ObservableCollection<Item> checkout = new ObservableCollection<Item>();
        private decimal total = 0;
        public Checkout(decimal itemTotal)
        {
            InitializeComponent();
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

            lblTotal.Content = "$" + total.ToString("C");
            if (total == 0) FinalizeTransaction();
        }

        private void FinalizeTransaction()
        {
            MessageBox.Show("Finalize?");
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
            Item payment;
            decimal amount;
            if (txtCheckout.Text != string.Empty)
            {
                amount = GetPaymentAmount();
            }
            else
            {
                amount = total;;
            }

            payment = new Item(TransactionTypes.PAYMENT_CASH, TransactionTypes.PAYMENT, -amount, 0, 0, 0, "0");
            checkout.Add(payment);
            UpdateTotal();
        }

        private void btnCredit_Click(object sender, RoutedEventArgs e)
        {
            Item payment;
            decimal amount;
            if (txtCheckout.Text != string.Empty)
            {
                amount = GetPaymentAmount();
            }
            else
            {
                amount = total;
            }

            payment = new Item(TransactionTypes.PAYMENT_CREDITCARD, TransactionTypes.PAYMENT, -amount, 0, 0, 0, "0");
            checkout.Add(payment);
            UpdateTotal();
        }

        private void btnStoreCredit_Click(object sender, RoutedEventArgs e)
        {
            Item payment;
            decimal amount;
            if (txtCheckout.Text != string.Empty)
            {
                amount = GetPaymentAmount();
            }
            else
            {
                amount = total;
            }

            payment = new Item(TransactionTypes.PAYMENT_STORECREDIT, TransactionTypes.PAYMENT, -amount, 0, 0, 0, "0");
            checkout.Add(payment);
            UpdateTotal();
        }

        private void btnRewards_Click(object sender, RoutedEventArgs e)
        {
            Item payment;
            decimal amount;
            if (txtCheckout.Text != string.Empty)
            {
                amount = GetPaymentAmount();
            }
            else
            {
                amount = total;
            }

            payment = new Item(TransactionTypes.PAYMENT_REWARDS, TransactionTypes.PAYMENT, -amount, 0, 0, 0, "0");
            checkout.Add(payment);
            UpdateTotal();
        }
    }
}
