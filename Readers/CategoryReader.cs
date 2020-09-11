using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ArchivalTibiaV71MapEditor.Utilities;

namespace ArchivalTibiaV71MapEditor.Readers
{
    public class Category
    {
        public string Name { get; }
        private List<Range<ushort>> _ranges = new List<Range<ushort>>();

        public Category(string name)
        {
            Name = name;
        }

        public void Add(Range<ushort> range)
        {
            _ranges.Add(range);
        }

        public bool Contains(ushort id)
        {
            return _ranges.Any(r => r.InRange(id));
        }
    }

    public class CategoryReader
    {
        private StreamReader _reader;

        public CategoryReader(Stream fs)
        {
            _reader = new StreamReader(fs);
        }

        public List<Category> ReadToEnd()
        {
            var line = 0;
            try
            {
                var categories = new List<Category>(20);
                Category current = null;
                string read;
                while ((read = _reader.ReadLine()?.Trim()) != null)
                {
                    line++;
                    if (string.IsNullOrWhiteSpace(read)) continue; // skip empty lines
                    switch (read[0])
                    {
                        case '[':
                            if (current != null)
                                categories.Add(current);
                            current = new Category(read.Substring(1, read.IndexOf(']') - 1));
                            break;
                        case 'R':
                            if (current == null)
                                throw new InvalidOperationException(
                                    $"Range(start,end) came before [CategoryName] on line {line}.");
                            var startStart = read.IndexOf('(') + 1;
                            var endStart = read.IndexOf(',') + 1;

                            current.Add(new Range<ushort>(
                                ushort.Parse(read.Substring(startStart, endStart - 1 - startStart)),
                                ushort.Parse(read.Substring(endStart, read.IndexOf(')') - endStart))));
                            break;
                    }
                }

                if (current != null)
                    categories.Add(current);

                categories.TrimExcess();
                return categories;
            }
            catch (Exception ex) when (ex is ArgumentOutOfRangeException
                                       || ex is OverflowException
                                       || ex is FormatException
                                       || ex is ArgumentNullException)
            {
                throw new InvalidOperationException($"Exception caused by line {line} when reading categories file.");
            }
            finally
            {
                _reader.Dispose();
            }
        }
    }
}