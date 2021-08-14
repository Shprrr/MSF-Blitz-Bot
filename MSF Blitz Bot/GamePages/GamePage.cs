using System;

namespace MSFBlitzBot.GamePages
{
    public enum GamePageId
    {
        Unknown = -1,
        Blitz,
        Count
    }

    public abstract class GamePage
    {
        private bool _isOpened;

        public GamePageId Id { get; }

        public GamePage(GamePageId id)
        {
            Id = id;
        }

        public static GamePage Create(GamePageId id)
        {
            return id switch
            {
                GamePageId.Blitz => new BlitzPage(),
                GamePageId.Unknown => null,
                _ => throw new NotImplementedException(),
            };
        }

        public void OnOpen()
        {
            if (!_isOpened)
            {
                _isOpened = true;
                OnOpenInternal();
            }
        }

        public void OnClose()
        {
            if (_isOpened)
            {
                _isOpened = false;
                OnCloseInternal();
            }
        }

        protected abstract void OnOpenInternal();

        protected abstract void OnCloseInternal();

        public abstract bool IsCurrentPage();
    }
}
