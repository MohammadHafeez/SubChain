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
    public class CommunicationDAL
    {
        private IConfiguration Configuration { get; }
        private SqlConnection conn;

        public CommunicationDAL()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("SubchainConnectionString");
            conn = new SqlConnection(strConn);
        }

        public Chat CreateChat(Chat chat)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Chat (UserId1, UserId2)
                              OUTPUT INSERTED.UserId1 VALUES(@uId1, @uId2)";

            cmd.Parameters.AddWithValue("@uId1", chat.UserId1);
            cmd.Parameters.AddWithValue("@uId2", chat.UserId2);

            conn.Open();
            int id = (int)cmd.ExecuteScalar();
            conn.Close();

            chat.ChatId = id;

            return chat;
        }

        public List<Chat> GetAllChats()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Select * from Chat";

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            List<Chat> chatList = new List<Chat>();
            while (reader.Read())
            {
                chatList.Add(new Chat
                {
                    ChatId = reader.GetInt32(0),
                    UserId1 = reader.GetInt32(1),
                    UserId2 = reader.GetInt32(2),
                });
            }
            reader.Close();
            conn.Close();

            return chatList;
        }

        public List<Messages> GetAllMessages()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Select * from Message";

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            List<Messages> chatList = new List<Messages>();
            while (reader.Read())
            {
                chatList.Add(new Messages
                {
                    ChatId = reader.GetInt32(0),
                    SenderId = reader.GetInt32(1),
                    Message = reader.GetString(2),
                    Timestamp = reader.GetDateTime(3)
                });
            }
            reader.Close();
            conn.Close();

            return chatList;
        }
    }
}
