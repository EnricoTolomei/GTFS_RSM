using System;
using System.Collections.Generic;


// Copyright 2015-2019 Google Inc., MobilityData
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.


namespace AtacFeed
{
    public class TransitRealtime
    {
        [ProtoBuf.ProtoContract()]
        public partial class FeedMessage : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"header", IsRequired = true)]
            public FeedHeader Header { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"entity")]
            public List<FeedEntity> Entities { get; } = new List<FeedEntity>();

        }

        [ProtoBuf.ProtoContract()]
        public partial class FeedHeader : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"gtfs_realtime_version", IsRequired = true)]
            public string GtfsRealtimeVersion { get; set; }

            [ProtoBuf.ProtoMember(2)]
            [System.ComponentModel.DefaultValue(Incrementality.FullDataset)]
            public Incrementality incrementality
            {
                get { return __pbn__incrementality ?? Incrementality.FullDataset; }
                set { __pbn__incrementality = value; }
            }
            public bool ShouldSerializeincrementality() => __pbn__incrementality != null;
            public void Resetincrementality() => __pbn__incrementality = null;
            private Incrementality? __pbn__incrementality;

            [ProtoBuf.ProtoMember(3, Name = @"timestamp")]
            public ulong Timestamp
            {
                get { return __pbn__Timestamp.GetValueOrDefault(); }
                set { __pbn__Timestamp = value; }
            }
            public bool ShouldSerializeTimestamp() => __pbn__Timestamp != null;
            public void ResetTimestamp() => __pbn__Timestamp = null;
            private ulong? __pbn__Timestamp;

            [ProtoBuf.ProtoContract()]
            public enum Incrementality
            {
                [ProtoBuf.ProtoEnum(Name = @"FULL_DATASET")]
                FullDataset = 0,
                [ProtoBuf.ProtoEnum(Name = @"DIFFERENTIAL")]
                Differential = 1,
            }

        }

        [ProtoBuf.ProtoContract()]
        public partial class FeedEntity : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"id", IsRequired = true)]
            public string Id { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"is_deleted")]
            [System.ComponentModel.DefaultValue(false)]
            public bool IsDeleted
            {
                get { return __pbn__IsDeleted ?? false; }
                set { __pbn__IsDeleted = value; }
            }
            public bool ShouldSerializeIsDeleted() => __pbn__IsDeleted != null;
            public void ResetIsDeleted() => __pbn__IsDeleted = null;
            private bool? __pbn__IsDeleted;

            [ProtoBuf.ProtoMember(3, Name = @"trip_update")]
            public TripUpdate TripUpdate { get; set; }

            [ProtoBuf.ProtoMember(4, Name = @"vehicle")]
            public VehiclePosition Vehicle { get; set; }

            [ProtoBuf.ProtoMember(5, Name = @"alert")]
            public Alert Alert { get; set; }

        }

        [ProtoBuf.ProtoContract()]
        public partial class TripUpdate : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"trip", IsRequired = true)]
            public TripDescriptor Trip { get; set; }

            [ProtoBuf.ProtoMember(3, Name = @"vehicle")]
            public VehicleDescriptor Vehicle { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"stop_time_update")]
            public List<StopTimeUpdate> StopTimeUpdates { get; } = new List<StopTimeUpdate>();

            [ProtoBuf.ProtoMember(4, Name = @"timestamp")]
            public ulong Timestamp
            {
                get { return __pbn__Timestamp.GetValueOrDefault(); }
                set { __pbn__Timestamp = value; }
            }
            public bool ShouldSerializeTimestamp() => __pbn__Timestamp != null;
            public void ResetTimestamp() => __pbn__Timestamp = null;
            private ulong? __pbn__Timestamp;

            [ProtoBuf.ProtoMember(5, Name = @"delay")]
            public int Delay
            {
                get { return __pbn__Delay.GetValueOrDefault(); }
                set { __pbn__Delay = value; }
            }
            public bool ShouldSerializeDelay() => __pbn__Delay != null;
            public void ResetDelay() => __pbn__Delay = null;
            private int? __pbn__Delay;

            [ProtoBuf.ProtoContract()]
            public partial class StopTimeEvent : ProtoBuf.IExtensible
            {
                private ProtoBuf.IExtension __pbn__extensionData;
                ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                    => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

                [ProtoBuf.ProtoMember(1, Name = @"delay")]
                public int Delay
                {
                    get { return __pbn__Delay.GetValueOrDefault(); }
                    set { __pbn__Delay = value; }
                }
                public bool ShouldSerializeDelay() => __pbn__Delay != null;
                public void ResetDelay() => __pbn__Delay = null;
                private int? __pbn__Delay;

                [ProtoBuf.ProtoMember(2, Name = @"time")]
                public long Time
                {
                    get { return __pbn__Time.GetValueOrDefault(); }
                    set { __pbn__Time = value; }
                }
                public bool ShouldSerializeTime() => __pbn__Time != null;
                public void ResetTime() => __pbn__Time = null;
                private long? __pbn__Time;

                [ProtoBuf.ProtoMember(3, Name = @"uncertainty")]
                public int Uncertainty
                {
                    get { return __pbn__Uncertainty.GetValueOrDefault(); }
                    set { __pbn__Uncertainty = value; }
                }
                public bool ShouldSerializeUncertainty() => __pbn__Uncertainty != null;
                public void ResetUncertainty() => __pbn__Uncertainty = null;
                private int? __pbn__Uncertainty;

            }

            [ProtoBuf.ProtoContract()]
            public partial class StopTimeUpdate : ProtoBuf.IExtensible
            {
                private ProtoBuf.IExtension __pbn__extensionData;
                ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                    => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

                [ProtoBuf.ProtoMember(1, Name = @"stop_sequence")]
                public uint StopSequence
                {
                    get { return __pbn__StopSequence.GetValueOrDefault(); }
                    set { __pbn__StopSequence = value; }
                }
                public bool ShouldSerializeStopSequence() => __pbn__StopSequence != null;
                public void ResetStopSequence() => __pbn__StopSequence = null;
                private uint? __pbn__StopSequence;

                [ProtoBuf.ProtoMember(4, Name = @"stop_id")]
                [System.ComponentModel.DefaultValue("")]
                public string StopId
                {
                    get { return __pbn__StopId ?? ""; }
                    set { __pbn__StopId = value; }
                }
                public bool ShouldSerializeStopId() => __pbn__StopId != null;
                public void ResetStopId() => __pbn__StopId = null;
                private string __pbn__StopId;

                [ProtoBuf.ProtoMember(2, Name = @"arrival")]
                public StopTimeEvent Arrival { get; set; }

                [ProtoBuf.ProtoMember(3, Name = @"departure")]
                public StopTimeEvent Departure { get; set; }

                [ProtoBuf.ProtoMember(5)]
                [System.ComponentModel.DefaultValue(ScheduleRelationship.Scheduled)]
                public ScheduleRelationship schedule_relationship
                {
                    get { return __pbn__schedule_relationship ?? ScheduleRelationship.Scheduled; }
                    set { __pbn__schedule_relationship = value; }
                }
                public bool ShouldSerializeschedule_relationship() => __pbn__schedule_relationship != null;
                public void Resetschedule_relationship() => __pbn__schedule_relationship = null;
                private ScheduleRelationship? __pbn__schedule_relationship;

                [ProtoBuf.ProtoContract()]
                public enum ScheduleRelationship
                {
                    [ProtoBuf.ProtoEnum(Name = @"SCHEDULED")]
                    Scheduled = 0,
                    [ProtoBuf.ProtoEnum(Name = @"SKIPPED")]
                    Skipped = 1,
                    [ProtoBuf.ProtoEnum(Name = @"NO_DATA")]
                    NoData = 2,
                }
            }
        }

        [ProtoBuf.ProtoContract()]
        public partial class VehiclePosition : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"trip")]
            public TripDescriptor Trip { get; set; }

            [ProtoBuf.ProtoMember(8, Name = @"vehicle")]
            public VehicleDescriptor Vehicle { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"position")]
            public Position Position { get; set; }

            [ProtoBuf.ProtoMember(3, Name = @"current_stop_sequence")]
            public uint CurrentStopSequence
            {
                get { return __pbn__CurrentStopSequence.GetValueOrDefault(); }
                set { __pbn__CurrentStopSequence = value; }
            }
            public bool ShouldSerializeCurrentStopSequence() => __pbn__CurrentStopSequence != null;
            public void ResetCurrentStopSequence() => __pbn__CurrentStopSequence = null;
            private uint? __pbn__CurrentStopSequence;

            [ProtoBuf.ProtoMember(7, Name = @"stop_id")]
            [System.ComponentModel.DefaultValue("")]
            public string StopId
            {
                get { return __pbn__StopId ?? ""; }
                set { __pbn__StopId = value; }
            }
            public bool ShouldSerializeStopId() => __pbn__StopId != null;
            public void ResetStopId() => __pbn__StopId = null;
            private string __pbn__StopId;

            [ProtoBuf.ProtoMember(4, Name = @"current_status")]
            [System.ComponentModel.DefaultValue(VehicleStopStatus.InTransitTo)]
            public VehicleStopStatus CurrentStatus
            {
                get { return __pbn__CurrentStatus ?? VehicleStopStatus.InTransitTo; }
                set { __pbn__CurrentStatus = value; }
            }
            public bool ShouldSerializeCurrentStatus() => __pbn__CurrentStatus != null;
            public void ResetCurrentStatus() => __pbn__CurrentStatus = null;
            private VehicleStopStatus? __pbn__CurrentStatus;

            [ProtoBuf.ProtoMember(5, Name = @"timestamp")]
            public ulong Timestamp
            {
                get { return __pbn__Timestamp.GetValueOrDefault(); }
                set { __pbn__Timestamp = value; }
            }
            public bool ShouldSerializeTimestamp() => __pbn__Timestamp != null;
            public void ResetTimestamp() => __pbn__Timestamp = null;
            private ulong? __pbn__Timestamp;

            [ProtoBuf.ProtoMember(6)]
            [System.ComponentModel.DefaultValue(CongestionLevel.UnknownCongestionLevel)]
            public CongestionLevel congestion_level
            {
                get { return __pbn__congestion_level ?? CongestionLevel.UnknownCongestionLevel; }
                set { __pbn__congestion_level = value; }
            }
            public bool ShouldSerializecongestion_level() => __pbn__congestion_level != null;
            public void Resetcongestion_level() => __pbn__congestion_level = null;
            private CongestionLevel? __pbn__congestion_level;

            [ProtoBuf.ProtoMember(9)]
            [System.ComponentModel.DefaultValue(OccupancyStatus.Empty)]
            public OccupancyStatus occupancy_status
            {
                get { return __pbn__occupancy_status ?? OccupancyStatus.Empty; }
                set { __pbn__occupancy_status = value; }
            }
            public bool ShouldSerializeoccupancy_status() => __pbn__occupancy_status != null;
            public void Resetoccupancy_status() => __pbn__occupancy_status = null;
            private OccupancyStatus? __pbn__occupancy_status;

            [ProtoBuf.ProtoContract()]
            public enum VehicleStopStatus
            {
                [ProtoBuf.ProtoEnum(Name = @"INCOMING_AT")]
                IncomingAt = 0,
                [ProtoBuf.ProtoEnum(Name = @"STOPPED_AT")]
                StoppedAt = 1,
                [ProtoBuf.ProtoEnum(Name = @"IN_TRANSIT_TO")]
                InTransitTo = 2,
            }

            [ProtoBuf.ProtoContract()]
            public enum CongestionLevel
            {
                [ProtoBuf.ProtoEnum(Name = @"UNKNOWN_CONGESTION_LEVEL")]
                UnknownCongestionLevel = 0,
                [ProtoBuf.ProtoEnum(Name = @"RUNNING_SMOOTHLY")]
                RunningSmoothly = 1,
                [ProtoBuf.ProtoEnum(Name = @"STOP_AND_GO")]
                StopAndGo = 2,
                [ProtoBuf.ProtoEnum(Name = @"CONGESTION")]
                Congestion = 3,
                [ProtoBuf.ProtoEnum(Name = @"SEVERE_CONGESTION")]
                SevereCongestion = 4,
            }

            [ProtoBuf.ProtoContract()]
            public enum OccupancyStatus
            {
                [ProtoBuf.ProtoEnum(Name = @"EMPTY")]
                Empty = 0,
                [ProtoBuf.ProtoEnum(Name = @"MANY_SEATS_AVAILABLE")]
                ManySeatsAvailable = 1,
                [ProtoBuf.ProtoEnum(Name = @"FEW_SEATS_AVAILABLE")]
                FewSeatsAvailable = 2,
                [ProtoBuf.ProtoEnum(Name = @"STANDING_ROOM_ONLY")]
                StandingRoomOnly = 3,
                [ProtoBuf.ProtoEnum(Name = @"CRUSHED_STANDING_ROOM_ONLY")]
                CrushedStandingRoomOnly = 4,
                [ProtoBuf.ProtoEnum(Name = @"FULL")]
                Full = 5,
                [ProtoBuf.ProtoEnum(Name = @"NOT_ACCEPTING_PASSENGERS")]
                NotAcceptingPassengers = 6,
                [ProtoBuf.ProtoEnum(Name = @"NO_DATA_AVAILABLE")]
                NoDataAvailable = 7,
                [ProtoBuf.ProtoEnum(Name = @"NOT_BOARDABLE")]
                NotBoardable = 8,
            }

        }

        [ProtoBuf.ProtoContract()]
        public partial class Alert : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"active_period")]
            public List<TimeRange> ActivePeriods { get; } = new List<TimeRange>();

            [ProtoBuf.ProtoMember(5, Name = @"informed_entity")]
            public List<EntitySelector> InformedEntities { get; } = new List<EntitySelector>();

            [ProtoBuf.ProtoMember(6)]
            [System.ComponentModel.DefaultValue(Cause.UnknownCause)]
            public Cause cause
            {
                get { return __pbn__cause ?? Cause.UnknownCause; }
                set { __pbn__cause = value; }
            }
            public bool ShouldSerializecause() => __pbn__cause != null;
            public void Resetcause() => __pbn__cause = null;
            private Cause? __pbn__cause;

            [ProtoBuf.ProtoMember(7)]
            [System.ComponentModel.DefaultValue(Effect.UnknownEffect)]
            public Effect effect
            {
                get { return __pbn__effect ?? Effect.UnknownEffect; }
                set { __pbn__effect = value; }
            }
            public bool ShouldSerializeeffect() => __pbn__effect != null;
            public void Reseteffect() => __pbn__effect = null;
            private Effect? __pbn__effect;

            [ProtoBuf.ProtoMember(8, Name = @"url")]
            public TranslatedString Url { get; set; }

            [ProtoBuf.ProtoMember(10, Name = @"header_text")]
            public TranslatedString HeaderText { get; set; }

            [ProtoBuf.ProtoMember(11, Name = @"description_text")]
            public TranslatedString DescriptionText { get; set; }

            [ProtoBuf.ProtoMember(12, Name = @"tts_header_text")]
            public TranslatedString TtsHeaderText { get; set; }

            [ProtoBuf.ProtoMember(13, Name = @"tts_description_text")]
            public TranslatedString TtsDescriptionText { get; set; }

            [ProtoBuf.ProtoMember(14)]
            [System.ComponentModel.DefaultValue(SeverityLevel.UnknownSeverity)]
            public SeverityLevel severity_level
            {
                get { return __pbn__severity_level ?? SeverityLevel.UnknownSeverity; }
                set { __pbn__severity_level = value; }
            }
            public bool ShouldSerializeseverity_level() => __pbn__severity_level != null;
            public void Resetseverity_level() => __pbn__severity_level = null;
            private SeverityLevel? __pbn__severity_level;

            [ProtoBuf.ProtoContract()]
            public enum Cause
            {
                [ProtoBuf.ProtoEnum(Name = @"UNKNOWN_CAUSE")]
                UnknownCause = 1,
                [ProtoBuf.ProtoEnum(Name = @"OTHER_CAUSE")]
                OtherCause = 2,
                [ProtoBuf.ProtoEnum(Name = @"TECHNICAL_PROBLEM")]
                TechnicalProblem = 3,
                [ProtoBuf.ProtoEnum(Name = @"STRIKE")]
                Strike = 4,
                [ProtoBuf.ProtoEnum(Name = @"DEMONSTRATION")]
                Demonstration = 5,
                [ProtoBuf.ProtoEnum(Name = @"ACCIDENT")]
                Accident = 6,
                [ProtoBuf.ProtoEnum(Name = @"HOLIDAY")]
                Holiday = 7,
                [ProtoBuf.ProtoEnum(Name = @"WEATHER")]
                Weather = 8,
                [ProtoBuf.ProtoEnum(Name = @"MAINTENANCE")]
                Maintenance = 9,
                [ProtoBuf.ProtoEnum(Name = @"CONSTRUCTION")]
                Construction = 10,
                [ProtoBuf.ProtoEnum(Name = @"POLICE_ACTIVITY")]
                PoliceActivity = 11,
                [ProtoBuf.ProtoEnum(Name = @"MEDICAL_EMERGENCY")]
                MedicalEmergency = 12,
            }

            [ProtoBuf.ProtoContract()]
            public enum Effect
            {
                [ProtoBuf.ProtoEnum(Name = @"NO_SERVICE")]
                NoService = 1,
                [ProtoBuf.ProtoEnum(Name = @"REDUCED_SERVICE")]
                ReducedService = 2,
                [ProtoBuf.ProtoEnum(Name = @"SIGNIFICANT_DELAYS")]
                SignificantDelays = 3,
                [ProtoBuf.ProtoEnum(Name = @"DETOUR")]
                Detour = 4,
                [ProtoBuf.ProtoEnum(Name = @"ADDITIONAL_SERVICE")]
                AdditionalService = 5,
                [ProtoBuf.ProtoEnum(Name = @"MODIFIED_SERVICE")]
                ModifiedService = 6,
                [ProtoBuf.ProtoEnum(Name = @"OTHER_EFFECT")]
                OtherEffect = 7,
                [ProtoBuf.ProtoEnum(Name = @"UNKNOWN_EFFECT")]
                UnknownEffect = 8,
                [ProtoBuf.ProtoEnum(Name = @"STOP_MOVED")]
                StopMoved = 9,
                [ProtoBuf.ProtoEnum(Name = @"NO_EFFECT")]
                NoEffect = 10,
                [ProtoBuf.ProtoEnum(Name = @"ACCESSIBILITY_ISSUE")]
                AccessibilityIssue = 11,
            }

            [ProtoBuf.ProtoContract()]
            public enum SeverityLevel
            {
                [ProtoBuf.ProtoEnum(Name = @"UNKNOWN_SEVERITY")]
                UnknownSeverity = 1,
                [ProtoBuf.ProtoEnum(Name = @"INFO")]
                Info = 2,
                [ProtoBuf.ProtoEnum(Name = @"WARNING")]
                Warning = 3,
                [ProtoBuf.ProtoEnum(Name = @"SEVERE")]
                Severe = 4,
            }

        }

        [ProtoBuf.ProtoContract()]
        public partial class TimeRange : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"start")]
            public ulong Start
            {
                get { return __pbn__Start.GetValueOrDefault(); }
                set { __pbn__Start = value; }
            }
            public bool ShouldSerializeStart() => __pbn__Start != null;
            public void ResetStart() => __pbn__Start = null;
            private ulong? __pbn__Start;

            [ProtoBuf.ProtoMember(2, Name = @"end")]
            public ulong End
            {
                get { return __pbn__End.GetValueOrDefault(); }
                set { __pbn__End = value; }
            }
            public bool ShouldSerializeEnd() => __pbn__End != null;
            public void ResetEnd() => __pbn__End = null;
            private ulong? __pbn__End;

        }

        [ProtoBuf.ProtoContract()]
        public partial class Position : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"latitude", IsRequired = true)]
            public float Latitude { get; set; }

            [ProtoBuf.ProtoMember(2, Name = @"longitude", IsRequired = true)]
            public float Longitude { get; set; }

            [ProtoBuf.ProtoMember(3, Name = @"bearing")]
            public float Bearing
            {
                get { return __pbn__Bearing.GetValueOrDefault(); }
                set { __pbn__Bearing = value; }
            }
            public bool ShouldSerializeBearing() => __pbn__Bearing != null;
            public void ResetBearing() => __pbn__Bearing = null;
            private float? __pbn__Bearing;

            [ProtoBuf.ProtoMember(4, Name = @"odometer")]
            public double Odometer
            {
                get { return __pbn__Odometer.GetValueOrDefault(); }
                set { __pbn__Odometer = value; }
            }
            public bool ShouldSerializeOdometer() => __pbn__Odometer != null;
            public void ResetOdometer() => __pbn__Odometer = null;
            private double? __pbn__Odometer;

            [ProtoBuf.ProtoMember(5, Name = @"speed")]
            public float Speed
            {
                get { return __pbn__Speed.GetValueOrDefault(); }
                set { __pbn__Speed = value; }
            }
            public bool ShouldSerializeSpeed() => __pbn__Speed != null;
            public void ResetSpeed() => __pbn__Speed = null;
            private float? __pbn__Speed;

        }

        [ProtoBuf.ProtoContract()]
        public partial class TripDescriptor : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"trip_id")]
            [System.ComponentModel.DefaultValue("")]
            public string TripId
            {
                get { return __pbn__TripId ?? ""; }
                set { __pbn__TripId = value; }
            }
            public bool ShouldSerializeTripId() => __pbn__TripId != null;
            public void ResetTripId() => __pbn__TripId = null;
            private string __pbn__TripId;

            [ProtoBuf.ProtoMember(5, Name = @"route_id")]
            [System.ComponentModel.DefaultValue("")]
            public string RouteId
            {
                get { return __pbn__RouteId ?? ""; }
                set { __pbn__RouteId = value; }
            }
            public bool ShouldSerializeRouteId() => __pbn__RouteId != null;
            public void ResetRouteId() => __pbn__RouteId = null;
            private string __pbn__RouteId;

            [ProtoBuf.ProtoMember(6, Name = @"direction_id")]
            public uint DirectionId
            {
                get { return __pbn__DirectionId.GetValueOrDefault(); }
                set { __pbn__DirectionId = value; }
            }
            public bool ShouldSerializeDirectionId() => __pbn__DirectionId != null;
            public void ResetDirectionId() => __pbn__DirectionId = null;
            private uint? __pbn__DirectionId;

            [ProtoBuf.ProtoMember(2, Name = @"start_time")]
            [System.ComponentModel.DefaultValue("")]
            public string StartTime
            {
                get { return __pbn__StartTime ?? ""; }
                set { __pbn__StartTime = value; }
            }
            public bool ShouldSerializeStartTime() => __pbn__StartTime != null;
            public void ResetStartTime() => __pbn__StartTime = null;
            private string __pbn__StartTime;

            [ProtoBuf.ProtoMember(3, Name = @"start_date")]
            [System.ComponentModel.DefaultValue("")]
            public string StartDate
            {
                get { return __pbn__StartDate ?? ""; }
                set { __pbn__StartDate = value; }
            }
            public bool ShouldSerializeStartDate() => __pbn__StartDate != null;
            public void ResetStartDate() => __pbn__StartDate = null;
            private string __pbn__StartDate;

            [ProtoBuf.ProtoMember(4)]
            [System.ComponentModel.DefaultValue(ScheduleRelationship.Scheduled)]
            public ScheduleRelationship schedule_relationship
            {
                get { return __pbn__schedule_relationship ?? ScheduleRelationship.Scheduled; }
                set { __pbn__schedule_relationship = value; }
            }
            public bool ShouldSerializeschedule_relationship() => __pbn__schedule_relationship != null;
            public void Resetschedule_relationship() => __pbn__schedule_relationship = null;
            private ScheduleRelationship? __pbn__schedule_relationship;

            [ProtoBuf.ProtoContract()]
            public enum ScheduleRelationship
            {
                [ProtoBuf.ProtoEnum(Name = @"SCHEDULED")]
                Scheduled = 0,
                [ProtoBuf.ProtoEnum(Name = @"ADDED")]
                Added = 1,
                [ProtoBuf.ProtoEnum(Name = @"UNSCHEDULED")]
                Unscheduled = 2,
                [ProtoBuf.ProtoEnum(Name = @"CANCELED")]
                Canceled = 3,
                [ProtoBuf.ProtoEnum(Name = @"REPLACEMENT")]
                [Obsolete]
                Replacement = 5,
            }

        }

        [ProtoBuf.ProtoContract()]
        public partial class VehicleDescriptor : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"id")]
            [System.ComponentModel.DefaultValue("")]
            public string Id
            {
                get { return __pbn__Id ?? ""; }
                set { __pbn__Id = value; }
            }
            public bool ShouldSerializeId() => __pbn__Id != null;
            public void ResetId() => __pbn__Id = null;
            private string __pbn__Id;

            [ProtoBuf.ProtoMember(2, Name = @"label")]
            [System.ComponentModel.DefaultValue("")]
            public string Label
            {
                get { return __pbn__Label ?? ""; }
                set { __pbn__Label = value; }
            }
            public bool ShouldSerializeLabel() => __pbn__Label != null;
            public void ResetLabel() => __pbn__Label = null;
            private string __pbn__Label;

            [ProtoBuf.ProtoMember(3, Name = @"license_plate")]
            [System.ComponentModel.DefaultValue("")]
            public string LicensePlate
            {
                get { return __pbn__LicensePlate ?? ""; }
                set { __pbn__LicensePlate = value; }
            }
            public bool ShouldSerializeLicensePlate() => __pbn__LicensePlate != null;
            public void ResetLicensePlate() => __pbn__LicensePlate = null;
            private string __pbn__LicensePlate;

        }

        [ProtoBuf.ProtoContract()]
        public partial class EntitySelector : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"agency_id")]
            [System.ComponentModel.DefaultValue("")]
            public string AgencyId
            {
                get { return __pbn__AgencyId ?? ""; }
                set { __pbn__AgencyId = value; }
            }
            public bool ShouldSerializeAgencyId() => __pbn__AgencyId != null;
            public void ResetAgencyId() => __pbn__AgencyId = null;
            private string __pbn__AgencyId;

            [ProtoBuf.ProtoMember(2, Name = @"route_id")]
            [System.ComponentModel.DefaultValue("")]
            public string RouteId
            {
                get { return __pbn__RouteId ?? ""; }
                set { __pbn__RouteId = value; }
            }
            public bool ShouldSerializeRouteId() => __pbn__RouteId != null;
            public void ResetRouteId() => __pbn__RouteId = null;
            private string __pbn__RouteId;

            [ProtoBuf.ProtoMember(3, Name = @"route_type")]
            public int RouteType
            {
                get { return __pbn__RouteType.GetValueOrDefault(); }
                set { __pbn__RouteType = value; }
            }
            public bool ShouldSerializeRouteType() => __pbn__RouteType != null;
            public void ResetRouteType() => __pbn__RouteType = null;
            private int? __pbn__RouteType;

            [ProtoBuf.ProtoMember(4, Name = @"trip")]
            public TripDescriptor Trip { get; set; }

            [ProtoBuf.ProtoMember(5, Name = @"stop_id")]
            [System.ComponentModel.DefaultValue("")]
            public string StopId
            {
                get { return __pbn__StopId ?? ""; }
                set { __pbn__StopId = value; }
            }
            public bool ShouldSerializeStopId() => __pbn__StopId != null;
            public void ResetStopId() => __pbn__StopId = null;
            private string __pbn__StopId;

        }

        [ProtoBuf.ProtoContract()]
        public partial class TranslatedString : ProtoBuf.IExtensible
        {
            private ProtoBuf.IExtension __pbn__extensionData;
            ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

            [ProtoBuf.ProtoMember(1, Name = @"translation")]
            public List<Translation> Translations { get; } = new List<Translation>();

            [ProtoBuf.ProtoContract()]
            public partial class Translation : ProtoBuf.IExtensible
            {
                private ProtoBuf.IExtension __pbn__extensionData;
                ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
                    => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

                [ProtoBuf.ProtoMember(1, Name = @"text", IsRequired = true)]
                public string Text { get; set; }

                [ProtoBuf.ProtoMember(2, Name = @"language")]
                [System.ComponentModel.DefaultValue("")]
                public string Language
                {
                    get { return __pbn__Language ?? ""; }
                    set { __pbn__Language = value; }
                }
                public bool ShouldSerializeLanguage() => __pbn__Language != null;
                public void ResetLanguage() => __pbn__Language = null;
                private string __pbn__Language;

            }

        }


    }
}
