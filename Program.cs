using Marten;
using Marten.Patching;
using marten_docs_pii;

await using var store = DocumentStore.For(opts =>
{
    opts.Connection("Host=localhost;Database=marten_testing;Username=postgres;Password=postgres");
    opts.UseMaskingRulesForProtectedInformation();
    opts.Schema.For<Person>()
        .AddMaskingRuleForProtectedInformation(
            x => x.Name, 
            "***"
        )
        .AddMaskingRuleForProtectedInformation(
            x => x.Phone, 
            "###-###"
        )
        .AddMaskingRuleForProtectedInformation(
            x => x.Address.Street, 
            "***"
        );
});

await using var session = store.LightweightSession();

// Create and store a person
var person1 = new Person(
    Guid.NewGuid(), 
    "John Doe", 
    "111-111", 
    new Address("123 Main St", "Anytown"));

var person2 = new Person(
    Guid.NewGuid(), 
    "Some Name", 
    "222-222", 
    new Address("Some Street", "Some City"));

session.Store(person1, person2);
await session.SaveChangesAsync();

session.Patch<Person>(x => true).ApplyMaskForProtectedInformation();
await session.SaveChangesAsync();

// Query back to verify
var maskedPerson = await session.LoadAsync<Person>(person1.Id);
Console.WriteLine($"Name: {maskedPerson?.Name}");
Console.WriteLine($"Phone: {maskedPerson?.Phone}");
Console.WriteLine($"Street: {maskedPerson?.Address.Street}, City: {maskedPerson?.Address.City}");

public record Address(string Street, string City);
public record Person(Guid Id, string Name, string Phone, Address Address);