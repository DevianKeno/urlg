using System;

namespace URLG.Telemetry
{
    /// <summary>
    /// Represents an individual statistic.
    /// </summary>
    [Serializable]
    public class Stat
    {
        public string Name;
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

        public Stat(string name, int value)
        {
            this.Name = name;
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
                Name = this.Name,
                Value = this.Value
            };
        }
    }
}
