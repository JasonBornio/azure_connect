using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Physio.Function
{
    public class my_new_azure_func_1
    {
        private readonly ILogger<my_new_azure_func_1> _logger;

        public my_new_azure_func_1(ILogger<my_new_azure_func_1> logger)
        {
            _logger = logger;
        }

        [Function("my_new_azure_func_1")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function started processing a request.");

            string requestBody = string.Empty;
            try
            {
                // Log raw request body
                requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                _logger.LogInformation("Received request body: {requestBody}", requestBody);

                // Deserialize request to UserModel and log the result
                UserModel user = JsonConvert.DeserializeObject<UserModel>(requestBody);
                _logger.LogInformation("Deserialized User: Name={name}, Email={email}, Age={age}", user.name, user.email, user.age);

                // Open SQL connection
                _logger.LogInformation("Opening SQL connection...");
                using (SqlConnection conn = new SqlConnection("Server=tcp:ios-swift-physio-server.database.windows.net,1433;Initial Catalog=ios-swift-physio-sql-db;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\";"))
                {
                    await conn.OpenAsync();
                    _logger.LogInformation("SQL connection opened.");

                    // Prepare SQL query
                    string query = "INSERT INTO Users (name, email, age) VALUES (@name, @email, @age)";
                    _logger.LogInformation("Executing query: {query}", query);

                    // Execute SQL command and log parameters
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", user.name);
                        cmd.Parameters.AddWithValue("@age", user.age);
                        cmd.Parameters.AddWithValue("@email", user.email);

                        _logger.LogInformation("Query parameters: Name={name}, Email={email}, Age={age}", user.name, user.email, user.age);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation("Query executed successfully, {rowsAffected} row(s) affected.", rowsAffected);
                    }
                }

                // Return success message
                return new OkObjectResult("User added successfully!");
            }
            catch (JsonException jsonEx)
            {
                // Log JSON deserialization errors
                _logger.LogError(jsonEx, "JSON deserialization failed. Request body: {requestBody}", requestBody);
                return new BadRequestObjectResult("Invalid JSON format.");
            }
            catch (SqlException sqlEx)
            {
                // Log SQL errors
                _logger.LogError(sqlEx, "SQL operation failed.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                // Log any other errors
                _logger.LogError(ex, "An unexpected error occurred.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }

    public class UserModel
    {
        public string name { get; set; }
        public string email { get; set; }
        public int age { get; set; }
    }
}


// { "name": "justin", "email": "jamerson@expodicious.cow", "age": "400" }\

//http://localhost:7071/api/<your-function-name>

// curl -X POST http://localhost:7071/api/my_new_azure_func_1 \
// -H "Content-Type: application/json" \
// -d '{"name": "John", "email": "john@example.com", "age": "30"}'