#### 1.0.0 November 14th 2018 ####
HOCON 1.0.0 is a complete and total rewrite of the HOCON parsing engine, designed to work with both the .NET Framework (4.5 and later) and .NET Standard 1.3.

The performance has significantly improved, as have the range of features supported by this library, including:
* Built-in enviroment variable substitution;
* [Support for better byte and time formats](https://github.com/akkadotnet/akka.net/pull/3600);
* [Improved object substitution and merging](https://github.com/akkadotnet/HOCON/pull/55);
* and many more features.

This library will be developed and released independently from Akka.NET itself, but beginning with Akka.NET v1.4 it will be taken as a dependency by the core Akka libraries.

#### 0.1.0 November 9th 2017 ####
* .NET Standard 1.2 and .NET 4.5 dual support for core HOCON library.
* Dropped dependency on JSON.NET.
* Separated HOCON from Akka conceptually and in terms of namespaces.