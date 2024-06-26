// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Tobii.StreamEngine;
using Unity.Jobs;
using UnityEngine;

namespace Tobii.XR
{
    /// <summary>
    /// Uses Tobii's Stream Engine library to provide eye tracking data to TobiiXR
    /// </summary>
    [ProviderDisplayName("Tobii")]
    public partial class TobiiProvider : IEyeTrackingProvider
    {
        internal const int AdvancedDataQueueSize = 30;
        private readonly object _lockEyeTrackingDataLocal = new object();
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();
        private readonly TobiiXR_EyeTrackingData _eyeTrackingDataLocalInternal = new TobiiXR_EyeTrackingData();

        private readonly object _lockAdvancedData = new object();
        private readonly TobiiXR_AdvancedEyeTrackingData _advancedEyeTrackingData =
            new TobiiXR_AdvancedEyeTrackingData();

        private readonly Queue<TobiiXR_AdvancedEyeTrackingData> _advancedInternalQueue =
            new Queue<TobiiXR_AdvancedEyeTrackingData>();

        private Vector3 _foveatedGazeDirectionLocal = Vector3.forward;
        private StreamEngineTracker _streamEngineTracker;
        private CameraPoseHistory _cameraPoseHistory;
        private Matrix4x4 _localToWorldMatrix;

        private PositionGuideData _positionGuideData;
        private readonly object _lockPositionGuideData = new object();
        
        internal Vector3 HeadToCenterEyeTranslation;

        public Matrix4x4 LocalToWorldMatrix => _localToWorldMatrix;
        public bool ConvergenceDistanceSupported => _streamEngineTracker.ConvergenceDistanceSupported;
        public bool PositionGuideSupported => _streamEngineTracker.PositionGuideSupported;

        public void GetEyeTrackingDataLocal(TobiiXR_EyeTrackingData data)
        {
            EyeTrackingDataHelper.Copy(_eyeTrackingDataLocal, data);
        }

        public TobiiXR_AdvancedEyeTrackingData AdvancedEyeTrackingData => _advancedEyeTrackingData;

        public Vector3 FoveatedGazeDirectionLocal => _foveatedGazeDirectionLocal;

        public bool HasValidOcumenLicense => _streamEngineTracker.LicenseLevel >= tobii_feature_group_t.TOBII_FEATURE_GROUP_PROFESSIONAL;

        public Queue<TobiiXR_AdvancedEyeTrackingData> AdvancedData { get; } = new Queue<TobiiXR_AdvancedEyeTrackingData>();

        public PositionGuideData PositionGuideData => _positionGuideData;

        public StreamEngineContext InternalHandle => _streamEngineTracker.Context;
        public List<string> FriendlyValidationErrors => _streamEngineTracker.FriendlyValidationErrors;

        public TobiiXR_EyeTrackerMetadata GetMetadata()
        {
            Interop.tobii_get_device_info(_streamEngineTracker.Context.Device, out var deviceInfo);
            Interop.tobii_get_output_frequency(_streamEngineTracker.Context.Device, out var outputFrequency);
            var result = new TobiiXR_EyeTrackerMetadata
            {
                SerialNumber = deviceInfo.serial_number,
                Model = deviceInfo.model,
                RuntimeVersion = deviceInfo.runtime_build_version,
                OutputFrequency = outputFrequency > 1 ? outputFrequency.ToString(CultureInfo.InvariantCulture) : "Unknown",
            };
            return result;
        }

        public bool Initialize()
        {
            return InitializeWithLicense(null, false);
        }

