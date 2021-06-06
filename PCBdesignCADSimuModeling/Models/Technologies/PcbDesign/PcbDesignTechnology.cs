using System;
using System.Collections.Generic;
using PCBdesignCADSimuModeling.Models.Loggers;
using PCBdesignCADSimuModeling.Models.Resources;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.PlacingAlgorithms;
using PCBdesignCADSimuModeling.Models.Resources.Algorithms.WireRoutingAlgorithms;
using PCBdesignCADSimuModeling.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PCBdesignCADSimuModeling.Models.Technologies.PcbDesign
{
    public class PcbDesignTechnology
    {
        private PcbDesignProcedure _curProcedure;
        private readonly IResourceManager _resourceManager;
        private readonly ISimpleLogger _logger;


        public PcbDesignTechnology(IResourceManager resourceManager, PcbParams pcbParams,
            IPcbAlgFactories pcbAlgFactories, ISimpleLogger logger)
        {
            _resourceManager = resourceManager;
            _logger = logger;
            PcbParams = pcbParams;
            PcbAlgFactories = pcbAlgFactories;
            CurProcedure = new PcbParamsInput(this);
        }


        public int TechId { get; } = Id;

        public PcbParams PcbParams { get; }
        public IPcbAlgFactories PcbAlgFactories { get; }

        public PcbDesignProcedure CurProcedure
        {
            get => _curProcedure;
            set
            {
                if (_curProcedure is not null)
                    _resourceManager.FreeResources(_curProcedure.ProcedureId, _curProcedure.ActiveResources);

                _curProcedure = value;
                if (_curProcedure is null) return;

                IsWaitResources = !_resourceManager.TryGetResources(
                    _curProcedure.ProcedureId, _curProcedure.RequiredResources, out var receivedResources);
                if (!IsWaitResources)
                    _curProcedure.ActiveResources.AddRange(receivedResources);
                else
                    _logger.Log(
                        $"{_logger.ModelTime} | Технология: {TechId} - Ожидание ресурсов - Проектная процедура: {_curProcedure.Name}");
            }
        }

        public bool IsWaitResources { get; private set; }


        public void UpdateModelTime(TimeSpan deltaTime)
        {
            if (!IsWaitResources)
                CurProcedure.UpdateModelTime(deltaTime);
        }


        public TimeSpan EstimateEndTime()
        {
            if (!IsWaitResources)
                return CurProcedure.EstimateEndTime();

            IsWaitResources = !_resourceManager.TryGetResources(
                _curProcedure.ProcedureId, _curProcedure.RequiredResources, out var receivedResources);
            if (!IsWaitResources)
            {
                _curProcedure.ActiveResources.AddRange(receivedResources);
                _logger.Log(
                    $"{_logger.ModelTime} | Технология: {TechId} - Ожидание ресурсов оконченно - Проектная процедура: {_curProcedure.Name}");
            }

            return IsWaitResources ? TimeSpan.MaxValue / 2.0 : CurProcedure.EstimateEndTime();
        }


        public bool MoveToNextProcedure() => _curProcedure.NextProcedure();


        private static int _id = 0;

        public static int Id
        {
            get
            {
                _id++;
                return _id;
            }
            set => _id = value;
        }
    }
}