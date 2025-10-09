using SoftFocusBackend.Notification.Domain.Model.Aggregates;
using SoftFocusBackend.Notification.Domain.Repositories;
using SoftFocusBackend.Shared.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoftFocusBackend.Notification.Infrastructure.Persistence.MongoDB.Repositories
{
    public class MongoNotificationTemplateRepository : INotificationTemplateRepository
    {
        // IBaseRepository<NotificationTemplate> implementation
        public Task AddAsync(NotificationTemplate entity)
        {
            throw new NotImplementedException();
        }

        public Task<NotificationTemplate?> FindByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public void Update(NotificationTemplate entity)
        {
            throw new NotImplementedException();
        }

        public void Remove(NotificationTemplate entity)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<NotificationTemplate>> ListAsync()
        {
            throw new NotImplementedException();
        }

        // INotificationTemplateRepository specific method
        public Task<NotificationTemplate?> GetByTypeAsync(string type)
        {
            throw new NotImplementedException();
        }
    }
}