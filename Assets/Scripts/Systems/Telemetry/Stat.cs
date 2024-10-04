using System;

namespace RL.Telemetry
{
    /// <summary>
    /// Represents an individual statistic.
    /// </summary>
    [Serializable]
    public class Stat
    {
        public StatKey key;
        int value;

        public event EventHandler OnValueChanged;

        public int Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public Stat(StatKey key, int value)
        {
            this.key = key;
            this.value = value;
            this.OnValueChanged = null;
        }

        public void Increment()
        {
            value++;
            OnValueChanged?.Invoke(this, new());
        }

        public void Decrement()
        {
            value--;
            OnValueChanged?.Invoke(this, new());
        }

        public StatSaveData SaveToJson()
        {
            return new()
            {
                Key = (int) this.key,
                Value = this.Value
            };
        }
    }
}
