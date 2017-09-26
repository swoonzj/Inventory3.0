using Inventory;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        decimal cashTotal = 0;
        decimal creditTotal = 0;

        public MainWindow()
        {
            InitializeComponent();

            List<Item> items = new List<Item>();
            items.Add(new Item("Game", "System!", 2.99m,1,.5m,1m,"1"));
            items.Add(new Item("Game 2", "System!", 3.99m,1,.5m,1m,"2"));
            items.Add(new Item("Game 47", "System!", 9.99m,3,2m,3m,"000023"));
            lvList.ItemsSource = items;
            UpdateTotals();
            
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            List<Item> addedItems = new List<Item>();
            addedItems = lvCart.Items.Cast<Item>().ToList();
            foreach (Item item in lvList.SelectedItems)
            {
                addedItems.Add(item);
            }
            lvCart.ItemsSource = addedItems;
            UpdateTotals();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            List<Item> newList = new List<Item>();
            newList = lvCart.Items.Cast<Item>().ToList();
            foreach (Item item in lvCart.SelectedItems)
            {
                newList.Remove(item);
            }
            lvCart.ItemsSource = newList;
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            cashTotal = 0;
            creditTotal = 0;
            foreach (Item item in lvCart.Items)
            {
                cashTotal += item.tradeCash;
                creditTotal += item.tradeCredit;
            }
            txtTotalCash.Text = "Cash:\t\t$" + cashTotal.ToString("0.00");
            txtTotalCredit.Text = "\nCredit:\t\t$" + creditTotal.ToString("0.00");
        }

        
    }
}
