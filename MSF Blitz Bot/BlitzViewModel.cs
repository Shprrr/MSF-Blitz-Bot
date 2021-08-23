using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MSFBlitzBot.GamePages;
using MSFBlitzBot.MachineLearning;

namespace MSFBlitzBot
{
    public class BlitzViewModel : GamePageViewModel
    {
        public enum AutoState
        {
            None,
            BestTarget,
            HighestTotal,
            TrainWorthy
        }

        private BlitzPage model;
        private Color[] _colorCheck;

        private string playerHeroes;
        private string opponentHeroes;
        private int? opponentTotalTeam1;
        private int? opponentTotalTeam2;
        private int? opponentTotalTeam3;
        private float? opponentPredictionTeam1;
        private float? opponentPredictionTeam2;
        private float? opponentPredictionTeam3;
        private bool opponentIsBestTargetTeam1;
        private bool opponentIsBestTargetTeam2;
        private bool opponentIsBestTargetTeam3;
        private bool opponentIsHighestTotalTeam1;
        private bool opponentIsHighestTotalTeam2;
        private bool opponentIsHighestTotalTeam3;
        private bool opponentIsTrainWorthyTeam1;
        private bool opponentIsTrainWorthyTeam2;
        private bool opponentIsTrainWorthyTeam3;
        private readonly MLTask[] mlThreads = new MLTask[3];
        private string combatState;
        private bool needRetrain = true;
        private BlitzFight currentFight;
        private bool autoScanCompleted;

        public string PlayerHeroes { get => playerHeroes; set { playerHeroes = value; RaisePropertyChanged(nameof(PlayerHeroes)); } }
        public string OpponentHeroes { get => opponentHeroes; set { opponentHeroes = value; RaisePropertyChanged(nameof(OpponentHeroes)); } }
        public int? OpponentTotalTeam1 { get => opponentTotalTeam1; set { opponentTotalTeam1 = value; RaisePropertyChanged(nameof(OpponentTotalTeam1)); } }
        public int? OpponentTotalTeam2 { get => opponentTotalTeam2; set { opponentTotalTeam2 = value; RaisePropertyChanged(nameof(OpponentTotalTeam2)); } }
        public int? OpponentTotalTeam3 { get => opponentTotalTeam3; set { opponentTotalTeam3 = value; RaisePropertyChanged(nameof(OpponentTotalTeam3)); } }
        public float? OpponentPredictionTeam1 { get => opponentPredictionTeam1; set { opponentPredictionTeam1 = value; RaisePropertyChanged(nameof(OpponentPredictionTeam1)); } }
        public float? OpponentPredictionTeam2 { get => opponentPredictionTeam2; set { opponentPredictionTeam2 = value; RaisePropertyChanged(nameof(OpponentPredictionTeam2)); } }
        public float? OpponentPredictionTeam3 { get => opponentPredictionTeam3; set { opponentPredictionTeam3 = value; RaisePropertyChanged(nameof(OpponentPredictionTeam3)); } }
        public bool OpponentIsBestTargetTeam1 { get => opponentIsBestTargetTeam1; set { opponentIsBestTargetTeam1 = value; RaisePropertyChanged(nameof(OpponentIsBestTargetTeam1)); } }
        public bool OpponentIsBestTargetTeam2 { get => opponentIsBestTargetTeam2; set { opponentIsBestTargetTeam2 = value; RaisePropertyChanged(nameof(OpponentIsBestTargetTeam2)); } }
        public bool OpponentIsBestTargetTeam3 { get => opponentIsBestTargetTeam3; set { opponentIsBestTargetTeam3 = value; RaisePropertyChanged(nameof(OpponentIsBestTargetTeam3)); } }
        public bool OpponentIsHighestTotalTeam1 { get => opponentIsHighestTotalTeam1; set { opponentIsHighestTotalTeam1 = value; RaisePropertyChanged(nameof(OpponentIsHighestTotalTeam1)); } }
        public bool OpponentIsHighestTotalTeam2 { get => opponentIsHighestTotalTeam2; set { opponentIsHighestTotalTeam2 = value; RaisePropertyChanged(nameof(OpponentIsHighestTotalTeam2)); } }
        public bool OpponentIsHighestTotalTeam3 { get => opponentIsHighestTotalTeam3; set { opponentIsHighestTotalTeam3 = value; RaisePropertyChanged(nameof(OpponentIsHighestTotalTeam3)); } }
        public bool OpponentIsTrainWorthyTeam1 { get => opponentIsTrainWorthyTeam1; set { opponentIsTrainWorthyTeam1 = value; RaisePropertyChanged(nameof(OpponentIsTrainWorthyTeam1)); } }
        public bool OpponentIsTrainWorthyTeam2 { get => opponentIsTrainWorthyTeam2; set { opponentIsTrainWorthyTeam2 = value; RaisePropertyChanged(nameof(OpponentIsTrainWorthyTeam2)); } }
        public bool OpponentIsTrainWorthyTeam3 { get => opponentIsTrainWorthyTeam3; set { opponentIsTrainWorthyTeam3 = value; RaisePropertyChanged(nameof(OpponentIsTrainWorthyTeam3)); } }
        public string CombatState { get => combatState; set { combatState = value; RaisePropertyChanged(nameof(CombatState)); } }
        public bool NeedRetrain { get => needRetrain; set { needRetrain = value; RaisePropertyChanged(nameof(NeedRetrain)); } }
        public AutoState CurrentAutoState { get; set; }

