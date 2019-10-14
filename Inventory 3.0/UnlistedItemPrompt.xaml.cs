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
    /// Interaction logic for UnlistedItemPrompt.xaml
    /// </summary>
    public partial class UnlistedItemPrompt : Window
    {
        private TextBox txtPrice;
        private TextBox txtCash;
        private TextBox txtCredit;
        private bool trade;

        public Item item;
        public UnlistedItemPrompt(bool trade = false)
        {
            InitializeComponent();
            if (trade) SetupForTrade();
            else SetupForPOS();
            this.trade = trade;
            txtDescription.Focus();
        }

        private void SetupForPOS()
        {
            Label lblPrice = new Label();
            lblPrice.Content = "Price:";
            txtPrice = new TextBox();
            PriceDockPanel.Children.Add(lblPrice);
            PriceDockPanel.Children.Add(txtPrice);
        }

        private void SetupForTrade()
        {
            Label lblCash = new Label();
            lblCash.Content = "Cash:";
            Label lblCredit = new Label();
            lblCredit.Content = "Credit:";
            txtCash = new TextBox();
            txtCash.Width = 100;
            txtCash.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            txtCredit = new TextBox();
            txtCredit.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            PriceDockPanel.Children.Add(lblCash);
            PriceDockPanel.Children.Add(txtCash);
            PriceDockPanel.Children.Add(lblCredit);
            PriceDockPanel.Children.Add(txtCredit);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            decimal price, cash, credit;
            // Verify
            if (!trade)
            {
                if (Decimal.TryParse(txtPrice.Text, out price))
                {
                    item = new Item(txtDescription.Text, "Unlisted Item", price, 0, 0m, 0m, "0");
                    DialogResult = true;
                }
                else MessageBox.Show("The given price (" + txtPrice.Text + ") is not a valid number.", "Not a valid price.", MessageBoxButton.OK, MessageBoxImage.Stop);
            }
            else
            {
                if (Decimal.TryParse(txtCash.Text, out cash) && Decimal.TryParse(txtCredit.Text, out credit))
                {
                    item = new Item(txtDescription.Text, "Unlisted Item", 0m, 0, cash, credit, "0");
                    DialogResult = true;
                }
                else MessageBox.Show("The given values (" + txtCash.Text + " or "+ txtCredit.Text +") is not a valid number.", "Not a valid price.", MessageBoxButton.OK, MessageBoxImage.Stop);
            
            }
        }

    }
}
