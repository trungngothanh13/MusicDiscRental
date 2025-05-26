using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace MusicDiscRental.Database.DBConnection
{
    public class OracleDatabaseService : IDatabaseService
    {
        private string ConnectionString =
            ConfigurationManager.ConnectionStrings["MusicDiscRentalConnection"]?.ConnectionString
            ?? "User Id=C##MUSICADMIN;Password=music_password;Data Source=localhost:1521/orcl;";

        public void SetConnectionString(string newConnectionString)
        {
            ConnectionString = newConnectionString;
        }

        #region Query Methods

        // Standard SQL query method for general use
        public async Task<DataTable> ExecuteQueryAsync(string query, Dictionary<string, object> parameters = null)
        {
            var result = new DataTable();

            try
            {
                using (var connection = new OracleConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    // For simple queries without parameters, use direct approach
                    if (parameters == null || parameters.Count == 0)
                    {
                        using (var command = new OracleCommand(query, connection))
                        {
                            using (var adapter = new OracleDataAdapter(command))
                            {
                                adapter.Fill(result);
                            }
                        }
                    }
                    // For parameterized queries, use direct SQL for consistency
                    else
                    {
                        string processedQuery = query;

                        // Replace parameters with values in SQL
                        foreach (var param in parameters)
                        {
                            string paramName = ":" + param.Key.TrimStart(':');

                            if (param.Value == null)
                            {
                                processedQuery = processedQuery.Replace(paramName, "NULL");
                            }
                            else if (param.Value is string strValue)
                            {
                                processedQuery = processedQuery.Replace(paramName, $"'{EscapeSql(strValue)}'");
                            }
                            else if (param.Value is DateTime dateValue)
                            {
                                processedQuery = processedQuery.Replace(paramName, $"TO_DATE('{dateValue:yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS')");
                            }
                            else
                            {
                                processedQuery = processedQuery.Replace(paramName, param.Value.ToString());
                            }
                        }

                        using (var command = new OracleCommand(processedQuery, connection))
                        {
                            using (var adapter = new OracleDataAdapter(command))
                            {
                                adapter.Fill(result);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing query: {ex.Message}");

                // Add error information to result
                result = new DataTable();
                result.Columns.Add("Error", typeof(string));
                result.Rows.Add($"Database error: {ex.Message}");
            }

            return result;
        }

        // Method for executing non-query SQL statements (INSERT, UPDATE, DELETE)
        public async Task<bool> ExecuteNonQueryAsync(string commandText, Dictionary<string, object> parameters = null)
        {
            try
            {
                // For stored procedures, use standard parameter binding
                if (IsStoredProcedure(commandText))
                {
                    return await ExecuteStoredProcedureAsync(commandText, parameters);
                }

                // For SQL statements, use direct SQL building for consistency
                using (var connection = new OracleConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    string processedSql = commandText;

                    // If parameters exist, replace them in the SQL
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (var param in parameters)
                        {
                            string paramName = ":" + param.Key.TrimStart(':');

                            if (param.Value == null)
                            {
                                processedSql = processedSql.Replace(paramName, "NULL");
                            }
                            else if (param.Value is string strValue)
                            {
                                processedSql = processedSql.Replace(paramName, $"'{EscapeSql(strValue)}'");
                            }
                            else if (param.Value is DateTime dateValue)
                            {
                                processedSql = processedSql.Replace(paramName, $"TO_DATE('{dateValue:yyyy-MM-dd HH:mm:ss}', 'YYYY-MM-DD HH24:MI:SS')");
                            }
                            else
                            {
                                processedSql = processedSql.Replace(paramName, param.Value.ToString());
                            }
                        }
                    }

                    // Log the SQL for debugging
                    System.Diagnostics.Debug.WriteLine($"Executing SQL: {processedSql}");

                    using (var command = new OracleCommand(processedSql, connection))
                    {
                        int result = await command.ExecuteNonQueryAsync();
                        return result >= 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing non-query: {ex.Message}");
                return false;
            }
        }

        // Execute stored procedures with proper parameter binding
        private async Task<bool> ExecuteStoredProcedureAsync(string procName, Dictionary<string, object> parameters)
        {
            try
            {
                using (var connection = new OracleConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new OracleCommand(procName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                // Create parameter with specific OracleDbType
                                OracleParameter oracleParam;

                                if (param.Value is int intValue)
                                {
                                    oracleParam = new OracleParameter(param.Key, OracleDbType.Int32) { Value = intValue };
                                }
                                else if (param.Value is string strValue)
                                {
                                    oracleParam = new OracleParameter(param.Key, OracleDbType.Varchar2)
                                    {
                                        Value = string.IsNullOrEmpty(strValue) ? DBNull.Value : (object)strValue
                                    };
                                }
                                else if (param.Value is DateTime dateValue)
                                {
                                    oracleParam = new OracleParameter(param.Key, OracleDbType.Date) { Value = dateValue };
                                }
                                else if (param.Value == null)
                                {
                                    oracleParam = new OracleParameter(param.Key, OracleDbType.Varchar2) { Value = DBNull.Value };
                                }
                                else
                                {
                                    oracleParam = new OracleParameter(param.Key, param.Value);
                                }

                                command.Parameters.Add(oracleParam);
                            }
                        }

                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing stored procedure: {ex.Message}");
                return false;
            }
        }

        // Method for executing functions that return a value
        public async Task<T> ExecuteFunctionAsync<T>(string functionName, Dictionary<string, object> parameters = null)
        {
            try
            {
                using (var connection = new OracleConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new OracleCommand(functionName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add return parameter
                        var returnParameter = new OracleParameter
                        {
                            ParameterName = "returnValue",
                            Direction = ParameterDirection.ReturnValue
                        };
                        command.Parameters.Add(returnParameter);

                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                // Create parameter with specific OracleDbType
                                OracleParameter oracleParam;

                                if (param.Value is int intValue)
                                {
                                    oracleParam = new OracleParameter(param.Key, OracleDbType.Int32) { Value = intValue };
                                }
                                else if (param.Value is string strValue)
                                {
                                    oracleParam = new OracleParameter(param.Key, OracleDbType.Varchar2)
                                    {
                                        Value = string.IsNullOrEmpty(strValue) ? DBNull.Value : (object)strValue
                                    };
                                }
                                else if (param.Value == null)
                                {
                                    oracleParam = new OracleParameter(param.Key, OracleDbType.Varchar2) { Value = DBNull.Value };
                                }
                                else
                                {
                                    oracleParam = new OracleParameter(param.Key, param.Value);
                                }

                                command.Parameters.Add(oracleParam);
                            }
                        }

                        await command.ExecuteNonQueryAsync();

                        return (T)Convert.ChangeType(returnParameter.Value, typeof(T));
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error executing function: {ex.Message}");
                return default;
            }
        }

        #endregion

        #region Helper Methods

        // Check if a command is a stored procedure or SQL statement
        private bool IsStoredProcedure(string commandText)
        {
            return !commandText.ToUpper().StartsWith("INSERT") &&
                   !commandText.ToUpper().StartsWith("UPDATE") &&
                   !commandText.ToUpper().StartsWith("DELETE") &&
                   !commandText.ToUpper().StartsWith("SELECT");
        }

        // Escape single quotes in SQL strings
        private string EscapeSql(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";

            // Escape single quotes by doubling them
            return input.Replace("'", "''");
        }

        #endregion

        // Test database connection
        public bool TestConnection()
        {
            try
            {
                using (var connection = new OracleConnection(ConnectionString))
                {
                    connection.Open();
                    return connection.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Connection test failed: {ex.Message}");
                return false;
            }
        }
    }
}