using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Webscan.NotificationProcessor.Datastore;

namespace Webscan.NotificationProcessor.Models.Repository
{
    public class UserRepository : IUserRepository<User>
    {
        private readonly WebscanContext _webscanContext;
        public UserRepository(WebscanContext webscanContext)
        {
            _webscanContext = webscanContext ?? throw new ArgumentNullException($"{nameof(webscanContext)} cannot be null");
        }
        public void Add(User entity)
        {
            _webscanContext.Users.Add(entity);
            _webscanContext.SaveChanges();                
        }

        public void Delete(User entity)
        {
            _webscanContext.Users.Remove(entity);
            _webscanContext.SaveChanges();
        }

        public User Get(int id)
        {            
            return _webscanContext.Users.Include(x => x.StatusChecks).FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<User> GetAll()
        {            
            return _webscanContext.Users.Include(x => x.StatusChecks).ToList();
        }

        public void Update(User dbEntity, User entity)
        {
            dbEntity.Username = entity.Username;
            dbEntity.email = entity.email;

            dbEntity.StatusChecks.Clear(); 
            foreach(var statusCheck in entity.StatusChecks)
            {
                dbEntity.StatusChecks.Add(statusCheck);
            }

            _webscanContext.SaveChanges();
        }
    }
}
