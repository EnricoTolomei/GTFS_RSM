using System;
using System.Collections.Generic;

namespace AtacFeed
{
    public class ExtendedVehicleComparer : IEqualityComparer<ExtendedVehicleInfo>
    {
        private readonly bool _strict;
        private readonly bool _superStrict;

        public ExtendedVehicleComparer(bool strict, bool superStrict)
        {
            _strict = strict;
            _superStrict = superStrict;
        }

        public bool Equals(ExtendedVehicleInfo x, ExtendedVehicleInfo y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;

            if (_superStrict)
                return x.Matricola == y.Matricola && x.TripId == y.TripId && x.CurrentStopSequence == y.CurrentStopSequence;
            if (_strict)
                return x.Matricola == y.Matricola && x.TripId == y.TripId;
            return x.Matricola == y.Matricola;
        }

        public int GetHashCode(ExtendedVehicleInfo obj)
        {
            if (obj is null) return 0;

            if (_superStrict)
                return Hash(obj.Matricola, obj.TripId, obj.CurrentStopSequence);
            if (_strict)
                return Hash(obj.Matricola, obj.TripId);
            return obj.Matricola?.GetHashCode() ?? 0;
        }

        private static int Hash(params object[] parts)
        {
            unchecked
            {
                int hash = 17;
                foreach (var p in parts)
                {
                    hash = hash * 31 + (p?.GetHashCode() ?? 0);
                }
                return hash;
            }
        }
    }
}