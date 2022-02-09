using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace RedisConnector.Core
{
    public interface IOutboxRepository
    {
        void SetContext(DbContext context);
        Task AddAsync(OutboxMessage message);
        Task Remove(OutboxMessage message);
    }
}
