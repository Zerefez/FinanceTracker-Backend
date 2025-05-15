using FinanceTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace FinanceTracker.DataAccess
{
    public class Dbseeder
    {
        public static void Initialize(FinanceTrackerContext context)
        {
            try
            {
                // First verify tables exist by attempting to access them
                try
                {
                    // If we can't access Users table, exit early
                    var userCount = context.Users.Count();
                    if (userCount > 0)
                    {
                        // We already have users, no need to seed
                        return;
                    }
                }
                catch (Exception)
                {
                    // Tables don't exist or can't be accessed correctly
                    // Don't proceed with seeding
                    return;
                }

                // We need to manually create a FinanceUser and save it
                var user = new FinanceUser
                {
                    UserName = "testuser@example.com",
                    Email = "testuser@example.com",
                    Age = 25,
                    EmailConfirmed = true,
                    NormalizedEmail = "TESTUSER@EXAMPLE.COM",
                    NormalizedUserName = "TESTUSER@EXAMPLE.COM",
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                // Set a password hash (this is a placeholder - normally would use a password hasher)
                user.PasswordHash = new PasswordHasher<FinanceUser>().HashPassword(user, "Test@123");

                // Add the user directly to the context
                context.Users.Add(user);
                
                try 
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    // If saving fails, exit
                    return;
                }

                //Get the user's ID
                var userId = user.Id;

                // Create workshifts and job
                var workshift = new WorkShift
                {
                    StartTime = new DateTime(2025, 4, 10, 9, 0, 0),
                    EndTime = new DateTime(2025, 4, 10, 17, 0, 0),
                    UserId = userId,
                };

                var workshift1 = new WorkShift
                {
                    StartTime = new DateTime(2025, 4, 11, 9, 0, 0),
                    EndTime = new DateTime(2025, 4, 11, 17, 0, 0),
                    UserId = userId,
                };

                var job = new Job
                {
                    CompanyName = "Demderveddet",
                    HourlyRate = 150,
                    UserId = userId
                };

                context.WorkShifts.Add(workshift);
                context.WorkShifts.Add(workshift1);
                context.Jobs.Add(job);

                try
                {
                    context.SaveChanges();
                }
                catch (Exception)
                {
                    // If saving fails, exit - at least we tried
                    return;
                }
            }
            catch (Exception)
            {
                // General exception handler - fail silently to prevent app crash
            }
        }
    }
}
