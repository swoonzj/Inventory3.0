using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Inventory_3._0
{
    public class Item : INotifyPropertyChanged
    {
        private int sqlid;
        public int SQLid
        { 
            get { return sqlid; }
            set 
            {
                sqlid = value;
                NotifyPropertyChanged("SQLid");
            } 
        }
        private string Name;
        public string name
        {
            get { return Name; }
            set
            {
                Name = value;
                NotifyPropertyChanged("name");
            }
        }
        private string System;
        public string system
        {
            get { return System; }
            set
            {
                System = value;
                NotifyPropertyChanged("system");
            }
        }
        private decimal Price;
        public decimal price
        {
            get { return Price; }
            set
            {
                Price = value;
                NotifyPropertyChanged("price");
                NotifyPropertyChanged("priceTotal");
            }
        }
        private List<int> Quantity;
        public List<int> quantity
        {
            get { return Quantity; }
            set
            {
                Quantity = value;
                NotifyPropertyChanged("quantity");
                NotifyPropertyChanged("priceTotal");
                NotifyPropertyChanged("cashTotal");
                NotifyPropertyChanged("tradeTotal");
            }
        }

        private decimal TradeCash;
        public decimal tradeCash
        {
            get { return TradeCash; }
            set
            {
                TradeCash = value;
                NotifyPropertyChanged("tradeCash");
                NotifyPropertyChanged("cashTotal");
            }
        }
        private decimal TradeCredit;
        public decimal tradeCredit
        {
            get { return TradeCredit; }
            set
            {
                TradeCredit = value;
                NotifyPropertyChanged("tradeCredit");
                NotifyPropertyChanged("tradeTotal");
            }
        }
        private List<string> upcs;
        public List<string> UPCs
        {
            get { return upcs; }
            set
            {
                upcs = value;
                NotifyPropertyChanged("UPCs");
            }
        }

        public decimal priceTotal
        {
            get { return quantity[0] * price; }
        }

        public decimal cashTotal
        {
            get { return quantity[0] * tradeCash; }
        }

        public decimal creditTotal
        {
            get { return quantity[0] * tradeCredit; }
        }

        public Item()        
        {
            UPCs = new List<string>();
            this.SQLid = 0;
            this.name = "";
            this.system = "";
            this.price = 0;
            this.quantity = new List<int>{0,0,0};
            this.tradeCash = 0;
            this.tradeCredit = 0;

        }

        public Item(string name, string system)
        {
            UPCs = new List<string>();
            this.SQLid = 0;
            this.name = name;
            this.system = system;
            this.price = 0;
            this.quantity = new List<int> { 0, 0, 0 };
            this.tradeCash = 0;
            this.tradeCredit = 0;

        }

        public Item(string name, string system, decimal price, int quantity, decimal cash, decimal credit, string upc)
        {
            UPCs = new List<string>();
            this.SQLid = 0;
            this.name = name;
            this.system = system;
            this.price = price;
            this.quantity = new List<int> { quantity, 0, 0 };
            this.tradeCash = cash;
            this.tradeCredit = credit;
            this.UPCs.Add(upc);
        }

        public Item(string name, string system, decimal price, int quantity, decimal cash, decimal credit, List<string> upcs)
        {
            UPCs = new List<string>();
            this.SQLid = 0;
            this.name = name;
            this.system = system;
            this.price = price;
            this.quantity = new List<int> { quantity, 0, 0 }; 
            this.tradeCash = cash;
            this.tradeCredit = credit;
            this.UPCs = upcs;
        }

        public Item(string name, string system = "", string price = "0", List<int> quantity = null, string cash = "0", string credit = "0", List<string> upcs = null, string SQLid = "0")
        {
            this.SQLid = Convert.ToInt32(SQLid);
            this.name = name;
            this.system = system;
            this.price = Convert.ToDecimal(price);
            this.quantity = quantity; 
            this.tradeCash = Convert.ToDecimal(cash);
            this.tradeCredit = Convert.ToDecimal(credit);
            this.upcs = upcs;
        }

        public Item(string name, string system = "", string price = "0", int quantity = 0, string cash = "0", string credit = "0", string upc = "", string SQLid = "0")
        {
            this.SQLid = Convert.ToInt32(SQLid);
            this.name = name;
            this.system = system;
            this.price = Convert.ToDecimal(price);
            this.quantity = new List<int> { quantity, 0, 0 };
            this.quantity.Add(quantity);
            this.tradeCash = Convert.ToDecimal(cash);
            this.tradeCredit = Convert.ToDecimal(credit);
            this.upcs = new List<string>();
            this.upcs.Add(upc);
        }

        public Item(string name, string system, decimal price, List<int> quantity, decimal cash, decimal credit, List<string> upcs, int SQLid = 0)
        {            
            this.SQLid = SQLid;
            this.name = name;
            this.system = system;
            this.price = price;
            this.quantity = quantity;
            this.tradeCash = cash;
            this.tradeCredit = credit;
            this.upcs = upcs;
        }

        public Item Clone()
        {
            return new Item(name, system, price, new List<int>(quantity), tradeCash, tradeCredit, new List<string>(UPCs), SQLid);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null) 
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return String.Format("Name: {0}\nSystem:{1}\nPrice:{2}\nInventory:\n\tSales Floor:{3}\n\tOut Back:{4}\n\tStorage:{5}\nTrade, Cash:{6}\nTrade, Store Credit:{7}\nUPCS:{8}", name, system, price, quantity[0], quantity[1], quantity[2], tradeCash, tradeCredit, String.Join(", ", UPCs));
        }

        public void AutoTradeValues()
        {

            if (this.price < 5 && this.price > 3)
            {
                this.tradeCash = .5m;
                this.tradeCredit = 1m;
            }
            if (this.price > 5)
            {
                this.tradeCash = Math.Truncate(this.price / 4);
                this.tradeCredit = Decimal.Round(this.price / 3);
            }
        }
    }
    
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

        public decimal total
        {
            get { return CalculateTotal(); }
        }

        public Transaction(int transactionNumber, string transactionType, DateTime date)
        {
            this.transactionNumber = transactionNumber;
            this.transactionType = transactionType;
            this.date = date;
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
                            sum += item.price;
                        }
                        return sum;
                    }
                case TransactionTypes.TRADE_CASH:
                    {
                        decimal sum = 0;
                        foreach (Item item in Items)
                        {
                            sum += item.tradeCash;
                        }
                        return sum;
                    }
                case TransactionTypes.TRADE_CREDIT:
                    {
                        decimal sum = 0;
                        foreach (Item item in Items)
                        {
                            sum += item.tradeCredit;
                        }
                        return sum;
                    }
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
