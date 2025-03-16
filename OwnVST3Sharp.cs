using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Reflection;

namespace OwnVst3Sharp
{
    /// <summary>
    /// C# wrapper for the OwnVst3 native library
    /// </summary>
    public class OwnVst3Wrapper : IDisposable
    {
        #region Native Structures

        [StructLayout(LayoutKind.Sequential)]
        public struct VST3ParameterC
        {
            public int id;
            public IntPtr name;
            public double minValue;
            public double maxValue;
            public double defaultValue;
            public double currentValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AudioBufferC
        {
            public IntPtr inputs;
            public IntPtr outputs;
            public int numChannels;
            public int numSamples;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MidiEventC
        {
            public byte status;
            public byte data1;
            public byte data2;
            public int sampleOffset;
        }

        #endregion

        #region Native Function Delegates

        private delegate IntPtr VST3Plugin_CreateDelegate();
        private delegate void VST3Plugin_DestroyDelegate(IntPtr handle);
        private delegate bool VST3Plugin_LoadPluginDelegate(IntPtr handle, string pluginPath);
        private delegate bool VST3Plugin_CreateEditorDelegate(IntPtr handle, IntPtr windowHandle);
        private delegate void VST3Plugin_CloseEditorDelegate(IntPtr handle);
        private delegate void VST3Plugin_ResizeEditorDelegate(IntPtr handle, int width, int height);
        private delegate int VST3Plugin_GetParameterCountDelegate(IntPtr handle);
        private delegate bool VST3Plugin_GetParameterAtDelegate(IntPtr handle, int index, ref VST3ParameterC parameter);
        private delegate bool VST3Plugin_SetParameterDelegate(IntPtr handle, int paramId, double value);
        private delegate double VST3Plugin_GetParameterDelegate(IntPtr handle, int paramId);
        private delegate bool VST3Plugin_InitializeDelegate(IntPtr handle, double sampleRate, int maxBlockSize);
        private delegate bool VST3Plugin_ProcessAudioDelegate(IntPtr handle, ref AudioBufferC buffer);
        private delegate bool VST3Plugin_ProcessMidiDelegate(IntPtr handle, [In] MidiEventC[] events, int eventCount);
        private delegate bool VST3Plugin_IsInstrumentDelegate(IntPtr handle);
        private delegate bool VST3Plugin_IsEffectDelegate(IntPtr handle);
        private delegate IntPtr VST3Plugin_GetNameDelegate(IntPtr handle);
        private delegate IntPtr VST3Plugin_GetVendorDelegate(IntPtr handle);
        private delegate IntPtr VST3Plugin_GetVersionDelegate(IntPtr handle);
        private delegate IntPtr VST3Plugin_GetPluginInfoDelegate(IntPtr handle);
        private delegate void VST3Plugin_ClearStringCacheDelegate();

        #endregion

        #region Private fields

        private IntPtr _pluginHandle;
        private IntPtr _libraryHandle;
        private bool _disposed = false;

        // Function delegate instances
        private VST3Plugin_CreateDelegate _createFunc;
        private VST3Plugin_DestroyDelegate _destroyFunc;
        private VST3Plugin_LoadPluginDelegate _loadPluginFunc;
        private VST3Plugin_CreateEditorDelegate _createEditorFunc;
        private VST3Plugin_CloseEditorDelegate _closeEditorFunc;
        private VST3Plugin_ResizeEditorDelegate _resizeEditorFunc;
        private VST3Plugin_GetParameterCountDelegate _getParameterCountFunc;
        private VST3Plugin_GetParameterAtDelegate _getParameterAtFunc;
        private VST3Plugin_SetParameterDelegate _setParameterFunc;
        private VST3Plugin_GetParameterDelegate _getParameterFunc;
        private VST3Plugin_InitializeDelegate _initializeFunc;
        private VST3Plugin_ProcessAudioDelegate _processAudioFunc;
        private VST3Plugin_ProcessMidiDelegate _processMidiFunc;
        private VST3Plugin_IsInstrumentDelegate _isInstrumentFunc;
        private VST3Plugin_IsEffectDelegate _isEffectFunc;
        private VST3Plugin_GetNameDelegate _getNameFunc;
        private VST3Plugin_GetVendorDelegate _getVendorFunc;
        private VST3Plugin_GetVersionDelegate _getVersionFunc;
        private VST3Plugin_GetPluginInfoDelegate _getPluginInfoFunc;
        private VST3Plugin_ClearStringCacheDelegate _clearStringCacheFunc;

        #endregion

        #region Constructor and Destructor

        /// <summary>
        /// Creates a new OwnVst3Wrapper instance
        /// </summary>
        /// <param name="dllPath">Path to the ownvst3_wrapper.dll</param>
        public OwnVst3Wrapper(string dllPath)
        {
            // Load the DLL using NativeLibrary.Load
            _libraryHandle = NativeLibrary.Load(dllPath);
            if (_libraryHandle == IntPtr.Zero)
            {
                throw new DllNotFoundException($"Failed to load library: {dllPath}");
            }

            // Initialize delegates from the loaded DLL
            InitializeDelegates();

            // Create the plugin instance
            _pluginHandle = _createFunc();
            if (_pluginHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create VST3 plugin instance");
            }
        }

        ~OwnVst3Wrapper()
        {
            Dispose(false);
        }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (_pluginHandle != IntPtr.Zero)
                {
                    _pluginHandle = IntPtr.Zero;
                }

                if (_libraryHandle != IntPtr.Zero)
                {
                    NativeLibrary.Free(_libraryHandle);
                    _libraryHandle = IntPtr.Zero;
                }

                _disposed = true;
            }
        }

