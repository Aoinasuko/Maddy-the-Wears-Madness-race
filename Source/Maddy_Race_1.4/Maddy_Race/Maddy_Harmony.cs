using AlienRace;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Maddy_Race
{

    [StaticConstructorOnStartup]
    static class Maddy_Harmony
    {
        static Maddy_Harmony()
        {
            var harmony = new Harmony("BEP.Maddy");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    // マッディーの派閥追加時、服装規定を無効化する
    [HarmonyPatch(typeof(Precept_Role), "GenerateNewApparelRequirements")]
    [HarmonyPatch(new Type[]
    {
        typeof(FactionDef),
    })]
    internal static class ApparelRequirement_Patch
    {

        [HarmonyPrefix]
        static bool Prefix(ref Precept_Role __instance, ref List<PreceptApparelRequirement> __result, FactionDef generatingFor)
        {
            if (generatingFor != null)
            {
                if (generatingFor.defName == "Maddy_MaddyGroup")
                {
                    List<PreceptApparelRequirement> ApparelRequirement = new List<PreceptApparelRequirement>();
                    // 役割のdefName取得
                    String role = __instance.def.defName;
                    // 役割によって装備を指定
                    if (!ApparelRequirement.NullOrEmpty())
                    {
                        __result = ApparelRequirement;
                    }
                    else
                    {
                        __result = null;
                    }
                    return false;
                }
            }
            return true;
        }
    }

    // 肉を人間扱いにしなくなる
    [HarmonyPatch(typeof(FoodUtility), "IsHumanlikeCorpseOrHumanlikeMeat")]
    static class Maddy_FixFoodUtility
    {
        [HarmonyPrefix]
        public static bool Fix_IsHumanlike(ref bool __result, ref Thing source, ref ThingDef foodDef)
        {
            Corpse corpse = source as Corpse;
            if (corpse != null && (corpse.InnerPawn.def.race.FleshType == FleshType_Maddy.Maddy))
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "GetMeatSourceCategoryFromCorpse")]
    [HarmonyPatch(new Type[]
    {
        typeof(Thing)
    })]
    static class Maddy_FixFoodUtility_GetMeatSourceCategoryCorpse
    {
        [HarmonyPrefix]
        public static bool Fix_IsHumanlike(ref MeatSourceCategory __result, Thing thing)
        {
            Corpse corpse = thing as Corpse;
            if (corpse != null && corpse.InnerPawn.def.race.FleshType == FleshType_Maddy.Maddy)
            {
                __result = MeatSourceCategory.Undefined;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FoodUtility), "GetMeatSourceCategory")]
    [HarmonyPatch(new Type[]
    {
        typeof(ThingDef)
    })]
    static class Maddy_FixFoodUtility_GetMeatSourceCategory
    {
        [HarmonyPrefix]
        public static bool Fix_IsHumanlike(ref MeatSourceCategory __result, ThingDef source)
        {
            if (source.ingestible == null)
            {
                return true;
            }
            if ((source.ingestible.foodType & FoodTypeFlags.Meat) == FoodTypeFlags.Meat)
            {
                if (source.ingestible.sourceDef != null)
                {
                    if (source.ingestible.sourceDef.race.FleshType == FleshType_Maddy.Maddy)
                    {
                        __result = MeatSourceCategory.Undefined;
                        return false;
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Corpse), "ButcherProducts")]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(float) })]
    static class Maddy_Corpse_Patch
    {

        [HarmonyBefore(new string[] { "rimworld.erdelf.alien_race.main" })]
        [HarmonyPrefix]
        static bool Prefix(Pawn butcher, float efficiency, ref IEnumerable<Thing> __result, Corpse __instance)
        {

            if (__instance.InnerPawn.def.race.FleshType == FleshType_Maddy.Maddy)
            {
                Pawn corpse = __instance.InnerPawn;
                IEnumerable<Thing> enumerable = corpse.ButcherProducts(butcher, efficiency);
                if (corpse.RaceProps.BloodDef != null)
                {
                    FilthMaker.TryMakeFilth(butcher.Position, butcher.Map, corpse.RaceProps.BloodDef, corpse.LabelIndefinite());
                }
                __result = enumerable;
                return false;
            }
            return true;
        }

    }

    /// <summary>
    /// 年齢補正無効化
    /// <summary>
    [HarmonyPatch(typeof(StatPart_Age), "AgeMultiplier")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    internal static class AgeMultiplier_Patch
    {
        [HarmonyPrefix]
        static bool Prefix(ref StatPart_Age __instance, ref float __result, Pawn pawn)
        {
            if (pawn.def.defName == "Maddy_Pawn")
            {
                __result = 1.0f;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(StatPart_AgeOffset), "AgeOffset")]
    [HarmonyPatch(new Type[]
    {
        typeof(Pawn),
    })]
    internal static class AgeMultiplier_Patch_Offset
    {
        [HarmonyPrefix]
        static bool Prefix(ref StatPart_AgeOffset __instance, ref float __result, Pawn pawn)
        {
            if (pawn.def.defName == "Maddy_Pawn")
            {
                __result = 0f;
                return false;
            }
            return true;
        }
    }

}
