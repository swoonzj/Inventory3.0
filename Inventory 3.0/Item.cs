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
    }

    public class TransactionItem : Item
    {
        
    }
}