        #endregion

        #region Private helper methods

        private void InitializeDelegates()
        {
            _createFunc = GetDelegate<VST3Plugin_CreateDelegate>("VST3Plugin_Create");
            _destroyFunc = GetDelegate<VST3Plugin_DestroyDelegate>("VST3Plugin_Destroy");
            _loadPluginFunc = GetDelegate<VST3Plugin_LoadPluginDelegate>("VST3Plugin_LoadPlugin");
            _createEditorFunc = GetDelegate<VST3Plugin_CreateEditorDelegate>("VST3Plugin_CreateEditor");
            _closeEditorFunc = GetDelegate<VST3Plugin_CloseEditorDelegate>("VST3Plugin_CloseEditor");
            _resizeEditorFunc = GetDelegate<VST3Plugin_ResizeEditorDelegate>("VST3Plugin_ResizeEditor");
            _getParameterCountFunc = GetDelegate<VST3Plugin_GetParameterCountDelegate>("VST3Plugin_GetParameterCount");
            _getParameterAtFunc = GetDelegate<VST3Plugin_GetParameterAtDelegate>("VST3Plugin_GetParameterAt");
            _setParameterFunc = GetDelegate<VST3Plugin_SetParameterDelegate>("VST3Plugin_SetParameter");
            _getParameterFunc = GetDelegate<VST3Plugin_GetParameterDelegate>("VST3Plugin_GetParameter");
            _initializeFunc = GetDelegate<VST3Plugin_InitializeDelegate>("VST3Plugin_Initialize");
            _processAudioFunc = GetDelegate<VST3Plugin_ProcessAudioDelegate>("VST3Plugin_ProcessAudio");
            _processMidiFunc = GetDelegate<VST3Plugin_ProcessMidiDelegate>("VST3Plugin_ProcessMidi");
            _isInstrumentFunc = GetDelegate<VST3Plugin_IsInstrumentDelegate>("VST3Plugin_IsInstrument");
            _isEffectFunc = GetDelegate<VST3Plugin_IsEffectDelegate>("VST3Plugin_IsEffect");
            _getNameFunc = GetDelegate<VST3Plugin_GetNameDelegate>("VST3Plugin_GetName");
            _getVendorFunc = GetDelegate<VST3Plugin_GetVendorDelegate>("VST3Plugin_GetVendor");
            _getVersionFunc = GetDelegate<VST3Plugin_GetVersionDelegate>("VST3Plugin_GetVersion");
            _getPluginInfoFunc = GetDelegate<VST3Plugin_GetPluginInfoDelegate>("VST3Plugin_GetPluginInfo");
            _clearStringCacheFunc = GetDelegate<VST3Plugin_ClearStringCacheDelegate>("VST3Plugin_ClearStringCache");
        }

        private T GetDelegate<T>(string functionName) where T : Delegate
        {
            IntPtr funcPtr = NativeLibrary.GetExport(_libraryHandle, functionName);
            if (funcPtr == IntPtr.Zero)
            {
                throw new EntryPointNotFoundException($"Function not found: {functionName}");
            }
            return Marshal.GetDelegateForFunctionPointer<T>(funcPtr);
        }

        #endregion

