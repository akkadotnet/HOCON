//-----------------------------------------------------------------------
// <copyright file="HoconAPISpecConfig.cs" company="Akka.NET Project">
//     Copyright (C) 2013-2019 .NET Foundation <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using ApprovalTests.Reporters;
#if(DEBUG)
[assembly: UseReporter(typeof(DiffReporter), typeof(AllFailingTestsClipboardReporter))]
#else
[assembly: UseReporter(typeof(DiffReporter))]
#endif
