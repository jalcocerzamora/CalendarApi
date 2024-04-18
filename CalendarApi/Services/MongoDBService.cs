
using CalendarApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;
using System.Data;

namespace CalendarApi.Services
{
    public class MongoDBService
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Users> _userCollection;
        private readonly IMongoCollection<Roles> _roleCollection;

        public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            var connectionString = mongoDBSettings.Value.ConnectionString;
            _mongoClient = new MongoClient(connectionString);
            _database = _mongoClient.GetDatabase("CalendarApi");

            _userCollection = _database.GetCollection<Users>("users");
            _roleCollection = _database.GetCollection<Roles>("roles");
        }

        public async Task<List<Users>> GetAllUsers()
        {
            return await _userCollection.Find(_ => true).ToListAsync();
        }

        public async Task<List<Roles>> GetRoles()
        {
            return await _roleCollection.Find(_ => true).ToListAsync();
        }

        // Método para crear un nuevo usuario
        public async Task CreateUser(Users user)
        {
            await _userCollection.InsertOneAsync(user);
        }

        public async Task CreateRole(Roles roles)
        {
            await _roleCollection.InsertOneAsync(roles);
        }

        // Método para obtener un usuario por su Id
        public async Task<Users> GetUserById(string userId)
        {
            var filter = Builders<Users>.Filter.Eq(u => u.Id, userId);
            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Users> GetUserByUsername(string username)
        {
            var filter = Builders<Users>.Filter.Eq(u => u.Username, username);
            return await _userCollection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Roles> GetRoleByName(string roleName)
        {
            var filter = Builders<Roles>.Filter.Eq(r => r.Name, roleName);
            return await _roleCollection.Find(filter).FirstOrDefaultAsync();
        }

        // Método para actualizar un usuario
        public async Task UpdateUser(string userId, Users updatedUser)
        {
            var filter = Builders<Users>.Filter.Eq(u => u.Id, userId);
            await _userCollection.ReplaceOneAsync(filter, updatedUser);
        }

        // Método para eliminar un usuario por su Id
        public async Task DeleteUser(string userId)
        {
            var filter = Builders<Users>.Filter.Eq(u => u.Id, userId);
            await _userCollection.DeleteOneAsync(filter);
        }

        public void PrintCollection()
        {
            var collectionNames = _database.ListCollectionNames().ToList();
            Console.WriteLine("Collections in the database:");
            foreach (var collectionName in collectionNames)
            {
                Console.WriteLine(collectionName);
            }
        }


    }
}
