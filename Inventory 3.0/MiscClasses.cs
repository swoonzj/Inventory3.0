using Inventory_3._0.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Printing;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Inventory_3._0
{
    #region SortableListViews

    /// <summary>
    /// Inherited from Window, but allows the sorting of ListViews (to eliminate boilerplate code)
    /// Taken and slightly modified from https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/how-to-sort-a-gridview-column-when-a-header-is-clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public class SortableListViews : Window
    {
        // Used for sorting ListView Columns
        GridViewColumnHeader _lastHeaderClicked = null; 
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
                        
        public void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            try
            {
                var headerClicked = e.OriginalSource as GridViewColumnHeader;

                ListSortDirection direction;

                if (headerClicked != null)
                {
                    if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                    {
                        if (headerClicked != _lastHeaderClicked)
                        {
                            direction = ListSortDirection.Ascending;
                        }
                        else
                        {
                            if (_lastDirection == ListSortDirection.Ascending)
                            {
                                direction = ListSortDirection.Descending;
                            }
                            else
                            {
                                direction = ListSortDirection.Ascending;
                            }
                        }

                        var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                        var sortBy = columnBinding.Path.Path ?? headerClicked.Column.Header as string;

                        Sort(sortBy, direction, sender as ListView);

                        if (direction == ListSortDirection.Ascending)
                        {
                            headerClicked.Column.HeaderTemplate =
                                Resources["HeaderTemplateArrowUp"] as DataTemplate;
                        }
                        else
                        {
                            headerClicked.Column.HeaderTemplate =
                                Resources["HeaderTemplateArrowDown"] as DataTemplate;
                        }

                        // Remove arrow from previously sorted header
                        if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                        {
                            _lastHeaderClicked.Column.HeaderTemplate = null;
                        }

                        _lastHeaderClicked = headerClicked;
                        _lastDirection = direction;
                    }
                }
            }
            catch{}
        }

        protected virtual void Sort(string sortBy, ListSortDirection direction, ListView lv)
        {
            try
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);

                dataView.SortDescriptions.Clear();
                dataView.SortDescriptions.Add(new SortDescription(sortBy, direction));
                dataView.Refresh();
            }
            catch{}
        }
    }

    public class SortableListViewsWithItems : SortableListViews
    {
        protected override void Sort(string sortBy, ListSortDirection direction, ListView lv)
        {
            try
            {
                ICollectionView dataView = CollectionViewSource.GetDefaultView(lv.ItemsSource);

                dataView.SortDescriptions.Clear();
                dataView.SortDescriptions.Add(new SortDescription(sortBy, direction));
                dataView.SortDescriptions.Add(new SortDescription("system", ListSortDirection.Ascending));
                dataView.SortDescriptions.Add(new SortDescription("name", ListSortDirection.Ascending));
                dataView.Refresh();
            }
            catch { }
        }
    }
    #endregion

    #region ContextMenu
    public class ListViewContextMenu : ContextMenu
    {
        ListView lv;
        bool allSelected = false;
        public ListViewContextMenu(ListView lv)
        {
            this.lv = lv;
        }        

        void SelectAllClick(object sender, RoutedEventArgs e)
        {
            if (!allSelected) lv.SelectAll();
            else lv.UnselectAll();
            allSelected = !allSelected;
        }

        protected override void OnOpened(RoutedEventArgs e)
        {
            Items.Clear();
            MenuItem itemTotal = new MenuItem();
            itemTotal.Header = "Total Items: " + lv.Items.Count;
            itemTotal.FontWeight = FontWeights.Bold;
            itemTotal.StaysOpenOnClick = true;
            Items.Add(itemTotal);
            MenuItem selectAll = new MenuItem();
            selectAll.Header = "Select all";
            selectAll.Click += SelectAllClick;
            Items.Add(selectAll);
            base.OnOpened(e);
        }
    }
    #endregion

    #region SearchTerms
    public class SearchTerms
    {
        public List<string> terms = new List<string>();

        /// <summary>
        /// Divides a single search string into individual terms (separated by spaces), and stores them in the array "terms"
        /// </summary>
        /// <param name="searchText">Single string</param>
        public SearchTerms(string searchText)
        {
            // Split up input by spaces
            List<string> splitList = new List<string>(searchText.Split(' '));
            string term;
            for (int i = 0; i < splitList.Count; i++)
            {
                term = splitList[i];
                // Check for quotes
                if (term.StartsWith("\""))
                {
                    for (int j = i + 1; j < splitList.Count; j++)
                    {
                        // Matching end quote, add term & continue
                        if (splitList[j].EndsWith("\""))
                        {
                            term += " " + splitList[j];
                            term = term.Replace("\"", "");
                            i = j;
                            break;
                        }
                        // If no closing quote in whole list, ignore inital quote and continue
                        else if (j == splitList.Count -1 && !splitList[j].EndsWith("\""))
                        {
                            term = splitList[i];
                            break;
                        }
                        else
                        {
                            term += " " + splitList[j];
                        }
                    }
                }
                // Remove terms composed entirely of spaces
                //string temp = term.Replace(" ", "");
                if (term != "")
                    terms.Add(term);
            }
        }

        public string GenerateItemSQLSearchString()
        {
            string output = "";

            // Case: list of terms is empty
            if (terms.Count == 0)
            {
                return string.Format("({0} LIKE \'%%\' OR {1} LIKE \'%%\') ", SQLTableColumnNames.NAME, SQLTableColumnNames.SYSTEM);
            }

            for (int i = 0; i < terms.Count; i++)
            {
                string term = terms[i];
                output += string.Format("({0} LIKE \'%{2}%\' OR {1} LIKE \'%{2}%\') ", SQLTableColumnNames.NAME, SQLTableColumnNames.SYSTEM, term);

                // Add an "AND" in between conditions
                // Do not add one if it is the last search term
                if (i != terms.Count - 1)
                {
                    output += "AND ";
                }
            }
            return output;
        }

        public string GenerateCustomerSQLSearchString()
        {
            string output = "";

            // Case: list of terms is empty
            if (terms.Count == 0)
            {
                return string.Format("({0} LIKE \'%%\' OR {1} LIKE \'%%\' OR {2} LIKE \'%%\' OR {3} LIKE \'%%\') ", SQLTableColumnNames.NAME, SQLTableColumnNames.PHONE, SQLTableColumnNames.EMAIL, SQLTableColumnNames.REWARDS);
            }

            for (int i = 0; i < terms.Count; i++)
            {
                string term = terms[i];
                output += string.Format("({0} LIKE \'%{4}%\' OR {1} LIKE \'%{4}%\' OR {2} LIKE \'%{4}%\' OR {3} LIKE \'%{4}%\') ", SQLTableColumnNames.NAME, SQLTableColumnNames.PHONE, SQLTableColumnNames.EMAIL, SQLTableColumnNames.REWARDS, term);

                // Add an "AND" in between conditions
                // Do not add one if it is the last search term
                if (i != terms.Count - 1)
                {
                    output += "AND ";
                }
            }
            return output;
        }
    }

    #endregion

    #region Converters

    public class IntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((int)value == int.MinValue)
                return string.Empty;
            

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (String.IsNullOrWhiteSpace((string)value))
                return int.MinValue;

            return value;
        }
    }

    public class DecimalConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {            
            if ((decimal)value == decimal.MinValue)
                return string.Empty;           

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (String.IsNullOrWhiteSpace((string)value))
                return decimal.MinValue;

            return value;
        }
    }

    #endregion

    #region ImportCSV
    /// <summary>
    /// Interaction logic for importing Product data from .CSV (comma separated values)
    /// </summary>
    public class ImportCSV
    {

        /// <summary>
        /// Used by loadCSV() to identify individual lines in the file. Returns each line, separated into strings
        /// </summary>
        /// <param name="input">String containing one line of data, separated by commas</param>
        /// <returns>Each line of input, separated into individual strings contained in a List</returns>

        private static List<string> ParseCSV(string input)
        {
            string temp = string.Empty;  //each individual word/string
            List<string> strarray = new List<string>(); // each line, separated by commas
            bool quote = false;
            foreach (char c in input)
            {
                if (c == '\"' && !quote) quote = true;   // Test for first ' " '  (quotation mark)
                else if (c == '\"' && quote) quote = false; // Test for second quotation mark
                else if ((c == ',' && quote == false) || c == '\n') // if comma is encountered & no quotations, add 'temp' to 'strarray'
                {                                                   // Or if a newline is encountered (end of line)
                    //if (temp == string.Empty) temp = "0";
                    strarray.Add(temp.Trim());
                    temp = string.Empty;
                }
                else temp += c;
            }
            if (temp != String.Empty) strarray.Add(temp);  // necessary if CSV file has only no commas after final item
            return strarray;
        }

        public async static void LoadCSV(string filepath) // Add items from a Comma Separated Value file to the inventory
        {
            foreach (string s in File.ReadLines(filepath))
            {
                // Add item to collection
                await DBAccess.AddNewItem(CreateItemFromCSVLine(s));
            }
        }

        public static Item CreateItemFromCSVLine(string CSVline)
        {
            List<string> line = ParseCSV(CSVline);

            if (line.Count > 7)
            {
                List<int> quantity = new List<int>();
                quantity.Add(Convert.ToInt32(line[3]));
                quantity.Add(Convert.ToInt32(line[4]));
                quantity.Add(Convert.ToInt32(line[5]));
                quantity.Add(Convert.ToInt32(line[6]));

                List<string> upcs = new List<string>();
                if (line.Count > 9)
                {
                    for (int i = 9; i < line.Count; i++) // Index of first UPC)
                    {
                        upcs.Add(line[i]);
                    }
                }

                return new Item(line[0], line[1], line[2], quantity, line[7], line[8], upcs);
            }
            else if (line.Count >= 2)
            {
                return new Item(line[0], line[1]);
            }
            else return new Item();
        }
    }

    #endregion

    #region ReceiptClasses

    public class ReceiptGenerator
    {
        Transaction transaction;
        List<Item> cart, payment;
        public StringBuilder receipt;
        public FlowDocument flowDoc;
        string date, transactionNumber;

        /// <summary>
        /// Generates a printable receipt
        /// </summary>
        /// <param name="cart">Items purchased</param>
        /// <param name="payment"></param>
        /// <param name="date"></param>
        /// <param name="transactionNumber"></param>
        public ReceiptGenerator(List<Item> cart, List<Item> payment, string date, string transactionNumber)
        {
            this.cart = cart;
            this.payment = payment;
            this.date = date;
            this.transactionNumber = transactionNumber;

            receipt = new StringBuilder();
            flowDoc = Generate();
        }

        public ReceiptGenerator(Transaction transaction)
        {
            this.transaction = transaction;

            receipt = new StringBuilder();
            flowDoc = GenerateFromTransaction();
        }

        private FlowDocument Generate()
        {
            FlowDocument flowDoc = new FlowDocument();
            //receipt.AppendLine(LOGO!!!!!!!!) // ADD LOGO!!!
            // Add Header at top
            // Separator
            //receipt.AppendLine(Separator());
            // Name - System           Price


            // Logo
            Image img = new Image();
            BitmapImage bimg = new BitmapImage();
            bimg.BeginInit();
            bimg.UriSource = new Uri(ReceiptVariables.LOGO, UriKind.Relative);
            bimg.EndInit();
            img.Source = bimg;
            flowDoc.Blocks.Add(new BlockUIContainer(img));

            // Headers
            flowDoc.Blocks.Add(new Paragraph(new Run(ReceiptVariables.RECEIPT_HEADER2 + "\n")) { TextAlignment = System.Windows.TextAlignment.Center, FontWeight = System.Windows.FontWeights.Bold });
            // Transaction details
            flowDoc.Blocks.Add(new Paragraph(new Run("Transaction Number: " + transactionNumber + "\n" + date + "\n")) { TextAlignment = System.Windows.TextAlignment.Left, FontWeight = System.Windows.FontWeights.DemiBold });

            flowDoc.FontSize = ReceiptVariables.FONTSIZE;
            flowDoc.FontFamily = new System.Windows.Media.FontFamily(ReceiptVariables.FONTNAME);
            
            foreach (Item item in cart)
            {
                string price;
                string name = Truncate(item.name, ReceiptVariables.name) + "\t-\t";
                string system = Truncate(item.system, ReceiptVariables.system).PadRight(ReceiptVariables.system);
                if (item.quantity[0] > 1)
                {
                    price = item.quantity[0].ToString() + " @ " + item.price.ToString("C") + " =    " + item.priceTotal.ToString("C");
                }
                else price = item.price.ToString("C");
                price.PadLeft(ReceiptVariables.price);
                //receipt.AppendLine(ReceiptVariables.BorderLeft+name+system+price+ReceiptVariables.BorderRight);

                Paragraph p = new Paragraph(new Run(name + system));
                p.Margin = new System.Windows.Thickness(0);
                p.TextAlignment = System.Windows.TextAlignment.Left;
                flowDoc.Blocks.Add(p);
                Paragraph p2 = new Paragraph(new Run(price));
                p2.Margin = new System.Windows.Thickness(0);
                p2.TextAlignment = System.Windows.TextAlignment.Right;
                flowDoc.Blocks.Add(p2);
            }
            flowDoc.Blocks.Add(new Paragraph(new Run(Separator())));
            foreach (Item item in payment)
            {
                string name = Truncate(item.name, ReceiptVariables.name).PadRight(ReceiptVariables.name);
                string system = String.Empty.PadRight(ReceiptVariables.system);
                string price = item.price.ToString("C").PadLeft(ReceiptVariables.price);
                //receipt.AppendLine(name + system + price);
                Paragraph p = new Paragraph(new Run(name + system + price));
                p.TextAlignment = System.Windows.TextAlignment.Right;

                flowDoc.Blocks.Add(p);
            }

            // Headers
            flowDoc.Blocks.Add(new Paragraph(new Run(ReceiptVariables.RECEIPT_FOOTER)));

            flowDoc.PageWidth = 300.0;
            return flowDoc;
        }

        private FlowDocument GenerateFromTransaction()
        {

            FlowDocument flowDoc = new FlowDocument();
            //receipt.AppendLine(LOGO!!!!!!!!) // ADD LOGO!!!
            // Add Header at top
            // Separator
            //receipt.AppendLine(Separator());
            // Name - System           Price


            // Logo
            Image img = new Image();
            BitmapImage bimg = new BitmapImage();
            bimg.BeginInit();
            bimg.UriSource = new Uri(ReceiptVariables.LOGO, UriKind.Relative);
            bimg.EndInit();
            img.Source = bimg;
            flowDoc.Blocks.Add(new BlockUIContainer(img));

            // Headers
            if (Settings.Default.useNashuaConnectionString)
            {
                flowDoc.Blocks.Add(new Paragraph(new Run(ReceiptVariables.NASHUA_RECEIPT_HEADER + "\n")) { TextAlignment = System.Windows.TextAlignment.Center, FontWeight = System.Windows.FontWeights.Bold });
            } else
            {
                flowDoc.Blocks.Add(new Paragraph(new Run(ReceiptVariables.RECEIPT_HEADER2 + "\n")) { TextAlignment = System.Windows.TextAlignment.Center, FontWeight = System.Windows.FontWeights.Bold });
            }
            // Transaction details
            flowDoc.Blocks.Add(new Paragraph(new Run("Transaction Number: " + transaction.transactionNumber + "\n" + transaction.date + "\n")) { TextAlignment = System.Windows.TextAlignment.Left, FontWeight = System.Windows.FontWeights.DemiBold });

            flowDoc.FontSize = ReceiptVariables.FONTSIZE;
            flowDoc.FontFamily = new System.Windows.Media.FontFamily(ReceiptVariables.FONTNAME);

            foreach (Item item in transaction.items)
            {
                string price;
                string name = Truncate(item.name, ReceiptVariables.name) + "\t-\t";
                string system = Truncate(item.system, ReceiptVariables.system).PadRight(ReceiptVariables.system);

                switch (transaction.transactionType)
                {
                    case TransactionTypes.SALE:
                        price = item.quantity[0].ToString() + " @ " + item.price.ToString("C") + " =    " + item.priceTotal.ToString("C");
                        break;
                    case TransactionTypes.TRADE_CASH:
                        price = item.quantity[0].ToString() + " @ " + item.tradeCash.ToString("C") + " =    " + item.cashTotal.ToString("C");
                        break;
                    case TransactionTypes.TRADE_CREDIT:
                        price = item.quantity[0].ToString() + " @ " + item.tradeCredit.ToString("C") + " =    " + item.creditTotal.ToString("C");
                        break;
                    default:
                        price = "NULL";
                        break;
                }
             
                price.PadLeft(ReceiptVariables.price);
                //receipt.AppendLine(ReceiptVariables.BorderLeft+name+system+price+ReceiptVariables.BorderRight);

                Paragraph p = new Paragraph(new Run(name + system));
                p.Margin = new System.Windows.Thickness(0);
                p.TextAlignment = System.Windows.TextAlignment.Left;
                flowDoc.Blocks.Add(p);
                Paragraph p2 = new Paragraph(new Run(price));
                p2.Margin = new System.Windows.Thickness(0);
                p2.TextAlignment = System.Windows.TextAlignment.Right;
                flowDoc.Blocks.Add(p2);
            }
            flowDoc.Blocks.Add(new Paragraph(new Run(Separator())));
            foreach (Transaction item in transaction.payment)
            {

                string name = Truncate(item.transactionType, ReceiptVariables.name).PadRight(ReceiptVariables.name);
                string system = String.Empty.PadRight(ReceiptVariables.system);
                string price = item.total.ToString("C").PadLeft(ReceiptVariables.price);
                //receipt.AppendLine(name + system + price);
                Paragraph p = new Paragraph(new Run(name + system + price));
                p.TextAlignment = System.Windows.TextAlignment.Right;

                flowDoc.Blocks.Add(p);
            }

            // Headers
            flowDoc.Blocks.Add(new Paragraph(new Run(ReceiptVariables.RECEIPT_FOOTER)));

            flowDoc.PageWidth = 300.0;
            return flowDoc;
        }

        private string Separator()
        {
            string separator = String.Empty;
            for (int i = 0; i < ReceiptVariables.receiptWidth; i++)
            {
               separator = separator.Insert(0, ReceiptVariables.BorderTop);
            }

            separator.Insert(separator.Length - 1, "\n");

            return separator;
        }
        
        private string Truncate(string s, int length)
        {
            if (s.Length > length)
            {
                return s.Substring(0, length);
            }
            else
                return s;
        }

        public void ViewFlowDoc()
        {
            FlowDocumentReader reader = new FlowDocumentReader();
            reader.Document = flowDoc;

        }
    }

    public class ReceiptPrinter
    {
        string receipt;
        FlowDocument flowDoc;

        public ReceiptPrinter(FlowDocument flowDoc)
        {
            this.flowDoc = flowDoc;
        }

        public ReceiptPrinter(string receipt)
        {
            this.receipt = receipt;
        }

        //public void Print()
        //{
        //    PrintDialog printDialog = new PrintDialog(); 
        //    LocalPrintServer printServer = new LocalPrintServer();
        //    PrintQueue pq = printServer.GetPrintQueue(PrinterVariables.PRINTERNAME);
        //    printDialog.PrintQueue = pq;
        //    FlowDocument flowDoc = new FlowDocument(new Paragraph(new Run(receipt)));
        //    flowDoc.Name = "Receipt";
        //    flowDoc.FontFamily =  new System.Windows.Media.FontFamily(PrinterVariables.FONTNAME);
        //    flowDoc.FontSize = PrinterVariables.FONTSIZE;

        //    IDocumentPaginatorSource idpSource = flowDoc;
        //    printDialog.PrintDocument(idpSource.DocumentPaginator, "Printing Receipt");

        //}

        public void PrintSilently()
        {
            PrintDialog printDialog = new PrintDialog();
            PrintQueue pq;
            IDocumentPaginatorSource idpSource = flowDoc;
            try
            {
                if (Settings.Default.useNetworkPrinter)
                {
                    string printServerName = @"\\Coregaming";
                    string printQueueName = "POS";

                    PrintServer ps = new PrintServer(printServerName);
                    pq = ps.GetPrintQueue(printQueueName);
                    flowDoc.PageWidth = printDialog.PrintableAreaWidth;
                }
                else
                {
                    LocalPrintServer printServer = new LocalPrintServer();
                    pq = printServer.GetPrintQueue(PrinterVariables.PRINTERNAME);
                    flowDoc.PageWidth = printDialog.PrintableAreaWidth;
                }
                idpSource = flowDoc;
                printDialog.PrintQueue = pq;
                printDialog.PrintDocument(idpSource.DocumentPaginator, "Printing Receipt");
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in PrintSilently():\n" + e.Message + "\n" + e.Data.ToString());
            }
        }

        public void PrintVerbose()
        {
            PrintDialog printDialog = new PrintDialog();
            PrintQueue pq;
            IDocumentPaginatorSource idpSource = flowDoc;
            try
            {
                printDialog.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show("Error in PrintVerbose():\n" + e.Message + "\n" + e.Data.ToString());
            }
        }
    }

    #endregion
}
