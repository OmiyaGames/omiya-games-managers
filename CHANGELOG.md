# Change Log:

## 1.0.1-pre.3

- Simplifying dependencies.

## 1.0.1-pre.2

- Changing [`TimeManager`](/Runtime/TimeManager.cs) to a static class.
    - **Breaking Change:** Renamed event `OnBeforeManualPausedChanged(TimeManager)` and `OnAfterManualPausedChanged(TimeManager)` in [`TimeManager`](/Runtime/TimeManager.cs) to `OnBeforeIsManuallyPausedChanged(bool, bool)` and `OnAfterIsManuallyPausedChanged(bool, bool)` respectively.

## 1.0.1-pre.1

- **Bug Fix:** making the call [`TimeManager.SetTimeScaleFor()`](/Runtime/TimeManager.cs) will no longer permanently change the time scale.

## 1.0.0-pre.1

- Initial release!
    - Adding [`TimeManager`](/Runtime/TimeManager.cs) to handle time scale management.