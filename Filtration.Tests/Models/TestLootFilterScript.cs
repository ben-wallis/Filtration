using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Filtration.Models;
using Filtration.Models.BlockItemTypes;
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

        [Test]
        public void ReplaceColors_ReplacesBackgroundColorsCorrectly()
        {
            // Arrange
            var oldColor = new Color {A = 255, R = 255, G = 0, B = 0};
            var newColor = new Color {A = 255, R = 0, G = 255, B = 100};

            var testInputReplaceColors = new ReplaceColorsParameterSet
            {
                OldBackgroundColor = oldColor,
                NewBackgroundColor = newColor,
                ReplaceBackgroundColor = true
            };

            var testInputBlock1 = new LootFilterBlock();
            testInputBlock1.BlockItems.Add(new BackgroundColorBlockItem(new Color {A = 255, R = 255, G = 0, B = 0}));
            var testInputBlock2 = new LootFilterBlock();
            testInputBlock2.BlockItems.Add(new BackgroundColorBlockItem(new Color { A = 255, R = 255, G = 1, B = 0 }));
            var testInputBlock3 = new LootFilterBlock();
            testInputBlock3.BlockItems.Add(new BackgroundColorBlockItem(new Color { A = 255, R = 255, G = 0, B = 0 }));

            var script = new LootFilterScript()
            {
                LootFilterBlocks = new ObservableCollection<LootFilterBlock>
                {
                    testInputBlock1,
                    testInputBlock2,
                    testInputBlock3
                }
            };

            // Act
            script.ReplaceColors(testInputReplaceColors);

            // Assert
            Assert.AreEqual(newColor, testInputBlock1.BlockItems.OfType<BackgroundColorBlockItem>().First().Color);
            Assert.AreNotEqual(newColor, testInputBlock2.BlockItems.OfType<BackgroundColorBlockItem>().First().Color);
            Assert.AreEqual(newColor, testInputBlock3.BlockItems.OfType<BackgroundColorBlockItem>().First().Color);
        }

        [Test]
        public void ReplaceColors_OnlyReplacesColorsWhenAllSetParametersMatched()
        {
            // Arrange
            var oldBackgroundColor = new Color { A = 255, R = 255, G = 0, B = 0 };
            var newBackgroundColor = new Color { A = 255, R = 0, G = 255, B = 100 };
            
            var oldTextColor = new Color { A = 255, R = 100, G = 0, B = 50 };
            var newTextColor = new Color { A = 255, R = 101, G = 255, B = 51 };

            var testInputReplaceColors = new ReplaceColorsParameterSet
            {
                OldBackgroundColor = oldBackgroundColor,
                NewBackgroundColor = newBackgroundColor,
                OldTextColor = oldTextColor,
                NewTextColor = newTextColor,
                ReplaceBackgroundColor = true,
                ReplaceTextColor = true
            };

            var testInputBlock1 = new LootFilterBlock();
            testInputBlock1.BlockItems.Add(new BackgroundColorBlockItem(oldBackgroundColor));
            testInputBlock1.BlockItems.Add(new TextColorBlockItem(oldTextColor));
            var testInputBlock2 = new LootFilterBlock();
            testInputBlock2.BlockItems.Add(new BackgroundColorBlockItem(oldBackgroundColor));
            testInputBlock2.BlockItems.Add(new TextColorBlockItem(new Color {A = 1, R = 2, G = 3, B = 4}));

            var script = new LootFilterScript
            {
                LootFilterBlocks = new ObservableCollection<LootFilterBlock>
                {
                    testInputBlock1,
                    testInputBlock2
                }
            };

            // Act
            script.ReplaceColors(testInputReplaceColors);

            // Assert
            // First test block has had its colors changed
            Assert.AreEqual(newBackgroundColor, testInputBlock1.BlockItems.OfType<BackgroundColorBlockItem>().First().Color);
            Assert.AreEqual(newTextColor, testInputBlock1.BlockItems.OfType<TextColorBlockItem>().First().Color);

            // Second test block has not had its colors changed
            Assert.AreEqual(oldBackgroundColor, testInputBlock2.BlockItems.OfType<BackgroundColorBlockItem>().First().Color);
            Assert.AreNotEqual(newTextColor, testInputBlock2.BlockItems.OfType<TextColorBlockItem>().First().Color);
        }
    }
}
