using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace DiehardMasterDisaster.ScoreScreen;

public class CustomFont : IDisposable
{
	public const string DukeNukem3DFont1x = "DukeNukem3DFont1x";
	public const string DukeNukem3DFont2x = "DukeNukem3DFont2x";
	public const string DukeNukem3DFont3x = "DukeNukem3DFont3x";
	public const string DukeNukem3DFont4x = "DukeNukem3DFont4x";

	public static string OverrideFont;

	public CustomFont(string name)
	{
		OverrideFont = name;
	}
	
	public void Dispose()
	{
		OverrideFont = null;
	}

	public static void Load()
    {
	    try
	    {
		    On.FFont.LoadAndParseConfigFile += FFont_LoadAndParseConfigFile;

		    for (var i = 1; i <= 4; i++)
		    {
			    Futile.atlasManager.LoadAtlas($"fonts/DukeNukem3DFont{i}xAtlas");
			    Futile.atlasManager.LoadFont($"DukeNukem3DFont{i}x", $"DukeNukem3DFont{i}xAtlas", $"fonts/DukeNukem3DFont{i}x", 0, 0);
		    }
	    }
	    finally
	    {
		    On.FFont.LoadAndParseConfigFile -= FFont_LoadAndParseConfigFile;
	    }

        On.RWCustom.Custom.GetFont += Custom_GetFont;
    }

	private static string Custom_GetFont(On.RWCustom.Custom.orig_GetFont orig)
	{
		var result = orig();
		return string.IsNullOrEmpty(OverrideFont) ? result : OverrideFont;
	}

