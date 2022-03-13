# Change Log:

## 1.0.1-pre.4

- Adjusting [`TimeManager`](/Runtime/TimeManager.cs) behavior.
    - **Breaking Change:** `TimeScale` property is no longer representative of the actual `Time.timeScale` value.  Instead, if the user turns on the accessibility feature to adjust the time scale, `TimeScale` will be multiplied with that new accessibility value into `Time.timeScale`.  For example, if `TimeScale` is set to 0.5, and user-set time scale is 0.2, then `Time.timeScale` will be set to 0.1 (`0.5 x 0.2 = 0.1`).  This will prevent the programmer from overriding the user settings, at least within `TimeManager`'s scope.
    - **New Feature:** exposed `OnBeforeTimeScaleChanged` and `OnAfterTimeScaleChanged` events to listen to programmer changing the `TimeScale` value.

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