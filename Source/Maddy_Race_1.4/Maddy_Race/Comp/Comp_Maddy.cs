using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Maddy_Race
{
	[StaticConstructorOnStartup]
	public class Gizmo_NPStatus : Gizmo
	{
		public Comp_Maddy Maddy;

		private static readonly Texture2D FullNPBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.0f, 0.2f));

		private static readonly Texture2D EmptyNPBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

		public Gizmo_NPStatus()
		{
			Order = -100f;
		}

		public override float GetWidth(float maxWidth)
		{
			return 140f;
		}

		public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
		{
			Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
			Rect rect2 = rect.ContractedBy(6f);
			Widgets.DrawWindowBackground(rect);
			Rect rect3 = rect2;
			rect3.height = rect.height / 2f;
			Text.Font = GameFont.Tiny;
			Widgets.Label(rect3, "Maddy.UI.NP".Translate());
			Rect rect4 = rect2;
			rect4.yMin = rect2.y + rect2.height / 2f;
			float fillPercent = Math.Min(400f, Maddy.NP) / 400;
			Widgets.FillableBar(rect4, fillPercent, FullNPBarTex, EmptyNPBarTex, doBorder: false);
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(rect4, Maddy.NP.ToString());
			Text.Anchor = TextAnchor.UpperLeft;
			return new GizmoResult(GizmoState.Clear);
		}
	}

	class CompProperties_Maddy : CompProperties
	{
		public CompProperties_Maddy()
		{
			this.compClass = typeof(Comp_Maddy);
		}

	}

	public class Comp_Maddy : ThingComp
	{
		public int NP = 0;

		public override void CompTick()
		{
			Pawn comppawn = (Pawn)this.parent;
			if (this.parent.IsHashIntervalTick(180))
			{
				int NP_Count = 0;
				if (this.parent.Map != null)
				{
					IEnumerable<Pawn> pawns = this.parent.Map.mapPawns.AllPawnsSpawned.Where(x => x.needs.mood != null & x.Position.DistanceTo(this.parent.Position) <= 20.9f & x.def.defName != "Maddy_Pawn" & x != this.parent);
					foreach (Pawn pawn in pawns)
					{
						List<Thought> thoughts = new List<Thought>();
						pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
						thoughts = thoughts.Where(x => x.MoodOffset() < 0f).ToList();
						foreach (Thought thou in thoughts)
						{
							NP_Count += (int)(thou.MoodOffset() * -1f);
						}
					}
					if (NP < NP_Count)
					{
						NP += 20;
						if (NP > Math.Min(400, NP_Count))
						{
							NP = Math.Min(400, NP_Count);
						}
					}
					else
					{
						NP -= 5;
						if (NP < Math.Max(0, NP_Count))
						{
							NP = Math.Max(0, NP_Count);
						}
					}
					Verse.Hediff hediff = comppawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Maddy_NegativePower"));
					if (hediff != null)
					{
						hediff.Severity = Math.Max(NP, 0.1f);
					}
					else
					{
						hediff = comppawn.health.AddHediff(HediffDef.Named("Maddy_NegativePower"));
						hediff.Severity = Math.Max(NP, 0.1f);
					}
					for (int i = pawns.Count() - 1; i >= 0; i--)
					{
						if (pawns.ElementAt(i).HostileTo(comppawn))
						{
							Verse.Hediff hediff2 = pawns.ElementAt(i).health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Maddy_NegativePowerEnemy"));
							if (hediff2 != null)
							{
								if (hediff2.Severity < Math.Max(NP, 0.1f))
								{
									hediff2.Severity = Math.Max(NP, 0.1f);
								}
							}
							else
							{
								hediff2 = pawns.ElementAt(i).health.AddHediff(HediffDef.Named("Maddy_NegativePowerEnemy"));
								hediff2.Severity = Math.Max(NP, 0.1f);
							}
						}
					}
				}
				else
				{
					if (comppawn.Faction != null & comppawn.Faction.IsPlayer)
					{
						IEnumerable<Caravan> caravans = Find.WorldObjects.Caravans.Where(x => x.pawns?.Contains(comppawn) ?? false);
						if (!caravans.EnumerableNullOrEmpty())
						{
							Caravan caravan = caravans.First();
							foreach (Pawn pawn in caravan.pawns)
							{
								List<Thought> thoughts = new List<Thought>();
								pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
								thoughts = thoughts.Where(x => x.MoodOffset() < 0f).ToList();
								foreach (Thought thou in thoughts)
								{
									NP_Count += (int)(thou.MoodOffset() * -1f);
								}
							}
							if (NP < NP_Count)
							{
								NP -= 5;
								if (NP < Math.Max(0, NP_Count))
								{
									NP = Math.Max(0, NP_Count);
								}
							}
							else
							{
								NP += 20;
								if (NP > Math.Min(400, NP_Count))
								{
									NP = Math.Min(400, NP_Count);
								}
							}
							Verse.Hediff hediff = comppawn.health.hediffSet.GetFirstHediffOfDef(HediffDef.Named("Maddy_NegativePower"));
							if (hediff != null)
							{
								hediff.Severity = Math.Max(NP, 0.1f);
							}
							else
							{
								hediff = comppawn.health.AddHediff(HediffDef.Named("Maddy_NegativePower"));
								hediff.Severity = Math.Max(NP, 0.1f);
							}
						}
					}
				}
			}
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			Pawn comppawn = (Pawn)this.parent;
			if (comppawn.Map != null)
			{
				if (Find.Selector.SelectedPawns.Contains(comppawn))
				{
					Gizmo_NPStatus gizmo_NPStatus = new Gizmo_NPStatus();
					gizmo_NPStatus.Maddy = this;
					yield return gizmo_NPStatus;
				}
			}
		}
		public override void PostDraw()
		{
			Pawn comppawn = (Pawn)this.parent;
			if (comppawn.Map != null)
			{
				if (Find.Selector.SelectedPawns.Contains(comppawn))
				{
					GenDraw.DrawRadiusRing(comppawn.Position, 20.9f, Color.magenta, null);
				}
			}
		}
	}

	public class ThoughtWorker_NP : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (p.def.defName == "Maddy_Pawn")
			{
				Comp_Maddy comp = p.GetComp<Comp_Maddy>();
				if (comp != null)
				{
					if (comp.NP < 50)
					{
						return ThoughtState.ActiveAtStage(0);
					}
					if (comp.NP < 69)
					{
						return ThoughtState.Inactive;
					}
					if (comp.NP < 100)
					{
						return ThoughtState.ActiveAtStage(1);
					}
					if (comp.NP < 200)
					{
						return ThoughtState.ActiveAtStage(2);
					}
					if (comp.NP < 300)
					{
						return ThoughtState.ActiveAtStage(3);
					}
					return ThoughtState.ActiveAtStage(4);
				}
			}
			return ThoughtState.Inactive;
		}

	}
}
