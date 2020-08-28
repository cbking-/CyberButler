using CyberButler.Data.Entities;
using CyberButler.Data.EntityContext;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CyberButler.Console.Data
{
    public class CustomCommandService
    {
        private readonly CyberButlerContext _dbContext;

        public CustomCommandService(CyberButlerContext db)
        {
            _dbContext = db;
        }

        public Task<List<CustomCommand>> GetCommandsAsync()
        {
            return _dbContext.CustomCommand.ToListAsync();
        }
    }
}
