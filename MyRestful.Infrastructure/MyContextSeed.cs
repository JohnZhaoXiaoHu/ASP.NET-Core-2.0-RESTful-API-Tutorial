using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MyRestful.Core.DomainModels;

namespace MyRestful.Infrastructure
{
    public class MyContextSeed
    {
        public static async Task SeedAsync(MyContext myContext,
                       ILoggerFactory loggerFactory, int retry = 0)
        {
            int retryForAvailability = retry;
            try
            {
                // TODO: Only run this if using a real database
                // context.Database.Migrate();

                if (!myContext.Countries.Any())
                {
                    myContext.Countries.AddRange(
                        new List<Country>{
                            new Country{
                                EnglishName = "China",
                                ChineseName = "中华人民共和国",
                                Abbreviation = "中国",
                                Cities = new List<City>
                                {
                                    new City{ Name = "北京", Description = "首都"},
                                    new City{ Name = "上海", Description = "魔都" },
                                    new City{ Name = "深圳" },
                                    new City{ Name = "杭州" },
                                    new City{ Name = "天津" }
                                }
                            },
                            new Country{
                                EnglishName = "USA",
                                ChineseName = "美利坚合众国",
                                Abbreviation = "美国",
                                Cities = new List<City>
                                {
                                    new City{ Name = "New York" },
                                    new City{ Name = "Chicago" },
                                    new City{ Name = "San Fransisco" },
                                    new City{ Name = "Los Angeles" },
                                    new City{ Name = "Miami" }
                                }
                            },
                            new Country{
                                EnglishName = "Finland",
                                ChineseName = "芬兰",
                                Abbreviation = "芬兰",
                                Cities = new List<City>
                                {
                                    new City{ Name = "Helsinki" },
                                    new City{ Name = "Espoo" },
                                    new City{ Name = "Tampere" }
                                }
                            },
                            new Country{
                                EnglishName = "UK",
                                ChineseName = "大不列颠及北爱尔兰联合王国",
                                Abbreviation = "英国",
                                Cities = new List<City>
                                {
                                    new City{ Name = "London" },
                                    new City{ Name = "Liverpool" },
                                    new City{ Name = "Manchester" },
                                    new City{ Name = "Birmingham" },
                                    new City{ Name = "Glasgow" }
                                }
                            }
                        }
                    );
                    await myContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                if (retryForAvailability < 10)
                {
                    retryForAvailability++;
                    var logger = loggerFactory.CreateLogger<MyContextSeed>();
                    logger.LogError(ex.Message);
                    await SeedAsync(myContext, loggerFactory, retryForAvailability);
                }
            }
        }
    }
}
