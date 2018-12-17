﻿using System;
using System.Collections.Generic;

namespace Inventory_3._0
{
    public class Item
    {
        public int SQLid { get; set; }
        public string name { get; set; }
        public string system { get; set; }
        public decimal price { get; set; }
        public List<int> quantity { get; set; }
        public decimal tradeCash { get; set; }
        public decimal tradeCredit { get; set; }
        public List<string> UPCs { get; set; }

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
            //this.quantity.Add(quantity); // Handle multiple quantities!!!!!!!
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
            //this.quantity = quantity; !!!!!!!!
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

//        public void AddToDatabase(string tablename)
//        {
//            DBaccess.AddToTable(tablename, name, system, price, quantity, tradeCash, tradeCredit, UPC);
//        }

//        public void RemoveFromDatabase(string tablename)
//        {
//            DBaccess.DeleteTableItem(tablename, this);
//        }

        public Item Clone()
        {
            return new Item(name, system, price, quantity, tradeCash, tradeCredit, UPCs, SQLid);
        }

//        // ==================================|
//        // Methods dealing with transactions |
//        // ==================================|

//        public void Sell(int transactionNumber, string date)
//        {
//            DBaccess.DecrementInventory(TableNames.INVENTORY, this);
//            DBaccess.AddToTransactionTable(TableNames.TRANSACTION, this, TransactionTypes.SALE, transactionNumber, date);
//        }

//        // ================================================================|
//        // Methods to set fields with either strings or their proper types |
//        // ================================================================|

//        public void SetName(string name)
//        {
//            this.name = name;
//        }

//        public void SetSystem(string system)
//        {
//            this.system = system;
//        }

//        public void SetPrice(string price)
//        {
//            this.price = Convert.ToDecimal(price);
//        }

//        public void SetPrice(decimal price)
//        {
//            this.price = price;
//        }

//        public void SetQuantity(string quantity)
//        {
//            this.quantity = Convert.ToInt32(quantity);
//        }

//        public void SetQuantity(int quantity)
//        {
//            this.quantity = quantity;
//        }

//        public void SetTradeCash(string cash)
//        {
//            this.tradeCash = Convert.ToDecimal(cash);
//        }

//        public void SetTradeCash(decimal cash)
//        {
//            this.tradeCash = cash;
//        }

//        public void SetTradeCredit(string credit)
//        {
//            this.tradeCredit = Convert.ToDecimal(credit);
//        }

//        public void SetTradeCredit(decimal credit)
//        {
//            this.tradeCredit = credit; ;
//        }
//    }
    
//    public class TransactionItem : Item
//    {
//        public int transactionNumber { get; set; }
//        public DateTime date { get; set; }
//        public string transactionType { get; set; }

//        public TransactionItem(string name, string system, decimal price, int quantity, string upc, string transactionType, int transactionNumber, DateTime date)
//            : base(name, system, price, quantity, 0, 0, upc)
//        {
//            this.transactionType = transactionType;
//            this.date = date;
//            this.transactionNumber = transactionNumber;
//        }

//        public TransactionItem(string name, string system, string price, string quantity, string upc, string transactionType, string transactionNumber, string date)
//            : base(name, system, price, quantity, "0", "0", upc)
//        {
//            this.transactionType = transactionType;
//            this.date = Convert.ToDateTime(date);
//            this.transactionNumber = Convert.ToInt32(transactionNumber);
//        }

//        new public TransactionItem Clone()
//        {
//            return new TransactionItem(name, system, price, quantity, UPC, transactionType, transactionNumber, date);
//        }
    }
}
