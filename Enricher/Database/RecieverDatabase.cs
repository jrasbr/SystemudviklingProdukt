using Enricher.Model.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enricher.Database
{
    public class RecieverDatabase
    {
        private string _connectionString = "Data Source=localhost;Initial Catalog=CustomerDB;Integrated Security=True;";
        public RecieverDatabase()
        {

        }

        /*
         CREATE TABLE Customer
(
	CustomerId int  IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(MAX) NOT NULL,
	CustomerEmail NVARCHAR(MAX) NOT NULL
)

CREATE TABLE CustomerContact
(
	CustomerContactId int  IDENTITY(1,1) PRIMARY KEY,
    ContactName NVARCHAR(MAX) NOT NULL,
	ContactEmail NVARCHAR(MAX) NOT NULL,
	ContactType NVARCHAR(MAX) NOT NULL,
	CustomerId int,
	FOREIGN KEY (CustomerId) REFEREnCES Customer(CustomerID)
)
         */

        public Customer GetCustomer(int customerId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
               
                connection.Open();
                string query = $"SELECT * FROM Customer WHERE CustomerId = {customerId}";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Customer customer = new Customer();
                            customer.CustomerId = (int)reader["CustomerId"];
                            customer.CustomerName = (string)reader["CustomerName"];
                            customer.CustomerEmail = (string)reader["CustomerEmail"];
                            customer.Contacts = GetContacts(customer.CustomerId);
                            return customer;
                        }
                    }
                }
            }
            return null;
        }

        public List<CustomerContact> GetContacts(int customerId)
        {
            List<CustomerContact> contacts = new List<CustomerContact>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                string query = $"SELECT * FROM CustomerContact WHERE CustomerId = {customerId}";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@customerId", customerId);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CustomerContact reciever = new CustomerContact();
                            reciever.CustomerId = (int)reader["CustomerContactId"];
                            reciever.CustomerContactId = (int)reader["CustomerContactId"];
                            reciever.ContactName = (string)reader["ContactName"];
                            reciever.ContactType = (string)reader["ContactType"];
                            reciever.RecieverEmail = (string)reader["ContactEmail"];
                            contacts.Add(reciever);

                        }
                    }
                }
            }
            return contacts;

        }
    }
}