        public static AsyncObservableCollection<BlitzFight> Fights { get; } = new AsyncObservableCollection<BlitzFight>();
        public static AsyncObservableCollection<string> FightsToday { get; } = new AsyncObservableCollection<string>();

        static BlitzViewModel()
        {
            Fights.CollectionChanged += Fights_CollectionChanged;
        }

        private static void Fights_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FightsToday.Clear();
            var fightsToday = Fights.Where(f => f.DateTime.Date == DateTime.UtcNow.Date).Select(f => f.ToString()).ToArray();
            FightsToday.Add($"{Fights.Count - fightsToday.Length} more fights loaded.");
            FightsToday.AddRange(fightsToday);
        }

        public BlitzViewModel()
        {
            PageName = "Blitz";
            currentFight = new BlitzFight();
            CombatState = "Lobby";
        }

        public override void OnOpen()
        {
            model = (BlitzPage)Engine.Singleton.CurrentPage;
        }

        public override void OnClose()
        {
            model = null;
        }

        public static void LoadFights(string path)
        {
            Fights.Clear();
            Fights.AddRange(Newtonsoft.Json.JsonConvert.DeserializeObject<BlitzFight[]>(System.IO.File.ReadAllText(path), new BlitzFightJsonConverter(System.IO.File.GetLastWriteTimeUtc(path))));
        }

        public override void Loop()
        {
            if (CombatState == "Retrain") return;

            model.SetImage(Emulator.GameImage);
            // Check if pixels are modified.
            bool hasColorChanged = _colorCheck == null;
            var array = new Color[10];
            for (var i = 0; i < 10; i++)
            {
                // size 92, 137
                // side 0 index 0 =  57, 343
                // side 0 index 1 = 138, 175
                // side 0 index 2 = 221, 343
                // side 0 index 3 = , 175
                // side 0 index 4 = , 343
                // side 1 index 0 = 1026, 343
                // Odd indexes are considered opponent side (0, 2, 4, 6, 8 vs 1, 3, 5, 7, 9)
                float posx = (i / 2 * 82 + 103f + i % 2 * 969) / 1505;
                float posy = (i / 2 % 2 == 1 ? 243.5f : 411.5f) / 847;
                array[i] = Emulator.GameImage.GetPixel(posx, posy);
                if (_colorCheck != null && array[i] != _colorCheck[i])
                {
                    hasColorChanged = true;
                }
            }
            _colorCheck = array;
            if (!hasColorChanged) return;

            var data = model.GetData();

            var hasPlayerHeroesChanged = currentFight.PlayerHeroes == null || !currentFight.PlayerHeroes.SequenceEqual(data.PlayerHeroes);
            if (CombatState == "Battle" || data.Victory || data.Defeat)
            {
                PlayerHeroes = "";
                OpponentHeroes = "";
                if (!currentFight.IsEmpty && (data.Victory || data.Defeat))
                {
                    autoScanCompleted = true;
                    currentFight.Result = data.Victory ? BlitzFight.FightResult.Victory : BlitzFight.FightResult.Defeat;
                    currentFight.DateTime = DateTime.UtcNow;
                    Fights.Add(currentFight);
                    NeedRetrain = true;
                    CombatState = data.Victory ? "Victory" : "Defeat";
                    currentFight = new BlitzFight();
                    ClearOpponentTeams();
                }
            }
            else
            {
                CombatState = "Lobby";
                currentFight.PlayerHeroes = data.PlayerHeroes;
                currentFight.OpponentHeroes = data.OpponentHeroes;
                PlayerHeroes = string.Join(" + ", data.PlayerHeroes.Select(h => $"{h.Name} ({h.PowerString})"));
                OpponentHeroes = data.HasOpponent ? $"#{data.TeamIndex + 1} " + string.Join(" + ", data.OpponentHeroes.Select(h => $"{h.Name} ({h.PowerString})")) : "";

                switch (data.TeamIndex)
                {
                    case 0:
                        OpponentTotalTeam1 = data.OpponentHeroes.Sum(h => h.Power);
                        break;
                    case 1:
                        OpponentTotalTeam2 = data.OpponentHeroes.Sum(h => h.Power);
                        break;
                    case 2:
                        OpponentTotalTeam3 = data.OpponentHeroes.Sum(h => h.Power);
                        break;
                }
                CheckOpponentTeams();

                if (data.HasOpponent) StartTeamPrediction(data);
            }

            if (data.CanFindOpponent || hasPlayerHeroesChanged && !autoScanCompleted)
            {
                ClearOpponentTeams();
                autoScanCompleted = false;
            }

            if (data.CanFindOpponent || !autoScanCompleted && data.HasOpponent) DoAutoBlitz(data.TeamIndex);
            if (data.HasOpponent) autoScanCompleted = false; // Ready to scan for the next image update after the initial load after a battle.
        }

