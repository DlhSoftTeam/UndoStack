using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DlhSoft.UndoManagementLibrary
{
    /// <summary>
    /// Provides support for recording (or executing and automatically recording) and undoing or redoing actions.
    /// </summary>
    public class UndoStack : INotifyPropertyChanged
    {
        #region Records

        private readonly List<ActionRecord> completedActions = new List<ActionRecord>();
        private readonly List<ActionRecord> undoneActions = new List<ActionRecord>();

        private class ActionRecord
        {
            public Action WhatWasDone;
            public Action HowToUndo;
            public int WhenWasDone;
        }

        #endregion

        #region Recording

        /// <summary>
        /// Records an already executed action to the undo stack.
        /// </summary>
        public void Record(Action whatWasDone, Action howToUndo, int? whenWasDone = null)
        {
            var originallyCanUndo = CanUndo;
            var originallyCanRedo = CanRedo;
            undoneActions.Clear();
            completedActions.Insert(0, new ActionRecord { WhatWasDone = whatWasDone, HowToUndo = howToUndo, WhenWasDone = whenWasDone ?? (int)(DateTime.Now.Ticks / 10000) });
            if (!originallyCanUndo)
                OnPropertyChanged(nameof(CanUndo));
            if (originallyCanRedo)
                OnPropertyChanged(nameof(CanRedo));
        }

        /// <summary>
        /// Executes an action and records it to the undo stack.
        /// </summary>
        public void DoAndRecord(Action whatToDo, Action howToUndo)
        {
            DateTime now = DateTime.Now;
            whatToDo();
            Record(whatToDo, howToUndo, (int)(now.Ticks / 10000));
        }

        #endregion

        #region Can undo/redo

        /// <summary>
        /// Indicates whether there are any completed operations that can be undone.
        /// </summary>
        public bool CanUndo => completedActions.Any();

        /// <summary>
        /// Indicates whether there are any undone operations that can be redone.
        /// </summary>
        public bool CanRedo => undoneActions.Any();

        #endregion

        #region Undo/redo

        /// <summary>
        /// Undoes the last completed action and related actions.
        /// </summary>
        /// <param name="relatedActionSpan">Maximum time span in milliseconds to consider for determining related actions.</param>
        public void Undo(int? relatedActionSpan = null)
        {
            if (!CanUndo)
                return;
            if (relatedActionSpan == null)
                relatedActionSpan = 0;
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

        /// <summary>
        /// Redoes the last undone action and related actions.
        /// </summary>
        /// <param name="relatedActionSpan">Maximum time span in milliseconds to consider for determining related actions.</param>
        public void Redo(int? relatedActionSpan = null)
        {
            if (!CanRedo)
                return;
            if (relatedActionSpan == null)
                relatedActionSpan = 0;
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

        /// <summary>
        /// Occurs when CanUndo/CanRedo property values change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises PropertyChanged event.
        /// </summary>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
