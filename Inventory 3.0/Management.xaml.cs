using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
                items = DBAccess.SQLTableToList(ColumnNames.STORE, searchtext: searchString);
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

            if (items.Count == 1)
            {
                lvUPC.IsEnabled = true;
                PopulateUPClistview(items[0]);
            }
            else
            {
                lvUPC.IsEnabled = false;
                PopulateUPClistview(null);
            }
        }

        private void FillTextBoxes(Item item)
        {
            if (item == null)
            {
                
                txtName.Text = "";
                txtSystem.Text = "";
                txtPrice.Text = "";
                txtQuantity.Text = "";
                txtCash.Text = "";
                txtCredit.Text = "";
                lvUPC.ItemsSource = null;
                return;
            }
            if (item.name != null)
                txtName.Text = item.name;
            else
            {
                txtName.IsEnabled = false;
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

        private void PopulateUPClistview(Item item)
        {
            // Get UPCs
            if (item == null)
            {
                lvUPC.ItemsSource = null;
                return;
            }
            if (item.UPCs.Count == 0)
            {
                DBAccess.GetItemUPCs(item);
            }
            
            lvUPC.ItemsSource = new ObservableCollection<string>(item.UPCs);
        }
            

        private Item CompareSelection(List<Item> items)
        {
            if (items.Count == 0)
            {
                return null;
            }

            if (items.Count == 1) 
                return items[0];

            Item selection = items[0].Clone();
            
            foreach (Item item in items)
            {
                if (item.name != selection.name) selection.name = null;
                if (item.system != selection.system) selection.system = null;
                if (item.price != selection.price) selection.price = decimal.MinValue;
                if (item.quantity != selection.quantity) selection.quantity = int.MinValue;
                if (item.tradeCash != selection.tradeCash) selection.tradeCash = decimal.MinValue;
                if (item.tradeCredit != selection.tradeCredit) selection.tradeCredit = decimal.MinValue;
                if (item.SQLid != selection.SQLid) selection.SQLid = 0;
            }

            return selection;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string changes = "";
            if (txtName.IsEnabled == true && txtName.Text.ToString() != "") 
                changes += txtName.Text + " \n";
            if (txtSystem.Text.ToString() != "") 
                changes += txtSystem.Text + "\n";
            if (txtPrice.Text.ToString() != "") 
                changes += txtPrice.Text + "\n";
            if (txtQuantity.Text.ToString() != "") 
                changes += txtQuantity.Text + "\n";
            if (txtCash.Text.ToString() != "") 
                changes += txtCash.Text + "\n";
            if (txtCredit.Text.ToString() != "") 
                changes += txtCredit.Text + "\n";


            changes += "\nUPCs:\n";
            foreach (string upc in lvUPC.ItemsSource)
            {
                changes += upc + "\n";
            }

            MessageBoxResult result = MessageBox.Show("Save the following changes?\n" + changes, "Save Changes?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Saved. (but not really)");
                // ACTUALLY SAVE CHANGES !!!!!!

            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Check if lvUPC is disabled
            if (lvList.SelectedItems.Count != 1)
            {
                MessageBox.Show("Can only add UPCs to single items. Select only one item.", "Too many/few items selected", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                return;
            }
            // Make sure there is text in the field. If not, do nothing.
            if (txtUPC.Text == "") return;

            ((Item)lvList.SelectedItems[0]).UPCs.Add(txtUPC.Text);
            PopulateUPClistview((Item)lvList.SelectedItems[0]); 
        }
    }
}
