### 2.0.0 February 13 2020 ####

* [Add JSON serialization by implementing `ISerializable`.](https://github.com/akkadotnet/HOCON/pull/214)
* [Resolve `Config` containing a single empty object as an empty `Config`.](https://github.com/akkadotnet/HOCON/pull/214)
* [Refactor `Hocon.Parser` class to `Hocon.HoconParser` for clarity.](https://github.com/akkadotnet/HOCON/pull/215)
* [Add `IEquitable<Config>` interface implementation to `Config` class.](https://github.com/akkadotnet/HOCON/pull/218)
* [Change `GetString()`, `GetStringList()`, `GetInt()`, and `GetDouble()` throws on failure instead of returning a default value to conform to Hocon spec.](https://github.com/akkadotnet/HOCON/pull/218)
* [Add `TryGet[DataType]()` functions to all getters to eliminate as much Exception throwing as possible to improve performance.](https://github.com/akkadotnet/HOCON/pull/218)
* [Remove as much wrapping functions as possible to improve performance.](https://github.com/akkadotnet/HOCON/pull/218)
* [Move `ConfigurationException` from `Akka.Configuration` to `Hocon`](https://github.com/akkadotnet/HOCON/pull/218)
* [Fix `Config.WithFallback()` with hocon files containing substitution failed to merge the fallbacks correctly..](https://github.com/akkadotnet/HOCON/pull/218)
* [Hocon is now compatible with Hyperion serializer.](https://github.com/akkadotnet/HOCON/pull/218)
