using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DlhSoft.UndoManagementLibrary
{
    public class UndoStack : INotifyPropertyChanged
    {
        #region Records

        private readonly List<ActionRecord> completedActions = new List<ActionRecord>();
        private readonly List<ActionRecord> undoneActions = new List<ActionRecord>();

        private class ActionRecord
        {
            public Action WhatWasDone;
            public Action HowToUndo;
            public DateTime WhenWasDone;
        }

        #endregion

        #region Recording

        public void Record(Action whatWasDone, Action howToUndo, DateTime? whenWasDone = null)
        {
            var originallyCanUndo = CanUndo;
            var originallyCanRedo = CanRedo;
            undoneActions.Clear();
            completedActions.Insert(0, new ActionRecord { WhatWasDone = whatWasDone, HowToUndo = howToUndo, WhenWasDone = whenWasDone ?? DateTime.Now });
            if (!originallyCanUndo)
                OnPropertyChanged(nameof(CanUndo));
            if (originallyCanRedo)
                OnPropertyChanged(nameof(CanRedo));
        }

        public void DoAndRecord(Action whatToDo, Action howToUndo)
        {
            DateTime now = DateTime.Now;
            whatToDo();
            Record(whatToDo, howToUndo, now);
        }

        #endregion

        #region Can undo/redo

        public bool CanUndo => completedActions.Any();
        public bool CanRedo => undoneActions.Any();

        #endregion

        #region Undo/redo

        public void Undo(TimeSpan? relatedActionSpan = null)
        {
            if (!CanUndo)
                return;
            if (relatedActionSpan == null)
                relatedActionSpan = TimeSpan.Zero;
            var originallyCanRedo = CanRedo;
            while (true)
            {
                var lastCompletedAction = completedActions.First();
                completedActions.RemoveAt(0);
                lastCompletedAction.HowToUndo();
                undoneActions.Insert(0, lastCompletedAction);
                var previouslyCompletedAction = completedActions.FirstOrDefault();
                if (previouslyCompletedAction == null || lastCompletedAction.WhenWasDone - previouslyCompletedAction.WhenWasDone > relatedActionSpan)
                    break;
            }
            if (!CanUndo)
                OnPropertyChanged(nameof(CanUndo));
            if (!originallyCanRedo)
                OnPropertyChanged(nameof(CanRedo));
        }

        public void Redo(TimeSpan? relatedActionSpan = null)
        {
            if (!CanRedo)
                return;
            if (relatedActionSpan == null)
                relatedActionSpan = TimeSpan.Zero;
            var originallyCanUndo = CanUndo;
            while (true)
            {
                var lastUndoneAction = undoneActions.First();
                undoneActions.RemoveAt(0);
                lastUndoneAction.WhatWasDone();
                completedActions.Insert(0, lastUndoneAction);
                var previouslyUndoneAction = undoneActions.FirstOrDefault();
                if (previouslyUndoneAction == null || previouslyUndoneAction.WhenWasDone - lastUndoneAction.WhenWasDone > relatedActionSpan)
                    break;
            }
            if (!CanRedo)
                OnPropertyChanged(nameof(CanRedo));
            if (!originallyCanUndo)
                OnPropertyChanged(nameof(CanUndo));
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
