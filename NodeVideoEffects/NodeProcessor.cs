using NodeVideoEffects.Nodes.Basic;
using NodeVideoEffects.Type;
using System.Reflection;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace NodeVideoEffects
{
    internal class NodeProcessor : IVideoEffectProcessor
    {
        readonly NodeVideoEffectsPlugin item;
        public ID2D1Image Output { set; get; }

        bool isFirst = true;
        string nodes;

        Nodes.Basic.InputNode inputNode = new();
        Nodes.Basic.OutputNode outputNode = new();

        public NodeProcessor(IGraphicsDevicesAndContext devices, NodeVideoEffectsPlugin item)
        {
            this.item = item;
        }

        public void SetInput(ID2D1Image input)
        {
            Console.WriteLine(input);
            inputNode.SetImage(input);
            Console.WriteLine(inputNode.GetOutput(0)??"null");
            outputNode.SetInputConnection(0, new(inputNode.Id, 0));
            Console.WriteLine(outputNode.Inputs[0].Connection.Value.id??"null");
            Console.WriteLine(outputNode.Inputs[0].Connection.Value.index);
            Console.WriteLine(outputNode.Inputs[0].Value??"null");
            Output = (ID2D1Image)outputNode.Inputs[0].Value;
        }

        public void ClearInput()
        {
        }

        public DrawDescription Update(EffectDescription effectDescription)
        {
            var frame = effectDescription.ItemPosition.Frame;
            var length = effectDescription.ItemDuration.Frame;
            var fps = effectDescription.FPS;
            try
            {
                typeof(NodesManager)
                    .GetMethod("SetInfo",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Invoke(null, [effectDescription]);
            } catch (NullReferenceException e)
            {

            }
            isFirst = false;

            return effectDescription.DrawDescription;
        }

        public void Dispose()
        {
            ClearInput();
            if (Output != null) Output.Dispose();
        }
    }
}
