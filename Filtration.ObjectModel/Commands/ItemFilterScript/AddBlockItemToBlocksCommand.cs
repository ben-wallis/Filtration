using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class AddBlockItemToBlocksCommand : IUndoableCommand
    {
        private readonly List<Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>> _input;

        public AddBlockItemToBlocksCommand(List<Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>> input)
        {
            _input = input;
        }

        public void Execute()
        {
            foreach (var v in _input)
            {
                var blockItems = v.Item1;
                var item = v.Item2;

                blockItems.Add(item);
            }
        }

        public void Undo()
        {
            foreach (var v in _input)
            {
                var blockItems = v.Item1;
                var item = v.Item2;
                blockItems.Remove(item);
            }
        }

        public void Redo() => Execute();
    }
}
