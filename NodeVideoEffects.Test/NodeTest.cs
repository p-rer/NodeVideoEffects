using NodeVideoEffects.Nodes;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class AddTest
    {
        Add AddNode = new();
        [TestMethod]
        public async Task AddAsync()
        {
            await AddNode.Calculate();
            Assert.AreEqual((double)AddNode.Outputs[0].Value, 0);

            AddNode.SetInput(0, 10.2);
            AddNode.SetInput(1, -4.54);
            await AddNode.Calculate();
            Assert.AreEqual((double)AddNode.Outputs[0].Value, 5.66);
        }
    }

    [TestClass]
    public class SubTest
    {
        Sub SubNode = new();
        [TestMethod]
        public async Task SubAsync()
        {
            await SubNode.Calculate();
            Assert.AreEqual((double)SubNode.Outputs[0].Value, 0);

            SubNode.SetInput(0, 2.633);
            SubNode.SetInput(1, 4.3);
            await SubNode.Calculate();
            Assert.AreEqual((double)SubNode.Outputs[0].Value, -1.667);
        }
    }
}
