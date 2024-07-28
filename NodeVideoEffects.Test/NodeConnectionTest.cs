using NodeVideoEffects.Nodes.Math;
using NodeVideoEffects.Type;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class NodeConnectionTest
    {
        AddNode addNode = new();
        PowNode powNode = new();
        SubNode subNode = new();

        [TestInitialize]
        public void Init()
        {
            addNode.SetInput(0, -13.0);
            addNode.SetInput(1, 15.0);

            powNode.SetInput(0, 8.0);
            powNode.SetInputConnection(1, new(addNode.Id, 0));

            subNode.SetInput(0, 68.0);
            subNode.SetInputConnection(1, new(powNode.Id, 0));
        }

        [TestMethod]
        public async Task ConnectionTestAsync()
        {
            double result;

            result = (double)await NodesManager.GetOutputValue(subNode.Id, 0);
            Assert.AreEqual(result, 4.0);

            return;
        }

        [TestMethod]
        public async Task ChangeOutputTest()
        {
            double result;

            addNode.SetInput(0, -12.0);
            
            result = (double)await NodesManager.GetOutputValue(subNode.Id, 0);
            Assert.AreEqual(result, -444.0);

            return;
        }
    }
}
