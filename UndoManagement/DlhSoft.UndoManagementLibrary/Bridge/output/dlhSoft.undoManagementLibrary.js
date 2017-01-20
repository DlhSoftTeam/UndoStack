/**
 * @version 1.0.0.0
 * @copyright Copyright © DlhSoft 2017
 * @compiler Bridge.NET 15.7.0
 */
Bridge.assembly("DlhSoft.UndoManagementLibrary", function ($asm, globals) {
    "use strict";

    Bridge.define("DlhSoft.UndoManagementLibrary.UndoStack", {
        inherits: [System.ComponentModel.INotifyPropertyChanged],
        completedActions: null,
        undoneActions: null,
        config: {
            events: {
                PropertyChanged: null
            },
            alias: [
            "addPropertyChanged", "System$ComponentModel$INotifyPropertyChanged$addPropertyChanged",
            "removePropertyChanged", "System$ComponentModel$INotifyPropertyChanged$removePropertyChanged"
            ],
            init: function () {
                this.completedActions = new (System.Collections.Generic.List$1(DlhSoft.UndoManagementLibrary.UndoStack.ActionRecord))();
                this.undoneActions = new (System.Collections.Generic.List$1(DlhSoft.UndoManagementLibrary.UndoStack.ActionRecord))();
            }
        },
        getCanUndo: function () {
            return System.Linq.Enumerable.from(this.completedActions).any();
        },
        getCanRedo: function () {
            return System.Linq.Enumerable.from(this.undoneActions).any();
        },
        record: function (whatWasDone, howToUndo, whenWasDone) {
            var $t;
            if (whenWasDone === void 0) { whenWasDone = null; }
            var originallyCanUndo = this.getCanUndo();
            var originallyCanRedo = this.getCanRedo();
            this.undoneActions.clear();
            this.completedActions.insert(0, Bridge.merge(new DlhSoft.UndoManagementLibrary.UndoStack.ActionRecord(), {
                whatWasDone: whatWasDone,
                howToUndo: howToUndo,
                whenWasDone: ($t = whenWasDone, $t != null ? $t : new Date())
            } ));
            if (!originallyCanUndo) {
                this.onPropertyChanged("canUndo");
            }
            if (originallyCanRedo) {
                this.onPropertyChanged("canRedo");
            }
        },
        doAndRecord: function (whatToDo, howToUndo) {
            var now = new Date();
            whatToDo();
            this.record(whatToDo, howToUndo, now);
        },
        undo: function (relatedActionSpan) {
            if (relatedActionSpan === void 0) { relatedActionSpan = null; }
            if (!this.getCanUndo()) {
                return;
            }
            if (System.TimeSpan.eq(relatedActionSpan, null)) {
                relatedActionSpan = System.TimeSpan.zero;
            }
            var originallyCanRedo = this.getCanRedo();
            while (true) {
                var lastCompletedAction = System.Linq.Enumerable.from(this.completedActions).first();
                this.completedActions.removeAt(0);
                lastCompletedAction.howToUndo();
                this.undoneActions.insert(0, lastCompletedAction);
                var previouslyCompletedAction = System.Linq.Enumerable.from(this.completedActions).firstOrDefault(null, null);
                if (previouslyCompletedAction == null || System.TimeSpan.gt(Bridge.Date.subdd(lastCompletedAction.whenWasDone, previouslyCompletedAction.whenWasDone), relatedActionSpan)) {
                    break;
                }
            }
            if (!this.getCanUndo()) {
                this.onPropertyChanged("canUndo");
            }
            if (!originallyCanRedo) {
                this.onPropertyChanged("canRedo");
            }
        },
        redo: function (relatedActionSpan) {
            if (relatedActionSpan === void 0) { relatedActionSpan = null; }
            if (!this.getCanRedo()) {
                return;
            }
            if (System.TimeSpan.eq(relatedActionSpan, null)) {
                relatedActionSpan = System.TimeSpan.zero;
            }
            var originallyCanUndo = this.getCanUndo();
            while (true) {
                var lastUndoneAction = System.Linq.Enumerable.from(this.undoneActions).first();
                this.undoneActions.removeAt(0);
                lastUndoneAction.whatWasDone();
                this.completedActions.insert(0, lastUndoneAction);
                var previouslyUndoneAction = System.Linq.Enumerable.from(this.undoneActions).firstOrDefault(null, null);
                if (previouslyUndoneAction == null || System.TimeSpan.gt(Bridge.Date.subdd(previouslyUndoneAction.whenWasDone, lastUndoneAction.whenWasDone), relatedActionSpan)) {
                    break;
                }
            }
            if (!this.getCanRedo()) {
                this.onPropertyChanged("canRedo");
            }
            if (!originallyCanUndo) {
                this.onPropertyChanged("canUndo");
            }
        },
        onPropertyChanged: function (propertyName) {
            !Bridge.staticEquals(this.PropertyChanged, null) ? this.PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName)) : null;
        }
    });

    Bridge.define("DlhSoft.UndoManagementLibrary.UndoStack.ActionRecord", {
        whatWasDone: null,
        howToUndo: null,
        config: {
            init: function () {
                this.whenWasDone = new Date(-864e13);
            }
        }
    });
});
