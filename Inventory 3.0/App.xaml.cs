using System;
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
            try
            {
                //List<Item> list = new List<Item>();
                //list.Add(new Item("Test1", "System", 10m, 1, 0, 0, 0.ToString()));
                //list.Add(new Item("Test2", "System", 100m, 3, 0, 0, 0.ToString()));


                //MultipleUPCHandler handler = new MultipleUPCHandler(list);
                //if (handler.ShowDialog() == true)
                //{
                //    MessageBox.Show(handler.selectedItem.ToString());
                //    handler.Close();
                //}

                //MoveInventory move = new MoveInventory();
                //move.Show();

                //TradeWindow trade = new TradeWindow();
                //trade.Show();

                //ManageTransactions manage = new ManageTransactions();
                //manage.Show();

                //Management manage = new Management();
                //manage.Show();

                PointOfSales sales = new PointOfSales();
                sales.Show();

                //Checkout checkout = new Checkout(100m);
                //checkout.ShowDialog();
                //MessageBox.Show(checkout.checkout[1].ToString());
                //checkout.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
