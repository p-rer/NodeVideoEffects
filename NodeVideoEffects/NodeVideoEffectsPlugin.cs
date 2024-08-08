﻿using System.ComponentModel.DataAnnotations;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;

namespace NodeVideoEffects
{
    [VideoEffect("NodeVideoEffects", [nameof(Translate.Node)], [], ResourceType = typeof(Translate))]
    internal class NodeVideoEffectsPlugin : VideoEffectBase
    {
        public override string Label => "NodeVideoEffects";

        [Display(Name = nameof(Translate.NodeEditor), GroupName = nameof(Translate.NodeVideoEffects), ResourceType = typeof(Translate))]
        [OpenNodeEditor]
        public string Nodes { get; } = "";

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