        private void CheckOpponentTeams()
        {
            var highestTotal = Max(OpponentTotalTeam1.GetValueOrDefault(), OpponentTotalTeam2.GetValueOrDefault(), OpponentTotalTeam3.GetValueOrDefault());
            OpponentIsHighestTotalTeam1 = OpponentTotalTeam1 == highestTotal;
            OpponentIsHighestTotalTeam2 = OpponentTotalTeam2 == highestTotal;
            OpponentIsHighestTotalTeam3 = OpponentTotalTeam3 == highestTotal;

            // Check highest training worth
            var trainingWorths = new float[] { CalculateTrainingWorth(OpponentPredictionTeam1.GetValueOrDefault()), CalculateTrainingWorth(OpponentPredictionTeam2.GetValueOrDefault()), CalculateTrainingWorth(OpponentPredictionTeam3.GetValueOrDefault()) };
            var highestTrainingWorth = Max(trainingWorths);

            OpponentIsTrainWorthyTeam1 = trainingWorths[0] == highestTrainingWorth;
            OpponentIsTrainWorthyTeam2 = trainingWorths[1] == highestTrainingWorth;
            OpponentIsTrainWorthyTeam3 = trainingWorths[2] == highestTrainingWorth;

            if (CheckHighestTotalWithPredictionHigherThan(.9f)) return;
            if (CheckHighestTotalWithPredictionHigherThan(.75f)) return;
            CheckHighestTotalWithPredictionHigherThan(.6f);
        }

        private bool CheckHighestTotalWithPredictionHigherThan(float predictionHigherThan)
        {
            // Get all total who check the condition
            var totals = new int[3];
            if (OpponentPredictionTeam1 >= predictionHigherThan) totals[0] = OpponentTotalTeam1.GetValueOrDefault();
            if (OpponentPredictionTeam2 >= predictionHigherThan) totals[1] = OpponentTotalTeam2.GetValueOrDefault();
            if (OpponentPredictionTeam3 >= predictionHigherThan) totals[2] = OpponentTotalTeam3.GetValueOrDefault();

            var highestTotal = Max(totals);
            if (highestTotal == 0) return false;

            OpponentIsBestTargetTeam1 = totals[0] == highestTotal;
            OpponentIsBestTargetTeam2 = totals[1] == highestTotal;
            OpponentIsBestTargetTeam3 = totals[2] == highestTotal;
            return true;
        }

        private static T Max<T>(params T[] values)
        {
            return values.Max();
        }

        private static float CalculateTrainingWorth(float prediction)
        {
            if (prediction <= .6 && prediction >= .4)
                return 1 - Math.Abs(prediction - .5f);
            if (prediction > .6)
                return Math.Abs(1 - prediction);
            return Math.Abs(prediction);
        }

        private void ClearOpponentTeams()
        {
            OpponentTotalTeam1 = null;
            OpponentTotalTeam2 = null;
            OpponentTotalTeam3 = null;
            OpponentIsHighestTotalTeam1 = false;
            OpponentIsHighestTotalTeam2 = false;
            OpponentIsHighestTotalTeam3 = false;
            ClearPredictions();
        }

        private void ClearPredictions()
        {
            OpponentPredictionTeam1 = null;
            OpponentPredictionTeam2 = null;
            OpponentPredictionTeam3 = null;
            OpponentIsBestTargetTeam1 = false;
            OpponentIsBestTargetTeam2 = false;
            OpponentIsBestTargetTeam3 = false;
            OpponentIsTrainWorthyTeam1 = false;
            OpponentIsTrainWorthyTeam2 = false;
            OpponentIsTrainWorthyTeam3 = false;
        }


