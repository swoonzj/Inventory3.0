﻿using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for Checkout.xaml
    /// </summary>
    public partial class Checkout : Window
    {
        public bool isReturn { get; private set; } = false;
        public bool isWebsite { get; private set; } = false;
        public bool success = false;
        public ObservableCollection<Item> checkout = new ObservableCollection<Item>();
        private decimal total = 0;
        public Checkout(decimal itemTotal)
        {
            InitializeComponent();
            txtCheckout.Focus();
            //txtCheckout.LostKeyboardFocus += (s,e) => txtCheckout.Focus();
            lbCheckout.ItemsSource = checkout;

            checkout.Add(new Item("Item Total", "Checkout", itemTotal,1,0,0,"0"));
            if (itemTotal > 0) UpdateTotal();
        }        
        
        private void UpdateTotal()
        {
            total = 0;
            foreach (Item payment in checkout)
            {
                total += payment.price;
            }

            lblTotal.Content = total.ToString("C");
            if (total <= 0) FinalizeTransaction();
        }

        private void FinalizeTransaction()
        {
            //checkout[0] = Product total
            //checkout[1...count-1] = payment
            // if change is due:
            //checkout[count-1] = change due

            if (total < 0)
            {
                decimal change = 0 - total;
                checkout.Add(new Item(TransactionTypes.CHANGE_DUE, "Checkout", change, 1, 0, 0, "0"));
                MessageBox.Show(TransactionTypes.CHANGE_DUE + change.ToString("C"));
                // Adjust final form of payment to match change due
                checkout[checkout.Count - 2].price += checkout[checkout.Count - 1].price;
            }
            success = true;

            //log payment
            int transactionNumber = DBAccess.GetNextUnusedTransactionNumber();

            for (int i = 1; i < checkout.Count; i++)
            {
                if (checkout[i].name != TransactionTypes.CHANGE_DUE)
                    DBAccess.AddPayment(checkout[i], transactionNumber);
            }
            isWebsite = checkout[1].name == TransactionTypes.PAYMENT_WEBSITE;
            DialogResult = success;
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

            // If Website Payment, make sure there are no other payments.
            if (paymentType == TransactionTypes.PAYMENT_WEBSITE && amount != total)
            {
                MessageBox.Show("Website Payment must be the entire value of cart.", "Website payment can't be combined.", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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

        private void btnWebsite_Click(object sender, RoutedEventArgs e)
        {
            Pay(TransactionTypes.PAYMENT_WEBSITE);
        }

        private void btnReturnCredit_Click(object sender, RoutedEventArgs e)
        {
            isReturn = true;
            Pay(TransactionTypes.RETURN_CREDIT);
        }

        private void btnReturnCash_Click(object sender, RoutedEventArgs e)
        {
            isReturn = true;
            Pay(TransactionTypes.RETURN_CASH);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = success;
        }

        private void menuDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lbCheckout.SelectedIndex == -1) 
                return;

            Item deletedItem = (Item)lbCheckout.Items.GetItemAt(lbCheckout.SelectedIndex);
            // Can't delete Item total
            if (deletedItem.name.Contains("Item Total")) 
                return;
            
            checkout.Remove(deletedItem);
            UpdateTotal();
        }
    }
}
