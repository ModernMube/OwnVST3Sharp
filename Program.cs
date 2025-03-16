using System;
using System.IO;
using System.Runtime.InteropServices;
using OwnVst3Sharp;

namespace OwnVst3SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("OwnVst3 C# Sample Program");
            Console.WriteLine("------------------------");

            // Path to the dll - ensure this is the correct path in your runtime environment
            string dllPath = @"Library\ownvst3.dll";

            // Check if the path exists
            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"ERROR: DLL not found: {dllPath}");
                Console.WriteLine("Please check the path or place the DLL in the application directory.");
                return;
            }

            try
            {
                // Create VST3 plugin wrapper instance
                using (OwnVst3Wrapper vst = new OwnVst3Wrapper(dllPath))
                {
                    Console.WriteLine("VST3 wrapper successfully initialized.");

                    // Load a VST3 plugin - replace the path with your VST3 plugin path
                    string pluginPath = @"C:\Program Files\Common Files\VST3\TDR Nova.vst3";
                    Console.WriteLine($"Loading VST3 plugin: {pluginPath}");

                    if (vst.LoadPlugin(pluginPath))
                    {
                        Console.WriteLine("Plugin loaded successfully!");
                        Console.WriteLine($"Name: {vst.Name}");
                        Console.WriteLine($"Is Instrument: {vst.IsInstrument}");
                        Console.WriteLine($"Is Effect: {vst.IsEffect}");

                        // Initialize the plugin
                        double sampleRate = 44100.0;
                        int blockSize = 512;
                        if (vst.Initialize(sampleRate, blockSize))
                        {
                            Console.WriteLine($"Plugin initialized: {sampleRate} Hz, {blockSize} samples/block");

                            // Query parameters
                            var parameters = vst.GetAllParameters();
                            Console.WriteLine($"\nNumber of parameters: {parameters.Count}");

                            if (parameters.Count > 0)
                            {
                                Console.WriteLine("\nParameters:");
                                Console.WriteLine("ID\tName\t\tCurrent\tMin\tMax\tDefault");
                                Console.WriteLine("------------------------------------------------------------------");

                                foreach (var param in parameters)
                                {
                                    Console.WriteLine($"{param.Id}\t{param.Name}\t\t{param.CurrentValue:F2}\t{param.MinValue:F2}\t{param.MaxValue:F2}\t{param.DefaultValue:F2}");
                                }

                                // Modify parameter example (on first parameter)
                                if (parameters.Count > 0)
                                {
                                    var firstParam = parameters[0];
                                    double newValue = (firstParam.MaxValue + firstParam.MinValue) / 2; // Mid value

                                    Console.WriteLine($"\nModifying parameter '{firstParam.Name}': {firstParam.CurrentValue:F2} -> {newValue:F2}");

                                    if (vst.SetParameter(firstParam.Id, newValue))
                                    {
                                        double actualValue = vst.GetParameter(firstParam.Id);
                                        Console.WriteLine($"Parameter modified successfully. Current value: {actualValue:F2}");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Failed to modify parameter.");
                                    }
                                }
                            }

                            // E.g., 2 channels, 512 samples
                            int numChannels = 2;
                            int numSamples = blockSize;

                            // Create input/output buffers
                            float[][] inputs = new float[numChannels][];
                            float[][] outputs = new float[numChannels][];

                            if(vst.IsEffect)
                            {
                                // Audio processing example
                                Console.WriteLine("\nSimulating audio processing...");

                                for (int c = 0; c < numChannels; c++)
                                {
                                    inputs[c] = new float[numSamples];
                                    outputs[c] = new float[numSamples];

                                    // Generate test input data (sine wave)
                                    for (int i = 0; i < numSamples; i++)
                                    {
                                        inputs[c][i] = (float)Math.Sin(2 * Math.PI * 440 * i / sampleRate) * 0.5f;
                                    }
                                }

                                // Process audio
                                bool processResult = vst.ProcessAudio(inputs, outputs, numChannels, numSamples);
                                Console.WriteLine($"Audio processing result: {(processResult ? "Success" : "Failed")}");

                            }
                            else
                            {
                                // MIDI example - play C4 note
                                Console.WriteLine("\nSending MIDI event (C4 note)...");

                                byte noteOn = 0x90;  // MIDI Note On, channel 1
                                byte noteC4 = 60;    // C4 note (middle C)
                                byte velocity = 100; // Velocity

                                MidiEvent[] midiEvents = new MidiEvent[]
                                {
                                new MidiEvent { Status = noteOn, Data1 = noteC4, Data2 = velocity, SampleOffset = 0 }
                                };

                                bool midiResult = vst.ProcessMidi(midiEvents);
                                Console.WriteLine($"MIDI processing result: {(midiResult ? "Success" : "Failed")}");

                                // Process audio after MIDI
                                vst.ProcessAudio(inputs, outputs, numChannels, numSamples);

                                // Turn off the MIDI note
                                Console.WriteLine("Turning off MIDI note...");

                                byte noteOff = 0x80;  // MIDI Note Off, channel 1
                                byte releaseVel = 0;  // Release velocity

                                midiEvents = new MidiEvent[]
                                {
                                new MidiEvent { Status = noteOff, Data1 = noteC4, Data2 = releaseVel, SampleOffset = 0 }
                                };

                                vst.ProcessMidi(midiEvents);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to initialize plugin.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to load plugin!");
                    }
                }
            }
            catch (DllNotFoundException ex)
            {
                Console.WriteLine($"ERROR: Failed to load DLL: {ex.Message}");
            }
            catch (EntryPointNotFoundException ex)
            {
                Console.WriteLine($"ERROR: Function not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}