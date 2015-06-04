using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Filtration.Models;
using NUnit.Framework;

namespace Filtration.Tests.Models
{
    [TestFixture]
    public class TestLootFilterScript
    {
        [Test]
        public void Validate_AtLeastOneBlock_Fail_ReturnsListWithCorrectError()
        {
            // Arrange

            var script = new LootFilterScript();

            // Act
            var result = script.Validate();

            // Assert
            Assert.AreEqual(1, result.Count(r => r.Contains("A script must have at least one block")));
        }

        [Test]
        public void Validate_AtLeastOneBlock_Pass_ReturnsListWithoutError()
        {
            // Arrange

            var script = new LootFilterScript
            {
                LootFilterBlocks = new ObservableCollection<LootFilterBlock> {new LootFilterBlock()}
            };

            // Act
            var result = script.Validate();

            // Assert
            Assert.AreEqual(0, result.Count(r => r.Contains("A script must have at least one block")));
        }
    }
}
