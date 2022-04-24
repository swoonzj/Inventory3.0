using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Inventory_3._0
{
    public class Transaction : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<Item> Items;
        public ObservableCollection<Item> items
        {
            get { return Items; }
            set
            {
                Items = value;
                NotifyPropertyChanged("items");
            }
        }

        private ObservableCollection<Transaction> Payment;
        public ObservableCollection<Transaction> payment
        {
            get { return Payment; }
            set
            {
                Payment = value;
                NotifyPropertyChanged("payment");
            }
        }

        private DateTime Date;
        public DateTime date
        {
            get { return Date; }
            set
            {
                Date = value;
                NotifyPropertyChanged("date");
            }
        }

        private int TransactionNumber;
        public int transactionNumber
        {
            get { return TransactionNumber; }
            set
            {
                TransactionNumber = value;
                NotifyPropertyChanged("transactionNumber");
            }
        }

        private string TransactionType;
        public string transactionType
        {
            get { return TransactionType; }
            set
            {
                TransactionType = value;
                NotifyPropertyChanged("transactionType");
            }
        }

        public int quantity
        {
            get { return CalculateQuantity(); }
        }

        private decimal Total;
        public decimal total
        {
            get
            {
                if (TransactionType == TransactionTypes.RETURN_CASH || TransactionType == TransactionTypes.RETURN_CREDIT)
                {
                    return -CalculateTotal();
                }

                else if (TransactionType != TransactionTypes.PAYMENT_CASH
                    && TransactionType != TransactionTypes.PAYMENT_CREDITCARD
                    && TransactionType != TransactionTypes.PAYMENT_WEBSITE
                    && TransactionType != TransactionTypes.PAYMENT_STORECREDIT
                    && TransactionType != TransactionTypes.RETURN_CASH
                    && TransactionType != TransactionTypes.RETURN_CREDIT)
                {
                    return CalculateTotal();
                }
                else return Total;
            }
        }

        public Transaction(int transactionNumber, string transactionType, DateTime date, decimal total = 0m)
        {
            this.transactionNumber = transactionNumber;
            this.transactionType = transactionType;
            this.date = date;
            Total = total;
            items = new ObservableCollection<Item>();
        }

        private decimal CalculateTotal()
        {
            switch (transactionType)
            {
                case TransactionTypes.SALE:
                    {
                        decimal sum = 0;
                        foreach (Item item in Items)
                        {
                            sum += item.priceTotal;
                        }
                        return sum;
                    }
                case TransactionTypes.TRADE_CASH:
                    {
                        decimal sum = 0;
                        foreach (Item item in Items)
                        {
                            sum += item.cashTotal;
                        }
                        return sum;
                    }
                case TransactionTypes.TRADE_CREDIT:
                    {
                        decimal sum = 0;
                        foreach (Item item in Items)
                        {
                            sum += item.creditTotal;
                        }
                        return sum;
                    }
                case TransactionTypes.RETURN:
                    {
                        decimal sum = 0;
                        foreach (Item item in Items)
                        {
                            sum += item.priceTotal;
                        }
                        return -sum;
                    }
                case TransactionTypes.RETURN_CASH:
                case TransactionTypes.RETURN_CREDIT:
                    return Total;
                default:
                    return -1;
            }
        }

        private int CalculateQuantity()
        {
            int quant = 0;
            foreach (Item item in items)
            {
                quant += item.quantity[0];
            }

            return quant;
        }
    }
}
