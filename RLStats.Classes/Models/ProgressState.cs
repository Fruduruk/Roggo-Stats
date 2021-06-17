using System;

namespace RLStats_Classes.Models
{
    public class ProgressState
    {
        public event EventHandler SomethingChanged;
        private bool _initial;
        private string _currentMessage;
        private int _totalCount;
        private int _partCount;
        private int _falsePartCount;

        public bool Initial
        {
            get => _initial;
            set
            {
                _initial = value;
                OnSomethingChanged();
            }
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set
            {
                _currentMessage = value;
                OnSomethingChanged();
            }
        }

        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                OnSomethingChanged();
            }
        }

        public int PartCount
        {
            get => _partCount;
            set
            {
                _partCount = value;
                OnSomethingChanged();
            }
        }

        public int FalsePartCount
        {
            get => _falsePartCount;
            set
            {
                _falsePartCount = value;
                OnSomethingChanged();
            }
        }

        private void OnSomethingChanged()
        {
            SomethingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
