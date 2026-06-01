using System;
using Godot;
using System.Collections.Generic; // Added for Dictionary

#region License / Copyright

/*
 * Copyright © 2023-2026, Michieal.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

#endregion

namespace ApophisSoftware;

public static class ImageManipulation{
	public static Texture2D AdjustBCS(Image Source, float Brightness, float Contrast, float Saturation){
		Image proxy = new Image();
		proxy.CopyFrom(Source);

		proxy.AdjustBcs(Brightness, Contrast, Saturation);
		return ImageTexture.CreateFromImage(proxy);
	}

	public static Texture2D Clip(Image Source, float Percentage, ClipImgSpecs HowToClip){
		// Get the size of the image
		int width = Source.GetWidth();
		int height = Source.GetHeight();

		Image proxy = new();
		proxy.CopyFrom(Source);

		Percentage = Mathf.Clamp(Percentage, 0f, 1f);

		int pixels = 0;

		switch (HowToClip){
			case ClipImgSpecs.HorizontalFromLeft:
				pixels = Mathf.FloorToInt(width * Percentage);

				// Loop through each pixel and apply clip
				for (int x = 0; x < width; x++){
					for (int y = 0; y < height; y++){
						// Get the original pixel color

						if (x > pixels){
							Color originalColor = proxy.GetPixel(x, y);
							// make transparent.
							Color shiftedColor = originalColor;
							shiftedColor.A = 0;

							// Set the new color to the pixel
							proxy.SetPixel(x, y, shiftedColor);
						}
					}
				}

				break;

			case ClipImgSpecs.HorizontalFromRight:
				pixels = Mathf.FloorToInt(width * Percentage);

				// Loop through each pixel and apply clip
				for (int x = width; x > 0; x--){
					for (int y = 0; y < height; y++){
						// Get the original pixel color

						if (x < pixels){
							Color originalColor = proxy.GetPixel(x, y);
							// make transparent.
							Color shiftedColor = originalColor;
							shiftedColor.A = 0;

							// Set the new color to the pixel
							proxy.SetPixel(x, y, shiftedColor);
						}
					}
				}

				break;

			case ClipImgSpecs.VerticalFromBottom:
				pixels = Mathf.FloorToInt(height * Percentage);

				// Loop through each pixel and apply clip
				for (int y = height; y > 0; y--){
					for (int x = 0; x < width; x++){
						// Get the original pixel color

						if (y < pixels){
							Color originalColor = proxy.GetPixel(x, y);
							// make transparent.
							Color shiftedColor = originalColor;
							shiftedColor.A = 0;

							// Set the new color to the pixel
							proxy.SetPixel(x, y, shiftedColor);
						}
					}
				}

				break;

			case ClipImgSpecs.VerticalFromTop:
				pixels = Mathf.FloorToInt(height * Percentage);

				// Loop through each pixel and apply clip
				for (int y = 0; y < height; y++){
					for (int x = 0; x < width; x++){
						// Get the original pixel color

						if (y > pixels){
							Color originalColor = proxy.GetPixel(x, y);
							// make transparent.
							Color shiftedColor = originalColor;
							shiftedColor.A = 0;

							// Set the new color to the pixel
							proxy.SetPixel(x, y, shiftedColor);
						}
					}
				}

				break;

			default:
				break;
		}


		return ImageTexture.CreateFromImage(proxy);
	}

	public static Texture2D ColorizeTexture(Image Source, Color ColorToUse){
		Image tex = new Image();
		tex.CopyFrom(Source);

		// Get the size of the image
		int width = tex.GetWidth();
		int height = tex.GetHeight();

		// Loop through each pixel and apply colorization
		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				// Get the original pixel color
				Color originalColor = tex.GetPixel(x, y);

				// Apply colorization (multiply the original color by the desired color)
				Color newColor = originalColor * ColorToUse;

				// Set the new color to the pixel
				tex.SetPixel(x, y, newColor);
			}
		}

		// return the modified image.
		return ImageTexture.CreateFromImage(tex);
	}

	public struct TextureAtlasResult{
		public ImageTexture AtlasTexture;
		public Dictionary<string, Rect2> UVRects;
	}

	public static TextureAtlasResult CreateTextureAtlas(string[] imagePaths){
		Dictionary<string, Rect2> uvRects = new Dictionary<string, Rect2>();
		TextureAtlasResult result = new TextureAtlasResult{ UVRects = uvRects };

		List<Image> images = LoadImages(imagePaths); // Use the new helper
		if (images == null || images.Count == 0){
			Logging.Log("error", "Failed to load images for atlas creation.");
			result.AtlasTexture = new ImageTexture(); // Return empty texture
			return result;
		}

		int atlasWidth = 0;
		int maxHeight = 0;
		// Calculate total width and max height
		// All images are now standardized to 32x32, so this simplifies
		if (images.Count > 0){
			atlasWidth = images.Count * 32; // N * 32
			maxHeight = 32; // All are 32 high
		}


		Image atlasImage = Image.Create(atlasWidth, maxHeight, false, Image.Format.Rgba8);
		int currentX = 0;
		for (int i = 0; i < images.Count; i++){
			Image img = images[i];
			atlasImage.BlitRect(img, new Rect2I(0, 0, img.GetWidth(), img.GetHeight()), new Vector2I(currentX, 0));

			// Store UV rect for this texture
			uvRects[imagePaths[i]] = new Rect2(
				(float)currentX / atlasWidth,
				0,
				(float)img.GetWidth() / atlasWidth,
				(float)img.GetHeight() / maxHeight
			);
			currentX += img.GetWidth();
		}

		result.AtlasTexture = ImageTexture.CreateFromImage(atlasImage);
		return result;
	}

	public static Texture2D CropImage(Image Source, int Height, int Width){
		Image tex = new Image();
		tex.CopyFrom(Source);
		Source.Crop(Width, Height);
		return ImageTexture.CreateFromImage(tex);
	}

	public static Texture2D LoadImageFromFile(string FilePath){
		Image tex;

		// Load each texture
		try{
			tex = Image.LoadFromFile(FilePath);
		}
		catch (Exception error){
			// Handle loading error, e.g., print a message
			Logging.Log("error", $"Error loading texture: {FilePath}.\nError message: {error.Message}");
			Image notex = Image.LoadFromFile("res://Sprites/MissingTexture.png");
			notex.Fill(Colors.Fuchsia);
			return ImageTexture.CreateFromImage(notex);
		}

		return ImageTexture.CreateFromImage(tex);
	}

	// Modified helper to load Images directly and resize them to 32x32
	private static List<Image> LoadImages(string[] paths){
		List<Image> loadedImages = new List<Image>();
		foreach (string path in paths){
			Image tex;
			try{
				tex = Image.LoadFromFile(path);
				if (tex.GetWidth() != 32 || tex.GetHeight() != 32){
					tex.Resize(32, 32, Image.Interpolation.Nearest); // Resize to 32x32 using nearest-neighbor interpolation
					GD.Print($"Resized texture '{path}' to 32x32.");
				}

				loadedImages.Add(tex);
			}
			catch (Exception error){
				Logging.Log("error", $"Error loading texture: {path}.\nError message: {error.Message}");
				// Decide how to handle: return null, add a placeholder, or skip
				// For now, we'll skip and log an error.
			}
		}

		return loadedImages;
	}

	public static Texture2D MakeTransparent(Image Source, Color SrcColor){
		Image proxy = new Image();
		proxy.CopyFrom(Source);

		int height = proxy.GetHeight();
		int width = proxy.GetWidth();

		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				Color pixel = proxy.GetPixel(x, y);
				pixel.A = 0;
				if (pixel == SrcColor){
					proxy.SetPixel(x, y, pixel);
				}
			}
		}

		return ImageTexture.CreateFromImage(proxy);
	}

	public static Texture2D ManipulateTexture(Image Source, bool FlipHorizontal, bool FlipVertical,
		RotateImgSpec Rotation = RotateImgSpec.None){
		Image tex = new Image();
		tex.CopyFrom(Source);

		// Flip horizontally
		if (FlipHorizontal)
			tex.FlipX();

		// Flip vertically
		if (FlipVertical)
			tex.FlipY();

		// Rotate the image

		switch (Rotation){
			case RotateImgSpec.Right90:
				tex.Rotate90(ClockDirection.Clockwise); // Rotate Right
				break;
			case RotateImgSpec.Left90:
				tex.Rotate90(ClockDirection.Counterclockwise); // Rotate left
				break;
			case RotateImgSpec.Rotate180:
				tex.Rotate180(); // Rotate 180 degrees
				break;
			default:
				break;
		}

		// return the modified image.
		return ImageTexture.CreateFromImage(tex);
	}

	public static Texture2D MaskImage(Image Source, Image Mask){
		Image proxy = new Image();
		proxy.CopyFrom(Source);
		Image proxyMask = new Image();
		proxyMask.CopyFrom(Mask);

		int height = proxy.GetHeight();
		int width = proxy.GetWidth();

		proxyMask.Resize(width, height, Image.Interpolation.Nearest); // make sure that the mask is the same size.

		Color removed = new Color(0f, 0f, 0f, 0f);

		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				Color pixel = proxyMask.GetPixel(x, y);
				if (pixel.A == 0 || pixel == Colors.Black){
					proxy.SetPixel(x, y, removed);
				}
			}
		}

		return ImageTexture.CreateFromImage(proxy);
	}

	public static Texture2D ReplaceColor(Image Source, Color SrcColor, Color NewColor){
		Image proxy = new Image();
		proxy.CopyFrom(Source);

		int height = proxy.GetHeight();
		int width = proxy.GetWidth();

		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				Color pixel = proxy.GetPixel(x, y);
				if (pixel == SrcColor){
					proxy.SetPixel(x, y, NewColor);
				}
			}
		}

		return ImageTexture.CreateFromImage(proxy);
	}

	public static Texture2D ShiftHsv(Image Source, float HueShift, float SaturationShift, float ValueShift){
		// Make a proxy...
		Image proxy = new Image();
		proxy.CopyFrom(Source);

		// Get the size of the image
		int width = proxy.GetWidth();
		int height = proxy.GetHeight();

		// Loop through each pixel and apply HSV shift
		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				// Get the original pixel color
				Color originalColor = proxy.GetPixel(x, y);

				// Convert the color to HSV
				originalColor.ToHsv(out float h, out float s, out float v);

				// Apply shifts
				h = (h + HueShift) % 1.0f;
				s = Mathf.Clamp(s + SaturationShift, 0.0f, 1.0f);
				v = Mathf.Clamp(v + ValueShift, 0.0f, 1.0f);

				// Convert back to RGB
				Color shiftedColor = Color.FromHsv(h, s, v);

				// Set the new color to the pixel
				proxy.SetPixel(x, y, shiftedColor);
			}
		}

		return ImageTexture.CreateFromImage(proxy);
	}

	public static Texture2D ShiftHue(Image Source, float HueShift){
		Image proxy = new Image();
		proxy.CopyFrom(Source);

		// Get the size of the image
		int width = proxy.GetWidth();
		int height = proxy.GetHeight();

		// Loop through each pixel and apply hue shift
		for (int x = 0; x < width; x++){
			for (int y = 0; y < height; y++){
				// Get the original pixel color
				Color originalColor = proxy.GetPixel(x, y);

				// Convert the color to HSL
				originalColor.ToHsv(out float h, out float s, out float v);

				// Shift the hue
				h = (h + HueShift) % 1.0f;

				// Convert back to RGB
				Color shiftedColor = Color.FromHsv(h, s, v);

				// Set the new color to the pixel
				proxy.SetPixel(x, y, shiftedColor);
			}
		}

		return ImageTexture.CreateFromImage(proxy);
	}
}

public enum RotateImgSpec{
	None = 0,
	Right90 = 1,
	Left90 = 2,
	Rotate180 = 4,
}

public enum ClipImgSpecs{
	VerticalFromTop = 1,
	VerticalFromBottom = 2,
	HorizontalFromLeft = 3,
	HorizontalFromRight = 4,
}