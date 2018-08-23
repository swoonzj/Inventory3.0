using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for Management.xaml
    /// </summary>
    public partial class Management : Window
    {
        public Management()
        {
            InitializeComponent();
            Search(String.Empty);
        }

        private void Search(string searchString)
        {
            try
            {
                List<Item> items = new List<Item>();
                items = DBAccess.SQLTableToList(TableNames.INVENTORY, searchtext: searchString);
                lvList.ItemsSource = items;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException.ToString());
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
        }

        private void DetectEnterKey(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search(txtSearch.Text);
            }
        }

        private void lvList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Item> items = new List<Item>();
            foreach (Item item in lvList.SelectedItems)
            {
                items.Add(item);
            }
            FillTextBoxes(CompareSelection(items)); 
        }

        private void FillTextBoxes(Item item)
        {
            if (item == null)
            {
                chkName.IsEnabled = true;
                chkSystem.IsEnabled = true;
                chkPrice.IsEnabled = true;
                chkQuantity.IsEnabled = true;
                chkCash.IsEnabled = true;
                chkCredit.IsEnabled = true;
                txtName.Text = "";
                txtSystem.Text = "";
                txtPrice.Text = "";
                txtQuantity.Text = "";
                txtCash.Text = "";
                txtCredit.Text = "";
                return;
            }
            if (item.name != null)
                txtName.Text = item.name;
            else
            {
                chkName.IsEnabled = false;
                txtName.Text = "";
            }
            if (item.system != null)
                txtSystem.Text = item.system;
            else
            {
                txtSystem.Text = "";
            }
            if (item.price != decimal.MinValue)
                txtPrice.Text = item.price.ToString("0.00");
            else
            {
                txtPrice.Text = "";
            }
            if (item.quantity != int.MinValue)
                txtQuantity.Text = item.quantity.ToString();
            else
            {
                txtQuantity.Text = "";
            }
            if (item.tradeCash != decimal.MinValue)
                txtCash.Text = item.tradeCash.ToString("0.00");
            else
            {
                txtCash.Text = "";
            }
            if (item.tradeCredit != decimal.MinValue)
                txtCredit.Text = item.tradeCredit.ToString("0.00");
            else
            { 
                txtCredit.Text = "";
            }
        }

        private Item CompareSelection(List<Item> items)
        {
            if (items.Count == 0)
            {
                return null;
            }
            Item selection = items[0].Clone();

            foreach (Item item in items)
            {
                if (item.name != selection.name) selection.name = null;
                if (item.system != selection.system) selection.system = null;
                if (item.price != selection.price) selection.price = decimal.MinValue;
                if (item.quantity != selection.quantity) selection.quantity = int.MinValue;
                if (item.tradeCash != selection.tradeCash) selection.tradeCash = decimal.MinValue;
                if (item.tradeCredit != selection.tradeCredit) selection.tradeCredit = decimal.MinValue;
                if (item.UPC != selection.UPC) selection.UPC = null;
            }

            return selection;
        }
    }
}
