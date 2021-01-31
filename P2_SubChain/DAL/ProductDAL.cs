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
    public class ProductDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        public ProductDAL()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("SubchainConnectionString");
            conn = new SqlConnection(strConn);
        }

        public int AddProduct(Product product)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Product (UserId, ProductName, ProductDesc, ProductType, ProductPrice, ImageUrl)
                              OUTPUT INSERTED.UserId VALUES(@uId, @pN, @pD, @pT, @pP, @iURL)";

            cmd.Parameters.AddWithValue("@uId", product.UserId);
            cmd.Parameters.AddWithValue("@pN", product.ProductName);
            cmd.Parameters.AddWithValue("@pD", product.ProductDesc);
            cmd.Parameters.AddWithValue("@pT", product.ProductType);
            cmd.Parameters.AddWithValue("@pP", product.ProductPrice);
            cmd.Parameters.AddWithValue("@iURL", product.ImageUrl);

            conn.Open();
            int id = (int)cmd.ExecuteScalar();
            conn.Close();

            return id;
        }

        public List<Product> GetAllProducts()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Select * from Product";

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            List<Product> productList = new List<Product>();
            while (reader.Read())
            {
                productList.Add(new Product
                {
                    ProductId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ProductName = reader.GetString(2),
                    ProductDesc = reader.GetString(3),
                    ProductPrice = reader.GetDouble(4),
                    ImageUrl = reader.GetString(5),
                }) ;
            }
            reader.Close();
            conn.Close();

            return productList;
        }
    }
}
