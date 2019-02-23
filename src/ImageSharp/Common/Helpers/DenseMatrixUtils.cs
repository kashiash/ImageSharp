﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.ImageSharp.Memory;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Primitives;

namespace SixLabors.ImageSharp
{
    /// <summary>
    /// Extension methods for <see cref="DenseMatrix{T}"/>.
    /// </summary>
    internal static class DenseMatrixUtils
    {
        /// <summary>
        /// Computes the sum of vectors in the span referenced by <paramref name="targetRowRef"/> weighted by the two kernel weight values.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="matrixY">The vertical dense matrix.</param>
        /// <param name="matrixX">The horizontal dense matrix.</param>
        /// <param name="sourcePixels">The source frame.</param>
        /// <param name="targetRowRef">The target row base reference.</param>
        /// <param name="row">The current row.</param>
        /// <param name="column">The current column.</param>
        /// <param name="maxRow">The maximum working area row.</param>
        /// <param name="maxColumn">The maximum working area column.</param>
        /// <param name="offsetColumn">The column offset to apply to source sampling.</param>
        public static void Convolve2D<TPixel>(
            in DenseMatrix<float> matrixY,
            in DenseMatrix<float> matrixX,
            Buffer2D<TPixel> sourcePixels,
            ref Vector4 targetRowRef,
            int row,
            int column,
            int maxRow,
            int maxColumn,
            int offsetColumn)
            where TPixel : struct, IPixel<TPixel>
        {
            Vector4 vectorY = default;
            Vector4 vectorX = default;
            int matrixHeight = matrixY.Rows;
            int matrixWidth = matrixY.Columns;
            int radiusY = matrixHeight >> 1;
            int radiusX = matrixWidth >> 1;
            int sourceOffsetColumnBase = column + offsetColumn;

            for (int y = 0; y < matrixHeight; y++)
            {
                int offsetY = (row + y - radiusY).Clamp(0, maxRow);
                Span<TPixel> sourceRowSpan = sourcePixels.GetRowSpan(offsetY);

                for (int x = 0; x < matrixWidth; x++)
                {
                    int offsetX = (sourceOffsetColumnBase + x - radiusX).Clamp(offsetColumn, maxColumn);
                    var currentColor = sourceRowSpan[offsetX].ToVector4();
                    Vector4Utils.Premultiply(ref currentColor);

                    vectorX += matrixX[y, x] * currentColor;
                    vectorY += matrixY[y, x] * currentColor;
                }
            }

            var vector = Vector4.SquareRoot((vectorX * vectorX) + (vectorY * vectorY));
            ref Vector4 target = ref Unsafe.Add(ref targetRowRef, column);
            vector.W = target.W;
            Vector4Utils.UnPremultiply(ref vector);
            target = vector;
        }

        /// <summary>
        /// Computes the sum of vectors in the span referenced by <paramref name="targetRowRef"/> weighted by the kernel weight values.
        /// </summary>
        /// <typeparam name="TPixel">The pixel format.</typeparam>
        /// <param name="matrix">The dense matrix.</param>
        /// <param name="sourcePixels">The source frame.</param>
        /// <param name="targetRowRef">The target row base reference.</param>
        /// <param name="row">The current row.</param>
        /// <param name="column">The current column.</param>
        /// <param name="maxRow">The maximum working area row.</param>
        /// <param name="maxColumn">The maximum working area column.</param>
        /// <param name="offsetColumn">The column offset to apply to source sampling.</param>
        /// <param name="passType">The convolution pass type.</param>
        public static void Convolve<TPixel>(
            in DenseMatrix<float> matrix,
            Buffer2D<TPixel> sourcePixels,
            ref Vector4 targetRowRef,
            int row,
            int column,
            int maxRow,
            int maxColumn,
            int offsetColumn,
            ConvolutionPassType passType)
            where TPixel : struct, IPixel<TPixel>
        {
            switch (passType)
            {
                case ConvolutionPassType.Single:
                    ConvolveSinglePass(matrix, sourcePixels, ref targetRowRef, row, column, maxRow, maxColumn, offsetColumn);
                    break;
                case ConvolutionPassType.First:
                    ConvolveFirstPass(matrix, sourcePixels, ref targetRowRef, row, column, maxRow, maxColumn, offsetColumn);
                    break;
                case ConvolutionPassType.Second:
                    ConvolveSecondPass(matrix, sourcePixels, ref targetRowRef, row, column, maxRow, maxColumn, offsetColumn);
                    break;
            }
        }

