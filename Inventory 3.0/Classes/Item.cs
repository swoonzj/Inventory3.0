using System;

namespace Inventory
{
    public class Item
    {
        public string name { get; set; }
        public string system { get; set; }
        public decimal price { get; set; }
        public int quantity { get; set; }
        public decimal tradeCash { get; set; }
        public decimal tradeCredit { get; set; }
        public string UPC { get; set; }

        public Item(string name, string system, decimal price, int quantity, decimal cash, decimal credit, string upc)
        {
            this.name = name;
            this.system = system;
            this.price = price;
            this.quantity = quantity;
            this.tradeCash = cash;
            this.tradeCredit = credit;
            this.UPC = upc;
        }

        public Item(string name, string system = "", string price = "0", string quantity = "0", string cash = "0", string credit = "0", string upc = "0")
        {
            this.name = name;
            this.system = system;
            this.price = Convert.ToDecimal(price);
            this.quantity = Convert.ToInt32(quantity);
            this.tradeCash = Convert.ToDecimal(cash);
            this.tradeCredit = Convert.ToDecimal(credit);
            this.UPC = upc;
        }

        public void AddToDatabase(string tablename)
        {
            DBaccess.AddToTable(tablename, name, system, price, quantity, tradeCash, tradeCredit, UPC);
        }

        public void RemoveFromDatabase(string tablename)
        {
            DBaccess.DeleteTableItem(tablename, this);
        }

        public Item Clone()
        {
            return new Item(name, system, price, quantity, tradeCash, tradeCredit, UPC);
        }

        // ==================================|
        // Methods dealing with transactions |
        // ==================================|

        public void Sell(int transactionNumber, string date)
        {
            DBaccess.DecrementInventory(TableNames.INVENTORY, this);
            DBaccess.AddToTransactionTable(TableNames.TRANSACTION, this, TransactionTypes.SALE, transactionNumber, date);
        }

        // ================================================================|
        // Methods to set fields with either strings or their proper types |
        // ================================================================|

        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetSystem(string system)
        {
            this.system = system;
        }

        public void SetPrice(string price)
        {
            this.price = Convert.ToDecimal(price);
        }

        public void SetPrice(decimal price)
        {
            this.price = price;
        }

        public void SetQuantity(string quantity)
        {
            this.quantity = Convert.ToInt32(quantity);
        }

        public void SetQuantity(int quantity)
        {
            this.quantity = quantity;
        }

        public void SetTradeCash(string cash)
        {
            this.tradeCash = Convert.ToDecimal(cash);
        }

        public void SetTradeCash(decimal cash)
        {
            this.tradeCash = cash;
        }

        public void SetTradeCredit(string credit)
        {
            this.tradeCredit = Convert.ToDecimal(credit);
        }

        public void SetTradeCredit(decimal credit)
        {
            this.tradeCredit = credit; ;
        }
    }
    
    public class TransactionItem : Item
    {
        public int transactionNumber { get; set; }
        public DateTime date { get; set; }
        public string transactionType { get; set; }

        public TransactionItem(string name, string system, decimal price, int quantity, string upc, string transactionType, int transactionNumber, DateTime date)
            : base(name, system, price, quantity, 0, 0, upc)
        {
            this.transactionType = transactionType;
            this.date = date;
            this.transactionNumber = transactionNumber;
        }

        public TransactionItem(string name, string system, string price, string quantity, string upc, string transactionType, string transactionNumber, string date)
            : base(name, system, price, quantity, "0", "0", upc)
        {
            this.transactionType = transactionType;
            this.date = Convert.ToDateTime(date);
            this.transactionNumber = Convert.ToInt32(transactionNumber);
        }

        new public TransactionItem Clone()
        {
            return new TransactionItem(name, system, price, quantity, UPC, transactionType, transactionNumber, date);
        }
    }
}
