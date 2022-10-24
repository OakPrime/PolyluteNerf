using BepInEx;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using R2API;
using R2API.Utils;
using System.Collections.Generic;

namespace PolyluteNerf
{
    //Loads R2API Submodules
    [R2APISubmoduleDependency(nameof(LanguageAPI))]

    //This is an example plugin that can be put in BepInEx/plugins/ExamplePlugin/ExamplePlugin.dll to test out.
    //It's a small plugin that adds a relatively simple item to the game, and gives you that item whenever you press F2.

    //This attribute is required, and lists metadata for your plugin.
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]

    //This is the main declaration of our plugin class. BepInEx searches for all classes inheriting from BaseUnityPlugin to initialize on startup.
    //BaseUnityPlugin itself inherits from MonoBehaviour, so you can use this as a reference for what you can declare and use in your plugin class: https://docs.unity3d.com/ScriptReference/MonoBehaviour.html
    public class PolyluteNerf : BaseUnityPlugin
    {
        //The Plugin GUID should be a unique ID for this plugin, which is human readable (as it is used in places like the config).
        //If we see this PluginGUID as it is on thunderstore, we will deprecate this mod. Change the PluginAuthor and the PluginName !
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "OakPrime";
        public const string PluginName = "PolyluteNerf";
        public const string PluginVersion = "1.1.0";

        private readonly Dictionary<string, string> DefaultLanguage = new Dictionary<string, string>();

        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            try
            {
                
                IL.RoR2.GlobalEventManager.OnHitEnemy += (il) =>
                {
                    ILCursor c = new ILCursor(il);
                    c.TryGotoNext(
                        x => x.MatchBrfalse(out _),
                        x => x.MatchLdcR4(0.6f),
                        x => x.MatchStloc(out _)
                    );
                    c.Index++;
                    c.Next.Operand = 0.4f;
                    c.TryGotoNext(
                        x => x.MatchLdloc(out _),
                        x => x.MatchLdcI4(out _),
                        x => x.MatchLdloc(out _),
                        x => x.MatchMul(),
                        x => x.MatchStfld<RoR2.Orbs.VoidLightningOrb>(nameof(RoR2.Orbs.VoidLightningOrb.totalStrikes))
                    );
                    c.Index += 4;
                    c.EmitDelegate<Func<int, int>>((totalStrikes) =>
                    {
                        if (totalStrikes > 3)
                        {
                            return totalStrikes - (totalStrikes / 3) + 1;
                        }
                        else
                        {
                            return totalStrikes;
                        }
                    });
                    this.ReplacePolyluteText();
                };
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message + " - " + e.StackTrace);
            }          
        }
        private void ReplacePolyluteText()
        {
            this.ReplaceString("ITEM_CHAINLIGHTNINGVOID_DESC", "<style=cIsDamage>25%</style> chance to fire <style=cIsDamage>lightning</style>" +
                "for <style=cIsDamage>40%</style> TOTAL damage up to <style=cIsDamage>3</style> <style=cStack>(+2 per stack)</style>" +
                "times. <style=cIsVoid>Corrupts all Ukeleles</style>.");
        }

        private void ReplaceString(string token, string newText)
        {
            this.DefaultLanguage[token] = Language.GetString(token);
            LanguageAPI.Add(token, newText);
        }
    }
}
