using Microsoft.EntityFrameworkCore;
using RedisConnector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisConnector.Outbox
{
    public class OutboxRepository : IOutboxRepository
    { 
        protected DbContext _context;

        public OutboxRepository()
        {
        }
         
        public void SetContext(object context)
        {
            context.Guard();
            _context = (DbContext)context;
        }

        public DbContext GetContext()
        {
            _context.Guard();
            return _context;
        }

        public async Task<OutboxMessage> InsertAsync(OutboxMessage message)
        {
            _context.Guard();

            message.Add();
            var result = await _context.Set<OutboxMessage>().AddAsync(message); 
            return message;
        }

        public async Task<OutboxMessage> GetByIdAsync(Guid id)
        {
            _context.Guard();
            return await _context.Set<OutboxMessage>().FirstAsync(o => o.Id.Equals(id));
        }

        public async Task<IEnumerable<OutboxMessage>> GetUnprocessedMessages()
        {
            _context.Guard();
            return await _context.Set<OutboxMessage>().Where(m => !m.Processed).ToListAsync();
        }

        public async Task<IEnumerable<OutboxMessage>> GetAsync()
        {
            _context.Guard();
            return await _context.Set<OutboxMessage>().ToListAsync();
        }

        public OutboxMessage Update(OutboxMessage message)
        {
            _context.Guard();

            message.Update();
            var result = _context.Set<OutboxMessage>().Update(message);
            return message;
        }  
    }
}