	private static void FFont_LoadAndParseConfigFile(On.FFont.orig_LoadAndParseConfigFile orig, FFont self, float fontScale)
    {
		var textFile = File.ReadAllText(AssetManager.ResolveFilePath(self._configPath + ".txt"));
		string[] array = new string[1] { "\n" };
		string[] array2 = textFile.Split(array, StringSplitOptions.RemoveEmptyEntries);
		if (array2.Length <= 1)
		{
			array[0] = "\r\n";
			array2 = textFile.Split(array, StringSplitOptions.RemoveEmptyEntries);
		}
		if (array2.Length <= 1)
		{
			array[0] = "\r";
			array2 = textFile.Split(array, StringSplitOptions.RemoveEmptyEntries);
		}
		if (array2.Length <= 1)
		{
			throw new FutileException("Your font file is messed up");
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		self._charInfosByID = new Dictionary<uint, FCharInfo>(127);
		FCharInfo value = new FCharInfo();
		self._charInfosByID[0u] = value;
		float resourceScaleInverse = Futile.resourceScaleInverse;
		Vector2 textureSize = self._element.atlas.textureSize;
		bool flag = false;
		int num4 = array2.Length;
		for (int i = 0; i < num4; i++)
		{
			string[] array3 = array2[i].Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (array3[0] == "common")
			{
				self._configWidth = int.Parse(array3[3].Split(new char[1] { '=' })[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				self._configRatio = self._element.sourcePixelSize.x / (float)self._configWidth;
				self._lineHeight = (float)int.Parse(array3[1].Split(new char[1] { '=' })[1], NumberStyles.Any, CultureInfo.InvariantCulture) * self._configRatio * resourceScaleInverse;
			}
			else if (array3[0] == "chars")
			{
				int num5 = int.Parse(array3[1].Split(new char[1] { '=' })[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				self._charInfos = new FCharInfo[num5 + 1];
			}
			else if (array3[0] == "char")
			{
				FCharInfo fCharInfo = new FCharInfo();
				num = array3.Length;
				for (int j = 1; j < num; j++)
				{
					string[] array4 = array3[j].Split(new char[1] { '=' });
					string text = array4[0];
					if (text == "letter")
					{
						if (array4[1].Length >= 3)
						{
							fCharInfo.letter = array4[1].Substring(1, 1);
						}
					}
					else if (!(text == "\r"))
					{
						int num6 = int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						float num7 = num6;
						switch (text)
						{
						case "id":
							fCharInfo.charID = num6;
							break;
						case "x":
							fCharInfo.x = num7 * self._configRatio - self._element.sourceRect.x * Futile.resourceScale;
							break;
						case "y":
							fCharInfo.y = num7 * self._configRatio - self._element.sourceRect.y * Futile.resourceScale;
							break;
						case "width":
							fCharInfo.width = num7 * self._configRatio;
							break;
						case "height":
							fCharInfo.height = num7 * self._configRatio;
							break;
						case "xoffset":
							fCharInfo.offsetX = num7 * self._configRatio;
							break;
						case "yoffset":
							fCharInfo.offsetY = num7 * self._configRatio;
							break;
						case "xadvance":
							fCharInfo.xadvance = num7 * self._configRatio;
							break;
						case "page":
							fCharInfo.page = num6;
							break;
						}
					}
				}
				Rect rect = (fCharInfo.uvRect = new Rect(self._element.uvRect.x + fCharInfo.x / textureSize.x, (textureSize.y - fCharInfo.y - fCharInfo.height) / textureSize.y - (1f - self._element.uvRect.yMax), fCharInfo.width / textureSize.x, fCharInfo.height / textureSize.y));
				fCharInfo.uvTopLeft.Set(rect.xMin, rect.yMax);
				fCharInfo.uvTopRight.Set(rect.xMax, rect.yMax);
				fCharInfo.uvBottomRight.Set(rect.xMax, rect.yMin);
				fCharInfo.uvBottomLeft.Set(rect.xMin, rect.yMin);
				fCharInfo.width *= resourceScaleInverse * fontScale;
				fCharInfo.height *= resourceScaleInverse * fontScale;
				fCharInfo.offsetX *= resourceScaleInverse * fontScale;
				fCharInfo.offsetY *= resourceScaleInverse * fontScale;
				fCharInfo.xadvance *= resourceScaleInverse * fontScale;
				self._charInfosByID[(uint)fCharInfo.charID] = fCharInfo;
				self._charInfos[num2] = fCharInfo;
				num2++;
			}
			else if (array3[0] == "kernings")
			{
				flag = true;
				int num8 = int.Parse(array3[1].Split(new char[1] { '=' })[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				self._kerningInfos = new FKerningInfo[num8 + 100];
			}
			else
			{
				if (!(array3[0] == "kerning"))
				{
					continue;
				}
				FKerningInfo fKerningInfo = new FKerningInfo();
				fKerningInfo.first = -1;
				num = array3.Length;
				for (int k = 1; k < num; k++)
				{
					string[] array5 = array3[k].Split(new char[1] { '=' });
					if (array5.Length >= 2)
					{
						string text2 = array5[0];
						int num9 = int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture);
						switch (text2)
						{
						case "first":
							fKerningInfo.first = num9;
							break;
						case "second":
							fKerningInfo.second = num9;
							break;
						case "amount":
							fKerningInfo.amount = (float)num9 * self._configRatio * resourceScaleInverse;
							break;
						}
					}
				}
				if (fKerningInfo.first != -1)
				{
					self._kerningInfos[num3] = fKerningInfo;
				}
				num3++;
			}
		}
		self._kerningCount = num3;
		if (!flag)
		{
			self._kerningInfos = new FKerningInfo[0];
		}
		if (self._charInfosByID.ContainsKey(32u))
		{
			self._charInfosByID[32u].offsetX = 0f;
			self._charInfosByID[32u].offsetY = 0f;
		}
		for (int l = 0; l < self._charInfos.Length; l++)
		{
			if (self._charInfos[l] != null && self._charInfos[l].width > self._maxCharWidth)
			{
				self._maxCharWidth = self._charInfos[l].width;
			}
		}
    }
}