using UnityEngine;

namespace FunticoGamesSDK.TextureResizer
{
    // Utility for resizing textures
    public static class TextureResizerUtility
    {
        /// <summary>
        /// Resizes a texture according to the specified options
        /// </summary>
        public static Texture2D ResizeTexture(Texture2D sourceTexture, TextureResizeOptions options)
        {
            if (sourceTexture == null)
            {
                Debug.LogError("Source texture is null");
                return null;
            }

            if (options == null || options.mode == TextureResizeMode.None)
            {
                return sourceTexture;
            }

            // Calculate target dimensions
            Vector2Int targetSize = CalculateTargetSize(
                sourceTexture.width,
                sourceTexture.height,
                options
            );

            // If dimensions are unchanged, return the original texture
            if (targetSize.x == sourceTexture.width && targetSize.y == sourceTexture.height)
            {
                return sourceTexture;
            }

            // Create a temporary RenderTexture for resizing
            RenderTexture rt = RenderTexture.GetTemporary(targetSize.x, targetSize.y);
            rt.filterMode = options.filterMode;

            // Save the current active RenderTexture
            RenderTexture previous = RenderTexture.active;

            // Copy the source texture into the RenderTexture with new size
            Graphics.Blit(sourceTexture, rt);
            RenderTexture.active = rt;

            // Create a new texture with the required size
            Texture2D resizedTexture = new Texture2D(targetSize.x, targetSize.y, sourceTexture.format, false)
                {
                    wrapMode = sourceTexture.wrapMode,
                    anisoLevel = sourceTexture.anisoLevel,
                    filterMode = options.filterMode,
                    name = $"{sourceTexture.name}_resized"
                };
            resizedTexture.ReadPixels(new Rect(0, 0, targetSize.x, targetSize.y), 0, 0);
            resizedTexture.Apply();

            // Restore the previous RenderTexture and release the temporary one
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            return resizedTexture;
        }

        /// <summary>
        /// Calculates target texture dimensions based on options
        /// </summary>
        private static Vector2Int CalculateTargetSize(int sourceWidth, int sourceHeight, TextureResizeOptions options)
        {
            switch (options.mode)
            {
                case TextureResizeMode.Exact:
                    return new Vector2Int(options.targetWidth, options.targetHeight);

                case TextureResizeMode.KeepAspectRatio:
                    float aspectRatio = (float) sourceWidth / sourceHeight;

                    switch (options.aspectConstraint)
                    {
                        case AspectRatioConstraint.Width:
                            return new Vector2Int(
                                options.targetWidth,
                                Mathf.RoundToInt(options.targetWidth / aspectRatio)
                            );

                        case AspectRatioConstraint.Height:
                            return new Vector2Int(
                                Mathf.RoundToInt(options.targetHeight * aspectRatio),
                                options.targetHeight
                            );

                        case AspectRatioConstraint.Auto:
                            // Choose the constraint that gives the smaller result
                            int widthBasedHeight = Mathf.RoundToInt(options.targetWidth / aspectRatio);
                            int heightBasedWidth = Mathf.RoundToInt(options.targetHeight * aspectRatio);

                            if (widthBasedHeight <= options.targetHeight)
                            {
                                return new Vector2Int(options.targetWidth, widthBasedHeight);
                            }
                            else
                            {
                                return new Vector2Int(heightBasedWidth, options.targetHeight);
                            }
                    }

                    break;
            }

            return new Vector2Int(sourceWidth, sourceHeight);
        }
    }
}