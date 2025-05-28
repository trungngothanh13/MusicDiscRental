using MusicDiscRental.Database.DBConnection;
using MusicDiscRental.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MusicDiscRental.Database.Repositories
{
    public class MemberRepository : IRepository<Member>
    {
        private readonly IDatabaseService _dbService;

        public MemberRepository(IDatabaseService dbService)
        {
            _dbService = dbService ?? MockDatabaseService.Instance;
        }

        public async Task<IEnumerable<Member>> GetAllAsync()
        {
            var result = new List<Member>();
            var dataTable = await _dbService.ExecuteQueryAsync("SELECT * FROM Members");

            foreach (DataRow row in dataTable.Rows)
            {
                result.Add(CreateMemberFromRow(row));
            }

            return result;
        }

        public async Task<Member> GetByIdAsync(int id)
        {
            var parameters = new Dictionary<string, object> { { "member_id", id } };
            var dataTable = await _dbService.ExecuteQueryAsync(
                "SELECT * FROM Members WHERE member_id = :member_id", parameters);

            if (dataTable.Rows.Count == 0)
                return null;

            return CreateMemberFromRow(dataTable.Rows[0]);
        }

        public async Task<bool> AddAsync(Member member)
        {
            var parameters = new Dictionary<string, object>
            {
                { "name", member.Name },
                { "phone_number", member.PhoneNumber },
                { "join_date", member.JoinDate }
            };

            return await _dbService.ExecuteNonQueryAsync(
                "INSERT INTO Members (member_id, name, phone_number, join_date) " +
                "VALUES (member_seq.NEXTVAL, :name, :phone_number, :join_date)",
                parameters);
        }

        public async Task<bool> UpdateAsync(Member member)
        {
            var parameters = new Dictionary<string, object>
            {
                { "member_id", member.MemberId },
                { "name", member.Name },
                { "phone_number", member.PhoneNumber },
                { "join_date", member.JoinDate }
            };

            return await _dbService.ExecuteNonQueryAsync(
                "UPDATE Members SET name = :name, phone_number = :phone_number, join_date = :join_date " +
                "WHERE member_id = :member_id",
                parameters);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var parameters = new Dictionary<string, object> { { "member_id", id } };
            return await _dbService.ExecuteNonQueryAsync(
                "DELETE FROM Members WHERE member_id = :member_id", parameters);
        }

        // Helper method to create a Member object from a DataRow
        private Member CreateMemberFromRow(DataRow row)
        {
            return new Member
            {
                MemberId = Convert.ToInt32(row["member_id"]),
                Name = row["name"].ToString(),
                PhoneNumber = row["phone_number"] != DBNull.Value ? row["phone_number"].ToString() : null,
                JoinDate = Convert.ToDateTime(row["join_date"])
            };
        }
    }
}