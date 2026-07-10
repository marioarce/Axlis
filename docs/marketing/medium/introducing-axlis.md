# When Sitecore Headless Meets the Real World

We were migrating a Sitecore project to a new technology stack. The architecture decision was clear: API-centric, with multiple heterogeneous datasources. One of those datasources had to be Sitecore.

After technical discussions, we chose Sitecore Headless with the GraphQL endpoint for read-only content access. The new WebAPI stack couldn't include the Sitecore API due to NuGet package dependencies, so GraphQL was the path forward.

## The Gap

We were accustomed to working with Sitecore ORMs like Synthesis or GlassMapper. When we researched what was available for Sitecore Headless GraphQL, we found a gap.

We would need to develop everything from scratch: the GraphQL connection, the ORM system, the POCOs, and the infrastructure to work with strongly-typed objects. This was low-level work—boilerplate that keeps developers from focusing on business logic.

As a software architect, I prefer to avoid boilerplate, reduce repetitive code, extract common logic, and let developers concentrate on business problems rather than structural plumbing.

## The Solution

I designed an ORM for GraphQL.

After many months of development, several years in production, and multiple development cycles including QA, the solution matured. I realized the Sitecore community might have the same need.

That's why I created Axlis.

## What Axlis Is

Axlis is a Sitecore Headless GraphQL ORM for .NET 8. It provides a strongly-typed, Synthesis-style item model built on raw `HttpClient` and `System.Text.Json`. No third-party GraphQL library required.

It's part of the PowerCSharp ecosystem and integrates with `PowerCSharp.Feature.Cache` for stampede-safe item caching.

## Why This Approach

The Synthesis-like API meant developers with Sitecore ORM experience found Axlis natural. They could apply existing patterns to the headless world.

Using PowerCSharp helped avoid boilerplate and provided a robust cache mechanism, making the ORM fast, efficient, and maintainable.

## Security and Dependencies

Avoiding third-party GraphQL libraries had practical benefits. Security audits passed without vulnerabilities—no unexpected dependencies in the supply chain.

The design keeps the ORM agnostic to the GraphQL transport. While it currently uses Sitecore's GraphQL endpoint, the architecture supports other transports, including planned support for the Sitecore API itself to facilitate legacy system migrations.

## The Third-Party Lesson

At one point, we used a third-party GraphQL client. It brought problems under high load—particularly with large amounts of Sitecore content. It also locked us into generated code that was difficult to version and limited our query capabilities.

We removed it. The new implementation gives us full control over the ORM, including custom GraphQL queries. This improved both control and performance.

## The Architecture

Axlis is structured as a family of layered packages, each independently installable:

- **Axlis.Abstractions**: Contracts only. Safe to reference from any layer.
- **Axlis.Core**: The domain model, field types, and item conversion.
- **Axlis.GraphQL**: The default HttpClient-based transport.
- **Axlis**: The top-level facade, cache manager, and DI wiring.

This separation means you reference only what you need.

## What It Looks Like

Define a template POCO:

```csharp
[SitecoreTemplate("{6D1CD897-1936-4A3A-A511-289A94C2A7B1}")]
public class DictionaryEntry : ExtendedItem
{
    [SitecoreField("Key")]
    public TextField Key => GetField<TextField>("Key");

    [SitecoreField("Phrase")]
    public TextField Phrase => GetField<TextField>("Phrase");
}
```

Fetch an item:

```csharp
var item = await facade.GetItemByPathAsync<DictionaryEntry>("/sitecore/content/dictionary/my-key");
Console.WriteLine(item?.Phrase.RawValue);
```

Traverse the content tree:

```csharp
var children = item?.Axes.Children;
var parent   = item?.Axes.Parent;
var siblings = item?.Axes.Siblings;
```

## Production-Tested

This isn't theoretical. Axlis is working in production-ready projects, handling real traffic and real content volumes. The performance improvements from removing the third-party client were measurable.

## Sharing With the Community

I'm sharing Axlis because the Sitecore community might face the same gap we did. When you move to Sitecore Headless and need strongly-typed access to content, you shouldn't have to build the infrastructure from scratch.

The goal is simple: let developers focus on business logic, not GraphQL plumbing.

---

If you're working with Sitecore Headless and need a strongly-typed ORM, Axlis might be what you're looking for. It's battle-tested, dependency-light, and designed to keep developers productive.

The code is on GitHub. The packages are on NuGet. I'd welcome your feedback.
