using System;

using NUnit.Framework;

using Packages.BrandonUtils.Runtime.Timing;

using static Packages.BrandonUtils.Runtime.CoercionUtils;

namespace Packages.BrandonUtils.Tests {
    public class CoercionUtilsTests {
        [Test]
        public void CoerciveAddition_int() {
            const int a_int = 5;
            const int b_int = 6;
            const int e_int = a_int + b_int;

            object a_obj = a_int;
            object b_obj = b_int;
            object e_obj = e_int;

            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj, b_obj)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_int, b_int)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj, b_int)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_int, b_obj)));

            Assert.That(e_int, Is.EqualTo(CoerciveAddition(a_obj, b_obj)));
            Assert.That(e_int, Is.EqualTo(CoerciveAddition(a_int, b_int)));
            Assert.That(e_int, Is.EqualTo(CoerciveAddition(a_obj, b_int)));
            Assert.That(e_int, Is.EqualTo(CoerciveAddition(a_int, b_obj)));
        }

        [Test]
        public void CoerciveAddition_long() {
            const long a_long = 5;
            const long b_long = 6;
            const long e_long = a_long + b_long;

            object a_obj = a_long;
            object b_obj = b_long;
            object e_obj = e_long;

            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,  b_obj)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_long, b_long)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,  b_long)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_long, b_obj)));

            Assert.That(e_long, Is.EqualTo(CoerciveAddition(a_obj,  b_obj)));
            Assert.That(e_long, Is.EqualTo(CoerciveAddition(a_long, b_long)));
            Assert.That(e_long, Is.EqualTo(CoerciveAddition(a_obj,  b_long)));
            Assert.That(e_long, Is.EqualTo(CoerciveAddition(a_long, b_obj)));
        }

        [Test]
        public void CoerciveAddition_float() {
            const float a_float = 5;
            const float b_float = 6;
            const float e_float = a_float + b_float;

            object a_obj = a_float;
            object b_obj = b_float;
            object e_obj = e_float;

            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,   b_obj)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_float, b_float)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,   b_float)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_float, b_obj)));

            Assert.That(e_float, Is.EqualTo(CoerciveAddition(a_obj,   b_obj)));
            Assert.That(e_float, Is.EqualTo(CoerciveAddition(a_float, b_float)));
            Assert.That(e_float, Is.EqualTo(CoerciveAddition(a_obj,   b_float)));
            Assert.That(e_float, Is.EqualTo(CoerciveAddition(a_float, b_obj)));
        }

        [Test]
        public void CoerciveAddition_double() {
            const double a_double = 5;
            const double b_double = 6;
            const double e_double = a_double + b_double;

            object a_obj = a_double;
            object b_obj = b_double;
            object e_obj = e_double;

            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,    b_obj)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_double, b_double)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,    b_double)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_double, b_obj)));

            Assert.That(e_double, Is.EqualTo(CoerciveAddition(a_obj,    b_obj)));
            Assert.That(e_double, Is.EqualTo(CoerciveAddition(a_double, b_double)));
            Assert.That(e_double, Is.EqualTo(CoerciveAddition(a_obj,    b_double)));
            Assert.That(e_double, Is.EqualTo(CoerciveAddition(a_double, b_obj)));
        }

        [Test]
        public void CoerciveAddition_decimal() {
            const decimal a_decimal = 5;
            const decimal b_decimal = 6;
            const decimal e_decimal = a_decimal + b_decimal;

            object a_obj = a_decimal;
            object b_obj = b_decimal;
            object e_obj = e_decimal;

            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,     b_obj)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_decimal, b_decimal)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,     b_decimal)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_decimal, b_obj)));

            Assert.That(e_decimal, Is.EqualTo(CoerciveAddition(a_obj,     b_obj)));
            Assert.That(e_decimal, Is.EqualTo(CoerciveAddition(a_decimal, b_decimal)));
            Assert.That(e_decimal, Is.EqualTo(CoerciveAddition(a_obj,     b_decimal)));
            Assert.That(e_decimal, Is.EqualTo(CoerciveAddition(a_decimal, b_obj)));
        }

        [Test]
        public void CoerciveAddition_DateTime() {
            var a_date = DateTime.Now;
            var b_span = TimeSpan.FromSeconds(1);
            var e_date = a_date + b_span;

            object a_obj = a_date;
            object b_obj = b_span;
            object e_obj = e_date;

            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,  b_obj)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_date, b_span)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_obj,  b_span)));
            Assert.That(e_obj, Is.EqualTo(CoerciveAddition(a_date, b_obj)));

            Assert.That(e_date, Is.EqualTo(CoerciveAddition(a_obj,  b_obj)));
            Assert.That(e_date, Is.EqualTo(CoerciveAddition(a_date, b_span)));
            Assert.That(e_date, Is.EqualTo(CoerciveAddition(a_obj,  b_span)));
            Assert.That(e_date, Is.EqualTo(CoerciveAddition(a_date, b_obj)));
        }

        [Test]
        public void CoerciveAddition_IsFasterThanDynamic() {
            // ReSharper disable ConvertToConstant.Local
            var a = 5;
            var b = 6;
            // ReSharper restore ConvertToConstant.Local

            const int iterations = 1000;

            var d_duration = TimeUtils.AverageExecutionTime(() => DynamicAddition(a, b), iterations);

            var c_duration = TimeUtils.AverageExecutionTime(() => CoerciveAddition(a, b), iterations);

            Assert.That(c_duration,     Is.LessThan(d_duration));
            Assert.That(c_duration.Max, Is.LessThanOrEqualTo(TimeSpan.FromMilliseconds(5)));
        }

        private static object DynamicAddition(object a, object b) {
            return (dynamic) a + (dynamic) b;
        }
    }
}