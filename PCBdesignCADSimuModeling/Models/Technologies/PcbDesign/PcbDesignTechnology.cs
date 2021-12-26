using System;
using System.Collections.Generic;
using System.Linq;
using PcbDesignCADSimuModeling.Models.Loggers;
using PcbDesignCADSimuModeling.Models.Resources;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PcbDesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PcbDesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PcbDesignCADSimuModeling.Models.Technologies.PcbDesign
{
    public class PcbDesignTechnology
    {
        private PcbDesignProcedure _curProcedure;
        private readonly IResourceManager _resourceManager;
        private readonly ISimpleLogger? _logger;
        public IPcbAlgFactories PcbAlgFactories { get; }
        public PcbParams PcbParams { get; }
        public bool IsWaitForResources { get; private set; } = true;
        public int TechId { get; }


        public PcbDesignTechnology(IResourceManager resourceManager, IPcbAlgFactories pcbAlgFactories,
            PcbParams pcbParams, int techId, ISimpleLogger? logger)
        {
            _resourceManager = resourceManager;
            _logger = logger;
            TechId = techId;
            PcbParams = pcbParams;
            PcbAlgFactories = pcbAlgFactories;

            _curProcedure = new PcbParamsInput(this);
        }


        public PcbDesignProcedure CurProcedure
        {
            get => _curProcedure;
            set
            {
                FreeResources();
                _curProcedure = value;
                IsWaitForResources = true;
            }
        }

        private void FreeResources()
        {
            _resourceManager.FreeResources(_curProcedure.ProcId, _curProcedure.ActiveResources);
            _curProcedure.ActiveResources.Clear();
        }

        public void TryGetResources()
        {
            if (!IsWaitForResources) return;

            IsWaitForResources = !_resourceManager.TryGetResources(
                _curProcedure.ProcId, _curProcedure.RequiredResources, out var receivedResources);

            if (!IsWaitForResources)
            {
                _curProcedure.ActiveResources.AddRange(receivedResources);
                _curProcedure.InitResourcesPower();
                _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                             $"Технология: {TechId} - Ожидание ресурсов оконченно - Проектная процедура: {_curProcedure.Name}");
                return;
            }

            _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                         $"Технология: {TechId} - Ожидание ресурсов - Проектная процедура: {_curProcedure.Name}");
        }


        public void UpdateModelTime(TimeSpan deltaTime)
        {
            if (!IsWaitForResources)
                CurProcedure.UpdateModelTime(deltaTime);
        }


        public TimeSpan EstimateEndTime()
        {
            if (!IsWaitForResources) return CurProcedure.EstimateEndTime();
            return TimeSpan.MaxValue / 2.0;
        }


        public bool MoveToNextProcedure()
        {
            var moveToNextProcedure = _curProcedure.NextProcedure();
            if (!moveToNextProcedure) FreeResources();
            return moveToNextProcedure;
        }

        public static readonly TimeSpan TimeTol = TimeSpan.FromSeconds(0.5);
    }
}