using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using P2_SubChain.Models;

namespace P2_SubChain.DAL
{
    public class InvoiceDAL
    {
        private IConfiguration Configuration { get; set; }
        private SqlConnection conn;

        public InvoiceDAL()
        {

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString("SubchainConnectionString");

            conn = new SqlConnection(strConn);
        }
        public List<Invoice> GetAllInvoice()
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Select * from Invoice";
            List<Invoice> invoiceList = new List<Invoice>();


            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                invoiceList.Add(new Invoice
                {
                    InvoiceId = reader.GetInt32(0),
                    ChainId = reader.GetInt32(1),
                    SenderId = reader.GetInt32(2),
                    ReceiverId = reader.GetInt32(3),
                    Filename = reader.GetString(4),
                    Status = reader.GetString(5),
                    ChainStatus = reader.GetString(6),
                    UploadDate = reader.GetDateTime(7),
                });
            }
            reader.Close();
            conn.Close();

            return invoiceList;
        }

        public void AddInvoice(Invoice invoice)
        {

            SqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = @"INSERT INTO Invoice (ChainId, SenderId, ReceiverId, Filename, Status, ChainStatus, UploadDate)
            OUTPUT INSERTED.ChainId VALUES(@cId, @sId, @rId, @fN, @s, @cS, @uD)";

            cmd.Parameters.AddWithValue("@cId", invoice.ChainId);
            cmd.Parameters.AddWithValue("@sId", invoice.SenderId);
            cmd.Parameters.AddWithValue("@rId", invoice.ReceiverId);
            cmd.Parameters.AddWithValue("@fN", invoice.Filename);
            cmd.Parameters.AddWithValue("@s", "Current");
            cmd.Parameters.AddWithValue("@cS", invoice.ChainStatus);
            cmd.Parameters.AddWithValue("@uD", invoice.UploadDate);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        public void UpdateInvoice(int id)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Update invoice set [Status] = 'Outdated' where [InvoiceId] = @id";
            cmd.Parameters.AddWithValue("@id", id);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }
}
