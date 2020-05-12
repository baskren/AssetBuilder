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
            //Recursion.IsMonitoringChanged += (sender, e) => _recursionCount.Clear();
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

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null, [CallerFilePath] string callerPath = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            if (propertyName == null)
                throw new InvalidDataContractException("null propertyName in SetField");

            RecursionEnter(callerPath);
            //System.Diagnostics.Debug.WriteLineIf(Logging, "\t " + this + " SetField [" + propertyName + "]");
            field = value;
            HasChanged = true;
            OnPropertyChanged(propertyName);
            RecursionExit(callerPath);
            return true;
        }

        #endregion


        #region Recursion detection
        const int _recursionLimit = 100;
        readonly Dictionary<string, int> _recursionCount = new Dictionary<string, int>();


        protected void RecursionEnter([CallerMemberName] string method = null, [CallerFilePath] string path = null)
        {

            if (Recursion.IsMonitoringInfiniteRecusion)
            {
                var name = method + " : " + path;
                _recursionCount[name] = _recursionCount.ContainsKey(name) ? _recursionCount[name] + 1 : 1;
                if (_recursionCount[name] > _recursionLimit)
                    throw new Exception("Recursion limit exceeded: " + GetType() + " : " + name);
            }
        }

        protected void RecursionExit([CallerMemberName] string method = null, [CallerFilePath] string path = null)
        {
            if (Recursion.IsMonitoringInfiniteRecusion)
            {
                var name = method + " : " + path;
                if (_recursionCount.ContainsKey(name))
                {
                    _recursionCount[name] = _recursionCount[name] - 1;
                    if (_recursionCount[name] < 0)
                        _recursionCount[name] = 0;
                }
            }
        }
        #endregion
    }
}
