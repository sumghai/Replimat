using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Replimat
{
    public class JobDriver_LoadReplimatCorpseRecycler : JobDriver
    {
        private const TargetIndex CorpseRecyclerInd = TargetIndex.A;

        private const TargetIndex CorpseInd = TargetIndex.B;

        private const int Duration = 200;

        protected Building_ReplimatCorpseRecycler CorpseRecycler => (Building_ReplimatCorpseRecycler)job.GetTarget(CorpseRecyclerInd).Thing;

        protected Corpse Corpse => (Corpse)job.GetTarget(CorpseInd).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (pawn.Reserve(CorpseRecycler, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(Corpse, job, 1, -1, null, errorOnFailed);
            }
            return false;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(CorpseRecyclerInd);
            this.FailOnBurningImmobile(CorpseRecyclerInd);
            this.FailOn(delegate 
            {
                // Fail if corpse recycler has no power, or its current storage settings does not allow the chosen corpse
                return !CorpseRecycler.powerComp.PowerOn || !CorpseRecycler.allowedCorpseFilterSettings.AllowedToAccept(Corpse);
            });
            AddEndCondition(() => (CorpseRecycler.Empty) ? JobCondition.Ongoing : JobCondition.Succeeded);
            yield return Toils_General.DoAtomic(delegate
            {
                job.count = 1;
            });
            Toil reserveCorpse = Toils_Reserve.Reserve(CorpseInd);
            yield return reserveCorpse;
            yield return Toils_Goto.GotoThing(CorpseInd, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(CorpseInd).FailOnSomeonePhysicallyInteracting(CorpseInd);
            yield return Toils_Haul.StartCarryThing(CorpseInd, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(CorpseInd);
            yield return Toils_Goto.GotoThing(CorpseRecyclerInd, PathEndMode.Touch);
            yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(CorpseInd).FailOnDestroyedNullOrForbidden(CorpseRecyclerInd).FailOnCannotTouch(CorpseRecyclerInd, PathEndMode.Touch).WithProgressBarToilDelay(CorpseRecyclerInd);
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                if (pawn.needs?.mood?.thoughts != null)
                {
                    ThoughtDef loaderPawnThought = (pawn.story.traits.HasTrait(TraitDefOf.Psychopath) || pawn.story.traits.HasTrait(TraitDefOf.Cannibal) || pawn.Ideo?.HasHumanMeatEatingRequiredPrecept() == true) ? ReplimatDef.Thought_RecycledCorpseInReplimatPsychopath : Corpse.InnerPawn.IsColonist ? ReplimatDef.Thought_RecycledColonistCorpseInReplimat : ReplimatDef.Thought_RecycledStrangerCorpseInReplimat;

                    pawn.needs.mood.thoughts.memories.TryGainMemory(loaderPawnThought);
                }

                foreach (Pawn otherPawn in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction))
                {
                    if (otherPawn != pawn && otherPawn.needs?.mood?.thoughts != null)
                    { 
                        ThoughtDef otherPawnThought = (otherPawn.story.traits.HasTrait(TraitDefOf.Psychopath) || otherPawn.story.traits.HasTrait(TraitDefOf.Cannibal) || otherPawn.Ideo?.HasHumanMeatEatingRequiredPrecept() == true) ? ReplimatDef.Thought_KnowRecycledCorpseInReplimatPsychopath : Corpse.InnerPawn.IsColonist ? ReplimatDef.Thought_KnowRecycledColonistCorpseInReplimat : ReplimatDef.Thought_KnowRecycledStrangerCorpseInReplimat;

                        otherPawn.needs.mood.thoughts.memories.TryGainMemory(otherPawnThought);
                    }
                }

                CorpseRecycler.LoadCorpse(Corpse);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }
    }
}
