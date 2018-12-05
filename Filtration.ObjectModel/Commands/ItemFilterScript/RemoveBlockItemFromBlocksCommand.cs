using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Filtration.ObjectModel.Commands.ItemFilterScript
{
    public class RemoveBlockItemFromBlocksCommand : IUndoableCommand
    {
        private readonly List<Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>> _input;

        public RemoveBlockItemFromBlocksCommand(List<Tuple<ObservableCollection<IItemFilterBlockItem>, IItemFilterBlockItem>> input)
        {
            _input = input;
        }

        public void Execute()
        {
            foreach (var pair in _input)
            {
                var blockItems = pair.Item1;
                var blockItem = pair.Item2;

                for (var i = 0; i < blockItems.Count; i++)
                {
                    if (blockItems[i] == blockItem)
                    {
                        blockItems.RemoveAt(i--);
                    }
                }
            }
        }

        public void Undo()
        {
            foreach (var pair in _input)
            {
                var blockItems = pair.Item1;
                var blockItem = pair.Item2;
                blockItems.Add(blockItem);
            }
        }

        public void Redo() => Execute();
    }
}
