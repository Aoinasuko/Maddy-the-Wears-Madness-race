using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Maddy_Race
{
    public class Maddy_Weapon_Scythe : ThingComp
    {
        public override void Notify_UsedWeapon(Pawn pawn)
        {
            MoteMaker.MakeStaticMote(pawn.Position, pawn.Map, ThingDef.Named("Maddy_Mote_MaddyScythe"), 2.0f);
            IEnumerable<Thing> things = pawn.Map.listerThings.AllThings.Where(x => x.Position.DistanceTo(pawn.Position) <= 1.5f && x != pawn);
            if (!things.EnumerableNullOrEmpty())
            {
                for (int i = things.Count() - 1; i >= 0; i--)
                {
                    Pawn target = things.ElementAt(i) as Pawn;
                    if (target != null)
                    {
                        target.health.AddHediff(HediffDef.Named("Maddy_SoulBond"));
                    }
                    things.ElementAt(i).TakeDamage(new DamageInfo(DamageDefOf.Cut, 15.0f, 1.0f, -1, pawn, null, this.parent.def));
                }
            }
        }
    }
}
