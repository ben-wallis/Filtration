using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Enums;
using NUnit.Framework;

namespace Filtration.ItemFilterPreview.Tests.Services
{
    class serializationtest
    {
        [Ignore("")]
        [Test]
        public void test_serialization()
        {
            //Arrange
            var item = new Item
            {
                ItemClass = "Test Class",
                BaseType = "Test Base Type",
                DropLevel = 54,
                Height = 2,
                Width = 2,
                ItemLevel = 50,
                ItemRarity = ItemRarity.Rare,
                SocketGroups = new List<SocketGroup> {new SocketGroup(new List<Socket> {new Socket(SocketColor.Red), new Socket(SocketColor.Blue), new Socket(SocketColor.White)}, true)},
                Quality = 12
            };

            //Act

            var serializer = new XmlSerializer(item.GetType());
            var output = string.Empty;
            using (var textWriter = new StringWriter())
            {
                serializer.Serialize(textWriter, item);
                 output = textWriter.ToString();
            }
            
            //Assert
        }
    }
}
