using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Maddy_Race
{
	public class IncidentWorker_MaddyRaid : IncidentWorker
	{
		public const float HivePoints = 220f;

		public static readonly SimpleCurve PointsFactorCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0.7f),
			new CurvePoint(5000f, 0.45f)
		};

		protected override bool CanFireNowSub(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			if (!map.mapPawns.AllPawnsSpawned.Where(x => x.Faction?.IsPlayer ?? false).Any())
            {
				return false;
            }
			if (base.CanFireNowSub(parms) && Find.FactionManager.FirstFactionOfDef(Faction_Forgotty.Maddy_Raider) != null)
			{
				return true;
			}
			return false;
		}

		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			IntVec3 cell = new IntVec3();
			parms.points *= PointsFactorCurve.Evaluate(parms.points);
			List<Pawn> maddy = new List<Pawn>();
			Faction faction = Find.FactionManager.FirstFactionOfDef(Faction_Forgotty.Maddy_Raider);
			for (int i = 0; i < Math.Min(parms.points / 500 + 1, 10); i++)
            {
				cell = map.mapPawns.AllPawnsSpawned.Where(x => x.Faction?.IsPlayer ?? false).RandomElement().Position;
				Pawn mad = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDef.Named("Maddy_Raider"), faction));
				GenSpawn.Spawn(mad, cell, map);
				GenExplosion.DoExplosion(cell, map, 2.9f, DamageDefOf.Smoke, mad);
				mad.stances.SetStance(new Stance_Cooldown(600, null, null));
				maddy.Add(mad);
			}
			LordMaker.MakeNewLord(faction, new LordJob_DefendPoint(cell), map, maddy);
			SendStandardLetter(parms, maddy);
			Find.TickManager.slower.SignalForceNormalSpeedShort();
			return true;
		}
	}

}
