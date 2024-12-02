# OSK.Extensions.Serialization.SystemTextJson
This library adds a custon json converter object to a dependency container using the `AddSystemTextJsonPolymorphism` extension. Additionally, the 
converter is a public object and be created where ever it needs to. The converter relies on the conventions and logic setup in `OSK.Serialization.Polymorphism`
and provides a way to parse JSON containing abstract data objects based on a provided polymorphism strategy.

# Contributions and Issues
Any and all contributions are appreciated! Please be sure to follow the branch naming convention OSK-{issue number}-{deliminated}-{branch}-{name} as current workflows rely on it for automatic issue closure. Please submit issues for discussion and tracking using the github issue tracker.