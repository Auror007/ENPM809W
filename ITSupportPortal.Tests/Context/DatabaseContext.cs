using ITSupportPortal.Data;
using Microsoft.EntityFrameworkCore;


namespace ITSupportPortal.Tests.Context
{
    public class DatabaseContext
    {
        public async Task<ApplicationDbContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.Users.CountAsync() <= 0)
            {
                for (int i = 1; i <= 10; i++)
                {
                    databaseContext.Case.Add(new Models.Case()
                    {
                        Id = "test_string_1",
                        CustomerID = "customer1",
                        EmployeeID = "agent1",
                        Title = "Case 1",
                        Description ="How to add user to AD group? ",
                        State = Data.Enums.CaseState.Open,
                        CreationTime = DateTime.Now,
                      
                    });
                    databaseContext.Case.Add(new Models.Case()
                    {
                        Id = "test_string_2",
                        CustomerID = "customer1",
                        EmployeeID = "agent1",
                        Title = "Case 2",
                        Description = "How to remove user to AD group? ",
                        State = Data.Enums.CaseState.Open,
                        CreationTime = DateTime.Now,

                    });
                    await databaseContext.SaveChangesAsync();
                }
            }
            return databaseContext;
        }
    }
}
