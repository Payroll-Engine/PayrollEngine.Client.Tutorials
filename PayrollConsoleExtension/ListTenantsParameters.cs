using System;
using PayrollEngine.Client.Command;

namespace PayrollEngine.Client.Tutorial.PayrollConsoleExtension;

public class ListTenantsParameters : ICommandParameters
{
    public Order Order { get; private init; } = Order.IdentifierAscending;

    public Type[] Toggles =>
    [
        typeof(Order)
    ];

    public string Test() => null;

    public static ListTenantsParameters ParserFrom(CommandLineParser parser) =>
        new()
        {
            Order = parser.GetEnumToggle(Order.IdentifierAscending)
        };
}