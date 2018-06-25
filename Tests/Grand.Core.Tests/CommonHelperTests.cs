using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Grand.Core.Tests
{
    [TestClass()]
    public class CommonHelperTests {
        [TestMethod()]
        public void EnsureSubscriberEmailOrThrowTest() {
            string[] allPossibleInvalidValues = {
                null,
                "qwert@.pl",
                "@asd.pl",
                "1111qwert#222.pl",
                "qwe@#(@*$@!@asd.pl",
                "qwe rt@asd.pl",
                "qweqwqew@gmail.pl", //valid email don't throw exception
                "q,wert@asd.pl",
                "qwert@as_d.pl"};

            try {
                foreach (string str in allPossibleInvalidValues) {
                    CommonHelper.EnsureSubscriberEmailOrThrow(str);
                }
            }
            catch (GrandException ex) {
                Assert.AreEqual("Email is not valid.", ex.Message);
            }
        }

        [TestMethod()]
        public void IsValidEmailTest() {
            //try invalid strings
            string invalidEmail = null; //null
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = ""; //empty string
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "qwert@.pl"; //no server name
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "@asd.pl"; //no user name
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "1111qwert#222.pl"; //without @
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "qwe@#(@*$@!@asd.pl"; //special characters               
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "qwe rt@asd.pl"; //white space
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "qwert@asd.pl33"; //numbers on end                       !
            //Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "q,wert@asd.pl"; //comma
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            invalidEmail = "qwert@as_d.pl"; //underscore in server name             #1
            Assert.IsFalse(CommonHelper.IsValidEmail(invalidEmail));

            //try valid strings
            string validEmail = "wer++tyu@aasd.pl"; //+ sign
            Assert.IsTrue(CommonHelper.IsValidEmail(validEmail));

            validEmail = "DFGHJDSA1231sdas321SDD@DDDDSDDD.COM"; //upper case
            Assert.IsTrue(CommonHelper.IsValidEmail(validEmail));

            validEmail = "q_wert@asd.pl"; //underscore in user name                 #1
            Assert.IsTrue(CommonHelper.IsValidEmail(validEmail));
        }

        [TestMethod()]
        public void GenerateRandomDigitCodeTest() {
            //the same number of digits
            Assert.AreEqual(123, CommonHelper.GenerateRandomDigitCode(123).Length);
        }

        [TestMethod()]
        public void GenerateRandomIntegerTest() {
            try {
                CommonHelper.GenerateRandomInteger(100, 10); //range 100-10
            }
            catch (Exception ex) {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), ex.GetType());
            }
        }

        [TestMethod()]
        public void EnsureMaximumLengthTest() {
            Assert.IsNull(CommonHelper.EnsureMaximumLength(null, 9));
            Assert.AreEqual("09letters", CommonHelper.EnsureMaximumLength("09letters", 9));
            Assert.AreEqual("09letters", CommonHelper.EnsureMaximumLength("09letters", 9, "_added"));
            Assert.AreEqual("09_added", CommonHelper.EnsureMaximumLength("09letters", 8, "_added"));
            Assert.AreEqual("_added", CommonHelper.EnsureMaximumLength("09letters", 6, "_added"));

            try {
                CommonHelper.EnsureMaximumLength("09letters", 5, "_added"); //max 5 characters, postfix has 6 (postifx exceeds it)
            }
            catch (Exception ex) {
                Assert.AreEqual(typeof(ArgumentOutOfRangeException), ex.GetType());
            }
        }

        [TestMethod()]
        public void EnsureNumericOnlyTest() {
            Assert.AreEqual("7927", CommonHelper.EnsureNumericOnly("dh79dhd27h"));
            Assert.AreEqual("78", CommonHelper.EnsureNumericOnly("78DGDGdd"));
            Assert.AreEqual("8212", CommonHelper.EnsureNumericOnly("821dDH*U2"));
            Assert.AreEqual("1111", CommonHelper.EnsureNumericOnly("1111dS"));
        }

        [TestMethod()]
        public void EnsureNotNullTest() {
            string sentString = null;
            Assert.IsNotNull(CommonHelper.EnsureNotNull(sentString));

            sentString = "asd";
            Assert.IsNotNull(CommonHelper.EnsureNotNull(sentString));
        }

        [TestMethod()]
        public void AreNullOrEmptyTest() {
            string[] arrayWithNull = { "asd", null, "asd" };
            string[] arrayWithoutNull = { "asd", "ddd", "asd" };

            Assert.IsTrue(CommonHelper.AreNullOrEmpty(arrayWithNull));
            Assert.IsFalse(CommonHelper.AreNullOrEmpty(arrayWithoutNull));
        }

        [TestMethod()]
        public void ArraysEqualTest() {
            //return true because: the same object reference/ address
            int[] array01 = { 1, 23, 2 };
            Assert.AreSame(array01, array01);                                   //the same address == the same object
            Assert.IsTrue(CommonHelper.ArraysEqual<int>(array01, array01));     //the same address == the same object

            //return true because: the same object values
            int[] array02 = { 1, 23, 2 };
            Assert.AreNotSame(array01, array02);                               //not the same object !
            Assert.IsTrue(CommonHelper.ArraysEqual<int>(array01, array02));    //still the same values

            //return false because: different number of elements
            int[] array03 = { 11, 23, 2, 4, 5, 5, 6 };
            int[] array04 = { 99, 23, 2, 4 };
            Assert.IsFalse(CommonHelper.ArraysEqual<int>(array03, array04));

            //return false because: different values
            int[] array05 = { 11, 23, 2, 4, 5, 5, 6 };
            int[] array06 = { 99, 23, 2, 4, 5, 5, 6 };
            Assert.IsFalse(CommonHelper.ArraysEqual<int>(array05, array06));
        }

        [TestMethod()]
        public void GetTrustLevelTest() {
            //untestable
            Assert.IsTrue(true);
        }

        public class tempClass {
            public tempClass() { tempProperty = "some value"; }
            public string tempProperty { get; set; }
        }


        [TestMethod()]
        public void ToTest_Generic_Method() {
            //little trickery done here - convert word-character 'd' into ASCII code of 100
            byte result = CommonHelper.To<byte>('d');
            Assert.AreEqual(typeof(byte), result.GetType());
            Assert.AreEqual(100, result);
        }

        [TestMethod()]
        public void ToTest() {
            //I am sending a byte variable - and receive an integral variable (type has changed, value (in this particular case) didn't)
            double floatingPoint = 1000.08765;
            int thousandInt = 1000;
            Assert.AreEqual(thousandInt, CommonHelper.To<int>(floatingPoint));

            //copied from tests, it was really surprise that it can convert from string to int
            string thousandString = "1000";
            Assert.AreEqual(thousandInt, CommonHelper.To<int>(thousandString));
        }

        [TestMethod()]
        public void ConvertEnumTest() {
            string actualWithUpper = "SoMeTeStStRiNg";
            string expectedWithUpper = "So Me Te St St Ri Ng";
            string stringWithoutUppers = "someteststring";

            Assert.AreEqual(expectedWithUpper, CommonHelper.ConvertEnum(actualWithUpper));
            Assert.AreEqual(stringWithoutUppers, CommonHelper.ConvertEnum(stringWithoutUppers));
        }

        [TestMethod()]
        public void SetTelerikCultureTest() {
            //untestable
            Assert.IsTrue(true);
        }

        [TestMethod()]
        public void GetDifferenceInYearsTest() {
            DateTime birth = DateTime.Parse("2000-01-01 00:00");
            DateTime now = DateTime.Parse("3000-01-01 00:00");

            //happypath
            Assert.AreEqual(1000, CommonHelper.GetDifferenceInYears(birth, now));

            //path
            Assert.AreEqual(-1000, CommonHelper.GetDifferenceInYears(now, birth));

            //one day before birthday
            birth = DateTime.Parse("2000-06-01 00:00");
            now = DateTime.Parse("2016-05-30 00:00");

            Assert.AreEqual(15, CommonHelper.GetDifferenceInYears(birth, now));
        }
    }
}