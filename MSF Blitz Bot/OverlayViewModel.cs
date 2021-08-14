using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using MSFBlitzBot.GamePages;

namespace MSFBlitzBot
{
    public class OverlayViewModel : ObservableObject
    {
        private bool _cancelTask;
        private GamePageViewModel _toolPage;
        private GamePageViewModel overlayPage;
        private GamePageViewModel contextPage;
        private double windowLeft;
        private double windowTop;
        private double windowWidth;
        private double windowHeight;

        public OverlayViewModel()
        {
            StartLoopWinLocation();
        }

        public GamePageViewModel ContextPage { get => contextPage; set { contextPage = value; RaisePropertyChanged(nameof(ContextPage)); } }
        public GamePageViewModel OverlayPage { get => overlayPage; set { overlayPage = value; RaisePropertyChanged(nameof(OverlayPage)); } }

        public double WindowLeft { get => windowLeft; set { windowLeft = value; RaisePropertyChanged(nameof(WindowLeft)); } }
        public double WindowTop { get => windowTop; set { windowTop = value; RaisePropertyChanged(nameof(WindowTop)); } }
        public double WindowWidth { get => windowWidth; set { windowWidth = value; RaisePropertyChanged(nameof(WindowWidth)); } }
        public double WindowHeight { get => windowHeight; set { windowHeight = value; RaisePropertyChanged(nameof(WindowHeight)); } }

        public static async void StartLoopTask()
        {
            await Task.Run(delegate
            {
                ViewModelLocator.OverlayViewModel.Loop();
            });
        }

        public static async void StartLoopWinLocation()
        {
            await Task.Run(delegate
            {
                ViewModelLocator.OverlayViewModel.LoopWinLocation();
            });
        }

        public bool IsWorkerRunning => !_cancelTask;

        public void StopWorker()
        {
            _cancelTask = true;
        }

        private void Loop()
        {
            while (!_cancelTask)
            {
                if (Engine.Singleton.Update())
                {
                    _toolPage?.OnClose();
                    ContextPage?.OnClose();
                    OverlayPage?.OnClose();
                    ContextPage = GetContextPageViewModel(Engine.Singleton.CurrentPageId);
                    OverlayPage = GetOverlayPageViewModel(Engine.Singleton.CurrentPageId);
                    _toolPage = GetToolPageViewModel(Engine.Singleton.CurrentPageId);
                    OverlayPage?.OnOpen();
                    ContextPage?.OnOpen();
                    _toolPage?.OnOpen();
                }
                if (Emulator.GameImage != null)
                {
                    if (_toolPage == null)
                    {
                        ContextPage?.Loop();
                        if (OverlayPage != ContextPage)
                        {
                            OverlayPage?.Loop();
                        }
                    }
                    else
                    {
                        _toolPage?.Loop();
                    }
                }
                Thread.Sleep(UserDataManager.EngineUpdateDelay);
            }
        }

        private void LoopWinLocation()
        {
            while (!_cancelTask)
            {
                Rectangle gameScreenArea = Emulator.GetGameScreenArea();
                Thread.Sleep(UserDataManager.OverlayUpdateDelay);
                WindowTop = gameScreenArea.Top;
                WindowLeft = gameScreenArea.Left;
                WindowHeight = gameScreenArea.Height;
                WindowWidth = gameScreenArea.Width;
            }
        }

        private static GamePageViewModel GetOverlayPageViewModel(GamePageId pageId)
        {
            return pageId switch
            {
                GamePageId.Blitz => ViewModelLocator.BlitzViewModel,
                _ => null,
            };
        }

        private static GamePageViewModel GetContextPageViewModel(GamePageId pageId)
        {
            return pageId switch
            {
                GamePageId.Blitz => null,
                GamePageId.Unknown => null,
                _ => throw new NotImplementedException(),
            };
        }

        private static GamePageViewModel GetToolPageViewModel(GamePageId pageId)
        {
            return pageId switch
            {
                GamePageId.Blitz => null,
                GamePageId.Unknown => null,
                _ => throw new NotImplementedException(),
            };
        }
    }
}