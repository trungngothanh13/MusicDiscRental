using System.Collections.Generic;
using System.Threading.Tasks;
using MusicDiscRental.Models;
using MusicDiscRental.Database.DBConnection;

namespace MusicDiscRental.Database.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<bool> AddAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(int id);
    }
}