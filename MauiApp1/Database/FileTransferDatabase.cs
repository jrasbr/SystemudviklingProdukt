using MauiApp1.Model.Transfers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MauiApp1.Util;

namespace MauiApp1.Database
{
    public class FileTransferDatabase
    {
        SQLiteAsyncConnection Database;
        public FileTransferDatabase()
        {
            Task.Run(Init).Wait();
        }

        async Task Init()
        {
            if (Database is not null)
                return;

            Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
            var result3 = await Database.CreateTableAsync<FileTransferState>();
        }

        public async Task<List<FileTransferState>> GetAll()
        {
            return await Database.Table<FileTransferState>().ToListAsync();
        }

        public async Task Update(FileTransferState model)
        {
            await Database.InsertOrReplaceAsync(model);
        }

        public async Task Delete(string fileId)
        {
            await Database.DeleteAsync<FileTransferState>(fileId);
        }

        internal async Task Save(FileTransferState fileTransferState)
        {
            await Database.InsertOrReplaceAsync(fileTransferState);
        }
    }
}
