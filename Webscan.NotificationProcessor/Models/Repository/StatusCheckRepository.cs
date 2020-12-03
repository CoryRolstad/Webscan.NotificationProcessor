using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Webscan.NotificationProcessor.Datastore;

namespace Webscan.NotificationProcessor.Models.Repository
{
    public class StatusCheckRepository : IStatusCheckRepository<StatusCheck>
    {
        private readonly WebscanContext _webscanContext; 

        public StatusCheckRepository(WebscanContext webscanContext)
        {
               _webscanContext = webscanContext ?? throw new ArgumentNullException($"{nameof(webscanContext)} cannot be null");
        }
        public void Add(StatusCheck entity)
        {
            _webscanContext.StatusChecks.Add(entity);
            _webscanContext.SaveChanges();
        }

        public void Delete(StatusCheck entity)
        {
            _webscanContext.StatusChecks.Remove(entity);
            _webscanContext.SaveChanges();
        }

        public StatusCheck Get(int id)
        {
           return _webscanContext.StatusChecks.Include(x => x.Users).FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<StatusCheck> GetAll()
        {
            return _webscanContext.StatusChecks.Include(x => x.Users).ToList(); 
        }

        public void Update(StatusCheck statusCheck)
        {
            StatusCheck entity = _webscanContext.StatusChecks.FirstOrDefault(x => x.Id == statusCheck.Id);
            entity.LastNotified = statusCheck.LastNotified;
            _webscanContext.SaveChanges(); 
        }
    }
}
