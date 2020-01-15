#### 1.3.0 January 14 2020 ####
HOCON 1.3.0 contains some significant API changes:

* [API parity with pre-existing Akka.NET HOCON implementation](https://github.com/akkadotnet/HOCON/issues/157)
* Added `HoconType.String`, `HoconType.Number`, `HoconType.Bool`, and removed `HoconType.Literal` - now it's possible to discover data types more easily while inspecting individual HOCON objects.
* [Fixed: Need to be able to include Config fallback values to string representation](https://github.com/akkadotnet/HOCON/issues/161)
* [Added SourceLink.Github support](https://github.com/akkadotnet/HOCON/pull/166)

For a set of complete bug fixes and changes, please see [the HOCON v1.3.0 milestone on Github](https://github.com/akkadotnet/HOCON/milestone/3).