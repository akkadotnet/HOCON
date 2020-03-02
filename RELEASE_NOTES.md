### 3.0.0 March 02 2020 ####
Major changes on the HOCON surface API

* Major restructure of HOCON public class and API to conform to HOCON immutability 
* Most of the API are backward compatible with HOCON 2.0 with the following exceptions:
  * All access APIs are declared in static extension classes, in order to access the values
    inside a `Config` or `HoconObject`, you will need to declare `using Hocon` in your .cs file.
  * The class structure of the public HOCON classes havee beeen simplified and a few have been
    removed. If you referenced the `HoconValue` or `HoconField` classes in your code, you will
    have to change your code to conform to the new API.