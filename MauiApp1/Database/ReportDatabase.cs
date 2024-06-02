using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp1.Util;
using MauiApp1.Model;
using MauiApp1.Model.Transfers;
//using CoreData;

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
            var result0 = await Database.CreateTableAsync<ReportEvent>();
            var result1 = await Database.CreateTableAsync<ReportImage>();
            var result2 = await Database.CreateTableAsync<Report>();
        }

        public async Task<List<Report>> GetAll()
        {
           var reports = await Database.Table<Report>().ToListAsync();
            foreach (var report in reports)
            {
                report.Events = await Database.Table<ReportEvent>().Where(i => i.ReportId == report.ReportId).ToListAsync();
                //report.ImageIds = await Database.Table<string>().Where(i => i == report.ReportId).ToListAsync();
            
            }
            return reports;
        }

        public async Task<Report> Get(string id)
        {
            var report = await Database.Table<Report>().Where(i => i.ReportId == id).FirstOrDefaultAsync();
            report.Events = await Database.Table<ReportEvent>().Where(i => i.ReportId == id).ToListAsync();
            return report;

        }

        internal async Task Save(Report model)
        {
            await Database.InsertOrReplaceAsync(model);
            await Database.InsertAllAsync(model.Events);
        }
    }
}