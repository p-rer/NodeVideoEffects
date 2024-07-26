using NodeVideoEffects.Nodes.Math;

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

    [TestClass]
    public class MulTest
    {
        Mul MulNode = new();
        [TestMethod]
        public async Task MulAsync()
        {
            await MulNode.Calculate();
            Assert.AreEqual((double)MulNode.Outputs[0].Value, 0);

            MulNode.SetInput(0, 4.23);
            MulNode.SetInput(1, -3.1);
            await MulNode.Calculate();
            Assert.AreEqual((double)MulNode.Outputs[0].Value, -13.113);
        }
    }

    [TestClass]
    public class DivTest
    {
        Div DivNode = new();
        [TestMethod]
        public async Task DivAsync()
        {
            await DivNode.Calculate();
            Assert.AreEqual((double)DivNode.Outputs[0].Value, 0);

            DivNode.SetInput(0, 2.5);
            DivNode.SetInput(1, -5.2);
            await DivNode.Calculate();
            Assert.AreEqual((double)DivNode.Outputs[0].Value, -0.48076923);
        }
    }

    [TestClass]
    public class PowTest
    {
        Pow PowNode = new();
        [TestMethod]
        public async Task PowAsync()
        {
            await PowNode.Calculate();
            Assert.AreEqual((double)PowNode.Outputs[0].Value, 1);

            PowNode.SetInput(0, 4.32);
            PowNode.SetInput(1, -2.11);
            await PowNode.Calculate();
            Assert.AreEqual((double)PowNode.Outputs[0].Value, 0.04561727);
        }
    }

    [TestClass]
    public class RootTest
    {
        Root RootNode = new();
        [TestMethod]
        public async Task RootAsync()
        {
            await RootNode.Calculate();
            Assert.AreEqual((double)RootNode.Outputs[0].Value, 0);

            RootNode.SetInput(0, -3.6);
            RootNode.SetInput(1, 2.3);
            await RootNode.Calculate();
            Assert.AreEqual((double)RootNode.Outputs[0].Value, -1.74530227);
        }
    }
}
