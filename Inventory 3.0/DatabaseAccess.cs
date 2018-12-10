﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
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




        //static SqlConnection connect = new SqlConnection(Properties.Settings.Default.SQLServerConnectionString);
        static SqlConnection connect = new SqlConnection(Properties.Settings.Default.SQLServerConnectionString2);



        #region Updated Methods (for new SQL table structure)



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
        /// </summary>
        /// <param name="inventoryColumn">Table name of an INVENTORY type table</param>
        /// <param name="sortBy">Column to sort by.   (Optional)</param>
        /// <param name="ascending">True: Results are sorted (A->Z), False: (Z->A).  (Optional)</param>
        /// <param name="searchtext">Text to narrow results to items containing this text.  (Optional)</param>
        /// <returns>A List of Items</returns>
        public static List<Item> SQLTableToList(string inventoryColumn, string sortBy = "System", bool ascending = true, string searchtext = "")
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

            // parameters cannot be null
            if (searchtext == null) searchtext = "";
            if (sortBy == null) sortBy = "System";

            // Check for special characters, then divide searchtext into individual terms
            searchtext = CheckForSpecialCharacters(searchtext);
            searchTerms = new SearchTerms(searchtext);

            if (sortBy != "Name")
            {
                cmd = new SqlCommand("SELECT Name, System, Price, " + inventoryColumn + ", Cash, Credit, " + TableNames.ITEMS + ".id FROM " + TableNames.ITEMS +
                    " JOIN " + TableNames.INVENTORY + " ON " + TableNames.INVENTORY + ".id = " + TableNames.ITEMS + ".id " +
                    "JOIN " + TableNames.PRICES + " ON " + TableNames.INVENTORY + ".id =  " + TableNames.PRICES + ".id " +
                    "WHERE " + searchTerms.GenerateSQLSearchString() +
                    " ORDER BY " + sortBy + " " + order + ", Name;", connect);
            }
            else
            {
                cmd = new SqlCommand("SELECT Name, System, Price, Store, Cash, Credit, " + TableNames.ITEMS + ".id FROM " + TableNames.ITEMS +
                    " JOIN " + TableNames.INVENTORY + " ON " + TableNames.INVENTORY + ".id = " + TableNames.ITEMS + ".id " +
                    "JOIN " + TableNames.PRICES + " ON " + TableNames.INVENTORY + ".id = " + TableNames.PRICES + ".id " +
                    "WHERE " + searchTerms.GenerateSQLSearchString() +
                    " ORDER BY " + sortBy + " " + order + ";", connect);
            }

            try
            {
                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read() == true)
                {
                    item = SQLReaderToItem(reader);
                    if (item != null)
                        collection.Add(item);
                }
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

        public static void AddNewItem(Item item, string tblname)
        {
            AddNewItemToTable(tblname, item.name, item.system, item.price, item.quantity, item.tradeCash, item.tradeCredit, item.UPCs);
        }

        public static void AddNewItemToTable(string tblname, string name, string system, decimal price, List<int> inventory, decimal cash, decimal credit, List<string> upcs)
        {

            name = CheckForSpecialCharacters(name);
            system = CheckForSpecialCharacters(system);

            // Add data to table
            SqlCommand cmdItem = new SqlCommand("INSERT INTO " + TableNames.ITEMS + " VALUES(@NAME, @SYSTEM) OUTPUT INSERTED.ID", connect);
            cmdItem.Parameters.Add("@NAME", SqlDbType.VarChar).Value = name;
            cmdItem.Parameters.Add("@SYSTEM", SqlDbType.VarChar).Value = system;

            SqlCommand cmdPrice = new SqlCommand("INSERT INTO " + TableNames.PRICES + " VALUES(@ID, @PRICE, @CASH, @CREDIT)", connect);
            cmdPrice.Parameters.Add("@PRICE", SqlDbType.Money).Value = price;
            cmdPrice.Parameters.Add("@CASH", SqlDbType.Money).Value = cash;
            cmdPrice.Parameters.Add("@CREDIT", SqlDbType.Money).Value = credit;
             
            //SqlCommand cmdInventory = new SqlCommand("INSERT INTO " + TableNames.INVENTORY + " VALUES(@ID, @QUANTITY)") // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // execute command  & close connection
            try
            {
                connect.Open();
                int ID = (int)cmdItem.ExecuteScalar(); // Get the unique, auto-incremented ID for the item.

                cmdPrice.Parameters.Add("@ID", SqlDbType.Int).Value = ID;
                cmdPrice.ExecuteNonQuery();

                AddUPCs(upcs, ID);
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN AddNewItemToTable:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }
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
                output += "(" + ID + ", " + upc + "),";
            }

            // Remove the last comma
            return output.Remove(output.Length - 1);
        }

        /// <summary>
        /// Returns Items with duplicate Name/System data
        /// </summary>
        /// <param name="item">Item to check for duplicates</param>
        /// <returns>List of duplicate items. If empty, there are no duplicates</returns>
        public List<Item> GetItemDuplicates(Item item)
        {
            List<Item> items = new List<Item>();

            string sqlcommand = String.Format("SELECT Name, System FROM {0} WHERE Name like \'{1}\' AND System like\'{2}\'", TableNames.ITEMS, item.name, item.system); // Might need to select more !!!!!!
            SqlCommand cmd = new SqlCommand(sqlcommand);

            try
            {
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
            
            return items;
        }

        /// <summary>
        /// Gets a list of duplicate UPCs matching the ID and passed. Item MUST have SQL ID.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<string> GetDuplicateUPCs(Item item)
        {
            return GetDuplicateUPCs(item.UPCs, item.SQLid);
        }

        public static List<string> GetDuplicateUPCs(List<string> upcs, int SQLid)
        {
            List<string> duplicateUPCs = new List<string>();

            for (int i = 0; i < upcs.Count; i++)
            {
                string sqlcommand = String.Format("SELECT UPC FROM {0} WHERE id={1} AND UPC={2}", TableNames.UPC, SQLid, upcs[i]);
                SqlCommand cmd = new SqlCommand(sqlcommand, connect);

                try
                {
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
            }
            return duplicateUPCs;
        }

        public static void SaveItemChanges(Item item, string inventoryColumn)
        {
            string itemUpdate = String.Format("UPDATE {0} SET Name = \'{1}\', System = \'{2}\' WHERE id = {3}" , TableNames.ITEMS, CheckForSpecialCharacters(item.name), CheckForSpecialCharacters(item.system), item.SQLid);
            string inventoryUpdate = String.Format("UPDATE {0} SET {1} = {2} WHERE id = {3}", TableNames.INVENTORY, inventoryColumn, item.quantity, item.SQLid);
            string priceUpdate = String.Format("UPDATE {0} SET Price = {1}, Cash = {2}, Credit = {3} WHERE id = {4}", TableNames.PRICES, item.price, item.tradeCash, item.tradeCredit, item.SQLid);

            SqlCommand cmd = new SqlCommand(itemUpdate, connect);
            
            try
            {
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
        }

        /// <summary>
        /// Adds List of UPCs to the UPC table. Does not add duplicates.
        /// </summary>
        /// <param name="upcs"></param>
        /// <param name="SQLid"></param>
        public static void AddUPCs(List<string> upcs, int SQLid)
        {
            List<string> duplicates = GetDuplicateUPCs(upcs, SQLid);
            foreach (string dup in duplicates)
            {
                upcs.Remove(dup);
            }

            if (upcs.Count == 0) // Return if no changes made to upcs
                return;

            SqlCommand cmdUPC = new SqlCommand("INSERT INTO " + TableNames.UPC + " (ID, UPC) VALUES @VALUE", connect);
            cmdUPC.CommandText = cmdUPC.CommandText.Replace("@VALUE", CreateUPCInsertString(upcs, SQLid));

            try
            {
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
        }

        public static void RemoveUPCs(List<string> upcs, int SQLid)
        {
            string upcValues = "(";
            if (upcs.Count == 0) // Return if no changes made to upcs
                return;

            SqlCommand cmdUPC = new SqlCommand("DELETE FROM " + TableNames.UPC + " WHERE UPC in @VALUE AND id = " + SQLid, connect);

            // Format Command String
            foreach (string upc in upcs)
            {
                upcValues += upc + ",";
            }
            upcValues = upcValues.Remove(upcValues.Length-1); // remove last ","
            upcValues += ")" ;
            cmdUPC.CommandText = cmdUPC.CommandText.Replace("@VALUE", upcValues);
            
            try
            {
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
        }

        public static List<int> GetQuantities(int ID)
        {
            
            List<int> quantities = new List<int>();
            SqlCommand cmd = new SqlCommand("SELECT * FROM " + TableNames.INVENTORY + "WHERE id = " + ID, connect);

            try
            {
                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    quantities.Add((int)reader[QuantityColumns.Store]);
                    quantities.Add((int)reader[QuantityColumns.OutBack]);
                    quantities.Add((int)reader[QuantityColumns.Storage]);
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


            return quantities;
        }

        #endregion
















        // Export table to Comma Separated Values file (.csv)
        public static void ExportCSV(string filepath, string tblname)
        {
            string temp = string.Empty;

            SqlCommand cmd = new SqlCommand("SELECT * FROM " + tblname, connect);
            connect.Open();
            SqlDataReader reader = cmd.ExecuteReader();     // set up SQL connection to table

            System.IO.StreamWriter file = new System.IO.StreamWriter(filepath); // prepare file to be written to.

            while (reader.Read() == true)
            {
                temp += reader[0].ToString() + ","; // Name
                temp += reader[1].ToString() + ","; // System
                temp += reader[2].ToString() + ","; // Price
                temp += reader[3].ToString() + ","; // # In stock
                temp += reader[4].ToString() + ","; // Cash Val.
                temp += reader[5].ToString() + ","; // Trade/Credit Val.

                file.WriteLine(temp);
                temp = string.Empty;
            }

            file.Close();
            connect.Close();

        }
        
        // For recording transactions (Transaction Total)
        //public static void AddToTransactionTable(string tblname, Item item, string type, int transactionNumber, string date) // Should only be used for Table of Transactions
        //{
        //    SqlCommand cmd = new SqlCommand("INSERT INTO " + tblname + " VALUES(@NAME, @SYSTEM, @PRICE, @QUANTITY, @UPC, @TYPE, @TRANSACTIONNUMBER, @DATE)", connect);
        //    cmd.Parameters.Add("@NAME", SqlDbType.VarChar).Value = item.name;
        //    cmd.Parameters.Add("@SYSTEM", SqlDbType.NVarChar).Value = item.system;
        //    cmd.Parameters.Add("@PRICE", SqlDbType.Money).Value = item.price;
        //    cmd.Parameters.Add("@QUANTITY", SqlDbType.Int).Value = item.quantity;
        //    cmd.Parameters.Add("@UPC", SqlDbType.VarChar).Value = item.UPC;
        //    cmd.Parameters.Add("@TYPE", SqlDbType.NVarChar).Value = type;
        //    cmd.Parameters.Add("@TRANSACTIONNUMBER", SqlDbType.Int).Value = transactionNumber;
        //    cmd.Parameters.Add("@DATE", SqlDbType.DateTime).Value = date;

        //    // execute command  & close connection
        //    try
        //    {
        //        connect.Open();
        //        cmd.ExecuteNonQuery();
        //        connect.Close();
        //    }
        //    catch (Exception e)
        //    {
        //        MessageBox.Show("ERROR IN AddToTransactionTable:\n" + e.Message);
        //    }
        //    finally
        //    {
        //        connect.Close();
        //    }
        //}

        // Decrease an item's inventory by subtracting the passed "Item.quantity" value
        public static void DecrementInventory(string tablename, Item item)
        {
            SqlCommand cmd = new SqlCommand("UPDATE " + tablename +
                                            " SET Quantity = Quantity - " + item.quantity +
                                            " WHERE Name = '" + CheckForSpecialCharacters(item.name) +
                                            "' AND System = '" + CheckForSpecialCharacters(item.system) + "' " +
                                            "AND PRICE = '" + item.price + "' " +
                                            "AND TRADECASH = '" + item.tradeCash + "' " +
                                            "AND TRADECREDIT = '" + item.tradeCredit + "' " +
                                            //"AND UPC = '" + item.UPC + "'",
                                            connect); // Find an exact match for the passed string, increase inventory

            try
            {
                connect.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN DecrementInventory:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

        }

        // Increase an item's inventory by adding the passed "Item.quantity" value
        public static void IncrementInventory(string tablename, Item item)
        {
            SqlCommand cmd = new SqlCommand("UPDATE " + tablename +
                                            " SET Quantity = Quantity + " + item.quantity +
                                            " WHERE Name = '" + CheckForSpecialCharacters(item.name) +
                                            "' AND System = '" + CheckForSpecialCharacters(item.system) + "' " + 
                                            "AND PRICE = '" + item.price + "' " +
                                            "AND TRADECASH = '" + item.tradeCash + "' " +
                                            "AND TRADECREDIT = '" + item.tradeCredit + "' " +
                                            //"AND UPC = '" + item.UPC + "'",
                                            connect); // Find an exact match for the passed string, increase inventory

            try
            {
                connect.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN IncrementInventory:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

        }

        // Change an existing item's information
        public static void EditInventory(string tablename, Item oldItem, Item newItem)
        {
            SqlCommand cmd = new SqlCommand("UPDATE " + tablename +
                                            " SET Name = '" + CheckForSpecialCharacters(newItem.name) + "', " +
                                            "System = '" + CheckForSpecialCharacters(newItem.system) + "', " +
                                            "Price = " + newItem.price + ", " +
                                            "Quantity = " + newItem.quantity + ", " +
                                            "TradeCash = " + newItem.tradeCash + ", " +
                                            "TradeCredit = " + newItem.tradeCredit + ", " +
                                           // "UPC = '" + newItem.UPC +
                                            "' WHERE Name = '" + CheckForSpecialCharacters(oldItem.name) +
                                            "' AND System = '" + CheckForSpecialCharacters(oldItem.system) + "' " + 
                                            "AND PRICE = '" + oldItem.price + "' " +
                                            "AND TRADECASH = '" + oldItem.tradeCash + "' " +
                                            "AND TRADECREDIT = '" + oldItem.tradeCredit + "' " +
                                           // "AND UPC = '" + oldItem.UPC + "'"
                                            connect); // Find an exact match for the passed string, increase inventory

            try
            {
                connect.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN EditInventory:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

        }

        // Remove an item from a SQL table based on item's Name and System
        public static void DeleteTableItem(string tablename, Item item)
        {
            SqlCommand cmd = new SqlCommand("DELETE FROM " + tablename +
                                            " WHERE Name = '" + CheckForSpecialCharacters(item.name) +
                                            "' AND System = '" + CheckForSpecialCharacters(item.system) + "' " +
                                            "AND PRICE = '" + item.price + "' " +
                                            "AND TRADECASH = '" + item.tradeCash + "' " +
                                            "AND TRADECREDIT = '" + item.tradeCredit + "' "                                   
                                            , connect); // Find an exact match for the passed string, increase inventory

            try
            {
                connect.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN DeleteTableItem:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

        }
        
        /// <summary>
        /// Checks if an Item is already present in an inventory table
        /// </summary>
        /// <param name="tablename">Name of an Inventory table</param>
        /// <param name="item">Item to check</param>
        /// <returns>True if the item is present, false otherwise</returns>
        public static bool IsItemInTable(string tablename, Item item)
        {
            object result = null;
            string command = "SELECT * FROM " + tablename +
                " WHERE NAME = '" + CheckForSpecialCharacters(item.name) + "' " +
                "AND SYSTEM = '" + CheckForSpecialCharacters(item.system) + "' " +
                "AND PRICE = '" + item.price + "' " +
                "AND TRADECASH = '" + item.tradeCash + "' " +
                "AND TRADECREDIT = '" + item.tradeCredit + "' ";
            SqlCommand cmd = new SqlCommand(command, connect);

            // Execute command
            try
            {
                connect.Open();
                result = cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN IsItemInTable:\n" + e.Message);
            }
            finally
            {
                connect.Close();
            }

            // If the result is still null, then the item does not exist in the table
            if (result == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Creates an item based on data contained in an SqlDataReader
        /// </summary>
        /// <param name="reader">SqlDataReader containing data</param>
        /// <returns>A new Item</returns>
        public static Item SQLReaderToItem(SqlDataReader reader)
        {
            Item item = null;

            // FOR OLD TABLE:            
            //item = new Item(reader[0].ToString(),
            //        reader[1].ToString(),
            //        reader[2].ToString(),
            //        reader[3].ToString(),
            //        reader[4].ToString(),
            //        reader[5].ToString(),
            //        reader[6].ToString());
           
            // FOR NEW TABLE
            item = new Item(reader[0].ToString(), // Name
                    reader[1].ToString(),   // System
                    reader[2].ToString(),   // Price
                    new List<int>{(int)reader[3]},   // Quantity
                    reader[4].ToString(),   // Cash
                    reader[5].ToString(),   // Credit
                    reader[6].ToString());   // SQL ID
                     
            
            return item;
        }

        #region UPC

        /// <summary>
        /// Gets List of Items that have the passed UPC parameter
        /// </summary>
        /// <param name="inventoryColumn">Table containing Item information</param>
        /// <param name="UPC">The UPC to search for</param>
        /// <returns></returns>
        public static List<Item> UPCLookup(string inventoryColumn, string UPC) 
        {
            List<Item> items = new List<Item>();

            SqlCommand cmd = new SqlCommand("SELECT Name, System, Price, " + inventoryColumn + ", Cash, Credit, " + TableNames.ITEMS + ".id FROM " + TableNames.ITEMS +
                    " JOIN " + TableNames.INVENTORY + " ON " + TableNames.INVENTORY + ".id = " + TableNames.ITEMS + ".id " +
                    "JOIN " + TableNames.PRICES + " ON " + TableNames.INVENTORY + ".id =  " + TableNames.PRICES + ".id " +
                " JOIN " + TableNames.UPC + " on " + TableNames.ITEMS + ".id=" + TableNames.UPC + ".id " +
                "WHERE UPC=\'" + UPC + "\'", connect);

            try
            {
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
            }
            catch (Exception e)
            {
                MessageBox.Show("ERROR IN UPCLookup():\n" + e.Message + e.Data);
            }
            finally
            {
                connect.Close();
            }

            return items;
        }

        /// <summary>
        /// Get all UPCs associated with passed item
        /// </summary>
        /// <param name="item">Item to find associated UPCs</param>
        public static void GetItemUPCs(Item item)
        {
            if (item.SQLid != 0)
                item.UPCs = GetUPCsWithID(item.SQLid);
            else
                MessageBox.Show("ITEM DOESN'T HAVE A SQL ID ASSOCIATED WITH IT"); // CHANGE THIS !!!!!!!!!!
        }

        /// <summary>
        /// Gets a list of UPCs (as strings) based on the unique sql ID of each item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<string> GetUPCsWithID(int id)
        {
            List<string> upcs = new List<string>();

            SqlCommand cmd = new SqlCommand("SELECT UPC FROM " + TableNames.UPC +
                " WHERE id =" + id, connect);
            try
            {
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
            SqlCommand cmd;

            cmd = new SqlCommand("UPDATE " + TableNames.VARIABLES + " SET TransactionNumber = TransactionNumber + 1", connect);

            connect.Open();
            cmd.ExecuteNonQuery();
            connect.Close();
        }

       

        /// <summary>
        /// Returns the monetary value of the entire inventory (each item's price * quantity)
        /// </summary>
        /// <returns>Monetary value of all in-stock items in inventory as a Decimal</returns>
        public static decimal GetInventoryValue()
        {
            decimal total;

            string command = "SELECT SUM(Price * Quantity) FROM " + TableNames.INVENTORY ;

            try
            {
                SqlCommand cmd = new SqlCommand(command, connect);
                connect.Open();
                total = Convert.ToDecimal(cmd.ExecuteScalar());
                connect.Close();
                return total; //return the total
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }

            return -1M; // Something went wrong. (Although, -1 could be a legitimate return value if there are an abundance of negative quantity of items in inventory.)
        }

        public static List<Item> GetBestSellingItems(string type, bool ascending = false)
        {
            List<Item> collection = new List<Item>();
            string order;
            if (ascending) order = "ASC";
            else order = "DESC";

            string command = "SELECT Name, System, SUM(Quantity) AS Total " +
                        "FROM " + TableNames.TRANSACTION +
                        " WHERE        (Type = '" + type + "') " +
                        "GROUP BY Name, System " +
                        "ORDER BY Total " + order;

            SqlCommand cmd = new SqlCommand(command, connect);

            try
            {
                connect.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read() == true)
                {
                    Item item = new Item(reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
                    collection.Add(item);
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }

            return collection;
        }

        /// <summary>
        /// Gets the monetary total of the given transaction type starting from (and including) the DateTime "from" up to the DateTime "to"
        /// </summary>
        /// <param name="type">Transaction Type</param>
        /// <param name="from">Starting date. Search includes this date in the results.</param>
        /// <param name="to">Endind date. Search includes everything UP TO, but NOT INCLUDING this date.</param>
        /// <returns>The monetary total as a decimal.</returns>
        public static decimal GetTransactionMonetaryTotal(string type, DateTime from, DateTime to)
        {
            object result = null;
            string command = "SELECT SUM(Price * Quantity) FROM " + TableNames.TRANSACTION + " WHERE Type = '" + type + "' AND Date >= '" + from + "' AND Date < '" + to + "'";
            SqlCommand cmd = new SqlCommand(command, connect);
                
            try
            {
                connect.Open();
                result = cmd.ExecuteScalar();
                connect.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }

            try
            {
                return (Convert.ToDecimal(result));
            }
            catch
            {
                return 0M;
            }
        }

        #endregion
        /// <summary>
        /// Returns the total quantity of items currently in stock
        /// </summary>
        /// <returns>An int representing the total quantity of items currently in stock</returns>
        public static int GetItemTotal()
        {
            object result = null;
            string command = "SELECT SUM(Quantity) FROM " + TableNames.INVENTORY;
            SqlCommand cmd = new SqlCommand(command, connect);

            try
            {
                connect.Open();
                result = cmd.ExecuteScalar();
                connect.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                connect.Close();
            }

            try
            {
                return Convert.ToInt32(result);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Determines whether or not the passed item is currently in stock, based on UPC and item Name
        /// </summary>
        /// <param name="item">Item to check</param>
        /// <returns>True if item is currently in stock, false otherwise</returns>
        public static bool IsItemInStock(Item item)
        {
            object result = null; // Result of SQL query
            string command;
                        
            command = "SELECT QUANTITY FROM " + TableNames.INVENTORY + " WHERE NAME = '" + item.name + "' AND SYSTEM = '" + item.system + "'";
            
            SqlCommand cmd = new SqlCommand(command, connect);

            try
            {
                connect.Open();
                result = cmd.ExecuteScalar();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in IsItemInStock():\n\n" + ex.Message);
                connect.Close();
            }

            // If item is in stock, return true. Else, return false
            if ((int)result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



    }
}