        public bool InitializeWithLicense(string licenseKey, bool enableAdvanced)
        {
            var createInfo = new StreamEngineTracker_Description();
            if (!string.IsNullOrEmpty(licenseKey))
            {
                createInfo.License = new[] {licenseKey};
            }
            
            try
            {
                _cameraPoseHistory = new CameraPoseHistory();
                _streamEngineTracker = new StreamEngineTracker(createInfo);

                // Subscribe to relevant streams
                var startInfo = new StreamEngineTrackerStartInfo();
                startInfo.WearableFoveatedDataCallback = OnFoveatedData;
                if (enableAdvanced) startInfo.WearableAdvancedDataCallback = OnAdvancedWearableData;
                else startInfo.WearableDataCallback = OnWearableData;
                _streamEngineTracker.Start(startInfo);
                
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public void Tick()
        {
            HeadToCenterEyeTranslation = CoordinatesHelper.GetHeadToCenterEyeTranslation();
            _cameraPoseHistory.Tick(GetSystemTimestamp());
            _localToWorldMatrix = _cameraPoseHistory.GetLocalToWorldMatrix();

            // Copy consumer data
            lock (_lockEyeTrackingDataLocal)
            {
                EyeTrackingDataHelper.Copy(_eyeTrackingDataLocalInternal, _eyeTrackingDataLocal);
            }
            _eyeTrackingDataLocal.Timestamp = Time.unscaledTime;

            // Shuffle data from internal queue to public queue
            lock (_lockAdvancedData)
            {
                while (_advancedInternalQueue.Count > 1)
                {
                    AdvancedData.Enqueue(_advancedInternalQueue.Dequeue());
                }

                // Copy newest data to _advancedEyeTrackingData
                if (_advancedInternalQueue.Count == 1)
                {
                    var data = _advancedInternalQueue.Dequeue();
                    EyeTrackingDataHelper.Copy(data, _advancedEyeTrackingData);
                    AdvancedData.Enqueue(data);
                }
                
                // Limit size of public queue
                while (AdvancedData.Count > AdvancedDataQueueSize) AdvancedData.Dequeue();
            }
        }

        public void Destroy()
        {
            if (_streamEngineTracker != null)
            {
                _streamEngineTracker.Destroy();
                _streamEngineTracker = null;
            }
        }

        public bool TryGetLocalToWorldMatrixFor(long timestampUs, out Matrix4x4 matrix)
        {
            return _cameraPoseHistory.TryGetLocalToWorldMatrixFor(timestampUs, out matrix);
        }

        public Matrix4x4 GetLocalToWorldMatrix()
        {
            return _cameraPoseHistory.GetLocalToWorldMatrix();
        }
        
        private void OnWearableData(ref tobii_wearable_consumer_data_t data)
        {
            lock (_lockEyeTrackingDataLocal)
            {
                StreamEngineDataMapper.FromConsumerData(_eyeTrackingDataLocalInternal, ref data,
                    _streamEngineTracker.ConvergenceDistanceSupported, HeadToCenterEyeTranslation);
            }
            
            lock (_lockPositionGuideData)
            {
                StreamEngineDataMapper.FillPositionGuideData(ref _positionGuideData, ref data);
            }
        }

        private void OnAdvancedWearableData(ref tobii_wearable_advanced_data_t data)
        {
            lock (_lockAdvancedData)
            {
                var advancedData = _advancedInternalQueue.Count >= AdvancedDataQueueSize
                    ? _advancedInternalQueue.Dequeue()
                    : new TobiiXR_AdvancedEyeTrackingData();

                StreamEngineDataMapper.MapAdvancedData(advancedData, ref data,
                    _streamEngineTracker.ConvergenceDistanceSupported, HeadToCenterEyeTranslation);
                _advancedInternalQueue.Enqueue(advancedData);
            }

            // Also fill in consumer api
            lock (_lockEyeTrackingDataLocal)
            {
                StreamEngineDataMapper.FromAdvancedData(_eyeTrackingDataLocalInternal, ref data,
                    _streamEngineTracker.ConvergenceDistanceSupported, HeadToCenterEyeTranslation);
            }
            
            lock (_lockPositionGuideData)
            {
                StreamEngineDataMapper.FillPositionGuideData(ref _positionGuideData, ref data);
            }
        }

        private void OnFoveatedData(ref tobii_wearable_foveated_gaze_t data)
        {
            _foveatedGazeDirectionLocal.x =
                data.gaze_direction_combined_normalized_xyz.x * -1; // Tobii to Unity CS conversion
            _foveatedGazeDirectionLocal.y = data.gaze_direction_combined_normalized_xyz.y;
            _foveatedGazeDirectionLocal.z = data.gaze_direction_combined_normalized_xyz.z;
        }

        #region Timesync

        public JobHandle StartTimesyncJob()
        {
            return _streamEngineTracker.StartTimesyncJob();
        }

        public TobiiXR_AdvancedTimesyncData? FinishTimesyncJob()
        {
            return _streamEngineTracker.FinishTimesyncJob();
        }

        public long GetSystemTimestamp()
        {
            Interop.tobii_system_clock(_streamEngineTracker.Context.Api, out var timestamp);
            return timestamp;
        }

        #endregion
    }

    public struct PositionGuideData
    {
        public Vector2 Left;
        public bool LeftIsValid;
        public Vector2 Right;
        public bool RightIsValid;
    }

    internal class CircularQueue<TData>
    {
        private readonly Queue<TData> _queue;
        private readonly object _lock = new object();
        private readonly int _capacity;

        public CircularQueue(int capacity)
        {
            _capacity = capacity;
            _queue = new Queue<TData>(capacity);
            _queue.Enqueue(default); // Seed with 1 item to enable Peek
        }

        public void WithLock(Action<Queue<TData>> action)
        {
            lock (_lock)
            {
                action.Invoke(_queue);
            }
        }

        public TData GetLatest()
        {
            lock (_lock)
            {
                return _queue.Peek();
            }
        }

        public void Add(TData item)
        {
            lock (_lock)
            {
                while (_queue.Count > _capacity - 1) _queue.Dequeue();
                _queue.Enqueue(item);    
            }
        }
    }
}