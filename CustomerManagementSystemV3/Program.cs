using CustomerManagementSystemV3.Data;
using CustomerManagementSystemV3.Models;
using Dapper;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    string connectionString = configuration.GetConnectionString("DBConnection") ?? throw new ApplicationException("The connection string is null.");
    return new DBAccesser(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/get-customers-list", (DBAccesser dBAccesser) =>
{
    using (var connection = dBAccesser.CreateDBConnection())
    {
        const string sql = "SELECT Id, FirstName, LastName, Email, DateOfBirth FROM Customer";
        IEnumerable<Customer> customers = connection.Query<Customer>(sql);
        return Results.Ok(customers);
    }
});

app.MapGet("/get-customer/{id}", (int id, DBAccesser dBAccesser) =>
{
    using (var connection = dBAccesser.CreateDBConnection()) 
    {
        string sql = $"SELECT Id, FirstName, LastName, Email, DateOfBirth FROM Customer WHERE Id = {id}";
        var customer = connection.Query<Customer>(sql);
        return Results.Ok(customer);
    }
});

app.MapPost("/create-customer", (CustomerDto customer, DBAccesser dBAccesser) =>
{
    using (var connection = dBAccesser.CreateDBConnection()) 
    {
        string sql = @" INSERT INTO Customer(FirstName, LastName, Email, DateOfBirth) VALUES (@FirstName, @LastName, @Email, @DateOfBirth)";
        var parameters = new DynamicParameters();
        parameters.Add("FirstName", customer.FirstName, DbType.String);
        parameters.Add("LastName", customer.LastName, DbType.String);
        parameters.Add("Email", customer.Email, DbType.String);
        parameters.Add("DateOfBirth", customer.DateOfBirth, DbType.DateTime);
        var customerAdd = connection.Execute(sql, parameters);
        return Results.Ok(customerAdd);
    }
});

app.MapPut("/update-customer/{id}", (int id, DBAccesser dBAccesser, CustomerDto customerDto) =>
{
    using (var connection = dBAccesser.CreateDBConnection()) 
    {
        string existing = $@" SELECT * FROM Customer WHERE Id = '{id}'";
        var existingCustomers = connection.Query<Customer>(existing);

        if (existingCustomers.Count() == 0)
        {
            return Results.NotFound("This Id value is not found");
        }

        if (existingCustomers.Count() > 1)
        {
            return Results.BadRequest("This Id value found more than one");
        }
        string sql = $@" UPDATE Customer SET FirstName = @FirstName, LastName = @LastName, Email = @Email, DateOfBirth = @DateOfBirth WHERE Id = '{id}'";
        var parameters = new DynamicParameters();
        parameters.Add("FirstName", customerDto.FirstName, DbType.String);
        parameters.Add("LastName", customerDto.LastName, DbType.String);
        parameters.Add("Email", customerDto.Email, DbType.String);
        parameters.Add("DateOfBirth", customerDto.DateOfBirth, DbType.DateTime);
        var customerUpdate = connection.Execute(sql, parameters);
        return Results.Ok(customerUpdate);
    }
});

app.MapDelete("/delete-customer/{id}", (int id, DBAccesser dBAccesser) =>
{
    using (var connection = dBAccesser.CreateDBConnection()) 
    {
        string existing = $@"SELECT * FROM Customer WHERE Id = '{id}'";
        var existingCustomers = connection.Query<Customer>(existing);

        if (existingCustomers.Count() == 0)
        {
            return Results.NotFound("This Id value is not found");
        }

        if (existingCustomers.Count() > 1)
        {
            return Results.BadRequest("This Id value found more than one");
        }

        string sql = $@"DELETE FROM Customer WHERE Id = {id}";
        var customers = connection.Execute(sql);
        return Results.Ok(customers);
    }
});

app.Run();
