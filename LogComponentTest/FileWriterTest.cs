using LogComponent;
using System;
using System.IO;
using Xunit;

namespace LogComponentTest
{
    public class FileWriterTest
    {
        [Fact]
        public void Constructor_CreateFolderIfNotExistent()
        {
            var tempPath = Path.Join(Path.GetTempPath(), "Constructor_CreateFolderIfNotExistent");

            Assert.False(Directory.Exists(tempPath));
            new FileWriter(tempPath, "test");

            Assert.True(Directory.Exists(tempPath));

            Directory.Delete(tempPath, true);
        }

        [Fact]
        public void Constructor_CreateFileWithPrefix()
        {
            var tempPath = Path.Join(Path.GetTempPath(), "Constructor_CreateFileWithPrefix");
            const string prefix = "specific_prefix";
            var fr = new FileWriter(tempPath, prefix);

            fr.Write("test");

            string[] fileEntries = Directory.GetFiles(tempPath);
            Assert.Single(fileEntries);
            Assert.Contains(prefix, fileEntries[0]);

            fr.Dispose();
            Directory.Delete(tempPath, true);
        }

        [Fact]
        public void Write_OneLine()
        {
            var tempPath = Path.Join(Path.GetTempPath(), "Write_OneLine");
            var fr = new FileWriter(tempPath, "test");
            const string log = "test";

            fr.Write(log);
            fr.Flush();
            fr.Dispose();

            var text = File.ReadAllText(Directory.GetFiles(tempPath)[0]);

            Assert.Equal(log, text);

            Directory.Delete(tempPath, true);
        }

        [Fact]
        public void Write_Header()
        {
            var tempPath = Path.Join(Path.GetTempPath(), "Write_Header");
            var fr = new FileWriter(tempPath, "test")
            {
                Header = "awesome header"
            };
            const string log = "log this";

            fr.Write(log);
            fr.Flush();
            fr.Dispose();

            var text = File.ReadAllText(Directory.GetFiles(tempPath)[0]);

            Assert.Equal(2, text.Split(Environment.NewLine).Length);
            Assert.Contains(fr.Header, text);
            Assert.Contains(log, text);

            Directory.Delete(tempPath, true);
        }

        [Fact]
        public void Write_WhenFileExists()
        {
            var tempPath = Path.Join(Path.GetTempPath(), "Write_WhenFileExists");
            const string FilePrefix = "test";
            Directory.CreateDirectory(tempPath);
            // this should have the same file as "created" by fileWriter
            var fileName = Path.Combine(tempPath, $"{FilePrefix}-{DateTime.Now:yyyyMMdd}.log");
            File.Create(fileName).Dispose();
            Assert.True(File.Exists(fileName));
            var fr = new FileWriter(tempPath, FilePrefix);
            const string log = "test";

            fr.Write(log);
            fr.Flush();
            fr.Dispose();

            var text = File.ReadAllText(Directory.GetFiles(tempPath)[0]);
            Assert.Equal(log, text);

            Directory.Delete(tempPath, true);
        }

        [Fact]
        public void Write_DayChange_CreateNewFile()
        {
            var tempPath = Path.Join(Path.GetTempPath(), "Write_DayChange_CreateNewFile");
            var fr = new FileWriter(tempPath, "test");
            const string log = "log this";
            fr.Write(log);
            fr.Flush();
            string[] fileEntries = Directory.GetFiles(tempPath);
            Assert.Single(fileEntries);
            fr.SetNowProvider(() => DateTime.Now.AddDays(1));

            fr.Write(log);
            fr.Flush();

            fileEntries = Directory.GetFiles(tempPath);
            Assert.Equal(2, fileEntries.Length);

            fr.Dispose();
            Directory.Delete(tempPath, true);
        }
    }
}
