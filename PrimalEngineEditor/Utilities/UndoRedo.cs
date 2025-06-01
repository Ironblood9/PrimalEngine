using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimalEngineEditor.Utilities
{
   public  interface InterfaceUndoRedo
    {
        string Name { get;}
        void Undo();
        void Redo();
    }

    public class UndoRedoActions : InterfaceUndoRedo
    {
        private Action _undoAction;
        private Action _redoAction;
        public string Name { get; }

        public void Redo() => _redoAction();

        public void Undo() => _undoAction();

        public UndoRedoActions(string name)
        {
            
            Name = name;
        }
        public UndoRedoActions(Action undoAction, Action redoAction, string name):this(name)
        {
            Debug.Assert(undoAction != null && redoAction != null);
            _undoAction = undoAction;
            _redoAction = redoAction;
            
        }
    }
    public class UndoRedo
    {
        private readonly ObservableCollection<InterfaceUndoRedo> _redoList = new ObservableCollection<InterfaceUndoRedo>();
        private readonly ObservableCollection<InterfaceUndoRedo> _undoList = new ObservableCollection<InterfaceUndoRedo>();
        public ReadOnlyObservableCollection<InterfaceUndoRedo> RedoList { get; }
        public ReadOnlyObservableCollection<InterfaceUndoRedo> UndoList { get; }

        public void Reset()
        {
            _redoList.Clear();
            _undoList.Clear();
        }
        public void Add(InterfaceUndoRedo cmd)
        {
            _undoList.Add(cmd);
            _redoList.Clear();
        }

        public void Undo()
        {
            if (_undoList.Any())
            {
                var cmd = _undoList.Last();
                _undoList.RemoveAt(_undoList.Count - 1);
                cmd.Undo();
                _redoList.Insert(0, cmd);
            }

        }
        public void Redo()
        {
            if (_redoList.Any())
            {
                var cmd = _redoList.First();
                _redoList.RemoveAt(0);
                cmd.Redo();
                _undoList.Add(cmd);
            }
        }


        public UndoRedo()
        {
            RedoList = new ReadOnlyObservableCollection<InterfaceUndoRedo>(_redoList);
            UndoList = new ReadOnlyObservableCollection<InterfaceUndoRedo>(_undoList);
        }


    }
    
}
