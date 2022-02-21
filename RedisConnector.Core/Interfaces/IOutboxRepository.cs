using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RedisConnector.Core
{
    public interface IOutboxRepository
    {
        void SetContext(object context);
        Task<OutboxMessage> InsertAsync(OutboxMessage message);
        OutboxMessage Update(OutboxMessage message);
        Task<IEnumerable<OutboxMessage>> GetAsync();
        Task<OutboxMessage> GetByIdAsync(Guid id);
        Task<IEnumerable<OutboxMessage>> GetUnprocessedMessages();
    }
}
