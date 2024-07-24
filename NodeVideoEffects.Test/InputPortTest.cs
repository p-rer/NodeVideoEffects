using NodeVideoEffects.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class InputPortTest
    {
        Input number_input = new(new Number(10, 0, 100, 1), "Number");

        [TestMethod]
        public void GetNumberInputTest()
        {
            Assert.AreEqual(number_input.Value, 10.0);
        }

        [TestMethod]
        public void SetNumberInputTest()
        {
            number_input.Value = 14.2;
            Assert.AreEqual(number_input.Value, 14.2);
        }

        [TestMethod]
        public void OutOfToleranceNumberInputTest()
        {
            number_input.Value = -4.0;
            Assert.AreEqual(number_input.Value, 0.0);
            number_input.Value = 125.0;
            Assert.AreEqual(number_input.Value, 100.0);
        }

        [TestMethod]
        public void RoudNumberInputTest()
        {
            number_input.Value = 8.68;
            Assert.AreEqual(number_input.Value, 8.7);
        }

        [TestMethod]
        public void TypeMismatchNumberInputTest()
        {
            var ex = Assert.ThrowsException<TypeMismatchException>(() =>
            {
                number_input.Value = "abc";
            }
            );
            Assert.AreEqual("Type mismatch: 'Double' <- 'String'", ex.Message);
        }
    }
}
