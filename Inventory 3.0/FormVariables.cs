namespace Inventory_3._0
{
    public static class Constants
    {
        public const string DISCOUNTUPC = "-1";
    }

    public static class PrinterVariables
    {

        public const string PRINTERNAME = "POS";
        public const string BACKUPPRINTERNAME = "\\\\Coregaming\\pos";
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
        public const string ITEMS = "tblItems";
        public const string INVENTORY = "tblInventory";
        public const string PRICES = "tblPrices";
        public const string UPC = "tblUPC";
        public const string TRANSACTION = "tblTransactions";
        public const string PAYMENT = "tblPayment";
        public const string CUSTOMERS = "tblCustomers";

        // Currently unused
        public const string VARIABLES = "tblVariables";
        public const string AUTOPRINT = "tblAutoPrint";           
    }

    public static class QuantityColumns
    {
        public const int Store = 1;
        public const int OutBack = 2;
        public const int Storage = 3;
        public const int Website = 4;
    }

    public static class InventoryColumnNames
    {
        public const string STORE = "Store";
        public const string OUTBACK = "OutBack";
        public const string STORAGE = "Storage";
        public const string WEBSITE = "Website";
    }

    public static class TransactionTypes
    {
        public const string SALE = "Sale";
        public const string PAYMENT = "Payment";
        public const string TRADE_CASH = "Trade-Cash";
        public const string TRADE_CREDIT = "Trade-Credit";
        public const string PAYMENT_CASH = "Payment - Cash";
        public const string PAYMENT_CREDITCARD = "Payment - Credit Card";
        public const string PAYMENT_STORECREDIT = "Payment - Store Credit";
        public const string PAYMENT_REWARDS = "Payment - Loyalty Rewards";
        public const string RETURN = "Return";
        public const string RETURN_CASH = "Return - Cash";
        public const string RETURN_CREDIT = "Return - Store Credit";
        public const string CHANGE_DUE = "Change Due:";
    }

    public static class RegisterStrings
    {
        public const string DISCOUNT = "Discount",
            TEN_PERCENT = "10% Off ",
            TWENTY_PERCENT = "20% Off ";
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
        public static string NAME = "Name",
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
            DATE = "Date",
            TYPE = "Type";
    }

    public static class ReceiptVariables
    {
        // Character width of receipt & components
        public const int receiptWidth = 38, 
            name = 30,
            system = 20,
            price = 10,
            payment = 30;

        public const string BorderLeft = "|",
            BorderRight = "|",
            BorderTop = "=",
            BorderCorner = "+",
            RECEIPT_HEADER = "Thank you for visiting!",
            RECEIPT_HEADER2 = "221 N. Broadway, Salem, NH \t(603)244-3393",
            RECEIPT_FOOTER = "RETURN POLICY:\n2 Weeks for defective items, 2 Days otherwise",
            LOGO = "Resources\\Logo.bmp";

        public const string FONTNAME = "Courier";
        public const int FONTSIZE = 10;
    }
}
