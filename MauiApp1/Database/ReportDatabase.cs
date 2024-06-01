using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp1.Util;
using MauiApp1.Model;
using CoreData;

namespace MauiApp1.Repository
{
    public class ReportDatabase
    {
        SQLiteAsyncConnection Database;
        public ReportDatabase()
        {
            Task.Run(Init).Wait();
        }
        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result = await Database.CreateTableAsync<T>();
        }

        public async Task<List<Report>> GetAll()
        {
           var reports = await Database.Table<Report>().ToListAsync();
            foreach (var report in reports)
            {
                report.Events = await Database.Table<ReportEvent>().Where(i => i.Re == report.ReportId).ToListAsync();
                //report.ImageIds = await Database.Table<string>().Where(i => i == report.ReportId).ToListAsync();
            
            }
            return reports;
        }

        public async Task<Report> Get(string id)
        {
            var report = await Database.Table<Report>().Where(i => i.ReportId == id).FirstOrDefaultAsync();

        }

    }
}