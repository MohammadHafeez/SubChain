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

        public void CreateMessage(Messages message)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO Message (ChatId, SenderId, Message, TimeStamp)
                              OUTPUT INSERTED.ChatId VALUES(@cId, @sId, @m, @tS)";

            cmd.Parameters.AddWithValue("@cId", message.ChatId);
            cmd.Parameters.AddWithValue("@sId", message.SenderId);
            cmd.Parameters.AddWithValue("@m", message.Message);
            cmd.Parameters.AddWithValue("@ts", message.Timestamp);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public List<Messages> GetAllMessages()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Select * from Message";

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            List<Messages> messageList = new List<Messages>();
            while (reader.Read())
            {
                messageList.Add(new Messages
                {
                    MessageId = reader.GetInt32(0),
                    ChatId = reader.GetInt32(1),
                    SenderId = reader.GetInt32(2),
                    Message = reader.GetString(3),
                    Timestamp = reader.GetDateTime(4)
                });
            }
            reader.Close();
            conn.Close();

            return messageList;
        }
    }
}
