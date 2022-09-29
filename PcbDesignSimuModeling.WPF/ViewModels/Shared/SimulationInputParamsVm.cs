using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using MvvmDialogs;
using MvvmDialogs.FrameworkDialogs.OpenFile;
using PcbDesignSimuModeling.WPF.Commands;
using PcbDesignSimuModeling.WPF.Models.Technologies.PcbDesign;
using PcbDesignSimuModeling.WPF.ViewModels.Helpers;

namespace PcbDesignSimuModeling.WPF.ViewModels.Shared;

public class SimulationInputParamsVm : BaseViewModel
{
    private readonly Random _rndSource;
    private readonly DialogService _dialogService = new();
    public MessageViewModel MessageViewModel { get; }

    public SimulationInputParamsVm(Random rndSource, MessageViewModel? messageViewModel = null)
    {
        _rndSource = rndSource;
        MessageViewModel = messageViewModel ?? new MessageViewModel();
        ElementCountDistr = new DblNormalDistrVm(300, 100, _rndSource);
        NewElemsPercentDistr = new DblNormalDistrVm(0.0, 0.0, _rndSource);
        ElementsDensityDistr = new DblNormalDistrVm(0.5, 0.1, _rndSource);
        PinsCountDistr = new DblNormalDistrVm(1200, 100, _rndSource);
        PinDensityDistr = new DblNormalDistrVm(3.3, 1.2, _rndSource);
        NumOfLayersDistr = new DblNormalDistrVm(2, 0, _rndSource);
    }

    public int TechPerYear { get; set; } = 100;
    public bool IsImportFromFile => TabIndex is not 0;
    public int TabIndex { get; set; } = 0;
    public IReadOnlyList<PcbDescription>? ImportedPcbDescriptions { get; private set; }
    public ImportedPcbDescriptionsSummary? ImportedPcbDescriptionsSummary { get; private set; }
    public DblNormalDistrVm ElementCountDistr { get; private set; }
    public DblNormalDistrVm NewElemsPercentDistr { get; private set; }
    public DblNormalDistrVm ElementsDensityDistr { get; private set; }
    public DblNormalDistrVm PinsCountDistr { get; private set; }
    public DblNormalDistrVm PinDensityDistr { get; private set; }
    public DblNormalDistrVm NumOfLayersDistr { get; private set; }
    public double IsDoubleSidePlacementProb { get; set; } = 0.5;
    public double IsManualSchemeInputProb { get; set; } = 0.5;

    public double GoalProductionTimeHours { get; set; } = TimeSpan.FromDays(5).TotalHours / 3;
    public TimeSpan RunUpSection { get; set; } = TimeSpan.FromDays(21);
    public TimeSpan FinalTime { get; set; } = TimeSpan.FromDays(126);

    public ICommand ImportPcbDescriptionsFromFileCommand =>
        new ActionCommand(_ => LoadPcbDescriptionsFromFile());


    private void LoadPcbDescriptionsFromFile()
    {
        var dialogSettings = new OpenFileDialogSettings
        {
            Title = "Импорт набора описаний печатных плат",
            InitialDirectory = $@"{Directory.GetCurrentDirectory()}\Files\PcbDescriptions",
            Filter = "JSON file (*.json)|*.json|Text Documents (*.txt)|*.txt|All Files (*.*)|*.*"
        };

        var success = _dialogService.ShowOpenFileDialog(this, dialogSettings);
        if (success != true) return;

        var fileName = dialogSettings.FileName;
        try
        {
            ImportedPcbDescriptions = PcbDescription.ImportPcbDescriptions(fileName);
            if (ImportedPcbDescriptions is not {Count: >= 1})
                throw new Exception("Число импортированных плат меньше еденицы, запуск моделирования не возможен");
            ImportedPcbDescriptionsSummary = new ImportedPcbDescriptionsSummary(ImportedPcbDescriptions);
        }
        catch (FileNotFoundException fileNotFoundException)
        {
            _dialogService.ShowMessageBox(this, fileNotFoundException.Message, "Ошибка!", icon: MessageBoxImage.Error);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            MessageViewModel.Message = exception.Message;
        }
    }

