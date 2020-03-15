using System;

namespace Bitwise.Game
{
    public abstract class GameDataProperty
    {
        public delegate void PropertyChanged(GameDataProperty property);

        public PropertyChanged OnPropertyChanged;

        public int Index { get; protected set; }

        public abstract Type PropertyType { get; }

        public T GetValue<T>() where T : IComparable<T>
        {
            return ((GameDataProperty<T>) this).Value;
        }
    }

    public class GameDataProperty<T> : GameDataProperty where T : IComparable<T>
    {
        public override Type PropertyType
        {
            get => typeof(T);
        }

        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if ((_value == null && value == null) || (_value != null && _value.CompareTo(value) == 0))
                {
                    return;
                }

                _value = value;
                OnPropertyChanged?.Invoke(this);
            }
        }

        public GameDataProperty(T defaultValue)
        {
            Index = GameData.InvalidPropertyIndex;
            Value = defaultValue;
            OnPropertyChanged = null;
        }

        public GameDataProperty(int index, T defaultValue)
        {
            Index = index;
            Value = defaultValue;
            OnPropertyChanged = null;
        }
    }
}