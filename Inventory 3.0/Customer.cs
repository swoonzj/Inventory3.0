using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Inventory_3._0
{
    class Customer : INotifyPropertyChanged, IEquatable<Customer>
    {
        private string Name;
        public string name
        {
            get { return Name; }
            set
            {
                Name = value;
                NotifyPropertyChanged("name");
            }
        }

        private string PhoneNumber;
        public string phoneNumber
        {
            get { return PhoneNumber; }
            set
            {
                PhoneNumber = value;
                NotifyPropertyChanged("phoneNumber");
            }
        }

        private string Email;
        public string email
        {
            get { return Email; }
            set
            {
                Email = value;
                NotifyPropertyChanged("email");
            }
        }

        private string RewardsNumber;
        public string rewardsNumber
        {
            get { return RewardsNumber; }
            set
            {
                RewardsNumber = value;
                NotifyPropertyChanged("rewardsNumber");
            }
        }

        private List<Item> WishList;
        public List<Item> wishList
        {
            get { return WishList; }
            set 
            {
                WishList = value;
                NotifyPropertyChanged("wishList");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Equals(Customer other)
        {
            if (this.email != other.email) return false;
            if (this.name != other.name) return false;
            if (this.phoneNumber != other.phoneNumber) return false;
            return true;
        }

        public Customer (string name, string phoneNumber = "", string email = "", string rewardsNumber = "", List<Item> wishList = null)
        {
            this.name = name;
            this.phoneNumber = phoneNumber;
            this.email = email;
            this.rewardsNumber = rewardsNumber;
            this.wishList = wishList;
        }
    }
}
