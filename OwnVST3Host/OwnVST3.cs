using System.Runtime.InteropServices;

namespace OwnVST3Host
{
    /// <summary>
    /// C# wrapper for the OwnVst3 native library
    /// </summary>
    public partial class OwnVst3Wrapper : IDisposable
    {
#nullable disable
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
#nullable restore
    }
}
