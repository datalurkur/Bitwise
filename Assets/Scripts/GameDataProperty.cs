using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements.Experimental;

namespace Bitwise.Game
{
    [Serializable]
    public abstract class GameDataProperty
    {
        public delegate void PropertyChanged(GameDataProperty property);

        protected PropertyChanged onPropertyChanged;

        public int Index { get; protected set; }

        public string Name;

        public abstract Type PropertyType { get; }

        public void Subscribe(PropertyChanged callback)
        {
            onPropertyChanged += callback;
            callback?.Invoke(this);
        }
        public void Unsubscribe(PropertyChanged callback) { onPropertyChanged -= callback; }

        public T GetValue<T>() where T : IComparable<T>
        {
            return ((GameDataProperty<T>) this).Value;
        }

        public List<T> GetListValue<T>() where T : IComparable<T>
        {
            return ((GameDataListProperty<T>) this).Value;
        }
    }

    [Serializable]
    public class GameDataProperty<T> : GameDataProperty where T : IComparable<T>
    {
        public static implicit operator T(GameDataProperty<T> prop) => prop.Value;

        public override Type PropertyType
        {
            get => typeof(T);
        }

        [SerializeField]
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
                onPropertyChanged?.Invoke(this);
            }
        }

        public GameDataProperty(T defaultValue)
        {
            Index = GameData.InvalidPropertyIndex;
            Name = "UNNAMED";
            Value = defaultValue;
            onPropertyChanged = null;
        }

        public GameDataProperty(int index, string name, T defaultValue)
        {
            Index = index;
            Name = name;
            Value = defaultValue;
            onPropertyChanged = null;
        }
    }

    [Serializable]
    public class GameDataListProperty<T> : GameDataProperty where T : IComparable<T>
    {
        public static implicit operator List<T>(GameDataListProperty<T> prop) => prop.Value;

        public override Type PropertyType
        {
            get => typeof(T);
        }

        [SerializeField]
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
                onPropertyChanged?.Invoke(this);
            }
        }

        public GameDataListProperty()
        {
            Index = GameData.InvalidPropertyIndex;
            Name = "UNNAMED";
            Value = new List<T>();
            onPropertyChanged = null;
        }

        public GameDataListProperty(int index, string name)
        {
            Index = index;
            Name = name;
            Value = new List<T>();
            onPropertyChanged = null;
        }

        public int Count => Value.Count;

        public T GetElementAt(int index)
        {
            return Value[index];
        }

        public void AddElement(T element)
        {
            Value.Add(element);
            onPropertyChanged?.Invoke(this);
        }

        public bool RemoveElement(T element)
        {
            bool ret = Value.Remove(element);
            if (ret) { onPropertyChanged?.Invoke(this); }
            return ret;
        }

        public void RemoveElementAt(int index)
        {
            Value.RemoveAt(index);
            onPropertyChanged?.Invoke(this);
        }

        public void ModifyElementAt(int index, T newValue)
        {
            T oldValue = Value[index];
            if ((oldValue == null && newValue == null) || (oldValue != null && oldValue.CompareTo(newValue) == 0))
            {
                return;
            }

            Value[index] = newValue;
            onPropertyChanged?.Invoke(this);
        }
    }
}