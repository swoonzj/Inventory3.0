﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace Inventory_3._0
{
    class DBAccess
    {
        // Example of SQL command to select data from all tables using unique item ID to JOIN tables

        //SELECT tblItems.Name, tblItems.System, tblPrices.Price, tblInventory.Store, tblPrices.Cash, tblPrices.Credit, tblUPC.UPC
        //From tblItems
        //JOIN tblInventory ON tblInventory.id = tblItems.id
        //JOIN tblPrices ON tblInventory.id = tblPrices.id
        //JOIN tblUPC ON tblInventory.id = tblUPC.id
        //WHERE UPC=2222;

        // SQL table structures:

        //tblInventory:
        //id
        //Store
        //OutBack
        //Storage
        //Website

        //tblItems:
        //id
        //Name
        //System

        //tblPrices:
        //id
        //Price
        //Cash
        //Credit

        //tblTransactions:
        //Number
        //Date
        //Type
        //Price
        //id

        //tblUPC:
        //UPC
        //id

        static SqlConnection homeConnection = new SqlConnection(Properties.Settings.Default.HomeInventoryConnectionString); // Home
        static SqlConnection simpConnection = new SqlConnection(Properties.Settings.Default.CgSimpConnectionString); // Simp
        static SqlConnection nashuaConnection = new SqlConnection(Properties.Settings.Default.NashuaConnectionString); // Nashua
        static SqlConnection traderJoeConnection = new SqlConnection(Properties.Settings.Default.SQLServerConnectionString2); // Default
        static SqlConnection connect 
        { 
            get
            {
#if HOMEDEBUG
                return homeConnection;
#else
                if (Properties.Settings.Default.useSimpInventory)
                {
                    return simpConnection; // SIMP
                }
                else if (Properties.Settings.Default.useNashuaConnectionString)
                {
                    return nashuaConnection; // NASHUA
                }
                else 
                {
                    return traderJoeConnection; // Store
                }
            #endif
            }
        }


    public static void CloseSQLConnection()
        {
            connect.Close();
        }

        // Check a string for characters that would throw off SQL formatting
        private static string CheckForSpecialCharacters(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\'') // check for ' (apostrophe)
                { input = input.Insert(i, "\'"); i++; }
            }
            return input;
        }

        /// <summary>
        /// Returns a List of all Items (matching an optional search string), with inventory information on passed Inventory column
        /// Does NOT fetch UPC information
        /// Selects only the top 200 results!!!
        /// </summary>
        /// <param name="inventoryColumn">Table name of an INVENTORY type table</param>
        /// <param name="sortBy">Column to sort by.   (Optional)</param>
        /// <param name="ascending">True: Results are sorted (A->Z), False: (Z->A).  (Optional)</param>
        /// <param name="searchtext">Text to narrow results to items containing this text.  (Optional)</param>
        /// <returns>A List of Items</returns>
        public static async Task<List<Item>> GetItemsAsList(string sortBy = "System", bool ascending = true, string searchtext = "", bool limitResults = true)
        {
            List<Item> collection = new List<Item>();
            Item item;
            SearchTerms searchTerms;

            // Save sorting order to string "order" (descending/ascending)
            string order;
            SqlCommand cmd;
            if (ascending)
                order = "ASC";
            else
                order = "DESC";
            
            // Limit number of results?
            string limit = "";
            if (limitResults)
            {
                limit = " TOP 200";
            }

            // parameters cannot be null
            if (searchtext == null) searchtext = "";
            if (sortBy == null) sortBy = "System";

            // Check for special characters, then divide searchtext into individual terms
            searchtext = CheckForSpecialCharacters(searchtext);
            searchTerms = new SearchTerms(searchtext);

            if (sortBy != "Name")
            {
                cmd = new SqlCommand("SELECT" + limit + " Name, System, Price, Cash, Credit, " + TableNames.ITEMS + ".id FROM " + TableNames.ITEMS +
                    " JOIN " + TableNames.PRICES + " ON " + TableNames.ITEMS + ".id =  " + TableNames.PRICES + ".id " +
                    "WHERE " + searchTerms.GenerateItemSQLSearchString() +
                    " ORDER BY " + sortBy + " " + order + ", Name", connect);
            }
            else
            {
                cmd = new SqlCommand("SELECT" + limit + " Name, System, Price, Cash, Credit, " + TableNames.ITEMS + ".id FROM " + TableNames.ITEMS +
                    " JOIN " + TableNames.PRICES + " ON " + TableNames.ITEMS + ".id =  " + TableNames.PRICES + ".id " +
                    "WHERE " + searchTerms.GenerateItemSQLSearchString() +
                    " ORDER BY " + sortBy + " " + order, connect);
            }

            try
            {
                await Task.Run(() =>
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read() == true)
                        {
                            item = SQLReaderToItem(reader);
                            if (item != null)
                            {
                                collection.Add(item);
                            }
                        }
                    });
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN SQLTableToCollection:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

            foreach (Item newitem in collection)
            {
                newitem.UPCs = await DBAccess.GetUPCsWithID(newitem.SQLid);
                newitem.quantity = new ObservableCollection<int>(await DBAccess.GetQuantities(newitem.SQLid));
                if (newitem.quantity.Count != 5) newitem.quantity = new ObservableCollection<int> { 0, 0, 0, 0, 0 }; // Should only be necessary for items with no quantities.
            }

            return collection;
        }

        /// <summary>
        /// Returns a List of all Customers (matching an optional search string)
        /// Does NOT fetch wish list info
        /// Selects only the top 200 results by default.
        /// </summary>
        /// <param name="sortBy">Column to sort by.   (Optional)</param>
        /// <param name="ascending">True: Results are sorted (A->Z), False: (Z->A).  (Optional)</param>
        /// <param name="searchtext">Text to narrow results to items containing this text.  (Optional)</param>
        /// <returns>A List of Items</returns>
        public static async Task<List<Customer>> GetCustomerList(string sortBy = SQLTableColumnNames.NAME, bool ascending = true, string searchtext = "", bool limitResults = true)
        {
            List<Customer> collection = new List<Customer>();
            SearchTerms searchTerms;

            // Save sorting order to string "order" (descending/ascending)
            string order;
            SqlCommand cmd;
            if (ascending)
                order = "ASC";
            else
                order = "DESC";

            // Limit number of results?
            string limit = "";
            if (limitResults)
            {
                limit = " TOP 200";
            }

            // parameters cannot be null
            if (searchtext == null) searchtext = "";
            if (sortBy == null) sortBy = SQLTableColumnNames.NAME;

            // Check for special characters, then divide searchtext into individual terms
            searchtext = CheckForSpecialCharacters(searchtext);
            searchTerms = new SearchTerms(searchtext);

            string columnsToSelect = String.Format(" {0}, {1}, {2}, {3}, {4} ", SQLTableColumnNames.NAME, SQLTableColumnNames.PHONE, SQLTableColumnNames.EMAIL, SQLTableColumnNames.REWARDS, SQLTableColumnNames.ID);

            if (sortBy != SQLTableColumnNames.NAME)
            {
                cmd = new SqlCommand("SELECT" + limit + columnsToSelect + " FROM " + TableNames.CUSTOMERS +
                    " WHERE " + searchTerms.GenerateCustomerSQLSearchString() +
                    " ORDER BY " + sortBy + " " + order + ", Name", connect);
            }
            else
            {
                cmd = new SqlCommand("SELECT" + limit + columnsToSelect + " FROM " + TableNames.CUSTOMERS +
                    " WHERE " + searchTerms.GenerateCustomerSQLSearchString() +
                    " ORDER BY " + sortBy + " " + order, connect);
            }

            try
            {
                await Task.Run(() =>
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read() == true)
                    {
                        Customer customer = SQLReaderToCustomer(reader);
                        if (customer != null)
                        {
                            collection.Add(customer);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN SQLTableToCollection:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

            return collection;
        }

        public async static Task<bool> AddNewItem(Item item)
        {
            return await AddNewItem(item.name, item.system, item.price, item.quantity, item.tradeCash, item.tradeCredit, item.UPCs);
        }

        public async static Task<bool> AddNewItem(string name, string system, decimal price, ObservableCollection<int> inventory, decimal cash, decimal credit, List<string> upcs)
        {
            bool success = false;
            //name = CheckForSpecialCharacters(name);
            //system = CheckForSpecialCharacters(system);

            // Add data to table
            SqlCommand cmdItem = new SqlCommand("INSERT INTO " + TableNames.ITEMS + " OUTPUT INSERTED.ID VALUES(@NAME, @SYSTEM)", connect);
            cmdItem.Parameters.Add("@NAME", SqlDbType.VarChar).Value = name;
            cmdItem.Parameters.Add("@SYSTEM", SqlDbType.VarChar).Value = system;

            SqlCommand cmdPrice = new SqlCommand("INSERT INTO " + TableNames.PRICES + " VALUES(@ID, @PRICE, @CASH, @CREDIT)", connect);
            cmdPrice.Parameters.Add("@PRICE", SqlDbType.Money).Value = price;
            cmdPrice.Parameters.Add("@CASH", SqlDbType.Money).Value = cash;
            cmdPrice.Parameters.Add("@CREDIT", SqlDbType.Money).Value = credit;

            SqlCommand cmdInventory = new SqlCommand("INSERT INTO " + TableNames.INVENTORY + " VALUES(@ID, @QUANTITY1, @QUANTITY2, @QUANTITY3, @QUANTITY4, @QUANTITY5)", connect);
            cmdInventory.Parameters.Add("@QUANTITY1", SqlDbType.Int).Value = inventory[0];
            cmdInventory.Parameters.Add("@QUANTITY2", SqlDbType.Int).Value = inventory[1];
            cmdInventory.Parameters.Add("@QUANTITY3", SqlDbType.Int).Value = inventory[2];
            cmdInventory.Parameters.Add("@QUANTITY4", SqlDbType.Int).Value = inventory[3];
            cmdInventory.Parameters.Add("@QUANTITY5", SqlDbType.Int).Value = inventory[4];

            // execute command  & close connection
            await Task.Run(async () =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    int ID = (int)cmdItem.ExecuteScalar(); // Get the unique, auto-incremented ID for the item.

                    cmdPrice.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                    cmdPrice.ExecuteNonQuery();

                    cmdInventory.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                    cmdInventory.ExecuteNonQuery();
                    connect.Close();
                    await AddUPCs(upcs, ID);
                    success = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("ERROR IN AddNewItemToTable:\n" + e.Message);
                    success = false;
                }
                finally
                {
                    connect.Close();
                }
            });
            return success;
        }

        public async static Task<bool> AddNewCustomer(string name, string phone = "", string email = "", string rewardsNumber = "")
        {
            bool success = false;
            //name = CheckForSpecialCharacters(name);
            //system = CheckForSpecialCharacters(system);

            // Add data to table
            SqlCommand cmdCustomer = new SqlCommand("INSERT INTO " + TableNames.CUSTOMERS + " VALUES(@NAME, @PHONE, @EMAIL, @REWARDS)", connect);
            cmdCustomer.Parameters.Add("@NAME", SqlDbType.VarChar).Value = name;
            cmdCustomer.Parameters.Add("@PHONE", SqlDbType.VarChar).Value = phone;
            cmdCustomer.Parameters.Add("@EMAIL", SqlDbType.VarChar).Value = email;
            cmdCustomer.Parameters.Add("@REWARDS", SqlDbType.VarChar).Value = rewardsNumber;

            // execute command  & close connection
            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    cmdCustomer.ExecuteNonQuery();
                    connect.Close();
                    success = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("ERROR IN AddNewItemToTable:\n" + e.Message);
                    success = false;
                }
                finally
                {
                    connect.Close();
                }
            });
            return success;
        }

        /// <summary>
        /// Creates a string in the format of "([ID],[UPC])" for insertion into SQL table
        /// </summary>
        /// <param name="upcs">List of UPCs</param>
        /// <param name="ID">SQL ID of associated item</param>
        /// <returns>A string in the format of "([ID],[UPC])"</returns>
        private static string CreateUPCInsertString(List<string> upcs, int ID)
        {
            string output = "";

            foreach (string upc in upcs)
            {
                output += "(" + ID + ", '" + upc + "'),";
            }

            // Remove the last comma
            return output.Remove(output.Length - 1);
        }

        /// <summary>
        /// Returns Items with duplicate Name/System data
        /// </summary>
        /// <param name="item">Item to check for duplicates</param>
        /// <returns>List of duplicate items. If empty, there are no duplicates</returns>
        public async Task<List<Item>> GetItemDuplicates(Item item)
        {
            List<Item> items = new List<Item>();

            string sqlcommand = String.Format("SELECT Name, System FROM {0} WHERE Name like \'{1}\' AND System like\'{2}\'", TableNames.ITEMS, item.name, item.system); // Might need to select more !!!!!!
            SqlCommand cmd = new SqlCommand(sqlcommand);

            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read() == true)
                    {
                        item = SQLReaderToItem(reader);
                        if (item != null)
                            items.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in GetItemDuplicates:\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
            
            return items;
        }

        /// <summary>
        /// Gets a list of duplicate UPCs matching the ID and passed. Item MUST have SQL ID.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async static Task<List<string>> GetDuplicateUPCs(Item item)
        {
            return await GetDuplicateUPCs(item.UPCs, item.SQLid);
        }

        public async static Task<List<string>> GetDuplicateUPCs(List<string> upcs, int SQLid)
        {
            List<string> duplicateUPCs = new List<string>();

            for (int i = 0; i < upcs.Count; i++)
            {
                string sqlcommand = String.Format("SELECT UPC FROM {0} WHERE id={1} AND UPC='{2}'", TableNames.UPC, SQLid, upcs[i]);
                SqlCommand cmd = new SqlCommand(sqlcommand, connect);

                await Task.Run(() =>
                {
                    try
                    {
                        if (connect.State == ConnectionState.Open) { connect.Close(); }
                        connect.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read() == true)
                        {
                            duplicateUPCs.Add(reader[0].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error in GetDuplicateUPCs:\n" + ex.Message);
                    }
                    finally
                    {
                        connect.Close();
                    }
                });
            }
            return duplicateUPCs;
        }

        public async static Task SaveItemChanges(Item item)
        {
            string itemUpdate = String.Format("UPDATE {0} SET Name = \'{1}\', System = \'{2}\' WHERE id = {3}" , TableNames.ITEMS, CheckForSpecialCharacters(item.name), CheckForSpecialCharacters(item.system), item.SQLid);
            string inventoryUpdate = String.Format("UPDATE {0} SET {1} = {2}, {3} = {4}, {5} = {6}, {7} = {8}, {9} = {10} WHERE id = {11}", TableNames.INVENTORY, InventoryLocationColumnNames.STORE, item.quantity[0], InventoryLocationColumnNames.OUTBACK, item.quantity[1], InventoryLocationColumnNames.STORAGE, item.quantity[2], InventoryLocationColumnNames.WEBSITE, item.quantity[3], InventoryLocationColumnNames.OTHER, item.quantity[4], item.SQLid);
            string priceUpdate = String.Format("UPDATE {0} SET Price = {1}, Cash = {2}, Credit = {3} WHERE id = {4}", TableNames.PRICES, item.price, item.tradeCash, item.tradeCredit, item.SQLid);

            SqlCommand cmd = new SqlCommand(itemUpdate, connect);

            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    cmd.ExecuteNonQuery();
                    connect.Close();
                    cmd = new SqlCommand(inventoryUpdate, connect);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                    connect.Close();
                    cmd = new SqlCommand(priceUpdate, connect);
                    connect.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in SaveItemChanges:\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
        }

        public async static Task SaveCustomerChanges(Customer customer)
        {
            string itemUpdate = String.Format("UPDATE {0} SET {1} = \'{2}\', {3} = \'{4}\', {5} = \'{6}\', {7} = \'{8}\',  WHERE id = {9}", 
                TableNames.CUSTOMERS, 
                SQLTableColumnNames.NAME, CheckForSpecialCharacters(customer.name), 
                SQLTableColumnNames.PHONE, CheckForSpecialCharacters(customer.phoneNumber),
                SQLTableColumnNames.EMAIL, CheckForSpecialCharacters(customer.email),
                SQLTableColumnNames.REWARDS, CheckForSpecialCharacters(customer.rewardsNumber), customer.sqlId);

            // TODO: Add Wishlist!

            SqlCommand cmd = new SqlCommand(itemUpdate, connect);

            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in SaveItemChanges:\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
        }

        /// <summary>
        /// Adds List of UPCs to the UPC table. Does not add duplicates.
        /// </summary>
        /// <param name="upcs"></param>
        /// <param name="SQLid"></param>
        public async static Task AddUPCs(List<string> upcs, int SQLid)
        {
            List<string> duplicates = await GetDuplicateUPCs(upcs, SQLid);
            foreach (string dup in duplicates)
            {
                upcs.Remove(dup);
            }

            if (upcs.Count == 0) // Return if no changes made to upcs
                return;

            SqlCommand cmdUPC = new SqlCommand("INSERT INTO " + TableNames.UPC + " (ID, UPC) VALUES @VALUE", connect);
            cmdUPC.CommandText = cmdUPC.CommandText.Replace("@VALUE", CreateUPCInsertString(upcs, SQLid));
            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    cmdUPC.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in AddUPCs():\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
        }

        public async static void RemoveUPCs(List<string> upcs, int SQLid)
        {
            string upcValues = "(";
            if (upcs.Count == 0) // Return if no changes made to upcs
                return;

            SqlCommand cmdUPC = new SqlCommand("DELETE FROM " + TableNames.UPC + " WHERE UPC in @VALUE AND id = " + SQLid, connect);

            // Format Command String
            foreach (string upc in upcs)
            {
                upcValues += "'" + upc + "',";
            }
            upcValues = upcValues.Remove(upcValues.Length-1); // remove last ","
            upcValues += ")" ;
            cmdUPC.CommandText = cmdUPC.CommandText.Replace("@VALUE", upcValues);

            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    cmdUPC.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in RemoveUPCs():\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
        }

        public async static Task<ObservableCollection<int>> GetQuantities(int ID)
        {
            ObservableCollection<int> quantities = new ObservableCollection<int>();
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + TableNames.INVENTORY + " WHERE id = " + ID, connect);

            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        quantities.Add((int)reader[QuantityColumns.Store]);
                        quantities.Add((int)reader[QuantityColumns.OutBack]);
                        quantities.Add((int)reader[QuantityColumns.Storage]);
                        quantities.Add((int)reader[QuantityColumns.Website]);
                        quantities.Add((int)reader[QuantityColumns.Other]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in GetQuantities():\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
        
            return quantities;
        }

        /// <summary>
        /// Adds passed "amount" to current value in passed quantity "column".
        /// </summary>
        /// <param name="ID">SQL ID of item requiring change.</param>
        /// <param name="amount">Integer to be added.</param>
        /// <param name="column"></param>
        public async static Task IncrementQuantities(int ID, int amount, string column)
        {
            string inventoryUpdate = String.Format("UPDATE {0} SET {1} = CASE WHEN {1} + {2} < 0 THEN 0 ELSE "+ 
                "{1} + {2} END WHERE id = {3}", TableNames.INVENTORY, column, amount, ID);
            SqlCommand cmd = new SqlCommand(inventoryUpdate, connect);

            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in IncrementQuantities():\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
        }

        public async static Task DeleteItem(int ID)
        {
            string deleteCommand = String.Format("DELETE FROM {0} WHERE ID = {1}; DELETE FROM {2} WHERE ID = {1}; DELETE FROM {3} WHERE ID = {1}; DELETE FROM {4} WHERE ID = {1};", TableNames.UPC, ID, TableNames.PRICES, TableNames.INVENTORY, TableNames.ITEMS);
            SqlCommand cmd = new SqlCommand(deleteCommand, connect);

            await Task.Run(() =>
            {
                try
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in DeleteItem():\n" + ex.Message);
                }
                finally
                {
                    connect.Close();
                }
            });
        }

        /// <summary>
        /// Creates an item based on data contained in an SqlDataReader
        /// </summary>
        /// <param name="reader">SqlDataReader containing data</param>
        /// <returns>A new Item</returns>
        public static Item SQLReaderToItem(SqlDataReader reader)
        {
            Item item = new Item();
            item.name = reader[0].ToString(); // Name
            item.system = reader[1].ToString();   // System
            item.price = (decimal)reader[2];   // Price
            item.tradeCash = (decimal)reader[3];   // Cash
            item.tradeCredit = (decimal)reader[4];   // Credit
            item.SQLid = (int)reader[5];   // SQL ID
                     
            return item;
        }

        /// <summary>
        /// Creates an item based on data contained in an SqlDataReader
        /// </summary>
        /// <param name="reader">SqlDataReader containing data</param>
        /// <returns>A new Customer</returns>
        public static Customer SQLReaderToCustomer(SqlDataReader reader)
        {
            Customer customer = new Customer();
            customer.name = reader[0].ToString(); // Name
            customer.phoneNumber = reader[1].ToString();   // PhoneNumber
            customer.email = reader[2].ToString();   // Email
            customer.rewardsNumber = reader[3].ToString();   // Rewards Number
            customer.sqlId = (int)reader[4];   // Id

            return customer;
        }

        public static void CreateCustomerTable()
        {
            string buildTableString = @"CREATE TABLE [dbo].[tblCustomers](
                    [id][int] NOT NULL,

                   [name] [nvarchar](max) NULL,
	                [phone]
                        [nvarchar](50) NULL,
	                [email]
                        [nvarchar](max) NULL,
	                [rewards]
                        [nvarchar](max) NULL,
                 CONSTRAINT[PK_tblCustomers] PRIMARY KEY CLUSTERED
                (
                   [id] ASC
                )WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON[PRIMARY]
                ) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]";

            // Test if table exists
            SqlCommand cmd = new SqlCommand("IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '" + TableNames.CUSTOMERS + 
                "')) BEGIN " + buildTableString + " END", connect);

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                cmd.ExecuteNonQuery();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error in CreateCustomerTable()\n" + ex.Message);
            }
            finally
            {
                connect.Close();
            }

            // Build if table does not exist
        }

