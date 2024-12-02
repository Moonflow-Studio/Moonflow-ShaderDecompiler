using UnityEngine;

namespace Moonflow
{
    public class HLSLAnalyzer : IResourceReceiver
    {
        
        
        public void AddResource(string path)
        {
            Debug.Log($"Adding resource as HLSLFile: {path}");
            //get file name from path
            string fileName = System.IO.Path.GetFileName(path);
            //get file name without extension
            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            //get extension of file
            string extension = System.IO.Path.GetExtension(path);
            //check if file is a .txt file
            if (extension == ".txt")
            {
                string[] nameArray = fileName.Split('_');
                if (nameArray.Length != 2)
                {
                    Debug.LogError($"HLSL File Name is not in the correct format. Expected format: passName_output.txt, Path:{path}");
                }
                else
                {
                    if (nameArray[0] == "vs")
                    {
                        
                    }else if (nameArray[0] == "ps")
                    {
                        
                    }
                }
            }
        }
    }
}