using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RedisConnector.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisConnector.Outbox
{ 
    internal class OutboxMessageEntityConfig : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("OutboxMessages", "RedisOutbox");
            entityTypeBuilder.HasKey(e => e.Id);
            entityTypeBuilder.Property(e => e.Id).IsRequired().ValueGeneratedNever();
            entityTypeBuilder.HasIndex(x => x.Id).IsUnique();
             
            entityTypeBuilder.Property(x => x.MessageKey).IsRequired();
            entityTypeBuilder.Property(x => x.StreamName).IsRequired();
            entityTypeBuilder.Property(x => x.AddedAt).IsRequired(); 
            entityTypeBuilder.Property(x => x.Message).IsRequired().HasColumnType("NVARCHAR(MAX)");

        }
    }
}
