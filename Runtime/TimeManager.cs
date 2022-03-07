using UnityEngine;
using OmiyaGames.Global;

namespace OmiyaGames.Managers
{
	///-----------------------------------------------------------------------
	/// <remarks>
	/// <copyright file="TimeManager.cs" company="Omiya Games">
	/// The MIT License (MIT)
	/// 
	/// Copyright (c) 2014-2022 Omiya Games
	/// 
	/// Permission is hereby granted, free of charge, to any person obtaining a copy
	/// of this software and associated documentation files (the "Software"), to deal
	/// in the Software without restriction, including without limitation the rights
	/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	/// copies of the Software, and to permit persons to whom the Software is
	/// furnished to do so, subject to the following conditions:
	/// 
	/// The above copyright notice and this permission notice shall be included in
	/// all copies or substantial portions of the Software.
	/// 
	/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
	/// THE SOFTWARE.
	/// </copyright>
	/// <list type="table">
	/// <listheader>
	/// <term>Revision</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>
	/// <strong>Date:</strong> 5/18/2015<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>Initial verison.</description>
	/// </item><item>
	/// <term>
	/// <strong>Version:</strong> 1.0.0-pre.1<br/>
	/// <strong>Date:</strong> 2/12/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>
	/// Converting from Singleton script to its own unique package.
	/// </description>
	/// </item><item>
	/// <term>
	/// <strong>Version:</strong> 1.0.1-pre.2<br/>
	/// <strong>Date:</strong> 2/12/2022<br/>
	/// <strong>Author:</strong> Taro Omiya
	/// </term>
	/// <description>
	/// Changing to a <c>static</c> class so this script can't be
	/// accidentally attached to another <see cref="GameObject"/>.
	/// Renamed <c>OnBeforeManualPausedChanged</c> to
	/// <see cref="OnBeforeIsManuallyPausedChanged"/>, and
	/// <c>OnAfterManualPausedChanged</c> to
	/// <see cref="OnAfterIsManuallyPausedChanged"/>.
	/// </description>
	/// </item>
	/// </list>
	/// </remarks>
	///-----------------------------------------------------------------------
	/// <summary>
	/// A manager for adjusting the time scale.  Used for manually
	/// pausing the game.  Also allows temporarily slowing down or quickening time,
	/// useful for creating common juicy effects.
	/// </summary>
	public static class TimeManager
	{
		/// <summary>
		/// Grabs the static instance of this manager.
		/// </summary>
		/// <seealso cref="Data"/>
		static Impl GetInstance()
		{
			Impl returnInstance = ComponentSingleton<Impl>.Get(false, out bool isFirstTimeCreated);
			if (isFirstTimeCreated)
			{
				// Update the time scales
				returnInstance.Setup();
			}
			return returnInstance;
		}

		/// <summary>
		/// Triggers when time scale changes, before the effects are settled.
		/// </summary>
		public static event ITrackable<float>.ChangeEvent OnBeforeTimeScaleChanged
		{
			add => GetInstance().TimeScale.OnBeforeValueChanged += value;
			remove => GetInstance().TimeScale.OnBeforeValueChanged -= value;
		}
		/// <summary>
		/// Triggers when time scale changes, after the effects are settled.
		/// </summary>
		public static event ITrackable<float>.ChangeEvent OnAfterTimeScaleChanged
		{
			add => GetInstance().TimeScale.OnAfterValueChanged += value;
			remove => GetInstance().TimeScale.OnAfterValueChanged -= value;
		}

		/// <summary>
		/// Triggers when pause state changes, before the effects are settled.
		/// </summary>
		public static event ITrackable<bool>.ChangeEvent OnBeforeIsManuallyPausedChanged
		{
			add => GetInstance().IsManuallyPaused.OnBeforeValueChanged += value;
			remove => GetInstance().IsManuallyPaused.OnBeforeValueChanged -= value;
		}
		/// <summary>
		/// Triggers when pause state changes, after the effects are settled.
		/// </summary>
		public static event ITrackable<bool>.ChangeEvent OnAfterIsManuallyPausedChanged
		{
			add => GetInstance().IsManuallyPaused.OnAfterValueChanged += value;
			remove => GetInstance().IsManuallyPaused.OnAfterValueChanged -= value;
		}

