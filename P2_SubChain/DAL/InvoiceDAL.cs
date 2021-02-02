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

            conn.Open();
            SqlDataReader reader = cmd.ExecuteReader();

            List<Invoice> invoiceList = new List<Invoice>();
            while (reader.Read())
            {
                invoiceList.Add(
                 new Invoice
                 {
                     InvoiceId = reader.GetInt32(0),
                     ChainId = reader.GetInt32(1),
                     SenderId = reader.GetInt32(2),
                     Filename = reader.GetString(3),
                     UploadDate = reader.GetDateTime(4),
                     EditDate = !reader.IsDBNull(5) ? reader.GetDateTime(5) : null
                 });
            }
            reader.Close();
            conn.Close();

            return invoiceList;
        }

        public void AddInvoice(Invoice invoice)
        {

            SqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = @"INSERT INTO Invoice (ChainId, SenderId, Filename, UploadDate)
            OUTPUT INSERTED.ChainOd VALUES(@cId, @sId, @fN, @uD)";

            cmd.Parameters.AddWithValue("@cId", invoice.ChainId);
            cmd.Parameters.AddWithValue("@sId", invoice.SenderId);
            cmd.Parameters.AddWithValue("@fN", invoice.Filename);
            cmd.Parameters.AddWithValue("@uD", invoice.UploadDate);

            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //public int Update(Invoice invoice)
        //{
        //    SqlCommand cmd = conn.CreateCommand();
        //    cmd.CommandText = @"Update invoice set supplierid = @supplierid, date=@date 
        //                            where @selectedid = distributorID";
        //    cmd.Parameters.AddWithValue("@supplierid", invoice.SupplierID);
        //    cmd.Parameters.AddWithValue("@date", invoice.IDate);
        //    cmd.Parameters.AddWithValue("@selectedid", invoice.DistributorID);
        //    conn.Open();
        //    int count = cmd.ExecuteNonQuery();
        //    conn.Close();
        //    return count;
        //}
    }
}
