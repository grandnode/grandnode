

   --------------------------------------------------------------------------------
                   README file for Web Markup Minifier: Core v2.3.0

   --------------------------------------------------------------------------------

           Copyright (c) 2013-2017 Andrey Taritsyn - http://www.taritsyn.ru


   ===========
   DESCRIPTION
   ===========
   The Web Markup Minifier (abbreviated WebMarkupMin) is a .NET library that
   contains a set of markup minifiers. The objective of this project is to improve
   the performance of web applications by reducing the size of HTML, XHTML and XML
   code.

   WebMarkupMin absorbed the best of existing solutions from non-microsoft
   platforms: Juriy Zaytsev's Experimental HTML Minifier
   (http://github.com/kangax/html-minifier/) (written in JavaScript) and Sergiy
   Kovalchuk's HtmlCompressor (http://code.google.com/p/htmlcompressor/) (written
   in Java).

   Minification of markup produces by removing extra whitespaces, comments and
   redundant code (only for HTML and XHTML). In addition, HTML and XHTML minifiers
   supports the minification of CSS code from style tags and attributes, and
   minification of JavaScript code from script tags, event attributes and
   hyperlinks with javascript: protocol. WebMarkupMin.Core contains built-in
   JavaScript minifier based on the Douglas Crockford's JSMin
   (http://github.com/douglascrockford/JSMin) and built-in CSS minifier based on
   the Mads Kristensen's Efficient stylesheet minifier
   (http://madskristensen.net/post/Efficient-stylesheet-minification-in-C).
   The above mentioned minifiers produce only the most simple minifications of
   CSS and JavaScript code, but you can always install additional modules that
   support the more powerful algorithms of minification: WebMarkupMin.MsAjax
   (contains minifier-adapters for the Microsoft Ajax Minifier -
   http://ajaxmin.codeplex.com), WebMarkupMin.Yui (contains minifier-adapters
   for YUI Compressor for .Net - http://github.com/PureKrome/YUICompressor.NET)
   and WebMarkupMin.NUglify (contains minifier-adapters for the NUglify -
   http://github.com/xoofx/NUglify).

   Also supports minification of views of popular JavaScript template engines:
   KnockoutJS, Kendo UI MVVM and AngularJS 1.X.

   =============
   RELEASE NOTES
   =============
   1. Added support of .NET Core 1.0.3;
   2. Downgraded .NET Framework version from 4.5.1 to 4.5;
   3. Fixed a error #31 “Perfomance is very slow when a HTML comment is inside a
      JavaScript block”;
   4. Fixed a error in `SourceCodeNavigator` class.

   =============
   DOCUMENTATION
   =============
   See documentation on GitHub - http://github.com/Taritsyn/WebMarkupMin/wiki