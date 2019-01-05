using System;
using System.Collections.Generic;
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
            this.quantity = new List<int> { 0, 0, 0 };
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
            this.quantity = new List<int> { 0, 0, 0 }; 
            this.tradeCash = cash;
            this.tradeCredit = credit;
            this.UPCs = upcs;
        }

        public Item(string name, string system = "", string price = "0", List<int> quantity = null, string cash = "0", string credit = "0", string SQLid = "0")
        {
            UPCs = new List<string>();
            this.SQLid = Convert.ToInt32(SQLid);
            this.name = name;
            this.system = system;
            this.price = Convert.ToDecimal(price);
            this.quantity = quantity; 
            this.tradeCash = Convert.ToDecimal(cash);
            this.tradeCredit = Convert.ToDecimal(credit);
        }

        public Item(string name, string system, decimal price, List<int> quantity, decimal cash, decimal credit, List<string> upcs, int SQLid = 0)
        {
            UPCs = new List<string>();
            this.SQLid = SQLid;
            this.name = name;
            this.system = system;
            this.price = price;
            this.quantity = quantity;
            this.tradeCash = cash;
            this.tradeCredit = credit;
        }

        public Item Clone()
        {
            return new Item(name, system, price, quantity, tradeCash, tradeCredit, UPCs, SQLid);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null) 
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
