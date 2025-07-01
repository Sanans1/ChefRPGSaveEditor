namespace ChefRPGSaveEditor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello!");

            Console.WriteLine("Finding your save folder...");

            var folder = @"c:\users\" + Environment.UserName + @"\appdata\LocalLow\World 2 Studio Inc\Chef RPG\FullData\";

            Console.WriteLine("Gathering your characters...");

            var directoryInfo = new DirectoryInfo(folder);
            var saveSlots = new Dictionary<int, DirectoryInfo>();
            var index = 1;

            foreach (var subFolder in directoryInfo.GetDirectories())
            {
                var dataFile = subFolder.FullName + @"\PlayerData.dat";

                if (!File.Exists(dataFile)) continue;
            
                var rawPlayerSaveData = ReadFromBinaryFile(dataFile);
                var playerSaveData = Convert.ChangeType(rawPlayerSaveData, typeof(PlayerSaveData));

                saveSlots.Add(index, subFolder);

                var fileDate = File.GetLastWriteTime(dataFile);

                Console.WriteLine($"{index}) {playerSaveData.GetType().GetField("PlayerName").GetValue(playerSaveData)} - {fileDate.ToString()}");

                index++;
            }

            Console.WriteLine("Please type the number of the character you want to edit:");

            var number = int.Parse(Console.ReadLine());

            Console.WriteLine("Getting the character's data...");

            var playerStatsFilePath = saveSlots[number].FullName;

            var subDirectoryInfo = new DirectoryInfo(playerStatsFilePath);

            index = 1;

            var characterDataFiles = new Dictionary<int, FileInfo>(); 

            foreach (var file in subDirectoryInfo.GetFiles())
            {
                characterDataFiles.Add(index, file);
                Console.WriteLine($"{index}) {file.Name}");
                index++;
            }

            Console.WriteLine("Please type the number of the file you want to edit:");

            number = int.Parse(Console.ReadLine());

            if (!File.Exists(characterDataFiles[number].FullName))
            {
                Console.WriteLine("Can't find that file!");
                throw new Exception();
            }

            var text = File.ReadAllText(characterDataFiles[number].FullName);

            var fileData = ReadFromBinaryFile(characterDataFiles[number].FullName);

            var stats = fileData.GetType().GetFields();

            var isEditing = true;

            while (isEditing)
            {
                int fieldIndex = 1;

                foreach (var stat in stats)
                {
                    var spaces = 40 - fieldIndex.ToString().Length + 1;
                    var paddedName = stat.Name.PadRight(spaces, ' ');
                    var propertyLine = $"{fieldIndex}) {paddedName}{stat.GetValue(fileData)}";
                    Console.WriteLine(propertyLine);
                    fieldIndex++;
                }

                Console.WriteLine("Please type the number of the stat you want to edit, or type S to save and finish:");

                var input = Console.ReadLine();

                if (input.ToLower() == "s")
                {
                    isEditing = false;

                    Console.WriteLine("Saving...");

                    WriteToBinaryFile(characterDataFiles[number].FullName, fileData);

                    Console.WriteLine("Finished!");
                }
                else
                {
                    Console.WriteLine("Please type the value you want the stat to be:");

                    var value = Console.ReadLine();

                    var parsedInput = int.Parse(input) - 1;
                    var convertedValue = Convert.ChangeType(value, stats[parsedInput].FieldType);

                    stats[parsedInput].SetValue(fileData, convertedValue);

                    Console.WriteLine("Set!");
                }
            }
        }
#pragma warning disable SYSLIB0011 // Type or member is obsolete
        public static void WriteToBinaryFile(string filePath, object objectToWrite)
        {
            using Stream serializationStream = File.Open(filePath, FileMode.Create);
            new BinaryFormatter().Serialize(serializationStream, objectToWrite);
        }

        public static object ReadFromBinaryFile(string filePath)
        {
            using Stream serializationStream = File.Open(filePath, FileMode.Open);
            return new BinaryFormatter().Deserialize(serializationStream);
        }
#pragma warning restore SYSLIB0011 // Type or member is obsolete
    }
}
