

   --------------------------------------------------------------------------------
               README file for Web Markup Minifier: ASP.NET Common v2.3.0

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2017 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   WebMarkupMin.AspNet.Common is auxiliary package, that contains classes and
   interfaces for all ASP.NET extensions.

   =============
   RELEASE NOTES
   =============
   1. Added support of .NET Core 1.0.3;
   2. Downgraded .NET Framework version from 4.5.1 to 4.5;
   3. In `IMarkupMinificationManager` interface was added a new property -
      `GenerateStatistics` (default `false`);
   4. From `IHttpCompressionManager` interface was removed `IsSupportedMediaType`
      method;
   5. In `IHttpCompressionManager` interface was added a new property -
      `SupportedMediaTypePredicate` (default `null`).

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub - http://github.com/Taritsyn/WebMarkupMin/wiki