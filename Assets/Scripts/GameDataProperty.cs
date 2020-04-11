using System;
using System.Collections.Generic;
using UnityEngine.UIElements.Experimental;

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

        public List<T> GetListValue<T>() where T : IComparable<T>
        {
            return ((GameDataListProperty<T>) this).Value;
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

    public class GameDataListProperty<T> : GameDataProperty where T : IComparable<T>
    {
        public override Type PropertyType
        {
            get => typeof(T);
        }

        private List<T> _value;

        public List<T> Value
        {
            get => _value;
            set
            {
                if (_value == value)
                {
                    return;
                }

                _value = value;
                OnPropertyChanged?.Invoke(this);
            }
        }

        public GameDataListProperty()
        {
            Index = GameData.InvalidPropertyIndex;
            Value = new List<T>();
            OnPropertyChanged = null;
        }

        public GameDataListProperty(int index)
        {
            Index = index;
            Value = new List<T>();
            OnPropertyChanged = null;
        }

        public void AddElement(T element)
        {
            Value.Add(element);
            OnPropertyChanged?.Invoke(this);
        }

        public void RemoveElement(int index)
        {
            Value.RemoveAt(index);
            OnPropertyChanged?.Invoke(this);
        }

        public void ModifyElement(int index, T newValue)
        {
            T oldValue = Value[index];
            if ((oldValue == null && newValue == null) || (oldValue != null && oldValue.CompareTo(newValue) == 0))
            {
                return;
            }

            Value[index] = newValue;
            OnPropertyChanged?.Invoke(this);
        }

        public void ModifyLast(T newValue)
        {
            if (Value.Count == 0)
            {
                throw new InvalidOperationException();
            }
            Value[Value.Count - 1] = newValue;
            OnPropertyChanged?.Invoke(this);
        }
    }
}