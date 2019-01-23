using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Diagnostics;

public static class SerializeManager
{
    private static readonly string SaveFileExtension = ".sav";

    public static void Save<T>(T serializableData)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(GetPath<T>(), FileMode.Create);

        bf.Serialize(stream, serializableData);

        stream.Close();
    }

    public static T Load<T>() where T : new()
    {
        if (Exists<T>())
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(GetPath<T>(), FileMode.Open);

            T data = (T)bf.Deserialize(stream);
            stream.Close();

            return data;
        }
        else
        {
            return new T();
        }
    }

    public static bool Exists<T>()
    {
        return File.Exists(GetPath<T>()) ? true : false;
    }

    public static void Delete<T>()
    {
        if (Exists<T>())
        {
            File.Delete(GetPath<T>());
        }
    }

    public static string GetPath<T>()
    {
        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), typeof(T) + SaveFileExtension);
        Debug.WriteLine(path);
        return Path.Combine(path);
    }
}
