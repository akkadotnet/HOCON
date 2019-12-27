#### 1.2.1 December 27 2019 ####
HOCON 1.2.1 contains many minor bug fixes and behavioral changes.

**Default HOCON loading order**
Per [issue 151](https://github.com/akkadotnet/HOCON/issues/151), `HOCON.Configuration` now looks for default HOCON content in the following places in the following order:

1. [.NET Core / .NET Framework] An "app.conf" or an "app.hocon" file in the current working directory of the executable when it loads;
2. [.NET Framework] - the `<hocon>` `ConfigurationSection` inside `App.config` or `Web.config`, which should also resolve #8 and #9 
3. [.NET Framework] - and a legacy option, to load the old `<akka>` HOCON section for backwards compatibility purposes with all users who have been using HOCON with Akka.NET.

**Bug fixes**:
For a set of complete bug fixes and changes, please see [the HOCON v1.2.1 milestone on Github](https://github.com/akkadotnet/HOCON/milestone/2).