        #region Public API methods

        /// <summary>
        /// Loads a VST3 plugin from the specified path
        /// </summary>
        /// <param name="pluginPath">Path to the VST3 plugin</param>
        /// <returns>True if successful</returns>
        public bool LoadPlugin(string pluginPath)
        {
            CheckDisposed();
            return _loadPluginFunc(_pluginHandle, pluginPath);
        }

        /// <summary>
        /// Creates an editor view for the plugin
        /// </summary>
        /// <param name="windowHandle">Window handle where the editor should appear</param>
        /// <returns>True if successful</returns>
        public bool CreateEditor(IntPtr windowHandle)
        {
            CheckDisposed();
            return _createEditorFunc(_pluginHandle, windowHandle);
        }

        /// <summary>
        /// Closes the plugin editor
        /// </summary>
        public void CloseEditor()
        {
            CheckDisposed();
            _closeEditorFunc(_pluginHandle);
        }

        /// <summary>
        /// Resizes the plugin editor
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void ResizeEditor(int width, int height)
        {
            CheckDisposed();
            _resizeEditorFunc(_pluginHandle, width, height);
        }

        /// <summary>
        /// Returns the number of parameters in the plugin
        /// </summary>
        /// <returns>Parameter count</returns>
        public int GetParameterCount()
        {
            CheckDisposed();
            return _getParameterCountFunc(_pluginHandle);
        }

        /// <summary>
        /// Gets a parameter at the specified index
        /// </summary>
        /// <param name="index">Parameter index</param>
        /// <returns>Parameter data</returns>
        public VST3Parameter GetParameterAt(int index)
        {
            CheckDisposed();

            VST3ParameterC paramC = new VST3ParameterC();
            bool success = _getParameterAtFunc(_pluginHandle, index, ref paramC);

            if (!success)
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid parameter index");

            return new VST3Parameter
            {
                Id = paramC.id,
                Name = Marshal.PtrToStringAnsi(paramC.name),
                MinValue = paramC.minValue,
                MaxValue = paramC.maxValue,
                DefaultValue = paramC.defaultValue,
                CurrentValue = paramC.currentValue
            };
        }

        /// <summary>
        /// Sets a parameter value
        /// </summary>
        /// <param name="paramId">Parameter ID</param>
        /// <param name="value">New value</param>
        /// <returns>True if successful</returns>
        public bool SetParameter(int paramId, double value)
        {
            CheckDisposed();
            return _setParameterFunc(_pluginHandle, paramId, value);
        }

        /// <summary>
        /// Gets a parameter's current value
        /// </summary>
        /// <param name="paramId">Parameter ID</param>
        /// <returns>Parameter value</returns>
        public double GetParameter(int paramId)
        {
            CheckDisposed();
            return _getParameterFunc(_pluginHandle, paramId);
        }

        /// <summary>
        /// Initializes the plugin
        /// </summary>
        /// <param name="sampleRate">Sample rate</param>
        /// <param name="maxBlockSize">Maximum block size</param>
        /// <returns>True if successful</returns>
        public bool Initialize(double sampleRate, int maxBlockSize)
        {
            CheckDisposed();
            return _initializeFunc(_pluginHandle, sampleRate, maxBlockSize);
        }

        /// <summary>
        /// Processes audio data through the plugin
        /// </summary>
        /// <param name="inputs">Input audio data</param>
        /// <param name="outputs">Output audio data</param>
        /// <param name="numChannels">Number of channels</param>
        /// <param name="numSamples">Number of samples per channel</param>
        /// <returns>True if successful</returns>
        public bool ProcessAudio(float[][] inputs, float[][] outputs, int numChannels, int numSamples)
        {
            CheckDisposed();

            // Convert to interop structure
            AudioBufferC buffer = new AudioBufferC();

            // Create appropriate pinned GCHandles for the data
            GCHandle[] inputHandles = new GCHandle[numChannels];
            GCHandle[] outputHandles = new GCHandle[numChannels];

            IntPtr[] inputPtrs = new IntPtr[numChannels];
            IntPtr[] outputPtrs = new IntPtr[numChannels];

            for (int i = 0; i < numChannels; i++)
            {
                inputHandles[i] = GCHandle.Alloc(inputs[i], GCHandleType.Pinned);
                outputHandles[i] = GCHandle.Alloc(outputs[i], GCHandleType.Pinned);

                inputPtrs[i] = inputHandles[i].AddrOfPinnedObject();
                outputPtrs[i] = outputHandles[i].AddrOfPinnedObject();
            }

            // Allocate pointers array
            GCHandle inputsHandle = GCHandle.Alloc(inputPtrs, GCHandleType.Pinned);
            GCHandle outputsHandle = GCHandle.Alloc(outputPtrs, GCHandleType.Pinned);

            buffer.inputs = inputsHandle.AddrOfPinnedObject();
            buffer.outputs = outputsHandle.AddrOfPinnedObject();
            buffer.numChannels = numChannels;
            buffer.numSamples = numSamples;

            bool result = _processAudioFunc(_pluginHandle, ref buffer);

            // Free GCHandles
            inputsHandle.Free();
            outputsHandle.Free();

            for (int i = 0; i < numChannels; i++)
            {
                inputHandles[i].Free();
                outputHandles[i].Free();
            }

            return result;
        }

