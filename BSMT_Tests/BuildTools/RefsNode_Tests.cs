﻿using BeatSaberModdingTools.BuildTools;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BSMT_Tests.BuildTools
{
    [TestClass]
    public class RefsNode_Tests
    {
        private readonly string DataPath = Path.Combine("Data", "BuildTools");
        private readonly string OutputPath = Path.Combine("Output", "BuildTools");

        [TestMethod]
        public void TryGetReference_Exists()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fileName = "Main.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            RefsNode testNode = rootNodes[0];
            Assert.IsTrue(testNode.TryGetReference(fileName, out FileNode fileNode));
            fileNode.Alias = "NewMain.dll";
            Console.WriteLine(string.Join("\n", testNode.GetLines()));
        }

        [TestMethod]
        public void TryGetReference_NonExistant()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fileName = "NonExistant.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            RefsNode testNode = rootNodes[0];
            int childCount = testNode.Count;
            testNode.Insert(childCount, new FileNode("TestEnd.dll"));
            testNode.Insert(0, new FileNode("TestStart.dll"));
            testNode.Insert(3, new FileNode("TestMiddle.dll"));
            Assert.IsFalse(testNode.TryGetReference(fileName, out FileNode fileNode));
            Assert.IsNull(fileNode);
        }

        [TestMethod]
        public void InsertChild()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNode = reader.ReadFile();
            RefsNode testNode = rootNode.First();
            int middleIndex = 3;
            RefsNode first = new FileNode("TestStart.dll");
            RefsNode middle = new FileNode("TestMiddle.dll");
            RefsNode last = new FileNode("TestEnd.dll");
            testNode.Insert(testNode.Count, last);
            testNode.Insert(0, first);
            testNode.Insert(middleIndex, middle);
            Assert.AreEqual(first, testNode[0]);
            Assert.AreEqual(middle, testNode[middleIndex]);
            Assert.AreEqual(last, testNode[testNode.Count - 1]);
        }

        [TestMethod]
        public void AddChild()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            RefsNode testNode = rootNodes[0];
            RefsNode last = new FileNode("TestEnd.dll");
            testNode.Add(last);
            Assert.AreEqual(last, testNode[testNode.Count - 1]);
        }

        [TestMethod]
        public void GetFilename()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fileName = "Main.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            RefsNode testNode = rootNodes[0];

            Assert.IsTrue(testNode.TryGetReference(fileName, out FileNode fileNode));

            Console.WriteLine(fileNode.GetRelativePath());
        }

        [TestMethod]
        public void GetRelativePath()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fileName = "Main.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            RefsNode testNode = rootNodes[0];
            Assert.IsTrue(testNode.TryGetReference(fileName, out FileNode fileNode));

            Console.WriteLine(fileNode.GetRelativePath());
        }

        [TestMethod]
        public void Find_File()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fileName = "UnityEngine.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            FileNode target = rootNodes.Find<FileNode>(f => f.GetFilename() == fileName);
            Assert.IsNotNull(target);
            Assert.AreEqual(fileName, target.GetFilename());
        }

        [TestMethod]
        public void Find_Leaf()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string relativePath = @"Beat Saber_Data/Managed";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            LeafNode target = rootNodes.Find<LeafNode>(l => l.GetRelativePath().Contains(relativePath));
            Assert.IsNotNull(target);
            Assert.IsTrue(target.GetRelativePath().Contains(relativePath));
        }
        [TestMethod]
        public void GetPathSource_MdInstall()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string alias = @"UnityEngine.UnityWebRequestModule.Net3.dll";
            string expectedPathSource = "./mdinstalldir.txt";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            LeafNode target = rootNodes.Find<FileNode>(f => f.Alias == alias);
            Assert.IsNotNull(target);
            Assert.AreEqual(expectedPathSource, target.GetPathSource());
        }

        [TestMethod]
        public void GetPathSource_BsInstall()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string name = @"UnityEngine.UIElementsModule.dll";
            string expectedPathSource = "./bsinstalldir.txt";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            LeafNode target = rootNodes.Find<FileNode>(f => f.GetFilename() == name);
            Assert.IsNotNull(target);
            Assert.AreEqual(expectedPathSource, target.GetPathSource());
        }

        [TestMethod]
        public void InOptionalBlock_True()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string alias = @"UnityEngine.UnityWebRequestModule.Net3.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            LeafNode target = rootNodes.Find<FileNode>(f => f.Alias == alias);
            Assert.IsNotNull(target);
            Assert.IsTrue(target.InOptionalBlock());
        }

        [TestMethod]
        public void InOptionalBlock_False()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string name = @"UnityEngine.UIElementsModule.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            LeafNode target = rootNodes.Find<FileNode>(f => f.GetFilename() == name);
            Assert.IsNotNull(target);
            Assert.IsFalse(target.InOptionalBlock());
        }

        [TestMethod]
        public void InsertReference_OptionalNoMatch()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fullName = @"Beat_Data/Managed/UnityEngine.InsertedReference.dll";
            FileEntry fileEntry = new FileEntry(fullName, FileFlag.Virtualize, null, "./mdinstalldir.txt");
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            Assert.IsTrue(rootNodes.InsertReference(fileEntry, true)); 
            Console.WriteLine("---------------");
            foreach (var line in rootNodes.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void InsertReference_NotOptional()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fullName = @"Beat Saber_Data/Managed/UnityEngine.InsertedReference.dll";
            FileEntry fileEntry = new FileEntry(fullName, FileFlag.Virtualize, null, "./bsinstalldir.txt");
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            Assert.IsTrue(rootNodes.InsertReference(fileEntry, false));
            Console.WriteLine("---------------");
            foreach (var line in rootNodes.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void InsertReference_NotOptional_NoFolder()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fullName = @"InsertedReference.dll";
            FileEntry fileEntry = new FileEntry(fullName, FileFlag.Virtualize, null, "./bsinstalldir.txt");
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            Assert.IsTrue(rootNodes.InsertReference(fileEntry, false));
            Console.WriteLine("---------------");
            foreach (var line in rootNodes.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void InsertReference_NotOptional_NoExistingLeafs()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fullName = @"Beat_Data/UnityEngine.InsertedReference.dll";
            FileEntry fileEntry = new FileEntry(fullName, FileFlag.Virtualize, null, "./bsinstalldir.txt");
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            Assert.IsTrue(rootNodes.InsertReference(fileEntry, false));
            Console.WriteLine("---------------");
            foreach (var line in rootNodes.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void InsertReference_NotOptional_OtherPathSource()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fullName = @"Beat Saber_Data/Managed/UnityEngine.InsertedReference.dll";
            FileEntry fileEntry = new FileEntry(fullName, FileFlag.Virtualize, null, "./secondthing.txt");
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            Assert.IsTrue(rootNodes.InsertReference(fileEntry, false));
            Console.WriteLine("---------------");
            foreach (var line in rootNodes.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void InsertReference_NotOptional_NewPathSource()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fullName = @"Beat Saber_Data/Managed/UnityEngine.InsertedReference.dll";
            FileEntry fileEntry = new FileEntry(fullName, FileFlag.Virtualize, null, "./newsource.txt");
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            Assert.IsTrue(rootNodes.InsertReference(fileEntry, false));
            Console.WriteLine("---------------");
            foreach (var line in rootNodes.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void InsertReference_Optional_NewPathSource()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fullName = @"Beat Saber_Data/Managed/UnityEngine.InsertedReference.dll";
            FileEntry fileEntry = new FileEntry(fullName, FileFlag.Virtualize, null, "./newsource.txt");
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            Assert.IsTrue(rootNodes.InsertReference(fileEntry, true));
            Console.WriteLine("---------------");
            foreach (var line in rootNodes.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void FindAll_Leaf()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string relativePath = @"Beat Saber_Data/Managed";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            LeafNode[] matches = rootNodes.FindAll<LeafNode>(l => l.GetRelativePath().Contains(relativePath) && l.NodeType == RefsNodesType.Leaf);
            Assert.IsNotNull(matches);

        }

        [TestMethod]
        public void Find_NoMatch()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fileName = "None.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            FileNode target = rootNodes.Find<FileNode>(f => f.GetFilename() == fileName);
            Assert.IsNull(target);
        }

        [TestMethod]
        public void GetFilenameAfterRelativePath()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string fileName = "Main.dll";
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNodes = reader.ReadFile();
            RefsNode testNode = rootNodes[0];
            Assert.IsTrue(testNode.TryGetReference(fileName, out FileNode fileNode));
            Console.WriteLine(fileNode.GetRelativePath());
            Assert.AreEqual(fileName, fileNode.GetFilename());
        }

        [TestMethod]
        public void GetLines()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode root = reader.ReadFile();
            Assert.IsTrue(root.Count > 0);
            foreach (var line in root.GetLines())
            {
                Console.WriteLine(line);
            }
        }

        [TestMethod]
        public void ReadFileAndCompare()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode things = reader.ReadFile();
            Assert.IsTrue(things.Count > 0);
            string text = string.Empty;
            string[] stringList = things.GetLines();

            text = string.Join("\n", stringList);
            //Assert.AreEqual(File.ReadAllText(refsText), text);
            string line;
            int lineNumber = 0;
            using (StreamReader streamReader = new StreamReader(refsText))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (lineNumber < stringList.Length)
                    {
                        Console.WriteLine(stringList[lineNumber]);
                        Assert.AreEqual(line, stringList[lineNumber]);
                    }
                    else
                        Assert.Fail("Different number of lines");
                    lineNumber++;
                }
            }
            Assert.AreEqual(lineNumber, stringList.Length);
        }

        [TestMethod]
        public void StreamCompare()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNode = reader.ReadFile();
            Assert.IsTrue(rootNode.Count > 0);
            string originalLine;
            string parsedLine;
            int index = 0;
            Directory.CreateDirectory(OutputPath);
            using (MemoryStream ms = new MemoryStream())
            using (StreamReader originalFile = new StreamReader(refsText))
            {
                rootNode.WriteToStream(ms);
                ms.Seek(0, SeekOrigin.Begin);
                using (StreamReader writtenFile = new StreamReader(ms))
                {
                    originalLine = originalFile.ReadLine();
                    parsedLine = writtenFile.ReadLine();
                    do
                    {
                        Console.WriteLine((++index).ToString("00") + "|" + originalLine ?? "<NULL>");
                        Console.WriteLine("   " + (parsedLine ?? "<NULL>"));
                        if (originalLine != parsedLine)
                        {
                            Assert.Fail($"{originalLine} != {parsedLine}");
                        }

                        originalLine = originalFile.ReadLine();
                        parsedLine = writtenFile.ReadLine();
                    } while (originalLine != null && parsedLine != null);
                    if (originalLine != null || parsedLine != null)
                        Assert.Fail("Lines aren't matching nulls");
                }
            }
        }

        [TestMethod]
        public void WriteToFile()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            string filePath = Path.GetFullPath(Path.Combine(OutputPath, "WriteToFile.txt"));
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode rootNode = reader.ReadFile();
            rootNode.WriteToFile(filePath);
            Assert.IsTrue(rootNode.Count > 0);
            string originalLine;
            string parsedLine;
            int index = 0;
            Directory.CreateDirectory(OutputPath);
            using (StreamReader writtenFile = new StreamReader(filePath))
            using (StreamReader originalFile = new StreamReader(refsText))
            {
                originalLine = originalFile.ReadLine();
                parsedLine = writtenFile.ReadLine();
                do
                {
                    Console.WriteLine((++index).ToString("00") + "|" + originalLine ?? "<NULL>");
                    Console.WriteLine("   " + (parsedLine ?? "<NULL>"));
                    if (originalLine != parsedLine)
                    {
                        Assert.Fail($"{originalLine} != {parsedLine}");
                    }

                    originalLine = originalFile.ReadLine();
                    parsedLine = writtenFile.ReadLine();
                } while (originalLine != null && parsedLine != null);
                if (originalLine != null || parsedLine != null)
                    Assert.Fail("Lines aren't matching nulls");
            }
        }

        [TestMethod]
        public void GetFileString()
        {
            string refsText = Path.GetFullPath(Path.Combine(DataPath, "refs.txt"));
            BuildToolsRefsParser reader = new BuildToolsRefsParser(refsText);
            Assert.IsTrue(reader.FileExists);
            RootNode things = reader.ReadFile();
            Assert.IsTrue(things.Count > 0);
            string[] parsedText = things.GetFileString().Split(new char[] { '\n' }, StringSplitOptions.None);
            string line;
            int lineNumber = 0;
            using (StreamReader streamReader = new StreamReader(refsText))
            {
                while ((line = streamReader.ReadLine()) != null)
                {
                    if (lineNumber < parsedText.Length)
                    {
                        Console.WriteLine(parsedText[lineNumber]);
                        Assert.AreEqual(line, parsedText[lineNumber]);
                    }
                    else
                        Assert.Fail("Different number of lines");
                    lineNumber++;
                }
            }
            //Assert.AreEqual(originalText, parsedText);
        }

        public string[] GetLines(RefsNode node)
        {
            List<string> lines = new List<string>();
            return GetLines(node, ref lines).ToArray();
        }

        private List<string> GetLines(RefsNode node, ref List<string> list)
        {
            string lineToAdd = node.RawLine;
            if (node is FileNode fileNode)
                lineToAdd = lineToAdd + " | " + fileNode.GetFilename(true);
            list.Add(lineToAdd);
            if (node.SupportsChildren)
            {
                foreach (RefsNode childNode in node.GetChildren())
                {
                    GetLines(childNode, ref list);
                }
            }
            return list;
        }

        public void PrintChildren(RefsNode node)
        {
            if (node is FileNode fileNode)
            {
                Console.WriteLine(node.NodeDepth.ToString("00") + " | " + node.RawLine + " | " + fileNode);
            }
            else
                Console.WriteLine(node.NodeDepth.ToString("00") + " | " + node.RawLine);
            if (!node.SupportsChildren)
                return;
            foreach (RefsNode childNode in node.GetChildren())
            {
                PrintChildren(childNode);
            }
        }
    }


}
