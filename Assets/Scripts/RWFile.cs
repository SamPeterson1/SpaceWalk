using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RWFile
{
    string path;
    
    public RWFile(string path)
    {
        if(!File.Exists(path))
        {
            File.Create(path);
        }

        this.path = path;
    }

    public void AppendLine(string line)
    {
        File.AppendAllText(path, line);
    }

    public void AppendLines(string[] lines)
    {
        File.AppendAllLines(path, lines);
    }

    public string[] ReadAll()
    {
        return File.ReadAllLines(path);
    }

    public StreamWriter GetWriter()
    {
        return File.CreateText(path);
    }

    public StreamReader GetReader()
    {
        return File.OpenText(path);
    }
}
