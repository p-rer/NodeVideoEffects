using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YukkuriMovieMaker.Resources.Localization;

namespace NodeVideoEffects
{
    [VideoEffect("NodeVideoEffects", [nameof(Translate.Node)], [],ResourceType = typeof(string))]
    internal class NodeVideoEffectsPlugin : VideoEffectBase
    {
        public override string Label => "NodeVideoEffects";

        [Display(Name = nameof(Translate.NodeEditor),GroupName = "NodeVideoEffects")]
        [OpenNodeEditor]
        public String Blur { get; } = "";

        public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
        {
            return [];
        }

        public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        {
            return new NodeProcessor(devices, this);
        }

        protected override IEnumerable<IAnimatable> GetAnimatables() => [];
    }
}
