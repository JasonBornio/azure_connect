using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Threading.Tasks;


public static class AddUser
{
    [FunctionName("AddUser")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        // Read and deserialize request body
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        UserModel user = JsonConvert.DeserializeObject<UserModel>(requestBody);

        // Get connection string from environment variable
        string connectionString = Environment.GetEnvironmentVariable("AzureSqlConnectionString");

        // Use SQL connection to insert data into Azure SQL Database
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            await conn.OpenAsync();
            string query = "INSERT INTO Users (name, email, age) VALUES (@name, @email, @age)";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@name", user.Name);
                cmd.Parameters.AddWithValue("@age", user.Age);
                cmd.Parameters.AddWithValue("@email", user.Email);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        return new OkObjectResult("User added successfully!");
    }
}

public class UserModel
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
}
