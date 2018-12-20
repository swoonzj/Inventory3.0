using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Inventory_3._0
{
    /// <summary>
    /// Contains search terms
    /// </summary>
    public class SearchTerms
    {
        public List<string> terms = new List<string>();

        /// <summary>
        /// Divides a single search string into individual terms (separated by spaces), and stores them in the array "terms"
        /// </summary>
        /// <param name="searchText">Single string</param>
        public SearchTerms(string searchText)
        {
            // Split up input by spaces
            foreach (string term in searchText.Split(' '))
            {
                // Remove terms composed entirely of spaces
                string temp = term.Replace(" ", "");
                if (temp != "")
                    terms.Add(temp);
            }
        }

        public string GenerateSQLSearchString()
        {
            string output = "";

            // Case: list of terms is empty
            if (terms.Count == 0)
            {
                return "(Name LIKE \'%%\' OR System LIKE \'%%\') ";
            }

            for (int i = 0; i < terms.Count; i++)
            {
                string term = terms[i];
                output += "(Name LIKE \'%" + term + "%\' OR System LIKE \'%" + term + "%\') ";

                // Add an "AND" in between conditions
                // Do not add one if it is the last search term
                if (i != terms.Count - 1)
                {
                    output += "AND ";
                }
            }
            return output;
        }
    }
}
