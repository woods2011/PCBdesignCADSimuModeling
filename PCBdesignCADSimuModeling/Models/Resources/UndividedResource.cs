﻿using System;

namespace PCBdesignCADSimuModeling.Models.Resources
{
    public abstract class UndividedResource : Resource
    {
        protected Guid UtilizingProcId = Guid.Empty;
    }


    public class Designer : UndividedResource
    {
        private readonly Func<Designer, double> _resValueConvolution;


        public Designer(ExperienceEn experience, Func<Designer, double> resValueConvolution = null)
        {
            Experience = experience;
            _resValueConvolution = resValueConvolution ?? (designer => (double) designer.Experience);
        }


        public ExperienceEn Experience { get; }


        public bool TryGetResource(Guid procId)
        {
            if (UtilizingProcId != Guid.Empty) return false;

            UtilizingProcId = procId;
            return true;
        }

        public override double ResValueForProc(Guid procId) => _resValueConvolution(this);

        public override void FreeResource(Guid procId) => UtilizingProcId = Guid.Empty;


        public enum ExperienceEn
        {
            Little = 1,
            Average = 2,
            Extensive = 3
        }
    }
}