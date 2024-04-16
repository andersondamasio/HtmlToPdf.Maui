using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Dynamic;
using Newtonsoft.Json;

namespace P42.Utils
{
    public class NotifiablePropertyObject : INotifyPropertyChanged
    {
        [JsonIgnore]
        public static long Instances { get; private set; }

        [JsonIgnore]
        public long InstanceId { get; private set; }

        [JsonIgnore]
        public virtual bool Logging { get; }

        protected List<string> BatchedPropertyChanges = new List<string>();

        int _batchChanges;
        [JsonIgnore]
        public bool BatchChanges
        {
            get => _batchChanges > 0;
            set
            {
                if (value)
                    _batchChanges++;
                else
                    _batchChanges--;
                _batchChanges = Math.Max(0, _batchChanges);
                if (_batchChanges == 0)
                {
                    var propertyNames = new List<string>(BatchedPropertyChanges);
                    BatchedPropertyChanges.Clear();
                    foreach (var propertyName in propertyNames)
                        OnPropertyChanged(propertyName);
                }
            }
        }

        [JsonIgnore]
        public bool HasChanged { get; set; }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
            => HasChanged = false;


        public NotifiablePropertyObject()
        {
            //Recursion.IsMonitoringChanged += (sender, e) => RecursionCount.Clear();
            InstanceId = Instances++;
        }

        #region Property Change Handler
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //System.Diagnostics.Debug.WriteLineIf(Logging, "\t " + this + " NotifiablePropertyObject.OnPropertyChanged [" + propertyName + "]");
            //System.Diagnostics.Debug.WriteLineIf(Logging, "\t BatchChanges=[" + BatchChanges + "]");

            if (BatchChanges)
                BatchedPropertyChanges.Add(propertyName);
            else
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, Action action = null, [CallerMemberName] string propertyName = null, [CallerFilePath] string callerPath = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            if (propertyName == null)
                throw new InvalidDataContractException("null propertyName in SetField");

            field = value;
            HasChanged = true;
            action?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion


    }
}
