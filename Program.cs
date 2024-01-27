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
        Emergency = true,
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
        EmployeeId = null,
        Description = "Jeff needs help promoting his shows.",
        Emergency = false,
        DateCompleted = new DateTime (2021, 09, 22)
    },
    new ServiceTicket
    {
        Id = 5,
        CustomerId = 3,
        EmployeeId = 2,
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

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket updatedServiceTicket) =>
{
    ServiceTicket existingServiceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    existingServiceTicket.EmployeeId = updatedServiceTicket.EmployeeId;

    if (existingServiceTicket == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(existingServiceTicket);

});


app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (ticketToComplete != null)
    {
        ticketToComplete.DateCompleted = DateTime.Today;
        return Results.Ok("Ticket marked complete");
    }
    else
    {
        return Results.NotFound("There are no complete service tickets.");
    }

});

app.MapGet("/pending-emergency-tickets", () =>
{
    List<ServiceTicket> pendingEmergencyTickets = serviceTickets
        .Where(st => st.Emergency && st.DateCompleted == null).ToList();

    if (pendingEmergencyTickets != null)
    {
        return Results.Ok(pendingEmergencyTickets);
    }
    else
    {
        return Results.NotFound("No pending emergency tickets found.");
    }
});

app.MapGet("/servicetickets/unassigned", () =>
{
    List<ServiceTicket> unassigned = serviceTickets
    .Where(st => st.EmployeeId == null).ToList();
    if (unassigned != null)
    {
        return Results.Ok(unassigned);
    }
    else
    {
        return Results.NotFound("There are no unassigned tickets.");
    }
});

app.MapGet("/customers/none-closed-in-year", () =>
{
    List<Customer> noClosesInOverAYear = customers
    .Where(c => serviceTickets
    .Any(st => st.CustomerId == c.Id && (st.DateCompleted == null || st.DateCompleted == DateTime.Now.AddYears(-1))))
    .ToList();

    if (noClosesInOverAYear != null)
    {
        return Results.Ok(noClosesInOverAYear);
    }
    else
    {
        return Results.NotFound();
    }
});

app.MapGet("/employees/available", () =>
{
    List<Employee> availableEmployees = employees
    .Where(e => !serviceTickets.Any(st => st.EmployeeId == e.Id && st.DateCompleted == null))
    .ToList();

    return Results.Ok(availableEmployees);
});

app.MapGet("/employees/{id}/customers", (int id) =>
{
    //Get employee by Id
    Employee employee = employees.FirstOrDefault(e => e.Id == id);

    if (employee == null)
    {
        return Results.NotFound("Employee not found");
    }
    //Gets all service tickets related to the employeeId
    List<ServiceTicket> employeeServiceTickets = serviceTickets.Where(st => st.EmployeeId == id)
    .ToList();

    if (employeeServiceTickets.Count == 0)
    {
        return Results.Ok("There are no service tickets assigned to this employee.");
    }
    //Gets the Ids of all customers in the employeeServiceTickets list with no duplicates
    List<int> customerIds = employeeServiceTickets.Select(st => st.CustomerId).Distinct().ToList();

    //Make a list of the customers whose Ids we grabbed in the last code
    List<Customer> customersAssignedToEmployee = customers.Where(c => customerIds.Contains(c.Id))
    .ToList();

    return Results.Ok(customersAssignedToEmployee);
});

app.MapGet("/employees/employee-of-the-month", () =>
{
    DateTime lastMonth = DateTime.Now.AddMonths(-1);
    Employee employeeOfMonth = employees
    .OrderByDescending(e => serviceTickets
    .Count(st => st.EmployeeId == e.Id && st.DateCompleted.HasValue && st.DateCompleted.Value.Month == lastMonth.Month))
    .FirstOrDefault();

    return Results.Ok(employeeOfMonth);
});

app.MapGet("/servicetickets/review", () =>
{
    List<ServiceTicket> completedTickets = serviceTickets
    .Where(st => st.DateCompleted.HasValue)
    .OrderBy(st => st.DateCompleted)
    .ToList();

    foreach (var ticket in completedTickets)
    {
        ticket.Customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
        ticket.Employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
    }

    return Results.Ok(completedTickets);
});

app.MapGet("/servicetickets/prioritized", () =>
{
    var prioritizedTickets = serviceTickets
       .Where(st => !st.DateCompleted.HasValue)
       .OrderByDescending(st => st.Emergency)
       .ThenBy(st => st.EmployeeId.HasValue)
       .ToList();

    foreach (var ticket in prioritizedTickets)
    {
        ticket.Customer = customers.FirstOrDefault(c => c.Id == ticket.CustomerId);
        ticket.Employee = employees.FirstOrDefault(e => e.Id == ticket.EmployeeId);
    }

    return Results.Ok(prioritizedTickets);
});

app.Run();

