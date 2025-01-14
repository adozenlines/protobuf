#region Copyright notice and license
// Protocol Buffers - Google's data interchange format
// Copyright 2008 Google Inc.  All rights reserved.
// https://developers.google.com/protocol-buffers/
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of Google Inc. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
#endregion

using System.Linq;
using Google.Protobuf.TestProtos;
using NUnit.Framework;
using UnitTest.Issues.TestProtos;

namespace Google.Protobuf.Reflection
{
    /// <summary>
    /// Tests for descriptors. (Not in its own namespace or broken up into individual classes as the
    /// size doesn't warrant it. On the other hand, this makes me feel a bit dirty...)
    /// </summary>
    public class DescriptorsTest
    {
        [Test]
        public void FileDescriptor()
        {
            FileDescriptor file = UnittestProto3.Descriptor;

            Assert.AreEqual("google/protobuf/unittest_proto3.proto", file.Name);
            Assert.AreEqual("protobuf_unittest", file.Package);

            Assert.AreEqual("UnittestProto", file.Proto.Options.JavaOuterClassname);
            Assert.AreEqual("google/protobuf/unittest_proto3.proto", file.Proto.Name);

            // unittest.proto doesn't have any public imports, but unittest_import.proto does.
            Assert.AreEqual(0, file.PublicDependencies.Count);
            Assert.AreEqual(1, UnittestImportProto3.Descriptor.PublicDependencies.Count);
            Assert.AreEqual(UnittestImportPublicProto3.Descriptor, UnittestImportProto3.Descriptor.PublicDependencies[0]);

            Assert.AreEqual(1, file.Dependencies.Count);
            Assert.AreEqual(UnittestImportProto3.Descriptor, file.Dependencies[0]);

            MessageDescriptor messageType = TestAllTypes.Descriptor;
            Assert.AreSame(typeof(TestAllTypes), messageType.GeneratedType);
            Assert.AreEqual(messageType, file.MessageTypes[0]);
            Assert.AreEqual(messageType, file.FindTypeByName<MessageDescriptor>("TestAllTypes"));
            Assert.Null(file.FindTypeByName<MessageDescriptor>("NoSuchType"));
            Assert.Null(file.FindTypeByName<MessageDescriptor>("protobuf_unittest.TestAllTypes"));
            for (int i = 0; i < file.MessageTypes.Count; i++)
            {
                Assert.AreEqual(i, file.MessageTypes[i].Index);
            }

            Assert.AreEqual(file.EnumTypes[0], file.FindTypeByName<EnumDescriptor>("ForeignEnum"));
            Assert.Null(file.FindTypeByName<EnumDescriptor>("NoSuchType"));
            Assert.Null(file.FindTypeByName<EnumDescriptor>("protobuf_unittest.ForeignEnum"));
            Assert.AreEqual(1, UnittestImportProto3.Descriptor.EnumTypes.Count);
            Assert.AreEqual("ImportEnum", UnittestImportProto3.Descriptor.EnumTypes[0].Name);
            for (int i = 0; i < file.EnumTypes.Count; i++)
            {
                Assert.AreEqual(i, file.EnumTypes[i].Index);
            }

            Assert.AreEqual(10, file.SerializedData[0]);
        }

        [Test]
        public void MessageDescriptor()
        {
            MessageDescriptor messageType = TestAllTypes.Descriptor;
            MessageDescriptor nestedType = TestAllTypes.Types.NestedMessage.Descriptor;

            Assert.AreEqual("TestAllTypes", messageType.Name);
            Assert.AreEqual("protobuf_unittest.TestAllTypes", messageType.FullName);
            Assert.AreEqual(UnittestProto3.Descriptor, messageType.File);
            Assert.IsNull(messageType.ContainingType);
            Assert.IsNull(messageType.Proto.Options);

            Assert.AreEqual("TestAllTypes", messageType.Name);

            Assert.AreEqual("NestedMessage", nestedType.Name);
            Assert.AreEqual("protobuf_unittest.TestAllTypes.NestedMessage", nestedType.FullName);
            Assert.AreEqual(UnittestProto3.Descriptor, nestedType.File);
            Assert.AreEqual(messageType, nestedType.ContainingType);

            FieldDescriptor field = messageType.Fields.InDeclarationOrder()[0];
            Assert.AreEqual("single_int32", field.Name);
            Assert.AreEqual(field, messageType.FindDescriptor<FieldDescriptor>("single_int32"));
            Assert.Null(messageType.FindDescriptor<FieldDescriptor>("no_such_field"));
            Assert.AreEqual(field, messageType.FindFieldByNumber(1));
            Assert.Null(messageType.FindFieldByNumber(571283));
            var fieldsInDeclarationOrder = messageType.Fields.InDeclarationOrder();
            for (int i = 0; i < fieldsInDeclarationOrder.Count; i++)
            {
                Assert.AreEqual(i, fieldsInDeclarationOrder[i].Index);
            }

            Assert.AreEqual(nestedType, messageType.NestedTypes[0]);
            Assert.AreEqual(nestedType, messageType.FindDescriptor<MessageDescriptor>("NestedMessage"));
            Assert.Null(messageType.FindDescriptor<MessageDescriptor>("NoSuchType"));
            for (int i = 0; i < messageType.NestedTypes.Count; i++)
            {
                Assert.AreEqual(i, messageType.NestedTypes[i].Index);
            }

            Assert.AreEqual(messageType.EnumTypes[0], messageType.FindDescriptor<EnumDescriptor>("NestedEnum"));
            Assert.Null(messageType.FindDescriptor<EnumDescriptor>("NoSuchType"));
            for (int i = 0; i < messageType.EnumTypes.Count; i++)
            {
                Assert.AreEqual(i, messageType.EnumTypes[i].Index);
            }
        }

