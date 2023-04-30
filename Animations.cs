using System.Collections.Generic;
using HarmonyLib;
using ItemManager;
using UnityEngine;


namespace PungusSoulsAnimations
{
    [HarmonyPatch]
    public class PungusAnimations
    {
        private static readonly Dictionary<string, string> CoolAnimation = new();
        private static Dictionary<string, AnimationClip> _externalAnimations = new();
        private static bool _firstInit;
        internal static RuntimeAnimatorController? MyNewAnimation;
        internal static RuntimeAnimatorController? OrigAnimation;
        internal static void AnimationAwake()
        {
            AssetBundle asset = PrefabManager.RegisterAssetBundle("animations", "assets");
            CoolAnimation.Add("Dance", "MyCoolDance1");
            CoolAnimation.Add("Greatsword Secondary Attack", "GreatSwordSlashNew");

            _externalAnimations.Add("MyCoolDance1", asset.LoadAsset<AnimationClip>("MyCoolDance1"));
            _externalAnimations.Add("GreatSwordSlashNew", asset.LoadAsset<AnimationClip>("GreatSwordSlashNew"));
        }
        private static RuntimeAnimatorController MakeAoc(IReadOnlyDictionary<string, string> replacement,
            RuntimeAnimatorController original)
        {
            AnimatorOverrideController aoc = new(original);
            List<KeyValuePair<AnimationClip, AnimationClip>> anims = new();
            foreach (AnimationClip animation in aoc.animationClips)
            {
                string name = animation.name;
                if (replacement.ContainsKey(name))
                {
                    AnimationClip newClip = Object.Instantiate(_externalAnimations[replacement[name]]);
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(animation, newClip));
                }
                else
                {
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(animation, animation));
                }
            }

            aoc.ApplyOverrides(anims);
            return aoc;
        }

        [HarmonyPatch(typeof(Player), nameof(Player.Start))]
        [HarmonyPriority(Priority.Last)]
        private static class TESTPATCHPLAYERANIMS
        {
            private static void Postfix(Player __instance)
            {
                if (_firstInit) return;
                _firstInit = true;

                OrigAnimation = MakeAoc(new Dictionary<string, string>(),
                    __instance.m_animator.runtimeAnimatorController);
                MyNewAnimation = MakeAoc(CoolAnimation, __instance.m_animator.runtimeAnimatorController);
            }
        }
    }
}