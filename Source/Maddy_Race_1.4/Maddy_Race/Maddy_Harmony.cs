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

}
