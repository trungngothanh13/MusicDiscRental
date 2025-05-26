using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MusicDiscRental.Database.DBConnection
{
    public interface IDatabaseService
    {
        Task<bool> ExecuteNonQueryAsync(string procedureName, Dictionary<string, object> parameters = null);
        Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null);
        Task<T> ExecuteFunctionAsync<T>(string functionName, Dictionary<string, object> parameters = null);
        bool TestConnection();
    }
}