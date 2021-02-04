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
    public class ChainDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        public ChainDAL()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("SubchainConnectionString");
            conn = new SqlConnection(strConn);
        }

        public int CreateChain(Chain chain)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Chain (ChainOwner, Status)
                              OUTPUT INSERTED.ChainOwner VALUES(@cO, @s)";

            cmd.Parameters.AddWithValue("@cO", chain.ChainOwner);
            cmd.Parameters.AddWithValue("@s", "Efficient");

            conn.Open();
            int id = (int)cmd.ExecuteScalar();
            conn.Close();

            return id;
        }

        public List<Chain> GetAllChains()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Select * from Chain";

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            List<Chain> chatList = new List<Chain>();
            while (reader.Read())
            {
                chatList.Add(new Chain
                {
                    ChainId = reader.GetInt32(0),
                    ChainOwner = reader.GetInt32(1),
                    EfficientChain = reader.IsDBNull(2) ? null : reader.GetString(2),
                    ResponsiveChain = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Status = reader.GetString(4)
                });
            }
            reader.Close();
            conn.Close();

            return chatList;
        }

        public void SetEfficientChain(string newChain, int chainId)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Update Chain set [EfficientChain] = @c where [ChainId] = @id";
            if (newChain == "")
            {
                cmd.Parameters.AddWithValue("@c", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@c", newChain);
            }
            cmd.Parameters.AddWithValue("@id", chainId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void SetResponsiveChain(string chain, int chainId)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Update Chain set [ResponsiveChain] = @c where [ChainId] = @id";
            if (chain == "")
            {
                cmd.Parameters.AddWithValue("@c", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@c", chain);
            }
            cmd.Parameters.AddWithValue("@id", chainId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void SetStatus(string status, int chainId)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Update Chain set [Status] = @s where [ChainId] = @id";
            cmd.Parameters.AddWithValue("@s", status);
            cmd.Parameters.AddWithValue("@id", chainId);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
