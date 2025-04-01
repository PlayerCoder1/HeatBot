using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;
using Log = Serilog.Log;
using System.Diagnostics;

namespace HeatBot
{
    public partial class Form1 : Form
    {
        private ImageDetector imageDetector;
        private PlayerCoordinateManager playerCoordinateManager;
        private readonly PodsDetector podsDetector;
        private readonly FightHandler fightHandler;
        private bool hasZaapTeleported = false;
        private CancellationTokenSource scriptCancellationTokenSource;
        private SchedulerManager schedulerManager;
        private ScriptingManager.Script lastLoadedScript;

        public List<CheckBox> combatImageCheckboxes { get; private set; }
        private FlowLayoutPanel combatPanel;

        public CheckBox chkFrene;
        public CheckBox chkOrtie;
        public CheckBox chkPaysan;
        public CheckBox chkAvoine;
        public CheckBox chkErable;
        public CheckBox chkNoyer;
        public CheckBox chkSauge;
        public CheckBox chkFer;
        public CheckBox chkEtain;
        public CheckBox chkBronze;
        public CheckBox chkOrme;
        public CheckBox chkTrefle;
        public CheckBox chkKobalte;
        public CheckBox chkChanvre;
        public CheckBox chkHoublon;
        public CheckBox chkLin;
        public CheckBox chkOrge;
        public CheckBox chkCharme;
        public CheckBox chkChataigne;
        public CheckBox chkMerisier;
        public CheckBox chkSeigle;
        public CheckBox chkMenthe;
        public CheckBox chkEldeweiss;
        public CheckBox chkIf;
        public CheckBox chkCombat;
        public CheckBox chkBelladone;
        public CheckBox chkMandragore;
        public CheckBox chkPandouille;
        public CheckBox chkOrchider;
        public CheckBox chkGinseng;
        public CheckBox chkPerceNeige;
        public CheckBox chkBambou;
        public CheckBox chkBambouSacre;
        public CheckBox chkEbene;
        public CheckBox chkOliviolet;
        public CheckBox chkBombu;
        public CheckBox chkKaliptus;
        public CheckBox chkNoisetier;
        public CheckBox chkPin;
        public CheckBox chkTremble;
        public CheckBox chkFrostiz;
        public CheckBox chkMais;
        public CheckBox chkMalt;
        public CheckBox chkMillet;
        public TextBox txtDefaultX;
        public TextBox txtDefaultY;
        public TextBox txtY;
        public TextBox txtX;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            LogConsoleForm.Instance.Show();
            LogConsoleForm.Instance.AppendLog("HeatBot Console Opened", Color.Cyan);
            schedulerManager = new SchedulerManager(StartBot, StopBot);
            schedulerManager.StartScheduler();

        }
        public static class Logger
        {
            public static void LogMessage(string message, Color color)
            {
                LogConsoleForm.Instance.AppendLog(message, color);
            }

            public static void LogMessage(string message)
            {
                LogMessage(message, Color.White);
            }
        }
        public void OpenSchedulerForm()
        {
            using (var schedulerForm = new SchedulerForm())
            {
                schedulerForm.ShowDialog();
                schedulerManager.ReloadSchedule();
            }
        }

