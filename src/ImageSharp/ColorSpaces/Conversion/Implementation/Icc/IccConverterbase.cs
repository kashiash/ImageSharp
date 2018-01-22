﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.MetaData.Profiles.Icc;

namespace SixLabors.ImageSharp.ColorSpaces.Conversion.Implementation.Icc
{
    /// <summary>
    /// Color converter for ICC profiles
    /// </summary>
    internal abstract partial class IccConverterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IccConverterBase"/> class.
        /// </summary>
        /// <param name="profile">The ICC profile to use for the conversions</param>
        /// <param name="toPcs">True if the conversion is to the profile connection space (PCS); False if the conversion is to the data space</param>
        protected IccConverterBase(IccProfile profile, bool toPcs)
        {
            Guard.NotNull(profile, nameof(profile));
            this.Init(profile, toPcs, profile.Header.RenderingIntent);
        }

        /// <summary>
        /// Converts colors with the initially provided ICC profile
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <returns>The converted value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 Calculate(Vector4 value)
        {
            return this.calculator.Calculate(value);
        }
    }
}