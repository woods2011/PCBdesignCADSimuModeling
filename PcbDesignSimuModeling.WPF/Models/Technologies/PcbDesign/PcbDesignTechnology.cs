using System;
using System.Linq;
using PcbDesignSimuModeling.WPF.Models.Resources;
using PcbDesignSimuModeling.WPF.Models.Resources.EadSybSystems;
using PcbDesignSimuModeling.WPF.Models.SimuSystem;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;

public class PcbDesignTechnology
{
    private readonly StatCollector? _statCollector;

    private PcbDesignProcedure _curProcedure;

    public IEadSubSystemFactories EadSubSystemFactories { get; }
    private readonly IResourceManager _resourceManager;
    public PcbDescription PcbDescription { get; }
    public bool IsWaitForResources { get; private set; } = true;
    public int TechId { get; }
    public int NewRequestId => _resourceManager.NewRequestId;


    public PcbDesignTechnology(int techId, IResourceManager resourceManager, PcbDescription pcbDescription, IEadSubSystemFactories eadSubSystemFactories, StatCollector? statCollector)
    {
        TechId = techId;
        _resourceManager = resourceManager;
        PcbDescription = pcbDescription;
        EadSubSystemFactories = eadSubSystemFactories;
        _statCollector = statCollector;

        _curProcedure = new PcbParamsInput(this);
    }


    public PcbDesignProcedure CurProcedure
    {
        get => _curProcedure;
        set
        {
            FreeResources();
            _curProcedure = value;
            _isFirstVisit = true;
        }
    }

    private void FreeResources()
    {
        _resourceManager.FreeResources(_curProcedure.ActiveResources);
        _curProcedure.ActiveResources.Clear();
        _curProcedure.PotentialFailureResources.Clear();
        IsWaitForResources = true;
    }


    public void TryGetResources()
    {
        _statCollector?.LogPcbDesignProcedureStart(this);
        
        if (!IsWaitForResources)
        {
            if (CurProcedure.PotentialFailureResources.All(resource => resource.IsActive)) return;

            _statCollector?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                         $"Процесс Проектирования: {TechId} - Отказ одного из ресурсов - Проектная процедура: {_curProcedure.Name}");
            FreeResources();
        }

        IsWaitForResources =
            !_resourceManager.TryGetResources(_curProcedure.RequiredResources, out var receivedResources);

        if (IsWaitForResources)
        {
            if (!_isFirstVisit) return;
            _statCollector?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                         $"Процесс Проектирования: {TechId} - Ожидание ресурсов - Проектная процедура: {_curProcedure.Name}");
            _isFirstVisit = false;
            return;
        }

        _curProcedure.ActiveResources.AddRange(receivedResources);
        _curProcedure.PotentialFailureResources.AddRange(
            receivedResources.Select(pair => pair.Resource).OfType<IPotentialFailureResource>());
        _curProcedure.InitResources();
        
        _statCollector?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                           $"Процесс Проектирования: {TechId} - Проектная процедура: {CurProcedure.Name} - Ожидание ресурсов Оконченно");
    }


    public void UpdateModelTime(TimeSpan deltaTime)
    {
        if (IsWaitForResources) return;
        CurProcedure.UpdateModelTime(deltaTime);
    }

    public TimeSpan EstimateRemainingTime()
    {
        if (IsWaitForResources) return TimeSpan.MaxValue / 2.0;
        return CurProcedure.EstimateEndTime();
    }


    public bool MoveToNextProcedure()
    {
        var moveToNextProcedure = _curProcedure.NextProcedure();
        if (!moveToNextProcedure) FreeResources();
        return moveToNextProcedure;
    }


    private bool _isFirstVisit = true;
    public static readonly TimeSpan TimeTol = TimeSpan.FromSeconds(0.5);
}