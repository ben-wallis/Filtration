using System.Collections.Generic;
using System.Windows.Media;
using Filtration.Enums;
using Filtration.Models;

namespace Filtration.ViewModels
{
    internal class LootFilterScriptViewModela : ILootFilterScriptViewModel
    {
        private readonly ILootFilterConditionViewModelFactory _lootFilterConditionViewModelFactory;

        public LootFilterScriptViewModela(ILootFilterConditionViewModelFactory lootFilterConditionViewModelFactory)
        {
            _lootFilterConditionViewModelFactory = lootFilterConditionViewModelFactory;
            LootFilterConditionViewModels = new List<ILootFilterConditionViewModel>();

            var testCondition = new LootFilterCondition
            {
                BackgroundColor = Colors.DarkCyan,
                TextColor = Colors.White,
                BorderColor = Colors.Red,
                Sockets =
                    new NumericFilterPredicate(FilterPredicateOperator.LessThanOrEqual, 5),
                LinkedSockets =
                    new NumericFilterPredicate(FilterPredicateOperator.Equal, 2),
                ItemRarity = new NumericFilterPredicate(FilterPredicateOperator.GreaterThan, (int) ItemRarity.Magic),
                FontSize = 10,
                Quality =
                    new NumericFilterPredicate(FilterPredicateOperator.GreaterThanOrEqual, 15),
                ItemLevel =
                    new NumericFilterPredicate(FilterPredicateOperator.GreaterThan, 50),
                DropLevel = new NumericFilterPredicate(),
                FilterDescription = "My Wicked Filter"
            };

            var testCondition2 = new LootFilterCondition
            {
                BackgroundColor = Colors.Beige,
                TextColor = Colors.Blue,
                BorderColor = Colors.Black,
                Sockets =
                    new NumericFilterPredicate(FilterPredicateOperator.LessThan, 4),
                LinkedSockets =
                    new NumericFilterPredicate(FilterPredicateOperator.GreaterThanOrEqual, 3),
                FontSize = 12,
                Quality =
                    new NumericFilterPredicate(FilterPredicateOperator.GreaterThanOrEqual, 15),
                ItemLevel =
                    new NumericFilterPredicate(FilterPredicateOperator.Equal, 32),
                DropLevel = new NumericFilterPredicate(FilterPredicateOperator.GreaterThanOrEqual, 85),
                FilterDescription = "This is a test filter"
            };



            var testCondition3 = new LootFilterCondition
            {
                BackgroundColor = Colors.Beige,
                TextColor = new Color { A = 128, R = 0, G = 0, B = 255},
                BorderColor = Colors.Black,
                Sockets =
                    new NumericFilterPredicate(FilterPredicateOperator.LessThan, 4),
                LinkedSockets =
                    new NumericFilterPredicate(FilterPredicateOperator.GreaterThanOrEqual, 3),
                FontSize = 12,
                Quality =
                    new NumericFilterPredicate(FilterPredicateOperator.GreaterThanOrEqual, 15),
                ItemLevel =
                    new NumericFilterPredicate(FilterPredicateOperator.Equal, 32),
                DropLevel = new NumericFilterPredicate(FilterPredicateOperator.GreaterThanOrEqual, 85),
                FilterDescription = "This is a test filter"
            };

            var testConditionFilter = _lootFilterConditionViewModelFactory.Create();
            var testConditionFilter2 = _lootFilterConditionViewModelFactory.Create();
            var testConditionFilter3 = _lootFilterConditionViewModelFactory.Create();
            testConditionFilter.Initialise(testCondition);
            testConditionFilter2.Initialise(testCondition2);
            testConditionFilter3.Initialise(testCondition3);

            testConditionFilter.Classes.Add("Test Class 1");
            testConditionFilter.Classes.Add("Test Class 2");
            testConditionFilter.Classes.Add("Test Class 3");
            testConditionFilter.Classes.Add("Test Class 4");
            testConditionFilter.Classes.Add("Test Class 5");
            testConditionFilter.Classes.Add("Test Class 6");
            testConditionFilter.Classes.Add("Test Class 7");
            testConditionFilter.Classes.Add("Test Class 8");
            testConditionFilter.Classes.Add("Test Class 9");
            testConditionFilter.Classes.Add("Test Class 10");
            testConditionFilter.BaseTypes.Add("Test Base Type 1");
            testConditionFilter.BaseTypes.Add("Test Base Type 2");
            LootFilterConditionViewModels.Add(testConditionFilter);
            LootFilterConditionViewModels.Add(testConditionFilter2);
            LootFilterConditionViewModels.Add(testConditionFilter3);
        }

        public List<ILootFilterConditionViewModel> LootFilterConditionViewModels { get; private set; }
    }
}
