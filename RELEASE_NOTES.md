### 2.0.4 October 14 2021 ####
* [Change config file load priority](https://github.com/akkadotnet/HOCON/issues/260)
* [Bugfix: Performance issue reported in #276](https://github.com/akkadotnet/HOCON/issues/277)
* [Bugfix: Quoted string should be closed by the same starting token](https://github.com/akkadotnet/HOCON/issues/278)
* [Bugfix: Flatten did not flatten all the way down](https://github.com/akkadotnet/HOCON/issues/280)
* [Bugfix: Escape backslash when serializing](https://github.com/akkadotnet/HOCON/issues/351)

- `HoconConfigurationFactory.Default()` will try and load HOCON configuration from default named files based on this priority:
  1. application.conf
  2. application.hocon
  3. application.properties
  4. app.conf
  5. app.hocon
  6. app.properties

- If `HoconConfigurationFactory.FromFile()` is called without any file extensions, it will try to load the file using these extensions: .conf, .hocon, and finally .properties 

### 2.0.3 February 28 2020 ####
Bugfixes and improvements made to HOCON 2.0.0

* [Bugfix: HoconValue.TryGetObject can throw System.ArgumentNullException](https://github.com/akkadotnet/HOCON/issues/233)
* [Bugfix: System.InvalidOperationException : Collection was modified; enumeration operation may not execute](https://github.com/akkadotnet/HOCON/issues/234)
* [Bugfix: unquoted key error during serialization](https://github.com/akkadotnet/HOCON/pull/223)
* [Fixed NuGet symbol package publication](https://github.com/akkadotnet/HOCON/issues/222)

For more details, please [see the issues in the HOCON v2.0.1 milestone here](https://github.com/akkadotnet/HOCON/milestone/7).