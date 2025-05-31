using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using WebApp.Abstract;
using WebApp.Models;

namespace WebApp.Service
{
    public class UserService : IUser
    {
        private readonly IConfiguration _cfg;

        public UserService(IConfiguration cfg)
        {
            _cfg = cfg;
        }

        public User AddUser(User user)
        {
            try
            {
                using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
                con.Execute("pUsers_Create", new
                {
                    name = user.Name,
                    email = user.Email,
                    password = user.Password
                }, commandType: CommandType.StoredProcedure);

                return user;
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601)
                {
                    throw new Exception("User with this email already exists.");
                }
                throw; 
            }
        }

        public bool BlockUser(int id)
        {
            using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
            var rowsAffected = con.Execute("pBlockUser", new { id }, commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public bool UnblockUser(int id)
        {
            using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
            var rowsAffected = con.Execute("pUnblockUser", new { id }, commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public bool DeleteUser(int id)
        {
            using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
            var rowsAffected = con.Execute("pUsers_Delete", new { id }, commandType: CommandType.StoredProcedure);
            return rowsAffected > 0;
        }

        public User? GetUserByEmail(string email)
        {
            using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
            return con.QueryFirstOrDefault<User>("pGetUserByEmail", new { Email = email }, commandType: CommandType.StoredProcedure);
        }

        public User? GetUserById(int id)
        {
            using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
            return con.QueryFirstOrDefault<User>("pUsers_GetById", new { id }, commandType: CommandType.StoredProcedure);
        }

        public IEnumerable<User> GetUsers()
        {
            using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
            return con.Query<User>("pUsers_GetAll", commandType: CommandType.StoredProcedure);
        }

        public void UpdateLastSeen(int id, DateTime LastSeen)
        {
            using var con = new SqlConnection(_cfg.GetConnectionString("DB"));
            con.Execute("pUsers_UpdateLastSeen", new { Id = id, LastSeen }, commandType: CommandType.StoredProcedure);
        }
    }
}
