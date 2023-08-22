using System.Text.Json.Serialization;
using HoneyRaesAPI.Models;
using Microsoft.AspNetCore.Http.Json;

// with "using HoneyRaesAPI.Models;" above, we can shorten this to the statements below (List<Customer> customers = new List<Customer> { };)
// List<HoneyRaesAPI.Models.Customer> customers = new List<HoneyRaesAPI.Models.Customer> { };
// List<HoneyRaesAPI.Models.Employee> employees = new List<HoneyRaesAPI.Models.Employee> { };
// List<HoneyRaesAPI.Models.ServiceTicket> serviceTickets = new List<HoneyRaesAPI.Models.ServiceTicket> { };

List<Customer> customers = new List<Customer>
{
    new Customer()
    {
        Id = 1,
        Name = "Bob Bobertson",
        Address = "123 Main Street"
    },
    new Customer()
    {
        Id = 2,
        Name = "John Johnson",
        Address = "456 1st Avenue"
    },
    new Customer()
    {
        Id = 3,
        Name = "Adam Adamson",
        Address = "789 9th Circle"
    }
};

List<Employee> employees = new List<Employee>
{
    new Employee()
    {
        Id = 1,
        Name = "Alice N. Wonderland",
        Specialty = "Glasswork"
    },
    new Employee()
    {
        Id = 2,
        Name = "White Rabbit",
        Specialty = "Clocks and Watches"
    }
};


List<ServiceTicket> serviceTickets = new List<ServiceTicket>
{
    new ServiceTicket()
    {
        Id = 1,
        CustomerId = 1,
        // EmployeeId = 1,
        Description = "Broken bathroom mirror",
        Emergency = false
    },
    new ServiceTicket()
    {
        Id = 2,
        CustomerId = 1,
        EmployeeId = 2,
        Description = "Grandfather clock needs new weights",
        Emergency = false,
        DateCompleted = new DateTime(2023, 08, 01)
    },
    new ServiceTicket()
    {
        Id = 3,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Broken sliding glass door",
        Emergency = true
    },
    new ServiceTicket()
    {
        Id = 4,
        CustomerId = 3,
        // EmployeeId = 2,
        Description = "City clock is broken",
        Emergency = true
    },
    new ServiceTicket()
    {
        Id = 5,
        CustomerId = 2,
        EmployeeId = 1,
        Description = "Glass conference table top cracked",
        Emergency = false,
        DateCompleted = new DateTime(2023, 08, 15)
    }
};



var builder = WebApplication.CreateBuilder(args);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast = Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateTime.Now.AddDays(index),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

// app.MapGet("/hello", () =>
// {
//     return "hello";
// });


// GET request to http://localhost:<port>//servicetickets => returns all service tickets in our database, ASP.NET code turns C# list of objects into JSON text and sends HTTP response with that data in the body
app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

// GET request to http://localhost:<port>//servicetickets/# => returns a service ticket in our database, ASP.NET code turns C# list of objects into JSON text and sends HTTP response with that data in the body
app.MapGet("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);
});

app.MapGet("customers", () =>
{
    return customers;
});

app.MapGet("customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);
});

// We are using the MapPost method to create this endpoint, because it should be triggered when a POST request is made to the API.
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Count > 0 ? serviceTickets.Max(st => st.Id) + 1 : 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

// MapDelete to delete a service ticket
app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket ticketToDelete = serviceTickets.FirstOrDefault(st => st.Id == id);
    serviceTickets.Remove(ticketToDelete);
}
);

// MapPut to update a service ticket // HTTP request with id to update and the object to replace
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    // ticketToUpdate is the one that matches the id
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    // ticketIndex = the index of the ticketToUpdate
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    // if not found (First or Default/Null)
    if (ticketToUpdate == null)
    {
        // return results not found
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    // replace the data with our ticketToUpdate/tickIndex data
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

// Complete a ticket
app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    // Next, get the service ticket from the database that needs to be marked as complete:
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    // Finally, set the service ticket's DateCompleted to today:
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.Run();

// record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }

