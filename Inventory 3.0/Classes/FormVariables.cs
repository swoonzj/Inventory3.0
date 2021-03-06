﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Inventory
{
    public static class Constants
    {
        public const string DISCOUNTUPC = "-1";
    }

    public static class PrinterVariables
    {
        public class UPCimage
        {
            public const int locX = 0;
            public const int locY = 0;
        }
        public class ItemInfo
        {
            public const int locX = 0;
            public const int nameLocY = 60;
            public const int systemLocY = 85;
        }
    }

    public static class TableNames
    {
        public const string INVENTORY = "tblInventory";
        public const string TRANSACTION = "tblTransactions";
        public const string VARIABLES = "tblVariables";
        public const string AUTOPRINT = "tblAutoPrint";
    }

    public static class TransactionTypes
    {
        public const string SALE = "Sale";
        public const string TRADE_CASH = "Trade-Cash";
        public const string TRADE_CREDIT = "Trade-Credit";
    }

    public static class POSTableIndex
    {
        public const int NAME = 0,
            SYSTEM = 1,
            PRICE = 2,
            INSTOCK = 3,
            CASHVALUE = 4,
            TRADEVALUE = 5,
            UPC = 6;
    }

    public static class ListViewType
    {
        public const int POS = 0,
            CART = 1,
            TRADECART = 2,
            MANAGEMENT = 3,
            TRANSACTION = 5;
    }

    public static class ListViewColumnNames
    {
        public const string NAME = "Name",
            SYSTEM = "System",
            PRICE = "Price",
            QUANTITY = "# In Stock",
            TRADE_CASH = "Trade: Cash",
            TRADE_CREDIT = "Trade: Store Credit",
            UPC = "UPC",
            ITEM_TOTAL = "Item Total",
            DATE = "Date";
    }

    public static class SQLTableColumnNames
    {
        public const string NAME = "Name",
            SYSTEM = "System",
            PRICE = "Price",
            QUANTITY = "Quantity",
            TRADE_CASH = "TradeCash",
            TRADE_CREDIT = "TradeCredit",
            UPC = "UPC",
            DATE = "Date";
    }
}
