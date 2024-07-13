using NodeVideoEffects.Node;
using NodeVideoEffects.Type;
using System.Collections.ObjectModel;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class PortTest
    {
        Output number_output = new(new Number(10, 0, 100, 1));
        [TestMethod]
        public void GetNumberOutputTest()
        {
            Assert.AreEqual(number_output.Value.Value, 10.0);
        }

        [TestMethod]
        public void SetNumberOutputTest()
        {
            number_output.Value.SetValue(14.2);
            Assert.AreEqual(number_output.Value.Value, 14.2);
        }

        [TestMethod]
        public void OutOfToleranceNumberOutputTest()
        {
            number_output.Value.SetValue(-4.0);
            Assert.AreEqual(number_output.Value.Value, 0.0);
            number_output.Value.SetValue(125.0);
            Assert.AreEqual(number_output.Value.Value, 100.0);
        }
    }
}