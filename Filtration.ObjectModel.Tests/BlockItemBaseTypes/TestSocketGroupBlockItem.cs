using System.Collections.Generic;
using System.Net.Sockets;
using Filtration.ObjectModel.BlockItemTypes;
using Filtration.ObjectModel.Enums;
using Filtration.ObjectModel.LootExplosionStudio;
using NUnit.Framework;

namespace Filtration.ObjectModel.Tests.BlockItemBaseTypes
{
    [TestFixture]
    public class TestSocketGroupBlockItem
    {
        [Test]
        public void MatchesBlockItem_BlankLootItem_ReturnsFalse()
        {
            // Arrange

            var blockItem = new SocketGroupBlockItem();
            var testInputLootItem = new LootItem();

            // Act
            var result = blockItem.MatchesLootItem(testInputLootItem);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void MatchesBlockItem_SocketsMatch_ReturnsTrue()
        {
            // Arrange
            var testInputSocketGroups = new List<SocketGroup>();
            var testInputSocketGroup1 = new SocketGroup();
            var testInputSocketGroup2 = new SocketGroup();
            testInputSocketGroup1.Sockets = new List<SocketColor> {SocketColor.Blue, SocketColor.Red};
            testInputSocketGroup2.Sockets = new List<SocketColor>
            {
                SocketColor.Blue,
                SocketColor.Blue,
                SocketColor.Blue,
                SocketColor.Red
            };

            testInputSocketGroups.Add(testInputSocketGroup1);
            testInputSocketGroups.Add(testInputSocketGroup2);

            var blockItem = new SocketGroupBlockItem();
            blockItem.Items.Add("RRG");
            blockItem.Items.Add("BRB");

            var testInputLootItem = new LootItem {SocketGroups = testInputSocketGroups};

            // Act
            var result = blockItem.MatchesLootItem(testInputLootItem);

            // Assert
            Assert.IsTrue(result);
        }
        
        [Test]
        public void MatchesBlockItem_SocketsAlmostMatch_ReturnsFalse()
        {
            // Arrange
            var testInputSocketGroups = new List<SocketGroup>();
            var testInputSocketGroup1 = new SocketGroup();
            var testInputSocketGroup2 = new SocketGroup();
            testInputSocketGroup1.Sockets = new List<SocketColor> { SocketColor.Blue, SocketColor.Red };
            testInputSocketGroup2.Sockets = new List<SocketColor>
            {
                SocketColor.Blue,
                SocketColor.Blue,
                SocketColor.Blue,
                SocketColor.Red,
                SocketColor.Green
            };

            testInputSocketGroups.Add(testInputSocketGroup1);
            testInputSocketGroups.Add(testInputSocketGroup2);

            var blockItem = new SocketGroupBlockItem();
            blockItem.Items.Add("BGBRWB");

            var testInputLootItem = new LootItem { SocketGroups = testInputSocketGroups };

            // Act
            var result = blockItem.MatchesLootItem(testInputLootItem);

            // Assert
            Assert.IsFalse(result);
        }
    }
}
