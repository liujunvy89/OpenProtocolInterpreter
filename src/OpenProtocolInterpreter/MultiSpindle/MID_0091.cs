﻿using OpenProtocolInterpreter.Converters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenProtocolInterpreter.MultiSpindle
{
    /// <summary>
    /// MID: Multi-spindle status
    /// Description: 
    ///      The multi-spindle status is sent after each sync tightening. The multiple status contains the common
    ///      status of the multiple as well as the individual status of each spindle.
    /// Message sent by: Controller
    /// Answer : MID 0092 Multi-spindle status acknowledge
    /// </summary>
    public class MID_0091 : Mid, IMultiSpindle
    {
        private readonly IValueConverter<int> _intConverter;
        private readonly IValueConverter<bool> _boolConverter;
        private readonly IValueConverter<DateTime> _dateConverter;
        private readonly IValueConverter<IEnumerable<SpindleStatus>> _spindlesStatusConverter;
        private const int LAST_REVISION = 1;
        public const int MID = 91;

        public int NumberOfSpindles
        {
            get => RevisionsByFields[1][(int)DataFields.NUMBER_OF_SPINDLES].GetValue(_intConverter.Convert);
            set => RevisionsByFields[1][(int)DataFields.NUMBER_OF_SPINDLES].SetValue(_intConverter.Convert, value);
        }
        public int SyncTighteningId
        {
            get => RevisionsByFields[1][(int)DataFields.SYNC_TIGHTENING_ID].GetValue(_intConverter.Convert);
            set => RevisionsByFields[1][(int)DataFields.SYNC_TIGHTENING_ID].SetValue(_intConverter.Convert, value);
        }
        public DateTime Time
        {
            get => RevisionsByFields[1][(int)DataFields.TIME].GetValue(_dateConverter.Convert);
            set => RevisionsByFields[1][(int)DataFields.TIME].SetValue(_dateConverter.Convert, value);
        }
        public bool SyncOverallStatus
        {
            get => RevisionsByFields[1][(int)DataFields.SYNC_OVERALL_STATUS].GetValue(_boolConverter.Convert);
            set => RevisionsByFields[1][(int)DataFields.SYNC_OVERALL_STATUS].SetValue(_boolConverter.Convert, value);
        }
        public List<SpindleStatus> SpindlesStatus { get; set; }
        

        public MID_0091(int? noAckFlag = 0) : base(MID, LAST_REVISION, noAckFlag)
        {
            _intConverter = new Int32Converter();
            _boolConverter = new BoolConverter();
            _dateConverter = new DateConverter();
            _spindlesStatusConverter = new SpindleStatusConverter();
            SpindlesStatus = new List<SpindleStatus>();
        }

        public MID_0091(int numberOfSpindles, int syncTighteningId, DateTime time, bool syncOverallStatus, IEnumerable<SpindleStatus> spindleStatus, int? noAckFlag = 0) : this(noAckFlag)
        {
            _intConverter = new Int32Converter();
            _boolConverter = new BoolConverter();
            _dateConverter = new DateConverter();
            _spindlesStatusConverter = new SpindleStatusConverter();
            NumberOfSpindles = numberOfSpindles;
            SyncTighteningId = syncTighteningId;
            Time = time;
            SpindlesStatus = spindleStatus.ToList();
        }

        internal MID_0091(IMid nextTemplate) : this() => NextTemplate = nextTemplate;

        public override string Pack()
        {
            RevisionsByFields[1][(int)DataFields.SPINDLE_STATUS].Value = _spindlesStatusConverter.Convert(SpindlesStatus);
            return base.Pack();
        }

        public override Mid Parse(string package)
        {
            if (IsCorrectType(package))
            {
                HeaderData = ProcessHeader(package);
                var spindleField = RevisionsByFields[1][(int)DataFields.SPINDLE_STATUS];
                spindleField.Size = package.Length - spindleField.Index - 2;
                base.Parse(package);
                SpindlesStatus = _spindlesStatusConverter.Convert(spindleField.Value).ToList();
                return this;
            }

            return NextTemplate.Parse(package);
        }

        protected override Dictionary<int, List<DataField>> RegisterDatafields()
        {
            return new Dictionary<int, List<DataField>>()
            {
                {
                    1, new List<DataField>()
                            {
                                new DataField((int)DataFields.NUMBER_OF_SPINDLES, 20, 2, '0', DataField.PaddingOrientations.LEFT_PADDED),
                                new DataField((int)DataFields.SYNC_TIGHTENING_ID, 24, 5, '0', DataField.PaddingOrientations.LEFT_PADDED),
                                new DataField((int)DataFields.TIME, 31, 19),
                                new DataField((int)DataFields.SYNC_OVERALL_STATUS, 52, 1),
                                new DataField((int)DataFields.SPINDLE_STATUS, 55, 5)
                            }
                }
            };
        }

        public enum DataFields
        {
            NUMBER_OF_SPINDLES,
            SYNC_TIGHTENING_ID,
            TIME,
            SYNC_OVERALL_STATUS,
            SPINDLE_STATUS
        }
    }
}