        private void DoAutoBlitz(int selectedIndex)
        {
            if (CurrentAutoState == AutoState.None) return;

            if (!OpponentTotalTeam1.HasValue || !OpponentTotalTeam2.HasValue || !OpponentTotalTeam3.HasValue)
            {
                ClickNextOpponent();
                return;
            }

            // Everything is scanned so get desired target.
            switch (CurrentAutoState)
            {
                case AutoState.BestTarget:
                    {
                        var bestTargetIndex = OpponentIsBestTargetTeam1 ? 0 : OpponentIsBestTargetTeam2 ? 1 : OpponentIsBestTargetTeam3 ? 2 : -1;
                        if (bestTargetIndex == -1) return;
                        ClickUntilThisOpponent(selectedIndex, bestTargetIndex);
                    }
                    break;

                case AutoState.HighestTotal:
                    {
                        var highestTotalIndex = OpponentIsHighestTotalTeam1 ? 0 : OpponentIsHighestTotalTeam2 ? 1 : OpponentIsHighestTotalTeam3 ? 2 : -1;
                        if (highestTotalIndex == -1) return;
                        ClickUntilThisOpponent(selectedIndex, highestTotalIndex);
                    }
                    break;

                case AutoState.TrainWorthy:
                    {
                        var trainWorthyIndex = OpponentIsTrainWorthyTeam1 ? 0 : OpponentIsTrainWorthyTeam2 ? 1 : OpponentIsTrainWorthyTeam3 ? 2 : -1;
                        if (trainWorthyIndex == -1) return;
                        ClickUntilThisOpponent(selectedIndex, trainWorthyIndex);
                    }
                    break;
            }
        }

        public static void ClickNextOpponent()
        {
            // 1159, 629  248, 70  total 1505, 847
            MouseControl.ClickArea(new RectangleF(1159f / 1505, 629f / 847, 248f / 1505, 70f / 847));
        }

        private static void ClickUntilThisOpponent(int selectedIndex, int desiredOpponentIndex)
        {
            while (selectedIndex != desiredOpponentIndex)
            {
                ClickNextOpponent();
                selectedIndex++;
                if (selectedIndex > 2) selectedIndex = 0;
            }
        }


        private class MLTask
        {
            public MLTask(BlitzData data, Thread thread)
            {
                Data = data ?? throw new ArgumentNullException(nameof(data));
                Thread = thread ?? throw new ArgumentNullException(nameof(thread));
                Thread.Start(this);
            }

            public BlitzData Data { get; }
            public Thread Thread { get; }
            public bool Cancel { get; set; }
        }
        private void StartTeamPrediction(BlitzData data)
        {
            // ML in thread to not block UI and get result when ready
            if (mlThreads[data.TeamIndex] != null && mlThreads[data.TeamIndex].Thread.ThreadState == (ThreadState.Unstarted | ThreadState.Running))
            {
                mlThreads[data.TeamIndex].Cancel = true;
            }

            mlThreads[data.TeamIndex] = new MLTask(data, new Thread((object parameter) =>
            {
                var taskParameter = (MLTask)parameter;
                if (taskParameter.Cancel || taskParameter.Data.PlayerHeroes.Any(h => !h.Power.HasValue) || taskParameter.Data.OpponentHeroes.Any(h => !h.Power.HasValue)) return;

                MLModelBlitz.ModelInput input = new()
                {
                    PlayerHeroId1 = taskParameter.Data.PlayerHeroes[0].Id,
                    PlayerHeroId2 = taskParameter.Data.PlayerHeroes[1].Id,
                    PlayerHeroId3 = taskParameter.Data.PlayerHeroes[2].Id,
                    PlayerHeroId4 = taskParameter.Data.PlayerHeroes[3].Id,
                    PlayerHeroId5 = taskParameter.Data.PlayerHeroes[4].Id,
                    PlayerHeroPower1 = taskParameter.Data.PlayerHeroes[0].Power.Value,
                    PlayerHeroPower2 = taskParameter.Data.PlayerHeroes[1].Power.Value,
                    PlayerHeroPower3 = taskParameter.Data.PlayerHeroes[2].Power.Value,
                    PlayerHeroPower4 = taskParameter.Data.PlayerHeroes[3].Power.Value,
                    PlayerHeroPower5 = taskParameter.Data.PlayerHeroes[4].Power.Value,
                    OpponentHeroId1 = taskParameter.Data.OpponentHeroes[0].Id,
                    OpponentHeroId2 = taskParameter.Data.OpponentHeroes[1].Id,
                    OpponentHeroId3 = taskParameter.Data.OpponentHeroes[2].Id,
                    OpponentHeroId4 = taskParameter.Data.OpponentHeroes[3].Id,
                    OpponentHeroId5 = taskParameter.Data.OpponentHeroes[4].Id,
                    OpponentHeroPower1 = taskParameter.Data.OpponentHeroes[0].Power.Value,
                    OpponentHeroPower2 = taskParameter.Data.OpponentHeroes[1].Power.Value,
                    OpponentHeroPower3 = taskParameter.Data.OpponentHeroes[2].Power.Value,
                    OpponentHeroPower4 = taskParameter.Data.OpponentHeroes[3].Power.Value,
                    OpponentHeroPower5 = taskParameter.Data.OpponentHeroes[4].Power.Value,
                };
                var prediction = MLModelBlitz.Predict(input);
                if (taskParameter.Cancel) return;
                switch (taskParameter.Data.TeamIndex)
                {
                    case 0:
                        OpponentPredictionTeam1 = prediction.Score;
                        break;
                    case 1:
                        OpponentPredictionTeam2 = prediction.Score;
                        break;
                    case 2:
                        OpponentPredictionTeam3 = prediction.Score;
                        break;
                }
                CheckOpponentTeams();
            }));
        }

