using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Dia
{
    public class GUIContentCreator
    {

        /// <summary>
        /// Node Names
        /// Add a new value when creating a new node type
        /// </summary>
        public const String MovieDialog = "Movie Dialog",
            MovieOptionDialog = "Movie Option Dialog",
            StandardDialog = "Standard Dialog",
            StandardOptionDialog = "Standard Option Dialog",
            TemplateDialog = "TEMPLATE";

        public static Vector2 StandardDialogNodeSize = new Vector2(250, 280);
        public static Vector2 OptionDialogNodeSize = new Vector2(250, 315);
        public static Vector2 MovieOptionNodeSize = new Vector2(250, 315);
        public static Vector2 MovieNodeSize = new Vector2(250, 280);

        //Template Value
        public static Vector2 TemplateDialogNodeSize = new Vector2(0, 0);

        private static GUIStyle BoxStyles = new GUIStyle();
        public static float ExtraBoxSize = 32.7f;

        private static GUIStyle TextFieldStyles;
        public static Color InputColor = Color.yellow;
        public static Color OutputColor = new Color(1, 153f / 255, 0);

        /// <summary>
        /// Create an EditorGUILayout LabelField, with text color
        /// </summary>
        /// <param name="label">Label</param>
        /// <param name="textColor">Text Color</param>
        public static void LabelField(String label, Color textColor)
        {
            TextFieldStyles = new GUIStyle(EditorStyles.textField);
            TextFieldStyles.normal.textColor = textColor;
            GUI.backgroundColor = Color.clear;
            EditorGUILayout.LabelField(label, TextFieldStyles);
            GUI.backgroundColor = Color.white;
            TextFieldStyles.normal.textColor = Color.black;
        }

        /// <summary>
        /// Create EditorGUILayout IntField with colored label color.
        /// </summary>
        /// <param name="value">Current field value</param>
        /// <param name="label">Label text</param>
        /// <param name="textColor">Label color</param>
        /// <returns>Field Value</returns>
        public static int IntField(int value, String label, Color textColor)
        {
            //Label Color
            EditorStyles.label.normal.textColor = textColor;
            var text = EditorGUILayout.IntField(label, value, GUILayout.Width(190));
            EditorStyles.label.normal.textColor = Color.black;
            return text;
        }

        /// <summary>
        /// Create EditorGUILayout IntField with colored label color.
        /// </summary>
        /// <param name="value">Current field value</param>
        /// <param name="label">Label text</param>
        /// <param name="textColor">Label color</param>
        /// <returns>Field Value</returns>
        public static String TextField(string value, String label, Color textColor)
        {
            //Label Color
            EditorStyles.label.normal.textColor = textColor;
            var text = EditorGUILayout.TextField(label, value, GUILayout.Width(190));
            EditorStyles.label.normal.textColor = Color.black;
            return text;
        }

        /// <summary>
        /// Create a colored box
        /// </summary>
        /// <param name="bounds">Size and position of box</param>
        /// <param name="backgroundColor">Color of box</param>
        public static void BoxField(Rect bounds, Color backgroundColor)
        {
            BoxStyles.border = new RectOffset(0, 0, 0, 0);
            BoxStyles.normal.background = MakeTex(1, 1, backgroundColor);
            Color temp = GUI.backgroundColor;
            GUI.Box(bounds, "", BoxStyles);
            GUI.backgroundColor = temp;
        }

        /// <summary>
        /// Create a GUIStyle with a background color of <color>
        /// </summary>
        /// <param name="color">Background color</param>
        /// <returns></returns>
        public static GUIStyle MakeStyle(Color color)
        {
            var style = new GUIStyle { normal = { background = MakeTex(1, 1, color) } };
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.MiddleCenter;
            return style;
        }

        /// <summary>
        /// Create a solid color texture. Minimal size = (1,1)
        /// </summary>
        /// <param name="width">Width of texture</param>
        /// <param name="height">Height of texture</param>
        /// <param name="col">Color of texture</param>
        /// <returns></returns>
        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        
        /// <summary>
        /// Create a tiled texture inside the gui. 
        /// </summary>
        /// <param name="fileName">Name of the image file, include file extension. Image location: Assets/dialogeditor/Resources</param>
        /// <param name="imageSize">The size of the image</param>
        /// <param name="tile">The size of the tile</param>
        /// <param name="areaToFill">The area to be filled with tiles</param>
        /// <param name="scaleMode">Scale mode for the texture</param>
        public static void TileTexture(String fileName, Vector2 imageSize,Rect tile, Rect areaToFill, ScaleMode scaleMode)
        {
            Texture2D background;
            if (LoadedTextures.TryGetValue(fileName, out background))
            {
                
            }
            else {
                background = new Texture2D((int)imageSize.x, (int)imageSize.y);
                background.wrapMode = TextureWrapMode.Repeat;
                var image = File.ReadAllBytes(Application.dataPath + "/Editor/dialogeditor/Resources/grid-01.png");
                background.LoadImage(image);
                LoadedTextures.Add(fileName, background);
            }
            for (float y = areaToFill.y; y < areaToFill.y + areaToFill.height; y = y + tile.height)
            {
                for (float x = areaToFill.x; x < areaToFill.x + areaToFill.width; x = x + tile.width)
                {
                    tile.x = x;
                    tile.y = y;
                    GUI.DrawTexture(tile, background, scaleMode);
                }
            }
        }

        public static Dictionary<string, Texture2D> LoadedTextures = new Dictionary<String,Texture2D>();

        /// <summary>
        /// Create texture from image
        /// </summary>
        /// <param name="fileName">Name of the image file, include file extension. Image location: Assets/dialogeditor/Resources</param>
        /// <param name="size">Size of the image</param>
        /// <returns>Texture2D containing the image</returns>
        public static Texture2D GetTextureFromFile(String fileName, Vector2 size)
        {
            Texture2D background;
            if (LoadedTextures.TryGetValue(fileName, out background))
            {
                return background;
            }
            background = new Texture2D((int) size.x, (int) size.y) {wrapMode = TextureWrapMode.Clamp};
            var image = File.ReadAllBytes(Application.dataPath + "/Editor/dialogeditor/Resources/"+fileName);
            background.LoadImage(image);
            LoadedTextures.Add(fileName,background);
            return background;
        }
    } 
}