using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Grand.Web.Tests
{
    public static class AssertExtentions
    {
        public static void IsType<T>(this Assert assert, Object obj)
        {
            if (obj is T)
                return;
            throw new AssertFailedException("Type does not match");
        }
    }
}
