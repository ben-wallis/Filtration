using System.Linq;
using Filtration.ItemFilterPreview.Data.Repositories;
using Filtration.ObjectModel;
using Filtration.ObjectModel.Enums;
using NUnit.Framework;

namespace Filtration.ItemFilterPreview.Data.Tests.Repositories
{
    [Ignore("integration test")]
    [TestFixture]
    public class TestItemSetRepository
    {
        [Test]
        public void All_ReturnsAllItemSets()
        {
            using (var repository = new ItemSetRepository())
            {
                var result = repository.All.ToList();
            }
        }

        [Test]
        public void AddItemToItemSet()
        {
            ItemSet fetchedItemSet;
            using (var repository = new ItemSetRepository())
            {
                fetchedItemSet = repository.Find(1);
            }

            var newItem = new Item
            {
                BaseType = "Test Base",
                Description = "Test Item Supreme",
                DropLevel = 75,
                Height = 3,
                Width = 2,
                ItemClass = "Super Class",
                ItemRarity = ItemRarity.Rare,
                ItemLevel = 50,
                ItemSet = fetchedItemSet,
                ItemSetId = fetchedItemSet.Id
                
            };

            fetchedItemSet.Items.Add(newItem);

            using (var repository = new ItemSetRepository())
            {
                repository.InsertOrUpdate(fetchedItemSet);
                repository.Save();
            }
        }
    }
}