        /// <summary>
        /// Processes MIDI events
        /// </summary>
        /// <param name="events">MIDI events</param>
        /// <returns>True if successful</returns>
        public bool ProcessMidi(MidiEvent[] events)
        {
            CheckDisposed();

            if (events == null || events.Length == 0)
                return false;

            MidiEventC[] eventsC = new MidiEventC[events.Length];

            for (int i = 0; i < events.Length; i++)
            {
                eventsC[i] = new MidiEventC
                {
                    status = events[i].Status,
                    data1 = events[i].Data1,
                    data2 = events[i].Data2,
                    sampleOffset = events[i].SampleOffset
                };
            }

            return _processMidiFunc(_pluginHandle, eventsC, events.Length);
        }

        /// <summary>
        /// Checks if the plugin is an instrument
        /// </summary>
        public bool IsInstrument
        {
            get
            {
                CheckDisposed();
                return _isInstrumentFunc(_pluginHandle);
            }
        }

        /// <summary>
        /// Checks if the plugin is an effect
        /// </summary>
        public bool IsEffect
        {
            get
            {
                CheckDisposed();
                return _isEffectFunc(_pluginHandle);
            }
        }

        /// <summary>
        /// Returns the plugin name
        /// </summary>
        public string Name
        {
            get
            {
                CheckDisposed();
                IntPtr namePtr = _getNameFunc(_pluginHandle);
                return Marshal.PtrToStringAnsi(namePtr);
            }
        }

        /// <summary>
        /// Returns the plugin vendor
        /// </summary>
        public string Vendor
        {
            get
            {
                CheckDisposed();
                IntPtr vendorPtr = _getVendorFunc(_pluginHandle);
                return Marshal.PtrToStringAnsi(vendorPtr);
            }
        }

        /// <summary>
        /// Returns the plugin version
        /// </summary>
        public string Version
        {
            get
            {
                CheckDisposed();
                IntPtr versionPtr = _getVersionFunc(_pluginHandle);
                return Marshal.PtrToStringAnsi(versionPtr);
            }
        }

        /// <summary>
        /// Returns the plugin information
        /// </summary>
        public string PluginInfo
        {
            get
            {
                CheckDisposed();
                IntPtr infoPtr = _getPluginInfoFunc(_pluginHandle);
                return Marshal.PtrToStringAnsi(infoPtr);
            }
        }

        /// <summary>
        /// Clears the string cache
        /// </summary>
        public void ClearStringCache()
        {
            CheckDisposed();
            _clearStringCacheFunc();
        }

        /// <summary>
        /// Gets all parameters
        /// </summary>
        /// <returns>List of parameters</returns>
        public List<VST3Parameter> GetAllParameters()
        {
            CheckDisposed();

            int count = GetParameterCount();
            List<VST3Parameter> parameters = new List<VST3Parameter>(count);

            for (int i = 0; i < count; i++)
            {
                parameters.Add(GetParameterAt(i));
            }

            return parameters;
        }

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(OwnVst3Wrapper));
        }

        #endregion
    }

    #region Helper classes

    /// <summary>
    /// C# representation of a VST3 parameter
    /// </summary>
    public class VST3Parameter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double DefaultValue { get; set; }
        public double CurrentValue { get; set; }
    }

    /// <summary>
    /// C# representation of a MIDI event
    /// </summary>
    public class MidiEvent
    {
        public byte Status { get; set; }
        public byte Data1 { get; set; }
        public byte Data2 { get; set; }
        public int SampleOffset { get; set; }
    }

    #endregion
}