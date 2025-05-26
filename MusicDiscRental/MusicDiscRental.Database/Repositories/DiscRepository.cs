using MusicDiscRental.Database.DBConnection;
using MusicDiscRental.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MusicDiscRental.Database.Repositories
{
    public class DiscRepository : IRepository<Disc>
    {
        private readonly IDatabaseService _dbService;

        public DiscRepository(IDatabaseService dbService)
        {
            _dbService = dbService ?? MockDatabaseService.Instance;
        }

        public async Task<IEnumerable<Disc>> GetAllAsync()
        {
            var result = new List<Disc>();
            var dataTable = await _dbService.ExecuteQueryAsync("SELECT * FROM Discs");

            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(CreateDiscFromRow(row));
            }

            return result;
        }

        public async Task<Disc> GetByIdAsync(int id)
        {
            var parameters = new Dictionary<string, object> { { "disc_id", id } };
            var dataTable = await _dbService.ExecuteQueryAsync(
                "SELECT * FROM Discs WHERE disc_id = :disc_id", parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            return CreateDiscFromRow(dataTable.Rows[0]);
        }

        public async Task<bool> AddAsync(Disc disc)
        {
            var parameters = new Dictionary<string, object>
            {
                { "title", disc.Title },
                { "artist", disc.Artist },
                { "genre", disc.Genre },
                { "total_stock", disc.TotalStock },
                { "available_stock", disc.AvailableStock }
            };

            return await _dbService.ExecuteNonQueryAsync(
                "INSERT INTO Discs (disc_id, title, artist, genre, total_stock, available_stock) " +
                "VALUES (disc_seq.NEXTVAL, :title, :artist, :genre, :total_stock, :available_stock)",
                parameters);
        }

        public async Task<bool> UpdateAsync(Disc disc)
        {
            var parameters = new Dictionary<string, object>
            {
                { "disc_id", disc.DiscId },
                { "title", disc.Title },
                { "artist", disc.Artist },
                { "genre", disc.Genre },
                { "total_stock", disc.TotalStock },
                { "available_stock", disc.AvailableStock }
            };

            return await _dbService.ExecuteNonQueryAsync(
                "UPDATE Discs SET title = :title, artist = :artist, " +
                "genre = :genre, total_stock = :total_stock, available_stock = :available_stock " +
                "WHERE disc_id = :disc_id",
                parameters);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // First check for active rentals
                var checkActiveParameters = new Dictionary<string, object> { { "disc_id", id } };
                var checkActiveResult = await _dbService.ExecuteQueryAsync(
                    "SELECT COUNT(*) FROM Rentals WHERE disc_id = :disc_id AND return_date IS NULL",
                    checkActiveParameters);

                if (checkActiveResult.Rows.Count > 0 && Convert.ToInt32(checkActiveResult.Rows[0][0]) > 0)
                {
                    throw new InvalidOperationException("Cannot delete disc because it is currently rented out.");
                }

                // Delete rental history first
                var deleteHistoryParameters = new Dictionary<string, object> { { "disc_id", id } };
                await _dbService.ExecuteNonQueryAsync(
                    "DELETE FROM Rentals WHERE disc_id = :disc_id AND return_date IS NOT NULL",
                    deleteHistoryParameters);

                // Then delete the disc
                var deleteDiscParameters = new Dictionary<string, object> { { "disc_id", id } };
                return await _dbService.ExecuteNonQueryAsync(
                    "DELETE FROM Discs WHERE disc_id = :disc_id",
                    deleteDiscParameters);
            }
            catch (Exception ex)
            {
                if (ex is InvalidOperationException)
                    throw; // Rethrow business rule violations

                System.Diagnostics.Debug.WriteLine($"Error deleting disc: {ex.Message}");
                return false;
            }
        }

        // Helper method to create a Disc object from a DataRow
        private Disc CreateDiscFromRow(DataRow row)
        {
            if (row.Table.Columns.Contains("Error"))
            {
                throw new Exception(row["Error"].ToString());
            }

            return new Disc
            {
                DiscId = Convert.ToInt32(row["disc_id"]),
                Title = row["title"].ToString(),
                Artist = row["artist"].ToString(),
                Genre = row["genre"] != DBNull.Value ? row["genre"].ToString() : null,
                TotalStock = Convert.ToInt32(row["total_stock"]),
                AvailableStock = Convert.ToInt32(row["available_stock"])
            };
        }
    }
}