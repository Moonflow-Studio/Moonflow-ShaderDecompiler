using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Moonflow
{
    public class TextureAnalyzer : IResourceReceiver
    {
        public List<TextureDeclaration> textureDeclarations = new List<TextureDeclaration>();
        [Serializable]
        public struct TextureDeclaration
        {
            public int setIndex;
            public int bindingIndex;
            public string resourceIndex;
            public ShaderPassDef passDef;
            public string linkedFile;
            public Texture2D texture;
        }
        public void AddResource(string path)
        {
            Debug.Log($"Adding resource as Texture File: {path}");
            //get file name from path
            string fileName = System.IO.Path.GetFileName(path);
            //get file name without extension
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            //get extension of file
            string extension = System.IO.Path.GetExtension(path);
            if (extension == ".png")
            {
                string[] nameArray = fileName.Split('_');
                //name looks like {shaderpass}_s{setIndex}_b{bindingIndex}_res{resourceIndex}_{textureName}.png
                if (nameArray.Length == 5)
                {
                    ShaderPassDef spd;
                    if (nameArray[0] == "Pixel")
                    {
                        spd = ShaderPassDef.Pixel;
                    }else if (nameArray[0] == "Vertex")
                    {
                        spd = ShaderPassDef.Vertex;
                    }
                    else
                    {
                        Debug.LogError($"Texture Analyzer: Unsupported shader pass type: {nameArray[0]}, Path: {path}");
                        return;
                    }
                    int setIndex = int.Parse(nameArray[1].Substring(1));
                    int bindingIndex = int.Parse(nameArray[2].Substring(1));
                    string resourceIndex = nameArray[3].Substring(3);
                    string relativePath = path.Substring(Application.dataPath.Length + 1);
                    TextureDeclaration textureDeclaration = new TextureDeclaration
                    {
                        passDef = spd,
                        setIndex = setIndex,
                        bindingIndex = bindingIndex,
                        resourceIndex = resourceIndex,
                        texture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/"+relativePath),
                        linkedFile = path
                    };
                    //load texture
                    textureDeclarations.Add(textureDeclaration);
                }
            }
            else
            {
                Debug.LogError($"Texture Analyzer: Unsupported file type: {extension}, Path: {path}");
            }
        }
    }
}