        private static void ConvolveSinglePass<TPixel>(
            in DenseMatrix<float> matrix,
            Buffer2D<TPixel> sourcePixels,
            ref Vector4 targetRowRef,
            int row,
            int column,
            int maxRow,
            int maxColumn,
            int offsetColumn)
            where TPixel : struct, IPixel<TPixel>
        {
            Vector4 vector = default;
            int matrixHeight = matrix.Rows;
            int matrixWidth = matrix.Columns;
            int radiusY = matrixHeight >> 1;
            int radiusX = matrixWidth >> 1;
            int sourceOffsetColumnBase = column + offsetColumn;

            for (int y = 0; y < matrixHeight; y++)
            {
                int offsetY = (row + y - radiusY).Clamp(0, maxRow);
                Span<TPixel> sourceRowSpan = sourcePixels.GetRowSpan(offsetY);

                for (int x = 0; x < matrixWidth; x++)
                {
                    int offsetX = (sourceOffsetColumnBase + x - radiusX).Clamp(offsetColumn, maxColumn);
                    var currentColor = sourceRowSpan[offsetX].ToVector4();
                    Vector4Utils.Premultiply(ref currentColor);

                    vector += matrix[y, x] * currentColor;
                }
            }

            ref Vector4 target = ref Unsafe.Add(ref targetRowRef, column);
            vector.W = target.W;
            Vector4Utils.UnPremultiply(ref vector);
            target = vector;
        }

        private static void ConvolveFirstPass<TPixel>(
            in DenseMatrix<float> matrix,
            Buffer2D<TPixel> sourcePixels,
            ref Vector4 targetRowRef,
            int row,
            int column,
            int maxRow,
            int maxColumn,
            int offsetColumn)
            where TPixel : struct, IPixel<TPixel>
        {
            Vector4 vector = default;
            int matrixHeight = matrix.Rows;
            int matrixWidth = matrix.Columns;
            int radiusY = matrixHeight >> 1;
            int radiusX = matrixWidth >> 1;
            int sourceOffsetColumnBase = column + offsetColumn;

            for (int y = 0; y < matrixHeight; y++)
            {
                int offsetY = (row + y - radiusY).Clamp(0, maxRow);
                Span<TPixel> sourceRowSpan = sourcePixels.GetRowSpan(offsetY);

                for (int x = 0; x < matrixWidth; x++)
                {
                    int offsetX = (sourceOffsetColumnBase + x - radiusX).Clamp(offsetColumn, maxColumn);
                    var currentColor = sourceRowSpan[offsetX].ToVector4();
                    Vector4Utils.Premultiply(ref currentColor);

                    vector += matrix[y, x] * currentColor;
                }
            }

            Unsafe.Add(ref targetRowRef, column) = vector;
        }

        private static void ConvolveSecondPass<TPixel>(
            in DenseMatrix<float> matrix,
            Buffer2D<TPixel> sourcePixels,
            ref Vector4 targetRowRef,
            int row,
            int column,
            int maxRow,
            int maxColumn,
            int offsetColumn)
            where TPixel : struct, IPixel<TPixel>
        {
            Vector4 vector = default;
            int matrixHeight = matrix.Rows;
            int matrixWidth = matrix.Columns;
            int radiusY = matrixHeight >> 1;
            int radiusX = matrixWidth >> 1;
            int sourceOffsetColumnBase = column + offsetColumn;

            for (int y = 0; y < matrixHeight; y++)
            {
                int offsetY = (row + y - radiusY).Clamp(0, maxRow);
                Span<TPixel> sourceRowSpan = sourcePixels.GetRowSpan(offsetY);

                for (int x = 0; x < matrixWidth; x++)
                {
                    int offsetX = (sourceOffsetColumnBase + x - radiusX).Clamp(offsetColumn, maxColumn);
                    var currentColor = sourceRowSpan[offsetX].ToVector4();
                    vector += matrix[y, x] * currentColor;
                }
            }

            ref Vector4 target = ref Unsafe.Add(ref targetRowRef, column);
            vector.W = target.W;
            Vector4Utils.UnPremultiply(ref vector);
            target = vector;
        }
    }
}