		/// <summary>
		/// The "stable" time scale, unaffected by hit-pauses, etc.
		/// </summary>
		/// <remarks>
		/// This value is multiplied to the player-customized time scale
		/// (usually on the accessibility menu) before changing
		/// <see cref="Time.timeScale"/>. For example, if the player adjusts
		/// their accessibility time scale to 0.5, and then the programmer sets
		/// this property to 1.0, <see cref="Time.timeScale"/> will be set to 0.5
		/// (<c>1.0 * 0.5 = 0.5</c>).
		/// </remarks>
		public static float TimeScale
		{
			get => GetInstance().TimeScale.Value;
			set => GetInstance().TimeScale.Value = value;
		}

		/// <summary>
		/// If true, pauses the game.
		/// </summary>
		public static bool IsManuallyPaused
		{
			get => GetInstance().IsManuallyPaused.Value;
			set => GetInstance().IsManuallyPaused.Value = value;
		}

		/// <summary>
		/// Reverts the default time scale stored in
		/// <see cref="Saves.GameSettings"/>.
		/// </summary>
		public static void RevertTimeScale()
		{
			IsManuallyPaused = false;
			TimeScale = 1;
		}

		/// <summary>
		/// Temporarily changes the game's timescale for a short duration.
		/// </summary>
		/// <param name="timeScale">The timescale to set to.</param>
		/// <param name="durationSeconds">
		/// How long the change lasts, in seconds.
		/// Duration is not affected by <seealso cref="Time.timeScale"/>.
		/// </param>
		public static void SetTimeScaleFor(float timeScale, float durationSeconds) => GetInstance().SetTimeScaleFor(timeScale, durationSeconds);

		class Impl : MonoBehaviour
		{
			const float IGNORE_TIMESCALE = -1f;

			TrackTimeScale timeScale = null;
			TrackIsPaused isManuallyPaused = null;
			
			// FIXME: Change this to a SaveFloat ASAP
			ITrackable<float> accessibilityScale = new Trackable<float>(1);
			ITrackable<float>.ChangeEvent onAccessibilityScaleChanged = null;

			Coroutine tempTimeScaleChange = null;
			float tempTimeScale = IGNORE_TIMESCALE;

			public ITrackable<float> TimeScale => timeScale;
			public ITrackable<bool> IsManuallyPaused => isManuallyPaused;

			public void Setup()
			{
				// Setup trackable time scale
				if (timeScale == null)
				{
					timeScale = new TrackTimeScale();
					timeScale.Update += UpdateTimeScale;
				}

				// Setup trackable pause state
				if (isManuallyPaused == null)
				{
					isManuallyPaused = new TrackIsPaused();
					isManuallyPaused.Update += UpdateTimeScale;
				}

				if (onAccessibilityScaleChanged == null)
				{
					// FIXME: retrieve time-scale through SaveFloat instead
					Saves.GameSettings settings = Singleton.Get<Saves.GameSettings>();
					if (settings != null)
					{
						accessibilityScale.Value = settings.CustomTimeScale;
					}

					// Listen to event
					onAccessibilityScaleChanged = new ITrackable<float>.ChangeEvent(UpdateTimeScale);
					accessibilityScale.OnAfterValueChanged += onAccessibilityScaleChanged;
				}
			}

			public void SetTimeScaleFor(float tempTimeScale, float durationSeconds)
			{
				// Clean-up before running a coroutine
				StopCoroutine();

				// Run the coroutine
				tempTimeScaleChange = Manager.StartCoroutine(SetTimeScaleCoroutine());

				System.Collections.IEnumerator SetTimeScaleCoroutine()
				{
					// Change the time scale
					this.tempTimeScale = tempTimeScale;
					UpdateTimeScale(TimeScale.Value);

					// Wait for a short duration
					yield return new WaitForSecondsRealtime(durationSeconds);

					// Revert the time scale
					RevertTime();
				}
			}

