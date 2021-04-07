using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace ADO_NET
{
    class Program
    {
        static void Main(string[] args)
        {
            //string connectionString = @"Data Source=.\MSSQLLocalDB;Initial Catalog=UsersDB;Integrated Security=True;/*User Id = valera; Password = 1*/";
            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            Console.WriteLine(connectionString);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                Console.WriteLine("Connection is open");

                Console.WriteLine("Get connection string: {0}", connection.ConnectionString);
                Console.WriteLine("Get database name: {0}", connection.Database);
                Console.WriteLine("Get server name: {0}", connection.DataSource);
                Console.WriteLine("Get server version: {0}", connection.ServerVersion);
                Console.WriteLine("Get connection state: {0}", connection.State);
                Console.WriteLine("Get Workstation ID: {0}", connection.WorkstationId);
                Console.WriteLine("Get Connection ID: {0}", connection.ClientConnectionId);

                var sqlExpression = "CREATE TABLE Customers(Id int not null identity(1,1) primary key, name NVARCHAR(255) not null, paymentType nvarchar(30))";
                using (var command = new SqlCommand(sqlExpression, connection))
                {
                    command.ExecuteNonQuery();
                }

                using (var command = new SqlCommand())
                {
                    command.CommandText = "INSERT INTO Users (Name, Address) VALUES ('Vasea', 'Chisinau')";
                    command.Connection = connection;
                    command.ExecuteNonQuery();
                }

                using (var command = new SqlCommand())
                {
                    command.CommandText = "Select count(*) from Users";
                    command.Connection = connection;
                    var number = command.ExecuteScalar();
                    Console.WriteLine(number);
                }

                using (var command = new SqlCommand())
                {
                    command.CommandText = "Select * from Users";
                    command.Connection = connection;
                    SqlDataReader usersReader = command.ExecuteReader();
                    while (usersReader.Read())
                    {
                        var id = usersReader["Id"];
                        var name = usersReader["name"];
                        var address = usersReader["address"];
                        Console.WriteLine("User Id={0}, Name={1}, Address={2}", id, name, address);
                    }
                    usersReader.Close();
                }

                void ExecuteNonQuery(string sqlExpression)
                {
                    using (var sqlNonQueryCommand = new SqlCommand())
                    {
                        sqlNonQueryCommand.CommandText = sqlExpression;
                        sqlNonQueryCommand.Connection = connection;
                        sqlNonQueryCommand.ExecuteNonQuery();
                    }
                }

                T ExecuteScalar<T>(string sqlExpression)
                {
                    using (var sqlScalarCommand = new SqlCommand())
                    {
                        sqlScalarCommand.CommandText = sqlExpression;
                        sqlScalarCommand.Connection = connection;
                        return (T)sqlScalarCommand.ExecuteScalar();
                    }
                }

                SqlDataReader ExecuteReader(string sqlExpression)
                {
                    using (var sqlReaderCommand = new SqlCommand())
                    {
                        sqlReaderCommand.CommandText = sqlExpression;
                        sqlReaderCommand.Connection = connection;
                        return sqlReaderCommand.ExecuteReader();
                    }
                }

                ExecuteNonQuery("INSERT INTO Users (Name, Address) VALUES ('Petea', 'Floresti')");
                ExecuteNonQuery("INSERT INTO Users (Name, Address) VALUES ('Gheorghe', 'Nisporeni')");
                ExecuteNonQuery("INSERT INTO Users (Name, Address) VALUES ('Andrei', 'Balti')");

                Console.WriteLine(ExecuteScalar<int>("Select count(*) from users"));

                var collection = ExecuteReader("Select * from users");
                while (collection.Read())
                {
                    Console.WriteLine($"User Id={collection["Id"]}, Name={collection["name"]}, Address={collection["address"]}");
                }
                collection.Close();

                ExecuteNonQuery("Update Users set name = 'Agripina' Where Id = 50");
                ExecuteNonQuery("Delete from Users where name = 'Petea'");

                var collection1 = ExecuteReader("Select * from users");
                while (collection1.Read())
                {
                    Console.WriteLine($"User Id={collection1["Id"]}, Name={collection1["name"]}, Address={collection1["address"]}");
                }
                collection1.Close();
                Console.WriteLine("======================================================================");
                using (var sqlNonQueryCommand = new SqlCommand())
                {
                    sqlNonQueryCommand.CommandText = "Update Users set name = @name1 Where name = @name2";
                    sqlNonQueryCommand.Connection = connection;
                    sqlNonQueryCommand.Parameters.Add("@name1", SqlDbType.NVarChar).Value = "Agripina";
                    sqlNonQueryCommand.Parameters.Add("@name2", SqlDbType.NVarChar);
                    sqlNonQueryCommand.Parameters["@name2"].Value = "Gheorghe";
                    sqlNonQueryCommand.ExecuteNonQuery();
                }
                var collection2 = ExecuteReader("Select * from users");
                while (collection2.Read())
                {
                    Console.WriteLine($"User Id={collection2["Id"]}, Name={collection2["name"]}, Address={collection2["address"]}");
                }
                collection1.Close();

                Console.WriteLine("======================================================================");

                DataTable unNamedTable = new DataTable();

                DataTable usersTable = new DataTable("Users");

                DataColumn idColumn = new DataColumn("Id", typeof(int));
                DataColumn nameColumn = new DataColumn("Name", typeof(string));

                usersTable.Columns.Add(idColumn);
                usersTable.Columns.Add(nameColumn);
                usersTable.Columns.Add("Address", typeof(string));

                DataRow row1 = usersTable.NewRow();
                row1["Id"] = 1000;
                row1["Name"] = "Nicolae";
                row1["Address"] = "Bucuresti";

                usersTable.Rows.Add(row1);

                DataTable ordersTable = new DataTable("Orders");

                DataColumn id1Column = new DataColumn("Id", typeof(int));
                DataColumn userColumn = new DataColumn("UserId", typeof(int));

                ordersTable.Columns.Add(id1Column);
                ordersTable.Columns.Add(userColumn);

                DataRow row2 = ordersTable.NewRow();
                row2["Id"] = 1;
                row2["UserId"] = 1;

                ordersTable.Rows.Add(row2);

                DataSet dataSet = new DataSet("UsersDB");
                dataSet.Tables.Add("Books");
                dataSet.Tables.Add(usersTable);
                dataSet.Tables.Add(ordersTable);
                //dataSet.Relations.Add("UsersOrders",
                //    dataSet.Tables["Users"].Columns["Id"],
                //    dataSet.Tables["Orders"].Columns["UserId"]
                //    );

                Console.WriteLine("======================================================================");

                var sqlCommandText = "SELECT * FROM Users;";

                DataAdapter dataAdapter = new SqlDataAdapter(sqlCommandText, connectionString);

                DataSet dataSet1 = new DataSet();

                dataAdapter.Fill(dataSet1);

                DataTableReader dt = dataSet1.Tables[0].CreateDataReader();
                while (dt.Read())
                {
                    Console.WriteLine($"User Id={dt["Id"]}, Name={dt["name"]}, Address={dt["address"]}");
                }

                Console.WriteLine("======================================================================");

                var insertText = "INSERT INTO Users(name) VALUES(@Name)";
                SqlCommand insertCommand = new SqlCommand(insertText, connection);

                insertCommand.Parameters.Add("@Name", SqlDbType.VarChar, 255, "name");

                DataAdapter dataAdapter1 = new SqlDataAdapter(insertCommand);

                var row3 = usersTable.NewRow();

                row3["Name"] = "Oman";

                usersTable.Rows.Add(row3);
                dataAdapter1.Update(dataSet1);

            }

            ////For asinc connection
            //ConnectWithDB().GetAwaiter();
        }

        ////For asinc connection
        //private static async Task AsincConnectWithDB()
        //{
        //    string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        //    using (SqlConnection connection = new SqlConnection(connectionString))
        //    {
        //        await connection.OpenAsync();
        //    }
        //}

        //public partial class Form1 : Form
        //{
        //    public Form1()
        //    {
        //        InitializeComponent();

        //        string connectionString = @"Data Source=.\SQLEXPRESS;Initial Catalog=usersdb;Integrated Security=True";
        //        string sql = "SELECT * FROM Users";
        //        using (SqlConnection connection = new SqlConnection(connectionString))
        //        {
        //            connection.Open();
        //            
        //            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
        //            
        //            DataSet ds = new DataSet();
        //            
        //            adapter.Fill(ds);
        //            
        //            dataGridView1.DataSource = ds.Tables[0];
        //        }
        //    }
        //}
    }
}
