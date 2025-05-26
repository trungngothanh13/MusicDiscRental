using MusicDiscRental.Database.DBConnection;
using MusicDiscRental.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MusicDiscRental.Database.Repositories
{
    public class RentalRepository : IRepository<Rental>
    {
        private readonly IDatabaseService _dbService;

        public RentalRepository(IDatabaseService dbService)
        {
            _dbService = dbService ?? MockDatabaseService.Instance;
        }

        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            var result = new List<Rental>();
            var dataTable = await _dbService.ExecuteQueryAsync(
                "SELECT r.rental_id, r.disc_id, r.member_id, r.rental_date, r.return_date, " +
                "d.title as disc_title, d.artist, m.name as member_name " +
                "FROM Rentals r " +
                "JOIN Discs d ON r.disc_id = d.disc_id " +
                "JOIN Members m ON r.member_id = m.member_id " +
                "ORDER BY r.rental_date DESC");

            foreach (DataRow row in dataTable.Rows)
            {
                if (row.Table.Columns.Contains("Error"))
                {
                    throw new Exception(row["Error"].ToString());
                }

                result.Add(CreateRentalFromRow(row));
            }

            return result;
        }

        public async Task<IEnumerable<Rental>> GetActiveRentalsAsync()
        {
            var result = new List<Rental>();
            var dataTable = await _dbService.ExecuteQueryAsync(
                "SELECT r.rental_id, r.disc_id, r.member_id, r.rental_date, r.return_date, " +
                "d.title as disc_title, d.artist, m.name as member_name " +
                "FROM Rentals r " +
                "JOIN Discs d ON r.disc_id = d.disc_id " +
                "JOIN Members m ON r.member_id = m.member_id " +
                "WHERE r.return_date IS NULL " +
                "ORDER BY r.rental_date DESC");

            foreach (DataRow row in dataTable.Rows)
            {
                if (row.Table.Columns.Contains("Error"))
                {
                    throw new Exception(row["Error"].ToString());
                }

                result.Add(CreateRentalFromRow(row));
            }

            return result;
        }

        public async Task<IEnumerable<Rental>> GetRentalsByMemberAsync(int memberId)
        {
            var result = new List<Rental>();
            var parameters = new Dictionary<string, object> { { "member_id", memberId } };

            var dataTable = await _dbService.ExecuteQueryAsync(
                "SELECT r.rental_id, r.disc_id, r.member_id, r.rental_date, r.return_date, " +
                "d.title as disc_title, d.artist, m.name as member_name " +
                "FROM Rentals r " +
                "JOIN Discs d ON r.disc_id = d.disc_id " +
                "JOIN Members m ON r.member_id = m.member_id " +
                "WHERE r.member_id = :member_id " +
                "ORDER BY r.rental_date DESC",
                parameters);

            foreach (DataRow row in dataTable.Rows)
            {
                if (row.Table.Columns.Contains("Error"))
                {
                    throw new Exception(row["Error"].ToString());
                }

                result.Add(CreateRentalFromRow(row));
            }

            return result;
        }

        public async Task<Rental> GetByIdAsync(int id)
        {
            var parameters = new Dictionary<string, object> { { "rental_id", id } };
            var dataTable = await _dbService.ExecuteQueryAsync(
                "SELECT r.rental_id, r.disc_id, r.member_id, r.rental_date, r.return_date, " +
                "d.title as disc_title, d.artist, m.name as member_name " +
                "FROM Rentals r " +
                "JOIN Discs d ON r.disc_id = d.disc_id " +
                "JOIN Members m ON r.member_id = m.member_id " +
                "WHERE r.rental_id = :rental_id",
                parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            if (dataTable.Columns.Contains("Error"))
            {
                throw new Exception(dataTable.Rows[0]["Error"].ToString());
            }

            return CreateRentalFromRow(dataTable.Rows[0]);
        }

        public async Task<bool> AddAsync(Rental rental)
        {
            // Use the RentDisc procedure for adding a new rental
            return await RentDiscAsync(rental.MemberId, rental.DiscId);
        }

        public async Task<bool> UpdateAsync(Rental rental)
        {
            // For rentals, we primarily just update return date
            if (rental.ReturnDate.HasValue)
            {
                return await ReturnDiscAsync(rental.RentalId);
            }

            return false;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parameters = new Dictionary<string, object> { { "rental_id", id } };
            return await _dbService.ExecuteNonQueryAsync(
                "DELETE FROM Rentals WHERE rental_id = :rental_id", parameters);
        }

        // Call the RentDisc procedure from the database
        public async Task<bool> RentDiscAsync(int memberId, int discId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_member_id", memberId },
                { "p_disc_id", discId }
            };

            return await _dbService.ExecuteNonQueryAsync("RentDisc", parameters);
        }

        // Call the ReturnDisc procedure from the database
        public async Task<bool> ReturnDiscAsync(int rentalId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_rental_id", rentalId }
            };

            return await _dbService.ExecuteNonQueryAsync("ReturnDisc", parameters);
        }

        // Call the GetActiveRentals function from the database
        public async Task<int> GetActiveRentalsCountAsync(int memberId)
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_member_id", memberId }
            };

            return await _dbService.ExecuteFunctionAsync<int>("GetActiveRentals", parameters);
        }

        // Helper method to create a Rental object from a DataRow
        private Rental CreateRentalFromRow(DataRow row)
        {
            var rental = new Rental
            {
                RentalId = Convert.ToInt32(row["rental_id"]),
                DiscId = Convert.ToInt32(row["disc_id"]),
                MemberId = Convert.ToInt32(row["member_id"]),
                RentalDate = Convert.ToDateTime(row["rental_date"]),
                ReturnDate = row["return_date"] != DBNull.Value ? Convert.ToDateTime(row["return_date"]) : (DateTime?)null
            };

            // Create simplified Disc and Member objects with available data
            if (row.Table.Columns.Contains("disc_title"))
            {
                rental.Disc = new Disc
                {
                    DiscId = rental.DiscId,
                    Title = row["disc_title"].ToString(),
                    Artist = row["artist"].ToString()
                };
            }

            if (row.Table.Columns.Contains("member_name"))
            {
                rental.Member = new Member
                {
                    MemberId = rental.MemberId,
                    Name = row["member_name"].ToString()
                };
            }

            return rental;
        }
    }
}