    public SimulationInputParamsEm Export() => IsImportFromFile switch
    {
        false => new SimInputPcbGenerationBasedEm(
            techPerYear: TechPerYear,
            goalProductionTimeHours: GoalProductionTimeHours,
            runUpSection: RunUpSection,
            finalTime: FinalTime,
            pcbGenerationParamsEm: new PcbGenerationParamsEm(
                elementCountDistr: ElementCountDistr,
                newElemsPercentDistr: NewElemsPercentDistr,
                elementsDensityDistr: ElementsDensityDistr,
                pinsCountDistr: PinsCountDistr,
                pinDensityDistr: PinDensityDistr,
                numOfLayersDistr: NumOfLayersDistr,
                isDoubleSidePlacementProb: IsDoubleSidePlacementProb,
                isManualSchemeInputProb: IsManualSchemeInputProb)
        ),
        true => new SimInputImportPcbBasedEm(
            techPerYear: TechPerYear,
            goalProductionTimeHours: GoalProductionTimeHours,
            runUpSection: RunUpSection,
            finalTime: FinalTime,
            importedPcbDescriptions: ImportedPcbDescriptions?.ToList() ?? new List<PcbDescription>()
        )
    };

    public void Import(SimulationInputParamsEm inputEm)
    {
        TechPerYear = inputEm.TechPerYear;
        GoalProductionTimeHours = inputEm.GoalProductionTimeHours;
        RunUpSection = inputEm.RunUpSection;
        FinalTime = inputEm.FinalTime;

        switch (inputEm)
        {
            case SimInputImportPcbBasedEm importPcbBased:
                TabIndex = 1;
                ImportedPcbDescriptions = importPcbBased.ImportedPcbDescriptions.ToList();
                ImportedPcbDescriptionsSummary = new ImportedPcbDescriptionsSummary(ImportedPcbDescriptions);
                break;
            case SimInputPcbGenerationBasedEm pcbGenerationBased:
                TabIndex = 0;

                ElementCountDistr = pcbGenerationBased.PcbGenerationParamsEm.ElementCountDistr;
                NewElemsPercentDistr = pcbGenerationBased.PcbGenerationParamsEm.NewElemsPercentDistr;
                ElementsDensityDistr = pcbGenerationBased.PcbGenerationParamsEm.ElementsDensityDistr;
                PinsCountDistr = pcbGenerationBased.PcbGenerationParamsEm.PinsCountDistr;
                PinDensityDistr = pcbGenerationBased.PcbGenerationParamsEm.PinDensityDistr;
                NumOfLayersDistr = pcbGenerationBased.PcbGenerationParamsEm.NumOfLayersDistr;
                IsDoubleSidePlacementProb = pcbGenerationBased.PcbGenerationParamsEm.IsDoubleSidePlacementProb;
                IsManualSchemeInputProb = pcbGenerationBased.PcbGenerationParamsEm.IsManualSchemeInputProb;

                ElementCountDistr.RndSource = _rndSource;
                NewElemsPercentDistr.RndSource = _rndSource;
                ElementsDensityDistr.RndSource = _rndSource;
                PinsCountDistr.RndSource = _rndSource;
                PinDensityDistr.RndSource = _rndSource;
                NumOfLayersDistr.RndSource = _rndSource;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(inputEm));
        }
    }
}

public class ImportedPcbDescriptionsSummary
{
    public double ElementCount { get; }
    public double NewElemsPercent { get; }
    public double ElementsDensity { get; }
    public double PinsCount { get; }
    public double PinDensity { get; }
    public double NumOfLayers { get; }
    public double DoubleSidePlacementPercent { get; }
    public double ManualSchemeInputPercent { get; }

    public ImportedPcbDescriptionsSummary(IReadOnlyCollection<PcbDescription> importedPcbDescriptions)
    {
        double count = importedPcbDescriptions.Count;

        ElementCount = importedPcbDescriptions.Average(description => description.ElementsCount);
        NewElemsPercent = importedPcbDescriptions.Average(description => description.NewElemsPercent);
        ElementsDensity = importedPcbDescriptions.Average(description => description.ElementsDensity);
        PinsCount = importedPcbDescriptions.Average(description => description.PinsCount);
        PinDensity = importedPcbDescriptions.Average(description => description.PinDensity);
        NumOfLayers = importedPcbDescriptions.Average(description => description.NumberOfLayers);

        DoubleSidePlacementPercent =
            importedPcbDescriptions.Count(description => description.IsDoubleSidePlacement) / count;
        ManualSchemeInputPercent =
            importedPcbDescriptions.Count(description => description.IsManualSchemeInput) / count;
    }
}