using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MusicDiscRental.Database.DBConnection
{
    public class MockDatabaseService : IDatabaseService
    {
        private static MockDatabaseService _instance;
        private static readonly object _lock = new object();

        private MockDatabaseService() { }

        public static MockDatabaseService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new MockDatabaseService();
                        }
                    }
                }
                return _instance;
            }
        }

        public async Task<bool> ExecuteNonQueryAsync(string procedureName, Dictionary<string, object> parameters = null)
        {
            // For development - simulate success
            await Task.Delay(100); // Simulate network delay
            Console.WriteLine($"Executed procedure: {procedureName}");
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    Console.WriteLine($"  Parameter: {param.Key} = {param.Value}");
                }
            }
            return true;
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            // Create a mock DataTable based on the query
            var result = new DataTable();
            await Task.Delay(100); // Simulate network delay

            Console.WriteLine($"Executing query: {query}");

            // Example: If query contains "Discs", return a mock discs table
            if (query.Contains("Disc", StringComparison.OrdinalIgnoreCase) &&
                !query.Contains("JOIN", StringComparison.OrdinalIgnoreCase))
            {
                result.Columns.Add("disc_id", typeof(int));
                result.Columns.Add("title", typeof(string));
                result.Columns.Add("artist", typeof(string));
                result.Columns.Add("genre", typeof(string));
                result.Columns.Add("total_stock", typeof(int));
                result.Columns.Add("available_stock", typeof(int));

                // Add sample data
                result.Rows.Add(1, "Thriller", "Michael Jackson", "Pop", 3, 3);
                result.Rows.Add(2, "Back in Black", "AC/DC", "Rock", 2, 2);
                result.Rows.Add(3, "Kind of Blue", "Miles Davis", "Jazz", 1, 1);
            }
            // Example: If query contains "Members", return a mock members table
            else if (query.Contains("Member", StringComparison.OrdinalIgnoreCase) &&
                     !query.Contains("JOIN", StringComparison.OrdinalIgnoreCase))
            {
                result.Columns.Add("member_id", typeof(int));
                result.Columns.Add("name", typeof(string));
                result.Columns.Add("join_date", typeof(DateTime));

                // Add sample data
                result.Rows.Add(1, "John Doe", DateTime.Now.AddDays(-30));
                result.Rows.Add(2, "Jane Smith", DateTime.Now.AddDays(-15));
            }
            // Handle the rental join query specifically (this is the one causing issues)
            else if (query.Contains("Rental", StringComparison.OrdinalIgnoreCase) &&
                     query.Contains("JOIN", StringComparison.OrdinalIgnoreCase))
            {
                // These column names must exactly match what's being selected in the query
                result.Columns.Add("rental_id", typeof(int));
                result.Columns.Add("disc_id", typeof(int));
                result.Columns.Add("member_id", typeof(int));
                result.Columns.Add("rental_date", typeof(DateTime));
                result.Columns.Add("return_date", typeof(DateTime));
                result.Columns.Add("disc_title", typeof(string));
                result.Columns.Add("artist", typeof(string));
                result.Columns.Add("member_name", typeof(string));

                // Add sample data for the joined query
                result.Rows.Add(1, 1, 1, DateTime.Now.AddDays(-5), DBNull.Value, "Thriller", "Michael Jackson", "John Doe");
                result.Rows.Add(2, 2, 1, DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1), "Back in Black", "AC/DC", "John Doe");
                result.Rows.Add(3, 3, 2, DateTime.Now.AddDays(-2), DBNull.Value, "Kind of Blue", "Miles Davis", "Jane Smith");
            }
            // Handle basic rentals query
            else if (query.Contains("Rental", StringComparison.OrdinalIgnoreCase))
            {
                result.Columns.Add("rental_id", typeof(int));
                result.Columns.Add("disc_id", typeof(int));
                result.Columns.Add("member_id", typeof(int));
                result.Columns.Add("rental_date", typeof(DateTime));
                result.Columns.Add("return_date", typeof(DateTime));

                // Add sample data
                result.Rows.Add(1, 1, 1, DateTime.Now.AddDays(-5), DBNull.Value);
                result.Rows.Add(2, 2, 1, DateTime.Now.AddDays(-3), DateTime.Now.AddDays(-1));
                result.Rows.Add(3, 3, 2, DateTime.Now.AddDays(-2), DBNull.Value);
            }

            return result;
        }

        public async Task<T> ExecuteFunctionAsync<T>(string functionName, Dictionary<string, object> parameters = null)
        {
            await Task.Delay(100); // Simulate network delay

            // Example: GetActiveRentals function returns number of rentals
            if (functionName.Contains("GetActiveRentals", StringComparison.OrdinalIgnoreCase))
            {
                int memberId = 1; // Default
                if (parameters != null && parameters.ContainsKey("p_member_id"))
                {
                    memberId = Convert.ToInt32(parameters["p_member_id"]);
                }

                // Mock: Return 1 active rental for member 1, 2 for member 2
                return (T)Convert.ChangeType(memberId == 1 ? 1 : 2, typeof(T));
            }

            return default;
        }

        public bool TestConnection()
        {
            // Always return success for mock service
            return true;
        }
    }
}