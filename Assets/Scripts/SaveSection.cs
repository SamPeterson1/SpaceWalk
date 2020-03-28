using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSection
{
    public byte identifier;

    public void Write(BinaryWriter writer)
    {
        writer.Write(identifier);
        WriteData(writer);
    }

    public void Read(BinaryReader reader)
    {
        ReadData(reader);
    }

    protected virtual void WriteData(BinaryWriter writer) { }

    protected virtual void ReadData(BinaryReader reader) { }

}
