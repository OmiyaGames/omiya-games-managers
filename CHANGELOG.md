# Change Log:

## 1.0.1-pre.2

- Changing [`TimeManager`](/Runtime/TimeManager.cs) to be deactivated. [`TimeManager.SetTimeScaleFor()`](/Runtime/TimeManager.cs) will spawn a coroutine on `Manager` from `com.omiyagames.global` package.

## 1.0.1-pre.1

- **Bug Fix:** making the call [`TimeManager.SetTimeScaleFor()`](/Runtime/TimeManager.cs) will no longer permanently change the time scale.

## 1.0.0-pre.1

- Initial release!
    - Adding [`TimeManager`](/Runtime/TimeManager.cs) to handle time scale management.