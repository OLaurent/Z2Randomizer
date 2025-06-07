using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using Z2Randomizer.Core;
using Z2Randomizer.Core.Overworld;
using Z2Randomizer.Core.Flags;
using System.Threading.Tasks;
using CrossPlatformUI.Helpers;
using Avalonia.Controls;

namespace CrossPlatformUI.ViewModels;

public partial class MainWindowViewModel : INotifyPropertyChanged
{
    private BackgroundWorker romWorker;
    private readonly IFilePickerService _filePickerService;
    private readonly Random r;
    private string _romFile;
    private string _seed;
    private string _flags;
    private int? _startingLives;
    public ICommand CreateSeedCommand { get; }
    public ICommand MaxRandoCommand { get; }
    public ICommand SelectFileCommand { get; }
    public ICommand GenerateRomCommand { get; }

    private RandomizerConfiguration config;

    public MainWindowViewModel(IFilePickerService filePickerService)
    {
        _filePickerService = filePickerService;
        config = new RandomizerConfiguration();
        r = new Random();
        MaxRandoCommand = new RelayCommand(ExecuteMaxRando);
        CreateSeedCommand = new RelayCommand(ExecuteCreateSeed);
        SelectFileCommand = new AsyncRelayCommand(ExecuteSelectFile);
        GenerateRomCommand = new RelayCommand(ExecuteGenerateRom);
    }

    public string RomFile
    {
        get => _romFile;
        set { _romFile = value; OnPropertyChanged(); }
    }

    public string Seed
    {
        get => _seed;
        set { _seed = value; OnPropertyChanged(); }
    }

    public string Flags
    {
        get => _flags;
        set { _flags = value; OnPropertyChanged(); }
    }



    public int? StartingLives
    {
        get => _startingLives;
        set { _startingLives = value; OnPropertyChanged(); }
    }


    private void ExecuteMaxRando(object? parameter)
    {
        Console.WriteLine("Executing Max Rando Command");
        config = RandomizerConfiguration.FromLegacyFlags("hEAB0thBYbWs4WEb14HGg+!59rtgbtJ!AAFRA");

        config = new RandomizerConfiguration("hEAB0thBYbWs4WEb14HGg+!59rtgbtJ!AAFRA");
        StartingLives = config.StartingLives switch
        {
            1 => 0,
            2 => 1,
            3 => 2,
            4 => 3,
            5 => 4,
            8 => 5,
            16 => 6,
            null => 7
        };
        Console.WriteLine("StartingLives set to: " + StartingLives);

        Flags = config.Serialize();
    }

    private void ExecuteCreateSeed(object? parameter)
    {
        Console.WriteLine("Executing Create Seed Command");
        Seed = r.Next(1000000000).ToString();
    }

    private async Task ExecuteSelectFile(object? parameter)
    {
        Console.WriteLine("Sélection du fichier ROM demandée.");

        var file = await _filePickerService.PickFileAsync("Sélectionner une ROM", new[] { "nes" });
        if (!string.IsNullOrEmpty(file))
        {
            RomFile = file;
        }
    }

    private void ExecuteGenerateRom(object? parameter)
    {
        Console.WriteLine("Génération de la ROM demandée.");

        if (string.IsNullOrEmpty(RomFile))
        {
            Console.WriteLine("Aucun fichier ROM sélectionné.");
            return;
        }
        if (string.IsNullOrEmpty(Seed))
        {
            Console.WriteLine("Aucune seed définie.");
            return;
        }
        if (string.IsNullOrEmpty(Flags))
        {
            Console.WriteLine("Aucun flag défini.");
            return;
        }

        try
        {
            Console.WriteLine("Démarrage de la génération de la ROM...");
            Console.WriteLine($"Fichier ROM : {RomFile}");
            Console.WriteLine($"Seed : {Seed}");
            Console.WriteLine($"Flags : {Flags}");
            // Parse the flags and seed, and initialize the configuration
            config = new RandomizerConfiguration(Flags);
            config.Seed = Int32.Parse(Seed.Trim());
            config.FileName = RomFile.Trim();

            romWorker = new BackgroundWorker();
            romWorker.DoWork += RomWorker_DoWork;
            romWorker.RunWorkerCompleted += RomWorker_RunWorkerCompleted;
            romWorker.WorkerSupportsCancellation = true;
            romWorker.WorkerReportsProgress = true;
            romWorker.ProgressChanged += RomWorker_ProgressChanged;

            if (!romWorker.IsBusy)
            {
                romWorker.RunWorkerAsync();
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine("Erreur lors de la génération de la ROM : " + ex.Message);
        }
    }

    private void RomWorker_DoWork(object sender, DoWorkEventArgs e)
    {
        var worker = sender as BackgroundWorker;
        try
        {
            new Hyrule(config, worker);

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }
        catch (Exception ex)
        {
            e.Result = ex;
        }
    }

    private void RomWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            Console.WriteLine("Génération annulée.");
        }
        else if (e.Error != null || e.Result is Exception)
        {
            Console.WriteLine("Erreur lors de la génération : " + (e.Error?.Message ?? ((Exception)e.Result).Message));
        }
        else
        {
            Console.WriteLine("ROM générée avec succès !");
        }
    }

    private void RomWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        // Exemple : afficher la progression dans la console ou mettre à jour une propriété liée à la progression
        Console.WriteLine($"Progression : {e.ProgressPercentage}%");
        if (e.ProgressPercentage == 2)
        {
            Console.WriteLine("Generating Western Hyrule");
        }
        else if (e.ProgressPercentage == 3)
        {
            Console.WriteLine("Generating Death Mountain");
        }
        else if (e.ProgressPercentage == 4)
        {
            Console.WriteLine("Generating East Hyrule");
        }
        else if (e.ProgressPercentage == 5)
        {
            Console.WriteLine("Generating Maze Island");
        }
        else if (e.ProgressPercentage == 6)
        {
            Console.WriteLine("Shuffling Items and Spells");
        }
        else if (e.ProgressPercentage == 7)
        {
            Console.WriteLine("Running Seed Completability Checks");
        }
        else if (e.ProgressPercentage == 8)
        {
            Console.WriteLine("Generating Hints");
        }
        else if (e.ProgressPercentage == 9)
        {
            Console.WriteLine("Finishing up");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
