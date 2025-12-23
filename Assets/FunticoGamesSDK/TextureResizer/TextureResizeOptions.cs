using System;
using UnityEngine;

namespace FunticoGamesSDK.TextureResizer
{
    // Enum for defining resize mode
    public enum TextureResizeMode
    {
        None, // Do not resize
        Exact, // Exact dimensions
        KeepAspectRatio // Keep aspect ratio
    }

    // Enum for defining which dimension to use when keeping aspect ratio
    public enum AspectRatioConstraint
    {
        Width, // Fix width
        Height, // Fix height
        Auto // Automatic choice (use the larger value)
    }

    // Class with texture resize options
    [Serializable]
    public class TextureResizeOptions
    {
        public TextureResizeMode mode = TextureResizeMode.None;
        public int targetWidth;
        public int targetHeight;
        public AspectRatioConstraint aspectConstraint = AspectRatioConstraint.Auto;
        public FilterMode filterMode = FilterMode.Bilinear;

        // Default constructor
        public TextureResizeOptions() { }

        // Constructor for exact dimensions
        public TextureResizeOptions(int width, int height)
        {
            mode = TextureResizeMode.Exact;
            targetWidth = width;
            targetHeight = height;
        }

        // Constructor for keeping aspect ratio
        public TextureResizeOptions(int size, AspectRatioConstraint constraint = AspectRatioConstraint.Auto)
        {
            mode = TextureResizeMode.KeepAspectRatio;
            aspectConstraint = constraint;

            switch (constraint)
            {
                case AspectRatioConstraint.Width:
                    targetWidth = size;
                    break;
                case AspectRatioConstraint.Height:
                    targetHeight = size;
                    break;
                case AspectRatioConstraint.Auto:
                    targetWidth = targetHeight = size;
                    break;
            }
        }
    }
}