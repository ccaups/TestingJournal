using SQLite;
using System.Linq.Expressions;
using System.Diagnostics;
using TravelJournalApp.Data;
using System.Collections.Generic;

namespace TravelJournalApp.Data
{
    public class DatabaseContext : IAsyncDisposable
    {
        private const string DbName = "TJAdb.db";
        //private static string DbPath => Path.Combine(".", DbName);

        //android jaoks
        private static string DbPath => Path.Combine(FileSystem.AppDataDirectory, DbName);

        //näitab faili db asukohta, view-> output
        public DatabaseContext()
        {
            // Log the DbPath to the Visual Studio Output window
            Debug.WriteLine($"Database path: {DbPath}");
        }

        private SQLiteAsyncConnection _connectionString;
        private SQLiteAsyncConnection Database =>
            (_connectionString ??= new SQLiteAsyncConnection(DbPath,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache));

        public async Task<IEnumerable<TTable>> GetAllAsync<TTable>() where TTable : class, new()
        {
            var table = await GetTableAsync<TTable>();
            return await table.ToListAsync();
        }

        private async Task<AsyncTableQuery<TTable>> GetTableAsync<TTable>() where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return Database.Table<TTable>();
        }

        private async Task CreateTableIfNotExists<TTable>() where TTable : class, new()
        {
            await Database.CreateTableAsync<TTable>();
        }

        private async Task<TResult> Execute<TTable, TResult>(Func<Task<TResult>> action) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await action();
        }

        public async Task<TTable> GetItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
        {
            return await Execute<TTable, TTable>(async () => await Database.GetAsync<TTable>(primaryKey));
        }

        public async Task<bool> AddItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            return await Execute<TTable, bool>(async () => await Database.InsertAsync(item) > 0);
        }

        public async Task<bool> UpdateItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.UpdateAsync(item) > 0;
        }

        public async Task<bool> DeleteItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.DeleteAsync(item) > 0;
        }

        public async Task<bool> DeleteItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.DeleteAsync<TTable>(primaryKey) > 0;
        }

        public async ValueTask DisposeAsync() => await _connectionString.CloseAsync();

        public async Task<IEnumerable<TTable>> GetFilteredAsync<TTable>(Expression<Func<TTable, bool>> predicate) where TTable : class, new()
        {
            var table = await GetTableAsync<TTable>();
            return await table.Where(predicate).ToListAsync();
        }

        //public async Task<string> GetImagePathForTravel(Guid travelId)
        //{
        //    string query = "SELECT ImagePath FROM ImageDatabase WHERE TravelJournalId = ? LIMIT 1";

        //    // Kasuta asünkroonset ühendust ja päringut
        //    var result = await Database.ExecuteScalarAsync<string>(query, travelId);

        //    return result; // Tagasta tulemus või null, kui tulemust ei leitud
        //}

        public async Task<bool> SaveImageAsync(ImageTable imageTable)
        {
            if (imageTable.Id == Guid.Empty)
            {
                return await AddItemAsync(imageTable);
            }
            else
            {
                return await UpdateItemAsync(imageTable);
            }
        }

		public Task<TravelJournalTable> GetItemAsync(Guid id)
		{
			return Database.Table<TravelJournalTable>().Where(t => t.Id == id).FirstOrDefaultAsync();
		}

		public async Task<List<ImageTable>> GetImagesForTravelJournalAsync(Guid travelJournalId)
		{
			try
			{
				return await Database.Table<ImageTable>()
									 .Where(img => img.TravelJournalId == travelJournalId)
									 .ToListAsync();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Error retrieving images: {ex.Message}");
				throw;
			}
		}
	}
}