using BankManagementSystem.Entities;

namespace BankManagementSystem.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (!context.Managers.Any())
            {
                var manager = new Manager
                {
                    FullName = "Ahmed Mohamed",
                    Email = "ahmed@bank.com",
                    PhoneNumber = "01000000000",
                    HireDate = new DateTime(2020, 1, 1)
                };

                context.Managers.Add(manager);
                context.SaveChanges();
            }

            if (!context.Branches.Any())
            {
                var manager = context.Managers.First();

                var branch = new Branch
                {
                    Code = "CAI-01",
                    Name = "Cairo Main Branch",
                    Address = "Cairo, Egypt",
                    PhoneNumber = "0220000000",
                    ManagerId = manager.Id
                };

                context.Branches.Add(branch);
                context.SaveChanges();
            }
        }
    }
}