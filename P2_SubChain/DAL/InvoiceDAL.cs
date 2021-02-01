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

            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
            string strConn = Configuration.GetConnectionString(
            "SubchainConnectionString");

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
                     SupplierID = reader.GetInt32(0),
                     IDate = reader.GetDateTime(1)
                 }
                 );
            }
            reader.Close();
            conn.Close();

            return invoiceList;
        }

        public int Add(Invoice invoice)
        {

            SqlCommand cmd = conn.CreateCommand();

            cmd.CommandText = @"INSERT INTO Invoice (SupplierId, Date)
            OUTPUT INSERTED.SupplierId
            VALUES(@SupplierId, @Date)";

            cmd.Parameters.AddWithValue("@SupplierId", invoice.SupplierID);
            cmd.Parameters.AddWithValue("@Date", invoice.IDate);

            conn.Open();

            invoice.SupplierID = (int)cmd.ExecuteScalar();

            conn.Close();

            return invoice.SupplierID;

        }
        public int Update (Invoice invoice)
        {
            SqlCommand cmd = conn.CreateCommand();
            cmd.CommandText = @"Update invoice set supplierid = @supplierid, date=@date 
                                    where @selectedid = distributorID" ;
            cmd.Parameters.AddWithValue("@supplierid", invoice.SupplierID);
            cmd.Parameters.AddWithValue("@date", invoice.IDate);
            cmd.Parameters.AddWithValue("@selectedid", invoice.DistributorID);
            conn.Open();
            int count = cmd.ExecuteNonQuery();
            conn.Close();
            return count;
        }

    }
}
