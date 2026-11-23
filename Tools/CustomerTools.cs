using System.ComponentModel;
using Crm.Models;
using Crm.Services;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace Crm.Tools;

// MCP tools for customers. Like the API endpoints, the service comes from DI.
// The Description texts are what the AI reads to choose a tool, so they matter.
// A McpException is sent back to the AI as an error it can read and correct.
[McpServerToolType]
public static class CustomerTools
{
    [McpServerTool, Description("Search customers by free text (name, email or company) and/or status. Returns all customers if no filter is given.")]
    public static List<Customer> SearchCustomers(
        ICustomerService customerService,
        [Description("Text to search for in name, email and company")] string? text = null,
        [Description("Lead, Active or Inactive")] string? status = null)
    {
        return customerService.Search(text, status);
    }

    [McpServerTool, Description("Get one customer by id. Returns null if the customer does not exist.")]
    public static Customer? GetCustomer(
        ICustomerService customerService,
        [Description("Customer id")] int id)
    {
        return customerService.GetById(id);
    }

    [McpServerTool, Description("Create a new customer. Returns the customer with its new id.")]
    public static Customer AddCustomer(
        ICustomerService customerService,
        [Description("Full name of the contact person")] string name,
        [Description("Email address")] string email,
        [Description("Lead, Active or Inactive")] string status = "Lead",
        [Description("Phone number")] string? phone = null,
        [Description("Company name")] string? company = null,
        [Description("Free text notes")] string? notes = null)
    {
        var customer = new Customer
        {
            Name = name,
            Email = email,
            Status = status,
            Phone = phone,
            Company = company,
            Notes = notes
        };

        string error = customer.Validate();
        if (error != "")
            throw new McpException(error);

        customerService.Add(customer);
        return customer;
    }

    [McpServerTool, Description("Create many customers in one call. Use this for test data instead of calling AddCustomer many times. Id and Created are set by the server. Nothing is saved if any customer is invalid.")]
    public static List<Customer> AddCustomers(
        ICustomerService customerService,
        [Description("The customers to create")] List<Customer> customers)
    {
        // Check all first, so either all or none are saved
        var errors = customers
            .Select((c, index) => (c, index, error: c.Validate()))
            .Where(x => x.error != "")
            .Select(x => $"#{x.index} ({x.c.Name}): {x.error}")
            .ToList();

        if (errors.Count > 0)
            throw new McpException("Invalid customers, nothing saved:\n" + string.Join("\n", errors));

        foreach (var customer in customers)
            customerService.Add(customer);

        return customers;
    }

    [McpServerTool, Description("Update an existing customer. All fields are replaced. Returns false if the customer does not exist.")]
    public static bool UpdateCustomer(
        ICustomerService customerService,
        [Description("The customer with updated values. Id must be an existing customer")] Customer customer)
    {
        string error = customer.Validate();
        if (error != "")
            throw new McpException(error);

        return customerService.Update(customer);
    }

    [McpServerTool, Description("Delete a customer and all its activities. Cannot be undone. Returns false if the customer does not exist.")]
    public static bool DeleteCustomer(
        ICustomerService customerService,
        IActivityService activityService,
        [Description("Customer id")] int id)
    {
        if (!customerService.Delete(id)) return false;

        activityService.DeleteByCustomer(id);
        return true;
    }
}