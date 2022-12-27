using System.Windows;

namespace Inventory_3._0
{
    /// <summary>
    /// Interaction logic for NewUserForm.xaml
    /// </summary>
    public partial class NewCustomerForm : Window
    {
        public NewCustomerForm()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            DBAccess.AddNewCustomer(txtName.Text, txtPhoneNumber.Text, txtEmail.Text, txtRewards.Text);
            this.Close();
        }
    }
}
