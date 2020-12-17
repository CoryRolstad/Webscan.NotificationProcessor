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

        public IEnumerable<User> GetUsers(int statusCheckId)
        {
            string queryString = @"SELECT Users.Id AS Id, Users.Email AS Email, Users.Username AS Username
                FROM dbo.Users AS Users
                JOIN dbo.StatusCheckUsers AS SCU ON Users.Id = SCU.UserId AND SCU.Enabled = 1
                JOIN dbo.StatusChecks AS StatusChecks ON SCU.StatusCheckId = StatusChecks.Id
                WHERE StatusChecks.Id = {0}";
            return _webscanContext.Users.FromSqlRaw(queryString, statusCheckId).ToList();
        }

        public void Update(StatusCheck statusCheck)
        {
            StatusCheck entity = _webscanContext.StatusChecks.FirstOrDefault(x => x.Id == statusCheck.Id);
            entity.LastNotified = statusCheck.LastNotified;
            _webscanContext.SaveChanges(); 
        }
    }
}
