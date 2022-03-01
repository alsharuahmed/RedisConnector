using Microsoft.EntityFrameworkCore;
using RedisConnector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RedisConnector.Outbox
{
    public class OutboxRepository : IOutboxRepository
    { 
        protected DbContext _dbContext;

        public OutboxRepository()
        {
        }
         
        public void SetDbContext(DbContext dbContext)
        {
            dbContext.Guard(nameof(dbContext));
            _dbContext = dbContext;
        }

        public DbContext GetContext() => _dbContext;

        public async Task<OutboxMessage> InsertAsync(OutboxMessage message, bool autoSave)
        { 
            message.Add();
            await _dbContext.Set<OutboxMessage>().AddAsync(message);

            if (autoSave)
                await SaveChangesAsync();

            return message;
        }

        public async Task<OutboxMessage> GetByIdAsync(Guid id) 
            => await _dbContext.Set<OutboxMessage>().FirstAsync(o => o.Id.Equals(id)); 

        public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessages()
            => await _dbContext.Set<OutboxMessage>().Where(m => !m.Processed).ToListAsync();

        public async Task<IEnumerable<OutboxMessage>> GetAsync()
            => await _dbContext.Set<OutboxMessage>().ToListAsync();

        public async Task<OutboxMessage> UpdateAsync(OutboxMessage message, bool autoSave)
        { 
            message.Update();
            _dbContext.Set<OutboxMessage>().Update(message);

            if (autoSave)
                await SaveChangesAsync();

            return message;
        }

        private async Task<int> SaveChangesAsync()
            =>  await _dbContext.SaveChangesAsync();
    }
}
