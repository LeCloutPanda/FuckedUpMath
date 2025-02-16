using System;
using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using SkyFrost.Base;

public class FuckedUpMath : ResoniteMod
{
    public enum MathFunction {
        cos,
        sin,
        ceil,
        floor,
        tan
    }

    public override string Author => "LeCloutPanda";
    public override string Name => "Fucked Up Math";
    public override string Version => "1.0.0";
    public override string Link => "https://github.com/LeCloutPanda/FuckedUpMath";

    public static ModConfiguration config;
    [AutoRegisterConfigKey] private static ModConfigurationKey<bool> ENABLED = new ModConfigurationKey<bool>("enabledToggle", "Enabled", () => false);
    [AutoRegisterConfigKey] private static ModConfigurationKey<MathFunction> COS_REPLACEMENT = new ModConfigurationKey<MathFunction>("cosReplacement", "Cos function replacement", () => MathFunction.cos);
    [AutoRegisterConfigKey] private static ModConfigurationKey<MathFunction> SIN_REPLACEMENT = new ModConfigurationKey<MathFunction>("sinReplacement", "Sin function replacement", () => MathFunction.sin);
    [AutoRegisterConfigKey] private static ModConfigurationKey<MathFunction> CEIL_REPLACEMENT = new ModConfigurationKey<MathFunction>("ceilReplacement", "Ceil function replacement", () => MathFunction.ceil);
    [AutoRegisterConfigKey] private static ModConfigurationKey<MathFunction> FLOOR_REPLACEMENT = new ModConfigurationKey<MathFunction>("floorReplacement", "Floor function replacement", () => MathFunction.floor);
    [AutoRegisterConfigKey] private static ModConfigurationKey<MathFunction> TAN_REPLACEMENT = new ModConfigurationKey<MathFunction>("tanReplacement", "Tan function replacement", () => MathFunction.tan);
   
    [AutoRegisterConfigKey] private static ModConfigurationKey<object> SPACER = new ModConfigurationKey<object>("spacer", " ", () => null);
    [AutoRegisterConfigKey] private static ModConfigurationKey<bool> RANDOMIZE = new ModConfigurationKey<bool>("randomize", "Randomize", () => false);
    //[AutoRegisterConfigKey] private static ModConfigurationKey<bool> ALLOW_MULTIPLAYER = new ModConfigurationKey<bool>("allowMultiplayer", "Bypasses LAN access level which will let others join", () => false);
    
    private static readonly Random random = new Random();

    public override void OnEngineInit()
    {
        config = GetConfiguration();
        config.Save(true);

        Harmony harmony = new Harmony("dev.lecloutpanda.fuckedupmath");
        harmony.PatchAll();

        Engine.Current.OnReady += () => {
            config.Set(ENABLED, false);    
        };
        
        config.OnThisConfigurationChanged += (e) => {
			if (e.Key == RANDOMIZE && config.GetValue(RANDOMIZE)) {
                config.Set(ENABLED, false);
                config.Set(COS_REPLACEMENT, GetRandomMathFunction());
                config.Set(SIN_REPLACEMENT, GetRandomMathFunction());
                config.Set(CEIL_REPLACEMENT, GetRandomMathFunction());
                config.Set(FLOOR_REPLACEMENT, GetRandomMathFunction());
                config.Set(TAN_REPLACEMENT, GetRandomMathFunction());
			}
		};
    }

    [HarmonyPatch(typeof(MathX))]
    class TheFuckening {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MathX.Cos), new Type[] { typeof(float) })]
        private static float CosPatch(float d, ref float __result) {
            if (!ShouldRun()) return __result;

            return FuckedMath(d, config.GetValue(COS_REPLACEMENT));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MathX.Sin), new Type[] { typeof(float) })]
        private static float SinPatch(float a, ref float __result) {
            if (!ShouldRun()) return __result;

            return FuckedMath(a, config.GetValue(SIN_REPLACEMENT));
        }
    
        [HarmonyPostfix]
        [HarmonyPatch(nameof(MathX.Ceil), new Type[] { typeof(float) })]
        private static float CeilPatch(float a, ref float __result) {
            if (!ShouldRun()) return __result;

            return FuckedMath(a, config.GetValue(CEIL_REPLACEMENT));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MathX.Floor), new Type[] { typeof(float) })]
        private static float FloorPatch(float d, ref float __result) {
            if (!ShouldRun()) return __result;

            return FuckedMath(d, config.GetValue(FLOOR_REPLACEMENT));
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(MathX.Tan), new Type[] { typeof(float) })]
        private static float TanPatch(float a, ref float __result) {
            if (!ShouldRun()) return __result;

            return FuckedMath(a, config.GetValue(TAN_REPLACEMENT));
        }
    }

    private static bool ShouldRun() {
        if (Engine.Current.WorldManager.FocusedWorld == null) return false;
        if (Engine.Current.WorldManager.FocusedWorld.IsAuthority) return false;
        if (!Engine.Current.WorldManager.FocusedWorld.UnsafeMode) return false;
        if (Engine.Current.WorldManager.FocusedWorld.AccessLevel != SessionAccessLevel.LAN) return false;
        //if (config.GetValue(ALLOW_MULTIPLAYER) && Engine.Current.WorldManager.FocusedWorld.AccessLevel != SessionAccessLevel.LAN) return false;
        if (!config.GetValue(ENABLED)) return false;

        return true;
    }

    private static float FuckedMath(float value, MathFunction function) {
        switch(function) {
            default: 
                return (float)value;

            case MathFunction.cos:
                return (float)Math.Cos(value);

            case MathFunction.sin: 
                return (float)Math.Sin(value);
                
            case MathFunction.ceil:
                return (float)Math.Ceiling(value);

            case MathFunction.floor:
                return (float)Math.Floor(value);

            case MathFunction.tan: 
                return (float)Math.Tan(value);
        }
    }

    public static MathFunction GetRandomMathFunction()
    {
        Array values = Enum.GetValues(typeof(MathFunction));
        return (MathFunction)values.GetValue(random.Next(values.Length));
    }
}