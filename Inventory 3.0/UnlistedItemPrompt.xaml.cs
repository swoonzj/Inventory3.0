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
        public Item item;
        public UnlistedItemPrompt()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            decimal price;
            // Verify
            if (Decimal.TryParse(txtPrice.Text, out price))
            {
                item = new Item(txtDescription.Text, "Unlisted Item", price, 0, 0m, 0m, "0");
                DialogResult = true;
            }
            else MessageBox.Show("The given price (" + txtPrice.Text + ") is not a valid number.", "Not a valid price.", MessageBoxButton.OK, MessageBoxImage.Stop);            
        }

    }
}