        public Form1()
        {
            InitializeComponent();
            var form1UI = new Form1UI(this);
            form1UI.InitializeUI();
            Load += (s, e) =>
            {
                LogConsoleForm.Instance.Show();
            };

            combatImageCheckboxes = new List<CheckBox>();

            int defaultX = int.TryParse(txtDefaultX.Text, out int x) ? x : 1;
            int defaultY = int.TryParse(txtDefaultY.Text, out int y) ? y : -4;

            playerCoordinateManager = new PlayerCoordinateManager(defaultX, defaultY);
            imageDetector = new ImageDetector();
            MapChange.Initialize(imageDetector);
            podsDetector = new PodsDetector();
            fightHandler = new FightHandler(combatImageCheckboxes, imageDetector);

            UpdateCoordinateDisplay();
            StartFightDetectionLoop();

        }
        private void StartFightDetectionLoop()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (fightHandler.HandleFight())
                    {

                    }
                    Thread.Sleep(150);
                }
            });
        }

        private static readonly Dictionary<string, Rectangle> BankInteractionAreas = new Dictionary<string, Rectangle>
{
    { "bank.astrub", new Rectangle(1065, 288, 130, 108) },
    { "bank.amakna", new Rectangle(917, 242, 89, 62) },
    { "bank.sufokia", new Rectangle(1089, 428, 36, 55) }
};

        private static readonly Dictionary<string, List<Point>> BankClickSequences = new Dictionary<string, List<Point>>
{
    {
        "npc.bank.astrub",
        new List<Point>
        {
            new Point(988, 401),
            new Point(1203, 428),
            new Point(1220, 253),
            new Point(1285, 325),
            new Point(1230, 182),
            new Point(672, 688)
        }
    },
    {
        "npc.bank.amakna",
        new List<Point>
        {
            new Point(1075, 457),
            new Point(1227, 504),
            new Point(1222, 253),
            new Point(1278, 319),
            new Point(1226, 177),
            new Point(766, 671)
        }
    },
    {
        "npc.bank.sufokia",
        new List<Point>
        {
            new Point(1074, 516),
            new Point(1254, 556),
            new Point(1217, 255),
            new Point(1277, 328),
            new Point(1232, 180),
            new Point(760, 804)
        }
    }
};

        public void LoadAndExecuteScript(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json",
                Title = "Select a Script File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string scriptPath = openFileDialog.FileName;
                try
                {
                    Console.WriteLine("Loading script from: " + scriptPath);
                    ScriptingManager.Script script = ScriptingManager.LoadScript(scriptPath);

                    if (script != null && script.Clicks.Count > 0)
                    {
                        Console.WriteLine("Script loaded successfully.");

                        lastLoadedScript = script;
                        StartDetectionWithScript(script);
                    }
                    else
                    {
                        MessageBox.Show("The script is empty or invalid.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading script: {ex.Message}");
                    MessageBox.Show($"Error loading script: {ex.Message}");
                }
            }
        }
        private void ExecuteLastLoadedScript()
        {
            if (lastLoadedScript != null)
            {
                Console.WriteLine("Executing last loaded script from memory...");
                Logger.LogMessage("Executing last loaded script...", Color.Cyan);

                StartDetectionWithScript(lastLoadedScript);
            }
            else
            {
                Console.WriteLine("No script found in memory to execute.");
                Logger.LogMessage("No script was previously loaded.", Color.Yellow);
            }
        }

        private void StartBot()
        {
            if (!isInFight)
            {
                Console.WriteLine("Starting bot based on schedule...");
                Logger.LogMessage("Starting the bot...", Color.Cyan);

                try
                {

                    var existingProcesses = Process.GetProcessesByName("Dofus");
                    if (existingProcesses.Length > 0)
                    {
                        Console.WriteLine("Dofus.exe is already running. No action taken.");
                        Logger.LogMessage("Dofus is already running.", Color.Yellow);

                        ExecuteLastLoadedScript();
                        return;
                    }

                    string zaapUrl = "zaap://app/games/game/dofus/dofus3?launch";

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = zaapUrl,
                        UseShellExecute = true
                    });

                    Console.WriteLine("Game launched successfully using zaap:// protocol.");
                    Logger.LogMessage("Game Launched Successfully", Color.Cyan);

                    Task.Delay(20000).Wait();

                    ExecuteLastLoadedScript();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to launch the game: {ex.Message}");
                    Logger.LogMessage($"Failed to launch the game: {ex.Message}", Color.Red);
                }
            }
        }

        private void StopBot()
        {
            Console.WriteLine("Stopping bot based on schedule...");
            Logger.LogMessage("Time is up, closing the game...", Color.Red);
            StopScript();
            Thread.Sleep(10000);
            try
            {

                var processes = Process.GetProcessesByName("Dofus");

                foreach (var process in processes)
                {

                    process.Kill();
                    process.WaitForExit();
                    Console.WriteLine($"Process {process.ProcessName} (ID: {process.Id}) has been terminated.");
                    Logger.LogMessage($"Process {process.ProcessName} (ID: {process.Id}) has been terminated.", Color.Red);
                }

                if (processes.Length == 0)
                {
                    Console.WriteLine("No instances of Dofus.exe were running.");
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"An error occurred while trying to terminate Dofus.exe: {ex.Message}");
            }
        }
        public volatile bool isInFight = false;
        public void StopScript()
        {
            if (scriptCancellationTokenSource != null)
            {
                scriptCancellationTokenSource.Cancel();
                scriptCancellationTokenSource.Dispose();
                scriptCancellationTokenSource = null;
            }

            Console.WriteLine("[INFO] Stop button clicked. Script execution will stop.");
            Logger.LogMessage("Script Has Been Stopped.", Color.Red);
        }

        private void StartDetectionWithScript(ScriptingManager.Script script)
        {
            scriptCancellationTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = scriptCancellationTokenSource.Token;

            Task.Run(() =>
            {
                Console.WriteLine($"[INFO] Starting execution from player coordinates: ({playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY})");
                Logger.LogMessage($"Starting Script at: ({playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY})", Color.Cyan);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {

                        if (isInFight)
                        {
                            Console.WriteLine("[INFO] In fight. Pausing resource detection and scripted actions.");
                            Thread.Sleep(500);
                            continue;
                        }

                        double podsFillPercentage = podsDetector.DetectPodsFillPercentage();
                        if (podsFillPercentage >= 80)
                        {
                            Console.WriteLine("[INFO] Pods are full.");
                            Logger.LogMessage("Pods are full", Color.Cyan);

                            if (script.AutoZaap?.Enabled == true && !string.IsNullOrEmpty(script.AutoZaap.Location) && !hasZaapTeleported)
                            {
                                Console.WriteLine($"[INFO] Teleporting to Zaap: {script.AutoZaap.Location}");
                                Logger.LogMessage($"Teleporting to Zaap: {script.AutoZaap.Location}", Color.Cyan);
                                var zaapMenu = new ZaapMenu(playerCoordinateManager);
                                if (!zaapMenu.TeleportToLocation(script.AutoZaap.Location))
                                {
                                    Console.WriteLine($"[ERROR] Failed to teleport to Zaap location: {script.AutoZaap.Location}");
                                    Logger.LogMessage($"Failed to teleport to Zaap location: {script.AutoZaap.Location}", Color.Red);
                                }
                                else
                                {
                                    Console.WriteLine("[INFO] Teleportation successful. Transitioning to bank actions.");
                                    Logger.LogMessage("Zaap teleportation done. Going Banking...", Color.Cyan);
                                    hasZaapTeleported = true;
                                    ExecuteBankingActions(script.Bank);
                                    continue;
                                }
                            }
                            else
                            {

                                Console.WriteLine("[INFO] Auto-Zaap disabled. Executing bank actions instead.");
                                Logger.LogMessage("Auto-Zaap disabled. Walking To The Bank...", Color.Cyan);
                                ExecuteBankingActions(script.Bank);
                                continue;
                            }
                        }

                        if (podsFillPercentage < 80)
                        {
                            hasZaapTeleported = false;
                        }

                        bool foundResources = imageDetector.DetectAndClick(GetResourceList(), playerCoordinateManager);
                        if (!foundResources)
                        {
                            Console.WriteLine("[INFO] No resources found. Checking for scripted actions.");
                            HandleScriptActions(script);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "[ERROR] Exception in detection loop.");
                    }

                    Thread.Sleep(500);
                }

                Console.WriteLine("[INFO] Script execution stopped.");
                Logger.LogMessage("The Script Is Now Stopped.", Color.Red);

            }, cancellationToken);
        }
        private List<string> GetResourceList()
        {
            List<string> resourcesToDetect = new List<string>();

            if (chkFrene != null && chkFrene.Checked) resourcesToDetect.AddRange(new[] { "frene1", "frene2", "frene3", "frene4", "frene5", "frene6", "frene7" });

            if (chkOrtie != null && chkOrtie.Checked) resourcesToDetect.AddRange(new[] { "ortie1", "ortie2", "ortie3", "ortie4" });

            if (chkPaysan != null && chkPaysan.Checked) resourcesToDetect.AddRange(new[] { "blé1", "blé2", "blé3", "blé4", "blé5", "blé6", "blé7" });

            if (chkAvoine != null && chkAvoine.Checked) resourcesToDetect.AddRange(new[] { "avoine1", "avoine2", "avoine3", "avoine4", "avoine5", "avoine6", "avoine7", "avoine8", "avoine9", "avoine10", "avoine11", });

            if (chkChanvre != null && chkChanvre.Checked) resourcesToDetect.AddRange(new[] { "chanvre1", "chanvre2" });

            if (chkHoublon != null && chkHoublon.Checked) resourcesToDetect.AddRange(new[] { "houblon1", "houblon2" });

            if (chkLin != null && chkLin.Checked) resourcesToDetect.AddRange(new[] { "lin1", "lin2", "lin3" });

            if (chkOrge != null && chkOrge.Checked) resourcesToDetect.AddRange(new[] { "orge1", "orge2", "orge3", "orge4", "orge5", "orge6", "orge7", "orge8", "orge9", "orge10" });

            if (chkSeigle != null && chkSeigle.Checked) resourcesToDetect.AddRange(new[] { "seigle1", "seigle2", "seigle3" });

            if (chkFrostiz != null && chkFrostiz.Checked) resourcesToDetect.AddRange(new[] { "frostiz1", "frostiz2", "frostiz3" });

            if (chkMais != null && chkMais.Checked) resourcesToDetect.AddRange(new[] { "mais1", "mais2", "mais3" });

            if (chkMalt != null && chkMalt.Checked) resourcesToDetect.AddRange(new[] { "malt1", "malt2", "malt3", "malt4" });

            if (chkMillet != null && chkMillet.Checked) resourcesToDetect.AddRange(new[] { "millet1", "millet2", "millet3", "millet4" });

            if (chkCharme != null && chkCharme.Checked) resourcesToDetect.AddRange(new[] { "charme1", "charme2", "charme3", "charme4" });

            if (chkChataigne != null && chkChataigne.Checked) resourcesToDetect.AddRange(new[] { "chataigne1", "chataigne2", "chataigne3", "chataigne4" });

            if (chkErable != null && chkErable.Checked) resourcesToDetect.AddRange(new[] { "erable1", "erable2", "erable3" });

            if (chkMerisier != null && chkMerisier.Checked) resourcesToDetect.AddRange(new[] { "merisier1", "merisier2", "merisier3" });

            if (chkNoyer != null && chkNoyer.Checked) resourcesToDetect.AddRange(new[] { "noyer1", "noyer2" });

            if (chkOrme != null && chkOrme.Checked) resourcesToDetect.AddRange(new[] { "orme1", "orme2" });

            if (chkIf != null && chkIf.Checked) resourcesToDetect.AddRange(new[] { "if1", "if2", "if3" });

            if (chkBambou != null && chkBambou.Checked) resourcesToDetect.AddRange(new[] { "bambou1", "bambou2", "bambou3", "bambou4" });

            if (chkBambouSacre != null && chkBambouSacre.Checked) resourcesToDetect.AddRange(new[] { "bambou-sacré1", "bambou-sacré2", "bambou-sacré3" });

            if (chkEbene != null && chkEbene.Checked) resourcesToDetect.AddRange(new[] { "ebene1", "ebene2" });

            if (chkOliviolet != null && chkOliviolet.Checked) resourcesToDetect.AddRange(new[] { "oliviolet1", "oliviolet2", "oliviolet3" });

            if (chkBombu != null && chkBombu.Checked) resourcesToDetect.AddRange(new[] { "bombu1", "bombu2" });

            if (chkKaliptus != null && chkKaliptus.Checked) resourcesToDetect.AddRange(new[] { "kaliptus1", "kaliptus2", "kaliptus3" });

            if (chkNoisetier != null && chkNoisetier.Checked) resourcesToDetect.AddRange(new[] { "noisetier1", "noisetier2", "noisetier3" });

            if (chkPin != null && chkPin.Checked) resourcesToDetect.AddRange(new[] { "pin1", "pin2" });

            if (chkTremble != null && chkTremble.Checked) resourcesToDetect.AddRange(new[] { "tremble1", "tremble2", "tremble3", "tremble4" });

            if (chkSauge != null && chkSauge.Checked) resourcesToDetect.AddRange(new[] { "sauge1", "sauge2", "sauge3" });

            if (chkTrefle != null && chkTrefle.Checked) resourcesToDetect.AddRange(new[] { "trefle1", "trefle2" });

            if (chkMenthe != null && chkMenthe.Checked) resourcesToDetect.AddRange(new[] { "menthe1", "menthe2" });

            if (chkEldeweiss != null && chkEldeweiss.Checked) resourcesToDetect.AddRange(new[] { "eldeweiss1", "eldeweiss2" });

            if (chkBelladone != null && chkBelladone.Checked) resourcesToDetect.AddRange(new[] { "belladone1", "belladone2" });

            if (chkMandragore != null && chkMandragore.Checked) resourcesToDetect.AddRange(new[] { "mandragore1", "mandragore2" });

            if (chkPandouille != null && chkPandouille.Checked) resourcesToDetect.AddRange(new[] { "pandouille1", "pandouille2", "pandouille3", "pandouille4" });

            if (chkOrchider != null && chkOrchider.Checked) resourcesToDetect.AddRange(new[] { "orchidée1", "orchidée2", "orchidée3", "orchidée4", "orchidée5", "orchidée6" });

            if (chkGinseng != null && chkGinseng.Checked) resourcesToDetect.AddRange(new[] { "ginseng1", "ginseng2", "ginseng3", "ginseng4" });

            if (chkPerceNeige != null && chkPerceNeige.Checked) resourcesToDetect.AddRange(new[] { "perce-neige1", "perce-neige2", "perce-neige3" });

            if (chkFer != null && chkFer.Checked) resourcesToDetect.AddRange(new[] { "fer1", "fer2", "fer3", "fer4", "fer5", "fer6" });

            if (chkEtain != null && chkEtain.Checked) resourcesToDetect.AddRange(new[] { "etain1" });

            if (chkBronze != null && chkBronze.Checked) resourcesToDetect.AddRange(new[] { "bronze1", "bronze2" });

            if (chkKobalte != null && chkKobalte.Checked) resourcesToDetect.AddRange(new[] { "kobalte" });
            return resourcesToDetect;
        }

        private void HandleScriptActions(ScriptingManager.Script script)
        {
            var matchingAction = script.Clicks.Find(action =>
                action.X == playerCoordinateManager.CurrentX &&
                action.Y == playerCoordinateManager.CurrentY);

            if (matchingAction != null)
            {
                Console.WriteLine($"[INFO] Executing scripted action: {matchingAction.Action}");
                ExecuteScriptAction(matchingAction);
            }
            else
            {
                Console.WriteLine($"[WARNING] No actions found for coordinates ({playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY}).");
                Logger.ReferenceEquals($"No actions found for coordinates ({playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY}).", Color.Yellow);
            }
        }
        private void ExecuteBankingActions(List<ScriptingManager.ActionAtCoordinate> bankActions)
        {
            if (bankActions == null || bankActions.Count == 0)
            {
                Console.WriteLine("No banking actions defined in the script.");
                return;
            }

            Console.WriteLine($"Checking for banking actions at coordinates ({playerCoordinateManager.CurrentX}, {playerCoordinateManager.CurrentY})");

            foreach (var action in bankActions)
            {
                if (action.X == playerCoordinateManager.CurrentX && action.Y == playerCoordinateManager.CurrentY)
                {
                    Console.WriteLine($"Executing banking action: {action.Action} at ({action.X}, {action.Y})");
                    Logger.LogMessage($"[Banking] Going: {action.Action} at ({action.X}, {action.Y})", Color.Green);

                    switch (action.Action.ToLower())
                    {
                        case "up":
                            MapChange.ChangeMap("up");
                            playerCoordinateManager.MoveUp();
                            break;
                        case "down":
                            MapChange.ChangeMap("down");
                            playerCoordinateManager.MoveDown();
                            break;
                        case "left":
                            MapChange.ChangeMap("left");
                            playerCoordinateManager.MoveLeft();
                            break;
                        case "right":
                            MapChange.ChangeMap("right");
                            playerCoordinateManager.MoveRight();
                            break;
                        case "bank":
                            SimulateBankInteraction(action.X, action.Y, action.BankStyle);
                            break;
                        default:
                            Console.WriteLine($"Invalid banking action '{action.Action}' specified.");
                            Logger.LogMessage($"Invalid banking action '{action.Action}' specified.", Color.Red);
                            break;
                    }

                    MapChange.WaitForBlackScreenToDisappear(playerCoordinateManager);
                    UpdateCoordinateDisplay();
                }

            }
        }
        private void SimulateBankInteraction(int x, int y, string bankStyle)
        {
            Console.WriteLine($"Simulating bank interaction at ({x}, {y}) with style {bankStyle}.");
            Logger.LogMessage($"Banking ({x}, {y}) at {bankStyle}.", Color.Cyan);

            if (BankInteractionAreas.ContainsKey(bankStyle))
            {
                var interactionArea = BankInteractionAreas[bankStyle];
                int clickX = interactionArea.X + interactionArea.Width / 2;
                int clickY = interactionArea.Y + interactionArea.Height / 2;

                Console.WriteLine($"Entering the bank: Clicking at ({clickX}, {clickY}).");
                Logger.LogMessage($"Entering the Bank", Color.Green);
                Thread.Sleep(2000);
                ClickSimulate.SimulateClick(clickX, clickY);

                Thread.Sleep(6000);
            }

            if (BankClickSequences.ContainsKey(bankStyle))
            {

                string associatedBank = bankStyle.Replace("npc.", "");
                if (!ValidateBankEntry(associatedBank))
                {
                    Console.WriteLine($"Bank entry validation failed for {associatedBank}. Unable to perform NPC interactions.");
                    return;
                }

                var clickSequence = BankClickSequences[bankStyle];
                foreach (var click in clickSequence)
                {
                    Console.WriteLine($"Simulating click at position ({click.X}, {click.Y}).");
                    ClickSimulate.SimulateClick(click.X, click.Y);
                    Thread.Sleep(2000);
                }

                Console.WriteLine("Bank interaction completed.");
                Logger.LogMessage("Finished Banking, Back to hard labour...", Color.Green);
            }
            else if (!BankInteractionAreas.ContainsKey(bankStyle))
            {
                Console.WriteLine($"Invalid or unsupported bank style: {bankStyle}. Cannot proceed with bank interaction.");
            }
        }

        private bool ValidateBankEntry(string bankStyle)
        {
            if (BankInteractionAreas.ContainsKey(bankStyle))
            {
                var interactionArea = BankInteractionAreas[bankStyle];

                Console.WriteLine($"Validating bank entry for style: {bankStyle} at area {interactionArea}.");

                return true;
            }

            return false;
        }

        private void ExecuteScriptAction(ScriptingManager.ActionAtCoordinate action)
        {
            Console.WriteLine($"Executing action '{action.Action}' at coordinates ({action.X}, {action.Y}).");
            Logger.LogMessage($"Going '{action.Action}' at ({action.X}, {action.Y}).", Color.Green);

            switch (action.Action.ToLower())
            {
                case "up":
                    MapChange.ChangeMap("up");
                    playerCoordinateManager.MoveUp();
                    break;
                case "down":
                    MapChange.ChangeMap("down");
                    playerCoordinateManager.MoveDown();
                    break;
                case "left":
                    MapChange.ChangeMap("left");
                    playerCoordinateManager.MoveLeft();
                    break;
                case "right":
                    MapChange.ChangeMap("right");
                    playerCoordinateManager.MoveRight();
                    break;
                case "zaap":
                    if (!string.IsNullOrEmpty(action.Location))
                    {
                        var zaapMenu = new ZaapMenu(playerCoordinateManager);
                        if (!zaapMenu.TeleportToLocation(action.Location))
                        {
                            Console.WriteLine($"Failed to teleport to location: {action.Location}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Zaap action missing location property at ({action.X}, {action.Y}).");
                    }
                    break;
                default:
                    Console.WriteLine($"Invalid action '{action.Action}' specified.");
                    break;
            }

            MapChange.WaitForBlackScreenToDisappear(playerCoordinateManager);
            UpdateCoordinateDisplay();
        }
        public void ResetCoordinates()
        {

            int newDefaultX = int.TryParse(txtDefaultX.Text, out int x) ? x : 4;
            int newDefaultY = int.TryParse(txtDefaultY.Text, out int y) ? y : -20;

            playerCoordinateManager = new PlayerCoordinateManager(newDefaultX, newDefaultY);
            UpdateCoordinateDisplay();

            Console.WriteLine($"Player default coordinates reset to: X = {newDefaultX}, Y = {newDefaultY}");
        }

        private void UpdateCoordinateDisplay()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateCoordinateDisplay));
                return;
            }

            txtX.Text = playerCoordinateManager.CurrentX.ToString();
            txtY.Text = playerCoordinateManager.CurrentY.ToString();

            Console.WriteLine($"Player coordinates updated: X = {playerCoordinateManager.CurrentX}, Y = {playerCoordinateManager.CurrentY}");
            Logger.LogMessage($"Current Map: X = {playerCoordinateManager.CurrentX}, Y = {playerCoordinateManager.CurrentY}", Color.Cyan);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}