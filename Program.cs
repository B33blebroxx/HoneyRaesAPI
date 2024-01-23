using HoneyRaesAPI.Models;

List<Customer> customers = new()
{
  new Customer
  {
      Id = 1,
      Name = "Jeff Cobb",
      Address = "123 Fake St."
  },
  new Customer
  {
      Id = 2,
      Name = "Kenny Omega",
      Address = "321 Unreal Rd."
  },
  new Customer
  {
      Id = 3,
      Name = "Adam Page",
      Address = "1010 Virginia Ave."
  }
};

List<Employee> employees = new()
{
    new Employee
    {
        Id = 1,
        Name = "Kazuchika Okada",
        Specialty = "Money"
    },
    new Employee
    {
        Id = 2,
        Name = "Kota Ibushi",
        Specialty = "Athletics"
    }
};

List<ServiceTicket> serviceTickets = new()
{
    new ServiceTicket
    {
        Id = 1,
        CustomerId = 3,
        EmployeeId = 1,
        Description = "Adam needs money help.",
        Emergency = false,
        DateCompleted = null
    },
    new ServiceTicket
    {
        Id = 2,
        CustomerId = 1,
        EmployeeId = null,
        Description = "Jeff needs help with athletic training.",
        Emergency = true,
        DateCompleted = null
    },
    new ServiceTicket
    {
        Id = 3,
        CustomerId = 2,
        EmployeeId = 2,
        Description = "Kenny needs a tag team partner.",
        Emergency = false,
        DateCompleted = new DateTime (2009, 01, 20)
    },
    new ServiceTicket
    {
        Id = 4,
        CustomerId = 1,
        EmployeeId = 1,
        Description = "Jeff needs help promoting his shows.",
        Emergency = false,
        DateCompleted = new DateTime (2021, 09, 22)
    },
    new ServiceTicket
    {
        Id = 5,
        CustomerId = 3,
        EmployeeId = null,
        Description = "Adam needs a new gimmick.",
        Emergency = true,
        DateCompleted = new DateTime (2011, 11, 11)
    }
};

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/serviceTickets", () =>
{
    return serviceTickets;
});

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

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(c => c.Id == id);
    if (customer == null)
    {
        return Results.NotFound();
    }
    customer.ServiceTicket = serviceTickets.FirstOrDefault(st => st.CustomerId == id);
    return Results.Ok(customer);
});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket != null)
    {
        serviceTickets.Remove(serviceTicket);
        return Results.Ok("Success!");
    }
    else
    {
        return Results.NotFound();
    }
});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.Run();

