using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Replimat
{
    public class Replimat_Main
    {

        // Patch into Rimworld.Toils_Ingest
        public static Toil TakeMealFromReplimatTerminal(TargetIndex ind, Pawn eater)
        {
            Toil toil = new Toil();
            toil.initAction = delegate
            {
                Pawn actor = toil.actor;
                Job curJob = actor.jobs.curJob;
                Building_ReplimatTerminal building_ReplimatTerminal = (Building_ReplimatTerminal)curJob.GetTarget(ind).Thing;
                Thing thing = building_ReplimatTerminal.TryDispenseFood();
                if (thing == null)
                {
                    actor.jobs.curDriver.EndJobWith(JobCondition.Incompletable);
                    return;
                }
                actor.carryTracker.TryStartCarry(thing);
                actor.CurJob.SetTarget(ind, actor.carryTracker.CarriedThing);
            };
            toil.FailOnCannotTouch(ind, PathEndMode.Touch);
            toil.defaultCompleteMode = ToilCompleteMode.Delay;
            toil.defaultDuration = Building_ReplimatTerminal.CollectDuration;
            return toil;
        }



    }
}
