using System;
using System.Collections.Generic;

namespace TaskFlow.Infrastructure.Persistence.Seed_Data
{
    public static class SeedData
    {
        internal record SeedWorkOrder(
            string Code,
            string Title,
            string Status,
            string Priority,
            string AssignedTo,
            DateTime DueDate
        );

        internal static IReadOnlyList<SeedWorkOrder> GetDemoWorkOrders()
        {
            var now = DateTime.UtcNow.Date;
            return new[]
            {
                new SeedWorkOrder(
                    Code: "wo-100",
                    Title: "Replace air filter",
                    Status: "Open",
                    Priority: "High",
                    AssignedTo: "tech.alex",
                    DueDate: now.AddDays(3)
                ),
                new SeedWorkOrder(
                    Code: "wo-101",
                    Title: "Calibrate thermostat",
                    Status: "In Progress",
                    Priority: "Medium",
                    AssignedTo: "tech.sam",
                    DueDate: now.AddDays(5)
                ),
                new SeedWorkOrder(
                    Code: "wo-102",
                    Title: "Inspect ducting",
                    Status: "On Hold",
                    Priority: "Low",
                    AssignedTo: "tech.maya",
                    DueDate: now.AddDays(10)
                ),
                new SeedWorkOrder(
                    Code: "wo-103",
                    Title: "Install smart meter",
                    Status: "Completed",
                    Priority: "High",
                    AssignedTo: "tech.alex",
                    DueDate: now.AddDays(-2) // due in the past
                )
            };
        }
    }
}
