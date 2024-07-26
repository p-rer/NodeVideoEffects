using NodeVideoEffects.Nodes.Math;
using NodeVideoEffects.Type;

namespace NodeVideoEffects.Test
{
    [TestClass]
    public class AddTest
    {
        Add AddNode = new();
        [TestMethod]
        public async Task AddAsync()
        {
            double result;
            result = (double)await NodesManager.GetOutputValue(AddNode.Id, 0);
            Assert.AreEqual(result, 0);

            AddNode.SetInput(0, 10.2);
            AddNode.SetInput(1, -4.54);
            result = (double)await NodesManager.GetOutputValue(AddNode.Id, 0);
            Assert.AreEqual(result, 5.66);
        }
    }

    [TestClass]
    public class SubTest
    {
        Sub SubNode = new();
        [TestMethod]
        public async Task SubAsync()
        {
            double result;
            result = (double)await NodesManager.GetOutputValue(SubNode.Id, 0);
            Assert.AreEqual(result, 0);

            SubNode.SetInput(0, 2.633);
            SubNode.SetInput(1, 4.3);
            result = (double)await NodesManager.GetOutputValue(SubNode.Id, 0);
            Assert.AreEqual(result, -1.667);
        }
    }

    [TestClass]
    public class MulTest
    {
        Mul MulNode = new();
        [TestMethod]
        public async Task MulAsync()
        {
            double result;
            result = (double)await NodesManager.GetOutputValue(MulNode.Id, 0);
            Assert.AreEqual(result, 0);

            MulNode.SetInput(0, 4.23);
            MulNode.SetInput(1, -3.1);
            result = (double)await NodesManager.GetOutputValue(MulNode.Id, 0);
            Assert.AreEqual(result, -13.113);
        }
    }

    [TestClass]
    public class DivTest
    {
        Div DivNode = new();
        [TestMethod]
        public async Task DivAsync()
        {
            double result;
            result = (double)await NodesManager.GetOutputValue(DivNode.Id, 0);
            Assert.AreEqual(result, 0);

            DivNode.SetInput(0, 2.5);
            DivNode.SetInput(1, -5.2);
            result = (double)await NodesManager.GetOutputValue(DivNode.Id, 0);
            Assert.AreEqual(result, -0.48076923);
        }
    }

    [TestClass]
    public class PowTest
    {
        Pow PowNode = new();
        [TestMethod]
        public async Task PowAsync()
        {
            double result;
            result = (double)await NodesManager.GetOutputValue(PowNode.Id, 0);
            Assert.AreEqual(result, 1);

            PowNode.SetInput(0, 4.32);
            PowNode.SetInput(1, -2.11);
            result = (double)await NodesManager.GetOutputValue(PowNode.Id, 0);
            Assert.AreEqual(result, 0.04561727);
        }
    }

    [TestClass]
    public class RootTest
    {
        Root RootNode = new();
        [TestMethod]
        public async Task RootAsync()
        {
            double result;
            result = (double)await NodesManager.GetOutputValue(RootNode.Id, 0);
            Assert.AreEqual(result, 0);

            RootNode.SetInput(0, -3.6);
            RootNode.SetInput(1, 2.3);
            result = (double)await NodesManager.GetOutputValue(RootNode.Id, 0);
            Assert.AreEqual(result, -1.74530227);
        }
    }
}
