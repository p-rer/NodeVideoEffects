using NodeVideoEffects.Node;
using NodeVideoEffects.Type;
using System.Collections.ObjectModel;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class OutputPortTest
    {
        Output number_output = new(new Number(10, 0, 100, 1));
        [TestMethod]
        public void GetNumberOutputTest()
        {
            Assert.AreEqual(number_output.Value, 10.0);
        }

        [TestMethod]
        public void SetNumberOutputTest()
        {
            number_output.Value = 14.2;
            Assert.AreEqual(number_output.Value, 14.2);
        }

        [TestMethod]
        public void OutOfToleranceNumberOutputTest()
        {
            number_output.Value = -4.0;
            Assert.AreEqual(number_output.Value, 0.0);
            number_output.Value = 125.0;
            Assert.AreEqual(number_output.Value, 100.0);
        }

        [TestMethod]
        public void RoudNumberOutputTest()
        {
            number_output.Value = 8.68;
            Assert.AreEqual(number_output.Value, 8.7);
        }

        [TestMethod]
        public void TypeMismatchNumberOutputTest()
        {
            var ex = Assert.ThrowsException<TypeMismatchException>(() =>
                {
                    number_output.Value = "abc";
                }
            );
            Assert.AreEqual("Type mismatch: 'Double' <- 'String'", ex.Message);
        }
    }
}