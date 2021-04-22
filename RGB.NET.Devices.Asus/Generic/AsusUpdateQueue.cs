﻿using System;
using AuraServiceLib;
using RGB.NET.Core;

namespace RGB.NET.Devices.Asus
{
    /// <inheritdoc />
    /// <summary>
    /// Represents the update-queue performing updates for asus devices.
    /// </summary>
    public class AsusUpdateQueue : UpdateQueue
    {
        #region Properties & Fields

        /// <summary>
        /// The device to be updated.
        /// </summary>
        protected IAuraSyncDevice Device { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsusUpdateQueue"/> class.
        /// </summary>
        /// <param name="updateTrigger">The update trigger used by this queue.</param>
        public AsusUpdateQueue(IDeviceUpdateTrigger updateTrigger, IAuraSyncDevice device)
            : base(updateTrigger)
        {
            this.Device = device;
        }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void Update(in ReadOnlySpan<(object key, Color color)> dataSet)
        {
            try
            {
                if ((Device.Type == (uint)AsusDeviceType.KEYBOARD_RGB) || (Device.Type == (uint)AsusDeviceType.NB_KB_RGB))
                {
                    if (Device is not IAuraSyncKeyboard keyboard)
                        return;
                    
                    foreach ((object customData, Color value) in dataSet)
                    {
                        (bool, int) customDataTuple = ((bool, int))customData;
                        if (customDataTuple.Item1)
                        {
                            IAuraRgbLight light = keyboard.Key[(ushort)customDataTuple.Item2];
                            (_, byte r, byte g, byte b) = value.GetRGBBytes();
                            light.Red = r;
                            light.Green = g;
                            light.Blue = b;
                        }
                        else
                        {
                            IAuraRgbLight light = keyboard.Lights[customDataTuple.Item2];
                            (_, byte r, byte g, byte b) = value.GetRGBBytes();
                            light.Red = r;
                            light.Green = g;
                            light.Blue = b;
                        }
                    }
                }
                else
                {
                    foreach ((object key, Color value) in dataSet)
                    {
                        int index = (int)key;
                        IAuraRgbLight light = Device.Lights[index];

                        (_, byte r, byte g, byte b) = value.GetRGBBytes();
                        light.Red = r;
                        light.Green = g;
                        light.Blue = b;
                    }
                }

                Device.Apply();
            }
            catch
            { /* "The server threw an exception." seems to be a thing here ... */
            }
        }

        #endregion
    }
}
