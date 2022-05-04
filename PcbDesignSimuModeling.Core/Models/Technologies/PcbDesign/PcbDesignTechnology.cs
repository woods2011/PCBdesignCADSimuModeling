using PcbDesignSimuModeling.Core.Models.Loggers;
using PcbDesignSimuModeling.Core.Models.Resources;
using PcbDesignSimuModeling.Core.Models.Resources.Algorithms;
using PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign.ProjectProcedures;

namespace PcbDesignSimuModeling.Core.Models.Technologies.PcbDesign;

public class PcbDesignTechnology
{
    private readonly ISimpleLogger? _logger;

    private PcbDesignProcedure _curProcedure;

    public IPcbAlgFactories PcbAlgFactories { get; }
    private readonly IResourceManager _resourceManager;
    public PcbParams PcbParams { get; }
    public bool IsWaitForResources { get; private set; } = true;
    public int TechId { get; }
    public int NewRequestId => _resourceManager.NewRequestId;


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
            _isFirstVisit = true;
        }
    }

    private void FreeResources()
    {
        _resourceManager.FreeResources(_curProcedure.ActiveResources);
        _curProcedure.ActiveResources.Clear();
        IsWaitForResources = true;
    }


    public void TryGetResources()
    {
        if (!IsWaitForResources)
        {
            if (CurProcedure.PotentialFailureResources.All(resource => resource.IsActive)) return;

            _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                         $"Технология: {TechId} - Отказ одного из ресурсов - Проектная процедура: {_curProcedure.Name}");
            FreeResources();
        }

        IsWaitForResources =
            !_resourceManager.TryGetResources(_curProcedure.RequiredResources, out var receivedResources);

        if (IsWaitForResources)
        {
            if (!_isFirstVisit) return;
            _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                         $"Технология: {TechId} - Ожидание ресурсов - Проектная процедура: {_curProcedure.Name}");
            _isFirstVisit = false;
            return;
        }

        _curProcedure.ActiveResources.AddRange(receivedResources);
        _curProcedure.PotentialFailureResources.AddRange(
            receivedResources.Select(pair => pair.Resource).OfType<IPotentialFailureResource>());
        _curProcedure.InitResourcesPower();

        if (_isFirstVisit) return;
        _logger?.Log($"{String.Concat(Enumerable.Repeat("---", (TechId - 1) % 15))}" +
                     $"Технология: {TechId} - Ожидание ресурсов Оконченно - Проектная процедура: {_curProcedure.Name}");
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