#region UPC

        /// <summary>
        /// Gets List of Items that have the passed UPC parameter
        /// </summary>
        /// <param name="inventoryColumn">Table containing Item information</param>
        /// <param name="UPC">The UPC to search for</param>
        /// <returns></returns>
        public async static Task<List<Item>> UPCLookup(string UPC) 
        {
            List<Item> items = new List<Item>();

            SqlCommand cmd = new SqlCommand("SELECT Name, System, Price, Cash, Credit, " + TableNames.ITEMS + ".id FROM " + TableNames.ITEMS +
                    " JOIN " + TableNames.PRICES + " ON " + TableNames.ITEMS + ".id =  " + TableNames.PRICES + ".id " +
                " JOIN " + TableNames.UPC + " on " + TableNames.ITEMS + ".id=" + TableNames.UPC + ".id " +
                "WHERE UPC=\'" + UPC + "\'", connect);

            try
            {
                await Task.Run(() =>
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            items.Add(SQLReaderToItem(reader));
                        }
                        else break;
                    }
                });
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN UPCLookup():\n" + e.Message + e.Data);
            }
            finally
            {
                connect.Close();
            }

            foreach (Item item in items)
            {
                item.quantity = await GetQuantities(item.SQLid);
            }

            return items;
        }

        /// <summary>
        /// Get all UPCs associated with passed item
        /// </summary>
        /// <param name="item">Item to find associated UPCs</param>
        public async static void GetItemUPCs(Item item)
        {
            if (item.SQLid != 0)
                item.UPCs = await GetUPCsWithID(item.SQLid);
            else
                MessageBox.Show("ITEM DOESN'T HAVE A SQL ID ASSOCIATED WITH IT"); // CHANGE THIS !!!!!!!!!!
        }

        /// <summary>
        /// Gets a list of UPCs (as strings) based on the unique sql ID of each item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async static Task<List<string>> GetUPCsWithID(int id)
        {
            List<string> upcs = new List<string>();

            SqlCommand cmd = new SqlCommand("SELECT UPC FROM " + TableNames.UPC +
                " WHERE id =" + id, connect);
            try
            {
                await Task.Run(() =>
                {
                    if (connect.State == ConnectionState.Open) { connect.Close(); }
                    connect.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            upcs.Add(reader[0].ToString());
                        }
                        else break;
                    }
                });
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN GetUPCsWithID():\n" + e.Message + e.Data);
            }
            finally
            {
                connect.Close();
            }

            return upcs;
        }
        
        /// <summary>
        /// Retrieves items from database with matching UPC
        /// </summary>
        /// <param name="tablename">String with the name of the Table to retrieve Item from</param>
        /// <param name="UPC">String containing UPC</param>
        /// <returns>Item containing matching UPC</returns>
        public static Item GetItemWithUPC(string tablename, string UPC)
        {
            Item item = null;

            SqlCommand cmd = new SqlCommand("SELECT * FROM " + tablename + " WHERE (UPC LIKE \'" + UPC + "\')", connect);

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                    item = SQLReaderToItem(reader);
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN GetItemWithUPC:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

            return item;
        }

        public static List<Item> GetCollectionWithUPC(string tablename, string UPC)
        {
            List<Item> collection = new List<Item>();

            SqlCommand cmd = new SqlCommand("SELECT * FROM " + tablename + " WHERE (UPC LIKE \'" + UPC + "\')", connect);

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.HasRows)
                {
                    if (reader.Read())
                    {
                        collection.Add(SQLReaderToItem(reader));
                    }
                    else break;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN GetCollectionWithUPC:\n" + e.Message + e.Data);
            }
            finally
            {
                connect.Close();
            }

            return collection;
        }  

        /// <summary>
        /// Gets the next unused UPC value
        /// </summary>
        /// <returns>A double contining the next unused UPC</returns>
        public static double GetNextUnusedUPC()
        {
            SqlCommand cmd;
            double value = 0;

            cmd = new SqlCommand("SELECT UPC FROM " + TableNames.VARIABLES, connect);

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                value = (double)cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN GetNextUnusedUPC:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

            return value;
        }

        /// <summary>
        /// Increments the value of the next available, unused UPC
        /// </summary>
        public static void IncrementUPC()
        {
            SqlCommand cmd;
            double value = Convert.ToDouble(GetNextUnusedUPC());

            value += 1;

            // Make sure new UPC value is not in use. 
            // If it is in use, keep incrementing until it is not
            while (IsUPCInUse(value.ToString()))
                value += 1;

            cmd = new SqlCommand("UPDATE " + TableNames.VARIABLES + " SET UPC = " + value.ToString(), connect);

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN IncrementUPC:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }
        }

        /// <summary>
        /// Determines whether or not the provided UPC code is currently assigned to an item
        /// </summary>
        /// <param name="upc">UPC code (as string) to check</param>
        /// <returns>True if UPC is in use, False if not</returns>
        public static bool IsUPCInUse(string upc)
        {
            SqlCommand cmd;
            int value = 1;

            cmd = new SqlCommand("IF EXISTS (SELECT * FROM " + TableNames.INVENTORY + " WHERE UPC LIKE \'" + upc + "\')SELECT 1 ELSE SELECT 0", connect);

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                value = (int)cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN IsUPCInUse:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

            if (value == 1) return true;
            else return false;
        }
