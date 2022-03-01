using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisConnector.Core
{
    public interface IOutboxRepository
    { 
        Task<OutboxMessage> InsertAsync(OutboxMessage message, bool autoSave);
        Task<OutboxMessage> UpdateAsync(OutboxMessage message, bool autoSave);
        Task<IEnumerable<OutboxMessage>> GetAsync();
        Task<OutboxMessage> GetByIdAsync(Guid id);
        Task<IEnumerable<OutboxMessage>> GetUnprocessedMessages();
        void SetDbContext(DbContext dbContext); 
    }
}
