#### 1.1.0 July 10 2019 ####
HOCON 1.1.0 contains a large number of bug fixes and improvements over HOCON 1.0.0, including:

* [Fix: fully qualified type name with assembly cant be used as property name ](https://github.com/akkadotnet/HOCON/issues/79)
* [Fix: unable to use include keyword at root level](https://github.com/akkadotnet/HOCON/pull/69)
* [Fix. Child field path was not concatenated with its parent path](https://github.com/akkadotnet/HOCON/pull/96)
* [Fix: bug when getting a string with a substitution as an array element](https://github.com/akkadotnet/HOCON/pull/88)
* [Fix: Object fields containing objects may not return proper object value in certain circumstances. ](https://github.com/akkadotnet/HOCON/pull/93)

#### 1.0.0 November 14th 2018 ####
HOCON 1.0.0 is a complete and total rewrite of the HOCON parsing engine, designed to work with both the .NET Framework (4.5 and later) and .NET Standard 1.3.

The performance has significantly improved, as have the range of features supported by this library, including:
* Built-in enviroment variable substitution;
* [Support for better byte and time formats](https://github.com/akkadotnet/akka.net/pull/3600);
* [Improved object substitution and merging](https://github.com/akkadotnet/HOCON/pull/55);
* and many more features.

This library will be developed and released independently from Akka.NET itself, but beginning with Akka.NET v1.4 it will be taken as a dependency by the core Akka libraries.

HOCON ships as two NuGet packages:
* `Hocon` - includes all core parser, substitution, tokenization, and other functionality;
* `Hocon.Configuration` - adds `App.config` and `System.Configuration` integration to HOCON.
* `Hocon.Extensions.Configuration` - adds HOCON support for the popular `Microsoft.Extensions.Configuration` library used in ASP.NET Core and other parts of the new .NET Core runtime.

#### 0.1.0 November 9th 2017 ####
* .NET Standard 1.2 and .NET 4.5 dual support for core HOCON library.
* Dropped dependency on JSON.NET.
* Separated HOCON from Akka conceptually and in terms of namespaces.