﻿using System;
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
            TradeWindow trade = new TradeWindow();
            trade.Show();

            //PointOfSales sales = new PointOfSales();
            //sales.Show();

            //Checkout checkout = new Checkout(100m);
            //checkout.ShowDialog();
        }
    }
}
