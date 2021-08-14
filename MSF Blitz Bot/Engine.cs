using System;
using MSFBlitzBot.GamePages;

namespace MSFBlitzBot
{
    internal class Engine
    {
        private int _changeCounter;

        private static int CurrentPageAntiFlickingCounter => -1;

        public static Engine Singleton { get; } = new Engine();


        public GamePageId CurrentPageId => CurrentPage?.Id ?? GamePageId.Unknown;

        public GamePage CurrentPage { get; private set; }

        public bool Update()
        {
            GamePageId newPageId = GetNewPageId();
            if (CurrentPageId != newPageId)
            {
                if (_changeCounter > CurrentPageAntiFlickingCounter)
                {
                    CurrentPage?.OnClose();
                    _changeCounter = 0;
                    CurrentPage = GamePage.Create(newPageId);
                    CurrentPage?.OnOpen();
                    return true;
                }
                _changeCounter++;
            }
            else
            {
                _changeCounter = 0;
            }
            return false;
        }

        private GamePageId GetNewPageId()
        {
            if (!Emulator.IsValid)
            {
                return GamePageId.Unknown;
            }
            Emulator.UpdateGameImage();
            if (Emulator.GameImage == null || !Emulator.GameImage.IsValid)
            {
                return GamePageId.Unknown;
            }
            if (CurrentPage != null && CurrentPage!.IsCurrentPage())
            {
                return CurrentPageId;
            }
            for (GamePageId gamePageId = 0; gamePageId < GamePageId.Count; gamePageId++)
            {
                if (CurrentPageId != gamePageId && IsNewPageId(gamePageId))
                {
                    return gamePageId;
                }
            }
            return GamePageId.Unknown;
        }

        private static bool IsNewPageId(GamePageId pageId)
        {
            return pageId switch
            {
                GamePageId.Blitz => BlitzPage.IsCurrentPageStatic(),
                GamePageId.Unknown => false,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
