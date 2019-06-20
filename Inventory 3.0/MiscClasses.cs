using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Inventory_3._0
{
    /// <summary>
    /// Contains search terms
    /// </summary>
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
            foreach (string term in searchText.Split(' '))
            {
                // Remove terms composed entirely of spaces
                string temp = term.Replace(" ", "");
                if (temp != "")
                    terms.Add(temp);
            }
        }

        public string GenerateSQLSearchString()
        {
            string output = "";

            // Case: list of terms is empty
            if (terms.Count == 0)
            {
                return "(Name LIKE \'%%\' OR System LIKE \'%%\') ";
            }

            for (int i = 0; i < terms.Count; i++)
            {
                string term = terms[i];
                output += "(Name LIKE \'%" + term + "%\' OR System LIKE \'%" + term + "%\') ";

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

        public static void LoadCSV(string filepath) // Add items from a Comma Separated Value file to the inventory
        {
            foreach (string s in File.ReadLines(filepath))
            {
                // Add item to collection
                DBAccess.AddNewItem(CreateItemFromCSVLine(s));
            }
        }

        public static Item CreateItemFromCSVLine(string CSVline)
        {
            List<string> line = ParseCSV(CSVline);
            List<int> quantity = new List<int>();
            quantity.Add(Convert.ToInt32(line[3]));
            quantity.Add(Convert.ToInt32(line[4]));
            quantity.Add(Convert.ToInt32(line[5]));

            List<string> upcs = new List<string>();
            for (int i = 8; i < line.Count; i++) // Index of first UPC)
            {
                upcs.Add(line[i]);
            }

            return new Item(line[0], line[1], line[2], quantity, line[6], line[7], upcs);
        }
    }

    public class ReceiptGenerator
    {
        List<Item> cart, payment;
        public StringBuilder receipt;

        public ReceiptGenerator(List<Item> cart, List<Item> payment)
        {
            this.cart = cart;
            this.payment = payment;

            receipt = new StringBuilder();
            Generate();
        }

        private void Generate()
        {
            //receipt.AppendLine(LOGO!!!!!!!!) // ADD LOGO!!!
            // Add Header at top
            // Separator
            receipt.AppendLine(Separator());
            // Name - System           Price
            foreach (Item item in cart)
            {
                string name = Truncate(item.name, ReceiptVariables.name).PadRight(ReceiptVariables.name);
                string system = Truncate(item.system, ReceiptVariables.system).PadRight(ReceiptVariables.system);
                string price = item.price.ToString("C").PadLeft(ReceiptVariables.price);
                receipt.AppendLine(ReceiptVariables.BorderLeft+name+system+price+ReceiptVariables.BorderRight);
            }
            receipt.AppendLine(Separator());
            foreach (Item item in payment)
            {
                string name = Truncate(item.name, ReceiptVariables.name).PadRight(ReceiptVariables.name);
                string system = String.Empty.PadRight(ReceiptVariables.system);
                string price = item.price.ToString("C").PadLeft(ReceiptVariables.price);
                receipt.AppendLine(ReceiptVariables.BorderLeft + name + system + price + ReceiptVariables.BorderRight);
            }
        }

        private string Separator()
        {
            string separator = String.Empty;
            for (int i = 0; i < ReceiptVariables.receiptWidth; i++)
            {
               separator = separator.Insert(0, ReceiptVariables.BorderTop);
            }

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
    }

    public class ReceiptPrinter
    {
        string receipt;

        public ReceiptPrinter(string receipt)
        {
            this.receipt = receipt;
        }

        public void Print()
        {
            PrintDialog printDialog = new PrintDialog(); 
            LocalPrintServer printServer = new LocalPrintServer();
            PrintQueue pq = printServer.GetPrintQueue(PrinterVariables.PRINTERNAME);
            printDialog.PrintQueue = pq;
            FlowDocument flowDoc = new FlowDocument(new Paragraph(new Run(receipt)));
            flowDoc.Name = "Receipt";
            flowDoc.FontFamily =  new System.Windows.Media.FontFamily(PrinterVariables.FONTNAME);
            flowDoc.FontSize = PrinterVariables.FONTSIZE;

            IDocumentPaginatorSource idpSource = flowDoc;
            printDialog.PrintDocument(idpSource.DocumentPaginator, "Printing Receipt");

        }
    }
}