        public void Retrain()
        {
            CombatState = "Retrain";
            NeedRetrain = false;

            // Clear old predictions.
            foreach (var mlTask in mlThreads)
            {
                if (mlTask != null && mlTask.Thread.ThreadState == (ThreadState.Unstarted | ThreadState.Running))
                {
                    mlTask.Cancel = true;
                }
            }
            ClearPredictions();

            Task.Run(() =>
            {
                // Get all data to retrain.
                var duplicateDataWithReverseOrder = Fights.SelectMany(f => new[]
                {
                    f,
                    new BlitzFight { PlayerHeroes = f.PlayerHeroes, OpponentHeroes = f.OpponentHeroes.Reverse().ToArray(), Result = f.Result, DateTime = f.DateTime },
                    new BlitzFight { PlayerHeroes = f.PlayerHeroes.Reverse().ToArray(), OpponentHeroes = f.OpponentHeroes, Result = f.Result, DateTime = f.DateTime },
                    new BlitzFight { PlayerHeroes = f.PlayerHeroes.Reverse().ToArray(), OpponentHeroes = f.OpponentHeroes.Reverse().ToArray(), Result = f.Result, DateTime = f.DateTime }
                });
                var modelData = duplicateDataWithReverseOrder.Select(d => new MLModelBlitz.ModelInput
                {
                    PlayerHeroId1 = d.PlayerHeroes[0].Id,
                    PlayerHeroId2 = d.PlayerHeroes[1].Id,
                    PlayerHeroId3 = d.PlayerHeroes[2].Id,
                    PlayerHeroId4 = d.PlayerHeroes[3].Id,
                    PlayerHeroId5 = d.PlayerHeroes[4].Id,
                    PlayerHeroPower1 = d.PlayerHeroes[0].Power.Value,
                    PlayerHeroPower2 = d.PlayerHeroes[1].Power.Value,
                    PlayerHeroPower3 = d.PlayerHeroes[2].Power.Value,
                    PlayerHeroPower4 = d.PlayerHeroes[3].Power.Value,
                    PlayerHeroPower5 = d.PlayerHeroes[4].Power.Value,
                    OpponentHeroId1 = d.OpponentHeroes[0].Id,
                    OpponentHeroId2 = d.OpponentHeroes[1].Id,
                    OpponentHeroId3 = d.OpponentHeroes[2].Id,
                    OpponentHeroId4 = d.OpponentHeroes[3].Id,
                    OpponentHeroId5 = d.OpponentHeroes[4].Id,
                    OpponentHeroPower1 = d.OpponentHeroes[0].Power.Value,
                    OpponentHeroPower2 = d.OpponentHeroes[1].Power.Value,
                    OpponentHeroPower3 = d.OpponentHeroes[2].Power.Value,
                    OpponentHeroPower4 = d.OpponentHeroes[3].Power.Value,
                    OpponentHeroPower5 = d.OpponentHeroes[4].Power.Value,
                    IsVictory = d.Result == BlitzFight.FightResult.Victory ? 1 : 0
                });
                MLModelBlitz.Retrain(modelData);

                CombatState = "Lobby";
                _colorCheck = null; // To force the refresh of the prediction.
            });
        }
    }
}