			void OnDestroy()
			{
				// Stop the coroutine
				if (StopCoroutine())
				{
					// Revert the time scale
					RevertTime();
				}

				// Unsubscribe from events
				if (timeScale != null)
				{
					timeScale.Update -= UpdateTimeScale;
					timeScale = null;
				}
				if (isManuallyPaused != null)
				{
					isManuallyPaused.Update -= UpdateTimeScale;
					isManuallyPaused = null;
				}
				if (onAccessibilityScaleChanged != null)
				{
					accessibilityScale.OnAfterValueChanged -= onAccessibilityScaleChanged;
					onAccessibilityScaleChanged = null;
				}
			}

			bool StopCoroutine()
			{
				if (tempTimeScaleChange != null)
				{
					// Stop the coroutine
					Manager.StopCoroutine(tempTimeScaleChange);
					tempTimeScaleChange = null;
					return true;
				}
				return false;
			}

			void RevertTime()
			{
				// Revert the time scale
				tempTimeScale = IGNORE_TIMESCALE;
				UpdateTimeScale(TimeScale.Value);
			}

			void UpdateTimeScale(float timeScale) => UpdateTimeScale(timeScale, IsManuallyPaused.Value, accessibilityScale.Value, tempTimeScale);
			void UpdateTimeScale(bool isManuallyPaused) => UpdateTimeScale(TimeScale.Value, isManuallyPaused, accessibilityScale.Value, tempTimeScale);
			void UpdateTimeScale(float _, float newAccessibilityScale) => UpdateTimeScale(TimeScale.Value, IsManuallyPaused.Value, newAccessibilityScale, tempTimeScale);
			static void UpdateTimeScale(float timeScale, bool isManuallyPaused, float accessibilityScale, float tempTimeScale)
			{
				// Check if paused
				if (isManuallyPaused)
				{
					// If so, pause
					Time.timeScale = 0f;
				}
				else if (tempTimeScale < 0f)
				{
					// If not, progress normally
					Time.timeScale = (timeScale * accessibilityScale);
				}
				else
				{
					Time.timeScale = (tempTimeScale * accessibilityScale);
				}
			}

			class TrackTimeScale : TrackableDecorator<float>
			{
				float timeScale = 1f;

				public event System.Action<float> Update;
				/// <inheritdoc/>
				public override event ITrackable<float>.ChangeEvent OnBeforeValueChanged;
				/// <inheritdoc/>
				public override event ITrackable<float>.ChangeEvent OnAfterValueChanged;

				/// <inheritdoc/>
				public override float Value
				{
					get => timeScale;
					set
					{
						// Clamp value
						if (value < 0f)
						{
							value = 0f;
						}

						// Check if the values are different
						if (Mathf.Approximately(value, timeScale))
						{
							// Halt, if not
							return;
						}

						// Trigger events
						OnBeforeValueChanged?.Invoke(timeScale, value);

						float oldValue = timeScale;
						timeScale = value;
						Update(timeScale);

						OnAfterValueChanged?.Invoke(oldValue, timeScale);
					}
				}
			}

			class TrackIsPaused : TrackableDecorator<bool>
			{
				bool isPaused = false;

				public event System.Action<bool> Update;
				/// <inheritdoc/>
				public override event ITrackable<bool>.ChangeEvent OnBeforeValueChanged;
				/// <inheritdoc/>
				public override event ITrackable<bool>.ChangeEvent OnAfterValueChanged;

				/// <inheritdoc/>
				public override bool Value
				{
					get => isPaused;
					set
					{
						if (isPaused != value)
						{
							OnBeforeValueChanged?.Invoke(isPaused, value);

							bool oldValue = isPaused;
							isPaused = value;
							Update(isPaused);

							OnAfterValueChanged?.Invoke(oldValue, isPaused);
						}
					}
				}
			}
		}
	}
}