#endregion

#region Transaction Methods
        /// <summary>
        /// Retrieves the next unused Transaction number from the Variable database
        /// </summary>
        /// <returns>Transaction number as int</returns>
        public static int GetNextUnusedTransactionNumber()
        {
            if (connect.State == ConnectionState.Open) { connect.Close(); }
            SqlCommand cmd;
            int value;

            cmd = new SqlCommand("SELECT TransactionNumber FROM " + TableNames.VARIABLES, connect);

            connect.Open();
            value = (int)cmd.ExecuteScalar();
            connect.Close();

            return value;
        }

        /// <summary>
        /// Increments the value of the next available, unused Transaction number
        /// </summary>
        public static void IncrementTransactionNumber()
        {
            if (connect.State == ConnectionState.Open) { connect.Close(); }
            SqlCommand cmd;

            cmd = new SqlCommand("UPDATE " + TableNames.VARIABLES + " SET TransactionNumber = TransactionNumber + 1", connect);

            connect.Open();
            cmd.ExecuteNonQuery();
            connect.Close();
        }

        public static void AddPayment(Item item, int transactionNumber) // Should only be used for Table of Payment
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO " + TableNames.PAYMENT + " VALUES(@TRANSACTIONNUMBER, @PAYMENTTYPE, @AMOUNT)", connect);
            cmd.Parameters.Add("@TRANSACTIONNUMBER", SqlDbType.Int).Value = transactionNumber;
            cmd.Parameters.Add("@PAYMENTTYPE", SqlDbType.NVarChar).Value = item.name;
            cmd.Parameters.Add("@AMOUNT", SqlDbType.Money).Value = item.price * -1; // Reverse Sign for logging payment           

            // execute command  & close connection
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN AddPayment:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }
        }

        public static List<Transaction> GetPayments(int transactionNumber)
        {
            List<Transaction> payments = new List<Transaction>();

            SqlCommand cmd;
            cmd = new SqlCommand("SELECT * FROM " + TableNames.PAYMENT +
                " WHERE TransactionNumber = " + transactionNumber.ToString(), connect);

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    payments.Add(new Transaction((int)reader[0], reader[1].ToString(), DateTime.Today, (decimal)reader[2]));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN GetPayment():\n" + e.Message + e.StackTrace.ToString());
            }
            finally
            {
                connect.Close();
            }

            return payments;
        }

        public static void DeleteAllTransactionPayments(int transactionNumber)
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM " + TableNames.PAYMENT + " WHERE TransactionNumber = " + transactionNumber.ToString(), connect); 

            // execute command  & close connection
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN DeleteAllTransactionPayments:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }
        }

        public static void AddTransaction(Item item, string type, int transactionNumber, string date)
        {
            SqlCommand cmd = new SqlCommand("INSERT INTO " + TableNames.TRANSACTION + " VALUES(@TRANSACTIONNUMBER, @ID, @NAME, @SYSTEM, @PRICE, @QUANTITY, @CASH, @CREDIT, @TYPE, @DATE)", connect);
            cmd.Parameters.Add("@TRANSACTIONNUMBER", SqlDbType.Int).Value = transactionNumber;
            cmd.Parameters.Add("@ID", SqlDbType.Int).Value = item.SQLid;
            cmd.Parameters.Add("@NAME", SqlDbType.VarChar).Value = item.name;
            cmd.Parameters.Add("@SYSTEM", SqlDbType.NVarChar).Value = item.system;
            cmd.Parameters.Add("@PRICE", SqlDbType.Money).Value = item.price;
            cmd.Parameters.Add("@QUANTITY", SqlDbType.Int).Value = item.quantity[0];
            cmd.Parameters.Add("@CASH", SqlDbType.Money).Value = item.tradeCash;
            cmd.Parameters.Add("@CREDIT", SqlDbType.Money).Value = item.tradeCredit;
            cmd.Parameters.Add("@TYPE", SqlDbType.NVarChar).Value = type;
            cmd.Parameters.Add("@DATE", SqlDbType.DateTime).Value = date;
            
            // execute command  & close connection
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN AddTransaction:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }
        }

        public static List<Transaction> GetTransactions(DateTime startRange, DateTime endRange, int number = 0, string searchText = "")
        {
            List<Transaction> transactions = new List<Transaction>();
            Transaction transaction = null;

            SqlCommand cmd;
            
            // Search for specific transaction
            if (number != 0)
            {
                cmd = new SqlCommand("SELECT * FROM " + TableNames.TRANSACTION + 
                    " WHERE TransactionNumber = " + number.ToString(), connect);
            }
            // Search for specific items
            else if (searchText != "")
            {
                // parameters cannot be null
                // Check for special characters, then divide searchtext into individual terms
                searchText = CheckForSpecialCharacters(searchText);
                SearchTerms searchTerms = new SearchTerms(searchText);
              
                cmd = new SqlCommand("SELECT * FROM " + TableNames.TRANSACTION +
                    " WHERE " + searchTerms.GenerateItemSQLSearchString() +
                    " ORDER BY " + SQLTableColumnNames.TRANSACTIONNUMBER + " DESC " + ", Name", connect);
            }

            else
            {
                cmd = new SqlCommand("SELECT * FROM " + TableNames.TRANSACTION +
                    " WHERE DATE BETWEEN \'" + startRange.ToString("MM/dd/yyyy hh:mm tt") + "\' AND \'" + endRange.ToString("MM/dd/yyyy hh:mm tt") +
                    "\' ORDER BY " + SQLTableColumnNames.TRANSACTIONNUMBER + " DESC", connect);
            }

            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read() == true)
                {
                    // If new transaction number, create a new transaction

                    if (transaction == null)
                    {
                        transaction = new Transaction((int)reader[0], reader[8].ToString(), (DateTime)reader[9]);
                    }
                    if ((int)reader[0] != transaction.transactionNumber )
                    {
                        if (transaction != null)
                        {
                            transactions.Add(transaction); // Store previous
                        }
                        transaction = new Transaction((int)reader[0], reader[8].ToString(), (DateTime)reader[9]); // create new transaction
                    }
                    Item item = SQLReaderToTransaction(reader);
                    if (item != null)
                    {
                        transaction.items.Add(item);
                    }
                }
                if (transaction != null) transactions.Add(transaction);
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN GetTransactions():\n" + e.Message + e.StackTrace.ToString());
            }
            finally
            {
                connect.Close();
            }

            return transactions;
        }

        public static void DeleteTransaction(int transactionNumber)
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM " + TableNames.TRANSACTION + " WHERE TransactionNumber = " + transactionNumber.ToString(), connect);

            // execute command  & close connection
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN DeleteTransaction:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }
        }

        public static Item SQLReaderToTransaction(SqlDataReader reader)
        {
            Item item = null;

            item = new Item();
            item.name = reader[2].ToString(); // Name
            item.system = reader[3].ToString();   // System
            item.price = (decimal)reader[4];   // Price
            int quant;
            if (int.TryParse(reader[5].ToString() ,out quant)) item.quantity[0] = quant; // Quantity
            decimal cash;
            if (decimal.TryParse(reader[6].ToString(), out cash)) item.tradeCash = cash; // Cash
            decimal credit;
            if (decimal.TryParse(reader[7].ToString() ,out credit)) item.tradeCredit = credit;   // Credit
            item.SQLid = (int)reader[1];   // SQL ID

            //0 cmd.Parameters.Add("@TRANSACTIONNUMBER", SqlDbType.Int).Value = transactionNumber;
            //1 cmd.Parameters.Add("@ID", SqlDbType.Int).Value = item.SQLid;
            //2 cmd.Parameters.Add("@NAME", SqlDbType.VarChar).Value = item.name;
            //3 cmd.Parameters.Add("@SYSTEM", SqlDbType.NVarChar).Value = item.system;
            //4 cmd.Parameters.Add("@PRICE", SqlDbType.Money).Value = item.price;
            //5 cmd.Parameters.Add("@QUANTITY", SqlDbType.Int).Value = item.quantity[0];
            //6 cmd.Parameters.Add("@CASH", SqlDbType.Money).Value = item.tradeCash;
            //7 cmd.Parameters.Add("@CREDIT", SqlDbType.Money).Value = item.tradeCredit;
            //8 cmd.Parameters.Add("@TYPE", SqlDbType.NVarChar).Value = type;
            //9 cmd.Parameters.Add("@DATE", SqlDbType.DateTime).Value = date;

            return item;
        }





        //public static List<Item> GetBestSellingItems(string type, bool ascending = false)
        //{
        //    List<Item> collection = new List<Item>();
        //    string order;
        //    if (ascending) order = "ASC";
        //    else order = "DESC";

        //    string command = "SELECT Name, System, SUM(Quantity) AS Total " +
        //                "FROM " + TableNames.TRANSACTION +
        //                " WHERE        (Type = '" + type + "') " +
        //                "GROUP BY Name, System " +
        //                "ORDER BY Total " + order;

        //    SqlCommand cmd = new SqlCommand(command, connect);

        //    try
        //    {
        //        connect.Open();
        //        SqlDataReader reader = cmd.ExecuteReader();

        //        while (reader.Read() == true)
        //        {
        //            Item item = new Item(reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
        //            collection.Add(item);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    finally
        //    {
        //        connect.Close();
        //    }
        //    return collection;
        //}

        ///// <summary>
        ///// Gets the monetary total of the given transaction type starting from (and including) the DateTime "from" up to the DateTime "to"
        ///// </summary>
        ///// <param name="type">Transaction Type</param>
        ///// <param name="from">Starting date. Search includes this date in the results.</param>
        ///// <param name="to">Endind date. Search includes everything UP TO, but NOT INCLUDING this date.</param>
        ///// <returns>The monetary total as a decimal.</returns>
        //public static decimal GetTransactionMonetaryTotal(string type, DateTime from, DateTime to)
        //{
        //    object result = null;
        //    string command = "SELECT SUM(Price * Quantity) FROM " + TableNames.TRANSACTION + " WHERE Type = '" + type + "' AND Date >= '" + from + "' AND Date < '" + to + "'";
        //    SqlCommand cmd = new SqlCommand(command, connect);
                
        //    try
        //    {
        //        connect.Open();
        //        result = cmd.ExecuteScalar();
        //        connect.Close();
        //    }
        //    catch (SqlException ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        connect.Close();
        //    }

        //    try
        //    {
        //        return (Convert.ToDecimal(result));
        //    }
        //    catch
        //    {
        //        return 0M;
        //    }
        //}

        public static void GetDailyTotal(string startDate, string endDate, out decimal cash, out decimal credit, out decimal sales, out decimal salesMinusStoreCredit)
        {
            // SELECT SUM(Price)
            //FROM tblTransactions
            //WHERE (Date between '2019-10-14 00:00:00.000' AND '2019-10-15 00:00:00.000') AND Type = 'Trade-Cash'

            sales = cash = credit = salesMinusStoreCredit = 0m;

            string salesCommand = String.Format("SELECT SUM({0} * {7}) FROM {6} WHERE ({1} between \'{2}\' AND \'{3}\') AND {4} = \'{5}\'", SQLTableColumnNames.PRICE, SQLTableColumnNames.DATE, startDate, endDate, SQLTableColumnNames.TYPE, TransactionTypes.SALE, TableNames.TRANSACTION, SQLTableColumnNames.QUANTITY);
            string cashCommand = String.Format("SELECT SUM({0} * {7}) FROM {6} WHERE ({1} between \'{2}\' AND \'{3}\') AND {4} = \'{5}\'", SQLTableColumnNames.TRADE_CASH, SQLTableColumnNames.DATE, startDate, endDate, SQLTableColumnNames.TYPE, TransactionTypes.TRADE_CASH, TableNames.TRANSACTION, SQLTableColumnNames.QUANTITY);
            string creditCommand = String.Format("SELECT SUM({0} * {7}) FROM {6} WHERE ({1} between \'{2}\' AND \'{3}\') AND {4} = \'{5}\'", SQLTableColumnNames.TRADE_CREDIT, SQLTableColumnNames.DATE, startDate, endDate, SQLTableColumnNames.TYPE, TransactionTypes.TRADE_CREDIT, TableNames.TRANSACTION, SQLTableColumnNames.QUANTITY);
            //string salesMinusStoreCreditCommand = String.Format("SELECT SUM({0}) FROM {1} WHERE ({5} between \'{6}\' AND \'{7}\') AND (({4} = \'{2}\') OR ({4} = \'{3}\'))", "AMOUNT", TableNames.PAYMENT, TransactionTypes.PAYMENT_CASH, TransactionTypes.PAYMENT_CREDITCARD, "PAYMENTTYPE", SQLTableColumnNames.DATE, startDate, endDate);      
            
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                // Sales
                SqlCommand cmd = new SqlCommand(salesCommand, connect);
                connect.Open();
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    sales = Convert.ToDecimal(result);
                }
                connect.Close();

                // Trade-Cash
                cmd = new SqlCommand(cashCommand, connect);
                connect.Open();
                result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    cash = Convert.ToDecimal(result);
                }
                connect.Close();

                // Trade-Credit
                cmd = new SqlCommand(creditCommand, connect);
                connect.Open();
                result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    credit = Convert.ToDecimal(result);
                }
                connect.Close();

                // Sales, only cash or credit card (no store credits/discounts)
                //cmd = new SqlCommand(salesMinusStoreCreditCommand, connect);
                //connect.Open();
                //result = cmd.ExecuteScalar();
                //if (result != DBNull.Value)
                //{
                //    salesMinusStoreCredit = Convert.ToDecimal(result);
                //}
                //connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }
        }
        #endregion
        #region Accounting & Insurance methods
        public static decimal GetValueOfEntireInventory()
        {
            //SELECT tblItems.id, Name, System, SUM((tblInventory.Store + tblInventory.OutBack + tblInventory.Storage + tblInventory.Website + tblInventory.Other) * tblPrices.Price) AS Total
            //FROM tblItems
            //JOIN tblPrices ON tblPrices.id = tblItems.id
            //JOIN tblInventory ON tblInventory.id = tblItems.id
            //GROUP BY tblItems.id, Name, System
            //ORDER BY Total, Name, System asc

            string command = "SELECT SUM((Store + OutBack + Storage + Website + Other) * tblPrices.Price) AS Total" +
            " FROM tblItems" +
            " JOIN tblPrices ON tblPrices.id = tblItems.id" +
            " JOIN tblInventory ON tblInventory.id = tblItems.id";

            decimal value = 0;
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                // Sales
                SqlCommand cmd = new SqlCommand(command, connect);
                connect.Open();
                var result = cmd.ExecuteScalar();
                if (result != DBNull.Value)
                {
                    value = Convert.ToDecimal(result);
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }

            return value;
        }

        #endregion

        #region One-Time Use Methods
        internal static bool CreateWebsiteQuantityColumn()
        {
            string addWebsiteCommand = string.Format("ALTER TABLE {0} ADD {1} INT NOT NULL DEFAULT '0'", TableNames.INVENTORY, InventoryLocationColumnNames.WEBSITE);
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                SqlCommand cmd = new SqlCommand(addWebsiteCommand, connect);
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
                return false;
            }
            return true;
        }

        internal static bool CreateOtherQuantityColumn()
        {
            string addWebsiteCommand = string.Format("ALTER TABLE {0} ADD {1} INT NOT NULL DEFAULT '0'", TableNames.INVENTORY, InventoryLocationColumnNames.OTHER);
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                SqlCommand cmd = new SqlCommand(addWebsiteCommand, connect);
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
                return false;
            }
            return true;
        }

        internal static void MoveInventoryFromStorageToWebsite()
        {
            string addWebsiteCommand = string.Format("UPDATE {0} SET {1} = {2}", TableNames.INVENTORY, InventoryLocationColumnNames.WEBSITE, InventoryLocationColumnNames.STORAGE);
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                SqlCommand cmd = new SqlCommand(addWebsiteCommand, connect);
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }
        }

        internal static void ZeroOutStorageInventory()
        {
            string addWebsiteCommand = string.Format("UPDATE {0} SET {1} = 0", TableNames.INVENTORY, InventoryLocationColumnNames.STORAGE);
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                SqlCommand cmd = new SqlCommand(addWebsiteCommand, connect);
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }
        }

        internal static void ChangeRewardsToStoreCredit()
        {
            string addWebsiteCommand = string.Format("UPDATE {0} SET {1} = '{2}' WHERE {1} = '{3}'", TableNames.PAYMENT, SQLTableColumnNames.PAYMENTTYPE, TransactionTypes.PAYMENT_STORECREDIT, TransactionTypes.PAYMENT_WEBSITE);
            try
            {
                if (connect.State == ConnectionState.Open) { connect.Close(); }
                SqlCommand cmd = new SqlCommand(addWebsiteCommand, connect);
                connect.Open();
                cmd.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }
        }

        #endregion
    }
}
