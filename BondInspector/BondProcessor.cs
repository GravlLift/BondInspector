// <copyright file="BondProcessor.cs" company="Den Delimarsky">
// Developed by Den Delimarsky.
// Den Delimarsky licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// </copyright>

using Bond;
using Bond.Protocols;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace BondReader
{
    /// <summary>
    /// Class responsible for processing Bond files.
    /// </summary>
    internal class BondProcessor
    {
        int StructureIndent = -2;

        /// <summary>
        /// Initializes a new instance of the <see cref="BondProcessor"/> class, providing the ability to process a Bond file.
        /// </summary>
        /// <param name="version">Bond protocol version.</param>
        internal BondProcessor(ushort version)
        {
            this.Version = version;
            this.BondBuilder = new StringBuilder();
        }

        private ushort Version { get; set; }

        private StringBuilder BondBuilder { get; set; }

        /// <summary>
        /// Processes the Bond file and outputs data to an external file, if applicable.
        /// </summary>
        /// <param name="inputFilePath">Path to the input file to be processed.</param>
        /// <param name="outputFilePath">Path to output file to be processed.</param>
        internal string ProcessBytes(byte[] byteContent, bool iterativeDiscovery, int skip = 0)
        {
            this.Log($"Skipping bytes: {skip}");

            if (byteContent != null && byteContent.Length > 0)
            {
                if (!iterativeDiscovery)
                {
                    var inputBuffer = new Bond.IO.Unsafe.InputBuffer(byteContent);
                    var reader = new CompactBinaryReader<Bond.IO.Unsafe.InputBuffer>(
                        inputBuffer,
                        this.Version
                    );
                    try
                    {
                        this.ProcessData(reader);
                    }
                    catch (Exception e)
                    {
                        this.Log(e.ToString());
                    }
                }
                else
                {
                    for (int i = 0; i < byteContent.Length; i++)
                    {
                        this.Log(
                            $"╔{new string('═', 25)} INCREMENTAL DISCOVERY ITERATION {i} {new string('═', 25)}╗"
                        );
                        var inputBuffer = new Bond.IO.Unsafe.InputBuffer(
                            byteContent.Skip(i).ToArray()
                        );
                        var reader = new CompactBinaryReader<Bond.IO.Unsafe.InputBuffer>(
                            inputBuffer,
                            this.Version
                        );

                        try
                        {
                            this.ProcessData(reader);
                        }
                        catch
                        {
                            this.Log(
                                "Failed to process iteration due to wrong byte structure. This is likely not the start of the envelope."
                            );
                            StructureIndent = StructureIndent - 2;
                        }

                        this.Log(
                            $"╚{new string('═', 25)} END INCREMENTAL DISCOVERY ITERATION {i} {new string('═', 25)}╝"
                        );
                    }
                }
            }
            else
            {
                this.Log("No byte content to read.");
            }
            return this.BondBuilder.ToString();
        }

        private void ProcessData(CompactBinaryReader<Bond.IO.Unsafe.InputBuffer> reader)
        {
            this.ReadData(reader);
        }

        private void ReadData(CompactBinaryReader<Bond.IO.Unsafe.InputBuffer> reader)
        {
            StructureIndent = StructureIndent + 2;
            this.Log(
                $"{new string('\t', StructureIndent)}╔{new string('═', 25)} STR {new string('═', 25)}╗"
            );

            reader.ReadStructBegin();

            BondDataType dataType;

            do
            {
                reader.ReadFieldBegin(out dataType, out ushort fieldId);

                this.Log($"{new string('\t', StructureIndent)}Data type: ", true);
                this.Log($"{dataType, 15}\t", true);
                this.Log($"Field ID: ", true);
                this.Log($"{fieldId, 15}", true);
                this.Log(string.Empty);

                this.DecideOnDataType(reader, dataType);

                reader.ReadFieldEnd();
            } while (dataType != BondDataType.BT_STOP);

            reader.ReadStructEnd();

            this.Log(
                $"{new string('\t', StructureIndent)}╚{new string('═', 25)} STR {new string('═', 25)}╝"
            );

            StructureIndent = StructureIndent - 2;
        }

        private void ReadContainer(
            CompactBinaryReader<Bond.IO.Unsafe.InputBuffer> reader,
            bool isMap = false
        )
        {
            string marker = "Mapped value type: ";
            int containerCounter;
            BondDataType containerDataType = BondDataType.BT_UNAVAILABLE;
            BondDataType valueDataType = BondDataType.BT_UNAVAILABLE;

            if (!isMap)
            {
                reader.ReadContainerBegin(out containerCounter, out containerDataType);
            }
            else
            {
                reader.ReadContainerBegin(
                    out containerCounter,
                    out containerDataType,
                    out valueDataType
                );
            }

            this.Log($"{new string('\t', StructureIndent)}Container item type: ", true);
            this.Log($"{containerDataType, 15}", true);
            this.Log($"\tItems: ", true);
            this.Log(
                $"{containerCounter, 10}\t{(isMap ? (marker + valueDataType) : string.Empty), 10}",
                true
            );
            this.Log(string.Empty);

            this.Log(
                $"{new string('\t', StructureIndent)}╔{new string('═', 25)} CON {new string('═', 25)}╗"
            );

            if (containerCounter < 1000)
            {
                for (int i = 0; i < containerCounter; i++)
                {
                    this.Log($"{new string('\t', StructureIndent)}List item: " + i);
                    this.DecideOnDataType(reader, containerDataType);
                    if (isMap)
                    {
                        this.DecideOnDataType(reader, valueDataType);
                    }
                }
            }
            else
            {
                this.Log(
                    $"{new string('\t', StructureIndent)}Container way too big. Unlikely we're looking at the right structure."
                );
            }

            this.Log($"{new string('\t', StructureIndent)}Done reading container.");
            this.Log(
                $"{new string('\t', StructureIndent)}╚{new string('═', 25)} CON {new string('═', 25)}╝"
            );

            reader.ReadContainerEnd();
        }

        private void DecideOnDataType(
            CompactBinaryReader<Bond.IO.Unsafe.InputBuffer> reader,
            BondDataType dataType
        )
        {
            switch (dataType)
            {
                case BondDataType.BT_STRUCT:
                {
                    this.ReadData(reader);
                    break;
                }

                case BondDataType.BT_LIST:
                {
                    this.ReadContainer(reader);
                    break;
                }

                case BondDataType.BT_SET:
                {
                    this.ReadContainer(reader);
                    break;
                }

                case BondDataType.BT_MAP:
                {
                    this.ReadContainer(reader, true);
                    break;
                }

                case BondDataType.BT_STRING:
                {
                    var stringValue = reader.ReadString();
                    this.Log($"{new string('\t', StructureIndent)}" + stringValue);
                    break;
                }

                case BondDataType.BT_WSTRING:
                {
                    var stringValue = reader.ReadWString();
                    this.Log($"{new string('\t', StructureIndent)}" + stringValue);
                    break;
                }

                case BondDataType.BT_BOOL:
                {
                    var boolValue = reader.ReadBool();
                    this.Log($"{new string('\t', StructureIndent)}" + boolValue.ToString());
                    break;
                }

                case BondDataType.BT_DOUBLE:
                {
                    double doubleValue = reader.ReadDouble();
                    this.Log($"{new string('\t', StructureIndent)}" + doubleValue.ToString());
                    break;
                }

                case BondDataType.BT_FLOAT:
                {
                    double floatValue = reader.ReadFloat();
                    this.Log($"{new string('\t', StructureIndent)}" + floatValue.ToString());
                    break;
                }

                case BondDataType.BT_INT8:
                {
                    var int8value = reader.ReadInt8();
                    this.Log($"{new string('\t', StructureIndent)}" + int8value.ToString());
                    break;
                }

                case BondDataType.BT_INT16:
                {
                    var int16value = reader.ReadInt16();
                    this.Log($"{new string('\t', StructureIndent)}" + int16value.ToString());
                    break;
                }

                case BondDataType.BT_INT32:
                {
                    var int32Value = reader.ReadInt32();
                    this.Log($"{new string('\t', StructureIndent)}" + int32Value.ToString());
                    break;
                }

                case BondDataType.BT_INT64:
                {
                    var int64Value = reader.ReadInt64();
                    this.Log($"{new string('\t', StructureIndent)}" + int64Value.ToString());
                    break;
                }

                case BondDataType.BT_UINT8:
                {
                    var uint8value = reader.ReadUInt8();
                    this.Log($"{new string('\t', StructureIndent)}" + uint8value.ToString());
                    break;
                }

                case BondDataType.BT_UINT16:
                {
                    var uint16Value = reader.ReadUInt16();
                    this.Log($"{new string('\t', StructureIndent)}" + uint16Value.ToString());
                    break;
                }

                case BondDataType.BT_UINT32:
                {
                    var uint32Value = reader.ReadUInt32();
                    this.Log($"{new string('\t', StructureIndent)}" + uint32Value.ToString());
                    break;
                }

                case BondDataType.BT_UINT64:
                {
                    var uint64Value = reader.ReadUInt64();
                    this.Log($"{new string('\t', StructureIndent)}" + uint64Value.ToString());
                    break;
                }

                default:
                    if (dataType != BondDataType.BT_STOP && dataType != BondDataType.BT_STOP_BASE)
                    {
                        this.Log(
                            $"{new string('\t', StructureIndent)}Skipping datatype: {dataType, 10}"
                        );
                        reader.Skip(dataType);
                    }

                    break;
            }
        }

        private void Log(string value, bool inline = false)
        {
            if (inline)
            {
                Console.Write(value);
                this.BondBuilder.Append(value);
            }
            else
            {
                Console.WriteLine(value);
                this.BondBuilder.AppendLine(value);
            }
        }
    }
}
