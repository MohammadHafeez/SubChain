using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data.SqlClient;
using P2_SubChain.Models;

namespace P2_SubChain.DAL
{
    public class UserDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        public UserDAL()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("SubchainConnectionString");
            conn = new SqlConnection(strConn);
        }

        public int AddUser(Users user)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Users (CompanyName, CompanyType, Email, Password, Country, CityOrState, ProductCategory)
                              OUTPUT INSERTED.UserId VALUES(@cn, @ct, @e, @p, @c, @cOrs, @pC)";

            cmd.Parameters.AddWithValue("@cn", user.CompanyName);
            cmd.Parameters.AddWithValue("@ct", user.CompanyType);
            cmd.Parameters.AddWithValue("@e", user.Email);
            cmd.Parameters.AddWithValue("@p", user.Password);

            if (user.Country != null)
            {
                cmd.Parameters.AddWithValue("@c", user.Country);
            }
            else
            {
                cmd.Parameters.AddWithValue("@c", DBNull.Value);
            }

            if (user.CityorState != null)
            {
                cmd.Parameters.AddWithValue("@cOrs", user.CityorState);
            }
            else
            {
                cmd.Parameters.AddWithValue("@cOrs", DBNull.Value);
            }

            if (user.ProductCategory != null)
            {
                cmd.Parameters.AddWithValue("@pC", string.Join(",", user.ProductCategory));
            }
            else
            {
                cmd.Parameters.AddWithValue("@pC", DBNull.Value);
            }

            conn.Open();
            int id = (int)cmd.ExecuteNonQuery();
            conn.Close();

            return id;
        }

        public List<Users> GetAllUser()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Select * from Users";
            
            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            List<Users> userList = new List<Users>();
            while (reader.Read())
            {
                Users user = new Users
                {
                    UserId = reader.GetInt32(0),
                    CompanyName = reader.GetString(1),
                    CompanyType = reader.GetString(2),
                    Email = reader.GetString(3),
                    Password = reader.GetString(4),
                    Country = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CityorState = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ProductCategoryString = reader.IsDBNull(7) ? null : reader.GetString(7),
                };

                if (user.ProductCategoryString != null)
                {
                    user.ProductCategory = user.ProductCategoryString.Split(",").ToList();

                }
                else
                {
                    user.ProductCategory = null;
                }
                userList.Add(user);
            }
            reader.Close();
            conn.Close();

            return userList;
        }
    }
}
