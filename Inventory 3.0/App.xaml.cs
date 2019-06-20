using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            List<Item> list = new List<Item>();
            list.Add(new Item("Test1", "System", 10m, 0, 0, 0, 0.ToString()));
            list.Add(new Item("Test2", "System", 10m, 0, 0, 0, 0.ToString()));


            //MultipleUPCHandler handler = new MultipleUPCHandler(list);
            //if (handler.ShowDialog() == true)
            //{
            //    MessageBox.Show(handler.selectedItem.ToString());
            //    handler.Close();
            //}

            //TradeWindow trade = new TradeWindow();
            //trade.Show();

            PointOfSales sales = new PointOfSales(list);
            sales.Show();

            //Checkout checkout = new Checkout(100m);
            //checkout.ShowDialog();
            //MessageBox.Show(checkout.checkout[1].ToString());
            //checkout.Close();
        }
    }
}