        [Test]
        public void FieldDescriptor()
        {
            MessageDescriptor messageType = TestAllTypes.Descriptor;
            FieldDescriptor primitiveField = messageType.FindDescriptor<FieldDescriptor>("single_int32");
            FieldDescriptor enumField = messageType.FindDescriptor<FieldDescriptor>("single_nested_enum");
            FieldDescriptor messageField = messageType.FindDescriptor<FieldDescriptor>("single_foreign_message");

            Assert.AreEqual("single_int32", primitiveField.Name);
            Assert.AreEqual("protobuf_unittest.TestAllTypes.single_int32",
                            primitiveField.FullName);
            Assert.AreEqual(1, primitiveField.FieldNumber);
            Assert.AreEqual(messageType, primitiveField.ContainingType);
            Assert.AreEqual(UnittestProto3.Descriptor, primitiveField.File);
            Assert.AreEqual(FieldType.Int32, primitiveField.FieldType);
            Assert.IsNull(primitiveField.Proto.Options);
            
            Assert.AreEqual("single_nested_enum", enumField.Name);
            Assert.AreEqual(FieldType.Enum, enumField.FieldType);
            // Assert.AreEqual(TestAllTypes.Types.NestedEnum.DescriptorProtoFile, enumField.EnumType);

            Assert.AreEqual("single_foreign_message", messageField.Name);
            Assert.AreEqual(FieldType.Message, messageField.FieldType);
            Assert.AreEqual(ForeignMessage.Descriptor, messageField.MessageType);
        }

        [Test]
        public void FieldDescriptorLabel()
        {
            FieldDescriptor singleField =
                TestAllTypes.Descriptor.FindDescriptor<FieldDescriptor>("single_int32");
            FieldDescriptor repeatedField =
                TestAllTypes.Descriptor.FindDescriptor<FieldDescriptor>("repeated_int32");

            Assert.IsFalse(singleField.IsRepeated);
            Assert.IsTrue(repeatedField.IsRepeated);
        }

        [Test]
        public void EnumDescriptor()
        {
            // Note: this test is a bit different to the Java version because there's no static way of getting to the descriptor
            EnumDescriptor enumType = UnittestProto3.Descriptor.FindTypeByName<EnumDescriptor>("ForeignEnum");
            EnumDescriptor nestedType = TestAllTypes.Descriptor.FindDescriptor<EnumDescriptor>("NestedEnum");

            Assert.AreEqual("ForeignEnum", enumType.Name);
            Assert.AreEqual("protobuf_unittest.ForeignEnum", enumType.FullName);
            Assert.AreEqual(UnittestProto3.Descriptor, enumType.File);
            Assert.Null(enumType.ContainingType);
            Assert.Null(enumType.Proto.Options);

            Assert.AreEqual("NestedEnum", nestedType.Name);
            Assert.AreEqual("protobuf_unittest.TestAllTypes.NestedEnum",
                            nestedType.FullName);
            Assert.AreEqual(UnittestProto3.Descriptor, nestedType.File);
            Assert.AreEqual(TestAllTypes.Descriptor, nestedType.ContainingType);

            EnumValueDescriptor value = enumType.FindValueByName("FOREIGN_FOO");
            Assert.AreEqual(value, enumType.Values[1]);
            Assert.AreEqual("FOREIGN_FOO", value.Name);
            Assert.AreEqual(4, value.Number);
            Assert.AreEqual((int) ForeignEnum.FOREIGN_FOO, value.Number);
            Assert.AreEqual(value, enumType.FindValueByNumber(4));
            Assert.Null(enumType.FindValueByName("NO_SUCH_VALUE"));
            for (int i = 0; i < enumType.Values.Count; i++)
            {
                Assert.AreEqual(i, enumType.Values[i].Index);
            }
        }

        [Test]
        public void OneofDescriptor()
        {
            OneofDescriptor descriptor = TestAllTypes.Descriptor.FindDescriptor<OneofDescriptor>("oneof_field");
            Assert.AreEqual("oneof_field", descriptor.Name);
            Assert.AreEqual("protobuf_unittest.TestAllTypes.oneof_field", descriptor.FullName);

            var expectedFields = new[] {
                TestAllTypes.OneofBytesFieldNumber,
                TestAllTypes.OneofNestedMessageFieldNumber,
                TestAllTypes.OneofStringFieldNumber,
                TestAllTypes.OneofUint32FieldNumber }
                .Select(fieldNumber => TestAllTypes.Descriptor.FindFieldByNumber(fieldNumber))
                .ToList();
            foreach (var field in expectedFields)
            {
                Assert.AreSame(descriptor, field.ContainingOneof);
            }

            CollectionAssert.AreEquivalent(expectedFields, descriptor.Fields);
        }

        [Test]
        public void ConstructionWithoutGeneratedCodeInfo()
        {
            var data = UnittestIssues.Descriptor.Proto.ToByteArray();
            var newDescriptor = Google.Protobuf.Reflection.FileDescriptor.InternalBuildGeneratedFileFrom(data, new Reflection.FileDescriptor[] { }, null);

            // We should still be able to get at a field...
            var messageDescriptor = newDescriptor.FindTypeByName<MessageDescriptor>("ItemField");
            var fieldDescriptor = messageDescriptor.FindFieldByName("item");
            // But there shouldn't be an accessor (or a generated type for the message)
            Assert.IsNull(fieldDescriptor.Accessor);
            Assert.IsNull(messageDescriptor.GeneratedType);
        }
    }
}
