using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalSMS.DAL.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortalSMS.DAL.Data.Context
{
    public class SystemContext : IdentityDbContext<User>
    {
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<SentMessage> SentMessages { get; set; }
        public DbSet<Log> Logs { get; set; }
        public SystemContext(DbContextOptions<SystemContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the primary key for MessageTemplate
            modelBuilder.Entity<MessageTemplate>()
                .HasKey(m => m.TemplateID);

            // Configure the primary key for SentMessage
            modelBuilder.Entity<SentMessage>()
                .HasKey(s => s.MessageID);

            // Configure the primary key for Log
            modelBuilder.Entity<Log>()
                .HasKey(l => l.LogID);

            // Configure the relationship between SentMessage and User
            modelBuilder.Entity<SentMessage>()
                .HasOne(s => s.User)
                .WithMany() // Assuming User does not have a collection of SentMessages
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Restrict); // Configure delete behavior if needed

            // Configure the relationship between SentMessage and MessageTemplate
            modelBuilder.Entity<SentMessage>()
                .HasOne(s => s.Template)
                .WithMany(m => m.SentMessages)
                .HasForeignKey(s => s.TemplateID)
                .OnDelete(DeleteBehavior.SetNull); // Configure delete behavior if needed

            // Configure the relationship between Log and SentMessage
            modelBuilder.Entity<Log>()
                .HasOne(l => l.SentMessage)
                .WithMany(s => s.Logs)
                .HasForeignKey(l => l.MessageID)
                .OnDelete(DeleteBehavior.Cascade); // Configure delete behavior if needed

            // Configure the relationship between MessageTemplate and User
            modelBuilder.Entity<MessageTemplate>()
                .HasOne(mt => mt.Creator)
                .WithMany() // Assuming User does not have a collection of MessageTemplates as Creator
                .HasForeignKey(mt => mt.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MessageTemplate>()
                .HasOne(mt => mt.LastModifier)
                .WithMany() // Assuming User does not have a collection of MessageTemplates as LastModifier
                .HasForeignKey(mt => mt.LastModifiedBy)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Log>()
       .HasOne(l => l.SentMessage)  // Assuming Log has a navigation property for SentMessage
       .WithMany(sm => sm.Logs)     // Assuming SentMessage has a collection of Logs
       .HasForeignKey(l => l.MessageID)
       .OnDelete(DeleteBehavior.Restrict); //

            modelBuilder.Entity<Log>()
        .Ignore(l => l.SentMessage); // If you don't need the relationship
        }
    

}
}
